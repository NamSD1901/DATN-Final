# 🧠 Core Business Logic & Google Token Resolver (C# .NET)

Tài liệu này đặc tả logic nghiệp vụ giải mã Token Google, cơ chế tự động tạo hồ sơ khách hàng mới (Auto-Provisioning) và cấp phát mã JWT tương đương tại tầng Backend.

---

## 1. Logic Xử lý Đăng nhập Google (`AuthService.LoginWithGoogleAsync`)

### 1.1. Luồng xử lý nghiệp vụ chính
*   **Xác thực phía Google:** Đổi IdToken lấy thông tin định danh của Google (`GoogleJsonWebSignature.Payload`).
*   **Liên kết tài khoản tự động:** Nếu email đã tồn tại trong DB, bỏ qua bước chèn dữ liệu và tiến hành đăng nhập trực tiếp.
*   **Sinh ngẫu nhiên password:** Người dùng đăng nhập qua Google sẽ không có mật khẩu. Hệ thống tự động tạo một chuỗi password băm ngẫu nhiên cực dài để khóa tài khoản khỏi kịch bản đăng nhập thông thường bằng mật khẩu trống.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Auth;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Common.Interfaces;

namespace MyPetClinic.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IGoogleTokenValidator _googleTokenValidator;

        public AuthService(
            IApplicationDbContext context, 
            IJwtTokenGenerator jwtTokenGenerator,
            IGoogleTokenValidator googleTokenValidator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _googleTokenValidator = googleTokenValidator;
        }

        public async Task<LoginResult> LoginWithGoogleAsync(GoogleLoginRequest request)
        {
            // 1. Xác thực Google Token thông qua Validator chuyên biệt
            var payload = await _googleTokenValidator.ValidateTokenAsync(request.IdToken);
            if (payload == null)
            {
                return LoginResult.Fail("Xác thực Token Google không thành công hoặc mã xác thực đã hết hạn.");
            }

            string googleEmail = payload.Email.ToLower().Trim();
            string googleFullName = payload.Name ?? "Google User";

            // 2. Tìm kiếm User trong Database theo Email nhận được từ Google
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == googleEmail);

            if (user == null)
            {
                // 3. Tự động Tạo mới Tài khoản (Auto-Provisioning)
                // Sinh một password hash ngẫu nhiên bảo mật cao để chặn đăng nhập mật khẩu thông thường
                string randomDummyPassword = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
                string dummyPasswordHash = BCrypt.Net.BCrypt.HashPassword(randomDummyPassword, workFactor: 11);

                user = new User
                {
                    FullName = googleFullName,
                    Email = googleEmail,
                    PhoneNumber = string.Empty, // Khách hàng sẽ được yêu cầu bổ sung SĐT ở trang Profile sau
                    PasswordHash = dummyPasswordHash,
                    IsActive = true, // Tự động kích hoạt do đã được Google kiểm tra email chính xác
                    Role = UserRole.KhachHang,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // 4. Nếu tài khoản đang bị khóa tạm thời (Lockout)
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var timeLeftMinutes = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return LoginResult.Fail($"Tài khoản liên kết của bạn hiện đang bị khóa tạm thời. Vui lòng quay lại sau {timeLeftMinutes} phút.");
            }

            // 5. Cập nhật lịch sử đăng nhập
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 6. Cấp mã JWT Token truy cập của MyPetClinic
            var token = _jwtTokenGenerator.GenerateToken(user);

            var response = new LoginResponse
            {
                Token = token,
                ExpiresInSeconds = 12 * 60 * 60, // 12 giờ
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };

            return LoginResult.Success(response);
        }
    }
}
```

---

## 2. Giải pháp Đồng bộ Ảnh đại diện (Profile Picture Sync)
*   **Hành vi nâng cao:** Đối tượng `GoogleJsonWebSignature.Payload` trả về thuộc tính `payload.Picture` chứa URL ảnh đại diện của tài khoản Google của người dùng.
*   **Giải pháp lưu trữ:** Trong các phase tiếp theo, Backend có thể tải ảnh này về và lưu vào thư mục Avatar tĩnh của User hoặc lưu liên kết trực tiếp để tối ưu hóa không gian lưu trữ đĩa của phòng khám.
