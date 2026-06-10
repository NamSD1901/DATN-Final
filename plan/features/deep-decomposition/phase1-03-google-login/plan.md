# 📝 Implementation Plan & Testing Strategy - Google Login (Phase 1)

Tài liệu này đặc tả lộ trình thực thi từng giai đoạn phát triển và bộ kịch bản kiểm thử (Test Suite) bao phủ toàn diện cho tính năng Đăng nhập qua bên thứ ba (Google OAuth).

---

## 1. Lộ trình Triển khai Chi tiết (Implementation Phases)

| Giai đoạn | Công việc Cụ thể (Tasks) | Tệp tin Tác động | Kỹ năng Kiểm soát |
| :--- | :--- | :--- | :--- |
| **Phase 1: Config & Package** | 1. Cài đặt gói NuGet `Google.Apis.Auth`. <br>2. Cấu hình `GoogleAuthSettings` Client ID trong json file. | `WebApi/appsettings.json`<br>`MyPetClinic.WebApi.csproj` | **BE-A03 (Dependencies)** |
| **Phase 2: Auth Service Code** | 1. Định nghĩa Interface `IGoogleTokenValidator`. <br>2. Viết dịch vụ `GoogleTokenValidator` giải mã token bằng thư viện Google. | `Application/Common/`<br>`Infrastructure/Authentication/` | **BE-F02 (SOLID)**<br>**BE-A03 (Security)** |
| **Phase 3: Controller & Service** | 1. Cập nhật `AuthService.LoginWithGoogleAsync()` xử lý Auto-Provisioning. <br>2. Viết endpoint `POST /api/account/google-login`. | `Application/Services/AuthService.cs`<br>`WebApi/Controllers/AccountController.cs` | **BE-C01 (WebAPI)**<br>**BE-C02 (EF Core)** |
| **Phase 4: Frontend SDK Load** | 1. Tích hợp nạp script Google GIS SDK ở file HTML chính. <br>2. Cấu hình Client ID của Google vào Vue config. | `frontend/index.html`<br>`frontend/src/config/` | **FE-C01 (Vue 3)** |
| **Phase 5: Store & Button Render** | 1. Mở rộng Pinia store `useAuthStore.ts` với action `loginWithGoogle`. <br>2. Render nút bấm Google động và đăng ký hàm Callback. | `frontend/src/store/useAuthStore.ts`<br>`frontend/src/components/auth/` | **FE-C03 (Pinia)** |

---

## 2. Chiến lược Kiểm thử tự động (Testing Strategy)

Chúng ta xây dựng bộ kiểm thử đơn vị (Unit Tests) để giả lập (mock) quá trình xác thực token Google mà không cần kết nối mạng thật.

### 2.1. Bộ Kiểm thử Đơn vị Backend (Unit Tests với xUnit & Moq)
Tệp tin: `tests/MyPetClinic.Tests/Services/AuthServiceGoogleTests.cs`

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using Xunit;

namespace MyPetClinic.Tests.Services
{
    public class AuthServiceGoogleTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly Mock<IGoogleTokenValidator> _googleValidatorMock;
        private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;

        public AuthServiceGoogleTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _googleValidatorMock = new Mock<IGoogleTokenValidator>();
            _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();
        }

        [Fact]
        public async Task LoginWithGoogleAsync_NewUser_ShouldRegisterNewUserAndIssueToken()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            
            // Giả lập Google trả về thông tin payload của một tài khoản mới tinh
            var googlePayload = new GoogleJsonWebSignature.Payload
            {
                Email = "google_new@gmail.com",
                Name = "Google New User"
            };
            
            _googleValidatorMock.Setup(v => v.ValidateTokenAsync(It.IsAny<string>()))
                                .ReturnsAsync(googlePayload);
                                
            _jwtGeneratorMock.Setup(g => g.GenerateToken(It.IsAny<User>()))
                             .Returns("my-custom-jwt-token");

            var authService = new AuthService(context, _jwtGeneratorMock.Object, _googleValidatorMock.Object);
            var request = new GoogleLoginRequest { IdToken = "valid-google-id-token" };

            // Act
            var result = await authService.LoginWithGoogleAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Email.Should().Be("google_new@gmail.com");

            // Xác minh tài khoản mới đã được tạo trong DB ở trạng thái kích hoạt sẵn
            var userInDb = await context.Users.FirstOrDefaultAsync(u => u.Email == "google_new@gmail.com");
            userInDb.Should().NotBeNull();
            userInDb!.FullName.Should().Be("Google New User");
            userInDb.IsActive.Should().BeTrue(); // Kích hoạt mặc định
        }
    }
}
```

---

## 3. Bộ Kịch bản Kiểm thử QA (QA Test Cases Suite)

### 🔴 Nhóm 1: Kiểm thử Nghiệp vụ Chính (Critical Paths)
*   **TC-GGL-001: Đăng nhập Google bằng tài khoản mới (Auto-Registration)**
    *   *Đầu vào:* Địa chỉ email Google chưa có trong hệ thống MyPetClinic.
    *   *Kỳ vọng:* Hệ thống tạo tài khoản mới, gán `IsActive = true`, cấp JWT Token hợp lệ, tự động điều hướng vào Customer Portal.
*   **TC-GGL-002: Đăng nhập Google liên kết tài khoản cũ (Auto-Link)**
    *   *Đầu vào:* Địa chỉ email Google đã có sẵn trong bảng `Users` của hệ thống.
    *   *Kỳ vọng:* Đăng nhập thành công, nhận token MyPetClinic, duy trì nguyên vẹn dữ liệu cũ.

### 🟠 Nhóm 2: Kiểm thử Giá trị Biên & Bảo mật (Boundary & Security)
*   **TC-GGL-003: Xác thực Token Google giả mạo**
    *   *Đầu vào:* Token Google bị thay đổi chữ ký số hoặc sai `Audience`.
    *   *Kỳ vọng:* API trả về lỗi `401 Unauthorized` với mã lỗi `GOOGLE_AUTH_FAILED`. Hệ thống không cấp token MyPetClinic.
*   **TC-GGL-004: Tắt Popup Google giữa chừng**
    *   *Hành động:* Click nút Google -> Đóng popup Google bằng dấu X.
    *   *Kỳ vọng:* Nút đăng nhập Google trên màn hình phục hồi trạng thái click bình thường, hiển thị thông báo toast báo hủy đăng nhập.
*   **TC-GGL-005: Spam gửi liên tiếp Token Google cũ**
    *   *Hành động:* Viết script gửi liên tiếp token Google đã hết hạn lên API.
    *   *Kỳ vọng:* API trả về lỗi `429 Too Many Requests` khi số lượng requests vượt hạn mức.
