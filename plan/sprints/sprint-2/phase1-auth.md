# ⚙️ Đặc Tả Kỹ Thuật Chi Tiết - Account Authentication (Sprint 2)

Tài liệu này đặc tả chi tiết mã nguồn, cấu trúc lớp, sơ đồ dữ liệu, API contracts và kịch bản unit test xác thực cho **Sprint 2** của dự án **MyPetClinic**.

---

## 1. Thiết Kế Cơ Sở Dữ Liệu & Thực Thể (Entities & EF Core Configuration)

Sprint 2 tập trung vào hai thực thể cốt lõi phục vụ đăng ký/đăng nhập là `User` và `Role`.

### 1.1. Thực Thể Domain (C# Domain Entities)

```csharp
// Location: Domain/Entities/Role.cs
namespace MyPetClinic.Domain.Entities
{
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
```

```csharp
// Location: Domain/Entities/User.cs
using System;

namespace MyPetClinic.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long RoleId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public short? Gender { get; set; } // 0: Nam, 1: Nữ, 2: Khác
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; } // Hỗ trợ Soft Delete

        // Navigation properties
        public virtual Role? Role { get; set; }
    }
}
```

### 1.2. Cấu Hình Fluent API & Ràng Buộc Dữ Liệu (Infrastructure Configurations)

```csharp
// Location: Infrastructure/Data/Configurations/RoleConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        }
    }
}
```

```csharp
// Location: Infrastructure/Data/Configurations/UserConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(255);
            builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            builder.Property(u => u.Phone).HasMaxLength(20);
            builder.Property(u => u.PasswordHash).IsRequired();
            
            // Soft Delete Global Filter
            builder.HasQueryFilter(u => u.DeletedAt == null);

            // Indexes phục vụ tìm kiếm tối ưu
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.RoleId);

            // Cấu hình mối quan hệ khóa ngoại
            builder.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
```

---

## 2. Giao Ước API & Nghiệp Vụ Xác Thực (API Contracts & Business Services)

### 2.1. Cấu Trúc Repositories & Interfaces (DIP)

```csharp
// Location: Application/Common/Interfaces/IUserRepository.cs
using MyPetClinic.Domain.Entities;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<bool> ExistsByEmailAsync(string email);
        Task UpdateAsync(User user);
    }
}
```

```csharp
// Location: Application/Common/Interfaces/ITokenService.cs
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}
```

### 2.2. Chi Tiết Lớp Nghiệp Vụ Xác Thực (AuthService.cs)

```csharp
// Location: Application/Services/AuthService.cs
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> RegisterAsync(string fullName, string email, string password, string? phone)
        {
            if (await _userRepository.ExistsByEmailAsync(email))
            {
                throw new ArgumentException("Email đã tồn tại trong hệ thống.");
            }

            // Băm mật khẩu sử dụng BCrypt với Work Factor = 11 bảo mật cao
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
            
            var user = new User
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                PasswordHash = passwordHash,
                RoleId = 4, // Gán vai trò mặc định là Customer
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return "Đăng ký tài khoản thành công!";
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Tài khoản hoặc mật khẩu không chính xác.");
            }

            return _tokenService.GenerateJwtToken(user);
        }
    }
}
```

### 2.3. Khai Báo API Endpoints (AccountsController)

```csharp
// Location: WebApi/Controllers/AccountsController.cs
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly AuthService _authService;

        public AccountsController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request.FullName, request.Email, request.Password, request.Phone);
                return Ok(new { Message = result });
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request.Email, request.Password);
                return Ok(new { Token = token });
            }
            catch (System.UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }
    }

    public record RegisterRequest(string FullName, string Email, string Password, string? Phone);
    public record LoginRequest(string Email, string Password);
}
```

---

## 3. Quản Lý Trạng Thái Phía Client-side SPA (Vue 3 Pinia Store)

### 3.1. Thiết Lập Axios Interceptors (Tự Động Chèn Token)

```typescript
// Location: frontend/src/utils/api.ts
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1',
  timeout: 10000,
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('access_token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default api;
```

### 3.2. Khai Báo Store Quản Lý Xác Thực (Pinia Store)

```typescript
// Location: frontend/src/stores/auth.ts
import { defineStore } from 'pinia';
import api from '../utils/api';
import { jwtDecode } from 'jwt-decode';

interface UserState {
  id: string;
  email: string;
  role: string;
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('access_token') || null as string | null,
    user: null as UserState | null,
    loading: false,
    error: null as string | null
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
    userRole: (state) => state.user?.role || null,
  },
  actions: {
    initAuth() {
      if (this.token) {
        try {
          const decoded: any = jwtDecode(this.token);
          this.user = {
            id: decoded.sub,
            email: decoded.email,
            role: decoded.role,
          };
        } catch {
          this.logout();
        }
      }
    },
    async login(payload: any) {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.post('/accounts/login', payload);
        const token = response.data.token;
        this.token = token;
        localStorage.setItem('access_token', token);
        this.initAuth();
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Đăng nhập thất bại.';
        throw err;
      } finally {
        this.loading = false;
      }
    },
    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem('access_token');
    }
  }
});
```

---

## 4. Kịch Bản Kiểm Thử Xác Thực (xUnit Tests)

Các ca kiểm thử tự động kiểm tra logic đăng ký trùng email và băm mật khẩu:

```csharp
// Location: MyPetClinic.Tests/Application/AuthServiceTests.cs
using FluentAssertions;
using Moq;
using MyPetClinic.Application.Common.Interfaces;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests.Application
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _authService = new AuthService(_userRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var email = "duplicate@mypetclinic.com";
            _userRepoMock.Setup(repo => repo.ExistsByEmailAsync(email)).ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync("Full Name", email, "password123", "0987654321");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Email đã tồn tại trong hệ thống.");
            _userRepoMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenInputIsValid()
        {
            // Arrange
            var email = "newuser@mypetclinic.com";
            _userRepoMock.Setup(repo => repo.ExistsByEmailAsync(email)).ReturnsAsync(false);

            // Act
            var result = await _authService.RegisterAsync("Full Name", email, "password123", "0987654321");

            // Assert
            result.Should().Be("Đăng ký tài khoản thành công!");
            _userRepoMock.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Email == email && u.FullName == "Full Name")), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
```
