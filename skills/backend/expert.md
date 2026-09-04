# 🔴 Level 4: Expert (Chuyên Gia) - Backend Skills

Bao gồm các kiến thức và kỹ năng cấp độ chuyên gia trong các Sprint 5 và 6 của dự án MyPetClinic.

---

## 📋 Danh Sách Kỹ Năng
1. [BE-E01: Background Services với Hangfire](#be-e01-background-services-v%E1%BB%9Bi-hangfire)
2. [BE-E02: Unit Testing với xUnit + Moq](#be-e02-unit-testing-v%E1%BB%9Bi-xunit--moq)
3. [BE-E03: Integration với AI Chatbot (Groq API) & OTP/MemoryCache](#be-e03-integration-v%E1%BB%9Bi-ai-chatbot-groq-api--otpmemorycache)

---

### BE-E01: Background Services với Hangfire

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐ Intermediate |
| **Sprint** | Sprint 6 |
| **Tại sao cần?** | Thực thi các tác vụ chạy nền định kỳ hoặc tốn nhiều thời gian (như gửi email nhắc lịch tiêm chủng, dọn dẹp log, cảnh báo hết thuốc) mà không chặn request chính. |

<details>
<summary><b>📚 Vaccination Reminder Job (Click để mở rộng)</b></summary>

```csharp
// Đăng ký Hangfire
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Job Service
public class VaccinationReminderService
{
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;
    
    // Chạy hàng ngày lúc 8:00 AM
    [AutomaticRetry(Attempts = 3)]
    public async Task SendVaccinationReminders()
    {
        var threeDaysFromNow = DateTime.UtcNow.AddDays(3).Date;
        var fiveDaysFromNow = DateTime.UtcNow.AddDays(5).Date;
        
        // Tìm các mũi tiêm cần tái chủng trong 3-5 ngày tới
        var upcomingVaccinations = await _context.VaccinationRecords
            .Include(v => v.Pet)
                .ThenInclude(p => p.Owner)
            .Where(v => v.NextDueDate >= threeDaysFromNow 
                     && v.NextDueDate <= fiveDaysFromNow
                     && !v.ReminderSent)
            .ToListAsync();
        
        foreach (var vaccination in upcomingVaccinations)
        {
            await _emailService.SendEmailAsync(
                to: vaccination.Pet.Owner.Email,
                subject: $"🔔 Nhắc lịch tái chủng cho {vaccination.Pet.Name}",
                body: $@"
                    <h2>Thông báo nhắc lịch tái chủng</h2>
                    <p>Kính gửi {vaccination.Pet.Owner.FullName},</p>
                    <p>Thú cưng <strong>{vaccination.Pet.Name}</strong> của bạn 
                    cần được tái chủng vaccine <strong>{vaccination.VaccineName}</strong> 
                    vào ngày <strong>{vaccination.NextDueDate:dd/MM/yyyy}</strong>.</p>
                    <p>Vui lòng đặt lịch tại: <a href='https://mypetclinic.com/booking'>Đặt lịch ngay</a></p>
                ");
            
            vaccination.ReminderSent = true;
        }
        
        await _context.SaveChangesAsync();
    }
}

// Schedule job trong Program.cs
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

RecurringJob.AddOrUpdate<VaccinationReminderService>(
    "vaccination-reminder",
    service => service.SendVaccinationReminders(),
    "0 8 * * *", // Cron expression: 8:00 AM mỗi ngày
    TimeZoneInfo.Local);
```

</details>

---

### BE-E02: Unit Testing với xUnit + Moq

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐ Intermediate |
| **Sprint** | Sprint 1-6 |
| **Tại sao cần?** | Đảm bảo tính đúng đắn của code nghiệp vụ, ngăn chặn regression bug khi refactor hệ thống. Mục tiêu Coverage > 70%. |

<details>
<summary><b>📚 Test Example (Click để mở rộng)</b></summary>

```csharp
public class BookingServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IValidator<BookingDto>> _mockValidator;
    private readonly BookingService _sut; // System Under Test
    
    public BookingServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockValidator = new Mock<IValidator<BookingDto>>();
        _sut = new BookingService(_mockUnitOfWork.Object, _mockValidator.Object);
    }
    
    [Fact]
    public async Task CreateBooking_ValidData_ShouldCreateBookingSuccessfully()
    {
        // Arrange
        var bookingDto = new BookingDto
        {
            PetId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid()
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(bookingDto, default))
            .ReturnsAsync(new ValidationResult());
        
        _mockUnitOfWork
            .Setup(u => u.Bookings.AddAsync(It.IsAny<Booking>()))
            .ReturnsAsync(new Booking { Id = Guid.NewGuid() });
        
        // Act
        var result = await _sut.CreateBooking(bookingDto);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
    
    [Fact]
    public async Task CreateBooking_SlotFull_ShouldReturnError()
    {
        // Arrange
        var bookingDto = new BookingDto { SlotId = Guid.NewGuid() };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(bookingDto, default))
            .ReturnsAsync(new ValidationResult());
        
        _mockUnitOfWork
            .Setup(u => u.Bookings.IsSlotAvailable(bookingDto.SlotId))
            .ReturnsAsync(false);
        
        // Act
        var result = await _sut.CreateBooking(bookingDto);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal("Slot không còn trống", result.Errors[0].Message);
    }
    
    [Theory]
    [InlineData("2024-01-01")] // Quá khứ
    [InlineData("2024-12-25")] // Ngày lễ (nếu không làm việc)
    public async Task CreateBooking_InvalidDate_ShouldReturnValidationError(string dateString)
    {
        // Arrange
        var bookingDto = new BookingDto 
        { 
            AppointmentDate = DateTime.Parse(dateString) 
        };
        
        var validationResult = new ValidationResult(
            new[] { new ValidationFailure("AppointmentDate", "Ngày không hợp lệ") });
        
        _mockValidator
            .Setup(v => v.ValidateAsync(bookingDto, default))
            .ReturnsAsync(validationResult);
        
        // Act
        var result = await _sut.CreateBooking(bookingDto);
        
        // Assert
        Assert.True(result.IsFailed);
    }
}
```

</details>

---

### BE-E03: Integration với AI Chatbot (Groq API) & OTP/MemoryCache

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 5-6 |
| **Tại sao cần?** | Tích hợp tính năng Chatbot tư vấn y tế cho phòng khám sử dụng Groq API (model Llama 3.3) và hệ thống xác thực OTP lưu trữ tạm thời trong memory cache. |

<details>
<summary><b>📚 AI & OTP Implementation (Click để mở rộng)</b></summary>

**1. Cấu hình AI Chatbot Service với Groq API:**
```csharp
public class AiChatbotService : IAiChatbotService
{
    private readonly string _apiKey;
    private static readonly HttpClient _httpClient = new HttpClient();

    public AiChatbotService(IConfiguration config)
    {
        // Sử dụng cấu hình API Key lưu trong appsettings.json
        _apiKey = config["GeminiAI:ApiKey"] ?? string.Empty;
    }

    public async Task<string> ChatAsync(string userMessage)
    {
        string url = "https://api.groq.com/openai/v1/chat/completions";

        var requestBody = new
        {
            model = "openai/gpt-oss-20b",
            messages = new[]
            {
                new { role = "system", content = "Bạn là một trợ lý ảo tư vấn y tế cho phòng khám thú y MyPetClinic. Hãy trả lời bằng tiếng Việt, thật ngắn gọn, thân thiện và chuyên nghiệp." },
                new { role = "user", content = userMessage }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");
        requestMessage.Content = content;

        var response = await _httpClient.SendAsync(requestMessage);
        var responseString = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Groq API Error ({response.StatusCode}): {responseString}");
        }

        using var jsonDoc = JsonDocument.Parse(responseString);
        var root = jsonDoc.RootElement;
        
        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var firstChoice = choices[0];
            if (firstChoice.TryGetProperty("message", out var messageElement) && 
                messageElement.TryGetProperty("content", out var contentElement))
            {
                return contentElement.GetString() ?? "Không có nội dung phản hồi.";
            }
        }

        return "Xin lỗi, tôi không thể xử lý câu trả lời lúc này.";
    }
}
```

**2. OTP Verification & MemoryCache:**
```csharp
// Đăng ký Memory Cache trong Program.cs
builder.Services.AddMemoryCache();

// OTP Service sử dụng Memory Cache lưu trữ mã OTP trong 5 phút
public class OtpService : IOtpService
{
    private readonly IMemoryCache _cache;
    private readonly Random _random = new Random();

    public OtpService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateOtp(string email)
    {
        var otp = _random.Next(100000, 999999).ToString();
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        
        _cache.Set($"OTP_{email}", otp, cacheOptions);
        return otp;
    }

    public bool VerifyOtp(string email, string otp)
    {
        if (_cache.TryGetValue($"OTP_{email}", out string? cachedOtp))
        {
            if (cachedOtp == otp)
            {
                _cache.Remove($"OTP_{email}"); // Xóa OTP sau khi verify thành công
                return true;
            }
        }
        return false;
    }
}
```

</details>
