# 📝 Implementation Plan & Testing Strategy - Login System (Phase 1)

Tài liệu này đặc tả lộ trình thực thi từng giai đoạn phát triển và bộ kịch bản kiểm thử (Test Suite) bao phủ toàn diện cho tính năng Đăng nhập hệ thống.

---

## 1. Lộ trình Triển khai Chi tiết (Implementation Phases)

| Giai đoạn | Công việc Cụ thể (Tasks) | Tệp tin Tác động | Kỹ năng Kiểm soát |
| :--- | :--- | :--- | :--- |
| **Phase 1: Security Config** | 1. Thêm cấu hình `JwtSettings` vào file JSON cấu hình. <br>2. Cài đặt JWT Bearer Middleware ở `Program.cs`. | `WebApi/appsettings.json`<br>`WebApi/Program.cs` | **BE-A03 (JWT Auth)** |
| **Phase 2: Database Update** | 1. Tạo migration bổ sung `AccessFailedCount` và `LockoutEnd` vào DB. <br>2. Cập nhật Entity `User` tương ứng. | `Domain/Entities/User.cs`<br>`Infrastructure/Data/` | **BE-C02 (EF Core)** |
| **Phase 3: Core Service Logic** | 1. Viết `JwtTokenGenerator` ký số HMAC-SHA256. <br>2. Cập nhật `AuthService.LoginAsync` kiểm soát BCrypt mật khẩu & khóa lockout. | `Infrastructure/Authentication/`<br>`Application/Services/AuthService.cs` | **BE-F02 (SOLID)**<br>**BE-A03 (Security)** |
| **Phase 4: Web API Endpoints** | 1. Tạo endpoint `POST /api/account/login`. <br>2. Cấu hình kiểm duyệt validation đầu vào bằng DTO. | `WebApi/Controllers/AccountController.cs` | **BE-C01 (WebAPI)** |
| **Phase 5: Frontend Pinia Store** | 1. Triển khai Pinia store `useAuthStore.ts` quản lý phiên đăng nhập. <br>2. Đăng ký tự động Axios Authorization headers. | `frontend/src/store/useAuthStore.ts` | **FE-C03 (Pinia)** |
| **Phase 6: UI Component & Route** | 1. Thiết kế component biểu mẫu đăng nhập `LoginTab.vue`. <br>2. Cấu hình Router Guards chuyển hướng tự động theo Role. | `frontend/src/components/auth/`<br>`frontend/src/router/` | **FE-C01 (Vue 3)** |

---

## 2. Chiến lược Kiểm thử tự động (Testing Strategy)

Chúng ta xây dựng bộ kiểm thử tích hợp (Integration Tests) để xác minh tính đúng đắn của API đăng nhập.

### 2.1. Bộ Kiểm thử Tích hợp Web API (Integration Tests với xUnit & FluentAssertions)
Tệp tin: `tests/MyPetClinic.Tests/Controllers/AccountControllerTests.cs`

```csharp
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MyPetClinic.Application.DTOs;
using Xunit;

namespace MyPetClinic.Tests.Controllers
{
    public class AccountControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AccountControllerTests(WebApplicationFactory<Program> factory)
        {
            // Khởi tạo máy chủ Web giả lập phục vụ kiểm thử tích hợp liên thông API
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturn41Unauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            
            var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorDto>();
            errorResult.Should().NotBeNull();
            errorResult!.ErrorType.Should().Be("INVALID_CREDENTIALS");
        }
    }

    public class ApiErrorDto
    {
        public bool Success { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
```

---

## 3. Bộ Kịch bản Kiểm thử QA (QA Test Cases Suite)

### 🔴 Nhóm 1: Kiểm thử Nghiệp vụ Chính (Critical Paths)
*   **TC-LGN-001: Đăng nhập thành công (Happy Path)**
    *   *Đầu vào:* Email đúng (`doctor.minh@mypetclinic.com`), Mật khẩu đúng.
    *   *Kỳ vọng:* API trả về `200 OK`, nhận chuỗi JWT Token hợp lệ, thông tin role là `BacSi`. Router Guard điều hướng chính xác vào `/portal/doctor`.
*   **TC-LGN-002: Đăng nhập thất bại do sai mật khẩu**
    *   *Đầu vào:* Email đúng, Mật khẩu sai.
    *   *Kỳ vọng:* API trả về `401 Unauthorized`, hiển thị lỗi `"Tài khoản hoặc mật khẩu không chính xác"`. Form thực hiện hiệu ứng rung lắc.
*   **TC-LGN-003: Đăng nhập tài khoản chưa kích hoạt**
    *   *Đầu vào:* Email của tài khoản chưa xác thực OTP.
    *   *Kỳ vọng:* API trả về `422 Unprocessable Entity` với mã lỗi `ACCOUNT_INACTIVE`. Frontend tự động chuyển hướng sang tab OTP.

### 🟠 Nhóm 2: Kiểm thử Giá trị Biên & Bảo mật (Boundary & Security)
*   **TC-LGN-004: Khóa tài khoản sau 5 lần nhập sai liên tục (Lockout Flow)**
    *   *Hành động:* Nhập sai mật khẩu liên tục 5 lần cho cùng 1 email.
    *   *Kỳ vọng:* Ở lần thứ 5, API trả về `422 Unprocessable Entity` với mã lỗi `ACCOUNT_LOCKED`. Hệ thống khóa tài khoản 15 phút. Request thứ 6 lập tức bị từ chối với thông báo khóa.
*   **TC-LGN-005: Kiểm duyệt Định dạng Đầu vào (Validation Check)**
    *   *Đầu vào:* Nhập email trống hoặc sai cấu trúc (ví dụ: `invalidemail.com`).
    *   *Kỳ vọng:* Hệ thống báo đỏ ngay lập tức tại Client, khóa nút Đăng nhập và không gửi request lên API.
