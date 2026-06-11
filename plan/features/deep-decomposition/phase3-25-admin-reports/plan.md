# 📅 Implementation Plan & Test Strategy - Admin Revenue Reports

Tài liệu kế hoạch triển khai chi tiết (Micro-roadmap) và bộ kịch bản kiểm thử (Test Strategy) dành cho phân hệ Báo cáo Doanh thu & Hiệu suất.

---

## 1. Lộ trình Triển khai Chi tiết (4-Phase Micro-Roadmap)

### Giai đoạn 1: Thiết kế Database & Index (Tuần 1)
- Nghiên cứu và tạo chỉ mục tổng hợp `idx_invoices_report_covering` trên bảng `Invoices` để tối ưu hóa truy vấn tính tổng.
- Thiết lập seed dữ liệu hóa đơn mẫu trong môi trường phát triển (Development) để phục vụ kiểm thử đồ họa.

### Giai đoạn 2: Phát triển Backend API & Caching (Tuần 2)
- Cài đặt lớp `ReportService` và các API endpoints trả về dữ liệu KPIs, Xu hướng, Cơ cấu.
- Tích hợp bộ đệm RAM ngắn hạn `IMemoryCache` (15 phút) tại Controller để giảm tải Database.
- Thiết lập Rate Limiting và phân quyền truy cập Roles.
- Phát triển API xuất file Excel sử dụng thư viện **ClosedXML** hoặc **EPPlus**.

### Giai đoạn 3: Phát triển Giao diện Vue 3 & Biểu đồ (Tuần 3)
- Cài đặt thư viện **Chart.js** và **vue-chartjs** trên SPA Client.
- Dựng Pinia Store `useReportStore.ts` quản lý dữ liệu và lọc ngày.
- Thiết kế component biểu đồ Line Chart và Donut Chart phong cách Glassmorphism mờ kính.
- Thêm hiệu ứng Skeleton loading và Tooltips thông tin chi tiết.

### Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 4)
- Viết unit tests kiểm tra thuật toán tính toán tăng trưởng tài chính và Average Order Value.
- Kiểm thử thâm nhập (Penetration Test) xác nhận vai trò Doctor/Receptionist bị chặn 100%.
- Kiểm tra tính đúng đắn khi xuất file Excel trên môi trường thực tế.

---

## 2. Kịch bản Kiểm thử QA (QA Test Cases)

| Mã Test Case | Phân loại | Mục tiêu kiểm thử | Các bước thực hiện | Kết quả mong đợi |
| :--- | :--- | :--- | :--- | :--- |
| **TC-REP-01** | Unit Test | Kiểm tra thuật toán tính tỷ lệ tăng trưởng | Gọi hàm tính tăng trưởng với Doanh thu kỳ này = 150 triệu, kỳ trước = 100 triệu. | Tỷ lệ tăng trưởng trả về đúng bằng `+50.0%`. |
| **TC-REP-02** | Boundary | Kiểm tra tăng trưởng khi kỳ trước bằng 0 | Gọi hàm tính tăng trưởng với Doanh thu kỳ này = 50 triệu, kỳ trước = 0đ. | Hệ thống gán tỷ lệ mặc định bằng `+100.0%`. |
| **TC-REP-03** | Security | Chặn truy cập dữ liệu báo cáo tài chính | Đăng nhập tài khoản `receptionist` hoặc `customer`, gọi API `GET /api/admin/reports/kpis`. | Hệ thống từ chối truy cập, trả về mã lỗi `403 Forbidden`. |
| **TC-REP-04** | Boundary | Chặn khoảng lọc ngày không hợp lệ | Chọn ngày bắt đầu là `2026-06-01` và ngày kết thúc là `2026-05-01` (Ngày kết thúc nhỏ hơn). | Hệ thống từ chối lọc, báo lỗi `400 Bad Request`. |

---

## 3. Mã nguồn Unit Test C# xUnit mẫu

Dưới đây là mã nguồn unit test sử dụng **xUnit** và **Category** kiểm định tính đúng đắn của logic tính toán tăng trưởng tài chính:

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
    public class ReportServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ReportService _reportService;

        public ReportServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _reportService = new ReportService(_context, NullLogger<ReportService>.Instance);
        }

        [Fact]
        public async Task GetKpiReport_ShouldCalculateCorrectGrowthRate_WhenComparingToPreviousPeriod()
        {
            // Arrange
            // Thiết lập ngày lọc: 10/06/2026 đến 12/06/2026 (3 ngày)
            // Kỳ trước tương đương: 07/06/2026 đến 09/06/2026 (3 ngày)
            var startDate = new DateTime(2026, 06, 10);
            var endDate = new DateTime(2026, 06, 12);

            // Ca 1 (Kỳ trước): Thu về 100,000 đ
            var pet = new Pet { Id = Guid.NewGuid(), Name = "Miu", Species = "Cat" };
            var app1 = new Appointment { Id = Guid.NewGuid(), Status = "Completed", Pet = pet };
            var invoice1 = new Invoice
            {
                Id = Guid.NewGuid(),
                Appointment = app1,
                InvoiceNumber = "INV-0001",
                TotalAmount = 100000m,
                Status = "Paid",
                PaidAt = new DateTime(2026, 06, 08) // Nằm trong kỳ trước
            };

            // Ca 2 (Kỳ này): Thu về 150,000 đ
            var app2 = new Appointment { Id = Guid.NewGuid(), Status = "Completed", Pet = pet };
            var invoice2 = new Invoice
            {
                Id = Guid.NewGuid(),
                Appointment = app2,
                InvoiceNumber = "INV-0002",
                TotalAmount = 150000m,
                Status = "Paid",
                PaidAt = new DateTime(2026, 06, 11) // Nằm trong kỳ này
            };

            _context.Invoices.AddRange(invoice1, invoice2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _reportService.GetKpiReportAsync(startDate, endDate);

            // Assert
            // Doanh thu kỳ này: 150k
            // Doanh thu kỳ trước: 100k
            // Tỷ lệ tăng trưởng: ((150 - 100) / 100) * 100 = +50%
            result.Should().NotBeNull();
            result.TotalRevenue.Should().Be(150000m);
            result.GrowthRate.Should().Be(50.0);
            result.AverageOrderValue.Should().Be(150000m);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
```
