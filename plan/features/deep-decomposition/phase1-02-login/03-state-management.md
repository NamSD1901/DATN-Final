# 🗃️ State Management Specification - Authentication Store (Pinia)

Tài liệu này đặc tả chi tiết cơ chế quản lý trạng thái đăng nhập, lưu trữ JWT Token an toàn và kiểm soát trạng thái phiên làm việc (Session Management) tại Client bằng Vue 3 Pinia Store.

---

## 🔗 Skills & Quy tắc lập trình liên quan
*   **FE-C01 (Vue 3 Composition API):** Sử dụng các biến phản ứng `ref`, `computed` xây dựng store dạng setup.
*   **FE-C03 (Pinia State Management):** Xây dựng `useAuthStore` tập trung quản lý thông tin định danh và vai trò của người dùng.
*   **Session Security:** Đảm bảo dọn dẹp sạch sẽ thông tin trong bộ nhớ localStorage và Headers của Axios Client khi người dùng nhấn Đăng xuất.

---

## 1. Thiết lập Pinia Store (`useAuthStore.ts`)

Chúng ta xây dựng Store quản trị trạng thái đăng nhập và tự động khôi phục phiên làm việc khi người dùng tải lại trang (F5):

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios, { AxiosError } from 'axios';

// Định nghĩa Interface thông tin người dùng
export interface UserInfo {
  id: string;
  fullName: string;
  email: string;
  role: 'KhachHang' | 'LeTan' | 'BacSi' | 'ThuNgan' | 'QuanTri';
}

export const useAuthStore = defineStore('auth', () => {
  // --- STATE ---
  const token = ref<string | null>(localStorage.getItem('mpc_token'));
  const user = ref<UserInfo | null>(
    localStorage.getItem('mpc_user') ? JSON.parse(localStorage.getItem('mpc_user')!) : null
  );
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string>('');

  // --- GETTERS ---
  const isAuthenticated = computed<boolean>(() => !!token.value);
  const userRole = computed<string>(() => user.value?.role || '');
  const userName = computed<string>(() => user.value?.fullName || '');

  // --- ACTIONS ---

  /**
   * Đăng nhập hệ thống bằng email và mật khẩu
   */
  async function login(email: string, password: string): Promise<boolean> {
    isLoading.value = true;
    errorMessage.value = '';

    try {
      const response = await axios.post('/api/account/login', {
        email: email.trim(),
        password: password
      });

      const { token: jwtToken, user: loggedUser } = response.data;

      // 1. Lưu vào trạng thái phản ứng của Store
      token.value = jwtToken;
      user.value = loggedUser;

      // 2. Lưu vào LocalStorage để duy trì phiên khi F5 (hoặc Cookie tùy chính sách bảo mật)
      localStorage.setItem('mpc_token', jwtToken);
      localStorage.setItem('mpc_user', JSON.stringify(loggedUser));

      // 3. Cấu hình JWT Token vào Authorization Header mặc định của Axios
      axios.defaults.headers.common['Authorization'] = `Bearer ${jwtToken}`;

      return true;
    } catch (error) {
      const err = error as AxiosError<{ message?: string }>;
      errorMessage.value = err.response?.data?.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại kết nối.';
      logout(); // Đảm bảo dọn sạch token rác nếu có lỗi
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Đăng xuất khỏi hệ thống
   */
  function logout() {
    // 1. Reset state về null
    token.value = null;
    user.value = null;
    errorMessage.value = '';

    // 2. Xóa khỏi LocalStorage
    localStorage.removeItem('mpc_token');
    localStorage.removeItem('mpc_user');

    // 3. Xóa Authorization Header trong Axios
    delete axios.defaults.headers.common['Authorization'];
  }

  /**
   * Khởi tạo và khôi phục trạng thái từ LocalStorage khi khởi động ứng dụng
   */
  function initializeAuth() {
    const savedToken = localStorage.getItem('mpc_token');
    const savedUser = localStorage.getItem('mpc_user');

    if (savedToken && savedUser) {
      token.value = savedToken;
      user.value = JSON.parse(savedUser);
      axios.defaults.headers.common['Authorization'] = `Bearer ${savedToken}`;
    } else {
      logout();
    }
  }

  return {
    token,
    user,
    isLoading,
    errorMessage,
    isAuthenticated,
    userRole,
    userName,
    login,
    logout,
    initializeAuth
  };
});
```

---

## 2. Quy tắc Điều hướng theo Vai trò (Router Guard Integration)
Sau khi `login()` thành công và cập nhật `userRole`, Router của Vue 3 (`vue-router`) sẽ đọc thuộc tính này và đưa người dùng đến Dashboard tương ứng:

```typescript
import { useAuthStore } from '@/store/useAuthStore';

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  
  // Kiểm tra route có yêu cầu đăng nhập không
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({ name: 'login' });
  }

  // Tự động điều hướng các vai trò về Dashboard nghiệp vụ riêng
  if (to.name === 'login' && authStore.isAuthenticated) {
    switch (authStore.userRole) {
      case 'BacSi':
        return next({ name: 'doctor-dashboard' });
      case 'LeTan':
        return next({ name: 'receptionist-queue' });
      case 'ThuNgan':
        return next({ name: 'cashier-invoicing' });
      case 'QuanTri':
        return next({ name: 'admin-dashboard' });
      default:
        return next({ name: 'customer-portal' });
    }
  }

  next();
});
```
