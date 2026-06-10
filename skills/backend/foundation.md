# 🟢 Level 1: Foundation (Nền Tảng) - Backend Skills

Bao gồm các kiến thức và kỹ năng cơ bản bắt buộc đối với tất cả thành viên phát triển Backend của dự án MyPetClinic.

---

## 📋 Danh Sách Kỹ Năng
1. [BE-F01: C# Fundamentals](#be-f01-c-fundamentals)
2. [BE-F02: OOP & SOLID Principles](#be-f02-oop--solid-principles)
3. [BE-F03: Async/Await & Task Parallel Library](#be-f03-asyncawait--task-parallel-library)

---

### BE-F01: C# Fundamentals

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 1 |
| **Tại sao cần?** | Là ngôn ngữ chính của toàn bộ Backend. Mọi logic nghiệp vụ đều viết bằng C#. |

<details>
<summary><b>📚 Kiến thức cần nắm (Click để mở rộng)</b></summary>

**Cú pháp cơ bản:**
```csharp
// Biến & Kiểu dữ liệu
string name = "Buddy";
int age = 3;
decimal weight = 12.5m;
DateTime appointmentDate = DateTime.UtcNow;
bool isVaccinated = true;

// Nullable types
int? microchipNumber = null;

// String interpolation
var message = $"Thú cưng {name} nặng {weight}kg";
```

**Cấu trúc điều khiển:**
```csharp
// Pattern Matching (C# 8+)
string GetPetSize(decimal weight) => weight switch
{
    <= 5 => "Nhỏ",
    <= 15 => "Trung bình",
    _ => "Lớn"
};

// Null-coalescing operators
var displayName = pet.Nickname ?? pet.Name;
var ageText = pet.Age?.ToString() ?? "Chưa rõ";
```

**Collections & LINQ:**
```csharp
// LINQ cơ bản
var activePets = pets.Where(p => p.IsActive);
var petNames = pets.Select(p => p.Name);
var groupedBySpecies = pets.GroupBy(p => p.Species);
var hasDog = pets.Any(p => p.Species == "Chó");

// Deferred Execution (quan trọng!)
var query = dbContext.Pets.Where(p => p.Age > 5); // Chưa thực thi
var result = await query.ToListAsync(); // Mới thực thi
```

</details>

**Bài tập thực hành:**
1. Viết hàm tính tuổi thú cưng từ ngày sinh, trả về định dạng "X năm Y tháng".
2. Sử dụng LINQ để lọc danh sách thú cưng theo loài và độ tuổi.
3. Xử lý null an toàn với Nullable Reference Types (NRT).

---

### BE-F02: OOP & SOLID Principles

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 1-2 |
| **Tại sao cần?** | Clean Architecture yêu cầu thiết kế hướng đối tượng tốt, tuân thủ SOLID để code dễ bảo trì. |

<details>
<summary><b>📚 Áp dụng vào dự án (Click để mở rộng)</b></summary>

**Single Responsibility Principle (SRP):**
```csharp
// ❌ Sai: Controller làm quá nhiều việc
[HttpPost]
public async Task<IActionResult> CreateBooking(BookingDto dto)
{
    // Validate
    if (dto.AppointmentDate < DateTime.Now)
        return BadRequest("Ngày không hợp lệ");
    
    // Business logic
    var slot = await _context.Slots.FindAsync(dto.SlotId);
    if (slot.AvailableSlots <= 0)
        return BadRequest("Hết slot");
    
    // Save
    var booking = new Booking { ... };
    _context.Bookings.Add(booking);
    
    // Send email
    await _emailService.SendConfirmationAsync(booking);
    
    await _context.SaveChangesAsync();
    return Ok(booking);
}

// ✅ Đúng: Tách riêng từng trách nhiệm
public class BookingService
{
    private readonly IValidator<BookingDto> _validator;
    private readonly ISlotChecker _slotChecker;
    private readonly IBookingRepository _bookingRepo;
    private readonly INotificationService _notificationService;
    
    public async Task<Result<Booking>> CreateBooking(BookingDto dto)
    {
        // 1. Validation
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return Result.Fail(validationResult.Errors);
        
        // 2. Business rule
        var slotAvailable = await _slotChecker.IsAvailable(dto.SlotId);
        if (!slotAvailable)
            return Result.Fail("Slot không còn trống");
        
        // 3. Persistence
        var booking = await _bookingRepo.AddAsync(dto.ToEntity());
        
        // 4. Notification (fire-and-forget)
        _ = _notificationService.SendConfirmationAsync(booking);
        
        return Result.Ok(booking);
    }
}
```

**Dependency Inversion Principle (DIP):**
```csharp
// Định nghĩa interface ở Domain layer
public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(Guid id);
    Task<List<Pet>> GetByOwnerIdAsync(Guid ownerId);
    Task<Pet> AddAsync(Pet pet);
}

// Implement ở Infrastructure layer
public class PetRepository : IPetRepository
{
    private readonly AppDbContext _context;
    
    public PetRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Pet?> GetByIdAsync(Guid id)
        => await _context.Pets.FindAsync(id);
}
```

</details>

---

### BE-F03: Async/Await & Task Parallel Library

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 1-6 |
| **Tại sao cần?** | Xử lý I/O operations (database, API calls) không block thread, tăng performance. |

<details>
<summary><b>📚 Best Practices (Click để mở rộng)</b></summary>

```csharp
// ✅ Đúng: Async all the way
public async Task<Booking> GetBookingAsync(Guid id)
{
    return await _context.Bookings
        .Include(b => b.Pet)
        .Include(b => b.Veterinarian)
        .FirstOrDefaultAsync(b => b.Id == id);
}

// ❌ Sai: Sync over Async (gây deadlock)
public Booking GetBooking(Guid id)
{
    return _context.Bookings
        .FirstOrDefaultAsync(b => b.Id == id)
        .GetAwaiter().GetResult(); // Có thể gây deadlock!
}

// ✅ Đúng: Chạy nhiều task song song
public async Task<DashboardDto> GetDashboardDataAsync()
{
    var todayAppointmentsTask = _bookingRepo.GetTodayAppointmentsAsync();
    var lowStockMedicinesTask = _medicineRepo.GetLowStockAsync();
    var revenueTask = _billingRepo.GetTodayRevenueAsync();
    
    await Task.WhenAll(todayAppointmentsTask, lowStockMedicinesTask, revenueTask);
    
    return new DashboardDto
    {
        TodayAppointments = todayAppointmentsTask.Result,
        LowStockMedicines = lowStockMedicinesTask.Result,
        TodayRevenue = revenueTask.Result
    };
}

// ✅ Đúng: Timeout cho external API calls
public async Task<string> GetAIChatResponse(string prompt)
{
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    try
    {
        return await _geminiService.GenerateAsync(prompt, cts.Token);
    }
    catch (OperationCanceledException)
    {
        return "Xin lỗi, AI đang bận. Vui lòng thử lại sau.";
    }
}
```

</details>
