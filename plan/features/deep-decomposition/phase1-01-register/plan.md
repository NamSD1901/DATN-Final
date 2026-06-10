# 📝 Implementation Plan & Testing Strategy - Register (Phase 1)

Tài liệu này đặc tả lộ trình triển khai chi tiết từng nhiệm vụ và bộ kịch bản kiểm thử (Test Suite) bao phủ toàn diện cho tính năng Đăng ký tài khoản và Xác thực kích hoạt OTP.

---

## 1. Lộ trình Triển khai Chi tiết (Implementation Phases)

| Giai đoạn | Công việc Cụ thể (Tasks) | Tệp tin Tác động | Kỹ năng Kiểm soát |
| :--- | :--- | :--- | :--- |
| **Phase 1: Database & Domain** | 1. Thiết lập Enum `UserRole` và Entity `User`. <br>2. Chạy migration tạo bảng `Users` trong PostgreSQL. | `Domain/Entities/User.cs`<br>`Infrastructure/Data/Migrations/` | **BE-C02 (EF Core)** |
| **Phase 2: Application Logic** | 1. Tạo các DTOs: `RegisterRequest`, `VerifyOtpRequest`. <br>2. Viết `AuthService` băm mật khẩu qua BCrypt, sinh mã OTP an toàn. | `Application/DTOs/`<br>`Application/Services/AuthService.cs` | **BE-F02 (SOLID)**<br>**BE-F03 (Async)** |
| **Phase 3: Validation & Security** | 1. Viết bộ kiểm duyệt dữ liệu đầu vào sử dụng `FluentValidation`. <br>2. Kiểm tra email duy nhất và chống Race Condition. | `Application/Validators/`<br>`Application/Services/AuthService.cs` | **BE-C01 (API)**<br>**BE-A03 (Security)** |
| **Phase 4: Infrastructure Email** | 1. Cấu hình Mail settings. <br>2. Triển khai `EmailService` gửi email HTML bất đồng bộ qua SMTP. | `Infrastructure/Services/EmailService.cs`<br>`appsettings.json` | **BE-F03 (Fire-and-Forget)** |
| **Phase 5: Web API Endpoints** | 1. Khai báo `AccountController` với các API endpoints. <br>2. Đảm bảo cấu hình CORS và Rate Limiting hoạt động. | `WebApi/Controllers/AccountController.cs`<br>`WebApi/Program.cs` | **BE-C01 (WebAPI)** |
| **Phase 6: Frontend SPA UI** | 1. Xây dựng Pinia store `useRegisterStore.ts`. <br>2. Thiết kế Component form đăng ký mờ kính `RegisterTab.vue`. | `frontend/src/store/useRegisterStore.ts`<br>`frontend/src/components/auth/` | **FE-C01 (Vue 3)**<br>**FE-C03 (Pinia)** |

---

## 2. Chiến lược Kiểm thử tự động (Testing Strategy)

Chúng ta xây dựng bộ kiểm thử phân tầng gồm Unit Test cho dịch vụ backend và Integration Test tích hợp kiểm thử liên thông từ đăng ký đến kích hoạt.

### 2.1. Bộ Kiểm thử Đơn vị Backend (Unit Tests với xUnit & FluentAssertions)
Tệp tin: `tests/MyPetClinic.Tests/Services/AuthServiceTests.cs`

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
    public class AuthServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly Mock<IEmailService> _emailServiceMock;

        public AuthServiceTests()
        {
            // Sử dụng In-Memory Database để kiểm thử unit test nhanh và độc lập
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _emailServiceMock = new Mock<IEmailService>();
        }

        [Fact]
        public async Task RegisterAsync_HappyPath_ShouldCreateInactiveUserAndSendOtp()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var authService = new AuthService(context, _emailServiceMock.Object);
            
            var request = new RegisterRequest
            {
                FullName = "Nguyễn Văn A",
                Email = "register_test@example.com",
                PhoneNumber = "0912345678",
                Password = "Password123!"
            };

            // Act
            var result = await authService.RegisterAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Email.Should().Be("register_test@example.com");

            // Kiểm tra DB lưu đúng thông tin
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            userInDb.Should().NotBeNull();
            userInDb!.FullName.Should().Be("Nguyễn Văn A");
            userInDb.IsActive.Should().BeFalse(); // Chưa kích hoạt
            userInDb.ActivationOtp.Should().HaveLength(6); // Sinh OTP 6 chữ số
            userInDb.OtpExpiry.Should().BeAfter(DateTime.UtcNow);

            // Xác minh EmailService đã được gọi gửi mail OTP đúng địa chỉ
            _emailServiceMock.Verify(x => x.SendActivationEmailAsync(
                request.Email, 
                request.FullName, 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task VerifyOtpAsync_CorrectOtp_ShouldActivateUserAndClearOtpFields()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var testUser = new User
            {
                FullName = "Nguyễn Văn A",
                Email = "verify_test@example.com",
                PhoneNumber = "0912345678",
                PasswordHash = "hashed_pass",
                IsActive = false,
                ActivationOtp = "888888",
                OtpExpiry = DateTime.UtcNow.AddMinutes(5)
            };
            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, _emailServiceMock.Object);
            var request = new VerifyOtpRequest
            {
                Email = "verify_test@example.com",
                OtpCode = "888888"
            };

            // Act
            var result = await authService.VerifyOtpAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            
            var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            updatedUser!.IsActive.Should().BeTrue(); // Đã kích hoạt
            updatedUser.ActivationOtp.Should().BeNull(); // Đã dọn dẹp OTP
            updatedUser.OtpExpiry.Should().BeNull();
        }
    }
}
```

---

## 3. Bộ Kịch bản Kiểm thử QA (QA Test Cases Suite)

### 🔴 Nhóm 1: Kiểm thử Nghiệp vụ Chính (Critical Paths)
*   **TC-REG-001: Đăng ký thành công hoàn chỉnh (Happy Path)**
    *   *Đầu vào:* Email chưa tồn tại, SĐT đúng chuẩn, password mạnh.
    *   *Kỳ vọng:* Hệ thống tạo tài khoản `IsActive = false` -> gửi OTP -> nhập OTP đúng -> kích hoạt thành công, chuyển hướng trang.
*   **TC-REG-002: Đăng ký trùng địa chỉ Email**
    *   *Đầu vào:* Email `trungemail@example.com` đã đăng ký trong DB.
    *   *Kỳ vọng:* API trả về lỗi `422 Unprocessable Entity` với mã lỗi `EMAIL_ALREADY_EXISTS`. Không sinh OTP mới.
*   **TC-REG-003: Xác thực mã OTP không chính xác**
    *   *Đầu vào:* Nhập OTP `000000` (mã đúng là `999999`).
    *   *Kỳ vọng:* Báo lỗi màu đỏ ở UI, API trả về `400 Bad Request`, tài khoản giữ nguyên trạng thái `IsActive = false`.

### 🟠 Nhóm 2: Kiểm thử Giá trị Biên & Bảo mật (Boundary & Security)
*   **TC-REG-004: Mật khẩu không đáp ứng độ mạnh tối thiểu**
    *   *Đầu vào:* Mật khẩu `12345` hoặc `abcdefgh`.
    *   *Kỳ vọng:* Client-side validation chặn ngay lập tức, nút đăng ký bị khóa, báo lỗi độ phức tạp mật khẩu.
*   **TC-REG-005: OTP hết hạn hiệu lực**
    *   *Đầu vào:* Đăng ký thành công -> Chờ quá 5 phút -> Nhập OTP đúng.
    *   *Kỳ vọng:* API trả về lỗi `400 Bad Request` với mã `OTP_EXPIRED`. Tài khoản vẫn chưa được kích hoạt.
*   **TC-REG-006: Spam nút Gửi lại OTP**
    *   *Đầu vào:* Nhấn "Gửi lại OTP" liên tục trong 10 giây.
    *   *Kỳ vọng:* Nút bị khóa hiển thị đồng hồ đếm ngược. Nếu gọi trực tiếp API gửi lại OTP lần 2 trong vòng 60 giây, API trả về lỗi `429 Too Many Requests`.
