# 📄 User & Dev Documentation - Authentication & Authorization

Tài liệu cung cấp hướng dẫn vận hành chi tiết dành cho người dùng và tài liệu tích hợp kỹ thuật dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng dành cho Người dùng (End-User Guide)

### Đăng ký tài khoản mới:
1. Truy cập vào trang web phòng khám, click nút **"Đăng ký"** tại góc trên bên phải.
2. Nhập đầy đủ thông tin: Họ tên, Email, Số điện thoại và Mật khẩu. Mật khẩu phải dài tối thiểu 8 ký tự, bao gồm cả chữ và số để đảm bảo an toàn.
3. Nhấp nút **"Đăng ký tài khoản"**. Hệ thống sẽ gửi một mã OTP 6 chữ số qua Email của bạn.
4. Nhập 6 số mã OTP vào các ô tương ứng trên màn hình để kích hoạt tài khoản. Hệ thống tự động chuyển bạn vào trang Dashboard sau khi xác thực thành công.

### Đăng nhập & Đổi phiên:
*   **Đăng nhập thông thường:** Nhập Email và Mật khẩu đã đăng ký, chọn cờ "Ghi nhớ mật khẩu" để không phải nhập lại trong vòng 24 giờ.
*   **Đăng nhập nhanh bằng Google:** Nhấp vào nút **"Đăng nhập bằng Google"**. Chọn tài khoản Gmail của bạn tại pop-up hệ thống để hoàn tất đăng nhập một chạm.
*   **Quên mật khẩu:** Click chọn **"Quên mật khẩu?"** tại form đăng nhập, nhập Email để nhận OTP đặt lại mật khẩu mới.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
*   **Backend Web API:**
    *   [ProfileController.cs](file:///e:/DATN/MyPetClinic/backend/src/WebApi/Controllers/ProfileController.cs) — Endpoint thông tin cá nhân yêu cầu `[Authorize]`.
    *   [AuthController.cs] (hoặc AccountController) — Quản lý đăng nhập/đăng ký/OTP.
    *   [UserService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/UserService.cs) — Logic băm mật khẩu và lấy thông tin phiên làm việc.
*   **Frontend SPA:**
    *   `frontend/src/stores/auth.ts` — Pinia Store quản lý token, claims và Google OAuth login.
    *   `frontend/src/router/index.ts` — Thiết lập Router Guard chặn truy cập trái phép.

### B. Kiểm thử nhanh API bằng công cụ Curl
Bạn có thể sử dụng công cụ dòng lệnh `curl` để gửi request kiểm tra nhanh API:

#### 1. Đăng ký tài khoản mới
```bash
curl -X POST "https://localhost:5001/api/account/register" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -d "{\"email\":\"newuser@example.com\",\"password\":\"SecurePass123!\",\"fullName\":\"Nguyen Van A\",\"phone\":\"0987654321\"}"
```

#### 2. Đăng nhập hệ thống
```bash
curl -X POST "https://localhost:5001/api/account/login" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -d "{\"email\":\"newuser@example.com\",\"password\":\"SecurePass123!\"}"
```

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Sự cố 1: Lỗi `401 Unauthorized` khi gọi API dù đã gửi Token
*   **Nguyên nhân:**
    1. Chuỗi Token JWT bị thiếu tiền tố `Bearer ` (ví dụ: gửi `Authorization: <Token>` thay vì `Authorization: Bearer <Token>`).
    2. Khóa bảo mật `SecurityKey` ở file cấu hình API Backend (`appsettings.json`) đã bị thay đổi hoặc không đồng bộ, làm chữ ký số bị coi là không hợp lệ.
*   **Cách khắc phục:**
    *   *Frontend:* Kiểm tra interceptor Axios đã gán chính xác `Bearer ${token}` vào Header hay chưa.
    *   *Backend:* Kiểm tra logs xem có lỗi xác thực chữ ký (Signature Validation Failed) hay không, đồng bộ lại biến môi trường của JWT.

### Sự cố 2: Lỗi CORS khi đăng nhập bằng Google trên Localhost
*   **Mô tả:** Trình duyệt từ chối request trả về từ Google hoặc chặn gửi request của client lên api Google OAuth.
*   **Khắc phục:** Đảm bảo bạn đã khai báo đường dẫn `http://localhost:5173` trong phần cấu hình **Authorized JavaScript origins** của Google Cloud Console Client ID.
*   Kiểm tra thẻ `<meta http-equiv="Content-Security-Policy">` trên frontend có cho phép nạp script từ `https://accounts.google.com` hay chưa.
