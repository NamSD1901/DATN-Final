# 🌐 Infrastructure & Security - Automatic Notification Service

## 🔗 Skills Liên Quan
- **BE-A03 (RBAC):** `[Authorize(Roles = "customer")]` đảm bảo khách hàng chỉ đọc được thông báo của chính mình, chặn đứng lỗ hổng bảo mật IDOR đọc trộm thông báo của người khác.
- **BE-C01 (SMTP Config):** Lưu giữ an toàn mật khẩu/host SMTP gửi mail trong cấu hình bí mật.

---

## 1. Bảo mật Endpoint Thông báo (IDOR Prevention)

Hệ thống bắt buộc phải kiểm tra quyền sở hữu thông báo trước khi cho phép người dùng đánh dấu đã đọc hoặc xem chi tiết:

```csharp
[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly MyPetClinicDbContext _context;

    public NotificationsController(MyPetClinicDbContext context)
    {
        _context = context;
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        // Tìm customer tương ứng
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer == null)
            return Forbid();

        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        // Chặn IDOR: Kiểm tra thông báo có thuộc về customer hiện tại không
        if (notification.CustomerId != customer.Id)
        {
            return Forbid("Bạn không có quyền chỉnh sửa thông báo này.");
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok();
    }
}
```
---

## 2. Thiết lập Hangfire Recurring Job
Đăng ký trigger định kỳ chạy lúc 08:00 mỗi ngày ở tệp `Program.cs` hoặc `Startup.cs`:
```csharp
RecurringJob.AddOrUpdate<VaccinationReminderJob>(
    "vaccination-daily-reminder",
    job => job.SendDailyVaccinationRemindersAsync(),
    Cron.Daily(8, 0) // Lớp chạy ngầm chạy vào 8 giờ sáng hàng ngày
);
```
