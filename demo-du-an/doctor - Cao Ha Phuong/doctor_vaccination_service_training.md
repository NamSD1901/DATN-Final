# TÀI LIỆU ĐÀO TẠO NỘI BỘ: PHÂN TÍCH CHUYÊN SÂU VACCINATION SERVICE (DOCTOR)

> [!NOTE]
> Đây là tài liệu đào tạo chuyên biệt phân tích từng dòng code của `VaccinationService.cs`. Điểm khác biệt lớn nhất giữa Tiêm phòng (Vaccination) và Khám lâm sàng (Medical) là tiêm phòng liên quan chặt chẽ đến **Mã lô hàng (Batch Number)** của vắc-xin và **Logic sinh Hóa đơn (Billing)** tùy thuộc vào quyết định của Bác sĩ (Tiêm hay Hoãn tiêm).

---

## 1. LƯU BỆNH ÁN TIÊM PHÒNG (SUBMIT SOAP RECORD)

Hàm `SubmitSoapRecordAsync` chịu trách nhiệm thu thập form mà bác sĩ vừa điền trên màn hình và phân luồng nghiệp vụ.

```c#
        public async Task<long> SubmitSoapRecordAsync(long appointmentId, Guid doctorId, VaccinationSoapRequestDto request)
        {
            var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(a => a.Id == appointmentId, a => a.Pet!);
            var appointment = appointments.FirstOrDefault();
            if (appointment == null) throw new KeyNotFoundException("Không tìm thấy cuộc hẹn.");

            // CHẶN HÀNH VI SỬA LẠI: Nếu ca khám đã xong hoặc đã hủy thì không cho sửa nữa.
            if (appointment.Status == "completed" || appointment.Status == "cancelled")
                throw new InvalidOperationException("Cuộc hẹn đã kết thúc, không thể lưu bệnh án.");

            // 1. TẠO BẢN GHI TIÊM PHÒNG (Gom tất cả các trường SOAP nhét vào Object)
            var record = CreateVaccinationRecord(appointment, doctorId, request);
            
            // 2. RẼ NHÁNH TỰ ĐỘNG THEO QUYẾT ĐỊNH CỦA BÁC SĨ (INTELLIGENT ROUTING)
            // Nếu con chó hoàn toàn khỏe mạnh -> Bác sĩ kết luận "Đủ điều kiện"
            if (request.ClinicalAssessment == "Đủ điều kiện")
            {
                // Gọi hàm xuất kho lọ Vắc-xin và lên hóa đơn tiền Vắc-xin
                await ProcessInventoryAndInvoiceAsync(appointment, request);
            }
            // Nếu con chó đang sốt -> Bác sĩ kết luận "Hoãn tiêm"
            else if (request.ClinicalAssessment == "Hoãn tiêm")
            {
                // Không tốn lọ Vắc-xin nào, nhưng vẫn phải thu "Phí khám lâm sàng" vì bác sĩ đã mất công khám
                await CreateConsultationFeeOnlyAsync(appointment);
            }

            // 3. ĐÓNG CA KHÁM VÀ LƯU DATABASE
            appointment.Status = "completed"; // Lưu ý: Tiêm phòng xong là xong luôn (completed), không cần qua cột ready_to_pay như Khám bệnh
            appointment.CheckOutTime = DateTime.UtcNow;
            
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.VaccinationRecords.AddAsync(record);
            
            // (Tùy chọn) Bác sĩ đo cân nặng và hệ thống tự cập nhật vào hồ sơ gốc của con vật
            if (appointment.Pet != null && request.Weight > 0)
            {
                appointment.Pet.Weight = request.Weight;
                _unitOfWork.Pets.Update(appointment.Pet);
            }
            
            await _unitOfWork.SaveChangesAsync();

            return record.Id;
        }
```

---

## 2. LOGIC TRỪ KHO THEO LÔ VÀ SINH HÓA ĐƠN

Khác với thuốc thông thường dùng luật FEFO tự động trừ, Vắc-xin phải bị trừ chính xác **đúng cái Lô (Batch)** mà bác sĩ cầm trên tay.

```c#
        private async Task ProcessInventoryAndInvoiceAsync(Appointment appointment, VaccinationSoapRequestDto request)
        {
            if (!request.VaccineId.HasValue || !request.VaccineBatchId.HasValue)
                throw new InvalidOperationException("Vui lòng chọn vắc-xin và lô hàng.");

            var batch = await _unitOfWork.VaccineBatches.GetByIdAsync(request.VaccineBatchId.Value);
            var vaccine = await _unitOfWork.Vaccines.GetByIdAsync(request.VaccineId.Value);

            if (batch == null || vaccine == null) throw new KeyNotFoundException("Lô vắc-xin không tồn tại.");

            // VALIDATION: Chặn đứng nếu lô đó đã hết hạn hoặc hết số lượng
            if (batch.StockQuantity <= 0 || batch.ExpirationDate < DateTime.UtcNow)
                throw new InvalidOperationException("Lô vắc-xin đã hết hạn hoặc hết hàng.");

            // 1. TRỪ TỒN KHO 2 CẤP ĐỘ
            // Cấp 1: Trừ trong Lô (Batch)
            batch.StockQuantity -= 1;
            _unitOfWork.VaccineBatches.Update(batch);
            
            // Cấp 2: Trừ số lượng Tổng của loại Vắc-xin đó
            vaccine.StockQuantity -= 1;
            _unitOfWork.Vaccines.Update(vaccine);

            // 2. TÌM VÀ CỘNG DỒN HÓA ĐƠN (INVOICE GENERATION)
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(i => i.AppointmentId == appointment.Id);
            if (invoice == null) // Nếu chưa có hóa đơn nào cho ca khám này
            {
                invoice = new Invoice
                {
                    AppointmentId = appointment.Id,
                    Subtotal = batch.SellingPrice,
                    TotalAmount = batch.SellingPrice, // Giá bán lấy từ Lô hàng, vì mỗi Lô nhập về có giá khác nhau
                    PaymentStatus = "unpaid"
                };
                await _unitOfWork.Invoices.AddAsync(invoice);
            }
            else // Lỡ như đã có hóa đơn (vd khách hàng mua thêm thức ăn ở sảnh)
            {
                invoice.Subtotal += batch.SellingPrice; // Cộng dồn tiền vào
                invoice.TotalAmount += batch.SellingPrice;
                _unitOfWork.Invoices.Update(invoice);
            }
            
            // 3. GHI CHI TIẾT DÒNG HÓA ĐƠN (INVOICE ITEM)
            var invoiceItem = new InvoiceItem
            {
                Invoice = invoice,
                ItemType = "Vaccine",
                ItemId = vaccine.Id,
                // In luôn cả Mã Lô lên hóa đơn để lỡ có sốc phản vệ còn biết đường truy vết
                ItemName = $"Tiêm phòng: {vaccine.Name} (Lô: {batch.BatchNumber})", 
                Quantity = 1,
                UnitPrice = batch.SellingPrice,
                TotalPrice = batch.SellingPrice
            };
            
            await _unitOfWork.InvoiceItems.AddAsync(invoiceItem);
        }
```

---

## 3. LOGIC HOÃN TIÊM NHƯNG VẪN THU PHÍ

Trong ngành thú y, khám để tiêm phòng cũng mất rất nhiều công sức. Nếu bác sĩ phát hiện con vật bị sốt và từ chối tiêm, họ vẫn phải thu phí khám lâm sàng. Hàm này giải quyết triệt để vấn đề đó.

```c#
        private async Task CreateConsultationFeeOnlyAsync(Appointment appointment)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(i => i.AppointmentId == appointment.Id);
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    AppointmentId = appointment.Id,
                    // (MAGIC NUMBER): Hardcode thu 100,000 VNĐ phí công khám
                    // Cải tiến tương lai: Lên lấy phí này từ bảng Service (ServiceId) thay vì hardcode.
                    Subtotal = 100000, 
                    TotalAmount = 100000,
                    PaymentStatus = "unpaid"
                };
                await _unitOfWork.Invoices.AddAsync(invoice);
            }
            
            var invoiceItem = new InvoiceItem
            {
                Invoice = invoice,
                ItemType = "Service",
                ItemId = appointment.ServiceId,
                // Ghi chú rõ trên Hóa đơn cho khách hiểu tại sao đi tiêm mà không tiêm lại mất tiền
                ItemName = "Phí khám lâm sàng (Hoãn tiêm)",
                Quantity = 1,
                UnitPrice = 100000,
                TotalPrice = 100000
            };
            await _unitOfWork.InvoiceItems.AddAsync(invoiceItem);
        }
```

*(Hết tài liệu phân tích Vaccination Service)*
