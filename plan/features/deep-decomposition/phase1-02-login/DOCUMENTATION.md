# 📖 Product & Developer Documentation - Login System (Phase 1)

Tài liệu này cung cấp hướng dẫn vận hành hệ thống đăng nhập cho người dùng và tài liệu phát triển/gỡ lỗi (Debugging Guide) dành cho các lập trình viên làm việc trên hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng cho Người dùng (User Guide)

### 1.1. Quy trình đăng nhập tiêu chuẩn
1. Truy cập trang đăng nhập **MyPetClinic** (`/login`).
2. Nhập **Email đăng nhập** và **Mật khẩu** đã đăng ký.
3. Nhấp vào biểu tượng **Con mắt** trong ô mật khẩu để kiểm tra lại tính chính xác nếu cần.
4. Tích chọn **Ghi nhớ đăng nhập** nếu bạn muốn hệ thống lưu sẵn email của bạn cho lần đăng nhập sau trên thiết bị này.
5. Nhấp nút **Đăng nhập hệ thống**.

### 1.2. Các tình huống lỗi thường gặp khi đăng nhập
*   **Tài khoản chưa được kích hoạt:** Giao diện sẽ hiển thị thông báo yêu cầu kích hoạt và tự động chuyển bạn sang màn hình nhập mã OTP. Vui lòng kiểm tra email để lấy OTP kích hoạt.
*   **Sai mật khẩu quá nhiều lần (Khóa tài khoản):** Nếu bạn nhập sai mật khẩu **5 lần liên tiếp**, tài khoản của bạn sẽ bị tạm khóa trong **15 phút** để bảo vệ an toàn. Bạn phải chờ hết thời gian khóa hoặc sử dụng chức năng **Quên mật khẩu** để đặt lại mật khẩu mới và mở khóa sớm.

---

## 2. Tài liệu dành cho Lập trình viên (Developer Guide)

### 2.1. Cấu trúc các File liên quan trong Codebase
*   **Backend Web API:**
    *   [AccountController.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.WebApi/Controllers/AccountController.cs) — Endpoint `/api/account/login`.
    *   [AuthService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AuthService.cs) — Thực hiện đối chiếu BCrypt mật khẩu, kiểm tra trạng thái khóa.
    *   [JwtTokenGenerator.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Infrastructure/Authentication/JwtTokenGenerator.cs) — Logic tạo JWT token và đính kèm claims.
*   **Frontend Vue 3:**
    *   [LoginTab.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/auth/LoginTab.vue) — Component giao diện đăng nhập.
    *   [useAuthStore.ts](file:///e:/DATN/MyPetClinic/frontend/src/store/useAuthStore.ts) — Quản lý trạng thái Token và phân quyền Route Guards.

### 2.2. Hướng dẫn sử dụng JWT Token trong Postman / Swagger
Khi debug các API yêu cầu quyền đăng nhập (ví dụ: lấy hồ sơ thú cưng, ghi nhận chẩn đoán y khoa):
1. Gọi API đăng nhập `/api/account/login` để lấy chuỗi `token` trả về.
2. Trong Postman: 
    *   Chuyển sang Tab **Authorization**.
    *   Chọn Type: **Bearer Token**.
    *   Dán chuỗi token nhận được vào ô **Token**.
3. Trong Swagger UI:
    *   Nhấp vào nút **Authorize** ở góc trên cùng bên phải.
    *   Nhập: `Bearer <token_cua_ban>` và bấm Authorize.

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### 3.1. Hướng dẫn Mở khóa Tài khoản nhanh trong DB (Dành cho Dev/QA)
Khi kiểm thử chức năng khóa tài khoản (Lockout), tài khoản sẽ bị khóa 15 phút. Để mở khóa ngay lập tức phục vụ kiểm thử:
*   Chạy truy vấn SQL trực tiếp trên Database PostgreSQL:
    ```sql
    UPDATE "Users" 
    SET "AccessFailedCount" = 0, "LockoutEnd" = NULL 
    WHERE "Email" = 'doctor.minh@mypetclinic.com';
    ```

### 3.2. Lỗi Token hết hạn hoặc không hợp lệ (HTTP 401 Unauthorized)
*   *Triệu chứng:* Người dùng đang sử dụng ứng dụng bình thường thì đột ngột bị đẩy ra trang đăng nhập hoặc các API trả về lỗi 401.
*   *Nguyên nhân:* Phiên làm việc JWT Token đã hết hạn sử dụng (quá 12 giờ) hoặc khóa bí mật `JwtSettings:Secret` ở backend đã bị thay đổi làm chữ ký số bị mất hiệu lực.
*   *Giải pháp:* Hệ thống Axios Interceptor ở Frontend sẽ tự động phát hiện mã lỗi `401` và trigger hàm `authStore.logout()` để dọn sạch token cũ và yêu cầu người dùng đăng nhập lại lấy phiên mới.
