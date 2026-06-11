# 04. Infrastructure & Security - Notifications

Tài liệu thiết kế hạ tầng kết nối, bảo mật WebSockets CORS, chính sách phòng chống IDOR đọc thông báo người dùng khác và cấu hình MailKit SMTP.

---

## 1. Phòng chống tấn công IDOR Hộp thư Thông báo (IDOR Prevention)

Mỗi tài khoản khách hàng đều có hộp thư thông báo in-app riêng chứa các chi tiết y tế và lịch hẹn cá nhân nhạy cảm. Để ngăn chặn lỗi IDOR khi người dùng gọi API `PUT /api/customer/notifications/{id}/read` hoặc `GET /{id}`:

### Thuật toán xác minh sở hữu phía Backend:
```csharp
[HttpPut("{id}/read")]
[Authorize]
public async Task<IActionResult> MarkAsRead(Guid id)
{
    // 1. Giải mã ID chủ sở hữu hiện tại từ JWT Claims
    var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
    {
        return Unauthorized();
    }

    // 2. Tìm thông báo trong database
    var notification = await _context.Notifications.FindAsync(id);
    if (notification == null)
    {
        return NotFound(new { message = "Không tìm thấy thông báo được yêu cầu." });
    }

    // 3. Đối chiếu: Nếu UserId của thông báo khác với UserId đang đăng nhập -> Chặn đứng!
    if (notification.UserId != currentUserId)
    {
        // Trả về 403 Forbidden để phòng chống IDOR
        return Forbid();
    }

    // 4. Nếu hợp lệ, cập nhật trạng thái đã đọc
    notification.IsRead = true;
    await _context.SaveChangesAsync();

    return Ok(new { success = true });
}
```

---

## 2. Cấu hình CORS WebSockets hỗ trợ SignalR

Bởi vì Frontend SPA chạy trên cổng khác (ví dụ: `localhost:5173`) so với cổng Backend API Gateway (`localhost:5001`), kết nối WebSockets của SignalR sẽ bị trình duyệt chặn lại nếu không cấu hình CORS chính sách nghiêm ngặt.

### Cấu hình Program.cs phía Backend .NET:
```csharp
// Đăng ký dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("https://mypetclinic.vn", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Bắt buộc phải cho phép gửi thông tin xác thực đối với WebSockets
    });
});

var app = builder.Build();

app.UseCors("SignalRPolicy");

// Đăng ký SignalR Hub routing
app.MapHub<NotificationHub>("/hubs/notifications");
```
> [!IMPORTANT]
> Thuộc tính `AllowCredentials()` bắt buộc phải kích hoạt, đồng thời trong `WithOrigins` không được sử dụng ký tự wildcard đại diện `*` để tránh lỗ hổng bảo mật CORS nghiêm trọng.

---

## 3. Cấu hình Dịch vụ gửi Email SMTP MailKit

Cấu hình máy chủ SMTP phục vụ nhắc lịch tiêm chủng tại tệp `appsettings.json`:

```json
{
  "MailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderName": "Phong Kham Thu Y MyPetClinic",
    "SenderEmail": "contact@mypetclinic.vn",
    "UserName": "mypetclinic.gmail@gmail.com",
    "Password": "your-app-password-here",
    "EnableSsl": false,
    "UseStartTls": true
  }
}
```

Mã nguồn C# gửi email sử dụng thư viện **MailKit**:
```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
{
    var email = new MimeMessage();
    email.Sender = MailboxAddress.Parse(_mailSettings.SenderEmail);
    email.To.Add(MailboxAddress.Parse(toEmail));
    email.Subject = subject;

    var builder = new BodyBuilder { HtmlBody = htmlBody };
    email.Body = builder.ToMessageBody();

    using var smtp = new SmtpClient();
    try
    {
        // Kết nối qua cổng bảo mật STARTTLS
        await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Gửi email thất bại đến địa chỉ: {toEmail}");
        throw;
    }
}
```

---

## 4. Rate Limiting Giới hạn Tần suất

- **API Đọc thông báo:** Giới hạn tối đa **60 requests / phút** trên một tài khoản khách hàng để tránh spam các API MarkAsRead.
- **WebSocket Connection:** Giới hạn tối đa **5 kết nối mở đồng thời** cho mỗi người dùng, tự động tắt các kết nối cũ hơn (Stale connections) để tối ưu tài nguyên máy chủ.
