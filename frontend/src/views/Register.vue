<template>
  <div class="register-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <div class="register-card">
      <!-- Clinic Branding -->
      <div class="brand">
        <div class="brand-icon">
          <PawPrint class="icon-paw" />
        </div>
        <h1 class="brand-name">MyPetClinic</h1>
        <p class="brand-tagline">Đăng ký tài khoản để bắt đầu chăm sóc pet cưng</p>
      </div>

      <!-- Toast Notifications -->
      <TransitionGroup name="toast-fade" tag="div" class="toast-container">
        <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
          <component :is="toast.icon" class="toast-icon" />
          <span class="toast-message">{{ toast.message }}</span>
        </div>
      </TransitionGroup>

      <!-- Registration Form View -->
      <div v-if="currentStep === 'register'" class="form-container">
        <h2 class="form-title">Đăng Ký Tài Khoản</h2>
        <p class="form-subtitle">Điền đầy đủ thông tin bên dưới</p>

        <form @submit.prevent="handleRegister" class="form">
          <div class="input-group">
            <label for="fullName">Họ và Tên</label>
            <div class="input-wrapper">
              <User class="input-icon" />
              <input 
                id="fullName" 
                type="text" 
                v-model="registerForm.fullName" 
                placeholder="Nguyễn Văn A" 
                required 
                class="form-input"
              />
            </div>
          </div>

          <div class="input-row">
            <div class="input-group">
              <label for="email">Email</label>
              <div class="input-wrapper">
                <Mail class="input-icon" />
                <input 
                  id="email" 
                  type="email" 
                  v-model="registerForm.email" 
                  placeholder="name@example.com" 
                  required 
                  class="form-input"
                />
              </div>
            </div>

            <div class="input-group">
              <label for="phoneNumber">Số điện thoại</label>
              <div class="input-wrapper">
                <Phone class="input-icon" />
                <input 
                  id="phoneNumber" 
                  type="tel" 
                  v-model="registerForm.phoneNumber" 
                  placeholder="0912345678" 
                  required 
                  class="form-input"
                />
              </div>
            </div>
          </div>

          <div class="input-row">
            <div class="input-group">
              <label for="password">Mật khẩu</label>
              <div class="input-wrapper">
                <Lock class="input-icon" />
                <input 
                  id="password" 
                  type="password" 
                  v-model="registerForm.password" 
                  placeholder="••••••••" 
                  required 
                  class="form-input"
                />
              </div>
            </div>

            <div class="input-group">
              <label for="confirmPassword">Nhập lại mật khẩu</label>
              <div class="input-wrapper">
                <Lock class="input-icon" />
                <input 
                  id="confirmPassword" 
                  type="password" 
                  v-model="registerForm.confirmPassword" 
                  placeholder="••••••••" 
                  required 
                  class="form-input"
                />
              </div>
            </div>
          </div>

          <button type="submit" :disabled="loading" class="btn-submit">
            <span v-if="!loading">Đăng Ký</span>
            <div v-else class="spinner"></div>
          </button>
        </form>

        <div class="login-footer">
          Đã có tài khoản? <router-link to="/login" class="login-link">Đăng nhập ngay</router-link>
        </div>
      </div>

      <!-- OTP Verification View -->
      <div v-else-if="currentStep === 'otp'" class="form-container">
        <h2 class="form-title">Xác thực tài khoản</h2>
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
            <span v-if="!loading">Kích Hoạt Tài Khoản</span>
            <div v-else class="spinner"></div>
          </button>
          
          <button type="button" @click="handleResendOtp" :disabled="resendCountdown > 0 || loading" class="btn-secondary">
            {{ resendCountdown > 0 ? `Gửi lại sau (${resendCountdown}s)` : 'Gửi lại mã OTP' }}
          </button>
          
          <button type="button" @click="goBackToRegister" class="btn-back">
            Quay lại Đăng ký
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
  User, 
  Phone,
  ShieldCheck, 
  AlertCircle, 
  CheckCircle2, 
  Info 
} from '@lucide/vue';

const router = useRouter();
const currentStep = ref<'register' | 'otp'>('register');
const loading = ref(false);
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

// Forms State
const registerForm = reactive({
  email: '',
  password: '',
  confirmPassword: '',
  fullName: '',
  phoneNumber: ''
});

const otpForm = reactive({
  otpCode: ''
});

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

const handleRegister = async () => {
  if (registerForm.password !== registerForm.confirmPassword) {
    showErrorToast('Mật khẩu nhập lại không khớp.');
    return;
  }
  loading.value = true;
  try {
    const response = await api.post('/account/register', registerForm);
    if (response.data.success) {
      showSuccessToast(response.data.message || 'Đăng ký thành công! Hãy nhập OTP để kích hoạt.');
      targetEmail.value = registerForm.email;
      currentStep.value = 'otp';
      startCountdown(60);
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Có lỗi xảy ra trong quá trình đăng ký.');
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
      showSuccessToast('Kích hoạt tài khoản thành công! Đang chuyển đến trang Đăng nhập...');
      setTimeout(() => {
        router.push('/login');
      }, 1500);
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Mã OTP không hợp lệ hoặc đã hết hạn.');
  } finally {
    loading.value = false;
  }
};

const handleResendOtp = async () => {
  loading.value = true;
  try {
    const response = await api.post('/account/resend-otp', {
      email: targetEmail.value,
      type: 'Verification'
    });
    if (response.data.success) {
      showSuccessToast('Mã OTP mới đã được gửi lại vào email của bạn.');
      startCountdown(60);
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Không thể gửi lại mã OTP.');
  } finally {
    loading.value = false;
  }
};

const goBackToRegister = () => {
  currentStep.value = 'register';
};

onUnmounted(() => {
  clearInterval(countdownInterval);
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.register-wrapper {
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

.register-card {
  position: relative;
  z-index: 1;
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 24px;
  padding: 2.5rem;
  width: 100%;
  max-width: 600px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
}

.brand {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 1.8rem;
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
  margin-bottom: 1.5rem;
}

.highlight-email {
  color: #14b8a6;
  font-weight: 500;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.input-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

@media (max-width: 500px) {
  .input-row {
    grid-template-columns: 1fr;
  }
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.input-group label {
  color: #cbd5e1;
  font-size: 0.85rem;
  font-weight: 500;
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
  padding: 0.8rem 1rem 0.8rem 2.8rem;
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
  margin-top: 0.8rem;
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

.login-footer {
  margin-top: 1.5rem;
  text-align: center;
  color: #94a3b8;
  font-size: 0.9rem;
}

.login-link {
  color: #14b8a6;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.2s;
}

.login-link:hover {
  color: #2dd4bf;
  text-decoration: underline;
}

.otp-input {
  letter-spacing: 0.5rem;
  font-size: 1.25rem;
  text-align: center;
  padding-left: 1rem;
}

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
