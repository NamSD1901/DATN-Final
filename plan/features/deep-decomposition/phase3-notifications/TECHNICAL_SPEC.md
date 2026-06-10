# 🛠️ Technical Specification - Automatic Notification Service

## 🔗 Skills Liên Quan
- **BE-F02 (SOLID - SRP):** Tách biệt logic gửi mail (`EmailSender`) ra khỏi logic quét hàng chờ nhắc lịch (`VaccinationReminderJob`).
- **BE-F03 (Async/Await):** Thực thi các cuộc gọi SMTP gửi email bất đồng bộ bằng `SendEmailAsync`.

---

## 1. Sequence Diagram: Luồng gửi thông báo tự động (Background Job Workflow)

```mermaid
sequenceDiagram
    participant OS as Cron Trigger (08:00 AM)
    participant Worker as Hangfire/Background Worker
    participant DB as PostgreSQL Database
    participant SMTP as SMTP Mail Server (Gmail/SendGrid)
    participant FE as Vue Frontend (SignalR)

    OS->>Worker: Kích hoạt Job hàng ngày
    Worker->>DB: Query các VaccinationRecord có NextDoseDate = Today + 3 ngày (hoặc 5 ngày)
    DB-->>Worker: Trả về danh sách thú cưng & email chủ nuôi
    
    loop Với từng bản ghi tiêm chủng
        Worker->>DB: Insert thông báo hệ thống (In-app Notification)
        Worker->>SMTP: Gửi email nhắc lịch tiêm phòng
        SMTP-->>Worker: Email gửi thành công
        Worker->>FE: Phát tín hiệu real-time qua SignalR báo có thông báo mới
    end
    
    Worker-->>OS: Hoàn thành Job quét nhắc lịch ngày hôm nay
```

---

## 2. API Schema & DTOs

```csharp
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MarkAsReadRequest
{
    public Guid NotificationId { get; set; }
}
```
