# 📖 Product & Developer Documentation - Register Account (Phase 1)

Tài liệu này cung cấp hướng dẫn vận hành cho người dùng và tài liệu phát triển/gỡ lỗi (Debugging Guide) dành cho các lập trình viên làm việc trên tính năng Đăng ký tài khoản trong hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng cho Khách hàng (User Guide)

### Bước 1: Khởi tạo thông tin
1. Truy cập trang chủ phòng khám **MyPetClinic**, nhấp chuột vào nút **Đăng ký** ở thanh điều hướng trên cùng.
2. Điền đầy đủ các thông tin bắt buộc trong biểu mẫu:
   - *Họ và tên:* Nhập đầy đủ họ tên (Ví dụ: Nguyễn Văn A).
   - *Email:* Nhập địa chỉ email cá nhân đang hoạt động (để nhận mã OTP).
   - *Số điện thoại:* Nhập số điện thoại di động (10 chữ số).
   - *Mật khẩu:* Nhập mật khẩu tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.
3. Tích chọn vào ô *"Tôi đồng ý với Điều khoản dịch vụ & Chính sách bảo mật"*.
4. Bấm nút **Đăng ký tài khoản**.

### Bước 2: Xác thực kích hoạt tài khoản qua OTP
1. Hệ thống sẽ gửi một mã OTP gồm 6 chữ số đến địa chỉ email bạn vừa dùng để đăng ký.
2. Kiểm tra hòm thư của bạn (nếu không thấy, hãy kiểm tra mục **Spam / Thư rác / Quảng cáo**).
3. Nhập mã OTP gồm 6 chữ số vào ô xác thực trên giao diện.
4. Hệ thống sẽ tự động kích hoạt tài khoản và chuyển hướng bạn đến trang **Đăng nhập** sau 3 giây.
5. *Lưu ý:* Mã OTP chỉ có hiệu lực trong **5 phút**. Nếu quá thời gian này hoặc không nhận được thư, hãy nhấp vào liên kết **Gửi lại mã OTP** (cho phép gửi lại sau mỗi 60 giây).

---

## 2. Tài liệu cho Lập trình viên (Developer Guide)

### 2.1. Cấu trúc các File liên quan trong Codebase
Để bảo trì hoặc mở rộng tính năng này, lập trình viên cần làm việc trên các tệp tin sau:
*   **Backend Web API:**
    *   [AccountController.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.WebApi/Controllers/AccountController.cs) — Định nghĩa API Endpoint `/api/account/register` và `/api/account/verify-otp`.
    *   [AuthService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AuthService.cs) — Logic nghiệp vụ băm mật khẩu, sinh OTP bảo mật, kích hoạt tài khoản.
    *   [RegisterRequest.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/DTOs/RegisterRequest.cs) — DTO định nghĩa Model Validation cho đầu vào.
    *   [EmailService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Infrastructure/Services/EmailService.cs) — Triển khai gửi email kết nối tới SMTP Server.
*   **Frontend Vue 3:**
    *   [RegisterTab.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/auth/RegisterTab.vue) — Component giao diện đăng ký và OTP.
    *   [useRegisterStore.ts](file:///e:/DATN/MyPetClinic/frontend/src/store/useRegisterStore.ts) — Store Pinia quản lý trạng thái luồng đăng ký.

### 2.2. Hướng dẫn Debug nhanh mã OTP ở môi trường Development
Khi chạy thử nghiệm ứng dụng dưới máy local (`localhost`), lập trình viên có thể lấy mã OTP bằng 2 cách mà không cần mở hòm thư email thật:
1.  **Xem qua Console Log của Web API:**
    *   Mỗi khi có request đăng ký thành công, AuthService sẽ in trực tiếp mã OTP sinh được ra màn hình Terminal chạy lệnh `dotnet run`.
    *   *Định dạng log:* `[DEBUG OTP] Đã gửi mã OTP kích hoạt 123456 tới địa chỉ email: test@example.com`
2.  **Xem qua Hộp thư ảo Mailtrap:**
    *   Truy cập vào tài khoản Mailtrap dùng thử được cấu hình trong `appsettings.Development.json` để kiểm tra email HTML được kết xuất (render) và kiểm tra giao diện hiển thị thư gửi đi thực tế.

---

## 3. Các lỗi thường gặp và cách xử lý (Troubleshooting)

| Sự cố | Nguyên nhân | Giải pháp |
| :--- | :--- | :--- |
| **Không nhận được email OTP** | Sai cấu hình SMTP Server trong file cấu hình JSON hoặc tài khoản Mailtrap/SendGrid bị hết hạn mức gửi (Limit exceeded). | Kiểm tra log lỗi trong console của Backend Web API. Đảm bảo các tham số Host, Port, Username, Password trong `appsettings.json` là chính xác. |
| **Lỗi 422 EMAIL_ALREADY_EXISTS** | Người dùng cố tình dùng một email đã đăng ký thành công trước đó để tạo tài khoản mới. | Hiển thị thông báo hướng dẫn người dùng chuyển sang trang Đăng nhập hoặc sử dụng chức năng Quên mật khẩu. |
| **Lỗi 400 INVALID_OTP_CODE** | Người dùng gõ sai ký tự OTP hoặc copy dư khoảng trắng ở đầu/cuối mã OTP. | Phía Frontend tự động thực hiện hàm `.trim()` loại bỏ khoảng trắng dư thừa trước khi gửi API xác thực. |
