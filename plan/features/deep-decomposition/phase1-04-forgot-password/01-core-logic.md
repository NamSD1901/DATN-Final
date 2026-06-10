# 🧠 Core Business Logic & Password Reset Resolver (C# .NET)

Tài liệu này đặc tả logic nghiệp vụ sinh mã OTP khôi phục, xử lý đặt lại mật khẩu bằng BCrypt, tự động mở khóa tài khoản và phòng chống rò rỉ thông tin (User Enumeration).

---

## 1. Logic Xử lý Khôi phục & Đặt lại mật khẩu (Application Layer)

### 1.1. Luồng Gửi mã OTP khôi phục (`AuthService.InitiateForgotPasswordAsync`)
*   **Bảo vệ quyền riêng tư (User Enumeration Protection):** Nếu email gửi lên không tồn tại trong hệ thống, Backend vẫn trả về `Success` tương đương email hợp lệ. Điều này ngăn chặn tin tặc gửi danh sách email tự chế để kiểm tra xem email nào đã có tài khoản trên hệ thống.

```csharp
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthService(IApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> InitiateForgotPasswordAsync(string email)
        {
            string cleanEmail = email.ToLower().Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == cleanEmail);

            // Nếu không tìm thấy User, không ném lỗi mà âm thầm trả về true
            if (user == null)
            {
                return true; 
            }

            // Sinh mã OTP 6 số bảo mật cao
            string otpCode = GenerateSecureOtp();
            user.PasswordResetOtp = otpCode;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5); // OTP có hiệu lực 5 phút
            user.ResetOtpFailedAttempts = 0; // Reset số lần nhập sai OTP của phiên khôi phục mới

            await _context.SaveChangesAsync();

            // Gửi email chứa OTP khôi phục bất đồng bộ
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, otpCode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Forgot Password Error] Lỗi gửi OTP khôi phục tới {user.Email}: {ex.Message}");
                }
            });

            return true;
        }

        public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());

            if (user == null)
            {
                return AuthResult.Fail("Yêu cầu đặt lại mật khẩu không hợp lệ.");
            }

            // 1. Kiểm tra mã OTP khôi phục tồn tại
            if (string.IsNullOrEmpty(user.PasswordResetOtp) || user.PasswordResetOtpExpiry < DateTime.UtcNow)
            {
                return AuthResult.Fail("Mã OTP khôi phục không tồn tại hoặc đã hết hiệu lực.");
            }

            // 2. Kiểm tra giới hạn số lần nhập sai OTP
            if (user.ResetOtpFailedAttempts >= 3)
            {
                // Hủy mã OTP hiện tại để bảo mật
                user.PasswordResetOtp = null;
                user.PasswordResetOtpExpiry = null;
                await _context.SaveChangesAsync();
                return AuthResult.Fail("Mã OTP này đã bị hủy bỏ do nhập sai quá 3 lần. Vui lòng yêu cầu gửi lại OTP mới.");
            }

            // 3. Đối chiếu mã OTP
            if (user.PasswordResetOtp != request.OtpCode)
            {
                user.ResetOtpFailedAttempts++;
                await _context.SaveChangesAsync();
                int attemptsLeft = 3 - user.ResetOtpFailedAttempts;
                return AuthResult.Fail($"Mã OTP không chính xác. Bạn còn {attemptsLeft} lần nhập lại.");
            }

            // 4. Mã hóa mật khẩu mới qua BCrypt
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 11);

            user.PasswordHash = newPasswordHash;
            
            // 5. Tự động mở khóa tài khoản & xóa OTP
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            user.ResetOtpFailedAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return AuthResult.Success(user.Email);
        }

        private string GenerateSecureOtp()
        {
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            uint randomValue = BitConverter.ToUInt32(bytes, 0);
            ulong otpNumber = (randomValue % 900000) + 100000;
            return otpNumber.ToString();
        }
    }
}
```

---

## 2. Giới hạn Nhập sai OTP Khôi phục (Attempts Safety Limit)
*   **Vấn đề bảo mật:** Tin tặc có thể biết email người dùng, bấm yêu cầu khôi phục mật khẩu để gửi OTP về email nạn nhân, sau đó viết script chạy thử liên tục các số từ `100000` đến `999999` để bẻ khóa OTP (Brute-force).
*   **Giải pháp phòng vệ:** Cột `ResetOtpFailedAttempts` lưu số lần xác thực OTP lỗi trong DB. Khi số lần nhập sai đạt **3 lần**, Backend hủy bỏ mã OTP này ngay lập tức. Tin tặc bị chặn đứng và người dùng thật nhận được cảnh báo bảo mật.
