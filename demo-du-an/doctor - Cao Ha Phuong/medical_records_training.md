# TÀI LIỆU ĐÀO TẠO NỘI BỘ: CHỨC NĂNG QUẢN LÝ BỆNH ÁN (MEDICAL RECORDS - SOAP FORMAT) - BẢN FULL DEEP DIVE (COMBO GIẢI THÍCH KÉP)

> [!NOTE]
> Khác với tài liệu số 3 (Khách hàng xem hồ sơ), tài liệu số 5 này xoáy sâu vào **Góc nhìn của Bác sĩ (Doctor)** khi thao tác tạo Bệnh án.
> Đây là chức năng phức tạp bậc nhất hệ thống vì nó áp dụng chuẩn y khoa quốc tế **SOAP** (Subjective, Objective, Assessment, Plan), đồng thời móc nối trực tiếp sang phân hệ **Quản lý Kho thuốc (Inventory)**.
> Tài liệu vẫn giữ nguyên phong cách **"Giải thích Kép"** truyền thống không bỏ sót bất kỳ một dòng logic nào.

---

## 1. Tổng quan chức năng
- **Tên chức năng:** Bác sĩ tạo và quản lý Bệnh án điện tử.
- **Mục đích:** Ghi nhận lại toàn bộ quá trình khám chữa bệnh. Từ lúc hỏi bệnh (S), khám lâm sàng (O), chẩn đoán (A) cho đến kê đơn và lên phác đồ (P).
- **Quy tắc kinh doanh (Business Rules):** 
  - Lưu dữ liệu dưới định dạng chuỗi JSON nguyên bản để dễ dàng vẽ biểu đồ trên Frontend.
  - Tự động kiểm tra tồn kho thuốc. Nếu kho không đủ, chặn ngay không cho kê đơn.
  - Tự động liên kết với phân hệ Kho để Xuất kho thuốc (Trừ số lượng) theo chuẩn FEFO (Hết hạn trước xuất trước).
  - Khám xong tự động đổi trạng thái lịch hẹn thành `ready_to_pay` đẩy ra ngoài cho Lễ tân thu tiền.

---

## 2. PHÂN TÍCH TỪNG DÒNG CODE CHI TIẾT (FULL DEEP DIVE)

### PHẦN 2.1 - TẦNG CONTROLLER (GIAO DIỆN BÁC SĨ)

**Tệp:** `WebApi/Controllers/MedicalRecordsController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    // (CHỐT CHẶN BẢO MẬT: Chỉ những role thuộc phe "Nội bộ" mới được phép gọi API này)
    [Authorize(Roles = "doctor,admin,receptionist,clinical_doctor,vaccination_doctor")]
    [ApiController]
    [Route("api/medical-records")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService; // (Tiêm dịch vụ bệnh án)

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        // (API TẠO BỆNH ÁN CHUẨN SOAP DÀNH CHO BÁC SĨ)
        [HttpPost("soap")]
        public async Task<IActionResult> CreateSoapMedicalRecord([FromBody] MedicalRecordSoapRequestDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // (Lấy ID của Bác sĩ từ Token)
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!Guid.TryParse(userIdStr, out var doctorId))
            {
                return BadRequest(new { message = "DoctorId không hợp lệ." });
            }

            try
            {
                // (Bàn giao dữ liệu thô và ID Bác sĩ cho Service xử lý)
                var recordId = await _medicalRecordService.CreateSoapMedicalRecordAsync(dto, doctorId);
                return Ok(new { success = true, recordId }); // (Trả về mã Bệnh án vừa tạo)
            }
            catch (KeyNotFoundException ex)
            {
                // (Bắt lỗi Không tìm thấy thuốc, Không tìm thấy cuộc hẹn...)
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // (Bắt lỗi Không đủ Tồn kho thuốc...)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // (Bắt lỗi sập Server)
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
    }
}
```

**Giải thích chi tiết:**
- Điểm khác biệt lớn nhất nằm ở `[Authorize(Roles = "...")]`. Khách hàng (`customer`) tuyệt đối không có cửa vào đây. Chỉ có Bác sĩ và Quản trị viên mới được thao tác thêm/sửa bệnh án.
- Exception Handling (Xử lý ngoại lệ) được chia làm nhiều tầng: `NotFound` cho dữ liệu thiếu, `BadRequest` cho nghiệp vụ sai (thiếu thuốc), và `500` cho lỗi máy chủ. Giúp Frontend dễ dàng báo lỗi bằng các Popup màu sắc khác nhau.

---

### PHẦN 2.2 - TẦNG SERVICE (TẠO BỆNH ÁN SOAP VÀ TRỪ TỒN KHO)

**Tệp:** `MyPetClinic.Application/Services/MedicalRecordService.cs`

Đây là nơi thực thi **Giao dịch đa bảng (UnitOfWork Transaction)**: Vừa tạo bệnh án, vừa tạo đơn thuốc, vừa trừ số lượng trong kho, vừa đổi trạng thái thanh toán. Nếu một trong 4 việc này lỗi, hủy bỏ toàn bộ.

```csharp
        public async Task<long> CreateSoapMedicalRecordAsync(MedicalRecordSoapRequestDto dto, Guid doctorId)
        {
            // (BƯỚC BẢO VỆ DỮ LIỆU: Mở Giao dịch (Transaction) khóa các bảng liên quan)
            // (IsolationLevel.RepeatableRead: Đảm bảo trong lúc đang trừ thuốc, không ai khác được vô lấy mất thuốc)
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                // 1. Kéo thông tin Lịch hẹn ra
                var appointments = await _unitOfWork.Appointments.FindWithIncludesAsync(a => a.Id == dto.AppointmentId, a => a.Pet!);
                var appointment = appointments.FirstOrDefault();
                if (appointment == null) throw new KeyNotFoundException("Không tìm thấy cuộc hẹn.");

                // (Cấu hình chuẩn hóa JSON: Chuyển tên biến C# thành camelCase giống JavaScript)
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                
                // 2. Nhồi dữ liệu SOAP (S-O-A-P) thành các chuỗi JSON để lưu vào SQL Server
                var medicalRecord = new MedicalRecord
                {
                    AppointmentId = dto.AppointmentId,
                    DoctorId = doctorId,
                    PetId = dto.PetId == 0 ? appointment.PetId : dto.PetId, // (Lấy ID con vật đang khám)
                    RecordType = "Consultation",
                    MedicalHistory = JsonSerializer.Serialize(dto.Subjective, options), // (S: Hỏi bệnh - Lưu thành JSON)
                    Weight = dto.Objective.Weight,
                    Temperature = dto.Objective.Temperature ?? 0,
                    ClinicalSigns = JsonSerializer.Serialize(dto.Objective, options), // (O: Khám lâm sàng - JSON)
                    Diagnosis = JsonSerializer.Serialize(dto.Assessment, options), // (A: Chẩn đoán - JSON)
                    TreatmentPlan = JsonSerializer.Serialize(dto.Plan.TreatmentDirections, options), // (P: Phác đồ - JSON)
                    DoctorNotes = dto.Plan.CareInstructions,
                    FollowUpDate = dto.Plan.FollowUpDate,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MedicalRecords.AddAsync(medicalRecord);
                await _unitOfWork.SaveChangesAsync(); // (Lưu tạm để lấy được cái ID của Bệnh án)

                // 3. XỬ LÝ ĐƠN THUỐC VÀ KHO HÀNG (Nếu bác sĩ có kê thuốc)
                if (dto.Plan.Prescriptions != null && dto.Plan.Prescriptions.Any())
                {
                    var prescription = new Prescription // (Tạo tờ Đơn thuốc)
                    {
                        MedicalRecordId = medicalRecord.Id,
                        DoctorId = doctorId,
                        Note = dto.Plan.CareInstructions,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Prescriptions.AddAsync(prescription);
                    await _unitOfWork.SaveChangesAsync(); // (Lưu tạm để lấy ID của Đơn thuốc)

                    // 3.1. KIỂM TRA TỒN KHO TRƯỚC (QUAN TRỌNG)
                    foreach (var item in dto.Plan.Prescriptions)
                    {
                        var medicine = await _medicineService.GetMedicineStockAsync(item.MedicineId); // (Gọi qua Service Kho kiểm tra)
                        if (medicine == null) throw new KeyNotFoundException($"Không tìm thấy thuốc với ID {item.MedicineId}");
                        if (medicine.StockQuantity < item.Quantity) // (Nếu số lượng trong kho ít hơn số bác sĩ kê)
                            throw new InvalidOperationException($"Thuốc '{medicine.Name}' không đủ tồn kho."); // (Chặn ngay, văng lỗi)
                    }

                    // 3.2. TIẾN HÀNH XUẤT KHO VÀ GHI CHI TIẾT ĐƠN THUỐC
                    foreach (var item in dto.Plan.Prescriptions)
                    {
                        // (Giao việc Xuất kho cho MedicineService. Nó sẽ tự động áp dụng luật FEFO - Lô nào cận date thì xuất trước)
                        await _medicineService.ExportMedicineAsync(new ExportMedicineDto
                        {
                            MedicineId = item.MedicineId,
                            Quantity = item.Quantity, // (Trừ đúng số lượng bác sĩ kê)
                            ReferenceCode = $"MR-{medicalRecord.Id}", // (Lưu vết: Thuốc này xuất ra để chữa cho bệnh án nào)
                            Notes = $"Kê đơn SOAP #{medicalRecord.Id}"
                        }, doctorId);

                        // (Ghi lại từng viên thuốc vào Đơn thuốc)
                        await _unitOfWork.PrescriptionItems.AddAsync(new PrescriptionItem
                        {
                            PrescriptionId = prescription.Id,
                            MedicineId = item.MedicineId,
                            Dosage = item.Dosage, // (Liều lượng)
                            Frequency = item.Frequency, // (Tần suất)
                            DurationDays = item.DurationDays, // (Số ngày uống)
                            Quantity = item.Quantity,
                            Instruction = item.Instruction // (Cách dùng)
                        });
                    }
                }

                // 4. LUÂN CHUYỂN TRẠNG THÁI (WORKFLOW)
                appointment.Status = "ready_to_pay"; // (Đổi cờ lịch hẹn: Khám xong -> Chuyển sang chờ thu ngân tính tiền)
                _unitOfWork.Appointments.Update(appointment);

                // 5. CHỐT GIAO DỊCH
                await _unitOfWork.SaveChangesAsync(); // (Lưu tất cả thay đổi)
                await _unitOfWork.CommitTransactionAsync(); // (Xác nhận Giao dịch thành công, mở khóa Database)

                return medicalRecord.Id; // (Trả ID Bệnh án về)
            }
            catch (Exception)
            {
                // (NẾU CÓ BẤT KỲ LỖI GÌ XẢY RA (Ví dụ sập mạng giữa chừng, hết thuốc giữa chừng)...)
                await _unitOfWork.RollbackTransactionAsync(); // (...HỦY BỎ TẤT CẢ mọi thay đổi. Không lưu bệnh án, không trừ kho)
                throw;
            }
        }
```

**Giải thích chi tiết (Kiến trúc Micro-Services giả lập):**
- **Sức mạnh của JSON:** Tại sao lại lưu JSON thay vì tạo thêm 20 cái Cột trong Database? Vì cấu trúc SOAP (khám lâm sàng Mắt, Mũi, Tim mạch, Hô hấp...) rất nhiều trường nhỏ nhặt. Lưu JSON giúp Database gọn gàng, và Frontend (Vue) có thể bóc trực tiếp JSON đó ra để binding thẳng vào các Form UI mà không cần qua bước trung gian rườm rà.
- **Tính toàn vẹn (ACID):** `BeginTransactionAsync(IsolationLevel.RepeatableRead)` là tuyệt chiêu của dự án. Giả sử 2 bác sĩ cùng kê đơn 1 loại thuốc cuối cùng trong kho. Cả 2 cùng ấn "Lưu". Transaction sẽ khóa dòng dữ liệu đó lại. Bác sĩ nào ấn nhanh hơn vài mili-giây sẽ giành được thuốc. Bác sĩ còn lại sẽ nhận thông báo lỗi "Không đủ tồn kho" ngay lập tức.
- **Tương tác chéo Service (Cross-Service Call):** `MedicalRecordService` không tự tiện chọc thẳng vào bảng `Inventory`. Nó phải gọi lịch sự qua hàm `_medicineService.ExportMedicineAsync`. Điều này tuân thủ nguyên tắc thiết kế mã Sạch (Clean Architecture - Loose Coupling).

---

### PHẦN 2.3 - TẦNG SERVICE (BIÊN DỊCH JSON SANG TIẾNG VIỆT CHO KHÁCH HÀNG)

**Tệp:** `MyPetClinic.Application/Services/MedicalRecordService.cs`

Như đã thấy ở trên, Bác sĩ lưu dữ liệu dưới dạng JSON (vd: `{"heartRate": 120, "eyes": {"isNormal": false, "note": "Đỏ mạc"}}`). Nhưng Khách hàng thì không thể đọc hiểu chuỗi JSON đó.
Vì vậy, khi khách hàng xem Bệnh án (trong mục Hồ sơ Y tế Khách hàng), ta phải gọi một hàm "Dịch thuật" (Parser) siêu đỉnh này:

```csharp
        private string ExtractReadableSoap(string? jsonStr, string fieldType)
        {
            if (string.IsNullOrWhiteSpace(jsonStr)) return string.Empty;
            if (!jsonStr.TrimStart().StartsWith("{") && !jsonStr.TrimStart().StartsWith("[")) return jsonStr; // (Nếu là text thường thì trả lại luôn)

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            try
            {
                // NẾU LÀ PHẦN S (SUBJECTIVE - HỎI BỆNH)
                if (fieldType == "S")
                {
                    var obj = JsonSerializer.Deserialize<SubjectiveDto>(jsonStr, options); // (Ép chuỗi JSON về dạng Cấu trúc C#)
                    if (obj == null) return jsonStr;
                    
                    var parts = new List<string>(); // (Tạo một cái mảng để chứa các câu tiếng Việt)
                    
                    if (!string.IsNullOrEmpty(obj.ChiefComplaint)) parts.Add($"Lý do khám: {obj.ChiefComplaint}");
                    
                    // (Gom nhóm các triệu chứng nôn ói, ho, hắt hơi thành câu văn)
                    var symptoms = new List<string>();
                    if (obj.HasVomiting) symptoms.Add("Nôn ói" + (!string.IsNullOrEmpty(obj.VomitingDetails) ? $" ({obj.VomitingDetails})" : ""));
                    if (obj.HasDiarrhea) symptoms.Add("Tiêu chảy" + (!string.IsNullOrEmpty(obj.DiarrheaDetails) ? $" ({obj.DiarrheaDetails})" : ""));
                    if (obj.HasCoughing) symptoms.Add("Ho");
                    if (symptoms.Any()) parts.Add("Triệu chứng: " + string.Join(", ", symptoms));

                    // (Ghép tất cả các câu lại, phân cách bằng dấu gạch đứng " | ")
                    return parts.Any() ? string.Join(" | ", parts) : "Khám tổng quát";
                }
                
                // NẾU LÀ PHẦN O (OBJECTIVE - LÂM SÀNG)
                else if (fieldType == "O")
                {
                    var obj = JsonSerializer.Deserialize<ObjectiveDto>(jsonStr, options);
                    if (obj == null) return jsonStr;
                    var parts = new List<string>();
                    
                    if (obj.HeartRate.HasValue) parts.Add($"Nhịp tim: {obj.HeartRate} bpm");
                    if (obj.RespiratoryRate.HasValue) parts.Add($"Nhịp thở: {obj.RespiratoryRate} lần/phút");

                    // (Đọc vào từng bộ phận cơ thể. Nếu biến IsNormal = false (bất thường) thì dịch note ra)
                    var abnormal = new List<string>();
                    if (obj.Eyes != null && !obj.Eyes.IsNormal) abnormal.Add("Mắt" + (!string.IsNullOrEmpty(obj.Eyes.Note) ? $": {obj.Eyes.Note}" : ""));
                    if (obj.SkinCoat != null && !obj.SkinCoat.IsNormal) abnormal.Add("Da lông" + (!string.IsNullOrEmpty(obj.SkinCoat.Note) ? $": {obj.SkinCoat.Note}" : ""));
                    
                    if (abnormal.Any()) parts.Add("Bất thường: " + string.Join(", ", abnormal));

                    return parts.Any() ? string.Join(", ", parts) : string.Empty;
                }
                // ... (Tương tự cho phần A và P)
            }
            catch
            {
                return jsonStr; // (Nếu quá trình giải mã JSON gặp lỗi, trả lại chuỗi gốc để tránh sập web)
            }
            return jsonStr;
        }
```

**Giải thích chi tiết:**
- Đoạn code này thể hiện sự **Tận tâm về UX (Trải nghiệm người dùng)**. Khách hàng không cần biết bác sĩ dùng phần mềm phức tạp ra sao. Thứ họ thấy trên màn hình điện thoại chỉ là một dòng text rất con người: *"Nhịp tim: 120 bpm, Bất thường: Mắt: Đỏ mạc, Da lông: Rụng nhiều"*.
- `JsonSerializer.Deserialize`: Cỗ máy dịch từ chuỗi mã nguồn thành Object C#. Sau đó dùng các vòng IF để bóc từng nhánh nhỏ ra, nối chữ (`string.Join`) rất khéo léo.

---
*(Hết tài liệu đào tạo chuyên sâu Quản lý Bệnh án Bác sĩ - Logic JSON & Transaction)*
