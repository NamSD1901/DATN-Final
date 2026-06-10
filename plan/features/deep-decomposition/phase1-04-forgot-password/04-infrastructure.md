# 🌐 Infrastructure & Security Specification - Forgot Password (Phase 1)

Tài liệu này đặc tả cấu hình hạ tầng gửi thư khôi phục mật khẩu, các chính sách bảo mật mạng xã hội/cổng API và giới hạn tần suất gửi email khôi phục chống spam.

---

## 1. Dịch vụ Gửi Email Khôi phục Mật khẩu (Mail Settings)

Hệ thống kế thừa cấu hình `EmailSettings` và lớp `EmailService` từ module đăng ký tài khoản. Tuy nhiên, nội dung thư (Email Template) được thiết lập giao diện chuyên biệt cho hoạt động reset mật khẩu:

### 1.1. Email Template khôi phục mật khẩu HTML
```html
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <h2 style='color: #0d9488; margin: 0;'>MyPetClinic Vet Hospital</h2>
        <p style='color: #64748b; font-size: 14px; margin: 5px 0 0 0;'>Khôi Phục Mật Khẩu Tài Khoản</p>
    </div>
    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin-bottom: 20px;' />
    <p>Chào <strong>{recipientName}</strong>,</p>
    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản MyPetClinic liên kết với email này.</p>
    <p>Vui lòng sử dụng mã OTP bên dưới để tiến hành khôi phục mật khẩu tài khoản của bạn:</p>
    <div style='text-align: center; margin: 30px 0;'>
        <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #0d9488; background-color: #f0fdfa; padding: 10px 30px; border: 1px dashed #0d9488; border-radius: 6px; display: inline-block;'>{otpCode}</span>
    </div>
    <p style='color: #e11d48; font-weight: bold;'>Lưu ý: Mã OTP khôi phục này có hiệu lực trong vòng 5 phút.</p>
    <p style='color: #64748b; font-size: 12px; margin-top: 30px;'>Nếu bạn không gửi yêu cầu này, vui lòng bỏ qua email này hoặc liên hệ ngay với bộ phận hỗ trợ của phòng khám để bảo vệ tài khoản.</p>
</div>
```

---

## 2. Các Lá chắn Bảo mật & Phòng chống Tấn công (Security Shields)

Endpoints quên mật khẩu thường bị lợi dụng để tấn công làm tràn hòm thư người dùng (Email Bombing) hoặc dò tìm email. Chúng ta cấu hình 3 lá chắn:

### 2.1. Cấu hình Rate Limiting nghiêm ngặt
*   *Endpoint bảo vệ:* `POST /api/account/forgot-password`.
*   *Phương thức:* Giới hạn theo địa chỉ IP và địa chỉ Email.
*   *Hạn mức:* **Tối đa 3 lần yêu cầu khôi phục mật khẩu trong vòng 1 giờ**.
*   *Mục đích:* Ngăn chặn kẻ xấu gửi liên tục hàng trăm email OTP rác tới hòm thư người dùng, đồng thời tiết kiệm chi phí/hạn mức gửi email của máy chủ.

### 2.2. Chống Tấn công Dò quét tài khoản (User Enumeration Defense)
*   Như đặc tả ở [Core Logic](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/01-core-logic.md), API trả về cùng một thông báo thành công `HTTP 200 OK` cho cả trường hợp email tồn tại và không tồn tại. Điều này ngăn chặn hacker thu thập thông tin xem địa chỉ email nào đã đăng ký tài khoản trên hệ thống phòng khám của chúng ta.

### 2.3. Hủy bỏ OTP ngay lập tức sau khi sử dụng (One-Time Use Enforcement)
*   Để tránh việc OTP bị chặn bắt (intercepted) và sử dụng lại nhiều lần, ngay sau khi so khớp đúng OTP tại API `reset-password`, thuộc tính `PasswordResetOtp` và `PasswordResetOtpExpiry` lập tức được ghi nhận về `null` trước khi lưu vào DB.
