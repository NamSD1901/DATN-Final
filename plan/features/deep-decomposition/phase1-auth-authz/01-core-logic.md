# 🧠 Core Business Logic - Authentication & Authorization

## 1. Cơ chế băm mật khẩu bảo mật (Password Hashing via BCrypt)
Hệ thống **MyPetClinic** tuân thủ tuyệt đối quy định không bao giờ lưu trữ mật khẩu ở dạng văn bản thô (Plaintext). 

*   **Thuật toán áp dụng:** **BCrypt.Net** (được chứng thực chống tấn công brute-force phần cứng ASIC/GPU nhờ cơ chế làm chậm nhân tạo).
*   **Work Factor (Độ muối):** Sử dụng Work Factor = `11` (mức cân bằng tối ưu giữa thời gian xử lý của CPU máy chủ ~100ms và độ khó phá mã).
*   **Mã nguồn C# mẫu:**
```csharp
using BC = BCrypt.Net.BCrypt;

namespace MyPetClinic.Application.Security
{
    public static class PasswordHasher
    {
        // Sinh Hash mật khẩu khi người dùng đăng ký hoặc đổi mật khẩu
        public static string HashPassword(string password)
        {
            return BC.HashPassword(password, workFactor: 11);
        }

        // Kiểm tra khớp mật khẩu khi đăng nhập
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword)) return false;
            return BC.Verify(password, hashedPassword);
        }
    }
}
```

---

## 2. Logic tạo khóa JWT Token (JWT Generation & Signing)
Sau khi người dùng vượt qua vòng xác thực mật khẩu, máy chủ API sẽ khởi tạo mã Token JWT chứa danh tính và quyền của người dùng để trả về cho Client.

*   **Bảo mật khóa chữ ký:** Sử dụng thuật toán `SymmetricSecurityKey` và `SigningCredentials` với thuật toán ký `SecurityAlgorithms.HmacSha256`.
*   **C# Code Reference:**
```csharp
using Microsoft.IdentityModel.Tokens;
using MyPetClinic.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public string GenerateJwtToken(User user, string roleName)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtOptions.SecurityKey); // Lấy khoá bảo mật từ config

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email ?? ""),
        new Claim(ClaimTypes.Role, roleName) // Vai trò: customer, doctor, receptionist, admin
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddHours(24), // Token có hiệu lực trong 24 giờ
        Issuer = _jwtOptions.Issuer,
        Audience = _jwtOptions.Audience,
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key), 
            SecurityAlgorithms.HmacSha256Signature
        )
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
```

---

## 3. Sinh mã OTP an toàn mật mã (Secure OTP Generation)
*   **Vấn đề:** Các hàm sinh số ngẫu nhiên thông thường (như `System.Random` trong C#) hoạt động dựa trên các thuật toán giả ngẫu nhiên có thể dự đoán trước được chuỗi số nếu kẻ tấn công biết thời gian hệ thống khởi chạy.
*   **Giải pháp xử lý:** Sử dụng bộ sinh số ngẫu nhiên chuyên dụng cho mật mã: `System.Security.Cryptography.RandomNumberGenerator` để bảo đảm mã OTP 6 số không thể dự đoán chéo.
*   **Mã nguồn C# sinh OTP:**
```csharp
using System.Security.Cryptography;

public static string GenerateSecureOtp()
{
    // Tạo 4 byte ngẫu nhiên bảo mật
    var bytes = new byte[4];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(bytes);
    }
    
    // Chuyển đổi sang số nguyên dương
    int val = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;
    
    // Lấy dư cho 1,000,000 để thu được số có tối đa 6 chữ số
    int otp = val % 1000000;
    
    // Trả về chuỗi 6 chữ số (tự động đệm số 0 ở đầu nếu cần)
    return otp.ToString("D6");
}
```

---

## 4. Logic phân quyền Route ở Middleware Web API
Tất cả các Endpoint API y tế đều được kiểm soát bởi cơ chế phân quyền dựa trên Role (RBAC) để chặn đứng các request truy cập trái phép.
*   **Cơ chế hoạt động:** Sử dụng bộ lọc `[Authorize(Roles = "receptionist,admin")]` trên Controller hoặc Method.
*   **Chi tiết Middleware block:** ASP.NET Core Authorize Filter tự động đọc Token từ Header, giải mã Claim `ClaimTypes.Role`, và đối chiếu với danh sách Roles được phép. Nếu không khớp, trả về mã lỗi `403 Forbidden` mà không đi vào phương thức xử lý của Controller.