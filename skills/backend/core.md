# 🟡 Level 2: Core (Cốt Lõi) - Backend Skills

Bao gồm các kiến thức và kỹ năng cốt lõi bắt buộc đối với tất cả thành viên phát triển Backend của dự án MyPetClinic trong các Sprint 1-2.

---

## 📋 Danh Sách Kỹ Năng
1. [BE-C01: ASP.NET Core Web API](#be-c01-aspnet-core-web-api)
2. [BE-C02: Entity Framework Core (EF Core)](#be-c02-entity-framework-core-ef-core)

---

### BE-C01: ASP.NET Core Web API

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 1-2 |
| **Tại sao cần?** | API là phương thức giao tiếp chính giữa Frontend và Backend. Cần nắm vững cấu trúc Routing, Controller, Middleware và Dependency Injection. |

<details>
<summary><b>📚 Cấu trúc Controller chuẩn (Click để mở rộng)</b></summary>

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PetsController : ControllerBase
{
    private readonly IPetService _petService;
    private readonly ILogger<PetsController> _logger;
    
    public PetsController(IPetService petService, ILogger<PetsController> logger)
    {
        _petService = petService;
        _logger = logger;
    }
    
    /// <summary>
    /// Lấy danh sách thú cưng của người dùng hiện tại
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(List<PetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _petService.GetPetsByOwnerAsync(Guid.Parse(userId!), page, pageSize);
        return Ok(result);
    }
    
    /// <summary>
    /// Tạo hồ sơ thú cưng mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePet([FromBody] CreatePetRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _petService.CreatePetAsync(Guid.Parse(userId!), request);
        
        return CreatedAtAction(
            nameof(GetPetById),
            new { id = result.Id },
            result);
    }
}
```

**Middleware Pipeline:**
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Đăng ký services theo thứ tự quan trọng
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IPetService, PetService>();

var app = builder.Build();

// Middleware pipeline - THỨ TỰ RẤT QUAN TRỌNG!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication(); // Trước Authorization
app.UseAuthorization();
app.UseMiddleware<GlobalExceptionMiddleware>(); // Custom middleware
app.MapControllers();

app.Run();
```

</details>

---

### BE-C02: Entity Framework Core (EF Core)

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 1-5 |
| **Tại sao cần?** | Database mapping và các thao tác truy vấn dữ liệu hiệu năng cao là nền tảng quản trị dữ liệu của hệ thống. |

<details>
<summary><b>📚 Fluent API Configuration (Click để mở rộng)</b></summary>

```csharp
public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("Pets");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.Species)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.Breed)
            .HasMaxLength(100);
        
        builder.Property(p => p.DateOfBirth)
            .IsRequired();
        
        builder.Property(p => p.Weight)
            .HasPrecision(5, 2); // decimal(5,2)
        
        // Relationship với Owner (User)
        builder.HasOne(p => p.Owner)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship với MedicalRecords
        builder.HasMany(p => p.MedicalRecords)
            .WithOne(m => m.Pet)
            .HasForeignKey(m => m.PetId);
        
        // Index cho tìm kiếm nhanh
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => new { p.Name, p.Species });
    }
}
```

**Query Optimization:**
```csharp
// ❌ Truy vấn chậm: Lấy tất cả columns + related data không cần thiết
var pets = await _context.Pets
    .Include(p => p.Owner)
    .Include(p => p.MedicalRecords)
    .ToListAsync();

// ✅ Truy vấn tối ưu: Chỉ lấy columns cần, dùng Projection
var petDtos = await _context.Pets
    .Where(p => p.OwnerId == ownerId)
    .Select(p => new PetDto
    {
        Id = p.Id,
        Name = p.Name,
        Species = p.Species,
        Age = EF.Functions.DateDiffYear(p.DateOfBirth, DateTime.UtcNow),
        LastVisit = p.MedicalRecords
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => m.CreatedAt)
            .FirstOrDefault()
    })
    .AsNoTracking() // Read-only -> Performance tốt hơn
    .ToListAsync();
```

</details>
