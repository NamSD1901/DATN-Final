# 🧠 Core Business Logic & Authentication Handler (C# .NET)

Tài liệu này đặc tả logic nghiệp vụ kiểm tra mật khẩu sử dụng BCrypt, sinh cấu trúc mã JWT bảo mật cao và quản lý kịch bản khóa tài khoản tạm thời chống tấn công brute-force ở Backend.

---

## 1. Logic Xử lý Đăng nhập & Sinh JWT Token (Application & Infrastructure)

### 1.1. Logic AuthService kiểm tra thông tin Đăng nhập (`AuthService.LoginAsync`)
*   **Kiểm tra tài khoản kích hoạt:** Chỉ cho phép đăng nhập nếu tài khoản đã qua xác thực OTP (`IsActive = true`).
*   **Quản lý trạng thái khóa tài khoản:** Đối chiếu thời gian khóa `LockoutEnd` (nếu có) trước khi thực hiện so khớp mật khẩu.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Common.Interfaces;

namespace MyPetClinic.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApplicationDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower().Trim());
            
            if (user == null)
            {
                return LoginResult.Fail("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 1. Kiểm tra tài khoản có đang bị khóa tạm thời hay không
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                var timeLeftMinutes = Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
                return LoginResult.Fail($"Tài khoản này đang tạm khóa. Vui lòng thử lại sau {timeLeftMinutes} phút.");
            }

            // 2. Kiểm tra tài khoản đã kích hoạt chưa
            if (!user.IsActive)
            {
                return LoginResult.Fail("Tài khoản chưa được kích hoạt qua OTP Email. Vui lòng hoàn tất kích hoạt.");
            }

            // 3. Đối chiếu mật khẩu băm bằng BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                // Tăng số lần nhập sai và khóa tài khoản nếu vượt ngưỡng 5 lần
                user.AccessFailedCount++;
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); // Khóa 15 phút
                    user.AccessFailedCount = 0; // Reset số lần đếm sau khi khóa
                    await _context.SaveChangesAsync();
                    return LoginResult.Fail("Tài khoản đã bị tạm khóa 15 phút do nhập sai mật khẩu quá 5 lần.");
                }

                await _context.SaveChangesAsync();
                return LoginResult.Fail("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 4. Reset các thông số thất bại khi đăng nhập thành công
            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            user.LastLoginAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 5. Sinh JWT Token truy cập
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
                    Role = user.Role.ToString() // Trả về text đại diện cho Role
                }
            };

            return LoginResult.Success(response);
        }
    }
}
```

---

## 2. Thiết lập Sinh JWT Token (Infrastructure Layer)

Dịch vụ `JwtTokenGenerator` chịu trách nhiệm tạo chữ ký số và kết xuất mã token an toàn sử dụng các khóa mã hóa:

```csharp
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            // Thiết lập danh sách các Claims quan trọng của người dùng
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Hỗ trợ Middleware RBAC Authorize ở Backend
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryInHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
```

---

## 3. Khóa tài khoản tạm thời chống dò Mật khẩu (Lockout Schema)
*   Để hỗ trợ tính năng này, bảng `Users` được thiết kế thêm 2 cột quản lý trạng thái:
    *   `AccessFailedCount` (Kiểu số nguyên, mặc định 0): Ghi lại số lần nhập sai liên tục.
    *   `LockoutEnd` (Kiểu thời gian với múi giờ, mặc định null): Ghi thời điểm mở khóa.
*   **Nguyên tắc tự động dọn dẹp:** Khi người dùng đăng nhập thành công, thuộc tính `AccessFailedCount` lập tức được ghi nhận về `0` để tránh ảnh hưởng đến các lần đăng nhập sau.
