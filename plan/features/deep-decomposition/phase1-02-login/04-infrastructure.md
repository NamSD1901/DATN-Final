# 🌐 Infrastructure & Security Specification - Login System (Phase 1)

Tài liệu này đặc tả các chính sách an toàn thông tin, bảo mật kênh truyền dẫn (HTTPS), thiết lập thời gian sống của token xác thực và cấu hình hạ tầng phòng vệ chống tấn công dò mật khẩu tại cổng API Đăng nhập.

---

## 1. Bảo mật Kênh truyền dẫn & Lưu trữ Token (Transmission & Storage Security)

### 1.1. Mã hóa HTTPS/SSL
*   Mọi yêu cầu gửi Email/Mật khẩu từ Client lên Web API bắt buộc phải đi qua giao thức mã hóa **HTTPS (TLS 1.3)**. 
*   *Hành vi chặn:* Nếu người dùng cố tình gửi request qua cổng HTTP thông thường, Web API sử dụng Middleware `UseHttpsRedirection()` để tự động nâng cấp kết nối lên HTTPS hoặc từ chối kết nối không an toàn.

### 1.2. Chiến lược lưu trữ JWT Token
*   **Môi trường phát triển:** Lưu JWT trong `localStorage` để tăng tốc quá trình phát triển Frontend.
*   **Môi trường Production (Khuyến nghị bảo mật cao):** JWT được lưu trữ bên trong **HTTP-Only, Secure Cookie**:
    *   `HttpOnly = true`: Ngăn chặn hoàn toàn các kịch bản tấn công XSS (Cross-Site Scripting) đọc trộm Token qua mã độc JavaScript (`document.cookie`).
    *   `Secure = true`: Đảm bảo Cookie chỉ được truyền qua kênh HTTPS.
    *   `SameSite = SameSiteMode.Strict`: Chặn đứng nguy cơ tấn công giả mạo yêu cầu chéo trang (CSRF - Cross-Site Request Forgery).

---

## 2. Phòng chống Tấn công dò mật khẩu Brute-force & DDoS

Đăng nhập là endpoint nhạy cảm thường xuyên bị tin tặc nhắm tới để tấn công dò tìm mật khẩu. Hệ thống áp dụng 2 lá chắn phòng ngự:

### 2.1. Cấu hình Rate Limiting chi tiết (Tầng ASP.NET Web API)
Chúng ta thiết lập một chính sách giới hạn tần suất truy cập riêng biệt cho API Đăng nhập:
*   *Đường dẫn áp dụng:* `POST /api/account/login`.
*   *Phương thức:* Giới hạn theo Địa chỉ IP và Email đăng nhập.
*   *Hạn mức:* **Tối đa 5 lần gửi yêu cầu trong 1 phút**.
*   *Khi vi phạm:* Hệ thống khóa tạm thời IP đó khỏi endpoint đăng nhập và trả về mã phản hồi `HTTP 429 Too Many Requests` với thông điệp: `"Bạn đã nhập sai hoặc gửi yêu cầu đăng nhập quá nhiều lần. Vui lòng chờ 60 giây."`

### 2.2. Cơ chế khóa tài khoản tự động (User Lockout Policy)
*   Như đặc tả ở [Core Logic](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/01-core-logic.md), khi phát hiện **5 lần nhập sai mật khẩu liên tiếp** trên cùng một Email tài khoản:
    *   Backend tự động thiết lập thời gian khóa `LockoutEnd = DateTime.UtcNow.AddMinutes(15)`.
    *   Mọi request đăng nhập tiếp theo sử dụng email này (kể cả khi gõ đúng mật khẩu mới) đều bị từ chối phục vụ ngay lập tức mà không cần kiểm tra DB, giúp giảm tải CPU xử lý BCrypt băm mật khẩu.

---

## 3. Cấu hình CORS (Cross-Origin Resource Sharing)
*   Backend chỉ phản hồi thông tin Header `Access-Control-Allow-Origin` cho domain Frontend chính thức được cấu hình trong `appsettings.json`.
*   Thuộc tính `Access-Control-Allow-Credentials: true` được cấu hình để cho phép truyền tải Cookie chứa JWT Token an toàn giữa Frontend và Backend.
