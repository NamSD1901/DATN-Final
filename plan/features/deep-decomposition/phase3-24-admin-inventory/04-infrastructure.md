# 🌐 Infrastructure & Security - Admin Drug Inventory

## 🔗 Skills Liên Quan
- **BE-A03 (RBAC):** `[Authorize(Roles = "admin")]` bảo vệ toàn bộ Endpoint thao tác kho.
- **BE-C02 (EF Core):** Tạo Index trên cột `ExpiryDate` để tối ưu truy vấn lọc thuốc sắp hết hạn.

---

## 1. Cấu hình Controller kho thuốc Admin

```csharp
[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/medicines")]
public class AdminMedicinesController : ControllerBase
{
    private readonly InventoryAlertService _alertService;

    public AdminMedicinesController(InventoryAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = await _alertService.GetInventoryAlertsAsync();
        return Ok(alerts);
    }

    [HttpPost("{id}/restock")]
    public async Task<IActionResult> Restock(Guid id, [FromBody] UpdateStockRequestDto request)
    {
        if (request.AdjustQuantity <= 0)
            return BadRequest(new { message = "Số lượng nhập phải lớn hơn 0" });

        await _alertService.RestockAsync(id, request.AdjustQuantity, request.Notes);
        return Ok(new { message = "Đã cập nhật tồn kho thành công" });
    }
}
```

---

## 2. Tối ưu DB Index cho cảnh báo hạn sử dụng

```sql
CREATE INDEX "IX_Medicines_ExpiryDate" ON "Medicines" ("ExpiryDate");
CREATE INDEX "IX_Medicines_StockQuantity_MinStockLimit" ON "Medicines" ("StockQuantity", "MinStockLimit");
```
