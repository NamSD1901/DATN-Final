# 📝 Implementation Plan & Testing Strategy - Vets Team (Phase 1)

Tài liệu này đặc tả lộ trình thực thi từng giai đoạn phát triển và bộ kịch bản kiểm thử (Test Suite) bao phủ toàn diện cho tính năng Danh mục Bác sĩ công khai.

---

## 1. Lộ trình Triển khai Chi tiết (Implementation Phases)

| Giai đoạn | Công việc Cụ thể (Tasks) | Tệp tin Tác động | Kỹ năng Kiểm soát |
| :--- | :--- | :--- | :--- |
| **Phase 1: DB Schema** | 1. Tạo thực thể `Doctor` liên kết 1-1 với `User`. <br>2. Viết migration liên kết khoá ngoại, chạy cập nhật PostgreSQL. | `Domain/Entities/`<br>`Infrastructure/Data/Migrations/` | **BE-C02 (EF Core)** |
| **Phase 2: Cache Service** | 1. Triển khai `DoctorService` truy vấn DB và ghi cache IMemoryCache. <br>2. Viết hàm dọn dẹp cache chủ động. | `Application/Services/DoctorService.cs` | **BE-A03 (Performance)**<br>**BE-F02 (SOLID)** |
| **Phase 3: Web API Controllers** | 1. Xây dựng API `/api/doctors` với thuộc tính `[AllowAnonymous]`. <br>2. Tạo endpoint Admin xóa Cache khi cập nhật bác sĩ. | `WebApi/Controllers/DoctorController.cs` | **BE-C01 (WebAPI)** |
| **Phase 4: Pinia Store** | 1. Triển khai store `useDoctorsStore.ts` quản lý state. <br>2. Viết computed getter `filteredDoctors` lọc cục bộ ở Client. | `frontend/src/store/useDoctorsStore.ts` | **FE-C03 (Pinia)** |
| **Phase 5: Responsive Card Grid** | 1. Thiết kế component `VetsTeamCatalog.vue` hiển thị lưới card mờ kính. <br>2. Thêm hiệu ứng Shimmer skeleton loading, pulse badge nhấp nháy. | `frontend/src/components/home/` | **FE-C01 (Vue 3)** |

---

## 2. Chiến lược Kiểm thử tự động (Testing Strategy)

Chúng ta xây dựng bộ kiểm thử đơn vị (Unit Tests) để xác minh tính đúng đắn của logic lưu đệm (Caching) và cơ chế dọn dẹp cache.

### 2.1. Bộ Kiểm thử Đơn vị Backend (Unit Tests với xUnit & FluentAssertions)
Tệp tin: `tests/MyPetClinic.Tests/Services/DoctorServiceTests.cs`

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
    public class DoctorServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
        private readonly IMemoryCache _cache;

        public DoctorServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [Fact]
        public async Task GetActiveDoctorsAsync_CacheMissThenHit_ShouldRetrieveFromDbThenCache()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbOptions);
            var user = new User { FullName = "Dr. Vy", Email = "doctor.vy@example.com", IsActive = true };
            context.Users.Add(user);
            context.Doctors.Add(new Doctor 
            { 
                User = user, 
                Specialty = Specialty.NoiKhoa, 
                Qualifications = "Thạc sĩ",
                IsOnDuty = true
            });
            await context.SaveChangesAsync();

            var doctorService = new DoctorService(context, _cache);

            // Act - Lần 1: Cache Miss -> Đọc DB và Ghi Cache
            var result1 = await doctorService.GetActiveDoctorsAsync(null);

            // Assert 1
            result1.Should().HaveCount(1);
            result1[0].FullName.Should().Be("Dr. Vy");

            // Thay đổi trực tiếp DB để kiểm chứng lần 2 đọc từ Cache chứ không đọc DB
            context.Doctors.RemoveRange(context.Doctors);
            await context.SaveChangesAsync();

            // Act - Lần 2: Cache Hit -> Phải đọc từ Cache ra kết quả cũ mặc dù DB đã trống
            var result2 = await doctorService.GetActiveDoctorsAsync(null);

            // Assert 2
            result2.Should().HaveCount(1);
            result2[0].FullName.Should().Be("Dr. Vy"); // Dữ liệu cũ trong Cache vẫn tồn tại
        }
    }
}
```

---

## 3. Bộ Kịch bản Kiểm thử QA (QA Test Cases Suite)

### 🔴 Nhóm 1: Kiểm thử Nghiệp vụ Chính (Critical Paths)
*   **TC-DOC-001: Tải danh sách bác sĩ công khai thành công**
    *   *Kỳ vọng:* Trang chủ load mượt mà, hiển thị đầy đủ hình ảnh, chuyên khoa, số năm kinh nghiệm các bác sĩ. Không yêu cầu đăng nhập.
*   **TC-DOC-002: Lọc bác sĩ theo chuyên khoa (Specialty Filtering)**
    *   *Hành động:* Click chọn tab "Da liễu".
    *   *Kỳ vọng:* Hệ thống lọc tức thời và hiển thị các bác sĩ da liễu. Các bác sĩ khác biến mất nhanh chóng.

### 🟠 Nhóm 2: Kiểm thử Hiệu năng & Biên (Performance & Boundary)
*   **TC-DOC-003: Xác nhận hiệu năng Caching (Response Time Test)**
    *   *Hành động:* Gửi liên tiếp 50 requests truy vấn API `/api/doctors`.
    *   *Kỳ vọng:* Thời gian phản hồi trung bình của API phải dưới **10ms** nhờ cơ chế lưu đệm Memory Cache.
*   **TC-DOC-004: Bác sĩ nghỉ phép không được đặt lịch**
    *   *Hành động:* Thẻ bác sĩ hiển thị trạng thái `Nghỉ phép` (chấm đỏ). Click vào thẻ.
    *   *Kỳ vọng:* Nút "Đặt lịch khám" bị disabled màu xám, không cho phép click tương tác.
