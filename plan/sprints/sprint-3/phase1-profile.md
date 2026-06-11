# ⚙️ Đặc Tả Kỹ Thuật Chi Tiết - Reset Password & Personal Profile (Sprint 3)

Tài liệu này đặc tả chi tiết mã nguồn, cấu trúc lớp, sơ đồ dữ liệu, API contracts và kịch bản unit test cho **Sprint 3** của dự án **MyPetClinic**.

---

## 1. Thiết Kế Cơ Sở Dữ Liệu & Thực Thể (Database Modifications)

Để hỗ trợ tính năng quên mật khẩu, thực thể `User` cần bổ sung các cột lưu trữ mã OTP lâm thời và thời điểm hết hạn của OTP đó.

### 1.1. Cập Nhật Thực Thể Domain (C# Domain Entity)

```csharp
// Location: Domain/Entities/User.cs (Bổ sung thuộc tính)
namespace MyPetClinic.Domain.Entities
{
    public class User
    {
        // ... Các thuộc tính đã có từ Sprint 1 & 2 ...

        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
    }
}
```

### 1.2. Cấu Hình Fluent API Bổ Sung

```csharp
// Location: Infrastructure/Data/Configurations/UserConfiguration.cs (Bổ sung)
builder.Property(u => u.OtpCode).HasMaxLength(6).IsRequired(false);
builder.Property(u => u.OtpExpiry).IsRequired(false);
```

---

## 2. Giao Ước API & Nghiệp Vụ Cập Nhật (API Contracts & Profile Services)

### 2.1. Thiết Kế Interfaces Cho Dịch Vụ Gửi Email (DIP)

```csharp
// Location: Application/Common/Interfaces/IEmailService.cs
using System.Threading.Tasks;

namespace MyPetClinic.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
```

### 2.2. Chi Tiết Lớp Nghiệp Vụ Hồ Sơ Cá Nhân & Quên Mật Khẩu (ProfileService.cs)

```csharp
// Location: Application/Services/ProfileService.cs
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Domain.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class ProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;

        public ProfileService(IUserRepository userRepository, IEmailService emailService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<User> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");
            return user;
        }

        public async Task UpdateProfileAsync(Guid userId, string fullName, string? phone, short? gender, DateTime? dateOfBirth, string? address)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy người dùng.");

            user.FullName = fullName;
            user.Phone = phone;
            user.Gender = gender;
            user.DateOfBirth = dateOfBirth;
            user.Address = address;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SendOtpAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Để bảo mật thông tin, không báo email không tồn tại. Chỉ log hoặc ném lỗi chung.
                throw new InvalidOperationException("Nếu email tồn tại, hệ thống đã gửi mã OTP.");
            }

            // Sinh mã OTP 6 chữ số ngẫu nhiên
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5); // Có hiệu lực trong 5 phút

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Gửi email SMTP
            var subject = "Mã xác thực phục hồi mật khẩu - MyPetClinic";
            var body = $"Chào {user.FullName},<br/><br/>Mã OTP phục hồi mật khẩu của bạn là: <b>{otp}</b>.<br/>Mã này có hiệu lực trong vòng 5 phút.<br/><br/>Thân ái!";
            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task<string> VerifyOtpAndGenerateResetTokenAsync(string email, string otp)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.OtpCode != otp || user.OtpExpiry < DateTime.UtcNow)
            {
                throw new ArgumentException("Mã OTP không hợp lệ hoặc đã hết hạn.");
            }

            // Sinh token tạm thời dùng một lần
            var tempResetToken = Guid.NewGuid().ToString();
            
            // Xóa OTP sau khi dùng để tránh replay attack
            user.OtpCode = null;
            user.OtpExpiry = null;
            
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return tempResetToken;
        }

        public async Task ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

### 2.3. Khai Báo API Endpoints (ProfilesController)

```csharp
// Location: WebApi/Controllers/ProfilesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/profiles")]
    public class ProfilesController : ControllerBase
    {
        private readonly ProfileService _profileService;

        public ProfilesController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdVal)) throw new UnauthorizedAccessException();
            return Guid.Parse(userIdVal);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var user = await _profileService.GetProfileAsync(GetCurrentUserId());
            return Ok(new { user.FullName, user.Email, user.Phone, user.Gender, user.DateOfBirth, user.Address, user.Avatar });
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            await _profileService.UpdateProfileAsync(GetCurrentUserId(), request.FullName, request.Phone, request.Gender, request.DateOfBirth, request.Address);
            return Ok(new { Message = "Cập nhật hồ sơ thành công!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _profileService.SendOtpAsync(request.Email);
                return Ok(new { Message = "Mã OTP đã được gửi về email của bạn." });
            }
            catch (Exception)
            {
                // Tránh lộ tài khoản có tồn tại hay không
                return Ok(new { Message = "Nếu email tồn tại, mã OTP đã được gửi đi." });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var resetToken = await _profileService.VerifyOtpAndGenerateResetTokenAsync(request.Email, request.Otp);
                return Ok(new { ResetToken = resetToken, Message = "Xác nhận OTP thành công." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _profileService.ResetPasswordAsync(request.Email, request.NewPassword);
            return Ok(new { Message = "Đặt lại mật khẩu thành công!" });
        }
    }

    public record UpdateProfileRequest(string FullName, string? Phone, short? Gender, DateTime? DateOfBirth, string? Address);
    public record ForgotPasswordRequest(string Email);
    public record VerifyOtpRequest(string Email, string Otp);
    public record ResetPasswordRequest(string Email, string NewPassword);
}
```

---

## 3. Quản Lý Trạng Thái Phía Client-side SPA (Vue 3 Pinia Store)

```typescript
// Location: frontend/src/stores/profile.ts
import { defineStore } from 'pinia';
import api from '../utils/api';

export const useProfileStore = defineStore('profile', {
  state: () => ({
    profileData: null as any,
    loading: false,
    error: null as string | null
  }),
  actions: {
    async fetchMyProfile() {
      this.loading = true;
      try {
        const response = await api.get('/profiles/me');
        this.profileData = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải thông tin cá nhân.';
      } finally {
        this.loading = false;
      }
    },
    async updateProfile(payload: any) {
      await api.put('/profiles/me', payload);
      this.profileData = { ...this.profileData, ...payload };
    },
    async requestOtp(email: string) {
      await api.post('/profiles/forgot-password', { email });
    },
    async verifyOtp(email: string, otp: string) {
      const response = await api.post('/profiles/verify-otp', { email, otp });
      return response.data.resetToken;
    },
    async resetPassword(email: string, newPassword: string) {
      await api.post('/profiles/reset-password', { email, newPassword });
    }
  }
});
```

---

## 4. Kịch Bản Kiểm Thử Xác Thực OTP & Đổi Mật Khẩu (xUnit Tests)

```csharp
// Location: MyPetClinic.Tests/Application/ProfileServiceTests.cs
using FluentAssertions;
using Moq;
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests.Application
{
    public class ProfileServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _profileService = new ProfileService(_userRepoMock.Object, _emailServiceMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task VerifyOtp_ShouldThrowException_WhenOtpIsExpired()
        {
            // Arrange
            var email = "user@mypetclinic.com";
            var existingUser = new User
            {
                Email = email,
                OtpCode = "123456",
                OtpExpiry = DateTime.UtcNow.AddMinutes(-1) // Hết hạn 1 phút trước
            };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _profileService.VerifyOtpAndGenerateResetTokenAsync(email, "123456");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Mã OTP không hợp lệ hoặc đã hết hạn.");
        }

        [Fact]
        public async Task VerifyOtp_ShouldReturnToken_WhenOtpIsValid()
        {
            // Arrange
            var email = "user@mypetclinic.com";
            var existingUser = new User
            {
                Email = email,
                OtpCode = "123456",
                OtpExpiry = DateTime.UtcNow.AddMinutes(5) // Còn hạn 5 phút
            };

            _userRepoMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(existingUser);

            // Act
            var token = await _profileService.VerifyOtpAndGenerateResetTokenAsync(email, "123456");

            // Assert
            token.Should().NotBeNullOrEmpty();
            existingUser.OtpCode.Should().BeNull(); // Đã xóa mã
            existingUser.OtpExpiry.Should().BeNull();
            _userRepoMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
        }
    }
}
```
