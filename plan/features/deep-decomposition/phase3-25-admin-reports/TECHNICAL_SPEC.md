# 🛠️ Technical Specification - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **BE-F03 (Async/Await):** Truy vấn song song các chỉ số thống kê bằng `Task.WhenAll` để tối ưu hóa tốc độ phản hồi API.
- **BE-C02 (EF Core):** Áp dụng Index trên cột `CreatedAt` và `Status` bảng `Invoices` để cải thiện hiệu năng truy vấn GROUP BY.

---

## 1. Sequence Diagram: Tải Dashboard Báo cáo Doanh thu

```mermaid
sequenceDiagram
    actor Admin as Quản trị viên
    participant FE as Vue Chart Components
    participant API as Web API Gateway
    participant DB as PostgreSQL Database

    Admin->>FE: Mở trang Báo cáo & chọn khoảng ngày lọc
    FE->>API: GET /api/admin/reports/revenue?startDate=...&endDate=...
    
    Note over API: Thực thi 3 truy vấn song song
    
    par Truy vấn song song
        API->>DB: Query doanh thu từng ngày (Group By Date)
    and
        API->>DB: Query Top 5 dịch vụ + bác sĩ
    and
        API->>DB: Query KPI tổng quan (Total, Count, Growth %)
    end
    
    DB-->>API: Trả về kết quả các truy vấn
    API-->>FE: HTTP 200 OK (Đối tượng JSON tổng hợp)
    FE-->>Admin: Vẽ biểu đồ Line, Donut và bảng xếp hạng
```

---

## 2. DTOs

```csharp
public class RevenueDashboardDto
{
    public decimal TotalRevenue { get; set; }
    public decimal GrowthRate { get; set; } // % so với kỳ trước
    public int TotalAppointments { get; set; }
    public decimal ServiceRevenue { get; set; }
    public decimal MedicineRevenue { get; set; }
    public List<DailyRevenueDto> DailyChart { get; set; }
    public List<TopItemDto> TopServices { get; set; }
    public List<TopItemDto> TopDoctors { get; set; }
}
```
