# 🌐 Infrastructure & Security - Clinical Diagnosis & Treatment

## 🔗 Skills Liên Quan
- **BE-A03 (RBAC):** Gán tag `[Authorize(Roles = "doctor,admin")]` bảo vệ tài nguyên bệnh án.
- **BE-C02 (EF Core):** Sử dụng giao dịch cơ sở dữ liệu `DbTransaction` để quản lý đồng thời.

---

## 1. Phân Quyền Bảo Mật (RBAC Security)

Bệnh án và đơn thuốc là thông tin y tế nhạy cảm. Chỉ bác sĩ chịu trách nhiệm điều trị hoặc quản trị viên cấp cao mới có quyền ghi nhận/chỉnh sửa thông tin này:

```csharp
[Authorize(Roles = "doctor,admin")]
[ApiController]
[Route("api/doctor/medical-records")]
public class DoctorMedicalRecordsController : ControllerBase
{
    private readonly MedicalRecordService _service;

    public DoctorMedicalRecordsController(MedicalRecordService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(MedicalRecordCreationDto dto)
    {
        var doctorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(doctorIdClaim, out var doctorId))
        {
            return Unauthorized();
        }

        var result = await _service.CreateRecordAndDeductInventoryAsync(dto, doctorId);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return StatusCode(201);
    }
}
```
---

## 2. Lock Row khi trừ tồn kho (Concurrency Control)
Để ngăn ngừa tình trạng hai bác sĩ cùng kê đơn một loại thuốc khan hiếm tại cùng một thời điểm dẫn đến số lượng tồn kho bị âm, hệ thống áp dụng cơ chế khóa dòng (Pessimistic Concurrency hoặc Row Locking) khi đọc dữ liệu thuốc lên cập nhật:
```sql
SELECT "Id", "StockQuantity" FROM "Medicines" WHERE "Id" = @id FOR UPDATE;
```
*(Trong EF Core, sử dụng Transaction kết hợp với Raw SQL hoặc cấu hình Isolation Level dạng RepeatableRead/Serializable).*
