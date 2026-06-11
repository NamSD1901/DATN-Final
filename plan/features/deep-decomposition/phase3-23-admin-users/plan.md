# 📅 Implementation Plan & Test Strategy - Admin Staff Management

Tài liệu kế hoạch triển khai chi tiết (Micro-roadmap) và bộ kịch bản kiểm thử (Test Strategy) dành cho phân hệ Quản trị Nhân sự & Phân quyền Admin.

---

## 1. Lộ trình Triển khai Chi tiết (4-Phase Micro-Roadmap)

### Giai đoạn 1: Thiết kế Database & Domain Entities (Tuần 1)
- Thiết lập bảng `AuditLogs` trong CSDL PostgreSQL.
- Mở rộng các trường `Status`, `Role`, `RequirePasswordChange` trong thực thể `User`.
- Viết migrations và thiết lập database index cho trường `Role` và `Status`.

### Giai đoạn 2: Phát triển Backend API & Core Security (Tuần 2)
- Viết logic sinh mật khẩu ngẫu nhiên an toàn mật mã tại `PasswordGenerator`.
- Cài đặt lớp `AdminStaffService` quản lý CRUD nhân sự, chặn tự khóa tài khoản Admin.
- Xây dựng Middleware ghi nhật ký Audit Log bảo mật.
- Đăng ký chính sách phân quyền vai trò `[Authorize(Roles = "admin")]`.

### Giai đoạn 3: Phát triển Giao diện Web (Vue 3 Client) (Tuần 3)
- Viết Pinia Store `useAdminStaffStore.ts` quản lý mảng nhân viên và lọc dữ liệu.
- Thiết kế giao diện Quản trị nhân viên mờ kính Glassmorphism.
- Tích hợp popup Thêm nhân viên và Dialog xác nhận khóa tài khoản.
- Cài đặt Router Guard chặn truy cập trái phép trên Client.

### Giai đoạn 4: Kiểm thử, Audit & Bàn giao (Tuần 4)
- Viết unit tests kiểm tra logic băm mật khẩu BCrypt và sinh mật khẩu ngẫu nhiên.
- Thực hiện kiểm thử thâm nhập (Penetration Test) kiểm tra lỗi Privilege Escalation.
- Tối ưu hóa hiệu năng phản hồi API khi tải danh sách Audit Log.

---

## 2. Kịch bản Kiểm thử QA (QA Test Cases)

| Mã Test Case | Phân loại | Mục tiêu kiểm thử | Các bước thực hiện | Kết quả mong đợi |
| :--- | :--- | :--- | :--- | :--- |
| **TC-ADM-01** | Security | Chống leo thang đặc quyền (Privilege Escalation) | Dùng token JWT của vai trò `doctor` để gọi API `PUT /api/admin/staff/{id}/role` đổi quyền thành `admin`. | Backend trả về mã lỗi `403 Forbidden`, giao dịch bị từ chối. |
| **TC-ADM-02** | Boundary | Chặn Admin tự thay đổi vai trò của chính mình | Admin đăng nhập gọi API đổi quyền của chính ID mình sang `receptionist`. | Hệ thống từ chối đổi quyền, trả về lỗi `400 Bad Request`. |
| **TC-ADM-03** | Boundary | Chặn Admin tự khóa tài khoản của chính mình | Admin gọi API toggle-status lên ID tài khoản của chính mình. | Hệ thống từ chối khóa, trả về lỗi `400 Bad Request`. |
| **TC-ADM-04** | Security | Bắt buộc đổi mật khẩu ở lần đăng nhập đầu tiên | Tạo nhân viên mới -> Đăng nhập bằng pass tạm -> Gửi request lấy danh sách thú cưng. | Hệ thống chặn request, trả về mã lỗi yêu cầu đổi mật khẩu. |

---

## 3. Mã nguồn Unit Test C# xUnit mẫu

Dưới đây là mã nguồn unit test sử dụng **xUnit** và **FluentAssertions** kiểm định tính đúng đắn của logic quản trị nhân sự:

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;
using Xunit;

namespace MyPetClinic.Tests
{
    public class AdminStaffServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AdminStaffService _adminStaffService;

        public AdminStaffServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            // Giả lập EmailService trống
            var mockEmailService = new MockEmailService();
            _adminStaffService = new AdminStaffService(_context, NullLogger<AdminStaffService>.Instance, mockEmailService);
        }

        [Fact]
        public async Task ChangeStaffRole_ShouldFail_WhenAdminTriesToChangeOwnRole()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var adminUser = new User
            {
                Id = adminId,
                FullName = "Admin Root",
                Email = "admin.root@mypet.vn",
                Role = "admin",
                Status = "Active"
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            var request = new ChangeRoleRequest { NewRole = "doctor" };

            // Act
            Func<Task> act = async () => await _adminStaffService.ChangeStaffRoleAsync(adminId, request, adminId, "127.0.0.1");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Không thể tự thay đổi vai trò của chính mình.");
        }

        [Fact]
        public async Task ToggleStaffStatus_ShouldFail_WhenAdminTriesToSuspendSelf()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var adminUser = new User
            {
                Id = adminId,
                FullName = "Admin Root",
                Email = "admin.root@mypet.vn",
                Role = "admin",
                Status = "Active"
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _adminStaffService.ToggleStaffStatusAsync(adminId, adminId, "127.0.0.1");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Không thể tự khóa tài khoản của chính mình.");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    // Lớp giả lập Email Service phục vụ Testing
    public class MockEmailService : IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string body)
        {
            // Không làm gì, giả lập gửi email thành công
            return Task.CompletedTask;
        }
    }
}
```
