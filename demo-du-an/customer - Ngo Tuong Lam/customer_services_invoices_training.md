# TÀI LIỆU ĐÀO TẠO NỘI BỘ: DỊCH VỤ & HÓA ĐƠN (CUSTOMER VIEW)

> [!NOTE]
> Đây là tài liệu Đào tạo số 21, dành cho góc nhìn của **Khách hàng (Customer App)**.
> Khác với Admin (thêm, sửa, xóa), Khách hàng đóng vai trò là người **Tiêu thụ (Consumer)**. Họ cần xem danh sách Dịch vụ/Vắc-xin để đặt lịch, và xem lại Hóa đơn thanh toán để đảm bảo tính minh bạch.
> Trọng tâm tài liệu này phân tích: **Bộ lọc Tồn kho Vắc-xin**, **Cơ chế xác thực phác đồ tiêm**, và **Truy xuất Hóa đơn an toàn (Chống IDOR)**.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Giao diện Khách hàng - Dịch vụ & Hóa đơn (Customer Services & Invoices).
- **Mục đích:** Cung cấp thông tin Dịch vụ để khách chọn khi Book lịch. Cung cấp lịch sử Hóa đơn thanh toán viện phí.
- **Điểm nổi bật (Kỹ thuật):** Xử lý luồng chặn lỗi (Ngăn khách chọn Vắc-xin đã hết hàng trong kho). Bảo vệ nghiêm ngặt Lịch sử Hóa đơn, đảm bảo khách A không bao giờ xem lén được Hóa đơn của khách B.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - BỘ LỌC TỒN KHO VẮC-XIN KHI ĐẶT LỊCH

Khi khách hàng mở màn hình Đặt Lịch Hẹn (Booking), nếu họ chọn loại dịch vụ là "Tiêm chủng", App sẽ gọi API lấy danh sách Vắc-xin. Tuy nhiên, hệ thống không bốc toàn bộ vắc-xin ra đưa cho khách!

**Tệp:** `MyPetClinic.Application/Services/CustomerAppointmentService.cs`

```csharp
        public async Task<object> GetAvailableVaccinesAsync()
        {
            // BỘ LỌC TỒN KHO: Chỉ lấy những loại Vắc-xin còn Hàng (StockQuantity > 0)
            var vaccines = await _unitOfWork.Vaccines.FindAsync(v => v.StockQuantity > 0);
            
            // Ép kiểu ẩn danh (Anonymous Object) để cắt gọt dữ liệu nhạy cảm
            // Khách hàng không cần (và không được phép) biết Giá Nhập kho (ImportPrice) của phòng khám!
            return vaccines.OrderBy(v => v.Name).Select(v => new
            {
                id = v.Id,
                name = v.Name,
                description = v.Description,
                targetSpecies = v.TargetSpecies
            });
        }
```

**Giải thích:** Nếu khách hàng đặt lịch tiêm Vắc-xin dại (Rabies) nhưng trong kho đã hết sạch, đến ngày khách dắt chó tới phòng khám Lễ tân lại báo hết thuốc sẽ gây trải nghiệm cực kỳ tệ. Lệnh `v.StockQuantity > 0` đã chặn đứng rủi ro này từ ngay "trứng nước" lúc khách đang mở điện thoại bấm đặt lịch.

---

### PHẦN 2.2 - XÁC THỰC PHÁC ĐỒ TIÊM TRƯỚC KHI BOOKING

Chưa dừng lại ở đó. Sau khi khách chọn được 1 Vắc-xin còn hàng, trước khi bấm nút "Xác nhận Đặt Lịch", App phải gọi một API Validation để kiểm tra xem Thú cưng đó có đủ điều kiện y tế để tiêm mũi này không.

**Tệp:** `MyPetClinic.Application/Services/CustomerAppointmentService.cs`

```csharp
        public async Task<object> ValidateVaccineAsync(Guid customerId, long petId, long vaccineId, DateTime targetDate)
        {
            // 1. Chống Hack IDOR: Mày là ai? Có đúng thú cưng này là của mày không?
            var pets = await _unitOfWork.Pets.FindAsync(p => p.Id == petId && p.CustomerId == customerId);
            var pet = pets.FirstOrDefault();
            if (pet == null)
                throw new InvalidOperationException("Thú cưng không hợp lệ hoặc không thuộc về bạn.");

            var vaccines = await _unitOfWork.Vaccines.FindAsync(v => v.Id == vaccineId);
            var vaccine = vaccines.FirstOrDefault();

            // 2. Tìm cuốn sổ tiêm chủng của bé (Lấy Mũi tiêm gần nhất của loại Vắc-xin này)
            var lastRecords = await _unitOfWork.VaccinationRecords.FindAsync(vr => vr.PetId == petId && vr.VaccineId == vaccineId);
            var lastRecord = lastRecords.OrderByDescending(vr => vr.InjectionDate).FirstOrDefault();

            // 3. Đưa vào Bộ máy chấm điểm Y tế (Kiểm tra xem chó đã đủ 6 tuần tuổi chưa? Hoặc đã cách mũi 1 đủ 21 ngày chưa?)
            var checker = new VaccinationScheduleChecker();
            return checker.ValidateInterval(lastRecord, vaccine, targetDate, pet);
            
            // Nếu checker trả về Lỗi -> App hiện Pop-up Đỏ chặn khách đặt lịch!
        }
```

---

### PHẦN 2.3 - XEM HÓA ĐƠN MINH BẠCH (CHỐNG LỖI IDOR)

Phòng khám thú y hiện đại đề cao sự minh bạch. Khách hàng về nhà có thể mở App lên và xem lại Hóa đơn chi tiết (Tiền khám bao nhiêu, thuốc giảm đau tên gì, tiêm mấy mũi, tổng tiền).

**Tệp:** `MyPetClinic.Application/Services/InvoiceService.cs`

```csharp
        public async Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(System.Guid customerId)
        {
            // BỘ LỌC CHỐNG IDOR XUYÊN THẤU
            // Lọc toàn bộ bảng Hóa Đơn, rẽ nhánh sang bảng Lịch Hẹn, kiểm tra xem CustomerId có khớp với cái Token đăng nhập trên điện thoại không!
            var invoicesList = await _unitOfWork.Invoices.FindWithIncludesAsync(
                i => i.Appointment != null && i.Appointment.CustomerId == customerId,
                
                // Kéo theo CÁC MÓN HÀNG trong hóa đơn
                i => i.InvoiceItems,
                
                // Kéo theo Tên bác sĩ và Tên thú cưng
                i => i.Appointment!.Customer!,
                i => i.Appointment!.Pet!,
                i => i.Appointment!.Doctor!
            );

            // Sắp xếp: Hóa đơn mới nhất nổi lên đầu
            var invoices = invoicesList
                .OrderByDescending(i => i.CreatedAt)
                .ToList();

            // Map qua DTO để giấu đi các biến nhạy cảm của hệ thống
            return invoices.Select(MapToDto).ToList();
        }
```

**Tệp:** DTO trả về cho Khách hàng (`MyPetClinic.Application/DTOs/InvoiceDto.cs`)

```csharp
            // Dữ liệu khách hàng nhìn thấy trên App
            return new InvoiceDto
            {
                Id = invoice.Id,
                Subtotal = invoice.Subtotal, // 500k
                DiscountAmount = invoice.DiscountAmount, // Trừ 50k
                TotalAmount = invoice.TotalAmount, // Tổng: 450k
                PaymentStatus = invoice.PaymentStatus, // "paid"
                PaidAt = invoice.PaidAt, // Ngày giờ thanh toán
                
                DoctorName = invoice.Appointment?.Doctor?.FullName, // Bác sĩ khám: Dr. John
                PetName = invoice.Appointment?.Pet?.Name ?? "Thú cưng",
                
                // Danh sách chi tiết các món hàng Khách đã dùng (Để khách không bị bỡ ngỡ vì sao mất nhiều tiền)
                Items = invoice.InvoiceItems.Select(ii => new InvoiceItemDto
                {
                    ItemType = ii.ItemType, // "service" hoặc "medicine"
                    ItemName = ii.ItemName, // VD: "Siêu âm thai" hoặc "Thuốc tẩy giun"
                    Quantity = ii.Quantity, // Số lượng: 2
                    UnitPrice = ii.UnitPrice, // Đơn giá: 100k
                    TotalPrice = ii.TotalPrice // Thành tiền: 200k
                }).OrderBy(ii => ii.ItemType).ToList()
            };
```

**Tổng kết:** Module này phản ánh đúng triết lý của App MyPetClinic: Khách hàng được phục vụ tận răng từ khâu gợi ý vắc-xin thông minh (tránh thiếu hàng, tránh sai độ tuổi tiêm) cho đến khâu xem lại hóa đơn minh bạch. Đồng thời, Backend luôn dựng sẵn tường lửa bằng mệnh đề `CustomerId == customerId` để chặn mọi hacker có ý đồ soi lén thông tin của người khác.

---
*(Hết tài liệu đào tạo: Dịch vụ & Hóa đơn - Customer View)*
