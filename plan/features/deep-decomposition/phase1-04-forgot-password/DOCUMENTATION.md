# 📖 Product & Developer Documentation - Forgot Password (Phase 1)

Tài liệu này cung cấp hướng dẫn vận hành cho người dùng và tài liệu phát triển/gỡ lỗi (Debugging Guide) dành cho các lập trình viên làm việc trên tính năng Khôi phục mật khẩu trong hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng cho Khách hàng (User Guide)

### Quy trình khôi phục mật khẩu từng bước:
1. Tại màn hình Đăng nhập (`/login`), nhấp vào liên kết **Quên mật khẩu?**.
2. Nhập địa chỉ **Email** của bạn đã đăng ký trên hệ thống và nhấn **Gửi mã xác nhận**.
3. Hệ thống sẽ gửi một mã OTP gồm 6 chữ số tới email của bạn.
4. Nhập mã OTP nhận được vào giao diện.
   - *Lưu ý:* Mã OTP có hiệu lực trong **5 phút**. Nếu quá 5 phút bạn mới nhập, hệ thống sẽ báo lỗi hết hạn.
   - Bạn chỉ được phép nhập sai OTP **tối đa 3 lần**. Nhập sai đến lần thứ 3, mã OTP sẽ bị hủy và bạn phải quay về Bước 1 để lấy mã mới.
5. Nhập **Mật khẩu mới** và **Xác nhận mật khẩu** (mật khẩu phải dài tối thiểu 8 ký tự, có chữ hoa, thường, số và ký tự đặc biệt).
6. Nhấp nút **Hoàn tất đặt lại**. Hệ thống sẽ báo thành công và chuyển bạn về giao diện Đăng nhập.

---

## 2. Tài liệu dành cho Lập trình viên (Developer Guide)

### 2.1. Cấu trúc các File liên quan trong Codebase
*   **Backend Web API:**
    *   [AccountController.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.WebApi/Controllers/AccountController.cs) — API Endpoints `/api/account/forgot-password` và `/api/account/reset-password`.
    *   [AuthService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AuthService.cs) — Logic sinh OTP khôi phục, mã hoá BCrypt mật khẩu mới, reset lockout.
    *   [ResetPasswordRequest.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/DTOs/ResetPasswordRequest.cs) — DTO Model Validation.
*   **Frontend Vue 3:**
    *   [ForgotPasswordWizard.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/auth/ForgotPasswordWizard.vue) — Component giao diện wizard 3 bước.
    *   [useForgotPasswordStore.ts](file:///e:/DATN/MyPetClinic/frontend/src/store/useForgotPasswordStore.ts) — Store Pinia điều phối luồng khôi phục.

### 2.2. Cách Debug lấy nhanh mã OTP khôi phục ở localhost
Tương tự như luồng Đăng ký tài khoản:
1.  **Xem qua Console Log Backend:**
    *   Mã OTP khôi phục được sinh ra sẽ tự động in ra màn hình Console Terminal chạy `dotnet run` để lập trình viên nhanh chóng copy gỡ lỗi.
    *   *Định dạng log:* `[DEBUG FORGOT PASSWORD OTP] Mã OTP khôi phục mật khẩu của nguyenvana@example.com là: 921504`
2.  **Xem qua Mailtrap Inbox:**
    *   Mở hộp thư Mailtrap ảo để kiểm tra định dạng email HTML kết xuất đã đúng chuẩn thiết kế hay chưa.

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Lỗi 400 OTP_ATTEMPTS_EXCEEDED (Nhập sai OTP quá 3 lần)
*   *Triệu chứng:* Khi người dùng gõ nhầm ký tự và bấm xác minh đến lần thứ 3, giao diện tự động nhảy về bước 1 và báo lỗi.
*   *Nguyên nhân:* Đây là tính năng bảo mật chủ động để chống hacker viết script brute-force dò mã OTP.
*   *Giải pháp:* Hướng dẫn người dùng kiểm tra kỹ email để lấy đúng mã OTP mới nhất và thực hiện lại từ đầu.
