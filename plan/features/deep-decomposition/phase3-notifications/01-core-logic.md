# 🧠 Core Business Logic - Automatic Notification Service

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Biên soạn nội dung email template động sử dụng StringBuilder hoặc Razor Engine.
- **BE-F03 (Async/Await):** Xử lý gửi song song các luồng email hàng loạt bằng `Task.WhenAll` tối ưu hóa thời gian xử lý của Worker.

---

## 1. C# Logic: Hangfire Background Job & Mailer

```csharp
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class VaccinationReminderJob
{
    private readonly MyPetClinicDbContext _context;
    private readonly IConfiguration _config;

    public VaccinationReminderJob(MyPetClinicDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task SendDailyVaccinationRemindersAsync()
    {
        var targetDate3Days = DateTime.UtcNow.Date.AddDays(3);
        var targetDate5Days = DateTime.UtcNow.Date.AddDays(5);

        // Lấy danh sách hồ sơ tiêm chủng cần nhắc lịch
        var recordsToRemind = await _context.VaccinationRecords
            .Include(vr => vr.Pet)
                .ThenInclude(p => p.Customer)
                    .ThenInclude(c => c.User)
            .Where(vr => vr.NextDoseDate.Date == targetDate3Days || vr.NextDoseDate.Date == targetDate5Days)
            .ToListAsync();

        var emailTasks = recordsToRemind.Select(async record =>
        {
            var customerEmail = record.Pet.Customer.User.Email;
            var customerName = record.Pet.Customer.FullName;
            var petName = record.Pet.Name;
            var daysRemaining = (record.NextDoseDate.Date - DateTime.UtcNow.Date).Days;

            // 1. Tạo thông báo hệ thống (In-app Notification)
            var notification = new Notification
            {
                CustomerId = record.Pet.Customer.Id,
                Title = "Nhắc lịch tiêm chủng vắc-xin",
                Message = $"Thú cưng {petName} của bạn có lịch hẹn tiêm phòng vắc-xin tiếp theo sau {daysRemaining} ngày nữa.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);

            // 2. Gửi Email thông báo
            await SendEmailAsync(customerEmail, customerName, petName, record.VaccineName, record.NextDoseDate, daysRemaining);
        });

        await Task.WhenAll(emailTasks);
        await _context.SaveChangesAsync();
    }

    private async Task SendEmailAsync(string email, string customerName, string petName, string vaccineName, DateTime nextDate, int daysLeft)
    {
        var smtpHost = _config["Smtp:Host"];
        var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");
        var smtpUser = _config["Smtp:Username"];
        var smtpPass = _config["Smtp:Password"];

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser, "Phòng Khám Thú Y MyPetClinic"),
            Subject = $"[MyPetClinic] Nhắc lịch tái chủng vaccine cho bé {petName}",
            IsBodyHtml = true
        };
        mailMessage.To.Add(email);

        var bodyBuilder = new StringBuilder();
        bodyBuilder.Append($"<h3>Xin chào {customerName},</h3>");
        bodyBuilder.Append($"<p>Đây là thông báo tự động từ hệ thống phòng khám <strong>MyPetClinic</strong>.</p>");
        bodyBuilder.Append($"<p>Thú cưng <strong>{petName}</strong> của bạn sắp đến hạn tiêm phòng mũi vắc-xin tiếp theo:</p>");
        bodyBuilder.Append($"<ul>");
        bodyBuilder.Append($"<li><strong>Loại vắc-xin:</strong> {vaccineName}</li>");
        bodyBuilder.Append($"<li><strong>Ngày hẹn tái chủng:</strong> {nextDate:dd/MM/yyyy}</li>");
        bodyBuilder.Append($"<li><strong>Thời gian còn lại:</strong> {daysLeft} ngày</li>");
        bodyBuilder.Append($"</ul>");
        bodyBuilder.Append($"<p>Để đảm bảo hiệu quả ngừa bệnh tốt nhất cho bé, vui lòng nhấp vào liên kết sau để đặt lịch tiêm phòng trực tuyến nhanh chóng:</p>");
        bodyBuilder.Append($"<p><a href='https://mypetclinic.vn/booking' style='display:inline-block;padding:10px 20px;color:white;background-color:#EAB308;text-decoration:none;border-radius:5px;'>Đặt Lịch Tiêm Phòng Ngay</a></p>");
        bodyBuilder.Append($"<br/><p>Trân trọng,<br/>Đội ngũ MyPetClinic.</p>");

        mailMessage.Body = bodyBuilder.ToString();

        await client.SendMailAsync(mailMessage);
    }
}
```
