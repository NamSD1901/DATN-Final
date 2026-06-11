# 01. Core Business Logic Reference - Notifications

Tài liệu đặc tả thuật toán chạy ngầm Quartz.NET quét lịch tiêm chủng và mã nguồn C# thực thi SignalR Hub đẩy tin thời gian thực.

---

## 1. Quartz.NET Job Quét Lịch Tiêm chủng Tự động (VaccinationReminderJob)

Tiến trình Quartz.NET được đăng ký chạy nền trong ứng dụng Web API để thực thi quét CSDL mỗi ngày.
Nhiệm vụ: Tìm các bản ghi tiêm phòng có ngày tái chủng dự kiến (`NextDoseDate`) cách ngày hiện tại chính xác 3 ngày hoặc 5 ngày.

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Services;
using MyPetClinic.Infrastructure.Data;
using Quartz;

namespace MyPetClinic.Application.Jobs
{
    [DisallowConcurrentExecution] // Chặn chạy song song nhiều Job trùng nhau nếu DB phản hồi chậm
    public class VaccinationReminderJob : IJob
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<VaccinationReminderJob> _logger;

        public VaccinationReminderJob(AppDbContext context, IEmailService emailService, ILogger<VaccinationReminderJob> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Bắt đầu tiến trình quét lịch tiêm chủng tự động...");

            var today = DateTime.UtcNow.Date;
            var targetDate3 = today.AddDays(3);
            var targetDate5 = today.AddDays(5);

            try
            {
                // Lấy danh sách hồ sơ tiêm chủng cần nhắc lịch
                var reminders = await _context.VaccinationRecords
                    .Include(vr => vr.Pet)
                        .ThenInclude(p => p.Owner)
                    .Where(vr => vr.NextDoseDate.HasValue && 
                                (vr.NextDoseDate.Value.Date == targetDate3 || vr.NextDoseDate.Value.Date == targetDate5))
                    .ToListAsync();

                _logger.LogInformation($"Tìm thấy {reminders.Count} bé cưng cần tái chủng.");

                foreach (var record in reminders)
                {
                    if (record.Pet?.Owner == null) continue;

                    string ownerEmail = record.Pet.Owner.Email;
                    string ownerName = record.Pet.Owner.FullName;
                    string petName = record.Pet.Name;
                    string vaccineName = record.VaccineName;
                    string nextDateStr = record.NextDoseDate!.Value.ToString("dd/MM/yyyy");
                    int daysLeft = (record.NextDoseDate.Value.Date - today).Days;

                    // Biên soạn nội dung HTML Email
                    string emailBody = $@"
                        <h3>Chào {ownerName},</h3>
                        <p>Đây là thông báo nhắc nhở từ Phòng khám thú y <b>MyPetClinic</b>.</p>
                        <p>Bé cưng <b>{petName}</b> của bạn có lịch tiêm nhắc lại vắc-xin <b>{vaccineName}</b> vào ngày <b>{nextDateStr}</b> (còn {daysLeft} ngày nữa).</p>
                        <p>Việc tiêm phòng đúng hẹn là vô cùng quan trọng để duy trì kháng thể bảo vệ bé khỏi các căn bệnh nguy hiểm.</p>
                        <p><a href='https://mypetclinic.vn/booking?petId={record.PetId}&serviceType=vaccination' 
                              style='padding: 10px 20px; background-color: #10b981; color: white; text-decoration: none; border-radius: 6px;'>
                              Đặt lịch hẹn tiêm phòng nhanh tại đây
                           </a></p>
                        <br/>
                        <p>Thân mến,<br/>Đội ngũ MyPetClinic</p>";

                    await _emailService.SendEmailAsync(
                        ownerEmail,
                        $"[MyPetClinic] Nhắc lịch tiêm phòng vắc-xin cho bé {petName}",
                        emailBody);

                    // Đồng thời lưu một thông báo in-app hệ thống
                    var inAppNotification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = record.Pet.OwnerId,
                        Title = $"Nhắc lịch tiêm vắc-xin cho bé {petName}",
                        Content = $"Bé cưng {petName} có lịch tiêm nhắc vắc-xin {vaccineName} vào ngày {nextDateStr}. Vui lòng đặt lịch hẹn.",
                        IsRead = false,
                        Type = "VaccineReminder",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(inAppNotification);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Hoàn tất gửi email nhắc tiêm chủng.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong tiến trình chạy ngầm VaccinationReminderJob");
                throw new JobExecutionException(ex) { RefireImmediately = true }; // Yêu cầu Quartz chạy lại nếu sập
            }
        }
    }
}
```

---

## 2. SignalR Hub Đẩy thông báo Thời gian thực (NotificationHub)

SignalR Hub đóng vai trò làm máy chủ trung gian quản lý các kết nối WebSockets của người dùng và đẩy tin nhắn thời gian thực.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MyPetClinic.Infrastructure.Realtime
{
    [Authorize] // Yêu cầu JWT token xác thực trước khi kết nối socket
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier ?? "Anonymous";
            _logger.LogInformation($"Người dùng {userId} đã kết nối WebSocket SignalR.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userId = Context.UserIdentifier ?? "Anonymous";
            _logger.LogWarning($"Người dùng {userId} đã ngắt kết nối WebSocket.");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
```

### Cách gọi đẩy thông báo đẩy thời gian thực từ Business Service:
Khi Lễ tân duyệt lịch hẹn khám bệnh:
```csharp
public async Task ApproveAppointmentAsync(Guid appointmentId)
{
    var app = await _context.Appointments.FindAsync(appointmentId);
    app.Status = "Confirmed";

    // 1. Tạo thông báo lưu DB
    var notification = new Notification
    {
        Id = Guid.NewGuid(),
        UserId = app.Pet.OwnerId,
        Title = "Lịch hẹn được phê duyệt",
        Content = $"Lịch hẹn khám cho bé {app.Pet.Name} đã được xác nhận vào {app.AppointmentDate:dd/MM/yyyy}.",
        IsRead = false,
        Type = "AppointmentUpdate",
        CreatedAt = DateTime.UtcNow
    };
    _context.Notifications.Add(notification);
    await _context.SaveChangesAsync();

    // 2. Gọi SignalR Hub để đẩy thông báo thời gian thực đến Client cụ thể (chặn đứng IDOR lộ tin)
    await _hubContext.Clients.User(app.Pet.OwnerId.ToString())
        .SendAsync("ReceiveNotification", new
        {
            id = notification.Id,
            title = notification.Title,
            content = notification.Content,
            createdAt = notification.CreatedAt
        });
}
```
*Phương thức `Clients.User(userId)` của SignalR bảo đảm tin nhắn chỉ bay đến đúng trình duyệt đang đăng nhập của chủ nuôi đó.*
