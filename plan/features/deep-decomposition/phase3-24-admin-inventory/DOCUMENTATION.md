# 📄 User & Dev Documentation - Admin Drug Inventory

## 1. Schema DB Nhật ký nhập kho (StockLogs Table)

```sql
CREATE TABLE "StockLogs" (
    "Id" UUID PRIMARY KEY,
    "MedicineId" UUID NOT NULL REFERENCES "Medicines"("Id"),
    "AdjustedQuantity" INT NOT NULL,
    "Notes" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL
);
```

---

## 2. Cấu hình trường MinStockLimit trong Entity Medicine

Lập trình viên backend cần thêm cột `MinStockLimit` vào bảng `Medicines` nếu chưa có và chạy Migration:

```csharp
// In Medicine Entity
public int MinStockLimit { get; set; } = 10; // Giá trị mặc định là 10

// In DbContext OnModelCreating
modelBuilder.Entity<Medicine>()
    .Property(m => m.MinStockLimit)
    .HasDefaultValue(10);
```
