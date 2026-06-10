# 🛠️ Technical Specification - Gemini AI Chatbot Advisor

## 🔗 Skills Liên Quan
- **BE-F02 (SOLID - SRP):** Tách biệt logic tích hợp API bên ngoài (`GeminiClient`) ra khỏi lớp nghiệp vụ xử lý Chat (`ChatService`).
- **BE-F03 (Async/Await):** Thiết lập Timeout (Sử dụng `CancellationToken`) cho các lệnh gọi External API (Gemini API) để tránh treo luồng hệ thống nếu phía Google bị chậm phản hồi.

---

## 1. Sequence Diagram: Luồng giao tiếp AI Chatbot

```mermaid
sequenceDiagram
    actor Customer as Khách hàng
    participant FE as Vue Chat Widget
    participant API as Web API Gateway
    participant Gemini as Google Gemini AI API

    Customer->>FE: Nhập câu hỏi & bấm gửi
    FE->>API: POST /api/ai-chatbot/ask (ChatHistory, Message)
    
    Note over API: Đọc API Key từ Config<br/>Khởi tạo System Instruction<br/>Thiết lập Timeout 10s bằng CancellationToken
    
    API->>Gemini: Gửi Prompt (System Instruction + Context + Message)
    
    alt Gemini phản hồi thành công < 10s
        Gemini-->>API: Trả về văn bản kết quả
        API-->>FE: HTTP 200 OK (Văn bản câu trả lời)
    else Hết hạn 10s (Timeout) hoặc Gemini lỗi
        API-->>FE: HTTP 200 OK (Nội dung phản hồi lỗi dự phòng)
    end
    
    FE-->>Customer: Render nội dung chat lên khung hội thoại
```

---

## 2. API Schema & DTOs

```csharp
public class ChatMessageDto
{
    public string Role { get; set; } // user, model
    public string Content { get; set; }
}

public class ChatRequestDto
{
    public List<ChatMessageDto> History { get; set; }
    public string Message { get; set; }
}

public class ChatResponseDto
{
    public string Response { get; set; }
}
```
