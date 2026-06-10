# 🎨 UI/UX Design Spec - Authentication & Authorization

## 1. Bố cục Giao diện & Trực quan hóa (Responsive Layout)

Giao diện đăng nhập và đăng ký được thiết kế nằm ở trung tâm màn hình, sử dụng khối mờ kính Glassmorphism sang trọng trên nền tối chủ đạo của ứng dụng MyPetClinic.

### A. Sơ đồ ASCII Mockup - Màn hình Đăng Nhập (Login Screen Layout)
```text
+-----------------------------------------------------------------------------+
|                                MYPETCLINIC                                  |
|                                                                             |
|         +---------------------------------------------------------+         |
|         | [GLASS CONTAINER - BIỂU MẪU ĐĂNG NHẬP]                   |         |
|         |                                                         |         |
|         |  Chào mừng trở lại!                                     |         |
|         |  Vui lòng đăng nhập để quản lý lịch hẹn và hồ sơ thú cưng|         |
|         |                                                         |         |
|         |  Địa chỉ Email:                                         |         |
|         |  [ customer@example.com                               ] |         |
|         |                                                         |         |
|         |  Mật khẩu:                                              |         |
|         |  [ ************                                  ][Eye] |         |
|         |                                                         |         |
|         |  [X] Ghi nhớ mật khẩu              [ Quên mật khẩu? ]   |         |
|         |                                                         |         |
|         |  [ ĐĂNG NHẬP HỆ THỐNG ]                                 |         |
|         |                                                         |         |
|         |  ---------------------- HOẶC ------------------------  |         |
|         |                                                         |         |
|         |  [ G  Đăng Nhập Bằng Google ]                           |         |
|         |                                                         |         |
|         |  Chưa có tài khoản? [ Đăng ký ngay ]                    |         |
|         +---------------------------------------------------------+         |
|                                                                             |
+-----------------------------------------------------------------------------+
```

### B. Sơ đồ ASCII Mockup - Màn hình Xác thực OTP (OTP Verification Layout)
```text
+-----------------------------------------------------------------------------+
|                                MYPETCLINIC                                  |
|                                                                             |
|         +---------------------------------------------------------+         |
|         | [GLASS CONTAINER - XÁC THỰC MÃ OTP]                     |         |
|         |                                                         |         |
|         |  Xác thực tài khoản                                     |         |
|         |  Hệ thống đã gửi mã OTP 6 số đến email của bạn          |         |
|         |                                                         |         |
|         |             [ 8 ] [ 5 ] [ 2 ] [ 0 ] [ 1 ] [ 9 ]         |         |
|         |                                                         |         |
|         |  ( Mã OTP có hiệu lực trong 04:59 phút )                |         |
|         |                                                         |         |
|         |  [ XÁC NHẬN MÃ OTP ]                                    |         |
|         |                                                         |         |
|         |  Chưa nhận được mã? [ Gửi lại mã ]                      |         |
|         +---------------------------------------------------------+         |
|                                                                             |
+-----------------------------------------------------------------------------+
```

---

## 2. Thiết kế Hệ thống Màu sắc & Trạng thái CSS (HSL Variables)

Biểu mẫu đăng nhập mờ kính Glassmorphism được thiết kế có hiệu ứng tương tác cao để thu hút người dùng:

```css
:root {
  /* Biến màu HSL Dark Mode */
  --auth-bg: HSL(222, 47%, 11%);
  --auth-glass-bg: HSL(217, 33%, 17%, 0.65);
  --auth-border: HSL(217, 30%, 22%);
  --auth-border-focus: HSL(239, 84%, 67%);
  --auth-glow: 0 0 15px HSL(239, 84%, 67%, 0.3);
  --auth-error: HSL(0, 84%, 60%);
  --auth-success: HSL(142, 71%, 45%);
}

/* Kiểu dáng Biểu mẫu */
.auth-card {
  background: var(--auth-glass-bg);
  border: 1px solid var(--auth-border);
  backdrop-filter: blur(16px);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
  padding: 40px;
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}

/* Hiệu ứng Rung lắc khi nhập sai (Shake Animation) */
.auth-card.shake {
  animation: shake 0.4s ease-in-out;
  border-color: var(--auth-error);
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-8px); }
  40%, 80% { transform: translateX(8px); }
}
```

---

## 3. Hoạt ảnh & Trải nghiệm Tương tác (UX/UI Animations)

### A. Hiệu ứng chuyển đổi qua lại (Form Toggle Transitions)
Khi người dùng nhấp chọn từ "Đăng nhập" sang "Đăng ký" hoặc "Quên mật khẩu":
*   Không tải lại trang (SPA behavior).
*   Form cũ mờ dần và trượt sang trái (`transform: translateX(-50px); opacity: 0;`), form mới trượt nhẹ từ phải vào (`transform: translateX(0); opacity: 1;`) trong vòng `300ms`.

### B. Dynamic Focus OTP Inputs (Tự động chuyển ô nhập OTP)
*   Màn hình nhập OTP gồm 6 ô `input` riêng biệt.
*   Khi người dùng gõ xong 1 chữ số vào ô số 1, con trỏ chuột tự động focus nhảy sang ô số 2.
*   Khi bấm phím Backspace xóa, con trỏ chuột tự động nhảy lùi về ô trước đó.
*   Khi ô thứ 6 được điền đầy đủ, hệ thống tự động kích hoạt tiến trình gửi request xác thực OTP lên server ngay mà không cần đợi người dùng click nút "Xác nhận".
