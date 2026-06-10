# 🎨 UI & UX Specifications - Register Account (Vue 3)

Tài liệu này đặc tả chi tiết giao diện biểu mẫu Đăng ký tài khoản, sơ đồ cấu trúc trực quan (ASCII Mockup) và các hiệu ứng chuyển đổi trạng thái giao diện người dùng (CSS HSL Variables & Transitions).

---

## 1. Thiết kế Giao diện Biểu mẫu (Input Form Layout)

Biểu mẫu được thiết kế theo phong cách tối giản, sang trọng sử dụng bố cục kính mờ (**Glassmorphism**) trên nền tối hiện đại (hoặc sáng tinh tế tùy theo chế độ hiển thị):

### 1.1. Màn hình Đăng ký thông tin (Register Step)
```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                        ĐĂNG KÝ TÀI KHOẢN                    |
|                                                             |
|  Họ và tên:                                                 |
|  +-------------------------------------------------------+  |
|  | Nguyễn Văn A                                          |  |
|  +-------------------------------------------------------+  |
|                                                             |
|  Email:                                                     |
|  +-------------------------------------------------------+  |
|  | nguyenvana@example.com_                               |  |
|  +-------------------------------------------------------+  |
|  [✔ Email hợp lệ]                                            |
|                                                             |
|  Số điện thoại:                                             |
|  +-------------------------------------------------------+  |
|  | 0912345678                                            |  |
|  +-------------------------------------------------------+  |
|                                                             |
|  Mật khẩu:                                                  |
|  +-------------------------------------------------------+  |
|  | ••••••••                                              |  | [👁]
|  +-------------------------------------------------------+  |
|   Độ mạnh mật khẩu: [████████░░] Trung bình                  |
|                                                             |
|  [X] Tôi đồng ý với Điều khoản dịch vụ & Chính sách bảo mật  |
|                                                             |
|                     [⚡ ĐĂNG KÝ TÀI KHOẢN]                    |
|                                                             |
|  Bạn đã có tài khoản? [Đăng nhập ngay]                      |
+-------------------------------------------------------------+
```

### 1.2. Màn hình Xác thực OTP (OTP Activation Step)
Sau khi nhấn nút Đăng ký thành công, giao diện sẽ kích hoạt hiệu ứng trượt ngang (slide-left transition) chuyển sang biểu mẫu nhập OTP:
```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                        XÁC THỰC EMAIL                       |
|                                                             |
|  Mã kích hoạt OTP đã được gửi đến email nguyenvana@...      |
|  Vui lòng kiểm tra hộp thư (bao gồm cả thư rác).            |
|                                                             |
|  Nhập mã OTP (6 chữ số):                                    |
|     +---+   +---+   +---+   +---+   +---+   +---+           |
|     | 4 |   | 8 |   | 2 |   | 9 |   | 1 |   | _ |           |
|     +---+   +---+   +---+   +---+   +---+   +---+           |
|                                                             |
|  Mã OTP hết hiệu lực sau: [ 04:59 ]                         |
|                                                             |
|                     [⚡ KÍCH HOẠT TÀI KHOẢN]                 |
|                                                             |
|  Không nhận được mã? [Gửi lại mã OTP] (Khóa sau 60s)        |
+-------------------------------------------------------------+
```

---

## 2. Đặc tả CSS Styling & Trạng thái Trực quan (Visual States)

Hệ thống sử dụng các CSS Variable dạng HSL để dễ dàng chuyển đổi Dark/Light mode và tạo hiệu ứng Glassmorphism sang trọng:

### 2.1. Cấu hình CSS Variables cốt lõi
```css
:root {
  /* Dark Glass Theme Palette */
  --bg-clinic-dark: hsl(222, 47%, 11%);
  --glass-bg: hsla(222, 47%, 15%, 0.7);
  --glass-border: hsla(217, 32%, 60%, 0.15);
  --glass-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
  
  /* Status Colors */
  --color-primary: hsl(172, 66%, 50%);       /* Teal Neon */
  --color-primary-hover: hsl(172, 66%, 45%);
  --color-success: hsl(142, 72%, 45%);       /* Emerald Green */
  --color-error: hsl(350, 89%, 60%);         /* Ruby Red */
  --color-warning: hsl(38, 92%, 50%);        /* Amber Gold */
  --text-muted: hsl(215, 15%, 65%);
}
```

### 2.2. Trạng thái phản hồi của Input Field
*   **Trạng thái Focus bình thường:**
    *   `border-color: var(--color-primary);`
    *   `box-shadow: 0 0 12px hsla(172, 66%, 50%, 0.3);`
    *   *Micro-animation:* Viền input chuyển màu mịn trong vòng **200ms** (`transition: all 0.2s ease-in-out`).
*   **Trạng thái Lỗi cú pháp (Error State):**
    *   `border-color: var(--color-error);`
    *   `box-shadow: 0 0 10px hsla(350, 89%, 60%, 0.25);`
    *   *Hiệu ứng:* Dưới ô nhập liệu hiển thị dòng chữ báo lỗi nhỏ màu đỏ chuyển động trượt xuống nhẹ từ trên ẩn (`transform: translateY(0); opacity: 1; transition: all 0.2s`).
*   **Nút bấm Trạng thái Loading (Đang gọi API):**
    *   Nút bấm bị khóa tương tác (`pointer-events: none`).
    *   Thêm lớp phủ mờ nhẹ và icon xoay vòng Shimmer Loading.

---

## 3. Logic Kiểm thử Biên thời gian thực (Client-Side Validation Helper)

Phía Frontend sử dụng phương thức bắt sự kiện `@input` hoặc `@blur` để kiểm tra độ mạnh mật khẩu và định dạng email/điện thoại ngay lập tức:

```javascript
/**
 * Đánh giá độ mạnh mật khẩu thời gian thực (Password Strength Meter)
 * @param {string} password 
 * @returns {object} { score: 0..4, label: string, colorClass: string }
 */
export function checkPasswordStrength(password) {
  if (!password) return { score: 0, label: 'Trống', colorClass: 'text-muted' };
  
  let score = 0;
  
  // 1. Kiểm tra độ dài
  if (password.length >= 8) score++;
  
  // 2. Chứa chữ số
  if (/\d/.test(password)) score++;
  
  // 3. Chứa chữ hoa và chữ thường
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) score++;
  
  // 4. Chứa ký tự đặc biệt
  if (/[@$!%*?&]/.test(password)) score++;

  const labels = ['Rất yếu ❌', 'Yếu 🟡', 'Trung bình 🟠', 'Mạnh ✔', 'Cực kỳ mạnh 🔥'];
  const colors = ['err-color', 'warn-color', 'warn-color', 'success-color', 'success-color'];

  return {
    score: score,
    label: labels[score],
    colorClass: colors[score]
  };
}
```
*Giao diện Vue component sẽ ràng buộc biến `strength` này để cập nhật thanh tiến trình hiển thị dưới input mật khẩu.*
