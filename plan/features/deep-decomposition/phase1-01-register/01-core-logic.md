# 🧠 Core Business Logic & Backend Service Pipeline (C# .NET)

Tài liệu này đặc tả logic nghiệp vụ cốt lõi, quy tắc xử lý mật khẩu, sinh mã OTP bảo mật cao và phòng tránh lỗi đồng thời (Concurrency / Race Condition) tại lớp Application Service.

---

## 1. Logic Xử lý Nghiệp vụ Đăng ký & Kích hoạt (Application Layer)

Chúng ta sử dụng `AuthService` triển khai quy trình đăng ký tài khoản và kích hoạt OTP một cách an toàn.

### 1.1. Logic Đăng ký Tài khoản mới (`AuthService.RegisterAsync`)
*   **Sinh mã OTP bảo mật:** Thay vì dùng `System.Random` (không an toàn và dễ bị đoán trước), chúng ta sử dụng `System.Security.Cryptography.RandomNumberGenerator` để tạo ra chuỗi OTP 6 số thực sự ngẫu nhiên.
*   **Băm mật khẩu:** Sử dụng thư viện `BCrypt.Net` với `workFactor: 11` (mức cân bằng tốt nhất giữa an toàn và hiệu năng, tốn khoảng 250-300ms xử lý trên CPU máy chủ).

```csharp
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BCrypt.Net;
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

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            // 1. Kiểm tra Email đã tồn tại trong Hệ thống chưa
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email.ToLower().Trim());
            if (emailExists)
            {
                return AuthResult.Fail("Địa chỉ email này đã được sử dụng để đăng ký tài khoản.");
            }

            // 2. Băm mật khẩu bằng BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);

            // 3. Khởi tạo thực thể User mới
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = request.Email.ToLower().Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                PasswordHash = passwordHash,
                IsActive = false, // Tài khoản ở trạng thái chờ kích hoạt
                Role = UserRole.KhachHang,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Sinh mã OTP 6 chữ số mật mã học (Cryptographically Secure OTP)
            string otpCode = GenerateSecureOtp();
            user.ActivationOtp = otpCode;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5); // OTP có hiệu lực 5 phút

            // 5. Lưu vào Cơ sở dữ liệu với xử lý Concurrency
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Xử lý kịch bản Race Condition khi hai request đăng ký cùng email đến database đồng thời
                if (ex.InnerException?.Message.Contains("IX_Users_Email") == true || 
                    ex.InnerException?.Message.Contains("23505") == true) // PostgreSQL unique_violation code
                {
                    return AuthResult.Fail("Địa chỉ email đã được đăng ký bởi một người dùng khác ngay trước đó.");
                }
                throw;
            }

            // 6. Gửi Email chứa mã OTP bất đồng bộ (Fire-and-Forget / Background Worker)
            // Không sử dụng await ở đây để tránh block luồng phản hồi HTTP trả về cho Client
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendActivationEmailAsync(user.Email, user.FullName, otpCode);
                }
                catch (Exception mailEx)
                {
                    // Ghi log lỗi gửi thư để quản trị viên theo dõi, không ném exception làm ngắt luồng đăng ký
                    Console.WriteLine($"[Email Service Error] Không thể gửi OTP tới {user.Email}: {mailEx.Message}");
                }
            });

            return AuthResult.Success(user.Email);
        }

        public async Task<AuthResult> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());
            
            if (user == null)
            {
                return AuthResult.Fail("Không tìm thấy tài khoản người dùng tương ứng với email này.");
            }

            if (user.IsActive)
            {
                return AuthResult.Fail("Tài khoản này đã được kích hoạt trước đó. Vui lòng tiến hành đăng nhập.");
            }

            if (user.ActivationOtp != request.OtpCode)
            {
                return AuthResult.Fail("Mã OTP bạn nhập không chính xác.");
            }

            if (user.OtpExpiry < DateTime.UtcNow)
            {
                return AuthResult.Fail("Mã OTP đã hết hiệu lực. Vui lòng bấm gửi lại mã OTP mới.");
            }

            // Kích hoạt tài khoản và xóa sạch thông tin OTP
            user.IsActive = true;
            user.ActivationOtp = null;
            user.OtpExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return AuthResult.Success(user.Email);
        }

        private string GenerateSecureOtp()
        {
            // Sử dụng RandomNumberGenerator để tạo số ngẫu nhiên không đoán trước được trong khoảng [100000, 999999]
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

    public class AuthResult
    {
        public bool IsSuccess { get; }
        public string Email { get; }
        public string ErrorMessage { get; }

        private AuthResult(bool success, string email, string errorMessage)
        {
            IsSuccess = success;
            Email = email;
            ErrorMessage = errorMessage;
        }

        public static AuthResult Success(string email) => new AuthResult(true, email, string.Empty);
        public static AuthResult Fail(string errorMessage) => new AuthResult(false, string.Empty, errorMessage);
    }
}
```

---

## 2. Phòng tránh Race Condition (Database Level)
Nếu hai luồng xử lý đăng ký tài khoản cùng sử dụng một địa chỉ Email đồng thời (gửi song song cùng một mili-giây):
1. Cả hai luồng kiểm tra `AnyAsync(u => u.Email == email)` đều nhận được kết quả `false`.
2. Luồng A tiến hành chèn bản ghi vào bảng và lưu thành công.
3. Luồng B tiếp tục chèn bản ghi, tuy nhiên database PostgreSQL phát hiện vi phạm ràng buộc duy nhất (`UNIQUE INDEX "IX_Users_Email"`).
4. Hệ thống ném ra ngoại lệ `DbUpdateException` với mã lỗi PostgreSQL `23505` (hoặc thông điệp chứa tên index).
5. Khối `try-catch` trong `AuthService` bắt lỗi này và trả về kết quả lỗi lịch sự: `"Địa chỉ email đã được đăng ký bởi một người dùng khác ngay trước đó"`.
