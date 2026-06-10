# 🧠 Core Business Logic - Admin Drug Inventory

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Tính toán ngày hết hạn và so sánh với ngày hiện tại để phân loại mức độ cảnh báo.
- **BE-F03 (Async/Await):** Thực thi truy vấn bất đồng bộ khi lọc danh sách thuốc cần cảnh báo.

---

## 1. C# Logic: Kiểm tra cảnh báo tồn kho

```csharp
public class InventoryAlertService
{
    private readonly MyPetClinicDbContext _context;

    public InventoryAlertService(MyPetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<List<MedicineAlertDto>> GetInventoryAlertsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var nearExpiryDate = today.AddDays(30);

        var alerts = await _context.Medicines
            .Where(m => m.StockQuantity < m.MinStockLimit || 
                       (m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date <= nearExpiryDate))
            .Select(m => new MedicineAlertDto
            {
                Id = m.Id,
                Name = m.Name,
                StockQuantity = m.StockQuantity,
                MinStockLimit = m.MinStockLimit,
                ExpiryDate = m.ExpiryDate,
                IsLowStock = m.StockQuantity < m.MinStockLimit,
                IsNearExpiry = m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date <= nearExpiryDate,
                DaysUntilExpiry = m.ExpiryDate.HasValue 
                    ? (int)(m.ExpiryDate.Value.Date - today).TotalDays 
                    : (int?)null
            })
            .OrderBy(m => m.DaysUntilExpiry)
            .ToListAsync();

        return alerts;
    }

    public async Task RestockAsync(Guid medicineId, int quantity, string notes)
    {
        var medicine = await _context.Medicines.FindAsync(medicineId);
        if (medicine == null) throw new Exception("Thuốc không tồn tại.");

        medicine.StockQuantity += quantity;

        // Ghi nhật ký nhập kho
        _context.StockLogs.Add(new StockLog
        {
            MedicineId = medicineId,
            AdjustedQuantity = quantity,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
```
