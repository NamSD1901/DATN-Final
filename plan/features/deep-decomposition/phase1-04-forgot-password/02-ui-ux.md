# 🎨 UI & UX Specifications - Forgot Password Steps (Vue 3)

Tài liệu này đặc tả chi tiết thiết kế giao diện biểu mẫu 3 bước khôi phục mật khẩu, sơ đồ trực quan (ASCII Mockup) và các hiệu ứng chuyển đổi mượt mà giữa các bước.

---

## 1. Thiết kế Giao diện Biểu mẫu 3 Bước (Layout Steps)

### Bước 1: Yêu cầu khôi phục (Request Step)
```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                        QUÊN MẬT KHẨU                        |
|                                                             |
|  Nhập Email tài khoản của bạn để nhận mã OTP khôi phục:     |
|  Email:                                                     |
|  +-------------------------------------------------------+  |
|  | nguyenvana@example.com_                               |  |
|  +-------------------------------------------------------+  |
|                                                             |
|                     [⚡ GỬI MÃ XÁC NHẬN]                     |
|                                                             |
|  [Quay lại Đăng nhập]                                       |
+-------------------------------------------------------------+
```

### Bước 2: Nhập mã xác thực OTP (OTP Verification Step)
```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                        XÁC THỰC MÃ OTP                      |
|                                                             |
|  Mã OTP đã được gửi đến email nguyenvana@...                |
|                                                             |
|  Nhập mã OTP (6 chữ số):                                    |
|     +---+   +---+   +---+   +---+   +---+   +---+           |
|     | 9 |   | 2 |   | 1 |   | 5 |   | _ |   |   |           |
|     +---+   +---+   +---+   +---+   +---+   +---+           |
|  [Bạn còn 3 lượt nhập sai OTP]                              |
|                                                             |
|                     [⚡ XÁC MINH MÃ OTP]                     |
+-------------------------------------------------------------+
```

### Bước 3: Đặt mật khẩu mới (Reset Password Step)
```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                        ĐẶT MẬT KHẨU MỚI                     |
|                                                             |
|  Nhập mật khẩu mới:                                         |
|  +-------------------------------------------------------+  |
|  | ••••••••                                              |  | [👁]
|  +-------------------------------------------------------+  |
|                                                             |
|  Xác nhận mật khẩu mới:                                     |
|  +-------------------------------------------------------+  |
|  | ••••••••                                              |  | [👁]
|  +-------------------------------------------------------+  |
|  [✔ Mật khẩu xác nhận trùng khớp]                           |
|                                                             |
|                     [⚡ HOÀN TẤT ĐẶT LẠI]                    |
+-------------------------------------------------------------+
```

---

## 2. Trải nghiệm Chuyển đổi và Xử lý Lỗi (Transitions & Errors)

### 2.1. Hiệu ứng chuyển động Slide (Wizard Slide Transition)
*   Để tạo cảm giác mượt mà và trực quan, 3 bước trên được đóng gói trong một cấu trúc **Wizard**. Khi chuyển đổi giữa các bước, container cũ sẽ thực hiện hiệu ứng trượt mượt mà trong vòng **250ms**:
    *   *Chuyển tiếp:* `transition: transform 0.25s ease-out, opacity 0.25s ease-out`.

### 2.2. Visual Feedback tại Bước 3 (Đặt mật khẩu mới)
*   **Trạng thái mật khẩu không khớp:**
    *   *Input Border Color:* `#EF4444` (Đỏ hồng) cho trường nhập lại mật khẩu.
    *   *Text Indicator:* Hiển thị nhãn cảnh báo `"Mật khẩu xác nhận không trùng khớp!"` dưới ô nhập, nút **Hoàn tất** bị khóa.
*   **Trạng thái mật khẩu khớp:**
    *   *Input Border Color:* `#0D9488` (Teal - Xanh ngọc) cho cả hai trường.
    *   *Text Indicator:* Hiện `"Mật khẩu hợp lệ và khớp"` màu xanh lá, mở khóa nút bấm gửi dữ liệu.

---

## 3. Mã nguồn xử lý Tự động chuyển Focus mã OTP (Vue 3 Client)

Thiết kế UX giúp người dùng gõ OTP nhanh chóng bằng cách tự động chuyển con trỏ sang ô kế tiếp:

```javascript
/**
 * Lắng nghe sự kiện gõ phím trên từng ô OTP
 * @param {Event} event - Sự kiện bàn phím
 * @param {number} index - Vị trí ô nhập hiện tại (0..5)
 */
function handleOtpInput(event, index) {
  const input = event.target;
  const value = input.value;
  
  // Chỉ cho phép nhập số
  if (!/^\d$/.test(value)) {
    input.value = '';
    return;
  }

  // Tự động nhảy sang ô tiếp theo nếu gõ xong 1 ký tự
  if (value && index < 5) {
    const nextInput = input.nextElementSibling;
    if (nextInput) {
      nextInput.focus();
    }
  }
}
```
