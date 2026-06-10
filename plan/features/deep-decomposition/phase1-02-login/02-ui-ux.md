# 🎨 UI & UX Specifications - Login Form (Vue 3)

Tài liệu này đặc tả chi tiết giao diện biểu mẫu Đăng nhập hệ thống, cấu trúc trực quan (ASCII Mockup) và các hiệu ứng chuyển đổi trạng thái giao diện người dùng.

---

## 1. Thiết kế Giao diện Biểu mẫu (Login Panel Layout)

Màn hình đăng nhập được trình bày dưới dạng một biểu mẫu kính mờ (**Glassmorphism**) nằm giữa trang, tạo điểm nhấn cao cấp:

```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                        ĐĂNG NHẬP HỆ THỐNG                   |
|                                                             |
|  Email đăng nhập:                                           |
|  +-------------------------------------------------------+  |
|  | nguyenvana@example.com                                |  |
|  +-------------------------------------------------------+  |
|                                                             |
|  Mật khẩu:                                                  |
|  +-------------------------------------------------------+  |
|  | ••••••••                                              |  | [👁] (Nút bật/tắt mật khẩu)
|  +-------------------------------------------------------+  |
|                                                             |
|  [ ] Ghi nhớ đăng nhập                      [Quên mật khẩu?] |
|                                                             |
|                     [⚡ ĐĂNG NHẬP HỆ THỐNG]                  |
|                                                             |
|  Bạn chưa có tài khoản? [Đăng ký ngay]                      |
+-------------------------------------------------------------+
```

### 1.1. Nút bật/tắt hiển thị mật khẩu (Password Eye Toggle)
*   **UX Tương tác:** Nút icon hình con mắt nằm lồng bên trong góc phải của ô nhập mật khẩu.
*   **Hành vi:**
    *   Mặc định: Trường input có kiểu `type="password"`. Icon con mắt hiển thị có gạch chéo.
    *   Click vào icon: Chuyển trường sang `type="text"` và ẩn nét gạch chéo trên icon con mắt giúp người dùng nhìn thấy rõ mật khẩu đã gõ.
    *   *Micro-animation:* Icon xoay nhẹ 45 độ khi chuyển đổi trạng thái để tạo cảm giác phản hồi nhạy bén.

### 1.2. Nút "Ghi nhớ đăng nhập" (Remember Me Checkbox)
*   **UX Tương tác:** Hộp kiểm chọn checkbox có thiết kế tùy biến (custom checkbox) với hiệu ứng trượt hoặc tick chuyển sang màu Neon Teal nổi bật khi được click chọn. Hỗ trợ lưu email đăng nhập vào LocalStorage để điền sẵn cho lần đăng nhập kế tiếp.

---

## 2. Xác thực Trực quan & Phản hồi Lỗi (Real-time Visual Feedback)

Hệ thống phản ánh trạng thái hợp lệ của dữ liệu thông qua màu sắc viền và bóng phát sáng:

*   **Trạng thái Focus Hợp lệ (Valid State):**
    *   *Input Border Color:* `#0D9488` (Teal - Xanh ngọc).
    *   *Glow shadow:* `box-shadow: 0 0 10px rgba(13, 148, 136, 0.3)`.
*   **Trạng thái Lỗi đăng nhập hoặc Sai cú pháp (Error State):**
    *   *Input Border Color:* `#EF4444` (Ruby Red - Đỏ hồng).
    *   *Glow shadow:* `box-shadow: 0 0 10px rgba(239, 68, 68, 0.25)`.
    *   *Text Indicator:* Xuất hiện thông điệp báo lỗi màu đỏ ngay dưới ô nhập liệu tương ứng.
*   **Hiệu ứng Rung lắc (Shake Animation):**
    *   Khi người dùng nhấn Đăng nhập mà API trả về lỗi xác thực `401 Unauthorized` (Sai email/mật khẩu), toàn bộ khung Form đăng nhập sẽ thực hiện hiệu ứng rung lắc nhẹ theo trục ngang (`transform: translateX`) trong **400ms** để cảnh báo trực quan cho người dùng.

---

## 3. Mã nguồn xử lý Tương tác Ẩn/Hiện Mật khẩu (Client-Side Vue)

```html
<script setup lang="ts">
import { ref } from 'vue';

const showPassword = ref(false);
const passwordValue = ref('');

function togglePasswordVisibility() {
  showPassword.value = !showPassword.value;
}
</script>

<template>
  <div class="input-wrapper">
    <input 
      :type="showPassword ? 'text' : 'password'" 
      v-model="passwordValue"
      placeholder="Nhập mật khẩu..."
      class="password-input"
    />
    <button 
      type="button" 
      class="eye-toggle-btn"
      @click="togglePasswordVisibility"
      aria-label="Ẩn/Hiện mật khẩu"
    >
      <!-- Icon con mắt động thay đổi theo trạng thái showPassword -->
      <svg v-if="showPassword" class="icon" viewBox="0 0 24 24">...</svg>
      <svg v-else class="icon icon-slash" viewBox="0 0 24 24">...</svg>
    </button>
  </div>
</template>
```
