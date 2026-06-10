# 🛠️ Technical Specification - Admin Drug Inventory

## 🔗 Skills Liên Quan
- **BE-F02 (SOLID - SRP):** Tách biệt nghiệp vụ CRUD thuốc (`MedicineService`) và nghiệp vụ kiểm kho cảnh báo (`InventoryAlertService`).
- **BE-A03 (RBAC):** Gán tag `[Authorize(Roles = "admin")]` bảo vệ tài nguyên kho.

---

## 1. Sequence Diagram: Kiểm tra cảnh báo tồn kho tự động (Inventory Alert Flow)

```mermaid
sequenceDiagram
    actor Admin as Quản trị viên
    participant FE as Vue Inventory View
    participant API as Web API Gateway
    participant DB as PostgreSQL Database

    Admin->>FE: Truy cập trang quản lý kho thuốc
    FE->>API: GET /api/admin/medicines/alerts
    Note over API: Đọc danh sách thuốc<br/>So sánh StockQuantity < MinStockLimit
    API->>DB: Query Medicines where StockQuantity < MinStockLimit OR ExpiryDate < Today + 30 Days
    DB-->>API: Trả về danh sách thuốc cảnh báo
    API-->>FE: HTTP 200 OK (Danh sách thuốc cần xử lý)
    FE-->>Admin: Hiển thị danh sách badge đỏ (Cảnh báo tồn kho / Sắp hết hạn)
```

---

## 2. API Schema & DTOs

```csharp
public class MedicineDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLimit { get; set; }
    public decimal ImportPrice { get; set; }
    public decimal Price { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateStockRequestDto
{
    public int AdjustQuantity { get; set; } // Số lượng cộng dồn thêm (nhập kho)
    public string Notes { get; set; }
}
```
