# 🟠 Level 3: Advanced (Nâng Cao) - Frontend Skills

Bao gồm các kỹ năng nâng cao về kết nối API xác thực Cookie, tích hợp TypeScript chặt chẽ, và triển khai Premium CSS Design.

---

## 📋 Danh Sách Kỹ Năng
1. [FE-A01: Axios với HTTP-Only Cookie Session Credentials & Tự Động Điều Hướng](#fe-a01-axios-với-http-only-cookie-session-credentials--tự-động-điều-ướng)
2. [FE-A02: Khai báo và Sử dụng Strict TypeScript Models Tập Trung](#fe-a02-khai-báo-và-sử-dụng-strict-typescript-models-tập-trung)
3. [FE-A03: Premium Custom CSS & CSS Variables Integration](#fe-a03-premium-custom-css--css-variables-integration)
4. [FE-A04: Tích hợp Pinia Auth Store và Route Guards](#fe-a04-tích-hợp-pinia-auth-store-và-route-guards)

---

### FE-A01: Axios với HTTP-Only Cookie Session Credentials & Tự Động Điều Hướng

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 2-4 |
| **Tại sao cần?** | Dự án sử dụng Cookie Session thay thế cho JWT LocalStorage nhằm chống tấn công XSS. Mọi instance Axios bắt buộc phải cấu hình `withCredentials: true`. Đồng thời cần tự động xử lý khi phiên đăng nhập hết hạn (401). |

<details>
<summary><b>📚 Cấu hình Axios Client chuẩn (Click để mở rộng)</b></summary>

Nhà phát triển cần hiểu rõ cách thức hoạt động của file cấu hình [services/api.ts](file:///e:/DATN/MyPetClinic/frontend/src/services/api.ts):

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7284/api',
  withCredentials: true, // ⚠️ BẮT BUỘC: Cho phép trình duyệt gửi Cookie Session
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  }
});

// Response Interceptor để bắt lỗi tập trung (Ví dụ 401 Unauthorized)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      console.warn('Phiên đăng nhập hết hạn! Đang chuyển hướng về trang đăng nhập...');
      // Tự động redirect về login khi session hết hạn hoặc không có quyền
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

</details>

---

### FE-A02: Khai báo và Sử dụng Strict TypeScript Models Tập Trung

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐ Advanced |
| **Sprint** | Sprint 3-5 |
| **Tại sao cần?** | Tránh lỗi Runtime. TypeScript giúp kiểm soát chặt chẽ kiểu dữ liệu nhận về từ API và truyền sang Component. Khai báo tập trung giúp tái sử dụng và quản lý code gọn gàng hơn. |

<details>
<summary><b>📚 Cấu trúc và Cách định nghĩa Type an toàn (Click để mở rộng)</b></summary>

**Cấu trúc thư mục khuyến nghị:**
Khai báo toàn bộ models/types tại thư mục `src/shared/types/` hoặc `src/core/types/`. Ví dụ tạo file `src/shared/types/user.ts`:

```typescript
// src/shared/types/user.ts
export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  role: 'admin' | 'doctor' | 'receptionist' | 'customer';
  isActive: boolean;
}
```

Sử dụng trong API services:
```typescript
import api from '../services/api';
import type { UserProfile } from '../shared/types/user';

export async function fetchProfile(): Promise<UserProfile> {
  const response = await api.get<UserProfile>('/profile');
  return response.data; // Trả về đúng kiểu UserProfile
}
```

</details>

---

### FE-A03: Premium Custom CSS & CSS Variables Integration

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 2-5 |
| **Tại sao cần?** | MyPetClinic hướng tới trải nghiệm người dùng cao cấp (Premium). Việc sử dụng thành thạo các class giao diện hiệu ứng kính (Glassmorphic) và hiệu ứng chuyển đổi giúp tối ưu hóa UI/UX. |

<details>
<summary><b>📚 Áp dụng các Class Premium Custom CSS (Click để mở rộng)</b></summary>

Nhà phát triển cần sử dụng linh hoạt các class có sẵn tại [style.css](file:///e:/DATN/MyPetClinic/frontend/src/style.css):

1. **`btn-premium` / `btn-premium-outline`:** Nút bấm màu vàng Gold cao cấp kèm hover hiệu ứng dịch chuyển 3D.
2. **`glass-card`:** Panel kính mờ, tự động đổ bóng và đổi màu viền sang Gold khi hover.
3. **`input-premium`:** Ô nhập dữ liệu với viền tinh tế và hiệu ứng phát sáng mờ khi focus.
4. **`reveal` / `reveal-left` / `reveal-right`:** Các class kết hợp với IntersectionObserver hoặc trigger để tạo hiệu ứng cuốn cuộn hiện hình dần (Scroll Reveal).

```vue
<template>
  <div class="glass-card bg-gold-gradient reveal active">
    <h2 class="gradient-text-gold">Đặt lịch khám nhanh</h2>
    <input type="text" class="input-premium" placeholder="Tên thú cưng của bạn..." />
    <button class="btn-premium hover-glow hover-arrow">
      Tiếp tục <i class="bi-arrow-right"></i>
    </button>
  </div>
</template>
```

</details>

---

### FE-A04: Tích hợp Pinia Auth Store và Route Guards

| Thuộc tính | Chi tiết |
|:---|:---|
| **Mức độ** | ⭐⭐⭐⭐ Mastery |
| **Sprint** | Sprint 3-5 |
| **Tại sao cần?** | Tránh việc gọi API `/profile` liên tục mỗi lần chuyển trang (gây quá tải server và chậm UI). Lưu trữ trạng thái đăng nhập toàn cục trong Pinia Store và kết hợp với Route Guards để kiểm tra nhanh. |

<details>
<summary><b>📚 Code mẫu Auth Store & Router Integration (Click để mở rộng)</b></summary>

```typescript
// stores/auth.ts
import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '../services/api';
import type { UserProfile } from '../shared/types/user';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserProfile | null>(null);
  const isAuthenticated = ref(false);
  const isLoading = ref(false);

  async function checkAuth() {
    if (user.value) return true; // Đã cache thông tin đăng nhập
    isLoading.value = true;
    try {
      const response = await api.get<UserProfile>('/profile');
      user.value = response.data;
      isAuthenticated.value = true;
      return true;
    } catch {
      user.value = null;
      isAuthenticated.value = false;
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function clearAuth() {
    user.value = null;
    isAuthenticated.value = false;
  }

  return { user, isAuthenticated, isLoading, checkAuth, clearAuth };
});
```

Sử dụng trong `router/index.ts`:
```typescript
import { useAuthStore } from '../stores/auth';

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  
  if (to.matched.some(record => record.meta.requiresAuth)) {
    const isAuthed = await authStore.checkAuth();
    if (isAuthed) {
      next();
    } else {
      next('/login');
    }
  } else {
    next();
  }
});
```

</details>
