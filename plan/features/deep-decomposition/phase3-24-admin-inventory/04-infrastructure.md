# 04. Infrastructure & Security - Admin Drug Inventory

Tài liệu thiết kế hạ tầng, phân quyền vai trò (RBAC), giải quyết xung đột đồng thời trừ kho và cơ chế an toàn vận hành kho thuốc phòng khám.

---

## 1. Phân quyền vai trò Truy cập Kho (Role-Based Access Control)

Nhằm đảm bảo an toàn kho và tránh thất thoát dược phẩm, các quyền hạn thao tác kho được chia cụ thể cho hai vai trò cốt lõi:
- **Vai trò Bác sĩ thú y (`doctor`):** 
  - Quyền truy cập: Chỉ được phép Đọc danh mục thuốc (`Read-Only`) để tra cứu tồn khả dụng khi kê đơn thuốc.
  - Quyền thay đổi: Không được phép trực tiếp sửa thông tin thuốc hoặc nhập lô hàng mới. Quyền xuất kho chỉ được thực hiện gián tiếp thông qua luồng lưu bệnh án chẩn đoán (ACID Transaction trừ kho tự động).
- **Vai trò Quản trị viên (`admin`):**
  - Có toàn quyền CRUD danh mục thuốc, tạo phiếu nhập kho, điều chỉnh số dư và truy xuất lịch sử Audit giao dịch kho.

### C# Controller Role Filters:
```csharp
[ApiController]
[Route("api/admin/inventory")]
public class AdminInventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public AdminInventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("batches")]
    [Authorize(Roles = "admin")] // Chỉ Admin được tạo phiếu nhập kho
    public async Task<IActionResult> ImportBatch([FromBody] ImportBatchRequest request)
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _inventoryService.ImportNewBatchAsync(request, adminId);
        return CreatedAtAction(nameof(GetBatchById), new { id = result.Id }, result);
    }

    [HttpGet("medicines")]
    [Authorize(Roles = "doctor,admin")] // Cả Doctor và Admin đều được xem số dư kho
    public async Task<IActionResult> GetMedicinesInventory()
    {
        var result = await _inventoryService.GetAllMedicinesWithStockAsync();
        return Ok(result);
    }
}
```

---

## 2. Giải quyết Xung đột Đồng thời (Concurrency Conflict Management)

Trong giờ cao điểm, có thể có 3-4 bác sĩ cùng kê một loại thuốc khan hiếm (ví dụ: *Thuốc kháng sinh Amoxicillin* chỉ còn tồn kho 15 viên, bác sĩ A kê 10 viên, bác sĩ B kê 10 viên). Nếu hệ thống sử dụng cơ chế đọc và ghi thông thường không khóa, sẽ xảy ra hiện tượng **bán âm kho** (Cả hai bác sĩ đều lưu đơn thành công và số tồn kho nhảy về -5 viên).

### Giải pháp kỹ thuật: Khóa dòng Bi quan (Pessimistic Locking - FOR UPDATE)
Khi bắt đầu quá trình kiểm duyệt kê đơn thuốc, Entity Framework Core sẽ biên dịch câu lệnh SQL chứa chỉ thị `FOR UPDATE` của PostgreSQL để khóa dòng thuốc đó lại, buộc các luồng khác phải xếp hàng chờ cho đến khi giao dịch hiện tại hoàn tất (Commit hoặc Rollback).

```sql
-- Câu lệnh SQL được biên dịch để khóa dòng biệt dược
SELECT "CurrentStock" 
FROM "Medicines" 
WHERE "Id" = @medicineId 
FOR UPDATE;
```

Trong C# Service, sử dụng SQL thô hoặc phương thức mở rộng hỗ trợ Transaction để khóa dòng:
```csharp
using (var dbTransaction = await _context.Database.BeginTransactionAsync())
{
    try
    {
        // Khóa dòng thuốc bi quan
        var medicine = await _context.Medicines
            .FromSqlInterpolated($"SELECT * FROM Medicines WHERE Id = {medicineId} FOR UPDATE")
            .FirstOrDefaultAsync();

        if (medicine.CurrentStock < requestedQty)
        {
            throw new InvalidOperationException("Số lượng tồn kho không đủ.");
        }

        // Thực hiện trừ kho...
        medicine.CurrentStock -= requestedQty;
        await _context.SaveChangesAsync();
        await dbTransaction.CommitAsync();
    }
    catch
    {
        await dbTransaction.RollbackAsync();
        throw;
    }
}
```

---

## 3. Rate Limiting Chính sách Giới hạn Tần suất

- **Tạo phiếu nhập kho:** Giới hạn tối đa **10 requests / phút** trên một tài khoản Admin để ngăn chặn lỗi double-submit tạo nhiều lô hàng trùng lặp.
- **Tra cứu tồn kho của bác sĩ:** Giới hạn tối đa **60 requests / phút** cho mỗi tài khoản để tối ưu hóa tải trọng Database trong ca trực.
