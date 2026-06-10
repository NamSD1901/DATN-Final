# 📝 Implementation Plan & Testing Strategy - Forgot Password (Phase 1)

Tài liệu này đặc tả lộ trình thực thi từng giai đoạn phát triển và bộ kịch bản kiểm thử (Test Suite) bao phủ toàn diện cho tính năng Khôi phục mật khẩu.

---

## 1. Lộ trình Triển khai Chi tiết (Implementation Phases)

| Giai đoạn | Công việc Cụ thể (Tasks) | Tệp tin Tác động | Kỹ năng Kiểm soát |
| :--- | :--- | :--- | :--- |
| **Phase 1: Database Setup** | 1. Bổ sung trường `PasswordResetOtp`, `PasswordResetOtpExpiry`, `ResetOtpFailedAttempts` vào Entity `User`. <br>2. Chạy migration cập nhật DB. | `Domain/Entities/User.cs`<br>`Infrastructure/Data/Migrations/` | **BE-C02 (EF Core)** |
| **Phase 2: Core Logic Service** | 1. Viết logic sinh OTP khôi phục an toàn. <br>2. Xây dựng dịch vụ băm mật khẩu mới BCrypt và tự động reset lockout. | `Application/Services/AuthService.cs` | **BE-F02 (SOLID)**<br>**BE-A03 (Security)** |
| **Phase 3: Email Templates** | 1. Cấu hình Email Template khôi phục mật khẩu dạng HTML. <br>2. Triển khai phương thức gửi mail bất đồng bộ. | `Infrastructure/Services/EmailService.cs` | **BE-F03 (Fire-and-Forget)** |
| **Phase 4: Web API Endpoints** | 1. Viết endpoints: `/api/account/forgot-password` và `/api/account/reset-password`. <br>2. Cấu hình Rate Limiting. | `WebApi/Controllers/AccountController.cs` | **BE-C01 (WebAPI)** |
| **Phase 5: Store & Wizard UI** | 1. Xây dựng store `useForgotPasswordStore.ts` quản lý wizard. <br>2. Thiết kế Component `ForgotPasswordWizard.vue` trượt mượt mà. | `frontend/src/store/`<br>`frontend/src/components/auth/` | **FE-C03 (Pinia)**<br>**FE-C01 (Vue 3)** |

---

## 2. Chiến lược Kiểm thử tự động (Testing Strategy)

Chúng ta xây dựng bộ kiểm thử đơn vị (Unit Tests) để kiểm tra các điều kiện biên nhập sai OTP và tự động mở khóa tài khoản.

### 2.1. Bộ Kiểm thử Đơn vị Backend (Unit Tests với xUnit & FluentAssertions)
Tệp tin: `tests/MyPetClinic.Tests/Services/AuthServiceForgotPasswordTests.cs`

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using Xunit;

namespace MyPetClinic.Tests.Services
{
    public class AuthServiceForgotPasswordTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly Mock<IEmailService> _emailServiceMock;

        public AuthServiceForgotPasswordTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _emailServiceMock = new Mock<IEmailService>();
        }

        [Fact]
        public async Task ResetPasswordAsync_WrongOtpThreeTimes_ShouldInvalidateOtp()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var testUser = new User
            {
                FullName = "Nguyễn Văn A",
                Email = "reset_fail@example.com",
                PasswordHash = "hashed_pass",
                IsActive = true,
                PasswordResetOtp = "999999",
                PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(5),
                ResetOtpFailedAttempts = 2 // Đã nhập sai 2 lần trước đó
            };
            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, _emailServiceMock.Object);
            var request = new ResetPasswordRequest
            {
                Email = "reset_fail@example.com",
                OtpCode = "000000", // Gửi sai OTP lần thứ 3
                NewPassword = "NewPassword123!"
            };

            // Act
            var result = await authService.ResetPasswordAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("hủy bỏ do nhập sai quá 3 lần");

            // Xác minh mã OTP trong database đã bị xoá sạch để bảo mật
            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            updatedUser!.PasswordResetOtp.Should().BeNull();
            updatedUser.PasswordResetOtpExpiry.Should().BeNull();
        }
    }
}
```

---

## 3. Bộ Kịch bản Kiểm thử QA (QA Test Cases Suite)

### 🔴 Nhóm 1: Kiểm thử Nghiệp vụ Chính (Critical Paths)
*   **TC-FGP-001: Khôi phục mật khẩu thành công (Happy Path)**
    *   *Đầu vào:* Email có thật, OTP nhập đúng, mật khẩu mới mạnh.
    *   *Kỳ vọng:* Đổi mật khẩu thành công, tài khoản tự động mở khóa, OTP bị xóa khỏi DB.
*   **TC-FGP-002: Yêu cầu khôi phục cho email không tồn tại**
    *   *Đầu vào:* Gửi email `nonexistent@gmail.com`.
    *   *Kỳ vọng:* API trả về `200 OK` với thông điệp chung chung (chống dò email). Không có email thực nào được gửi đi.

### 🟠 Nhóm 2: Kiểm thử Giá trị Biên & Bảo mật (Boundary & Security)
*   **TC-FGP-003: Nhập sai OTP quá 3 lần**
    *   *Hành động:* Nhập sai OTP khôi phục 3 lần liên tiếp.
    *   *Kỳ vọng:* API báo lỗi hủy OTP. Frontend tự trượt ngược về Bước 1. OTP trong DB biến mất.
*   **TC-FGP-004: Đặt lại mật khẩu yếu**
    *   *Đầu vào:* Nhập mật khẩu mới là `123`.
    *   *Kỳ vọng:* Validation của Client khóa nút Hoàn tất, hiện cảnh báo đỏ về độ mạnh mật khẩu.
