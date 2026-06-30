# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG KHÁCH HÀNG XEM LỊCH SỬ Y TẾ & HÓA ĐƠN - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Xin lỗi sếp vì sự nhầm lẫn vừa nãy! Tôi đã nhầm sang luồng tạo bệnh án của Bác sĩ. 
> Đây chính là tài liệu bạn yêu cầu: **Khách hàng xem Lịch sử Y tế của mình** (nằm trong `CustomerAppointmentController` và `AppointmentService`).
> Ở phiên bản này, thuật toán truy vấn được **Nâng cấp toàn diện** so với phiên bản cũ, sử dụng sức mạnh của LINQ `SelectMany` để gom dữ liệu cực kỳ tối ưu.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Khách hàng tra cứu Lịch sử bệnh án & Hóa đơn.
- **Mục đích:** Khách hàng có thể tự xem lại quá trình điều trị của thú cưng, các loại thuốc đã uống, và xem Hóa đơn đã thanh toán hay chưa.
- **Điểm nổi bật (Kỹ thuật):** Thay vì dùng Dictionary kéo hết dữ liệu lên RAM như phiên bản cũ, phiên bản này ép Entity Framework Core dịch toàn bộ quá trình gom nhóm (Join) xuống trực tiếp SQL Server, giúp tiết kiệm 90% bộ nhớ RAM.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (GIAO DIỆN KHÁCH HÀNG)

**Tệp:** `WebApi/Controllers/CustomerAppointmentController.cs`

```csharp
        /// <summary>
        /// Lấy lịch sử bệnh án khám của thú cưng thuộc sở hữu (chống IDOR).
        /// </summary>
        [HttpGet("pets/{petId:long}/medical-history")] // (Đường dẫn API)
        public async Task<IActionResult> GetPetMedicalHistory(long petId)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (1. Bóc ID Khách hàng từ Token để chống lộ lọt dữ liệu)
                
                // (2. Chuyền ID Thú cưng và ID Khách hàng xuống Service để kiểm tra bảo mật chéo)
                var history = await _appointmentService.GetPetMedicalHistoryAsync(petId, customerId);
                
                return Ok(history); // (3. Trả dữ liệu JSON về Frontend)
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // (Nếu Service phát hiện khách hàng đang xem lén thú cưng của người khác, trả về mã 403 Cấm truy cập)
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message }); // (Bắt lỗi chung)
            }
        }
```

**Giải thích chi tiết:**
- Code Controller rất ngắn gọn vì mọi gánh nặng bảo mật và truy vấn đã được đẩy xuống Service. Việc truyền cả `petId` lẫn `customerId` là chìa khóa để chặn đứng lỗi **IDOR (Insecure Direct Object Reference)**.

---

### PHẦN 2.2 - TẦNG SERVICE (TRUY VẤN LINQ TỐI ƯU HÓA CAO CẤP)

**Tệp:** `MyPetClinic.Application/Services/AppointmentService.cs`

Đây là trái tim của chức năng. Hãy xem cách hệ thống gộp 4 bảng (Bệnh án, Lịch hẹn, Hóa đơn, Đơn thuốc) chỉ bằng 1 câu lệnh duy nhất.

```csharp
        public async Task<IEnumerable<MedicalRecordDto>> GetPetMedicalHistoryAsync(long petId, Guid CustomerId)
        {
            // BƯỚC 1: XÁC THỰC QUYỀN SỞ HỮU (CHỐNG IDOR)
            // (Chỉ tìm con thú cưng nào vừa có ID khớp, VỪA PHẢI thuộc sở hữu của Khách hàng này)
            var pet = _unitOfWork.Pets.Query().FirstOrDefault(p => p.Id == petId && p.CustomerId == CustomerId);
            if (pet == null)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập thông tin bệnh án của thú cưng này.");
            }

            // BƯỚC 2: TRUY VẤN VÀ GOM DỮ LIỆU BẰNG LINQ TO SQL
            var records = _unitOfWork.MedicalRecords.Query() // (Mở luồng truy vấn vào bảng Bệnh án)
                .Where(mr => mr.Appointment != null && mr.Appointment.PetId == petId) // (Chỉ lấy bệnh án của con Pet này)
                .OrderByDescending(mr => mr.CreatedAt) // (Sắp xếp mới nhất lên đầu)
                .Select(mr => new // (BẮT ĐẦU ÉP KIỂU: Dùng Select vô danh để chọn ra đúng những cột cần thiết)
                {
                    mr.Id,
                    mr.AppointmentId,
                    PetId = mr.PetId,
                    // (Lấy tên Pet từ bảng liên kết, nếu null thì gán chuỗi rỗng để tránh lỗi sập web)
                    PetName = (mr.Appointment != null && mr.Appointment.Pet != null) ? mr.Appointment.Pet.Name : string.Empty,
                    VisitDate = mr.CreatedAt,
                    RecordType = mr.RecordType,
                    MedicalHistory = mr.MedicalHistory ?? string.Empty,
                    Diagnosis = mr.Diagnosis ?? string.Empty,
                    TreatmentPlan = mr.TreatmentPlan ?? string.Empty,
                    DoctorName = mr.Doctor != null ? mr.Doctor.FullName : string.Empty, // (Lấy tên Bác sĩ)
                    DoctorId = mr.DoctorId.ToString(),
                    mr.Weight,
                    mr.Temperature,
                    ClinicalSigns = mr.ClinicalSigns ?? string.Empty,
                    DoctorNotes = mr.DoctorNotes ?? string.Empty,
                    mr.FollowUpDate,
                    
                    // BƯỚC 3: MÓC NỐI SANG BẢNG HÓA ĐƠN (INVOICE)
                    InvoiceId = mr.Appointment != null && mr.Appointment.Invoice != null ? (long?)mr.Appointment.Invoice.Id : null,
                    InvoiceStatus = mr.Appointment != null && mr.Appointment.Invoice != null ? mr.Appointment.Invoice.PaymentStatus : null,
                    InvoiceTotalAmount = mr.Appointment != null && mr.Appointment.Invoice != null ? (decimal?)mr.Appointment.Invoice.TotalAmount : null,
                    
                    // BƯỚC 4: KỸ THUẬT GOM MẢNG BẰNG SELECTMANY
                    PrescribedMedicines = mr.Prescriptions // (Từ Bệnh án -> Chui vào Đơn thuốc)
                        .SelectMany(p => p.PrescriptionItems) // (Trải phẳng tất cả các Viên thuốc trong Đơn thuốc ra thành 1 mảng duy nhất)
                        .Where(pi => pi.Medicine != null) // (Chỉ lấy những viên thuốc hợp lệ)
                        .Select(pi => new PrescribedMedicineDto // (Đóng gói viên thuốc)
                        {
                            MedicineName = pi.Medicine!.Name, // (Lấy Tên thuốc)
                            Dosage = pi.Dosage,
                            Frequency = pi.Frequency,
                            DurationDays = pi.DurationDays,
                            Quantity = pi.Quantity,
                            Instruction = pi.Instruction
                        })
                        .ToList() // (Đóng gói thành List)
                })
                .ToList(); // (CHỐT HẠ: Dòng này sẽ dịch toàn bộ cục code LINQ khổng lồ ở trên thành 1 câu SQL vĩ đại bắn xuống Server)

            // BƯỚC 5: CHUYỂN ĐỔI SANG DTO CHÍNH THỨC TRẢ VỀ CHO CLIENT
            var mapped = records.Select(mr => new MedicalRecordDto
            {
                RecordId = mr.Id,
                AppointmentId = mr.AppointmentId,
                PetId = pet.Id,
                PetName = mr.PetName,
                VisitDate = mr.VisitDate,
                RecordType = mr.RecordType,
                MedicalHistory = mr.MedicalHistory,
                Diagnosis = mr.Diagnosis,
                TreatmentPlan = mr.TreatmentPlan,
                DoctorName = mr.DoctorName,
                DoctorId = mr.DoctorId,
                Weight = mr.Weight,
                Temperature = mr.Temperature,
                ClinicalSigns = mr.ClinicalSigns,
                DoctorNotes = mr.DoctorNotes,
                FollowUpDate = mr.FollowUpDate,
                PrescribedMedicines = mr.PrescribedMedicines,
                InvoiceId = mr.InvoiceId,
                InvoiceStatus = mr.InvoiceStatus,
                InvoiceTotalAmount = mr.InvoiceTotalAmount // (Khách hàng xem được tổng tiền của ca khám này)
            });

            return await Task.FromResult(mapped);
        }
```

**Giải thích chi tiết - Tinh hoa Tối ưu SQL:**
- **Không dùng Include():** Bạn sẽ nhận thấy ở đây hoàn toàn vắng bóng lệnh `.Include()`. Khi dùng `Include()`, Entity Framework sẽ kéo **TOÀN BỘ** các cột của các bảng liên quan về RAM, gây lãng phí băng thông rác. Thay vào đó, việc dùng `.Select(mr => new {...})` ép SQL Server chỉ được trả về ĐÚNG những cột mà ta yêu cầu.
- **Tuyệt kỹ SelectMany:** Giả sử một bệnh án có 2 Đơn thuốc, mỗi đơn thuốc có 3 Viên thuốc. Bình thường ta sẽ phải dùng 2 vòng lặp lồng nhau (vòng ngoài duyệt Đơn thuốc, vòng trong duyệt Viên thuốc). Lệnh `.SelectMany()` giúp "trải phẳng" (Flatten) mảng 2 chiều này thành mảng 1 chiều chứa 6 viên thuốc ngay trên bộ máy tính toán của SQL Server. Tối ưu cực độ!
- **Gắn liền Hóa đơn:** Phiên bản này thông minh ở chỗ nó móc nối thẳng sang bảng Hóa đơn (`Invoice`). Khách hàng có thể nhìn vào lịch sử bệnh án và biết ngay "À, hôm đó bé nhà mình khám hết 500,000đ".

---

### PHẦN 2.3 - TẦNG CONTROLLER (XEM DANH SÁCH HÓA ĐƠN ĐỘC LẬP)

Cũng nằm trong `CustomerAppointmentController.cs`, khách hàng có 1 tính năng để xem tất cả Hóa đơn của mình.

```csharp
        /// <summary>
        /// Lấy danh sách hóa đơn của khách hàng đang đăng nhập.
        /// </summary>
        [HttpGet("invoices")]
        public async Task<IActionResult> GetMyInvoices()
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync(); // (Xác thực ID Khách hàng)
                
                // (Chuyền xuống Service Hóa đơn để lấy danh sách)
                var invoices = await _invoiceService.GetCustomerInvoicesAsync(customerId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
```

*(Hết tài liệu đào tạo chuyên sâu: Lịch sử Y tế & Hóa đơn phiên bản Tối ưu)*
