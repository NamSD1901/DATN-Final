# 🧠 Core Business Logic - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Tính toán tỷ lệ tăng trưởng theo công thức phần trăm so sánh doanh thu 2 kỳ.
- **BE-F03 (Async/Await):** Sử dụng `Task.WhenAll` để chạy song song nhiều truy vấn tổng hợp.

---

## 1. C# Logic: Revenue Report Service

```csharp
public class RevenueReportService
{
    private readonly MyPetClinicDbContext _context;

    public RevenueReportService(MyPetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueDashboardDto> GetDashboardAsync(DateTime start, DateTime end)
    {
        var paidInvoicesTask = _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt.Date >= start.Date && i.CreatedAt.Date <= end.Date)
            .ToListAsync();

        var appointmentsCountTask = _context.Appointments
            .Where(a => a.AppointmentDate.Date >= start.Date && a.AppointmentDate.Date <= end.Date)
            .CountAsync();

        // Cùng kỳ tháng trước để tính tăng trưởng
        var range = (end - start).Days;
        var prevStart = start.AddDays(-(range + 1));
        var prevEnd = start.AddDays(-1);
        var prevRevenueTask = _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.CreatedAt.Date >= prevStart.Date && i.CreatedAt.Date <= prevEnd.Date)
            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

        await Task.WhenAll(paidInvoicesTask, appointmentsCountTask);
        var invoices = paidInvoicesTask.Result;
        var prevRevenue = await prevRevenueTask;

        decimal totalRevenue = invoices.Sum(i => i.TotalAmount);
        decimal growthRate = prevRevenue > 0 ? ((totalRevenue - prevRevenue) / prevRevenue) * 100 : 0;

        var dailyChart = invoices
            .GroupBy(i => i.CreatedAt.Date)
            .Select(g => new DailyRevenueDto { Date = g.Key.ToString("yyyy-MM-dd"), Revenue = g.Sum(i => i.TotalAmount) })
            .OrderBy(d => d.Date)
            .ToList();

        var topServices = await _context.InvoiceItems
            .GroupBy(ii => ii.ItemName)
            .Select(g => new TopItemDto { Name = g.Key, Count = g.Sum(ii => ii.Quantity), Value = g.Sum(ii => ii.TotalPrice) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync();

        return new RevenueDashboardDto
        {
            TotalRevenue = totalRevenue,
            GrowthRate = Math.Round(growthRate, 2),
            TotalAppointments = appointmentsCountTask.Result,
            ServiceRevenue = invoices.Sum(i => i.ServiceAmount),
            MedicineRevenue = invoices.Sum(i => i.MedicineAmount),
            DailyChart = dailyChart,
            TopServices = topServices
        };
    }
}
```
