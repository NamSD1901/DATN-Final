# 🎨 UI & UX Specifications - Google Login Button (Vue 3)

Tài liệu này đặc tả chi tiết thiết kế nút bấm Đăng nhập bằng Google, tuân thủ các nguyên tắc thương hiệu (Brand Guidelines) của Google và tích hợp mượt mà vào giao diện Glassmorphism của **MyPetClinic**.

---

## 1. Thiết kế Giao diện Nút bấm (Google Button Layout)

Nút Đăng nhập Google được đặt nằm dưới biểu mẫu đăng nhập bằng email/mật khẩu truyền thống, ngăn cách bởi đường kẻ ngang tinh tế:

```
+-------------------------------------------------------------+
|                     🏥 MYPETCLINIC PORTAL                   |
|                                                             |
|                       [⚡ ĐĂNG NHẬP ]                       |
|                                                             |
|                   ------- HOẶC -------                      |
|                                                             |
|  +-------------------------------------------------------+  |
|  | [ G ] Đăng nhập với tài khoản Google                  |  | (Google Sign-In Button)
|  +-------------------------------------------------------+  |
|                                                             |
|  Bạn chưa có tài khoản? [Đăng ký ngay]                      |
+-------------------------------------------------------------+
```

### 1.1. Cấu trúc thiết kế thương hiệu của Google (Branding Rules)
*   **Màu sắc:** Sử dụng màu nền trắng hoàn toàn (`#FFFFFF`) với viền xám nhạt (`#DADCE0`) hoặc màu tối (`#131314`) để thích ứng với chế độ tối.
*   **Icon Logo Google:** Bắt buộc sử dụng logo Google tiêu chuẩn (G màu đa sắc: Đỏ, Vàng, Xanh lá, Xanh dương) đặt ở góc trái nút. Không bóp méo hoặc đổi màu logo.
*   **Font chữ:** Sử dụng font chữ Google Sans hoặc Roboto với nhãn hiển thị mặc định: *"Đăng nhập với Google"* (Sign in with Google).
*   **Khoảng cách và Bo góc:** Bo tròn góc mặc định 4px (`border-radius: 4px`) để đồng bộ với cấu trúc nút bấm của hệ thống.

---

## 2. Xác thực Trực quan & Phản hồi Tương tác (Visual Feedback)

*   **Trạng thái Hover (Rê chuột):**
    *   *Nền nút:* Đổi từ trắng sang xám cực nhạt (`#F8F9FA`).
    *   *Bóng đổ:* Hiển thị box-shadow phát sáng mịn xung quanh (`box-shadow: 0 1px 3px 1px rgba(60,64,67,.15), 0 1px 2px 0 rgba(60,64,67,.3)`).
    *   *Transition:* Mượt mà trong vòng **150ms** (`transition: background-color 0.15s, box-shadow 0.15s`).
*   **Trạng thái Đang tải (API Exchange Loading):**
    *   Khi người dùng chọn tài khoản Google xong và trình duyệt gửi mã IdToken lên Web API, nút Google bị khóa.
    *   Thay thế icon logo Google bằng một vòng xoay loading spinner đồng màu.

---

## 3. Khai báo Component Vue 3 Tích hợp Nút Google động

Chúng ta sử dụng thư viện Google Identity Services (GIS) để render nút an toàn trực tiếp từ máy chủ Google, đảm bảo chống giả mạo nút bấm (clickjacking):

```html
<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRegisterStore } from '@/store/useRegisterStore';

const googleBtnContainer = ref<HTMLElement | null>(null);

onMounted(() => {
  // 1. Kiểm tra SDK Google đã được tải chưa
  if (typeof google3 !== 'undefined' || (window as any).google) {
    initializeGoogleSignIn();
  } else {
    // Tải động script nếu chưa có
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = initializeGoogleSignIn;
    document.head.appendChild(script);
  }
});

function initializeGoogleSignIn() {
  const google = (window as any).google;
  
  // 2. Khởi tạo cấu hình ứng dụng với Client ID
  google.accounts.id.initialize({
    client_id: 'your-google-client-id-here.apps.googleusercontent.com',
    callback: handleCredentialResponse, // Hàm nhận Token sau khi đăng nhập xong
    auto_select: false
  });

  // 3. Render nút bấm tự động vào Container
  google.accounts.id.renderButton(
    googleBtnContainer.value,
    { 
      theme: 'outline', 
      size: 'large', 
      text: 'signin_with',
      shape: 'rectangular',
      width: '380' 
    }
  );
}

function handleCredentialResponse(response: any) {
  // Gửi response.credential (Google IdToken) lên Pinia Store để xử lý đổi JWT
  console.log("Google Credential Token nhận được:", response.credential);
}
</script>

<template>
  <div class="google-login-container">
    <div ref="googleBtnContainer" class="google-btn"></div>
  </div>
</template>
```
