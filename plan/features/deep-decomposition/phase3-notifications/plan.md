# 📅 Implementation Plan & Test Strategy - Notifications

Tài liệu kế hoạch triển khai chi tiết (Micro-roadmap) và bộ kịch bản kiểm thử (Test Strategy) dành cho phân hệ Nhắc lịch tiêm phòng & Thông báo tự động.

---

## 1. Lộ trình Triển khai Chi tiết (4-Phase Micro-Roadmap)

### Giai đoạn 1: Database Setup & SignalR Core (Tuần 1)
- Xây dựng thực thể `Notification` trong Domain Layer.
- Tạo migrations CSDL PostgreSQL tạo bảng `Notifications` và thiết lập các index hỗ trợ tìm kiếm tin chưa đọc.
- Triển khai lớp Hub SignalR `NotificationHub` ở Backend.

### Giai đoạn 2: Cài đặt Quartz.NET & MailKit SMTP (Tuần 2)
- Tích hợp và cấu hình thư viện **Quartz.NET** Hosted Service chạy nền hàng ngày.
- Viết logic lớp Job `VaccinationReminderJob` tìm kiếm thú cưng cận hạn tái chủng 3 hoặc 5 ngày.
- Thiết lập email template HTML nhắc tái chủng.
- Tích hợp MailKit/SMTP gọi dịch vụ STARTTLS gửi email tự động.

### Giai đoạn 3: Phát triển Giao diện Client (Vue 3 SignalR) (Tuần 3)
- Cài đặt thư viện `@microsoft/signalr` trên Vue 3 SPA Client.
- Dựng Pinia Store `useNotificationStore.ts` quản lý WebSockets connection.
- Thiết kế component Icon Chuông thông báo mờ kính và menu thả dropdown hiển thị tin.
- Thiết lập hoạt ảnh chuông rung lắc (Bell Shake) và Toast thông báo realtime.

### Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 4)
- Viết unit tests kiểm tra Quartz Job quét đúng ngày tái chủng dự kiến.
- Viết integration tests kiểm tra SignalR đẩy tin đúng tài khoản UserId (không rò rỉ IDOR).
- Tối ưu hóa hiệu năng và thiết lập chính sách dọn dẹp tin cũ quá 30 ngày.

---

## 2. Kịch bản Kiểm thử QA (QA Test Cases)

| Mã Test Case | Phân loại | Mục tiêu kiểm thử | Các bước thực hiện | Kết quả mong đợi |
| :--- | :--- | :--- | :--- | :--- |
| **TC-NTF-01** | Unit Test | Kiểm tra ngày tái chủng dự kiến của Quartz Job | Có bản ghi NextDoseDate cách ngày hiện tại đúng 3 ngày. Chạy thủ công Job quét. | Hệ thống tìm ra bản ghi, sinh 1 bản ghi email gửi đi và 1 bản ghi notification lưu DB. |
| **TC-NTF-02** | Security | Chống tấn công IDOR xem thông báo người khác | Đăng nhập tài khoản Customer A, gọi API `PUT /api/customer/notifications/{id}/read` với ID thông báo của Customer B. | Backend chặn đứng giao dịch và trả về mã lỗi `403 Forbidden`. |
| **TC-NTF-03** | Integration | Kiểm tra SignalR đẩy thông báo thời gian thực | Mở 2 trình duyệt đăng nhập Customer A và B. Lễ tân phê duyệt lịch hẹn khám của A. | Trình duyệt A lập tức nhận thông báo in-app (chuông rung). Trình duyệt B hoàn toàn không bị ảnh hưởng. |
| **TC-NTF-04** | Boundary | Kiểm tra giới hạn 50 tin nhắn hộp thư | Gửi liên tiếp 51 thông báo in-app đến cùng một tài khoản người dùng. | Bản ghi thông báo cũ nhất (thứ 1) bị tự động xóa khỏi Database. Số lượng tin trong DB giữ nguyên = 50. |

---

## 3. Mã nguồn Unit Test C# xUnit mẫu

Dưới đây là mã nguồn unit test sử dụng **xUnit** và **FluentAssertions** kiểm định tính đúng đắn của tiến trình nền Quartz Job trong việc quét tìm đúng ngày tái chủng:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyPetClinic.Application.Jobs;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;
using Quartz;
using Xunit;

namespace MyPetClinic.Tests
{
    public class VaccinationReminderJobTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly MockEmailService _mockEmailService;
        private readonly VaccinationReminderJob _job;

        public VaccinationReminderJobTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockEmailService = new MockEmailService();
            _job = new VaccinationReminderJob(_context, _mockEmailService, NullLogger<VaccinationReminderJob>.Instance);
        }

        [Fact]
        public async Task Execute_ShouldFindAndNotifyPets_WhenNextDoseDateIsExactly3DaysFromToday()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var targetNextDoseDate = today.AddDays(3); // Cách đúng 3 ngày

            var ownerId = Guid.NewGuid();
            var owner = new User
            {
                Id = ownerId,
                FullName = "Nguyễn Văn Nam",
                Email = "nam.nv@gmail.com",
                Role = "customer"
            };

            var pet = new Pet
            {
                Id = Guid.NewGuid(),
                Name = "LuLu",
                OwnerId = ownerId,
                Owner = owner
            };

            var record = new VaccinationRecord
            {
                Id = Guid.NewGuid(),
                PetId = pet.Id,
                Pet = pet,
                VaccineName = "Vắc-xin Dại",
                VaccinationDate = today.AddMonths(-11),
                NextDoseDate = targetNextDoseDate // Đặt ngày tái chủng đích
            };

            _context.Users.Add(owner);
            _context.Pets.Add(pet);
            _context.VaccinationRecords.Add(record);
            await _context.SaveChangesAsync();

            // Khởi tạo mock context cho Quartz
            var mockJobContext = new MockJobExecutionContext();

            // Act: Thực thi chạy ngầm Job quét lịch
            await _job.Execute(mockJobContext);

            // Assert
            // 1. Dịch vụ email giả lập phải nhận được yêu cầu gửi 1 bức thư
            _mockEmailService.SentEmails.Should().HaveCount(1);
            _mockEmailService.SentEmails[0].ToEmail.Should().Be("nam.nv@gmail.com");
            _mockEmailService.SentEmails[0].Subject.Should().Contain("LuLu");

            // 2. Hệ thống phải tự động lưu 1 thông báo in-app vào DB cho chủ nuôi
            var savedNotifications = await _context.Notifications.ToListAsync();
            savedNotifications.Should().HaveCount(1);
            savedNotifications[0].UserId.Should().Be(ownerId);
            savedNotifications[0].Title.Should().Contain("LuLu");
            savedNotifications[0].IsRead.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }

    // Lớp giả lập Email Service phục vụ Testing và kiểm đếm thư gửi đi
    public class MockEmailService : IEmailService
    {
        public List<(string ToEmail, string Subject, string Body)> SentEmails { get; } = new();

        public Task SendEmailAsync(string email, string subject, string body)
        {
            SentEmails.Add((email, subject, body));
            return Task.CompletedTask;
        }
    }

    // Mock Job Execution Context của Quartz phục vụ chạy test
    public class MockJobExecutionContext : IJobExecutionContext
    {
        public IScheduler Scheduler => null!;
        public ITrigger Trigger => null!;
        public ICalendar Calendar => null!;
        public IJobDetail JobDetail => null!;
        public IJob JobInstance => null!;
        public TimeSpan JobRunTime => TimeSpan.Zero;
        public DateTimeOffset? FireTimeUtc => DateTimeOffset.UtcNow;
        public DateTimeOffset? ScheduledFireTimeUtc => DateTimeOffset.UtcNow;
        public DateTimeOffset? PreviousFireTimeUtc => null;
        public DateTimeOffset? NextFireTimeUtc => null;
        public int RefireCount => 0;
        public JobDataMap MergedJobDataMap => null!;
        public object? Result { get; set; }
        public void Put(object key, object value) { }
        public object Get(object key) => null!;
    }
}
```
