# 📄 User & Dev Documentation - Gemini AI Chatbot Advisor

## 1. Hướng dẫn lấy Gemini API Key từ Google AI Studio
1. Truy cập [Google AI Studio](https://aistudio.google.com/).
2. Đăng nhập bằng tài khoản Google.
3. Nhấp chọn **Create API Key**.
4. Sao chép API Key và lưu cấu hình vào môi trường biến như hướng dẫn ở tệp `04-infrastructure.md`.

---

## 2. Thiết lập Dependency Injection ở Backend
Đăng ký `HttpClient` và `GeminiChatService` trong file khởi chạy `Program.cs`:

```csharp
builder.Services.AddHttpClient<GeminiChatService>();
builder.Services.AddScoped<GeminiChatService>();
```

---

## 3. Hướng dẫn hiển thị Markdown trong Vue Component (Frontend)
Vì phản hồi từ Gemini API thường chứa mã định dạng Markdown (như chữ đậm `**chữ**` hoặc danh sách thụt lề), hãy cài đặt thư viện `marked` để hiển thị định dạng văn bản mượt mà, tránh việc hiển thị text thô:

```bash
npm install marked
```

Sử dụng trong component:
```html
<div class="msg-content" v-html="renderMarkdown(msg.content)"></div>
```
```typescript
import { marked } from 'marked';

const renderMarkdown = (text: string) => {
  return marked.parse(text);
};
```
*(Lưu ý: Tùy biến CSS để lọc an toàn XSS khi sử dụng `v-html`).*
