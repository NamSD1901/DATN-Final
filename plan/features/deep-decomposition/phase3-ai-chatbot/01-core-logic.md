# 01. Core Business Logic Reference - Gemini AI Chatbot

Tài liệu đặc tả các thuật toán xử lý cuộc hội thoại, thiết lập chỉ thị hệ thống (System Prompt Guardrails) và mã nguồn C# kết nối Google Gemini API phía Backend.

---

## 1. Cơ chế Format Lịch sử hội thoại (Chat History Formatter)

Để gọi API Gemini, mảng lịch sử chat từ Client gửi lên phải được chuyển đổi định dạng tương thích với cấu trúc của REST API Google Gemini.
Cấu trúc đích yêu cầu mảng đối tượng `contents` gồm các vai trò `user` và `model`, mỗi vai trò chứa mảng `parts` có trường `text`.

### Thuật toán C# Format Lịch sử:
```csharp
private List<object> FormatChatHistory(List<ChatHistoryItemDto> rawHistory, string currentMessage)
{
    var formattedContents = new List<object>();

    // 1. Đưa các tin nhắn cũ trong lịch sử vào
    foreach (var item in rawHistory)
    {
        formattedContents.Add(new
        {
            role = item.Role == "user" ? "user" : "model",
            parts = new[] { new { text = item.Text } }
        });
    }

    // 2. Đưa tin nhắn hiện tại của người dùng vào cuối mảng
    formattedContents.Add(new
    {
        role = "user",
        parts = new[] { new { text = currentMessage } }
    });

    return formattedContents;
}
```

---

## 2. Kiểm duyệt câu trả lời phòng chống Kê Đơn Thuốc (Safety Filtering Logic)

Mặc dù có System Instruction ở mức API, Backend Proxy của MyPetClinic vẫn áp dụng một lớp kiểm duyệt từ khóa nhạy cảm thứ hai (Post-Processing Filter) trên chuỗi văn bản trả về của Gemini để đảm bảo an toàn tuyệt đối.

### Thuật toán quét từ khóa y học nhạy cảm:
```csharp
private static readonly string[] ForbiddenKeywords = { 
    "amoxicillin", "dexafort", "enrofloxacin", "paracetamol", "depocillin", "ketamine", "uống 2 viên", "uống 3 viên" 
};

public bool ScanForMedicalDangers(string textResponse)
{
    string lowerText = textResponse.ToLower();
    
    // Quét phát hiện tên thuốc cấm hoặc liều lượng uống
    foreach (var keyword in ForbiddenKeywords)
    {
        if (lowerText.Contains(keyword))
        {
            return true; // Phát hiện cảnh báo y khoa
        }
    }
    
    // Quét phát hiện các triệu chứng bệnh nặng cần hướng đặt lịch khám thực tế
    string[] dangerSymptoms = { "nôn ra máu", "co giật", "khó thở", "hôn mê", "ngộ độc bả" };
    foreach (var symptom in dangerSymptoms)
    {
        if (lowerText.Contains(symptom))
        {
            return true;
        }
    }

    return false;
}
```

---

## 3. Mã nguồn C# Service Thực thi Logic Tích hợp Gemini

Dưới đây là cài đặt chi tiết của lớp `GeminiChatService` thực hiện cuộc gọi HTTPS trực tiếp sang Google API bằng `HttpClientFactory`.

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.DTOs.AI;

namespace MyPetClinic.Application.Services
{
    public interface IGeminiChatService
    {
        Task<ChatResponse> GetAiResponseAsync(ChatRequest request);
    }

    public class GeminiChatService : IGeminiChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GeminiChatService> _logger;
        private readonly string _apiKey;
        private readonly string _apiUrl;

        public GeminiChatService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeminiChatService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiKey = configuration["GeminiSettings:ApiKey"] ?? throw new ArgumentNullException("ApiKey của Gemini chưa được cấu hình.");
            _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        }

        public async Task<ChatResponse> GetAiResponseAsync(ChatRequest request)
        {
            var client = _httpClientFactory.CreateClient("GeminiClient");
            
            // 1. Thiết lập System Instruction
            string systemInstruction = "Bạn là Trợ lý bác sĩ thú y ảo vô cùng thân thiện của phòng khám thú y MyPetClinic. " +
                                       "Nhiệm vụ của bạn là tư vấn chăm sóc sức khỏe, dinh dưỡng, hành vi và sơ cứu khẩn cấp cơ bản cho thú cưng. " +
                                       "CÁC RÀNG BUỘC SẮT BẮT BUỘC TUÂN THỦ:\n" +
                                       "1. TUYỆT ĐỐI KHÔNG được phép kê đơn thuốc kháng sinh (như Amoxicillin, Enrofloxacin...), thuốc đặc trị hoặc đưa ra liều lượng thuốc.\n" +
                                       "2. TUYỆT ĐỐI KHÔNG tự chẩn đoán xác định bệnh nặng thay bác sĩ lâm sàng.\n" +
                                       "3. Khi khách hỏi về triệu chứng nguy kịch (nôn ra máu, co giật, khó thở, sốt cao > 40 độ C, ngộ độc bả...), bạn phải khuyên họ mang ngay thú cưng đến bệnh viện thú y gần nhất hoặc đặt lịch hẹn khám trực tiếp tại MyPetClinic.";

            // 2. Định dạng payloads hội thoại
            var formattedContents = FormatChatHistory(request.History, request.Message);

            var payload = new
            {
                contents = formattedContents,
                systemInstruction = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                generationConfig = new
                {
                    temperature = 0.2, // Giảm độ sáng tạo để AI trả lời nhất quán, khoa học
                    maxOutputTokens = 800
                }
            };

            string requestUrl = $"{_apiUrl}?key={_apiKey}";

            try
            {
                var httpResponse = await client.PostAsJsonAsync(requestUrl, payload);
                
                if (!httpResponse.IsSuccessStatusCode)
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Gemini API trả về lỗi: {httpResponse.StatusCode} - {errorContent}");
                    
                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        throw new InvalidOperationException("Trợ lý AI đang quá tải lượt truy vấn (Rate Limit). Vui lòng thử lại sau ít phút.");
                    }
                    
                    throw new InvalidOperationException("Không thể kết nối đến Trợ lý AI lúc này.");
                }

                var jsonResult = await httpResponse.Content.ReadFromJsonAsync<JsonElement>();
                
                // 3. Trích xuất Text trả về từ cấu trúc JSON phức tạp của Google
                string aiText = jsonResult
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Xin lỗi, tôi không thể xử lý câu hỏi này.";

                // 4. Kiểm duyệt an toàn y khoa
                bool requiresAppointment = ScanForMedicalDangers(aiText);
                string? warningMsg = null;

                if (requiresAppointment)
                {
                    warningMsg = "CẢNH BÁO Y KHOA: Vui lòng đưa thú cưng tới phòng khám hoặc liên hệ Bác sĩ ngay nếu có dấu hiệu nặng.";
                }

                return new ChatResponse
                {
                    TextResponse = aiText,
                    RequiresAppointment = requiresAppointment,
                    SystemWarning = warningMsg
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi gọi dịch vụ Gemini Chat");
                throw;
            }
        }

        private List<object> FormatChatHistory(List<ChatHistoryItemDto> rawHistory, string currentMessage)
        {
            var formattedContents = new List<object>();
            foreach (var item in rawHistory)
            {
                formattedContents.Add(new
                {
                    role = item.Role == "user" ? "user" : "model",
                    parts = new[] { new { text = item.Text } }
                });
            }
            formattedContents.Add(new
            {
                role = "user",
                parts = new[] { new { text = currentMessage } }
            });
            return formattedContents;
        }

        private bool ScanForMedicalDangers(string textResponse)
        {
            string lowerText = textResponse.ToLower();
            string[] forbiddenKeywords = { "amoxicillin", "dexafort", "enrofloxacin", "paracetamol", "depocillin", "ketamine", "uống 2 viên", "uống 3 viên" };
            foreach (var keyword in forbiddenKeywords)
            {
                if (lowerText.Contains(keyword)) return true;
            }
            string[] dangerSymptoms = { "nôn ra máu", "co giật", "khó thở", "hôn mê", "ngộ độc bả", "sốt cao" };
            foreach (var symptom in dangerSymptoms)
            {
                if (lowerText.Contains(symptom)) return true;
            }
            return false;
        }
    }
}
```
