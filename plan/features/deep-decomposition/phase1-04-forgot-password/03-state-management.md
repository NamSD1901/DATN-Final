# 🗃️ State Management Specification - Forgot Password Store (Pinia)

Tài liệu này đặc tả chi tiết cơ chế quản lý trạng thái biểu mẫu wizard 3 bước khôi phục mật khẩu, kiểm soát tải dữ liệu và gọi API khôi phục bằng Vue 3 Pinia Store.

---

## 🔗 Skills & Quy tắc lập trình liên quan
*   **FE-C01 (Vue 3 Composition API):** Sử dụng `ref` và `computed` xây dựng store phản ứng nhanh.
*   **FE-C03 (Pinia State Management):** Đóng gói toàn bộ logic gọi API khôi phục vào trong `useForgotPasswordStore` nhằm tách biệt tầng giao diện và tầng kết nối dữ liệu.

---

## 1. Thiết lập Pinia Store (`useForgotPasswordStore.ts`)

Chúng ta xây dựng Store quản trị trạng thái khôi phục mật khẩu từng bước:

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import axios, { AxiosError } from 'axios';

export type ForgotPasswordStep = 'enter-email' | 'enter-otp' | 'reset-password';

interface ApiErrorResponse {
  message?: string;
  errorType?: string;
}

export const useForgotPasswordStore = defineStore('forgotPassword', () => {
  // --- STATE ---
  const currentStep = ref<ForgotPasswordStep>('enter-email');
  const userEmail = ref<string>('');
  const otpCode = ref<string>('');
  const isLoading = ref<boolean>(false);
  const errorMessage = ref<string>('');
  const successMessage = ref<string>('');
  const resendCountdown = ref<number>(0);
  let countdownTimer: number | null = null;

  // --- GETTERS ---
  const canResend = computed<boolean>(() => resendCountdown.value === 0);

  // --- ACTIONS ---

  function startCountdown(seconds: number = 60) {
    if (countdownTimer) clearInterval(countdownTimer);
    resendCountdown.value = seconds;
    countdownTimer = window.setInterval(() => {
      if (resendCountdown.value > 0) {
        resendCountdown.value--;
      } else {
        if (countdownTimer) {
          clearInterval(countdownTimer);
          countdownTimer = null;
        }
      }
    }, 1000);
  }

  /**
   * Bước 1: Gửi yêu cầu mã OTP khôi phục về Email
   */
  async function requestRecoveryOtp(email: string): Promise<boolean> {
    isLoading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    try {
      const response = await axios.post('/api/account/forgot-password', {
        email: email.trim()
      });

      userEmail.value = email.toLowerCase().trim();
      currentStep.value = 'enter-otp';
      successMessage.value = response.data?.message || 'Yêu cầu thành công. Vui lòng kiểm tra email lấy OTP.';
      startCountdown(60);
      return true;
    } catch (error) {
      const err = error as AxiosError<ApiErrorResponse>;
      errorMessage.value = err.response?.data?.message || 'Có lỗi xảy ra khi gửi yêu cầu.';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Bước 2 & 3: Đặt lại mật khẩu mới cùng mã OTP
   */
  async function submitPasswordReset(newPassword: string): Promise<boolean> {
    isLoading.value = true;
    errorMessage.value = '';
    successMessage.value = '';

    try {
      const response = await axios.post('/api/account/reset-password', {
        email: userEmail.value,
        otpCode: otpCode.value.trim(),
        newPassword: newPassword
      });

      successMessage.value = response.data?.message || 'Mật khẩu của bạn đã được đặt lại thành công.';
      // Hoàn tất khôi phục, reset store dọn dẹp bộ đếm
      resetStore();
      return true;
    } catch (error) {
      const err = error as AxiosError<ApiErrorResponse>;
      errorMessage.value = err.response?.data?.message || 'Không thể đặt lại mật khẩu mới. Vui lòng kiểm tra lại OTP.';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  /**
   * Gửi lại mã OTP khôi phục mật khẩu
   */
  async function resendRecoveryOtp(): Promise<boolean> {
    if (!canResend.value) return false;
    return requestRecoveryOtp(userEmail.value);
  }

  function resetStore() {
    currentStep.value = 'enter-email';
    userEmail.value = '';
    otpCode.value = '';
    isLoading.value = false;
    errorMessage.value = '';
    successMessage.value = '';
    resendCountdown.value = 0;
    if (countdownTimer) {
      clearInterval(countdownTimer);
      countdownTimer = null;
    }
  }

  return {
    currentStep,
    userEmail,
    otpCode,
    isLoading,
    errorMessage,
    successMessage,
    resendCountdown,
    canResend,
    requestRecoveryOtp,
    submitPasswordReset,
    resendRecoveryOtp,
    resetStore
  };
});
```
