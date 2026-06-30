# TÀI LIỆU ĐÀO TẠO NỘI BỘ: THIẾT LẬP BỆNH ÁN & TIÊM CHỦNG (DOCTOR) - BẢN FULL DEEP DIVE ENTITY & DTO

> [!NOTE]
> Đây là tài liệu Đào tạo số 13. Khác với các hệ thống phòng khám thông thường chỉ có 1 form ghi chú chung chung, MyPetClinic phân tách rạch ròi 2 luồng chuyên môn: **Khám bệnh lâm sàng (Clinical)** và **Tiêm chủng (Vaccination)**.
> Trọng tâm kỹ thuật của mô-đun này là kiến trúc **DTO lồng nhau (Nested DTO)** để xử lý form SOAP khổng lồ, kỹ thuật **Cross-Validation (Xác thực chéo)** và **Tầng Service tự động xử lý Tồn kho FEFO & Hóa đơn**.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Ghi nhận Bệnh án Khám bệnh & Sổ Tiêm chủng (Lưu thông tin điền trường).
- **Mục đích:** Cung cấp API (Controller) để Giao diện UI có thể "điền trường" (submit) form Bệnh án điện tử chuẩn S.O.A.P.
- **Điểm nổi bật (Kỹ thuật):** Tách biệt 2 bảng Database riêng (`MedicalRecord` và `VaccinationRecord`). Tầng Controller được bảo vệ bởi Role `doctor`. Tầng Service được trang bị thuật toán FEFO (First-Expired-First-Out) để tự động xuất lô thuốc hết hạn trước.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - LUỒNG KHÁM BỆNH LÂM SÀNG: THỰC THỂ & ĐÓNG GÓI (ENTITY & DTO)

Khám bệnh lâm sàng cần điền hàng chục trường thông tin từ Hệ Tiêu hóa, Hô hấp đến Thần kinh. Do đó, form DTO phải được chia nhỏ (Nested) để frontend dễ map dữ liệu.

**Tệp:** `MyPetClinic.Domain/Entities/MedicalRecord.cs` & `MyPetClinic.Application/DTOs/MedicalRecordSoapRequestDto.cs`

```csharp
// --- THỰC THỂ BỆNH ÁN GỐC (DB TABLE) ---
    public class MedicalRecord
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; } // Liên kết với Ca khám hiện tại
        public Guid DoctorId { get; set; }
        
        // (Lưu trữ theo chuẩn SOAP 4 phần)
        public string? MedicalHistory { get; set; } // S - Bệnh sử
        public decimal Weight { get; set; } // O - Cân nặng
        public string ClinicalSigns { get; set; } = string.Empty; // O - Triệu chứng lâm sàng
        public string Diagnosis { get; set; } = string.Empty; // A - Chẩn đoán
        public string TreatmentPlan { get; set; } = string.Empty; // P - Phác đồ điều trị
        
        // (1 Bệnh án có thể đẻ ra N tờ Đơn thuốc)
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }

// --- LỚP VỎ DTO TRUYỀN TẢI (NESTED DTO CHO VIỆC ĐIỀN TRƯỜNG) ---
    public class MedicalRecordSoapRequestDto
    {
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        
        // (KỸ THUẬT NESTED DTO: Chia nhỏ 1 cục JSON khổng lồ thành 4 object nhỏ)
        public SubjectiveDto Subjective { get; set; } = new(); 
        public ObjectiveDto Objective { get; set; } = new(); 
        public AssessmentDto Assessment { get; set; } = new(); 
        public PlanDto Plan { get; set; } = new(); 
    }

    public class ObjectiveDto
    {
        // (Lại tiếp tục lồng nhau để phân rã 7 hệ cơ quan)
        public SystemExamDto Eyes { get; set; } = new(); // Mắt
        public SystemExamDto Ears { get; set; } = new(); // Tai
        public SystemExamDto Gastrointestinal { get; set; } = new(); // Tiêu hóa
        // ...
    }
```

**Giải thích chi tiết:**
- **Nested DTO:** Frontend UI khi "điền trường" sẽ đóng gói 1 cục JSON có cấu trúc cây rất đẹp: `{"Objective": {"Eyes": {"IsNormal": true}}}`. Cách thiết kế này giúp Backend code C# rất sạch sẽ, dễ dàng Mapping mà không bị ngợp bởi 50 tham số truyền vào hàm.

---

### PHẦN 2.2 - LUỒNG KHÁM BỆNH LÂM SÀNG: TẦNG CONTROLLER & SERVICE

Sau khi Bác sĩ bấm nút "Lưu Bệnh án" trên UI, cục dữ liệu Nested DTO khổng lồ sẽ được bắn vào Controller này.

**Tệp:** `WebApi/Controllers/MedicalRecordsController.cs` & `MyPetClinic.Application/Services/MedicalRecordService.cs`

```csharp
// --- TẦNG CONTROLLER (ĐIỂM NHẬN DỮ LIỆU ĐIỀN TRƯỜNG) ---
    [Authorize(Roles = "doctor,admin,clinical_doctor")]
    [ApiController]
    [Route("api/medical-records")]
    public class MedicalRecordsController : ControllerBase
    {
        [HttpPost("soap")]
        public async Task<IActionResult> CreateSoapMedicalRecord([FromBody] MedicalRecordSoapRequestDto dto)
        {
            // (1. Bóc ID bác sĩ từ Token để bảo mật)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var doctorId)) return BadRequest();

            // (2. Đẩy thẳng xuống Service xử lý Logic)
            var recordId = await _medicalRecordService.CreateSoapMedicalRecordAsync(dto, doctorId);
            return Ok(new { success = true, recordId });
        }
    }

// --- TẦNG SERVICE (TRỪ KHO FEFO CHỐNG CHÁY KHO) ---
        public async Task<long> CreateMedicalRecordAsync(CreateMedicalRecordDto dto, Guid doctorId)
        {
            // BƯỚC 1: KHÓA DATABASE CHỐNG TRANH CHẤP (DEADLOCK)
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                // BƯỚC 2: TẠO HỒ SƠ BỆNH ÁN
                var medicalRecord = new MedicalRecord { ... }; // (Đổ dữ liệu từ DTO sang)
                await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);
                await _unitOfWork.SaveChangesAsync(); 

                // BƯỚC 3: XỬ LÝ ĐƠN THUỐC VÀ KHO HÀNG FEFO
                if (dto.Prescriptions != null && dto.Prescriptions.Any())
                {
                    // VÒNG LẶP TRỪ KHO THEO THUẬT TOÁN FEFO (First-Expired-First-Out)
                    foreach (var item in dto.Prescriptions)
                    {
                        // Thay vì tự ý trừ kho, Service này "nhờ" MedicineService chạy thuật toán thông minh
                        await _medicineService.ExportMedicineAsync(new ExportMedicineDto
                        {
                            MedicineId = item.MedicineId,
                            Quantity = item.Quantity,
                            ReferenceCode = $"MR-{medicalRecord.Id}"
                        }, doctorId);
                    }
                }

                // BƯỚC 4: KẾT THÚC CA KHÁM VÀ BÁO CHO THU NGÂN
                appointment.Status = "ready_to_pay";
                _unitOfWork.Appointments.Update(appointment);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync(); 
                return medicalRecord.Id;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(); // (Rollback toàn vẹn Kho thuốc)
                throw;
            }
        }
```

**Giải thích chi tiết (Tinh hoa thuật toán Kho):**
- **IsolationLevel.RepeatableRead:** Nếu có 2 ca cấp cứu vào cùng lúc, 2 Bác sĩ cùng lên phần mềm "điền trường" kê lọ kháng sinh cuối cùng trong Kho. Ai bấm "Lưu" trước người đó được, người bấm sau lập tức bị văng thông báo "Không đủ tồn kho". Tuyệt đối không bao giờ Kho bị Âm số lượng.
- **Thuật toán FEFO (First-Expired-First-Out):** Thuật toán `ExportMedicineAsync` sẽ lục tung trong Kho xem Lô thuốc nào **Gần Hết Hạn Nhất**, nó sẽ tự động bốc lô đó ra bán cho Khách hàng.

---

### PHẦN 2.3 - LUỒNG TIÊM CHỦNG: THỰC THỂ & XÁC THỰC CHÉO

Tiêm chủng nguy hiểm ở chỗ có rủi ro Sốc phản vệ (Anaphylaxis). Việc "điền trường" (Validation) phải cực kỳ khắt khe bằng kỹ thuật **Xác thực chéo (Cross-Validation)**.

**Tệp:** `MyPetClinic.Application/DTOs/VaccinationSoapRequestDto.cs`

```csharp
    // (BÍ QUYẾT: Kế thừa Interface IValidatableObject để viết logic kiểm tra form phức tạp)
    public class VaccinationSoapRequestDto : IValidatableObject
    {
        // ... (Các trường cơ bản)
        public string ClinicalAssessment { get; set; } = string.Empty; // Kết luận của Bác sĩ (Đủ ĐK / Hoãn tiêm)
        public long? VaccineId { get; set; } // ID của lọ vaccine sẽ tiêm
        
        // HÀM KIỂM TRA FORM ĐA TẦNG KHI BÁC SĨ BẤM NÚT ĐIỀN
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // (LUẬT 1: Nếu Bác sĩ điền form chọn "Đủ điều kiện tiêm", thì BẮT BUỘC phải điền thêm lọ Vaccine)
            if (ClinicalAssessment == "Đủ điều kiện" && (!VaccineId.HasValue || VaccineId.Value <= 0))
            {
                yield return new ValidationResult("Vui lòng chọn vaccine sử dụng", new[] { nameof(VaccineId) });
            }
            // ... (Các luật khác: Hẹn ngày nhắc lại không được nằm trong quá khứ)
        }
    }
```

**Giải thích chi tiết:**
- Bằng cách implement `IValidatableObject`, framework ASP.NET Core sẽ **tự động** chạy hàm `Validate()` này ngay tại thời điểm DTO chui qua cửa Controller. Nếu bác sĩ "điền trường" sai luật, hệ thống tự động văng lỗi 400 Bad Request ngay tắp lự.

---

### PHẦN 2.4 - LUỒNG TIÊM CHỦNG: TẦNG CONTROLLER & SERVICE

**Tệp:** `WebApi/Controllers/VaccinationsController.cs` & `MyPetClinic.Application/Services/VaccinationService.cs`

```csharp
// --- TẦNG CONTROLLER (ĐIỂM NHẬN FORM TIÊM CHỦNG) ---
    [Authorize(Roles = "doctor,admin,vaccination_doctor")]
    [ApiController]
    [Route("api/vaccinations")]
    public class VaccinationsController : ControllerBase
    {
        [HttpPost("appointments/{appointmentId}")]
        public async Task<IActionResult> SubmitSoapRecord(long appointmentId, [FromBody] VaccinationSoapRequestDto request)
        {
            // (Hàng rào bảo vệ: DTO đã tự chạy hàm Validate(). Nếu điền sai thì văng lỗi luôn)
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var recordId = await _vaccinationService.SubmitSoapRecordAsync(appointmentId, doctorId, request);
            return Ok(new { success = true, recordId });
        }
    }

// --- TẦNG SERVICE (LÊN BILL THÔNG MINH) ---
        public async Task<long> SubmitSoapRecordAsync(long appointmentId, Guid doctorId, VaccinationSoapRequestDto request)
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
            var record = CreateVaccinationRecord(appointment, doctorId, request);
            
            // LOGIC RẼ NHÁNH TỰ ĐỘNG LÊN BILL DỰA VÀO FORM ĐIỀN TRƯỜNG: ĐỦ ĐIỀU KIỆN TIÊM HAY HOÃN TIÊM?
            if (request.ClinicalAssessment == "Đủ điều kiện")
            {
                // (Nếu Tiêm: Trừ tồn kho lô Vaccine + Xuất hóa đơn tiền lọ Vaccine)
                await ProcessInventoryAndInvoiceAsync(appointment, request);
            }
            else if (request.ClinicalAssessment == "Hoãn tiêm")
            {
                // (Nếu Hoãn tiêm vì chó ốm: KHÔNG trừ kho Vaccine, nhưng hệ thống thông minh xuất Hóa đơn tính "Phí Khám Lâm Sàng" 100k)
                await CreateConsultationFeeOnlyAsync(appointment);
            }

            appointment.Status = "completed";
            appointment.CheckOutTime = DateTime.UtcNow;
            
            // LƯU DB
            _unitOfWork.Appointments.Update(appointment);
            await _unitOfWork.VaccinationRecords.AddAsync(record);
            await _unitOfWork.SaveChangesAsync(); 

            return record.Id;
        }
```
**Giải thích chi tiết:**
- Khác với thuốc thông thường, Vắc-xin quản lý theo mã Lô Hàng cứng (VaccineBatchId) mà Bác sĩ điền vào, do đó nó không chạy thuật toán xuất tự động FEFO mà trừ thẳng vào Lô đó.
- Đặc biệt, hệ thống xử lý dòng tiền (Billing Intelligence) rất tự động: Khách đi về tay không (Hoãn tiêm) vẫn tự động xuất bill tiền công khám!

---
*(Hết tài liệu đào tạo chuyên sâu Bác sĩ: Thiết lập Hồ sơ Bệnh án & Tiêm chủng)*
