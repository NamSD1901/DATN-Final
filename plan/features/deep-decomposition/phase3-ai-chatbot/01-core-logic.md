# 🧠 Core Business Logic - Gemini AI Chatbot Advisor

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Đọc an toàn các tham số cấu hình API từ `appsettings.json` thông qua `IOptions` hoặc `IConfiguration`.
- **BE-F03 (Async/Await):** Quản lý tiến trình bất đồng bộ kèm Timeout và hủy lệnh gọi API thông minh.

---

## 1. C# Logic: Gemini Integration Service

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class GeminiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string GeminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    private const string SystemInstruction = 
        "Bạn là bác sĩ thú y ảo tư vấn thân thiện của phòng khám thú y MyPetClinic. " +
        "Hãy trả lời ngắn gọn, nhiệt tình. Tuyệt đối KHÔNG được tự ý kê đơn thuốc chuyên khoa " +
        "hoặc các loại kháng sinh. Chỉ tư vấn chăm sóc, dinh dưỡng và sơ cứu cơ bản. " +
        "Nếu phát hiện triệu chứng nặng hoặc khẩn cấp, bắt buộc phải khuyên người dùng đặt lịch hẹn khám " +
        "hoặc mang thú cưng trực tiếp đến phòng khám MyPetClinic.";

    public GeminiChatService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing");
    }

    public async Task<string> AskGeminiAsync(List<ChatMessageDto> history, string newMessage)
    {
        // Thiết lập Timeout tối đa 10 giây cho external API
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        
        try
        {
            var contentsList = new List<object>();

            // 1. Thêm System Instruction định hướng vai trò
            contentsList.Add(new { role = "user", parts = new[] { new { text = SystemInstruction } } });
            contentsList.Add(new { role = "model", parts = new[] { new { text = "Tôi đã hiểu rõ vai trò và các quy định của mình." } } });

            // 2. Thêm Lịch sử trò chuyện
            foreach (var chat in history)
            {
                contentsList.Add(new { role = chat.Role == "model" ? "model" : "user", parts = new[] { new { text = chat.Content } } });
            }

            // 3. Thêm câu hỏi mới hiện tại
            contentsList.Add(new { role = "user", parts = new[] { new { text = newMessage } } });

            var requestBody = new { contents = contentsList };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{GeminiUrl}?key={_apiKey}", jsonContent, cts.Token);
            
            if (!response.IsSuccessStatusCode)
            {
                return "Xin lỗi, hiện tại tôi không thể kết nối tới máy chủ AI. Bạn vui lòng thử lại sau.";
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            
            // Parse dữ liệu JSON trả về từ Google Gemini API
            var textResponse = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textResponse ?? "Tôi không nhận được phản hồi từ bác sĩ thú y ảo.";
        }
        catch (OperationCanceledException)
        {
            return "Kết nối tới bác sĩ thú y ảo bị quá thời gian phản hồi (Timeout). Bạn vui lòng thử lại câu hỏi ngắn hơn nhé.";
        }
        catch (Exception ex)
        {
            return $"Đã xảy ra lỗi khi kết nối với AI: {ex.Message}";
        }
    }
}
```
