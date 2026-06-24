# Tài liệu Kế hoạch Triển khai chi tiết & Test Strategy - Module Hồ Sơ Khách Hàng & Thú Cưng

Tài liệu kế hoạch triển khai chi tiết (Micro-roadmap) và bộ kịch bản kiểm thử (Test Strategy) dành cho phân hệ Quản lý Hồ sơ Khách hàng & Thú cưng.

---

## 1. Lộ trình Triển khai Chi tiết (4-Phase Micro-Roadmap)

### Giai đoạn 1: Tái cấu trúc Cố sở dữ liệu & C# Entities (Tuần 1)
- Xây dựng thực thể `Customer` trong Domain Layer.
- Thiết lập cấu hình EF Core Fluent API: Chuyển hướng Khóa ngoại của `Pet.OwnerId` và `Appointment.CustomerId` sang bảng `Customer`.
- Áp dụng chiến lược **Đồng bộ ID** (Customer.Id = User.Id) để đảm bảo không đứt gãy quan hệ dữ liệu cũ.
- Chạy Database Migrations và chèn Raw SQL để copy dữ liệu từ bảng `Users`.

### Giai đoạn 2: Phát triển Backend API & Logic Đồng bộ (Tuần 2)
- Phát triển `CustomerService` với các hàm Tìm kiếm, Tạo mới.
- Viết thuật toán Auto-Link (Liên kết tài khoản): Xác thực OTP và tự động map `UserId` mới với `CustomerId` đã có của khách Walk-in.
- Phát triển `ReceptionistCustomerController` dành riêng cho Lễ tân.
- Thiết lập phân quyền truy cập (Role-based Access Control).

### Giai đoạn 3: Phát triển Giao diện Vue 3 Client (Tuần 3)
- Viết Pinia Store `useCustomerStore.ts` để quản lý cache dữ liệu trên Client.
- Dựng giao diện `CustomerList.vue` dạng DataGrid chuyên nghiệp với tính năng tìm kiếm Realtime.
- Thiết kế màn hình chi tiết gồm 7 Tabs (Thông tin, Thú cưng, Lịch khám, Tiêm chủng...).
- Tích hợp biểu đồ Chart.js để vẽ đồ thị theo dõi cân nặng Thú cưng.

### Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 4)
- Viết Unit Tests kiểm tra tính toàn vẹn của logic Liên kết tài khoản (Auto-link).
- Viết Integration Tests kiểm tra lỗi Constraint khi thêm Pet không có Chủ.
- Tối ưu hóa truy vấn LINQ để giảm tải cho DB khi Load danh sách khách hàng.

---

## 2. Kịch bản Kiểm thử QA (QA Test Cases)

| Mã Test Case | Phân loại | Mục tiêu kiểm thử | Các bước thực hiện | Kết quả mong đợi |
| :--- | :--- | :--- | :--- | :--- |
| **TC-CUS-01** | Unit Test | Kiểm tra Auto-link OTP | Nhập SĐT đã có ở bảng `Customers`. Gửi OTP chuẩn xác. | Hệ thống tạo `Users` mới, cập nhật `HasAccount=True` ở Customer cũ. Trả về Token. |
| **TC-CUS-02** | Boundary | Chặn trùng lặp SĐT Walk-in | Lễ tân tạo Customer với SĐT `0912345678` đã tồn tại. | Trả về HTTP 400 `SĐT đã tồn tại`. |
| **TC-CUS-03** | Security | Chống IDOR (Truy cập trái phép) | `User_A` gọi API `/api/pets/{PetId_Cua_User_B}` để xem bệnh án. | Hệ thống chặn quyền, trả về HTTP 403 Forbidden. |
| **TC-CUS-04** | Concurrency | Xóa Pet (Deceased) khi đang có lịch hẹn | Chuyển `Status` của Pet sang `Deceased` trong khi Pet này đang có lịch khám ngày mai. | Trạng thái Pet = Deceased. Lịch hẹn ngày mai bị Cancel tự động. |

---

## 3. Mã nguồn Unit Test C# xUnit mẫu

Dưới đây là mã nguồn unit test sử dụng **xUnit** và **FluentAssertions** kiểm định tính đúng đắn của logic Liên kết Tài khoản (Auto-link OTP):

```csharp
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;
using Xunit;

namespace MyPetClinic.Tests
{
    public class CustomerServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _customerService = new CustomerService(_context);
        }

        [Fact]
        public async Task AutoLinkAccount_ShouldCreateUserAndLinkToCustomer_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var phoneNumber = "0901234567";

            // Tạo sẵn 1 khách Walk-in trong hệ thống
            var existingCustomer = new Customer
            {
                Id = customerId,
                FullName = "Nguyen Van Walkin",
                PhoneNumber = phoneNumber,
                HasAccount = false
            };
            _context.Customers.Add(existingCustomer);
            await _context.SaveChangesAsync();

            var registerDto = new RegisterDto
            {
                PhoneNumber = phoneNumber,
                Password = "hashedpassword",
                OtpCode = "123456" // Giả lập OTP đúng
            };

            // Act: Khách tải App và đăng ký bằng SĐT này
            var newUserId = await _customerService.AutoLinkWalkinCustomerAsync(registerDto);

            // Assert
            var updatedCustomer = await _context.Customers.FindAsync(customerId);
            updatedCustomer!.HasAccount.Should().BeTrue();

            var newUser = await _context.Users.FindAsync(newUserId);
            newUser.Should().NotBeNull();
            newUser!.CustomerId.Should().Be(customerId);
            newUser.PhoneNumber.Should().Be(phoneNumber);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
```
