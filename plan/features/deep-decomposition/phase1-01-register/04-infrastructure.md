# 🌐 Infrastructure & Security Specification - Register Account (Phase 1)

Tài liệu này đặc tả cấu trúc hạ tầng email, cơ chế gửi thư kích hoạt bất đồng bộ và các chính sách bảo mật tầng mạng/tầng ứng dụng để bảo vệ tính năng Đăng ký tài khoản khỏi các hành vi spam và tấn công brute-force.

---

## 1. Dịch vụ Gửi Email Kích hoạt (Email SMTP Infrastructure)

Hệ thống sử dụng thư viện **MailKit** (khuyên dùng cho .NET thay thế cho `SmtpClient` đã lỗi thời) để thực hiện kết nối và gửi email HTML bất đồng bộ qua giao thức SMTP (hỗ trợ Mailtrap cho môi trường phát triển/kiểm thử và SendGrid/Gmail SMTP cho môi trường Production).

### 1.1. Cấu hình Email Settings (`appsettings.json`)
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailtrap.io",
    "Port": 587,
    "SenderName": "Hệ thống Phòng Khám Thú Y MyPetClinic",
    "SenderEmail": "no-reply@mypetclinic.com",
    "Username": "your-smtp-username",
    "Password": "your-smtp-password",
    "EnableSsl": true
  }
}
```

### 1.2. Triển khai Dịch vụ gửi Email (`EmailService.cs`)
```csharp
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using MyPetClinic.Application.Common.Interfaces;

namespace MyPetClinic.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendActivationEmailAsync(string toEmail, string recipientName, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(recipientName, toEmail));
            message.Subject = "🏥 Kích hoạt Tài khoản MyPetClinic - Mã OTP của bạn";

            // Xây dựng template nội dung HTML chuyên nghiệp cho phòng khám thú y
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <div style='text-align: center; margin-bottom: 20px;'>
                        <h2 style='color: #0d9488; margin: 0;'>MyPetClinic Vet Hospital</h2>
                        <p style='color: #64748b; font-size: 14px; margin: 5px 0 0 0;'>Chăm sóc thú cưng bằng cả trái tim</p>
                    </div>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin-bottom: 20px;' />
                    <p>Chào <strong>{recipientName}</strong>,</p>
                    <p>Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi để đồng hành chăm sóc sức khỏe cho các bé cưng.</p>
                    <p>Để hoàn tất quá trình đăng ký và kích hoạt tài khoản, vui lòng sử dụng mã OTP xác minh bên dưới:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #0d9488; background-color: #f0fdfa; padding: 10px 30px; border: 1px dashed #0d9488; border-radius: 6px; display: inline-block;'>{otpCode}</span>
                    </div>
                    <p style='color: #e11d48; font-weight: bold;'>Lưu ý: Mã OTP này có hiệu lực trong vòng 5 phút.</p>
                    <p style='color: #64748b; font-size: 12px; margin-top: 30px;'>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này. Tài khoản sẽ tự động bị dọn dẹp sau 24 giờ nếu không được kích hoạt.</p>
                </div>"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            // Cho phép bỏ qua kiểm tra chứng chỉ SSL/TLS khi debug cục bộ
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
```

---

## 2. Các Biện pháp An ninh & Phòng chống Spam (Security Shields)

### 2.1. Rate Limiting trên Endpoint Đăng ký & Gửi OTP
*   **Mục tiêu:** Ngăn chặn tin tặc chạy tool spam liên tục gửi request đăng ký tạo tài khoản rác làm cạn kiệt tài nguyên database và phát sinh chi phí gửi email lớn.
*   **Chính sách Rate Limiting (.NET Core Middleware):**
    *   *Endpoint:* `POST /api/account/register` và `POST /api/account/resend-otp`.
    *   *Thuật toán:* Token Bucket / Fixed Window.
    *   *Giới hạn:* **Tối đa 3 requests/phút trên mỗi địa chỉ IP**.
    *   *Xử lý vi phạm:* Trả về mã lỗi `HTTP 429 Too Many Requests` kèm thông báo `"Bạn đã gửi yêu cầu quá nhanh. Vui lòng thử lại sau 1 phút."`

### 2.2. Làm sạch Dữ liệu & Chống tấn công XSS/SQL Injection
*   **SQL Injection:** Toàn bộ câu lệnh truy vấn EF Core được dịch mã thành các Parameterized Queries (truy vấn có tham số), hoàn toàn chặn đứng nguy cơ chèn mã độc SQL qua các trường `FullName` hay `Email`.
*   **XSS Protection (Cross-Site Scripting):** Phía Web API thực hiện lọc bỏ và mã hóa HTML Entities cho trường `FullName` trước khi lưu vào database để tránh việc người dùng nhập các thẻ nguy hại như `<script>alert('xss')</script>`.

### 2.3. Cấu hình CORS (Cross-Origin Resource Sharing)
*   Chỉ cho phép ứng dụng Single Page Application (SPA) chạy từ domain Frontend chính thức (được định cấu hình trong biến môi trường `AllowedOrigins`) được quyền kết nối tới API Đăng ký tài khoản. Mọi truy cập chéo tên miền lạ từ trình duyệt sẽ bị chặn đứng ngay ở tầng CORS Middleware.
