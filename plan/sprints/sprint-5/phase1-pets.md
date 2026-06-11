# ⚙️ Đặc Tả Kỹ Thuật Chi Tiết - Pet Portfolio Management (Sprint 5)

Tài liệu này đặc tả chi tiết mã nguồn, cấu trúc lớp, sơ đồ dữ liệu, API contracts và kịch bản unit test bảo mật cho **Sprint 5** của dự án **MyPetClinic**.

---

## 1. Thiết Kế Cơ Sở Dữ Liệu & Thực Thể (Entities & EF Core Configuration)

Sprint 5 tập trung vào quản lý hồ sơ thú cưng (`Pets`) liên kết với chủ nuôi.

### 1.1. Thực Thể Domain (C# Domain Entity)

```csharp
// Location: Domain/Entities/Pet.cs
using System;

namespace MyPetClinic.Domain.Entities
{
    public class Pet
    {
        public long Id { get; set; }
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Species { get; set; } // Chó, mèo...
        public string? Breed { get; set; } // Giống
        public short? Gender { get; set; } // 0: Đực, 1: Cái
        public DateTime? BirthDate { get; set; }
        public decimal? Weight { get; set; }
        public string? Color { get; set; }
        public string? BloodType { get; set; }
        public bool? Sterilized { get; set; }
        public string? MicrochipCode { get; set; }
        public string? AllergyNote { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        // Navigation property
        public virtual User? Owner { get; set; }
    }
}
```

### 1.2. Cấu Hình Fluent API & Soft Delete (Infrastructure Configurations)

```csharp
// Location: Infrastructure/Data/Configurations/PetConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Data.Configurations
{
    public class PetConfiguration : IEntityTypeConfiguration<Pet>
    {
        public void Configure(EntityTypeBuilder<Pet> builder)
        {
            builder.ToTable("Pets");
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Name).IsRequired().HasMaxLength(255);
            builder.Property(p => p.Species).HasMaxLength(100);
            builder.Property(p => p.Breed).HasMaxLength(100);
            builder.Property(p => p.Weight).HasPrecision(5, 2);
            
            // Bộ lọc Soft Delete Global Filter
            builder.HasQueryFilter(p => p.DeletedAt == null);
            builder.HasIndex(p => p.OwnerId);

            builder.HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

---

## 2. Giao Ước API & Chống Tấn Công IDOR (IDOR Prevention & Business Services)

Để chặn đứng nguy cơ tấn công IDOR, mọi truy vấn chỉnh sửa dữ liệu thú cưng đều phải đối chiếu `OwnerId == currentUserId` phân tích từ JWT.

### 2.1. Cấu Trúc Repositories & Interfaces

```csharp
// Location: Application/Common/Interfaces/IPetRepository.cs
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Common.Interfaces
{
    public interface IPetRepository
    {
        Task<Pet?> GetByIdAsync(long id);
        Task<List<Pet>> GetByOwnerIdAsync(Guid ownerId);
        Task AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
    }
}
```

### 2.2. Lớp Dịch Vụ Nghiệp Vụ Hồ Sơ Thú Cưng (PetService.cs)

```csharp
// Location: Application/Services/PetService.cs
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class PetService
    {
        private readonly IPetRepository _petRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PetService(IPetRepository petRepository, IUnitOfWork unitOfWork)
        {
            _petRepository = petRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Pet>> GetPetsByOwnerAsync(Guid ownerId)
        {
            return await _petRepository.GetByOwnerIdAsync(ownerId);
        }

        public async Task<Pet> CreatePetAsync(Guid ownerId, CreatePetDto dto)
        {
            var pet = new Pet
            {
                OwnerId = ownerId,
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                Gender = dto.Gender,
                BirthDate = dto.BirthDate,
                Weight = dto.Weight,
                Color = dto.Color,
                AllergyNote = dto.AllergyNote
            };

            await _petRepository.AddAsync(pet);
            await _unitOfWork.SaveChangesAsync();
            return pet;
        }

        public async Task UpdatePetAsync(Guid currentUserId, long petId, UpdatePetDto dto)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null)
            {
                throw new KeyNotFoundException("Không tìm thấy thông tin thú cưng.");
            }

            // Chống IDOR: Chỉ chủ sở hữu mới có quyền thay đổi
            if (pet.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa hồ sơ thú cưng này.");
            }

            pet.Name = dto.Name;
            pet.Species = dto.Species;
            pet.Breed = dto.Breed;
            pet.Gender = dto.Gender;
            pet.BirthDate = dto.BirthDate;
            pet.Weight = dto.Weight;
            pet.Color = dto.Color;
            pet.AllergyNote = dto.AllergyNote;

            await _petRepository.UpdateAsync(pet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeletePetAsync(Guid currentUserId, long petId)
        {
            var pet = await _petRepository.GetByIdAsync(petId);
            if (pet == null) return;

            // Chống IDOR
            if (pet.OwnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa hồ sơ thú cưng này.");
            }

            pet.DeletedAt = DateTime.UtcNow; // Xóa mềm
            await _petRepository.UpdateAsync(pet);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public record CreatePetDto(string Name, string? Species, string? Breed, short? Gender, DateTime? BirthDate, decimal? Weight, string? Color, string? AllergyNote);
    public record UpdatePetDto(string Name, string? Species, string? Breed, short? Gender, DateTime? BirthDate, decimal? Weight, string? Color, string? AllergyNote);
}
```

### 2.3. Khai Báo API Endpoints (PetsController)

```csharp
// Location: WebApi/Controllers/PetsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/my-pets")]
    [Authorize(Roles = "Customer")]
    public class PetsController : ControllerBase
    {
        private readonly PetService _petService;

        public PetsController(PetService petService)
        {
            _petService = petService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdVal = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdVal)) throw new UnauthorizedAccessException();
            return Guid.Parse(userIdVal);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPets()
        {
            var pets = await _petService.GetPetsByOwnerAsync(GetCurrentUserId());
            return Ok(pets);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePet([FromBody] CreatePetDto request)
        {
            var pet = await _petService.CreatePetAsync(GetCurrentUserId(), request);
            return CreatedAtAction(nameof(GetMyPets), new { id = pet.Id }, pet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(long id, [FromBody] UpdatePetDto request)
        {
            try
            {
                await _petService.UpdatePetAsync(GetCurrentUserId(), id, request);
                return Ok(new { Message = "Cập nhật hồ sơ thú cưng thành công!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(long id)
        {
            try
            {
                await _petService.DeletePetAsync(GetCurrentUserId(), id);
                return Ok(new { Message = "Xóa hồ sơ thú cưng thành công!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
```

---

## 3. Quản Lý Trạng Thái Phía Client-side SPA (Vue 3 Pinia Store)

```typescript
// Location: frontend/src/stores/pet.ts
import { defineStore } from 'pinia';
import api from '../utils/api';

export interface Pet {
  id: number;
  name: string;
  species: string;
  breed: string;
  gender: number;
  birthDate: string;
  weight: number;
  color: string;
  allergyNote: string;
}

export const usePetStore = defineStore('pet', {
  state: () => ({
    pets: [] as Pet[],
    loading: false,
    error: null as string | null
  }),
  actions: {
    async fetchMyPets() {
      this.loading = true;
      try {
        const response = await api.get('/my-pets');
        this.pets = response.data;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể tải danh sách thú cưng.';
      } finally {
        this.loading = false;
      }
    },
    async addPet(petData: Partial<Pet>) {
      const response = await api.post('/my-pets', petData);
      this.pets.push(response.data);
    },
    async updatePet(id: number, petData: Partial<Pet>) {
      await api.put(`/my-pets/${id}`, petData);
      const index = this.pets.findIndex(p => p.id === id);
      if (index !== -1) {
        this.pets[index] = { ...this.pets[index], ...petData };
      }
    },
    async deletePet(id: number) {
      await api.delete(`/my-pets/${id}`);
      this.pets = this.pets.filter(p => p.id !== id);
    }
  }
});
```

---

## 4. Kịch Bản Kiểm Thử Bảo Mật Chặn IDOR (xUnit Tests)

```csharp
// Location: MyPetClinic.Tests/Application/PetServiceTests.cs
using FluentAssertions;
using Moq;
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests.Application
{
    public class PetServiceTests
    {
        private readonly Mock<IPetRepository> _petRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly PetService _petService;

        public PetServiceTests()
        {
            _petRepoMock = new Mock<IPetRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _petService = new PetService(_petRepoMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldThrowUnauthorizedException_WhenUserIsNotOwner()
        {
            // Arrange
            var userOwnerId = Guid.NewGuid();
            var attackerUserId = Guid.NewGuid(); // Tài khoản tấn công
            var petId = 123L;

            var existingPet = new Pet
            {
                Id = petId,
                OwnerId = userOwnerId, // Thú cưng của User A
                Name = "Milo"
            };

            _petRepoMock.Setup(repo => repo.GetByIdAsync(petId)).ReturnsAsync(existingPet);
            var updateDto = new UpdatePetDto("Milo New", "Chó", "Corgi", 0, null, 10.5m, "Vàng", null);

            // Act
            Func<Task> act = async () => await _petService.UpdatePetAsync(attackerUserId, petId, updateDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Bạn không có quyền chỉnh sửa hồ sơ thú cưng này.");
                
            _petRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Pet>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdatePetAsync_ShouldUpdate_WhenUserIsOwner()
        {
            // Arrange
            var userOwnerId = Guid.NewGuid();
            var petId = 123L;

            var existingPet = new Pet
            {
                Id = petId,
                OwnerId = userOwnerId,
                Name = "Milo"
            };

            _petRepoMock.Setup(repo => repo.GetByIdAsync(petId)).ReturnsAsync(existingPet);
            var updateDto = new UpdatePetDto("Milo New", "Chó", "Corgi", 0, null, 10.5m, "Vàng", null);

            // Act
            await _petService.UpdatePetAsync(userOwnerId, petId, updateDto);

            // Assert
            existingPet.Name.Should().Be("Milo New");
            _petRepoMock.Verify(repo => repo.UpdateAsync(existingPet), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
```
