# 🛠️ Technical Specification - Customer Appointment Management

## 1. Luồng xử lý Kỹ thuật (Sequence Diagrams)

### A. Luồng Xem danh sách & Lọc trạng thái Lịch hẹn
Dưới đây là quy trình tải dữ liệu tối ưu hóa cho Dashboard:

```mermaid
sequenceDiagram
    autonumber
    actor Client as Vue 3 Client
    participant API as CustomerAppointmentController
    participant Auth as JwtMiddleware / Claims
    participant ApptService as AppointmentService
    participant Repo as UnitOfWork
    participant DB as PostgreSQL Database

    Client->>API: GET /api/my-appointments?status=confirmed&page=1&pageSize=10
    API->>Auth: Xác thực Token
    Auth-->>API: Trích xuất currentUserId
    
    API->>ApptService: GetAppointmentsForCustomerAsync(customerId, status, page, pageSize)
    ApptService->>Repo: GetPagedCustomerAppointmentsAsync(customerId, status, page, pageSize)
    Note over Repo: Thực hiện truy vấn Eager Loading<br/>tránh lỗi N+1 Query
    Repo->>DB: SELECT * FROM Appointments JOIN Pets JOIN Services JOIN Users WHERE CustomerId = @CustomerId AND Status = @Status ORDER BY AppointmentDate DESC LIMIT 10 OFFSET 0
    DB-->>Repo: List<AppointmentEntity>
    Repo-->>ApptService: List<Appointment>
    ApptService-->>API: PagedList<AppointmentDetailDto>
    API-->>Client: 200 OK (JSON List + Metadata Phân trang)
```

### B. Luồng Hủy lịch hẹn & Giải phóng ca trực Bác sĩ
Quy trình hủy lịch kiểm tra IDOR nghiêm ngặt và giải phóng ca làm việc:

```mermaid
sequenceDiagram
    autonumber
    actor Client as Vue 3 Client
    participant API as CustomerAppointmentController
    participant ApptService as AppointmentService
    participant Event as CancelAppointmentEventHandler
    participant Mail as EmailService
    participant DB as PostgreSQL Database

    Client->>API: PUT /api/my-appointments/{id}/cancel (CancelAppointmentRequest DTO)
    Note over API: Trích xuất currentUserId từ Token Claims
    API->>ApptService: CancelAppointmentAsync(appointmentId, currentUserId, request.Reason)
    
    ApptService->>DB: SELECT * FROM Appointments WHERE Id = @Id (FOR UPDATE Lock)
    DB-->>ApptService: Appointment Entity
    
    alt Lịch hẹn null hoặc Appointment.CustomerId != currentUserId
        ApptService-->>API: Quăng UnauthorizedAccessException / KeyNotFoundException
        API-->>Client: 403 Forbidden / 404 Not Found
    else Trạng thái hiện tại NOT IN ('pending', 'confirmed')
        ApptService-->>API: Quăng InvalidOperationException
        API-->>Client: 400 Bad Request ("Không thể hủy lịch ở trạng thái hiện tại")
    else Hợp lệ
        ApptService->>ApptService: Cập nhật Status = 'cancelled', CancelledReason = Reason, CancelledAt = UTC_NOW
        ApptService->>DB: UPDATE Appointments SET Status = 'cancelled'... WHERE Id = @Id
        DB-->>ApptService: Row updated
        ApptService->>Event: Publish CancelAppointmentEvent(appointmentId)
        Note over Event: Tự động chạy nền (Background Job)
        Event->>Mail: SendCancellationEmailAsync(customerEmail, appointmentDetails)
        Mail-->>Client: Gửi Email hoàn tất
        ApptService-->>API: Trả về kết quả true
        API-->>Client: 200 OK (success: true, message: "Hủy lịch thành công")
    end
```

---

## 2. Đặc tả Mô hình Cơ sở dữ liệu Mở rộng (Database Schema)

Để hỗ trợ ghi nhận lý do hủy và quản lý dòng thời gian chi tiết của lịch hẹn:

```mermaid
erDiagram
    Appointments {
        bigint Id PK
        bigint PetId FK
        bigint ServiceId FK
        bigint VaccineId FK
        uuid DoctorId FK
        timestamp AppointmentDate
        varchar Status "pending, confirmed, waiting, in_progress, completed, cancelled"
        varchar QrToken
        varchar Symptom
        varchar Note
        varchar CancelledReason "nullable"
        timestamp CancelledAt "nullable"
        timestamp CreatedAt
    }
```

### SQL DDL cập nhật bảng Appointments:
```sql
-- Thêm các trường hỗ trợ hủy lịch hẹn vào bảng Appointments
ALTER TABLE "Appointments" ADD COLUMN IF NOT EXISTS "CancelledReason" VARCHAR(500) NULL;
ALTER TABLE "Appointments" ADD COLUMN IF NOT EXISTS "CancelledAt" TIMESTAMP NULL;

-- Cập nhật ràng buộc Check Constraint cho cột Status
ALTER TABLE "Appointments" DROP CONSTRAINT IF EXISTS "chk_appointments_status";
ALTER TABLE "Appointments" ADD CONSTRAINT "chk_appointments_status" 
CHECK ("Status" IN ('pending', 'confirmed', 'waiting', 'in_progress', 'completed', 'cancelled'));
```

---

## 3. Đặc tả DTOs & Validation Rules

### A. CancelAppointmentRequest (DTO gửi yêu cầu hủy)
```csharp
namespace MyPetClinic.Application.DTOs
{
    public class CancelAppointmentRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
```

### B. FluentValidation C# Rules
```csharp
using FluentValidation;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Validators
{
    public class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
    {
        public CancelAppointmentRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Vui lòng nhập lý do hủy lịch hẹn.")
                .MinimumLength(10).WithMessage("Lý do hủy lịch phải chứa tối thiểu 10 ký tự.")
                .MaximumLength(500).WithMessage("Lý do hủy lịch không được vượt quá 500 ký tự.");
        }
    }
}
```
