# ⚙️ Đặc Tả Kỹ Thuật Chi Tiết - Clinic Public Services & Vets View (Sprint 4)

Tài liệu này đặc tả chi tiết mã nguồn, cấu trúc lớp, sơ đồ dữ liệu, API contracts và kịch bản unit test cho **Sprint 4** của dự án **MyPetClinic**.

---

## 1. Thiết Kế Cơ Sở Dữ Liệu & Thực Thể (Entities & EF Core Configuration)

Sprint 4 tập trung vào quản lý danh mục dịch vụ (`ServiceCategory`) và các gói dịch vụ (`ClinicService`).

### 1.1. Thực Thể Domain (C# Domain Entities)

```csharp
// Location: Domain/Entities/ServiceCategory.cs
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class ServiceCategory
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<ClinicService> Services { get; set; } = new List<ClinicService>();
    }
}
```

```csharp
// Location: Domain/Entities/ClinicService.cs
using System;

namespace MyPetClinic.Domain.Entities
{
    public class ClinicService
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? DeletedAt { get; set; }

        public virtual ServiceCategory? Category { get; set; }
    }
}
```

### 1.2. Cấu Hình Fluent API & Bộ Lọc (Infrastructure Configurations)

```csharp
// Location: Infrastructure/Data/Configurations/ServiceCategoryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Data.Configurations
{
    public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder)
        {
            builder.ToTable("ServiceCategories");
            builder.HasKey(sc => sc.Id);
            builder.Property(sc => sc.Name).IsRequired().HasMaxLength(255);
            builder.HasQueryFilter(sc => sc.DeletedAt == null);
        }
    }
}
```

```csharp
// Location: Infrastructure/Data/Configurations/ClinicServiceConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Data.Configurations
{
    public class ClinicServiceConfiguration : IEntityTypeConfiguration<ClinicService>
    {
        public void Configure(EntityTypeBuilder<ClinicService> builder)
        {
            builder.ToTable("ClinicServices");
            builder.HasKey(cs => cs.Id);
            
            builder.Property(cs => cs.Name).IsRequired().HasMaxLength(255);
            builder.Property(cs => cs.Price).HasPrecision(12, 2).IsRequired();
            builder.Property(cs => cs.Description).HasMaxLength(1000);
            
            builder.HasQueryFilter(cs => cs.DeletedAt == null);

            builder.HasOne(cs => cs.Category)
                .WithMany(sc => sc.Services)
                .HasForeignKey(cs => cs.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
```

---

## 2. Giao Ước API & Đội Ngũ Bác Sĩ (API Contracts & Catalog Services)

### 2.1. Cấu Trúc Repositories & Interfaces (DIP)

```csharp
// Location: Application/Common/Interfaces/IClinicServiceRepository.cs
using MyPetClinic.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Common.Interfaces
{
    public interface IClinicServiceRepository
    {
        Task<List<ClinicService>> GetActiveServicesAsync();
        Task<List<User>> GetActiveDoctorsAsync();
    }
}
```

### 2.2. Lớp Dịch Vụ Tra Cứu Catalog (CatalogService.cs)

```csharp
// Location: Application/Services/CatalogService.cs
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class CatalogService
    {
        private readonly IClinicServiceRepository _catalogRepository;

        public CatalogService(IClinicServiceRepository catalogRepository)
        {
            _catalogRepository = catalogRepository;
        }

        public async Task<List<ClinicService>> GetActiveServicesAsync()
        {
            // Sử dụng Repository để lấy danh sách dịch vụ đang hoạt động
            return await _catalogRepository.GetActiveServicesAsync();
        }

        public async Task<List<User>> GetActiveDoctorsAsync()
        {
            // Trả về danh sách bác sĩ thú y đang làm việc
            return await _catalogRepository.GetActiveDoctorsAsync();
        }
    }
}
```

### 2.3. Khai Báo API Endpoints (CatalogController)

```csharp
// Location: WebApi/Controllers/CatalogController.cs
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Services;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogService _catalogService;

        public CatalogController(CatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _catalogService.GetActiveServicesAsync();
            var result = services.Select(s => new ClinicServiceResponse(
                s.Id,
                s.Name,
                s.Price,
                s.DurationMinutes,
                s.Description,
                s.Category?.Name ?? "Khác"
            ));
            return Ok(result);
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _catalogService.GetActiveDoctorsAsync();
            var result = doctors.Select(d => new DoctorPublicResponse(
                d.Id,
                d.FullName,
                d.Email,
                d.Phone,
                d.Avatar,
                d.Gender == 0 ? "Nam" : "Nữ"
            ));
            return Ok(result);
        }
    }

    public record ClinicServiceResponse(long Id, string Name, decimal Price, int? DurationMinutes, string? Description, string CategoryName);
    public record DoctorPublicResponse(System.Guid Id, string FullName, string Email, string? Phone, string? Avatar, string Gender);
}
```

---

## 3. Quản Lý Trạng Thái Phía Client-side SPA (Vue 3 Pinia Store)

```typescript
// Location: frontend/src/stores/catalog.ts
import { defineStore } from 'pinia';
import api from '../utils/api';

export interface ClinicService {
  id: number;
  name: string;
  price: number;
  durationMinutes: number;
  description: string;
  categoryName: string;
}

export interface Doctor {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  avatar: string;
  gender: string;
}

export const useCatalogStore = defineStore('catalog', {
  state: () => ({
    services: [] as ClinicService[],
    doctors: [] as Doctor[],
    loading: false,
    error: null as string | null
  }),
  actions: {
    async fetchServices() {
      this.loading = true;
      try {
        const response = await api.get('/services');
        this.services = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách dịch vụ.';
      } finally {
        this.loading = false;
      }
    },
    async fetchDoctors() {
      this.loading = true;
      try {
        const response = await api.get('/doctors');
        this.doctors = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách bác sĩ.';
      } finally {
        this.loading = false;
      }
    }
  }
});
```

---

## 4. Kịch Bản Kiểm Thử Tra Cứu Catalog (xUnit Tests)

```csharp
// Location: MyPetClinic.Tests/Application/CatalogServiceTests.cs
using FluentAssertions;
using Moq;
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests.Application
{
    public class CatalogServiceTests
    {
        private readonly Mock<IClinicServiceRepository> _catalogRepoMock;
        private readonly CatalogService _catalogService;

        public CatalogServiceTests()
        {
            _catalogRepoMock = new Mock<IClinicServiceRepository>();
            _catalogService = new CatalogService(_catalogRepoMock.Object);
        }

        [Fact]
        public async Task GetActiveServicesAsync_ShouldReturnServicesList_WhenServicesExist()
        {
            // Arrange
            var mockServices = new List<ClinicService>
            {
                new ClinicService { Id = 1, Name = "Khám lâm sàng", Price = 150000, IsActive = true },
                new ClinicService { Id = 2, Name = "Tiêm vắc-xin dại", Price = 250000, IsActive = true }
            };

            _catalogRepoMock.Setup(repo => repo.GetActiveServicesAsync()).ReturnsAsync(mockServices);

            // Act
            var result = await _catalogService.GetActiveServicesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Khám lâm sàng");
            _catalogRepoMock.Verify(repo => repo.GetActiveServicesAsync(), Times.Once);
        }
    }
}
```
