# 🟠 Level 3: Advanced (Nâng Cao) - Backend Skills

Bao gồm các kiến thức và kỹ năng nâng cao của dự án MyPetClinic trong các Sprint từ 3 đến 5.

---

## 📋 Danh Sách Kỹ Năng
1. [BE-A01: Clean Architecture Implementation](#be-a01-clean-architecture-implementation)
2. [BE-A02: Repository Pattern & Unit of Work](#be-a02-repository-pattern--unit-of-work)
3. [BE-A03: Cookie-based Authentication & Google OAuth](#be-a03-cookie-based-authentication--google-oauth)

---

### BE-A01: Clean Architecture Implementation

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 3-5 |
| **Tại sao cần?** | Cấu trúc code được phân chia thành các Layer cô lập tốt giúp ứng dụng dễ mở rộng, bảo trì và viết Unit Test hiệu quả. |

<details>
<summary><b>📚 Cấu trúc thư mục chi tiết (Click để mở rộng)</b></summary>

```
backend/
├── src/
│   ├── MyPetClinic.Domain/           # Lớp Domain (Innermost)
│   │   ├── Entities/
│   │   │   ├── Pet.cs
│   │   │   ├── User.cs
│   │   │   ├── Booking.cs
│   │   │   ├── MedicalRecord.cs
│   │   │   └── Medicine.cs
│   │   ├── ValueObjects/
│   │   │   ├── Address.cs
│   │   │   ├── PhoneNumber.cs
│   │   │   └── Money.cs
│   │   ├── Enums/
│   │   │   ├── BookingStatus.cs
│   │   │   ├── UserRole.cs
│   │   │   └── MedicineUnit.cs
│   │   ├── DomainEvents/
│   │   │   ├── BookingCreatedEvent.cs
│   │   │   └── MedicineStockLowEvent.cs
│   │   └── Interfaces/
│   │       ├── IPetRepository.cs
│   │       ├── IBookingRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── MyPetClinic.Application/      # Lớp Application
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   ├── ICurrentUserService.cs
│   │   │   │   └── IEmailService.cs
│   │   │   ├── Behaviours/
│   │   │   │   ├── ValidationBehaviour.cs
│   │   │   │   └── PerformanceBehaviour.cs
│   │   │   └── Exceptions/
│   │   │       ├── NotFoundException.cs
│   │   │       └── BusinessRuleException.cs
│   │   ├── DTOs/
│   │   │   ├── PetDto.cs
│   │   │   ├── BookingDto.cs
│   │   │   └── MedicalRecordDto.cs
│   │   ├── Validators/
│   │   │   ├── CreateBookingValidator.cs
│   │   │   └── CreatePetValidator.cs
│   │   ├── Services/
│   │   │   ├── IPetService.cs
│   │   │   ├── PetService.cs
│   │   │   ├── IBookingService.cs
│   │   │   └── BookingService.cs
│   │   └── Mapping/
│   │       └── MappingProfile.cs
│   │
│   ├── MyPetClinic.Infrastructure/   # Lớp Infrastructure
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── PetConfiguration.cs
│   │   │   │   └── BookingConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── PetRepository.cs
│   │   │   └── BookingRepository.cs
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   ├── GeminiAIService.cs
│   │   │   └── FileStorageService.cs
│   │   └── BackgroundJobs/
│   │       └── VaccinationReminderJob.cs
│   │
│   └── MyPetClinic.WebApi/           # Lớp Presentation
│       ├── Controllers/
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       ├── Filters/
│       │   └── ApiExceptionFilter.cs
│       └── Program.cs
```

</details>

---

### BE-A02: Repository Pattern & Unit of Work

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 3-5 |
| **Tại sao cần?** | Quản lý tốt Transaction Database khi thay đổi nhiều Entity trong một Use Case nghiệp vụ phức tạp. |

<details>
<summary><b>📚 Implementation (Click để mở rộng)</b></summary>

```csharp
// Interface ở Domain Layer
public interface IUnitOfWork : IDisposable
{
    IPetRepository Pets { get; }
    IBookingRepository Bookings { get; }
    IMedicalRecordRepository MedicalRecords { get; }
    IMedicineRepository Medicines { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// Implementation ở Infrastructure Layer
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IPetRepository? _pets;
    private IBookingRepository? _bookings;
    
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }
    
    public IPetRepository Pets => 
        _pets ??= new PetRepository(_context);
    
    public IBookingRepository Bookings => 
        _bookings ??= new BookingRepository(_context);
    
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
    
    public void Dispose() => _context.Dispose();
}

// Sử dụng trong Service
public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result<BookingDto>> CreateBookingWithPrescription(
        BookingDto bookingDto, 
        List<PrescriptionItemDto> medicines)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // 1. Tạo booking
            var booking = await _unitOfWork.Bookings.AddAsync(bookingDto.ToEntity());
            
            // 2. Trừ kho thuốc
            foreach (var item in medicines)
            {
                var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.MedicineId);
                if (medicine.StockQuantity < item.Quantity)
                    throw new BusinessRuleException($"Thuốc {medicine.Name} không đủ số lượng");
                
                medicine.StockQuantity -= item.Quantity;
                _unitOfWork.Medicines.Update(medicine);
            }
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return Result.Ok(booking.ToDto());
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

</details>

---

### BE-A03: Cookie-based Authentication & Google OAuth

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 1, 4 |
| **Tại sao cần?** | Bảo mật API endpoint bằng Cookie (HttpOnly, SameSite, Secure) cho Single Page Application (SPA), tích hợp đăng nhập qua Google OAuth 2.0 và phân quyền chi tiết cho Admin, Bác sĩ (doctor), Lễ tân (receptionist), và Khách hàng (customer). |

<details>
<summary><b>📚 Cookie & Google OAuth Configuration (Click để mở rộng)</b></summary>

```csharp
// Đăng ký Authentication trong Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });

// Authorization dựa trên vai trò (Roles) trong Controller
[HttpGet("admin/dashboard")]
[Authorize(Roles = "admin")]
public IActionResult GetAdminDashboard()
{
    return Ok(new { Message = "Chào mừng Admin" });
}

// Custom Policy-based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageSchedule", policy =>
        policy.RequireRole("admin", "receptionist"));
});

[HttpGet("schedules")]
[Authorize(Policy = "CanManageSchedule")]
public IActionResult GetSchedules()
{
    return Ok();
}
```

</details>

---

### BE-A04: Centralized IDOR Prevention using ActionFilters

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 5 |
| **Tại sao cần?** | Chặn đứng 100% các cuộc tấn công IDOR bằng cách kiểm tra quyền sở hữu của user đối với tài nguyên trước khi vào luồng xử lý Service, giữ cho Application Layer sạch sẽ. |

<details>
<summary><b>📚 ActionFilter Implementation Details (Click để mở rộng)</b></summary>

Để bảo vệ các tài nguyên nhạy cảm như thú cưng, bệnh án hoặc hóa đơn khỏi IDOR, chúng ta tạo một ActionFilter tùy chỉnh:

```csharp
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MyPetClinic.Application.Interfaces.Repositories;

namespace MyPetClinic.WebApi.Filters
{
    public class AuthorizeOwnerAttribute : TypeFilterAttribute
    {
        public AuthorizeOwnerAttribute() : base(typeof(AuthorizeOwnerFilter))
        {
        }

        private class AuthorizeOwnerFilter : IAsyncActionFilter
        {
            private readonly IPetRepository _petRepository;

            public AuthorizeOwnerFilter(IPetRepository petRepository)
            {
                _petRepository = petRepository;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var userIdStr = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    context.Result = new UnauthorizedObjectResult(new { message = "Không xác thực người dùng." });
                    return;
                }

                if (context.RouteData.Values.TryGetValue("id", out var idVal) && idVal != null)
                {
                    if (long.TryParse(idVal.ToString(), out long petId))
                    {
                        var pet = await _petRepository.GetPetByIdAsync(petId);
                        if (pet == null)
                        {
                            context.Result = new NotFoundObjectResult(new { message = "Không tìm thấy thú cưng." });
                            return;
                        }

                        if (pet.OwnerId != userId)
                        {
                            context.Result = new ObjectResult(new { message = "Bạn không có quyền truy cập thú cưng này." }) { StatusCode = 403 };
                            return;
                        }
                    }
                }

                await next();
            }
        }
    }
}
```

Sử dụng trong Controller:
```csharp
[HttpDelete("{id}")]
[AuthorizeOwner]
public async Task<IActionResult> DeletePet(long id)
{
    // Không cần check IDOR thủ công tại đây nữa! ActionFilter đã xử lý
    await _petService.DeletePetAsync(id, GetCurrentUserId());
    return Ok();
}
```

</details>

