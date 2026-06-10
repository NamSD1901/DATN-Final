# 🗄️ Infrastructure & Security - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **BE-A03 (Security & Identity):** Thiết lập phân quyền RBAC chặt chẽ (chỉ cho phép Role `Admin` truy cập dữ liệu tài chính nhạy cảm).
- **BE-C01 (EF Core & SQL Optimization):** Tối ưu hóa hiệu năng truy vấn báo cáo thông qua việc tạo index hợp lý trên bảng Invoices và Appointments để tránh Full Table Scan khi lượng dữ liệu lớn.

---

## 1. Security & Authorization

Để bảo mật các thông tin tài chính nhạy cảm của phòng khám, API Controller bắt buộc phải áp dụng thuộc tính phân quyền cấp lớp:

```csharp
[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/reports")]
public class AdminReportsController : ControllerBase
{
    private readonly RevenueReportService _reportService;

    public AdminReportsController(RevenueReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
        
        var result = await _reportService.GetDashboardAsync(startDate, endDate);
        return Ok(result);
    }
}
```

---

## 2. Database Optimization

### 2.1. SQL Server Indexes
Báo cáo doanh thu lọc theo khoảng thời gian và trạng thái hóa đơn (`Status = Paid`). Vì vậy, cần bổ sung Index kết hợp (Composite Index) để tăng tốc độ tìm kiếm:

```sql
-- Index tối ưu hóa cho truy vấn doanh thu hóa đơn theo ngày
CREATE INDEX IX_Invoices_Status_CreatedAt_Includes
ON Invoices (Status, CreatedAt)
INCLUDE (TotalAmount, ServiceAmount, MedicineAmount);

-- Index tối ưu hóa cho truy vấn đếm lịch hẹn theo ngày
CREATE INDEX IX_Appointments_AppointmentDate
ON Appointments (AppointmentDate);
```

### 2.2. Entity Framework Migration
Được khai báo thông qua `DbContext` Fluent API hoặc Migration class:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Invoice>()
        .HasIndex(i => new { i.Status, i.CreatedAt })
        .HasDatabaseName("IX_Invoices_Status_CreatedAt")
        .IncludeProperties(i => new { i.TotalAmount, i.ServiceAmount, i.MedicineAmount });

    modelBuilder.Entity<Appointment>()
        .HasIndex(a => a.AppointmentDate)
        .HasDatabaseName("IX_Appointments_AppointmentDate");
}
```
