# 01. Core Business Logic Reference - Admin Revenue Reports

Tài liệu đặc tả thuật toán tính tăng trưởng tài chính phòng khám và mã nguồn C# Service xử lý truy vấn gom nhóm hiệu năng cao.

---

## 1. Thuật toán Tính toán Tỷ lệ Tăng trưởng (Growth Rate Formula)

Tỷ lệ tăng trưởng doanh thu là chỉ số quan trọng giúp Admin biết tình hình kinh doanh của phòng khám đang phát triển tốt lên hay đi xuống. 

### Công thức toán học:
$$\text{GrowthRate} = \left( \frac{\text{CurrentRevenue} - \text{PreviousRevenue}}{\text{PreviousRevenue}} \right) \times 100$$
*(Trong đó $\text{PreviousRevenue}$ là doanh thu của khoảng thời gian tương đương ở kỳ trước).*

### Edge Cases (Trường hợp biên):
- Nếu $\text{PreviousRevenue} = 0$ và $\text{CurrentRevenue} > 0$: Tỷ lệ tăng trưởng gán mặc định bằng **`100.0%`**.
- Nếu $\text{PreviousRevenue} = 0$ và $\text{CurrentRevenue} = 0$: Tỷ lệ tăng trưởng bằng **`0.0%`**.

### C# Helper Logic:
```csharp
public static double CalculateGrowthRate(decimal current, decimal previous)
{
    if (previous == 0)
    {
        return current > 0 ? 100.0 : 0.0;
    }
    
    decimal diff = current - previous;
    decimal rate = (diff / previous) * 100;
    return (double)Math.Round(rate, 2); // Làm tròn 2 chữ số thập phân
}
```

---

## 2. Mã nguồn C# Service Thực thi Logic Báo cáo

Dưới đây là cài đặt lớp `ReportService` thuộc tầng `Application`, tối ưu hóa LINQ để gom nhóm và tính tổng tiền trực tiếp dưới Database Server.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.DTOs.Report;
using MyPetClinic.Infrastructure.Data;

namespace MyPetClinic.Application.Services
{
    public interface IReportService
    {
        Task<KpiReportDto> GetKpiReportAsync(DateTime startDate, DateTime endDate);
        Task<RevenueTrendDto> GetRevenueTrendAsync(DateTime startDate, DateTime endDate);
        Task<RevenueStructureDto> GetRevenueStructureAsync(DateTime startDate, DateTime endDate);
    }

    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(AppDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<KpiReportDto> GetKpiReportAsync(DateTime startDate, DateTime endDate)
        {
            // 1. Xác định kỳ trước tương đương để so sánh
            var duration = endDate - startDate;
            var prevStartDate = startDate.AddDays(-duration.Days - 1);
            var prevEndDate = startDate.AddDays(-1);

            // 2. Doanh thu kỳ này (Chỉ tính hóa đơn ở trạng thái Paid)
            var currentRevenue = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.PaidAt >= startDate && i.PaidAt <= endDate)
                .SumAsync(i => i.TotalAmount);

            // 3. Doanh thu kỳ trước
            var previousRevenue = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.PaidAt >= prevStartDate && i.PaidAt <= prevEndDate)
                .SumAsync(i => i.TotalAmount);

            // 4. Đếm số ca lịch hẹn thành công kỳ này
            var currentAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Completed" && a.UpdatedAt >= startDate && a.UpdatedAt <= endDate);

            // 5. Đếm số khách hàng hoạt động (Có phát sinh lịch hẹn khám thành công)
            var activeCustomers = await _context.Appointments
                .Where(a => a.Status == "Completed" && a.UpdatedAt >= startDate && a.UpdatedAt <= endDate)
                .Select(a => a.Pet.OwnerId)
                .Distinct()
                .CountAsync();

            // 6. Tính toán giá trị trung bình trên mỗi hóa đơn
            var invoiceCount = await _context.Invoices
                .CountAsync(i => i.Status == "Paid" && i.PaidAt >= startDate && i.PaidAt <= endDate);
            
            decimal averageOrderValue = invoiceCount > 0 ? currentRevenue / invoiceCount : 0;

            // 7. Tính phần trăm tăng trưởng
            double growth = CalculateGrowthRate(currentRevenue, previousRevenue);

            return new KpiReportDto
            {
                TotalRevenue = currentRevenue,
                GrowthRate = growth,
                TotalAppointments = currentAppointments,
                ActiveCustomersCount = activeCustomers,
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<RevenueTrendDto> GetRevenueTrendAsync(DateTime startDate, DateTime endDate)
        {
            // Gom nhóm doanh thu theo ngày và thực hiện tính tổng dưới DB
            var rawData = await _context.Invoices
                .Where(i => i.Status == "Paid" && i.PaidAt >= startDate && i.PaidAt <= endDate)
                .GroupBy(i => i.PaidAt!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(i => i.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .AsNoTracking()
                .ToListAsync();

            var trend = new RevenueTrendDto();
            
            // Đổ dữ liệu vào DTO phục vụ cho Chart.js Labels và Datasets
            foreach (var item in rawData)
            {
                trend.Labels.Add(item.Date.ToString("yyyy-MM-dd"));
                trend.DataPoints.Add(item.Revenue);
                trend.InvoiceCounts.Add(item.Count);
            }

            return trend;
        }

        public async Task<RevenueStructureDto> GetRevenueStructureAsync(DateTime startDate, DateTime endDate)
        {
            // Gom nhóm theo loại dòng tiền (Phí dịch vụ, Tiền thuốc, Vắc-xin)
            var itemsGrouped = await _context.InvoiceItems
                .Where(ii => ii.Invoice.Status == "Paid" && ii.Invoice.PaidAt >= startDate && ii.Invoice.PaidAt <= endDate)
                .GroupBy(ii => ii.ItemType)
                .Select(g => new
                {
                    ItemType = g.Key,
                    Total = g.Sum(ii => ii.SubTotal)
                })
                .AsNoTracking()
                .ToListAsync();

            var structure = new RevenueStructureDto();

            foreach (var group in itemsGrouped)
            {
                if (group.ItemType == "Service")
                {
                    structure.ServiceRevenue = group.Total;
                }
                else if (group.ItemType == "Medicine")
                {
                    structure.MedicineRevenue = group.Total;
                }
                else if (group.ItemType == "Vaccine")
                {
                    structure.VaccineRevenue = group.Total;
                }
            }

            return structure;
        }

        private double CalculateGrowthRate(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100.0 : 0.0;
            }

            decimal diff = current - previous;
            decimal rate = (diff / previous) * 100;
            return (double)Math.Round(rate, 2);
        }
    }
}
```
