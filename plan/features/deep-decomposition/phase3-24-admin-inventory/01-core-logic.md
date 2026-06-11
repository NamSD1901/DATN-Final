# 01. Core Business Logic Reference - Admin Drug Inventory

Tài liệu đặc tả các thuật toán xử lý nghiệp vụ kho cốt lõi, quản lý xuất kho theo hạn dùng (FEFO) và mã nguồn C# thực thi phía Backend cho phân hệ Quản lý Kho thuốc & Vật tư.

---

## 1. Thuật toán Xuất kho Ưu tiên Hạn dùng (FEFO - First Expired First Out)

Để tối ưu hóa thời gian sử dụng thuốc và tránh lãng phí biệt dược hết hạn, hệ thống MyPetClinic áp dụng nguyên tắc **FEFO (Hạn dùng gần nhất xuất trước)** khi trừ kho thuốc trong đơn kê của Bác sĩ.

### Quy trình thuật toán:
1. Khi có yêu cầu xuất số lượng $Q$ cho thuốc $M$:
2. Lấy danh sách toàn bộ các lô hàng (`InventoryBatches`) của thuốc $M$ thỏa mãn:
   - Trạng thái `Status = 'InStock'` (Không bị khóa/hết hạn).
   - Số lượng còn lại `CurrentQuantity > 0`.
   - Sắp xếp tăng dần theo `ExpiryDate` (Lô hạn dùng gần nhất xếp đầu).
3. Duyệt qua từng lô hàng:
   - Nếu số lượng còn lại của lô $B_i \ge Q$:
     - Trừ lô $B_i$ đi $Q$. Set $Q = 0$. Kết thúc vòng lặp.
   - Nếu số lượng còn lại của lô $B_i < Q$:
     - Trừ số lượng còn lại của lô $B_i$ về 0. Cập nhật trạng thái lô thành `Depleted`.
     - Giảm $Q$ đi một lượng bằng số lượng lô $B_i$ vừa xuất. Tiếp tục vòng lặp sang lô tiếp theo.
4. Nếu kết thúc toàn bộ lô hàng mà $Q > 0$:
   - Rollback giao dịch vì không đủ hàng thực tế khả dụng. Ném ra lỗi `InvalidOperationException`.

---

## 2. Giao dịch ACID Nhập kho & Trừ kho (C# Implementation)

Đoạn mã C# dưới đây triển khai dịch vụ `InventoryService` quản trị kho, bảo đảm tính toàn vẹn cơ sở dữ liệu khi nhập kho lô mới hoặc xuất kho kê đơn.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.DTOs.Inventory;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;

namespace MyPetClinic.Application.Services
{
    public interface IInventoryService
    {
        Task<BatchDto> ImportNewBatchAsync(ImportBatchRequest request, Guid adminId);
        Task DeductStockAsync(Guid medicineId, int quantity, Guid actorId);
        Task<List<MedicineInventoryDto>> GetLowStockMedicinesAsync();
        Task<List<BatchDto>> GetExpiringBatchesAsync(int daysThreshold = 30);
    }

    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(AppDbContext context, ILogger<InventoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BatchDto> ImportNewBatchAsync(ImportBatchRequest request, Guid adminId)
        {
            if (request.ExpiryDate <= DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Hạn sử dụng của lô thuốc mới nhập phải là ngày trong tương lai.");
            }

            var medicine = await _context.Medicines.FindAsync(request.MedicineId);
            if (medicine == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy biệt dược với ID {request.MedicineId}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo bản ghi lô hàng mới
                var batch = new InventoryBatch
                {
                    Id = Guid.NewGuid(),
                    MedicineId = request.MedicineId,
                    BatchNumber = request.BatchNumber,
                    InitialQuantity = request.Quantity,
                    CurrentQuantity = request.Quantity,
                    ManufacturingDate = request.ManufacturingDate,
                    ExpiryDate = request.ExpiryDate,
                    Status = "InStock"
                };
                _context.InventoryBatches.Add(batch);

                // 2. Ghi nhận giao dịch nhập kho
                var txLog = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    MedicineId = request.MedicineId,
                    BatchId = batch.Id,
                    TransactionType = "IMPORT",
                    Quantity = request.Quantity,
                    ActorId = adminId,
                    Timestamp = DateTime.UtcNow
                };
                _context.InventoryTransactions.Add(txLog);

                // 3. Cộng dồn số dư tổng của thuốc
                medicine.CurrentStock += request.Quantity;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Admin {adminId} nhập thành công lô {request.BatchNumber} thuốc {medicine.Name}");

                return new BatchDto
                {
                    Id = batch.Id,
                    BatchNumber = batch.BatchNumber,
                    CurrentQuantity = batch.CurrentQuantity,
                    ExpiryDate = batch.ExpiryDate,
                    BatchStatus = batch.Status
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi xảy ra khi thực thi giao dịch nhập kho.");
                throw;
            }
        }

        public async Task DeductStockAsync(Guid medicineId, int quantity, Guid actorId)
        {
            if (quantity <= 0) throw new ArgumentException("Số lượng xuất kho phải lớn hơn 0.");

            // Khóa dòng thuốc bi quan để chặn race-condition khi nhiều bác sĩ kê đơn cùng lúc
            var medicine = await _context.Medicines
                .FromSqlInterpolated($"SELECT * FROM Medicines WHERE Id = {medicineId} FOR UPDATE")
                .FirstOrDefaultAsync();

            if (medicine == null)
            {
                throw new KeyNotFoundException("Không tìm thấy biệt dược được yêu cầu.");
            }

            if (medicine.CurrentStock < quantity)
            {
                throw new InvalidOperationException($"Không đủ tồn kho khả dụng cho thuốc {medicine.Name}. Hiện còn: {medicine.CurrentStock}");
            }

            // Lấy danh sách các lô hàng theo nguyên tắc FEFO (Hạn dùng gần nhất xuất trước)
            var activeBatches = await _context.InventoryBatches
                .Where(b => b.MedicineId == medicineId && b.Status == "InStock" && b.CurrentQuantity > 0)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            int remainingToDeduct = quantity;

            foreach (var batch in activeBatches)
            {
                if (remainingToDeduct <= 0) break;

                if (batch.CurrentQuantity >= remainingToDeduct)
                {
                    batch.CurrentQuantity -= remainingToDeduct;
                    
                    // Tạo log giao dịch xuất kho
                    var txLog = new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        MedicineId = medicineId,
                        BatchId = batch.Id,
                        TransactionType = "EXPORT",
                        Quantity = remainingToDeduct,
                        ActorId = actorId,
                        Timestamp = DateTime.UtcNow
                    };
                    _context.InventoryTransactions.Add(txLog);

                    remainingToDeduct = 0;
                }
                else
                {
                    remainingToDeduct -= batch.CurrentQuantity;

                    var txLog = new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        MedicineId = medicineId,
                        BatchId = batch.Id,
                        TransactionType = "EXPORT",
                        Quantity = batch.CurrentQuantity,
                        ActorId = actorId,
                        Timestamp = DateTime.UtcNow
                    };
                    _context.InventoryTransactions.Add(txLog);

                    batch.CurrentQuantity = 0;
                    batch.Status = "Depleted";
                }
            }

            if (remainingToDeduct > 0)
            {
                throw new InvalidOperationException("Tổng số lượng khả dụng trong các lô hàng không khớp với số dư tổng.");
            }

            // Trừ số dư tổng
            medicine.CurrentStock -= quantity;
            await _context.SaveChangesAsync();
        }

        public async Task<List<MedicineInventoryDto>> GetLowStockMedicinesAsync()
        {
            return await _context.Medicines
                .Where(m => m.CurrentStock <= m.MinStockThreshold)
                .Select(m => new MedicineInventoryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ActiveIngredient = m.ActiveIngredient,
                    Price = m.Price,
                    MinStockThreshold = m.MinStockThreshold,
                    CurrentStock = m.CurrentStock,
                    StockStatus = m.CurrentStock == 0 ? "Depleted" : "LowStock"
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BatchDto>> GetExpiringBatchesAsync(int daysThreshold = 30)
        {
            var limitDate = DateTime.UtcNow.AddDays(daysThreshold).Date;
            return await _context.InventoryBatches
                .Where(b => b.ExpiryDate <= limitDate && b.Status == "InStock" && b.CurrentQuantity > 0)
                .OrderBy(b => b.ExpiryDate)
                .Select(b => new BatchDto
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    CurrentQuantity = b.CurrentQuantity,
                    ExpiryDate = b.ExpiryDate,
                    BatchStatus = b.ExpiryDate < DateTime.UtcNow.Date ? "Expired" : "ExpiringSoon"
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
```
