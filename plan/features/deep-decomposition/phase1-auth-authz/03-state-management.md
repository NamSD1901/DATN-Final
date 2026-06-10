# 🗃️ State Management (Pinia Store) - Authentication & Authorization

## 1. Vai trò của Pinia Store trong Hệ thống Xác thực
Pinia Store (`authStore`) đóng vai trò là "bộ não" điều phối toàn bộ trạng thái bảo mật của phiên làm việc phía client:
1.  **Lưu trữ Token & Claims:** Lưu giữ JWT Access Token để đính kèm tự động vào Header của mọi request gửi lên API qua interceptor.
2.  **Lưu trạng thái Đăng nhập (Reactivity):** Cung cấp các cờ reactive như `isAuthenticated` và `userRole` giúp giao diện ẩn/hiển thị các nút chức năng tương ứng (ví dụ: giỏ hàng, nút đặt lịch, menu admin).
3.  **Bảo vệ Route (Client-side Guard):** Phối hợp trực tiếp với Vue Router để chặn người dùng truy cập các trang Dashboard chuyên môn khi chưa đăng nhập hoặc sai vai trò.

---

## 2. Mã nguồn Pinia Store TypeScript chi tiết

Dưới đây là mã nguồn đặc tả đầy đủ cho `authStore` viết bằng TypeScript:

```typescript
import { defineStore } from 'pinia';
import api from '@/services/api';

export interface UserSession {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  role: string; // admin, doctor, receptionist, customer
  avatar: string | null;
}

interface AuthState {
  user: UserSession | null;
  token: string | null;
  loading: boolean;
  error: string | null;
  requiresOtpVerification: boolean; // Cờ báo tài khoản cần kích hoạt OTP
  otpEmailTarget: string | null;     // Lưu tạm email nhận OTP
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: null,
    token: localStorage.getItem('token'), // Lấy token từ cache lưu trữ
    loading: false,
    error: null,
    requiresOtpVerification: false,
    otpEmailTarget: null,
  }),

  getters: {
    isAuthenticated(state): boolean {
      return !!state.token && !!state.user;
    },
    userRole(state): string | null {
      return state.user ? state.user.role : null;
    }
  },

  actions: {
    /**
     * Khởi tạo và thiết lập phiên làm việc sau khi đăng nhập thành công
     */
    setSession(token: string, user: UserSession) {
      this.token = token;
      this.user = user;
      localStorage.setItem('token', token);
      
      // Cập nhật cấu hình Authorization header mặc định của Axios
      api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
    },

    /**
     * Đăng nhập bằng tài khoản email & mật khẩu
     */
    async login(payload: any): Promise<boolean> {
      this.loading = true;
      this.error = null;
      this.requiresOtpVerification = false;
      this.otpEmailTarget = null;

      try {
        const response = await api.post<{
          success: boolean;
          token?: string;
          user?: UserSession;
          requiresOtp?: boolean;
          email?: string;
          message: string;
        }>('/account/login', payload);

        if (response.data.requiresOtp && response.data.email) {
          this.requiresOtpVerification = true;
          this.otpEmailTarget = response.data.email;
          return false;
        }

        if (response.data.token && response.data.user) {
          this.setSession(response.data.token, response.data.user);
          return true;
        }

        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Đăng nhập thất bại. Vui lòng thử lại.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Đăng nhập nhanh bằng Google OAuth Token
     */
    async loginWithGoogle(googleIdToken: string): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.post<{
          success: boolean;
          token: string;
          user: UserSession;
        }>('/account/google-login', { idToken: googleIdToken });

        if (response.data.token && response.data.user) {
          this.setSession(response.data.token, response.data.user);
          return true;
        }
        return false;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể đăng nhập bằng tài khoản Google.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Xác thực mã OTP để kích hoạt tài khoản / đổi mật khẩu
     */
    async verifyOtp(email: string, otpCode: string, purpose: string): Promise<boolean> {
      this.loading = true;
      this.error = null;
      try {
        const response = await api.post<{
          success: boolean;
          token?: string;
          user?: UserSession;
          message: string;
        }>('/account/verify-otp', { email, otpCode, purpose });

        if (response.data.token && response.data.user) {
          this.setSession(response.data.token, response.data.user);
          this.requiresOtpVerification = false;
          this.otpEmailTarget = null;
        }
        return true;
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Mã xác thực OTP không chính xác hoặc đã hết hạn.';
        return false;
      } finally {
        this.loading = false;
      }
    },

    /**
     * Đăng xuất hệ thống, dọn dẹp bộ nhớ cache
     */
    logout() {
      this.user = null;
      this.token = null;
      this.requiresOtpVerification = false;
      this.otpEmailTarget = null;
      
      localStorage.removeItem('token');
      delete api.defaults.headers.common['Authorization'];
    }
  }
});
```
