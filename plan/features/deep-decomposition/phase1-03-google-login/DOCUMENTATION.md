# 📖 Product & Developer Documentation - Google Login (Phase 1)

Tài liệu này cung cấp hướng dẫn cấu hình, tích hợp và gỡ lỗi (Debugging Guide) cho tính năng đăng nhập Google OAuth trong hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng cho Khách hàng (User Guide)

### 1.1. Quy trình đăng nhập 1-Click bằng Google
1. Truy cập trang đăng nhập **MyPetClinic** (`/login`).
2. Nhấp chuột vào nút **Đăng nhập với Google**.
3. Một cửa sổ popup sẽ xuất hiện, yêu cầu bạn lựa chọn tài khoản Google (Gmail) của mình.
4. Nhấp chọn tài khoản Gmail muốn sử dụng và xác nhận mật khẩu Google nếu trình duyệt yêu cầu.
5. Cửa sổ popup tự động đóng lại, tài khoản của bạn được kích hoạt và đăng nhập ngay lập tức.
6. *Lưu ý bổ sung:* Nếu đây là lần đầu tiên bạn sử dụng tài khoản Google này để đăng nhập MyPetClinic, hệ thống sẽ tự động khởi tạo cho bạn một hồ sơ khách hàng mới. Bạn vui lòng truy cập trang **Hồ sơ cá nhân** sau khi đăng nhập để cập nhật bổ sung **Số điện thoại** phục vụ việc đặt lịch khám.

---

## 2. Tài liệu dành cho Lập trình viên (Developer Guide)

### 2.1. Cấu trúc các File liên quan trong Codebase
*   **Backend Web API:**
    *   [AccountController.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.WebApi/Controllers/AccountController.cs) — API `/api/account/google-login`.
    *   [AuthService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AuthService.cs) — Logic tự động tạo tài khoản (Auto-Provisioning) khi đăng nhập Google lần đầu.
    *   [GoogleTokenValidator.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Infrastructure/Authentication/GoogleTokenValidator.cs) — Triển khai gọi thư viện giải mã và xác thực chữ ký token từ Google Server.
*   **Frontend Vue 3:**
    *   [LoginTab.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/auth/LoginTab.vue) — Nơi render nút Google Sign-In và gọi Callback.
    *   [useAuthStore.ts](file:///e:/DATN/MyPetClinic/frontend/src/store/useAuthStore.ts) — Gửi payload Google Token lên Backend đổi JWT.

### 2.2. Hướng dẫn Cấu hình Môi trường Phát triển (Local Development)
Để chạy thử đăng nhập Google ở localhost:
1. Đảm bảo file [appsettings.Development.json](file:///e:/DATN/MyPetClinic/backend/src/WebApi/appsettings.Development.json) có cấu hình đúng Client ID Google của bạn:
   ```json
   "GoogleAuthSettings": {
     "ClientId": "your-client-id-here.apps.googleusercontent.com"
   }
   ```
2. Frontend phải chạy trên cổng được cấu hình Authorized Origins trên Google Cloud (mặc định là `http://localhost:5173`). Nếu đổi sang cổng khác, nút Google sẽ báo lỗi không tải được.

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### 3.1. Lỗi cửa sổ Popup Google không xuất hiện
*   *Nguyên nhân:* Trình duyệt của người dùng cài đặt phần mềm chặn quảng cáo (AdBlocker) hoặc cấu hình chặn tất cả các cửa sổ bật lên (Pop-ups Blocker).
*   *Giải pháp:* Nhắc nhở người dùng cho phép hiển thị popup từ địa chỉ trang web `mypetclinic.com` hoặc tạm tắt phần mềm chặn quảng cáo để xác thực.

### 3.2. Lỗi bảng điều khiển: `[Google SDK] idpiframe_initialization_failed` hoặc `Not a valid origin`
*   *Nguyên nhân:* Client ID Google của bạn chưa được đăng ký địa chỉ URL Origin đang chạy (ví dụ bạn chạy Frontend trên `http://127.0.0.1:5173` nhưng trên Google Console chỉ cấu hình `http://localhost:5173`).
*   *Giải pháp:* Truy cập Google Cloud Console -> Credentials -> Chỉnh sửa OAuth Client ID và bổ sung địa chỉ IP/Domain chính xác vào mục **Authorized JavaScript Origins**.
