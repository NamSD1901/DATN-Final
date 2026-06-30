# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG THU NGÂN & HÓA ĐƠN (RECEPTIONIST) - BẢN FULL DEEP DIVE

> [!NOTE]
> Đây là tài liệu Đào tạo số 11 (Đã Bổ sung cấu trúc Entity & DTO). Sau khi Khách hàng khám xong, nhiệm vụ cuối cùng của Lễ tân là thu tiền.
> Hệ thống MyPetClinic được thiết kế để Lễ tân **KHÔNG PHẢI NHẬP TAY LẠI** bất kỳ thông tin nào. Mọi dịch vụ Bác sĩ đã khám, mọi đơn thuốc Bác sĩ đã kê đều được hệ thống tự động gom vào Hóa đơn (Auto-Capture). Hơn thế nữa, khi nhấn "Thanh toán", kho thuốc sẽ tự động bị trừ đi lượng tương ứng.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Quản lý Thanh toán (Billing / Checkout).
- **Mục đích:** Lên bill tính tiền cho khách. Xử lý các nghiệp vụ thêm/bớt hàng hóa (ví dụ: mua thêm thức ăn, đồ chơi), áp dụng mã giảm giá và thanh toán.
- **Điểm nổi bật (Kỹ thuật):** Thuật toán dò tìm đa tầng (Cross-table Lookup) để móc nối Dịch vụ (Service) và Đơn thuốc (Prescription) từ Bệnh án (Medical Record) sang Hóa đơn (Invoice). Tích hợp logic xử lý Tồn kho (Inventory Deduction) chạy chung trong một giao dịch (Transaction).

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - THỰC THỂ (ENTITY) VÀ ĐÓNG GÓI (DTO) - TRÁI TIM CỦA THU NGÂN

Để đảm bảo tính toán tiền bạc tuyệt đối chính xác (chuẩn xác tới từng chữ số thập phân), cấu trúc Database phải được thiết kế theo dạng Quan hệ 1-N (1 Hóa đơn chứa N Dòng chi tiết).

**Tệp:** `MyPetClinic.Domain/Entities/Invoice.cs` & `InvoiceItem.cs`

```csharp
// --- THỰC THỂ HÓA ĐƠN GỐC (INVOICE) ---
    public class Invoice
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; } // (Gắn với ca khám nào?)
        
        // (Kiểu dữ liệu Decimal để lưu Tiền Tệ, tránh sai số làm tròn của kiểu Float/Double)
        public decimal Subtotal { get; set; } = 0; 
        public decimal DiscountAmount { get; set; } = 0; 
        public decimal TotalAmount { get; set; } = 0; 
        
        public string PaymentStatus { get; set; } = "unpaid"; // (Chưa trả, Đã trả)
        public string? PaymentMethod { get; set; } // (Tiền mặt, Chuyển khoản, Quẹt thẻ)
        
        // (Danh sách các Món đồ/Dịch vụ mua trong Hóa đơn này - Quan hệ 1-N)
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }

// --- THỰC THỂ DÒNG CHI TIẾT HÓA ĐƠN (INVOICE ITEM) ---
    public class InvoiceItem
    {
        public long InvoiceId { get; set; } // (Thuộc bill nào?)
        public string? ItemType { get; set; } // (Phân loại: "service" hay "medicine"?)
        public long? ItemId { get; set; }
        public string? ItemName { get; set; } // (Lưu cứng Tên món hàng tại thời điểm mua, lỡ sau này Dịch vụ đổi tên thì Hóa đơn cũ không bị đổi theo)
        
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0; // (Lưu cứng Giá tiền tại thời điểm mua)
        public decimal TotalPrice { get; set; } = 0;
    }
```

**Giải thích chi tiết:**
- Kỹ thuật **Snapshot (Lưu ảnh chụp)**: Trong bảng `InvoiceItem`, hệ thống không chỉ lưu `ItemId`, mà nó CỐ TÌNH copy cả `ItemName` và `UnitPrice` lưu chết vào đó. Tại sao? Vì hôm nay giá Khám là 100k, tháng sau lạm phát tăng giá lên 200k. Nếu ta không lưu cứng giá vào `InvoiceItem` mà dùng JOIN để gọi ra, thì lúc xem lại các Bill của tháng trước sẽ bị đội giá lên thành 200k. Đây là quy tắc bất di bất dịch của Kế toán.

---

### PHẦN 2.2 - TẦNG CONTROLLER (ĐIỂM CHẠM GIAO DIỆN)

**Tệp:** `WebApi/Controllers/InvoiceController.cs`

```csharp
    [Authorize(Roles = "receptionist,admin,Receptionist,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        // ... (Khai báo Service)

        /// <summary>
        /// Lấy danh sách những Khách đang xếp hàng chờ đóng tiền
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingCheckouts()
        {
            var queue = await _invoiceService.GetPendingCheckoutsAsync();
            return Ok(queue);
        }

        /// <summary>
        /// Bấm vào 1 Khách để xem/lập Hóa đơn chi tiết
        /// </summary>
        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetDetail(long appointmentId)
        {
            // (Nếu chưa có Hóa đơn thì hệ thống tự sinh 1 cái mới dựa trên Bệnh án)
            var invoice = await _invoiceService.GetOrCreateInvoiceAsync(appointmentId);
            return Ok(invoice);
        }
    }
```

---

### PHẦN 2.3 - TẦNG SERVICE (THUẬT TOÁN TỰ ĐỘNG GOM CHI PHÍ - AUTO CAPTURE)

**Tệp:** `MyPetClinic.Application/Services/InvoiceService.cs`

Khi Lễ tân bấm vào xem Bill của một khách, hệ thống không chỉ Select bảng Invoice, mà nó lội ngược dòng vào phòng khám để xem Bác sĩ đã làm gì với khách hàng này.

```csharp
        public async Task<InvoiceDto> GetOrCreateInvoiceAsync(long appointmentId)
        {
            // (1. Tìm xem Hóa đơn này đã được tạo trước đó chưa?)
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(i => i.AppointmentId == appointmentId, ...);
            if (invoice != null) return MapToDto(invoice); 

            // NẾU CHƯA CÓ: BẮT ĐẦU QUY TRÌNH GOM CHI PHÍ TỰ ĐỘNG
            var appointment = await _unitOfWork.Appointments.GetFirstOrDefaultWithIncludesAsync(a => a.Id == appointmentId, ...);
            invoice = new Invoice { AppointmentId = appointmentId, PaymentStatus = "unpaid", CreatedAt = DateTime.UtcNow };

            // BƯỚC 1: CỘNG TIỀN CÔNG DỊCH VỤ (Service Charge)
            if (appointment.Service != null)
            {
                invoice.InvoiceItems.Add(new InvoiceItem
                {
                    ItemType = "service",
                    ItemId = appointment.ServiceId,
                    ItemName = appointment.Service.Name,
                    Quantity = 1,
                    UnitPrice = appointment.Service.Price ?? 0,
                    TotalPrice = appointment.Service.Price ?? 0
                });
            }

            // BƯỚC 2: CỘNG TIỀN THUỐC BÁC SĨ ĐÃ KÊ (Prescription Capture)
            // (Dò ngược về bảng Bệnh Án (MedicalRecord) của ca này)
            var medicalRecord = await _unitOfWork.MedicalRecords.GetFirstOrDefaultWithIncludesAsync(mr => mr.AppointmentId == appointmentId);

            if (medicalRecord != null)
            {
                // (Móc tiếp vào bảng Đơn thuốc của Bệnh án đó)
                var prescriptions = await _unitOfWork.Prescriptions.FindWithIncludesAsync(p => p.MedicalRecordId == medicalRecord.Id);
                var prescriptionIds = prescriptions.Select(p => p.Id).ToList();
                
                if (prescriptionIds.Any())
                {
                    var prescriptionItems = await _unitOfWork.PrescriptionItems.FindWithIncludesAsync(pi => prescriptionIds.Contains(pi.PrescriptionId), pi => pi.Medicine!);

                    foreach (var pi in prescriptionItems)
                    {
                        if (pi.Medicine != null)
                        {
                            invoice.InvoiceItems.Add(new InvoiceItem
                            {
                                ItemType = "medicine",
                                ItemId = pi.MedicineId,
                                ItemName = pi.Medicine.Name,
                                Quantity = pi.Quantity ?? 1,
                                UnitPrice = pi.Medicine.SellPrice,
                                TotalPrice = (pi.Quantity ?? 1) * pi.Medicine.SellPrice
                            });
                        }
                    }
                }
            }

            // BƯỚC 3: TỔNG KẾT VÀ LƯU XUỐNG DB
            invoice.Subtotal = invoice.InvoiceItems.Sum(ii => ii.TotalPrice); // (Tính tổng tiền bằng LINQ)
            invoice.TotalAmount = invoice.Subtotal;
            
            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync(); 
        }
```

**Giải thích chi tiết:**
- **Triết lý One-Source-of-Truth:** Tiền công khám bệnh đến từ bảng `Service`. Tiền thuốc đến từ bảng `PrescriptionItem`. Thuật toán lội qua 4 bảng để nhặt từng đồng tiền về 1 chỗ. Lễ tân không bao giờ phải hạch toán lại bằng tay, rủi ro thất thoát doanh thu là 0%.

---

### PHẦN 2.4 - TẦNG SERVICE (THANH TOÁN & TRỪ TỒN KHO)

Khách hàng rút thẻ quẹt hoặc đưa tiền mặt, Lễ tân bấm "Xác nhận thanh toán". 3 sự kiện sẽ diễn ra ĐỒNG THỜI.

```csharp
        public async Task<bool> ProcessPaymentAsync(long invoiceId, string paymentMethod, decimal discountAmount)
        {
            var invoice = await _unitOfWork.Invoices.GetFirstOrDefaultWithIncludesAsync(i => i.Id == invoiceId, i => i.Appointment!);
            if (invoice == null) return false;
            if (invoice.PaymentStatus == "paid") return true; // (Chống đúp bill - Idempotency)

            // SỰ KIỆN 1: CẬP NHẬT HÓA ĐƠN
            invoice.PaymentStatus = "paid";
            invoice.PaymentMethod = paymentMethod;
            invoice.TotalAmount = invoice.Subtotal - discountAmount;
            invoice.PaidAt = DateTime.UtcNow;

            // SỰ KIỆN 2: KẾT THÚC VÒNG ĐỜI CA KHÁM
            if (invoice.Appointment != null)
            {
                invoice.Appointment.Status = "completed"; // (Đuổi khách ra khỏi Kanban)
                invoice.Appointment.CheckOutTime = DateTime.UtcNow; 
            }

            // SỰ KIỆN 3: XUẤT KHO TỰ ĐỘNG (INVENTORY DEDUCTION)
            var invoiceItems = await _unitOfWork.InvoiceItems.FindAsync(ii => ii.InvoiceId == invoiceId && ii.ItemType == "medicine");

            foreach (var item in invoiceItems)
            {
                if (item.ItemId.HasValue)
                {
                    // (Lấy lọ thuốc ra khỏi kho)
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.ItemId.Value);
                    if (medicine != null)
                    {
                        medicine.StockQuantity -= item.Quantity; // (Trừ đi số lượng đã bán)
                        if (medicine.StockQuantity < 0) medicine.StockQuantity = 0; // (Chống âm kho)
                    }
                }
            }

            // GÓI TẤT CẢ VÀO 1 TRANSACTION NGẦM VÀ LƯU XUỐNG
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
```

**Giải thích chi tiết (ACID Transaction):**
- Việc trừ tồn kho (`medicine.StockQuantity -= item.Quantity`) được thực hiện ngay trước lệnh `SaveChangesAsync()`. Điều này đảm bảo Tính toàn vẹn Dữ liệu. Nếu cúp điện lúc đang thanh toán dở, mọi thứ sẽ rollback: Tiền chưa thu, Hóa đơn chưa cập nhật, Thuốc chưa trừ kho. Tuyệt đối không bao giờ có chuyện tiền đã thu mà kho không trừ.

---
*(Hết tài liệu đào tạo chuyên sâu Lễ tân: Quản lý Thu ngân và Hóa đơn)*
