<template>
  <div class="login-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <div class="login-card">
      <!-- Clinic Branding -->
      <div class="brand">
        <div class="brand-icon">
          <PawPrint class="icon-paw" />
        </div>
        <h1 class="brand-name">MyPetClinic</h1>
        <p class="brand-tagline">Chăm sóc thú cưng của bạn bằng cả trái tim</p>
      </div>

      <!-- Toast Notifications -->
      <TransitionGroup name="toast-fade" tag="div" class="toast-container">
        <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
          <component :is="toast.icon" class="toast-icon" />
          <span class="toast-message">{{ toast.message }}</span>
        </div>
      </TransitionGroup>

      <!-- Main Login View -->
      <div v-if="currentStep === 'login'" class="form-container">
        <h2 class="form-title">Đăng Nhập</h2>
        <p class="form-subtitle">Chào mừng bạn quay trở lại với chúng tôi</p>

        <form @submit.prevent="handleLogin" class="form">
          <div class="input-group">
            <label for="email">Địa chỉ Email</label>
            <div class="input-wrapper">
              <Mail class="input-icon" />
              <input 
                id="email" 
                type="email" 
                v-model="loginForm.email" 
                placeholder="example@gmail.com" 
                required 
                class="form-input"
              />
            </div>
          </div>

          <div class="input-group">
            <div class="label-row">
              <label for="password">Mật khẩu</label>
              <a href="#" @click.prevent="goToForgotPassword" class="forgot-link">Quên mật khẩu?</a>
            </div>
            <div class="input-wrapper">
              <Lock class="input-icon" />
              <input 
                id="password" 
                :type="showPassword ? 'text' : 'password'" 
                v-model="loginForm.password" 
                placeholder="••••••••" 
                required 
                class="form-input"
              />
              <button type="button" @click="showPassword = !showPassword" class="eye-btn">
                <Eye v-if="!showPassword" class="eye-icon" />
                <EyeOff v-else class="eye-icon" />
              </button>
            </div>
          </div>

          <div class="remember-me">
            <label class="checkbox-container">
              <input type="checkbox" v-model="loginForm.rememberMe" />
              <span class="checkmark"></span>
              Ghi nhớ đăng nhập
            </label>
          </div>

          <button type="submit" :disabled="loading" class="btn-submit">
            <span v-if="!loading">Đăng Nhập</span>
            <div v-else class="spinner"></div>
          </button>
        </form>

        <div class="divider">
          <span>Hoặc đăng nhập bằng</span>
        </div>

        <button @click="handleGoogleLogin" class="btn-google">
          <svg class="google-icon" viewBox="0 0 24 24" width="18" height="18" xmlns="http://www.w3.org/2000/svg">
            <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"/>
            <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"/>
            <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z" fill="#FBBC05"/>
            <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.53 12-4.53z" fill="#EA4335"/>
          </svg>
          <span>Đăng nhập bằng Google</span>
        </button>

        <div class="register-footer">
          Chưa có tài khoản? <router-link to="/register" class="register-link">Đăng ký ngay</router-link>
        </div>
      </div>

      <!-- OTP Verification View -->
      <div v-else-if="currentStep === 'otp'" class="form-container">
        <h2 class="form-title">Xác thực OTP</h2>
        <p class="form-subtitle">Chúng tôi đã gửi mã xác thực tới <strong class="highlight-email">{{ targetEmail }}</strong></p>

        <form @submit.prevent="handleVerifyOtp" class="form">
          <div class="input-group">
            <label for="otp">Mã xác thực OTP</label>
            <div class="input-wrapper">
              <ShieldCheck class="input-icon" />
              <input 
                id="otp" 
                type="text" 
                v-model="otpForm.otpCode" 
                placeholder="Nhập 6 ký tự OTP" 
                required 
                maxlength="6"
                class="form-input otp-input"
              />
            </div>
          </div>

          <button type="submit" :disabled="loading" class="btn-submit">
            <span v-if="!loading">Xác Nhận OTP</span>
            <div v-else class="spinner"></div>
          </button>
          
          <button type="button" @click="handleResendOtp" :disabled="resendCountdown > 0 || loading" class="btn-secondary">
            {{ resendCountdown > 0 ? `Gửi lại sau (${resendCountdown}s)` : 'Gửi lại mã OTP' }}
          </button>
          
          <button type="button" @click="goBackToLogin" class="btn-back">
            Quay lại Đăng nhập
          </button>
        </form>
      </div>

      <!-- Forgot Password View -->
      <div v-else-if="currentStep === 'forgot'" class="form-container">
        <h2 class="form-title">Quên Mật Khẩu</h2>
        <p class="form-subtitle">Nhập email của bạn để nhận mã OTP khôi phục mật khẩu</p>

        <form @submit.prevent="handleForgotPassword" class="form">
          <div class="input-group">
            <label for="forgot-email">Email khôi phục</label>
            <div class="input-wrapper">
              <Mail class="input-icon" />
              <input 
                id="forgot-email" 
                type="email" 
                v-model="forgotForm.email" 
                placeholder="example@gmail.com" 
                required 
                class="form-input"
              />
            </div>
          </div>

          <button type="submit" :disabled="loading" class="btn-submit">
            <span v-if="!loading">Gửi OTP Khôi Phục</span>
            <div v-else class="spinner"></div>
          </button>

          <button type="button" @click="goBackToLogin" class="btn-back">
            Quay lại Đăng nhập
          </button>
        </form>
      </div>

      <!-- Reset Password View -->
      <div v-else-if="currentStep === 'reset'" class="form-container">
        <h2 class="form-title">Đặt Lại Mật Khẩu</h2>
        <p class="form-subtitle">Tạo mật khẩu mới cho tài khoản của bạn</p>

        <form @submit.prevent="handleResetPassword" class="form">
          <div class="input-group">
            <label for="reset-otp">Mã OTP</label>
            <div class="input-wrapper">
              <ShieldCheck class="input-icon" />
              <input 
                id="reset-otp" 
                type="text" 
                v-model="resetForm.otpCode" 
                placeholder="Nhập mã OTP" 
                required 
                class="form-input"
              />
            </div>
          </div>

          <div class="input-group">
            <label for="new-password">Mật khẩu mới</label>
            <div class="input-wrapper">
              <Lock class="input-icon" />
              <input 
                id="new-password" 
                type="password" 
                v-model="resetForm.newPassword" 
                placeholder="Mật khẩu mới" 
                required 
                class="form-input"
              />
            </div>
          </div>

          <div class="input-group">
            <label for="confirm-password">Xác nhận mật khẩu</label>
            <div class="input-wrapper">
              <Lock class="input-icon" />
              <input 
                id="confirm-password" 
                type="password" 
                v-model="resetForm.confirmPassword" 
                placeholder="Nhập lại mật khẩu mới" 
                required 
                class="form-input"
              />
            </div>
          </div>

          <button type="submit" :disabled="loading" class="btn-submit">
            <span v-if="!loading">Đổi Mật Khẩu</span>
            <div v-else class="spinner"></div>
          </button>

          <button type="button" @click="goBackToLogin" class="btn-back">
            Hủy và quay lại
          </button>
        </form>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { 
  PawPrint, 
  Mail, 
  Lock, 
  Eye, 
  EyeOff, 
  ShieldCheck, 
  AlertCircle, 
  CheckCircle2, 
  Info 
} from '@lucide/vue';

const router = useRouter();

// Steps: 'login', 'otp', 'forgot', 'reset'
const currentStep = ref<'login' | 'otp' | 'forgot' | 'reset'>('login');
const loading = ref(false);
const showPassword = ref(false);
const targetEmail = ref('');
const resendCountdown = ref(0);
let countdownInterval: any = null;

// Toast Notification State
interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
  icon: any;
}
const toasts = ref<Toast[]>([]);
let toastId = 0;

const showToast = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
  const id = toastId++;
  let icon = Info;
  if (type === 'success') icon = CheckCircle2;
  if (type === 'error') icon = AlertCircle;

  toasts.value.push({ id, message, type, icon });
  setTimeout(() => {
    toasts.value = toasts.value.filter(t => t.id !== id);
  }, 4000);
};

const showSuccessToast = (msg: string) => showToast(msg, 'success');
const showErrorToast = (msg: string) => showToast(msg, 'error');
const showInfoToast = (msg: string) => showToast(msg, 'info');

// Forms State
const loginForm = reactive({
  email: '',
  password: '',
  rememberMe: false
});

const otpForm = reactive({
  otpCode: ''
});

const forgotForm = reactive({
  email: ''
});

const resetForm = reactive({
  otpCode: '',
  newPassword: '',
  confirmPassword: ''
});

// Resend OTP Countdown logic
const startCountdown = (seconds = 60) => {
  resendCountdown.value = seconds;
  clearInterval(countdownInterval);
  countdownInterval = setInterval(() => {
    if (resendCountdown.value > 0) {
      resendCountdown.value--;
    } else {
      clearInterval(countdownInterval);
    }
  }, 1000);
};

// Handlers
const handleLogin = async () => {
  loading.value = true;
  try {
    const response = await api.post('/account/login', loginForm);
    if (response.data.success) {
      showSuccessToast('Đăng nhập thành công! Đang chuyển hướng...');
      setTimeout(() => {
        router.push('/dashboard');
      }, 1000);
    }
  } catch (error: any) {
    const res = error.response?.data;
    if (res?.requiresOtp) {
      showInfoToast(res.message || 'Tài khoản cần xác thực OTP.');
      targetEmail.value = res.email || loginForm.email;
      currentStep.value = 'otp';
      startCountdown(60);
    } else {
      showErrorToast(res?.message || 'Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.');
    }
  } finally {
    loading.value = false;
  }
};

const handleVerifyOtp = async () => {
  loading.value = true;
  try {
    const response = await api.post('/account/verify-otp', {
      email: targetEmail.value,
      otpCode: otpForm.otpCode
    });
    if (response.data.success) {
      showSuccessToast('Xác thực tài khoản thành công! Bạn có thể đăng nhập ngay.');
      currentStep.value = 'login';
      otpForm.otpCode = '';
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Mã OTP không chính xác hoặc đã hết hạn.');
  } finally {
    loading.value = false;
  }
};

const handleResendOtp = async () => {
  loading.value = true;
  try {
    const response = await api.post('/account/resend-otp', {
      email: targetEmail.value,
      type: 'Verification' // Hoặc 'ResetPassword' tùy theo ngữ cảnh
    });
    if (response.data.success) {
      showSuccessToast('Mã OTP mới đã được gửi lại vào Email của bạn.');
      startCountdown(60);
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Không thể gửi lại OTP.');
  } finally {
    loading.value = false;
  }
};

const handleForgotPassword = async () => {
  loading.value = true;
  try {
    const response = await api.post('/account/forgot-password', {
      email: forgotForm.email
    });
    if (response.data.success) {
      showSuccessToast('Đã gửi mã OTP đặt lại mật khẩu.');
      targetEmail.value = forgotForm.email;
      resetForm.otpCode = '';
      currentStep.value = 'reset';
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Địa chỉ email không tồn tại trong hệ thống.');
  } finally {
    loading.value = false;
  }
};

const handleResetPassword = async () => {
  if (resetForm.newPassword !== resetForm.confirmPassword) {
    showErrorToast('Mật khẩu xác nhận không khớp.');
    return;
  }
  loading.value = true;
  try {
    const response = await api.post('/account/reset-password', {
      email: targetEmail.value,
      otpCode: resetForm.otpCode,
      newPassword: resetForm.newPassword,
      confirmPassword: resetForm.confirmPassword
    });
    if (response.data.success) {
      showSuccessToast(response.data.message || 'Đổi mật khẩu thành công! Hãy đăng nhập.');
      currentStep.value = 'login';
      resetForm.otpCode = '';
      resetForm.newPassword = '';
      resetForm.confirmPassword = '';
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại OTP.');
  } finally {
    loading.value = false;
  }
};

const handleGoogleLogin = () => {
  showInfoToast('Đang kết nối tới Google...');
  // Gọi trực tiếp API redirect của backend
  window.location.href = 'https://localhost:7284/api/account/google-login';
};

const goToForgotPassword = () => {
  forgotForm.email = loginForm.email;
  currentStep.value = 'forgot';
};

const goBackToLogin = () => {
  currentStep.value = 'login';
};

onUnmounted(() => {
  clearInterval(countdownInterval);
});
</script>

<style scoped>
/* Google Font & Layout styling */
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.login-wrapper {
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: radial-gradient(circle at top right, #1e293b, #0f172a, #0b0f19);
  font-family: 'Outfit', sans-serif;
  overflow: hidden;
  padding: 20px;
}

/* Background ambient glows */
.bg-glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.15;
  z-index: 0;
  pointer-events: none;
}

.bg-glow-1 {
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, #0d9488, transparent);
  top: -100px;
  right: -100px;
}

.bg-glow-2 {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, #6366f1, transparent);
  bottom: -50px;
  left: -50px;
}

/* Premium Card with Glassmorphism */
.login-card {
  position: relative;
  z-index: 1;
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 24px;
  padding: 3rem 2.5rem;
  width: 100%;
  max-width: 480px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.login-card:hover {
  box-shadow: 0 24px 50px rgba(13, 148, 136, 0.15);
}

/* Brand Section */
.brand {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 2.2rem;
  text-align: center;
}

.brand-icon {
  background: linear-gradient(135deg, #14b8a6, #6366f1);
  padding: 0.8rem;
  border-radius: 16px;
  display: flex;
  justify-content: center;
  align-items: center;
  box-shadow: 0 8px 20px rgba(20, 184, 166, 0.3);
  margin-bottom: 0.8rem;
  transition: transform 0.3s ease;
}

.brand-icon:hover {
  transform: rotate(10deg) scale(1.05);
}

.icon-paw {
  color: white;
  width: 28px;
  height: 28px;
}

.brand-name {
  color: #ffffff;
  font-size: 2rem;
  font-weight: 700;
  letter-spacing: -0.5px;
  margin-bottom: 0.2rem;
}

.brand-tagline {
  color: #94a3b8;
  font-size: 0.9rem;
  font-weight: 400;
}

/* Forms general */
.form-container {
  display: flex;
  flex-direction: column;
}

.form-title {
  color: #ffffff;
  font-size: 1.4rem;
  font-weight: 600;
  margin-bottom: 0.3rem;
}

.form-subtitle {
  color: #94a3b8;
  font-size: 0.9rem;
  margin-bottom: 1.8rem;
}

.highlight-email {
  color: #14b8a6;
  font-weight: 500;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 1.2rem;
}

/* Inputs styling */
.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.input-group label {
  color: #cbd5e1;
  font-size: 0.85rem;
  font-weight: 500;
}

.label-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 14px;
  color: #64748b;
  width: 18px;
  height: 18px;
  pointer-events: none;
}

.form-input {
  width: 100%;
  padding: 0.85rem 1rem 0.85rem 2.8rem;
  background: rgba(15, 23, 42, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  color: #ffffff;
  font-size: 0.95rem;
  font-family: inherit;
  transition: all 0.3s ease;
}

.form-input::placeholder {
  color: #475569;
}

.form-input:focus {
  outline: none;
  border-color: #14b8a6;
  box-shadow: 0 0 0 3px rgba(20, 184, 166, 0.15);
  background: rgba(15, 23, 42, 0.8);
}

.eye-btn {
  position: absolute;
  right: 12px;
  background: none;
  border: none;
  color: #64748b;
  cursor: pointer;
  display: flex;
  align-items: center;
  padding: 4px;
}

.eye-icon {
  width: 18px;
  height: 18px;
}

.eye-btn:hover {
  color: #94a3b8;
}

/* Remember Me Checkbox */
.remember-me {
  margin: 0.2rem 0;
}

.checkbox-container {
  display: flex;
  align-items: center;
  position: relative;
  padding-left: 28px;
  cursor: pointer;
  color: #94a3b8;
  font-size: 0.85rem;
  user-select: none;
}

.checkbox-container input {
  position: absolute;
  opacity: 0;
  cursor: pointer;
  height: 0;
  width: 0;
}

.checkmark {
  position: absolute;
  top: 0;
  left: 0;
  height: 18px;
  width: 18px;
  background-color: rgba(15, 23, 42, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 5px;
  transition: all 0.2s ease;
}

.checkbox-container:hover input ~ .checkmark {
  border-color: #14b8a6;
}

.checkbox-container input:checked ~ .checkmark {
  background-color: #14b8a6;
  border-color: #14b8a6;
}

.checkmark:after {
  content: "";
  position: absolute;
  display: none;
}

.checkbox-container input:checked ~ .checkmark:after {
  display: block;
}

.checkbox-container .checkmark:after {
  left: 6px;
  top: 2px;
  width: 4px;
  height: 9px;
  border: solid white;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg);
}

/* Buttons */
.btn-submit {
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: #ffffff;
  border: none;
  border-radius: 12px;
  padding: 0.9rem;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 0.5rem;
}

.btn-submit:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(20, 184, 166, 0.35);
  background: linear-gradient(135deg, #2dd4bf, #14b8a6);
}

.btn-submit:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.btn-secondary {
  background: rgba(255, 255, 255, 0.05);
  color: #cbd5e1;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 0.9rem;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-secondary:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
}

.btn-secondary:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-back {
  background: none;
  border: none;
  color: #94a3b8;
  font-size: 0.9rem;
  cursor: pointer;
  margin-top: 0.5rem;
  transition: color 0.2s;
  text-decoration: underline;
}

.btn-back:hover {
  color: #ffffff;
}

/* Forgot and register links */
.forgot-link {
  color: #6366f1;
  font-size: 0.85rem;
  text-decoration: none;
  transition: color 0.2s;
}

.forgot-link:hover {
  color: #818cf8;
  text-decoration: underline;
}

.divider {
  display: flex;
  align-items: center;
  text-align: center;
  color: #475569;
  font-size: 0.8rem;
  margin: 1.5rem 0;
}

.divider::before, .divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.divider::before {
  margin-right: .5em;
}

.divider::after {
  margin-left: .5em;
}

.btn-google {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  background: #ffffff;
  color: #1e293b;
  border: none;
  border-radius: 12px;
  padding: 0.85rem;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
}

.btn-google:hover {
  background: #f1f5f9;
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(255, 255, 255, 0.1);
}

.google-icon {
  width: 18px;
  height: 18px;
  color: #ea4335; /* Standard Google Red */
}

.register-footer {
  margin-top: 1.8rem;
  text-align: center;
  color: #94a3b8;
  font-size: 0.9rem;
}

.register-link {
  color: #14b8a6;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.register-link:hover {
  color: #2dd4bf;
  text-decoration: underline;
}

/* OTP Specifics */
.otp-input {
  letter-spacing: 0.5rem;
  font-size: 1.25rem;
  text-align: center;
  padding-left: 1rem;
}

/* Spinner animation */
.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Custom Toasts container */
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  z-index: 9999;
  max-width: 350px;
}

.toast {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 1rem 1.25rem;
  border-radius: 12px;
  color: #ffffff;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.35);
  font-size: 0.9rem;
  font-weight: 500;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.toast-success {
  background: rgba(16, 185, 129, 0.9);
  border-color: rgba(16, 185, 129, 0.2);
}

.toast-error {
  background: rgba(239, 68, 68, 0.9);
  border-color: rgba(239, 68, 68, 0.2);
}

.toast-info {
  background: rgba(59, 130, 246, 0.9);
  border-color: rgba(59, 130, 246, 0.2);
}

.toast-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

.toast-message {
  line-height: 1.4;
}

/* Toast Transitions */
.toast-fade-enter-active,
.toast-fade-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.toast-fade-enter-from {
  opacity: 0;
  transform: translateY(-20px) scale(0.9);
}

.toast-fade-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.9);
}
</style>
