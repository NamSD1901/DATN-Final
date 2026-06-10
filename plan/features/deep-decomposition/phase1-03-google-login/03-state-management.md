# 🗃️ State Management Specification - Google OAuth Action (Pinia)

Tài liệu này đặc tả cơ chế quản lý trạng thái, gửi yêu cầu trao đổi mã định danh của Google lấy JWT Token và cập nhật bộ nhớ Client thông qua Pinia Store.

---

## 🔗 Skills & Quy tắc lập trình liên quan
*   **FE-C03 (Pinia State Management):** Đóng gói logic giao tiếp OAuth bên ngoài vào trong `useAuthStore` của ứng dụng để đảm bảo tính đóng gói (Encapsulation).
*   **Session Storage:** Lưu trữ an toàn phiên làm việc của người dùng vừa đăng nhập thành công.

---

## 1. Nâng cấp Store hỗ trợ Đăng nhập Google (`useAuthStore.ts`)

Chúng ta mở rộng `useAuthStore` viết bằng TypeScript để tích hợp thêm action `loginWithGoogle`:

```typescript
import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios, { AxiosError } from 'axios';
import { useRouter } from 'vue-router';

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('mpc_token'));
  const user = ref<any>(
    localStorage.getItem('mpc_user') ? JSON.parse(localStorage.getItem('mpc_user')!) : null
  );
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string>('');
  
  const router = useRouter();

  /**
   * Thực hiện gửi Google IdToken lên Backend để nhận JWT Token của MyPetClinic
   * @param googleIdToken Chuỗi Token nhận được từ Google Popup
   */
  async function loginWithGoogle(googleIdToken: string): Promise<boolean> {
    isLoading.value = true;
    errorMessage.value = '';

    try {
      // Gọi API đổi Token của Backend
      const response = await axios.post('/api/account/google-login', {
        idToken: googleIdToken
      });

      const { token: mpcToken, user: loggedUser } = response.data;

      // 1. Cập nhật dữ liệu vào Store phản ứng
      token.value = mpcToken;
      user.value = loggedUser;

      // 2. Lưu trữ vào localStorage để duy trì trạng thái khi người dùng tải lại trang
      localStorage.setItem('mpc_token', mpcToken);
      localStorage.setItem('mpc_user', JSON.stringify(loggedUser));

      // 3. Thiết lập header mặc định cho tất cả các request Axios tiếp theo
      axios.defaults.headers.common['Authorization'] = `Bearer ${mpcToken}`;

      // 4. Tự động chuyển hướng khách hàng vào Customer Portal
      router.push({ name: 'customer-portal' });

      return true;
    } catch (error) {
      const err = error as AxiosError<{ message?: string }>;
      
      // Bóc tách lỗi từ backend (ví dụ: Google Client ID không khớp, Token hết hạn)
      errorMessage.value = err.response?.data?.message || 'Đăng nhập Google thất bại. Vui lòng thử lại.';
      
      // Xóa sạch thông tin cũ phòng trường hợp lỗi
      logout();
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function logout() {
    token.value = null;
    user.value = null;
    localStorage.removeItem('mpc_token');
    localStorage.removeItem('mpc_user');
    delete axios.defaults.headers.common['Authorization'];
  }

  return {
    token,
    user,
    isLoading,
    errorMessage,
    loginWithGoogle,
    logout
  };
});
```

---

## 2. Đồng bộ Luồng chuyển dịch Giao diện (Routing Flow)
*   **Trường hợp Đăng nhập Google thành công:** Người dùng luôn luôn được hệ thống gán vai trò mặc định là `KhachHang` (vì chỉ khách hàng/chủ nuôi thú cưng mới được phép tự đăng ký bằng Google).
*   **Trường hợp Tài khoản có vai trò đặc biệt:** Nếu Email của tài khoản Google trùng khớp với Email nhân sự phòng khám đã được Admin khởi tạo trước đó (ví dụ: `doctor.minh@gmail.com` với vai trò `BacSi`), hệ thống tự động gán đúng vai trò của nhân viên đó và đưa họ vào Portal nghiệp vụ chuyên biệt thay vì Customer Portal.
