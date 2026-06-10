# 🌐 Infrastructure & Security - Gemini AI Chatbot Advisor

## 🔗 Skills Liên Quan
- **BE-A03 (RBAC):** `[Authorize]` yêu cầu người dùng phải đăng nhập tài khoản khách hàng để sử dụng dịch vụ tư vấn AI.
- **BE-C01 (Config Management):** Đọc an toàn khóa bí mật API Key của Gemini API từ biến môi trường của hệ điều hành, tránh lộ lọt API Key lên mã nguồn Git công khai.

---

## 1. Bảo mật API Key cấu hình ngoài (External Security Configuration)

Để tránh rò rỉ khóa bí mật Google Gemini API Key, tuyệt đối không được ghi đè trực tiếp key vào tệp cấu hình `appsettings.json`. Hãy khai báo thông qua biến môi trường (Environment Variables) hoặc cấu hình `user-secrets` ở máy phát triển:

```json
// appsettings.json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_ENV_VARIABLE"
  }
}
```

Trong hệ thống CI/CD hoặc môi trường chạy sản phẩm (Production), thiết lập biến môi trường:
```bash
# Windows cmd/powershell
[System.Environment]::SetEnvironmentVariable("Gemini__ApiKey", "AIzaSy...", "User")
```

---

## 2. Giới hạn Tần suất Yêu cầu (Rate Limiting)
Mỗi tài khoản khách hàng chỉ được phép gửi tối đa 10 câu hỏi tư vấn AI trong vòng 1 phút nhằm phòng chống tấn công từ chối dịch vụ (DDoS) và lạm dụng làm hao hụt hạn mức API Key của Google. Cấu hình Rate Limiting Middleware ở tệp `Program.cs` cho Endpoint `/api/ai-chatbot/*`.
