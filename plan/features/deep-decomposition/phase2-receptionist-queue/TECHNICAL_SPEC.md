# 🛠️ Technical Specification - Receptionist Portal & Queue Management

## 1. Kiến trúc Đồng bộ Hàng đợi Thời gian thực (Real-time SignalR Hub)

Để đảm bảo các màn hình tivi công cộng tại sảnh chờ (Public Queue Board) và màn hình điều hành của Lễ tân, Bác sĩ được cập nhật đồng bộ tức thời dưới 500ms mà không cần tải lại trang, hệ thống sử dụng kiến trúc WebSockets thông qua **ASP.NET Core SignalR**:

```mermaid
flowchart TD
    subgraph Client App
        A[Receptionist Dashboard]
        B[Doctor Room View]
        C[Public TV Screen]
    end
    subgraph Web API Server
        D[QueueHub : Hub]
        E[QueueService : Application Layer]
    end
    subgraph Database Layer
        F[(PostgreSQL DB)]
    end

    A -- "1. Send Check-in (HTTP POST)" --> E
    E -- "2. Generate Queue & Save" --> F
    E -- "3. Trigger Real-time Event" --> D
    D -- "4. Broadcast 'QueueUpdated' Event" --> A
    D -- "4. Broadcast 'QueueUpdated' Event" --> B
    D -- "4. Broadcast 'QueueUpdated' Event" --> C
```

---

## 2. Luồng xử lý Kỹ thuật chi tiết (Sequence Diagrams)

### A. Luồng Quét QR Check-in & Cấp Số Thứ Tự Tự Động
Quy trình lễ tân quét mã QR tiếp đón nhanh và phân phòng khám:

```mermaid
sequenceDiagram
    autonumber
    actor Rec as Lễ tân (Scanner USB)
    participant FE as Vue 3 Client
    participant Hub as SignalR QueueHub
    participant API as ReceptionistQueueController
    participant Service as QueueService
    participant DB as PostgreSQL Database

    Rec->>FE: Quét mã QR của Khách hàng (Autofocus Input)
    FE->>API: POST /api/receptionist/check-in-qr (QrToken)
    Note over API: Giải mã QrToken lấy AppointmentId
    
    API->>Service: CheckInAppointmentAsync(appointmentId)
    Service->>DB: SELECT * FROM Appointments WHERE Id = @AppointmentId FOR UPDATE
    DB-->>Service: Appointment Entity (Status = 'confirmed')
    
    Note over Service: 1. Đổi Status = 'waiting'<br/>2. Điền CheckInTime = UTC_NOW<br/>3. Gọi SQL Sequence sinh số thứ tự (e.g. Q-014)<br/>4. Phân bổ vào phòng khám trống nhất
    
    Service->>DB: UPDATE Appointments SET Status='waiting', QueueNumber='Q-014', CheckInTime=NOW()
    DB-->>Service: Row Affected
    
    Service->>Hub: NotifyQueueChanged()
    Hub-->>FE: Broadcast Event 'QueueUpdated'
    Service-->>API: CheckInSuccessResult (QueueNumber, PetName)
    API-->>FE: 200 OK (CheckInSuccessResult)
    FE->>Rec: In phiếu khám nhiệt tự động & hiện Toast xanh
```

---

## 3. Đặc tả Mô hình Cơ sở dữ liệu Mở rộng (Database Schemas)

Để quản lý số thứ tự và phân bổ phòng khám, bảng `Appointments` được mở rộng thêm các cột quản trị hàng đợi:

```mermaid
erDiagram
    Appointments {
        bigint Id PK
        varchar Status "waiting, in_progress, completed..."
        varchar QueueNumber "Q-001 đến Q-999"
        timestamp CheckInTime "nullable"
        varchar ClinicRoom "Room 101, Room 102..."
        timestamp StartExamTime "nullable"
        timestamp EndExamTime "nullable"
    }
```

### Script tạo Postgres Sequence tự động reset mỗi ngày:
Để đảm bảo số thứ tự khám tự động quay về `1` vào lúc 00:00 đêm:
```sql
-- Tạo Sequence cho số thứ tự khám
CREATE SEQUENCE IF NOT EXISTS queue_number_seq
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 999
    CYCLE;

-- Tạo Function tự động reset Sequence hàng ngày thông qua pg_cron hoặc Scheduler
CREATE OR REPLACE FUNCTION reset_queue_sequence()
RETURNS void AS $$
BEGIN
    ALTER SEQUENCE queue_number_seq RESTART WITH 1;
END;
$$ LANGUAGE plpgsql;
```

---

## 4. Đặc tả DTOs & Validation Rules

### A. QuickWalkInRequestDto (Đăng ký nhanh khách vãng lai)
```csharp
namespace MyPetClinic.Application.DTOs
{
    public class QuickWalkInRequestDto
    {
        // Thông tin Chủ nuôi
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        
        // Thông tin Thú cưng
        public string PetName { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty; // Dog, Cat
        public string Breed { get; set; } = string.Empty; // Giống
        
        // Chỉ định Khám
        public long ServiceId { get; set; }
        public Guid? AssignedDoctorId { get; set; }
        public string ClinicRoom { get; set; } = string.Empty;
        public string Symptom { get; set; } = string.Empty;
    }
}
```

### B. FluentValidation C# Rules
```csharp
using FluentValidation;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Validators
{
    public class QuickWalkInRequestDtoValidator : AbstractValidator<QuickWalkInRequestDto>
    {
        public QuickWalkInRequestDtoValidator()
        {
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Tên chủ nuôi không được bỏ trống.")
                .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.CustomerPhone)
                .NotEmpty().WithMessage("Số điện thoại không được bỏ trống.")
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Số điện thoại không đúng định dạng Việt Nam.");

            RuleFor(x => x.PetName)
                .NotEmpty().WithMessage("Tên thú cưng không được bỏ trống.");

            RuleFor(x => x.Species)
                .NotEmpty().WithMessage("Vui lòng chọn loài thú cưng (Dog/Cat).")
                .Must(s => s == "Dog" || s == "Cat").WithMessage("Loài thú cưng phải là Dog hoặc Cat.");

            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage("Vui lòng chọn dịch vụ chỉ định khám.");
        }
    }
}
```
