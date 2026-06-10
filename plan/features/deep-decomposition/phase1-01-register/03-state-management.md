# 🗃️ State Management Specification - Register & Activation (Pinia)

Tài liệu này đặc tả chi tiết cơ chế quản lý trạng thái tập trung cho luồng đăng ký tài khoản và kích hoạt OTP tại Client bằng Vue 3 Pinia Store viết bằng TypeScript.

---

## 🔗 Skills & Quy tắc lập trình liên quan
*   **FE-C01 (Vue 3 Composition API):** Sử dụng các ref phản ứng (`ref`, `computed`) theo phong cách Composition Setup Store.
*   **FE-C03 (Pinia State Management):** Định nghĩa Store an toàn kiểu dữ liệu (strongly-typed), chia tách rõ ràng giữa State, Getters và Actions.
*   **API Client integration:** Tích hợp gọi Axios Client bất đồng bộ, xử lý bóc tách mã lỗi từ Backend Response.

---

## 1. Khai báo Pinia Store (`useRegisterStore.ts`)

Chúng ta xây dựng Store quản lý toàn bộ vòng đời của biểu mẫu đăng ký, bao gồm quản lý bước giao diện, trạng thái loading, lỗi từ API, và các bộ đếm ngược (countdown) thời gian hết hạn mã OTP.

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios, { AxiosError } from 'axios';

// Định nghĩa Interface dữ liệu gửi đi khi đăng ký
export interface RegisterPayload {
  fullName: string;
  email: string;
  phoneNumber: string;
  password?: string;
}

// Định nghĩa cấu trúc lỗi trả về từ Web API
interface ApiErrorResponse {
  message?: string;
  errorType?: string;
  details?: any;
}

export const useRegisterStore = defineStore('register', () => {
  // --- STATE ---
  const currentStep = ref<'register-form' | 'otp-verification'>('register-form');
  const userEmail = ref<string>('');
  const userFullName = ref<string>('');
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string>('');
  const successMessage = ref<string>('');
  const otpResendCountdown = ref<number>(0);
  let countdownTimer: number | null = null;

  // --- GETTERS ---
  const isOtpStep = computed<boolean>(() => currentStep.value === 'otp-verification');
  const canResendOtp = computed<boolean>(() => otpResendCountdown.value === 0);
  
  // --- ACTIONS ---

  /**
   * Khởi động bộ đếm ngược thời gian cho phép gửi lại OTP
   */
  function startCountdown(durationSeconds: number = 60) {
    if (countdownTimer) {
      clearInterval(countdownTimer);
    }
    otpResendCountdown.value = durationSeconds;
    countdownTimer = window.setInterval(() => {
      if (otpResendCountdown.value > 0) {
        otpResendCountdown.value--;
      } else {
        if (countdownTimer) {
          clearInterval(countdownTimer);
          countdownTimer = null;
        }
      }
    }, 1000);
  }

  /**
   * Đăng ký tài khoản mới
   */
  async function submitRegistration(payload: RegisterPayload): Promise<boolean> {
    isLoading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    try {
      // Gọi API Register của Backend
      const response = await axios.post('/api/account/register', payload);
      
      userEmail.value = payload.email.toLowerCase().trim();
      userFullName.value = payload.fullName.trim();
      
      // Chuyển sang màn hình xác thực OTP và bắt đầu đếm ngược 60 giây
      currentStep.value = 'otp-verification';
      successMessage.value = response.data?.message || 'Đăng ký tài khoản thành công. Vui lòng nhập OTP.';
      startCountdown(60); 
      
      return true;
    } catch (error) {
      const err = error as AxiosError<ApiErrorResponse>;
      errorMessage.value = err.response?.data?.message || 'Đã xảy ra lỗi trong quá trình kết nối đăng ký.';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Xác thực mã kích hoạt OTP
   */
  async function verifyOtpCode(otpCode: string): Promise<boolean> {
    isLoading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    try {
      const response = await axios.post('/api/account/verify-otp', {
        email: userEmail.value,
        otpCode: otpCode.trim()
      });

      // Kích hoạt thành công, reset trạng thái biểu mẫu về ban đầu
      resetStore();
      successMessage.value = response.data?.message || 'Tài khoản đã kích hoạt thành công. Đang điều hướng đăng nhập...';
      return true;
    } catch (error) {
      const err = error as AxiosError<ApiErrorResponse>;
      errorMessage.value = err.response?.data?.message || 'Mã xác thực OTP không chính xác hoặc đã hết hạn.';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Yêu cầu gửi lại mã OTP mới (Resend OTP)
   */
  async function resendOtpCode(): Promise<boolean> {
    if (!canResendOtp.value) return false;

    isLoading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    try {
      const response = await axios.post('/api/account/resend-otp', {
        email: userEmail.value
      });

      successMessage.value = response.data?.message || 'Một mã OTP mới đã được gửi tới email của bạn.';
      startCountdown(60); // Đặt lại bộ đếm chờ 60 giây tiếp theo
      return true;
    } catch (error) {
      const err = error as AxiosError<ApiErrorResponse>;
      errorMessage.value = err.response?.data?.message || 'Không thể gửi lại mã OTP. Vui lòng thử lại sau.';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Dọn sạch trạng thái Store
   */
  function resetStore() {
    currentStep.value = 'register-form';
    userEmail.value = '';
    userFullName.value = '';
    isLoading.value = false;
    errorMessage.value = '';
    successMessage.value = '';
    otpResendCountdown.value = 0;
    if (countdownTimer) {
      clearInterval(countdownTimer);
      countdownTimer = null;
    }
  }

  return {
    currentStep,
    userEmail,
    userFullName,
    isLoading,
    errorMessage,
    successMessage,
    otpResendCountdown,
    isOtpStep,
    canResendOtp,
    submitRegistration,
    verifyOtpCode,
    resendOtpCode,
    resetStore
  };
});
```
*Lưu ý: Store trên tự động giải phóng tài nguyên timer `clearInterval` trong hàm `resetStore` để tránh hiện tượng rò rỉ bộ nhớ (Memory Leak) khi component Vue bị hủy bỏ.*
