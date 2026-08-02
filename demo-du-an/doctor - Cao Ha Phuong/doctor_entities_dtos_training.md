# TÀI LIỆU ĐÀO TẠO NỘI BỘ: PHÂN TÍCH CHUYÊN SÂU DTO & ENTITY (DOCTOR)

> [!NOTE]
> Tài liệu này sẽ "mổ xẻ" các lớp dữ liệu (Entities) tương tác trực tiếp với Database, và các lớp truyền tải dữ liệu (DTO) tương tác với Frontend trong module Khám chữa bệnh & Tiêm phòng của Bác sĩ.

---

## 1. THỰC THỂ (ENTITY) - KẾT CẤU DATABASE

Entity là trái tim của hệ thống, định nghĩa các cột sẽ được sinh ra dưới Database thông qua Entity Framework Core.

### 1.1 `MedicalRecord.cs` (Bệnh án Lâm sàng)
```c#
    public class MedicalRecord
    {
        public long Id { get; set; }
        
        // CÁC KHÓA NGOẠI (FOREIGN KEYS) - Ràng buộc dữ liệu
        public long AppointmentId { get; set; } // Liên kết với Cuộc hẹn
        public Guid DoctorId { get; set; }      // Bác sĩ trực tiếp khám (ngăn IDOR)
        public long PetId { get; set; }         // Con vật được khám
        
        public string RecordType { get; set; } = "Consultation"; 

        // [S] - SUBJECTIVE: Lịch sử y tế (JSON String)
        public string? MedicalHistory { get; set; } 

        // [O] - OBJECTIVE: Khám thực thể
        public decimal Weight { get; set; }       // Bắt buộc phải có để tính liều lượng thuốc
        public decimal Temperature { get; set; } 
        // ClinicalSigns lưu các triệu chứng hoặc Kết quả sàng lọc dưới dạng JSON
        public string ClinicalSigns { get; set; } = string.Empty; 

        // [A] - ASSESSMENT: Đánh giá & Chẩn đoán
        public string Diagnosis { get; set; } = string.Empty; // Chuỗi JSON chứa chẩn đoán
        
        // [P] - PLAN: Kế hoạch điều trị
        public string TreatmentPlan { get; set; } = string.Empty; // JSON
        public string? DoctorNotes { get; set; }                  // Lời dặn dò hiển thị cho khách
        public DateTime? FollowUpDate { get; set; }               // Ngày hẹn tái khám
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // VIRTUAL NAVIGATION PROPERTIES (Dùng để .Include() trong LINQ)
        public Appointment? Appointment { get; set; }
        public User? Doctor { get; set; }
        public Pet? Pet { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
```
**Phân tích kỹ thuật:** Thay vì tách các phần S, O, A, P thành hàng chục cột riêng lẻ làm phình to Database, thiết kế này ưu tiên **Lưu trữ JSON** vào `MedicalHistory`, `ClinicalSigns`, `Diagnosis`. Việc này giúp hệ thống linh hoạt, dễ dàng thêm bớt trường trên giao diện (ví dụ: thêm mục đo đường huyết) mà không cần phải gõ lệnh Migration sửa Database.

### 1.2 `VaccinationRecord.cs` (Sổ Tiêm Phòng)
```c#
    public class VaccinationRecord
    {
        // ... (Khóa ngoại tương tự MedicalRecord)
        public long? VaccineId { get; set; }      // Liên kết tới loại Vắc-xin (vd: Dại, 7 bệnh)
        public long? VaccineBatchId { get; set; } // Liên kết chặt chẽ tới Từng Lô hàng cụ thể

        // ... Các trường S (Lý do tiêm, Lịch sử dị ứng) 
        // ... Các trường O (Cân nặng, Nhịp tim, BCS)

        // ĐẶC THÙ TIÊM PHÒNG (Vaccine Plan Specifics)
        public decimal? Dose { get; set; }          // Liều lượng (vd: 1ml)
        public string? Route { get; set; }          // Đường tiêm (vd: Tiêm dưới da - SC)
        public string? InjectionSite { get; set; }  // Vị trí tiêm (vd: Dưới da cổ)

        // [A] - KẾT LUẬN LÂM SÀNG
        public string? ClinicalAssessment { get; set; } // Sẽ là "Đủ điều kiện" hoặc "Hoãn tiêm"
        public string? DoctorRemarks { get; set; }      // Nếu hoãn tiêm, phải ghi rõ lý do ở đây

        public DateTime InjectionDate { get; set; }     // Ngày thực tiêm
        public DateTime? NextDueDate { get; set; }      // Lịch hẹn tiêm mũi tiếp theo
        // ...
    }
```
**Phân tích kỹ thuật:** Khác với Khám lâm sàng lưu JSON, Tiêm phòng được thiết kế **Column-Based (Lưu từng cột)**. Vì sao? Vì Form tiêm phòng có cấu trúc rất cứng, hiếm khi thay đổi. Hơn nữa, việc tách riêng cột `VaccineBatchId` là cực kỳ quan trọng để hệ thống truy vết lô hàng tồn kho, và tách `NextDueDate` để hệ thống tự động chạy cronjob gửi SMS nhắc khách hàng đi tiêm mũi tiếp theo.

---

## 2. DTO (DATA TRANSFER OBJECTS) - KIỂM SOÁT ĐẦU VÀO

DTO là lớp áo giáp bảo vệ Database khỏi các luồng dữ liệu bẩn từ phía Client (Frontend, Postman). Chúng định nghĩa cấu trúc JSON mà Client phải gửi lên.

### 2.1 `MedicalRecordSoapRequestDto.cs`
Đây là một **Nested DTO** (DTO lồng nhau) cực kỳ đẹp mắt, phản ánh đúng cấu trúc S-O-A-P:

```c#
    public class MedicalRecordSoapRequestDto
    {
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        
        // Chia làm 4 khối rõ rệt: Subjective, Objective, Assessment, Plan
        public SubjectiveDto Subjective { get; set; } = new();
        public ObjectiveDto Objective { get; set; } = new();
        public AssessmentDto Assessment { get; set; } = new();
        public PlanDto Plan { get; set; } = new();
    }
    
    // Ví dụ về Objective (Khám khách quan)
    public class ObjectiveDto
    {
        public decimal Weight { get; set; }
        public decimal? Temperature { get; set; }
        // ...
        
        // Khám chuyên sâu từng hệ cơ quan
        public SystemExamDto Eyes { get; set; } = new();
        public SystemExamDto Ears { get; set; } = new();
        // ...
    }
    
    public class SystemExamDto
    {
        public bool IsNormal { get; set; } = true; // Mặc định là Bình thường
        public string? Note { get; set; }          // Nếu IsNormal = false, bắt buộc phải có Note
    }
```
**Phân tích kỹ thuật:** Cấu trúc phân cấp này giúp Frontend Bind dữ liệu (Vue `v-model`) cực kỳ dễ dàng. `SystemExamDto` là một ví dụ điển hình của thiết kế tái sử dụng (Reusability). Toàn bộ các hệ cơ quan (Mắt, mũi, tai, tiêu hóa) đều xài chung 1 cấu trúc: Có bình thường không? Nếu không thì ghi chú gì?

### 2.2 `VaccinationSoapRequestDto.cs` (Validation Siêu chặt)
Điểm sáng giá nhất của DTO này là việc kế thừa interface `IValidatableObject` để thực hiện **Cross-field Validation (Kiểm tra chéo nhiều trường cùng lúc)**.

```c#
    public class VaccinationSoapRequestDto : IValidatableObject
    {
        // 1. DATA ANNOTATIONS (Kiểm tra cơ bản trên từng trường)
        [Required(ErrorMessage = "Vui lòng chọn lý do tiêm")]
        public string ReasonForVisit { get; set; } = string.Empty;

        [Range(0.1, 150.0, ErrorMessage = "Cân nặng phải > 0 và <= 150 kg")] // Bắt buộc phải thực tế
        public decimal Weight { get; set; }
        
        [Range(30.0, 45.0, ErrorMessage = "Nhiệt độ không hợp lệ (30 - 45°C)")]
        public decimal? Temperature { get; set; }
        
        // ...

        // 2. IValidatableObject (Kiểm tra logic chéo nghiệp vụ)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // VR03: Nếu đánh giá là ĐỦ ĐIỀU KIỆN thì BẮT BUỘC phải chọn Vắc-xin
            if (ClinicalAssessment == "Đủ điều kiện" && (!VaccineId.HasValue || VaccineId.Value <= 0))
            {
                yield return new ValidationResult("Vui lòng chọn vaccine sử dụng", new[] { nameof(VaccineId) });
            }

            // VR04: Nếu có hẹn lịch tiêm mũi sau, ngày đó KHÔNG ĐƯỢC LÀ QUÁ KHỨ
            if (NextDueDate.HasValue && NextDueDate.Value.Date <= DateTime.UtcNow.Date)
            {
                yield return new ValidationResult("Ngày nhắc lại không hợp lệ", new[] { nameof(NextDueDate) });
            }

            // VR06: Nếu đánh giá là HOÃN TIÊM, BẮT BUỘC bác sĩ phải giải trình dài hơn 10 ký tự
            if (ClinicalAssessment == "Hoãn tiêm" && (string.IsNullOrWhiteSpace(DoctorRemarks) || DoctorRemarks.Length < 10))
            {
                yield return new ValidationResult("Ghi rõ lý do hoãn tiêm (ít nhất 10 ký tự)", new[] { nameof(DoctorRemarks) });
            }
            
            // ... Kiểm tra dị ứng
        }
    }
```
**Phân tích kỹ thuật:**
- **Data Annotations (`[Required]`, `[Range]`):** Dùng để bắt lỗi tức thì (Ví dụ bác sĩ gõ nhầm cân nặng con mèo thành 200kg).
- **`Validate()` Method:** Xử lý nghiệp vụ phức tạp. Việc Validate logic này trực tiếp trong DTO (tại tầng Controller trước khi chui vào Service) giúp API tiết kiệm chi phí gọi Database (Fail-fast). Nếu bác sĩ chọn "Hoãn tiêm" mà không ghi giải trình, API sẽ trả về `400 Bad Request` ngay lập tức mà không cần đi sâu vào hàm `SubmitSoapRecordAsync`.
