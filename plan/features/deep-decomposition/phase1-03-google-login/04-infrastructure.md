# 🌐 Infrastructure & Security Specification - Google OAuth (Phase 1)

Tài liệu này đặc tả các thiết lập hạ tầng trên Google Cloud Console, cấu hình kết nối mạng an toàn (HTTPS/CORS) và các lá chắn bảo mật chống giả mạo token Google.

---

## 1. Cấu hình Cổng kết nối trên Google Cloud Console (OAuth 2.0 Credentials)

Để ứng dụng có quyền kết nối và yêu cầu xác thực từ Google Server, quản trị viên bắt buộc phải đăng ký dự án trên Google Cloud Console:

### 1.1. Thiết lập Màn hình Đồng ý OAuth (OAuth Consent Screen)
*   **User Type:** External.
*   **App Name:** MyPetClinic Vet Hospital.
*   **Scopes cần cấp quyền:** `openid`, `https://www.googleapis.com/auth/userinfo.email`, `https://www.googleapis.com/auth/userinfo.profile`.

### 1.2. Khởi tạo mã OAuth 2.0 Client ID
*   **Application Type:** Web Application.
*   **Authorized JavaScript Origins (Nguồn gốc JS hợp lệ):**
    *   *Development:* `http://localhost:5173` (Cổng mặc định của Vite).
    *   *Production:* `https://mypetclinic.com`.
*   **Authorized Redirect URIs (URI chuyển hướng hợp lệ):**
    *   *Development:* `http://localhost:5173/login`.
    *   *Production:* `https://mypetclinic.com/login`.
*   *Kết quả thu được:* Mã **Client ID** và **Client Secret** dùng để cấu hình trong `appsettings.json` và code Frontend.

---

## 2. Bảo mật CORS & Chặn đứng Tấn công Tái sử dụng Token (Replay Attack)

### 2.1. Phòng chống Tấn công Tái sử dụng Token (Token Replay Attack)
Một tin tặc có thể đánh cắp Google IdToken hợp lệ từ một ứng dụng khác và gửi lên API MyPetClinic để cố gắng đăng nhập. Chúng ta ngăn chặn bằng 2 quy tắc Backend:
1.  **Kiểm tra Audience (`aud`):** Thư viện `GoogleJsonWebSignature.ValidateAsync` bắt buộc phải cấu hình `Audience = ClientId`. Nếu token chứa `aud` của một ứng dụng khác, API lập tức từ chối với mã lỗi `401 Unauthorized`.
2.  **Kiểm tra thời gian hết hạn (`exp`):** Google IdToken có thời gian sống rất ngắn (5 phút). Backend đối chiếu thuộc tính `exp` với `DateTime.UtcNow`, ngăn chặn tin tặc lưu trữ token cũ để đăng nhập lại sau đó.

### 2.2. Rate Limiting trên Endpoint Google Login
*   *Đường dẫn bảo vệ:* `POST /api/account/google-login`.
*   *Chính sách:* Tương tự đăng nhập thường, giới hạn **Tối đa 5 requests/phút trên mỗi địa chỉ IP** để tránh tin tặc viết script liên tục spam gửi token lỗi làm quá tải luồng xác thực chữ ký của CPU.

---

## 3. Cấu hình Content Security Policy (CSP)
Để cho phép trình duyệt của khách hàng nạp script và Iframe đăng nhập từ Google an toàn mà không bị trình duyệt chặn do vi phạm chính sách bảo mật nội dung:
*   Cần cấu hình bổ sung vào thẻ meta HTML hoặc HTTP Header phản hồi của Server:
    ```http
    Content-Security-Policy: script-src 'self' https://accounts.google.com/gsi/client; frame-src 'self' https://accounts.google.com/;
    ```
