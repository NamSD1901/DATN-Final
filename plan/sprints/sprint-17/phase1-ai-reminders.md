# 🛠️ Đặc Tả Kỹ Thuật: Tích Hợp Gemini AI & Tự Động Nhắc Lịch
## Thiết kế AI Advisor Chatbot, Background Worker Quét Lịch Hẹn và Gửi Email Nhắc Nhở Tự Động

Tài liệu này đặc tả chi tiết kiến trúc tích hợp trí tuệ nhân tạo (Gemini API) và dịch vụ chạy ngầm tự động hóa thuộc **Sprint 17: Tích Hợp Gemini AI & Tự Động Nhắc Lịch**.

---

## 🤖 1. Tích Hợp Gemini AI SDK & Custom System Prompt

Hệ thống cung cấp một API Endpoint an toàn cho phép khách hàng trò chuyện với Trợ lý AI. Backend sẽ đứng vai trò trung gian gọi đến Google AI Studio nhằm bảo mật API Key.

### 1.1. `GeminiChatService.cs`

```csharp
using Google.GenerativeAI;
using Microsoft.Extensions.Configuration;

namespace MyPetClinic.Infrastructure.Services;

public class GeminiChatService : IGeminiChatService
{
    private readonly string _apiKey;
    private const string SystemInstruction = 
        "Bạn là trợ lý y tế thú y chuyên nghiệp của phòng khám MyPetClinic. " +
        "Chỉ trả lời các câu hỏi liên quan đến sức khỏe thú cưng, dinh dưỡng, cách chăm sóc và sơ cứu cơ bản. " +
        "Tuyệt đối từ chối trả lời các vấn đề chính trị, tôn giáo, lập trình hoặc câu hỏi không liên quan đến động vật. " +
        "Luôn khuyên chủ nuôi đưa thú cưng đến phòng khám nếu phát hiện triệu chứng nguy kịch.";

    public GeminiChatService(IConfiguration configuration)
    {
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Thiếu cấu hình API Key Gemini");
    }

    public async Task<string> AskAdvisorAsync(string userPrompt)
    {
        // Khởi tạo Client kết nối với mô hình gemini-2.5-flash
        var client = new GenerativeModel(modelName: "gemini-2.5-flash", apiKey: _apiKey);
        
        var chat = client.StartChat(new ChatSessionOptions
        {
            SystemInstruction = SystemInstruction
        });

        var response = await chat.SendMessageAsync(userPrompt);
        return response.Text ?? "Rất tiếc, tôi không thể xử lý câu hỏi này lúc này.";
    }
}
```

---

## ⏰ 2. Background Service Gửi Email Nhắc Lịch (`IHostedService`)

Dịch vụ chạy ngầm hoạt động liên tục trên ứng dụng Web Server, tự động kích hoạt mỗi ngày để quét các mũi tiêm chủng vắc-xin sắp đến ngày nhắc lại (`NextDueDate`) và gửi thông báo nhắc lịch cho khách hàng.

### 2.1. `VaccineReminderWorker.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces;

namespace MyPetClinic.Infrastructure.Workers;

public class VaccineReminderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VaccineReminderWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Chạy quét định kỳ mỗi 24 giờ

    public VaccineReminderWorker(IServiceProvider serviceProvider, ILogger<VaccineReminderWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Worker nhắc lịch vắc-xin đã khởi động.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendVaccineRemindersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong tiến trình chạy ngầm gửi email nhắc lịch.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    public async Task SendVaccineRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var targetDate = DateTime.UtcNow.Date.AddDays(3); // Nhắc lịch trước đúng 3 ngày

        // Lọc các mũi tiêm chủng có NextDueDate rơi vào ngày mục tiêu
        var upcomingVaccinations = await context.VaccinationRecords
            .Include(v => v.Pet)
            .ThenInclude(p => p!.Owner)
            .Where(v => v.NextDueDate.Date == targetDate && v.IsActive)
            .ToListAsync();

        foreach (var record in upcomingVaccinations)
        {
            if (record.Pet?.Owner != null)
            {
                var email = record.Pet.Owner.Email;
                var subject = $"🔔 Nhắc lịch tiêm chủng vắc-xin cho {record.Pet.Name}";
                var body = $"Chào {record.Pet.Owner.FullName},<br/>" +
                           $"Thú cưng <b>{record.Pet.Name}</b> của bạn có lịch tiêm nhắc lại mũi <b>{record.VaccineName}</b> vào ngày {record.NextDueDate:dd/MM/yyyy}. " +
                           $"Vui lòng đặt lịch trực tuyến hoặc mang thú cưng đến MyPetClinic để được hỗ trợ.";

                await emailService.SendEmailAsync(email, subject, body);
                _logger.LogInformation("Đã gửi email nhắc lịch tiêm vắc-xin cho {OwnerEmail} về thú cưng {PetName}", email, record.Pet.Name);
            }
        }
    }
}
```

---

## 🗄️ 3. Quản Lý Trạng Thái Giao Diện (Vue 3 Pinia Store)

Pinia Store `useChatStore` quản lý hội thoại thời gian thực của Bong bóng tư vấn AI.

### 3.1. `useChatStore.ts`

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

interface Message {
  sender: 'user' | 'ai';
  text: string;
  timestamp: Date;
}

export const useChatStore = defineStore('chat', {
  state: () => ({
    messages: [] as Message[],
    isLoading: false
  }),

  actions: {
    async sendMessage(prompt: string) {
      if (!prompt.trim()) return;

      // Thêm câu hỏi của user
      this.messages.push({
        sender: 'user',
        text: prompt,
        timestamp: new Date()
      });

      this.isLoading = true;
      try {
        const response = await axios.post('/api/ai/advisor', { prompt });
        this.messages.push({
          sender: 'ai',
          text: response.data.reply,
          timestamp: new Date()
        });
      } catch (error) {
        this.messages.push({
          sender: 'ai',
          text: 'Xin lỗi, kết nối của tôi bị gián đoạn. Vui lòng thử lại sau.',
          timestamp: new Date()
        });
      } finally {
        this.isLoading = false;
      }
    }
  }
});
```

---

## 🧪 4. Kịch Bản Kiểm Thử Unit Test (xUnit & FluentAssertions)

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Workers;
using MyPetClinic.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests;

public class VaccineReminderWorkerTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<VaccineReminderWorker>> _mockLogger;

    public VaccineReminderWorkerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<VaccineReminderWorker>>();
    }

    private (IApplicationDbContext, IServiceProvider) SetupMockServices(List<VaccinationRecord> records)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        context.VaccinationRecords.AddRange(records);
        context.SaveChanges();

        var serviceProviderMock = new Mock<IServiceProvider>();
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactoryMock.Object);
        serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(serviceScopeMock.Object);
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IApplicationDbContext)))
            .Returns(context);
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(IEmailService)))
            .Returns(_mockEmailService.Object);

        return (context, serviceProviderMock.Object);
    }

    [Fact]
    public async Task SendVaccineReminders_ShouldSendEmail_OnlyWhenNextDueDateIsExactlyThreeDaysAhead()
    {
        // Arrange
        var owner = new User { Id = "owner-1", FullName = "Nam Nguyen", Email = "nam@gmail.com" };
        var pet = new Pet { Id = 1, Name = "LuLu", Owner = owner };
        
        var records = new List<VaccinationRecord>
        {
            // Mũi tiêm đúng hạn nhắc nhở sau 3 ngày
            new VaccinationRecord 
            { 
                Id = 1, 
                VaccineName = "Rabies", 
                NextDueDate = DateTime.UtcNow.Date.AddDays(3), 
                Pet = pet,
                IsActive = true 
            },
            // Mũi tiêm chưa đến hạn nhắc nhở (còn 5 ngày)
            new VaccinationRecord 
            { 
                Id = 2, 
                VaccineName = "Parvo", 
                NextDueDate = DateTime.UtcNow.Date.AddDays(5), 
                Pet = pet,
                IsActive = true 
            }
        };

        var (_, serviceProvider) = SetupMockServices(records);
        var worker = new VaccineReminderWorker(serviceProvider, _mockLogger.Object);

        // Act
        await worker.SendVaccineRemindersAsync();

        // Assert
        _mockEmailService.Verify(e => e.SendEmailAsync(
            "nam@gmail.com", 
            It.Is<string>(s => s.Contains("LuLu")), 
            It.Is<string>(b => b.Contains("Rabies"))
        ), Times.Once);

        _mockEmailService.Verify(e => e.SendEmailAsync(
            "nam@gmail.com", 
            It.IsAny<string>(), 
            It.Is<string>(b => b.Contains("Parvo"))
        ), Times.Never);
    }
}
```
