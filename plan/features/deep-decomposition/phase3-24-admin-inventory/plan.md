# 📅 Implementation Plan & Test Strategy - Admin Drug Inventory

Tài liệu kế hoạch triển khai chi tiết (Micro-roadmap) và bộ kịch bản kiểm thử (Test Strategy) dành cho phân hệ Quản lý Kho thuốc & Vật tư.

---

## 1. Lộ trình Triển khai Chi tiết (4-Phase Micro-Roadmap)

### Giai đoạn 1: Thiết kế Cơ sở dữ liệu & C# Entities (Tuần 1)
- Xây dựng thực thể `InventoryBatch` và `InventoryTransaction` trong Domain Layer.
- Thiết lập cấu hình EF Core Fluent API, cấu hình khóa ngoại và các index cho hạn sử dụng và số lô.
- Chạy Database Migrations cập nhật bảng `Medicines` và tạo bảng mới.

### Giai đoạn 2: Phát triển Backend API & Lock Logic (Tuần 2)
- Viết thuật toán trừ kho ưu tiên hạn dùng (FEFO) tại `InventoryService`.
- Tích hợp khóa dòng Postgres bi quan `FOR UPDATE` cho API xuất kho để chống race-condition.
- Phát triển API Controller và các DTO validations.
- Thiết lập Rate Limiting và phân quyền truy cập Roles.

### Giai đoạn 3: Phát triển Giao diện Vue 3 Client (Tuần 3)
- Viết Pinia Store `useInventoryStore.ts` bằng TypeScript kết nối RESTful API.
- Dựng giao diện Workspace Quản lý kho với phong cách Glassmorphism mờ kính.
- Thiết kế Popup Nhập lô hàng và Dialog cảnh báo thuốc cận hạn/hết hàng.
- Đồng bộ hiển thị tồn khả dụng trên màn hình khám lâm sàng của Bác sĩ.

### Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 4)
- Viết unit tests kiểm tra logic trừ kho FEFO.
- Viết integration tests kiểm tra xung đột đồng thời khi 2 bác sĩ cùng kê một loại thuốc.
- Tối ưu hóa hiệu năng phản hồi API khi tải danh sách thuốc kèm lô hàng.

---

## 2. Kịch bản Kiểm thử QA (QA Test Cases)

| Mã Test Case | Phân loại | Mục tiêu kiểm thử | Các bước thực hiện | Kết quả mong đợi |
| :--- | :--- | :--- | :--- | :--- |
| **TC-INV-01** | Unit Test | Kiểm tra thuật toán FEFO xuất kho | Có lô A (Hạn dùng 12/2026, SL 50) và lô B (Hạn dùng 06/2027, SL 100). Gọi API xuất kho 60 viên thuốc. | Lô A bị trừ sạch về 0 và chuyển sang `Depleted`. Lô B bị trừ 10 viên còn lại 90 viên. |
| **TC-INV-02** | Security | Chống thay đổi kho của các vai trò thường | Đăng nhập tài khoản `doctor` hoặc `customer`, gọi API `POST /api/admin/inventory/batches` để nhập lô hàng mới. | Backend trả về mã lỗi `403 Forbidden`, giao dịch bị từ chối. |
| **TC-INV-03** | Boundary | Chặn nhập lô thuốc có hạn sử dụng quá khứ | Gửi payload nhập kho với `ExpiryDate` là ngày hôm qua. | Hệ thống trả về lỗi `400 Bad Request` do vi phạm DTO validation. |
| **TC-INV-04** | Concurrency | Kiểm tra trừ kho đồng thời (Race Condition) | Tạo 2 luồng đồng thời gọi API xuất kho 10 viên thuốc Amoxicillin (Tồn tổng thực tế = 15 viên). | Giao dịch 1 thành công (còn tồn 5). Giao dịch 2 thất bại và rollback, trả về lỗi `422 Unprocessable` báo không đủ tồn kho. |

---

## 3. Mã nguồn Unit Test C# xUnit mẫu

Dưới đây là mã nguồn unit test sử dụng **xUnit** và **FluentAssertions** kiểm định tính đúng đắn của logic trừ kho theo nguyên tắc FEFO:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;
using Xunit;

namespace MyPetClinic.Tests
{
    public class InventoryServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly InventoryService _inventoryService;

        public InventoryServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _inventoryService = new InventoryService(_context, NullLogger<InventoryService>.Instance);
        }

        [Fact]
        public async Task DeductStock_ShouldApplyFefoRule_AndDeductFirstExpiringBatch()
        {
            // Arrange
            var medicineId = Guid.NewGuid();
            var actorId = Guid.NewGuid();

            var medicine = new Medicine
            {
                Id = medicineId,
                Name = "Amoxicillin 500mg",
                ActiveIngredient = "Amoxicillin",
                Price = 15000m,
                CurrentStock = 150,
                MinStockThreshold = 20
            };
            _context.Medicines.Add(medicine);

            // Lô A hết hạn trước (31/12/2026) - Số lượng 50
            var batchA = new InventoryBatch
            {
                Id = Guid.NewGuid(),
                MedicineId = medicineId,
                BatchNumber = "LOT-A",
                InitialQuantity = 50,
                CurrentQuantity = 50,
                ExpiryDate = new DateTime(2026, 12, 31),
                Status = "InStock"
            };

            // Lô B hết hạn sau (30/06/2027) - Số lượng 100
            var batchB = new InventoryBatch
            {
                Id = Guid.NewGuid(),
                MedicineId = medicineId,
                BatchNumber = "LOT-B",
                InitialQuantity = 100,
                CurrentQuantity = 100,
                ExpiryDate = new DateTime(2027, 06, 30),
                Status = "InStock"
            };

            _context.InventoryBatches.AddRange(batchA, batchB);
            await _context.SaveChangesAsync();

            // Act: Xuất kho 60 viên
            await _inventoryService.DeductStockAsync(medicineId, 60, actorId);

            // Assert
            // Lô A (50 viên) phải bị trừ hết sạch và chuyển sang Depleted
            // Lô B (100 viên) bị trừ nốt 10 viên -> còn 90 viên
            // Tồn tổng thuốc: 150 - 60 = 90 viên
            var updatedMedicine = await _context.Medicines.FindAsync(medicineId);
            updatedMedicine!.CurrentStock.Should().Be(90);

            var updatedBatchA = await _context.InventoryBatches.FindAsync(batchA.Id);
            updatedBatchA!.CurrentQuantity.Should().Be(0);
            updatedBatchA.Status.Should().Be("Depleted");

            var updatedBatchB = await _context.InventoryBatches.FindAsync(batchB.Id);
            updatedBatchB!.CurrentQuantity.Should().Be(90);
            updatedBatchB.Status.Should().Be("InStock");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
```
