using Microsoft.Extensions.Configuration;
using MyPetClinic.Application.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Services;

public class AiChatbotService : IAiChatbotService
{
    private readonly string _apiKey;
    private static readonly HttpClient _httpClient = new HttpClient();

    public AiChatbotService(IConfiguration config)
    {
        _apiKey = config["GroqAI:ApiKey"] ?? string.Empty;
    }

    public async Task<string> ChatAsync(string userMessage)
    {
        string url = "https://api.groq.com/openai/v1/chat/completions";

        var requestBody = new
        {
            model = "llama-3.3-70b-versatile", // Cập nhật sang model Llama 3.3 70B mới nhất của Groq
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
            throw new System.Exception($"Groq API Error ({response.StatusCode}): {responseString}");
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
