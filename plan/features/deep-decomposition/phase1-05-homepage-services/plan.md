# 📝 Implementation Plan & Testing Strategy - Homepage Services (Phase 1)

Tài liệu này đặc tả lộ trình thực thi từng giai đoạn phát triển và bộ kịch bản kiểm thử (Test Suite) bao phủ toàn diện cho tính năng Danh mục Dịch vụ công khai.

---

## 1. Lộ trình Triển khai Chi tiết (Implementation Phases)

| Giai đoạn | Công việc Cụ thể (Tasks) | Tệp tin Tác động | Kỹ năng Kiểm soát |
| :--- | :--- | :--- | :--- |
| **Phase 1: DB Schema** | 1. Tạo thực thể `ServiceCategory` và `Service`. <br>2. Viết migration liên kết khoá ngoại, chạy cập nhật PostgreSQL. | `Domain/Entities/`<br>`Infrastructure/Data/Migrations/` | **BE-C02 (EF Core)** |
| **Phase 2: Cache Service** | 1. Đăng ký dịch vụ `AddMemoryCache` ở Program.cs. <br>2. Triển khai `ServiceService` truy vấn DB và ghi cache IMemoryCache. | `WebApi/Program.cs`<br>`Application/Services/ServiceService.cs` | **BE-A03 (Performance)**<br>**BE-F02 (SOLID)** |
| **Phase 3: Web API Controllers** | 1. Xây dựng API `/api/services` với thuộc tính `[AllowAnonymous]`. <br>2. Tạo endpoint Admin xóa Cache khi cập nhật dịch vụ. | `WebApi/Controllers/ServiceController.cs` | **BE-C01 (WebAPI)** |
| **Phase 4: Pinia Store** | 1. Triển khai store `useServicesStore.ts` quản lý state. <br>2. Viết computed getter `filteredServices` lọc cục bộ ở Client. | `frontend/src/store/useServicesStore.ts` | **FE-C03 (Pinia)** |
| **Phase 5: Responsive Card Grid** | 1. Thiết kế component `ServicesCatalog.vue` hiển thị lưới card mờ kính. <br>2. Thêm hiệu ứng Shimmer skeleton loading, zoom ảnh hover. | `frontend/src/components/home/` | **FE-C01 (Vue 3)** |

---

## 2. Chiến lược Kiểm thử tự động (Testing Strategy)

Chúng ta xây dựng bộ kiểm thử đơn vị (Unit Tests) để xác minh tính đúng đắn của logic lưu đệm (Caching) và cơ chế dọn dẹp cache.

### 2.1. Bộ Kiểm thử Đơn vị Backend (Unit Tests với xUnit & FluentAssertions)
Tệp tin: `tests/MyPetClinic.Tests/Services/ServiceServiceTests.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using Xunit;

namespace MyPetClinic.Tests.Services
{
    public class ServiceServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMemoryCache _cache;

        public ServiceServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Khởi tạo bộ đệm Memory Cache thật để kiểm tra luồng chạy ghi/đọc cache
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async Task GetActiveServicesAsync_CacheMissThenHit_ShouldRetrieveFromDbThenCache()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var category = new ServiceCategory { Name = "Spa", Code = "spa" };
            context.ServiceCategories.Add(category);
            context.Services.Add(new Service 
            { 
                Name = "Tắm sấy", 
                Description = "Dịch vụ tắm sấy", 
                BasePrice = 100000, 
                Category = category 
            });
            await context.SaveChangesAsync();

            var serviceService = new ServiceService(context, _cache);

            // Act - Lần 1: Cache Miss -> Đọc DB và Ghi Cache
            var result1 = await serviceService.GetActiveServicesAsync(null, null);

            // Assert 1
            result1.Should().HaveCount(1);
            result1[0].Name.Should().Be("Tắm sấy");

            // Thay đổi trực tiếp DB để kiểm chứng lần 2 đọc từ Cache chứ không đọc DB
            context.Services.RemoveRange(context.Services);
            await context.SaveChangesAsync();

            // Act - Lần 2: Cache Hit -> Phải đọc từ Cache ra kết quả cũ mặc dù DB đã trống
            var result2 = await serviceService.GetActiveServicesAsync(null, null);

            // Assert 2
            result2.Should().HaveCount(1);
            result2[0].Name.Should().Be("Tắm sấy"); // Dữ liệu cũ trong Cache vẫn tồn tại
        }
    }
}
```

---

## 3. Bộ Kịch bản Kiểm thử QA (QA Test Cases Suite)

### 🔴 Nhóm 1: Kiểm thử Nghiệp vụ Chính (Critical Paths)
*   **TC-SRV-001: Tải danh mục dịch vụ công khai thành công**
    *   *Kỳ vọng:* Trang chủ load mượt mà, hiển thị đầy đủ hình ảnh, mô tả, giá cả các dịch vụ. Không yêu cầu đăng nhập.
*   **TC-SRV-002: Lọc dịch vụ theo danh mục (Category Filtering)**
    *   *Hành động:* Click chọn tab "Tiêm phòng".
    *   *Kỳ vọng:* Hệ thống lọc tức thời và hiển thị các dịch vụ tiêm phòng. Các dịch vụ khác biến mất nhanh chóng.
*   **TC-SRV-003: Tìm kiếm dịch vụ theo từ khóa (Live Search)**
    *   *Hành động:* Gõ chữ `Khám` vào thanh tìm kiếm.
    *   *Kỳ vọng:* Lưới dịch vụ hiển thị các dịch vụ như "Khám lâm sàng", "Khám sức khỏe tổng quát".

### 🟠 Nhóm 2: Kiểm thử Hiệu năng & Biên (Performance & Boundary)
*   **TC-SRV-004: Xác nhận hiệu năng Caching (Response Time Test)**
    *   *Hành động:* Gửi liên tiếp 50 requests truy vấn API `/api/services`.
    *   *Kỳ vọng:* Thời gian phản hồi trung bình của API phải dưới **10ms** nhờ cơ chế lưu đệm Memory Cache.
*   **TC-SRV-005: Tìm kiếm không ra kết quả (No Results Boundary)**
    *   *Hành động:* Gõ `xyz123` vào ô tìm kiếm.
    *   *Kỳ vọng:* Ẩn toàn bộ lưới dịch vụ, hiển thị hình ảnh kính lúp phóng to cùng thông điệp `"Không tìm thấy kết quả"`.
