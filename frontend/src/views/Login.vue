<template>
  <div class="login-page-container">
    <!-- Toast Notifications -->
    <TransitionGroup name="toast-fade" tag="div" class="toast-container">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
        <component :is="toast.icon" class="toast-icon" />
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </TransitionGroup>

    <div class="login-split-layout">
      <!-- ===== LEFT HERO PANEL ===== -->
      <div class="hero-panel" style="background-image: url('https://images.unsplash.com/photo-1576201836106-db1758fd1c97?auto=format&fit=crop&q=80&w=1200');">
        <div class="hero-overlay"></div>
        <div class="hero-panel-inner">
          <router-link to="/" class="hero-brand-white">
            <i class="bi bi-heart-pulse-fill me-2"></i>
            MyPet<span>Clinic</span>
          </router-link>

          <div class="hero-content-bottom">
            <h2 class="hero-title">Discover your pet's best care.</h2>
            <p class="hero-desc">Log in to manage appointments, access medical records, and connect with veterinarians instantly.</p>
          </div>
        </div>
      </div>

      <!-- ===== RIGHT FORM PANEL ===== -->
      <div class="form-panel">
        <div class="form-panel-inner">
          <router-link to="/" class="back-home-btn">
            <i class="bi bi-arrow-left me-1"></i> Trang chủ
          </router-link>

          <Transition name="step-fade" mode="out-in">
            <div v-if="currentStep === 'login'" key="login">
              <div class="form-header">
                <h3>Đăng nhập tài khoản</h3>
                <p>Chào mừng bạn quay trở lại với MyPet Clinic</p>
              </div>

              <div class="demo-box mb-4">
                <button type="button" class="demo-toggle-btn" @click="showDemoBox = !showDemoBox">
                  <span><i class="bi bi-info-circle me-1 text-warning"></i> Xem tài khoản Demo</span>
                  <i class="bi" :class="showDemoBox ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                </button>
                <div class="demo-content-wrapper" :class="{ 'is-open': showDemoBox }">
                  <ul class="demo-list">
                    <li @click="autofillDemo('admindemo@gmail.com')" class="demo-account-item" title="Click để tự động điền">
                      <span><strong>Admin:</strong> admindemo@gmail.com</span>
                      <i class="bi bi-file-earmark-arrow-down-fill autofill-icon"></i>
                    </li>
                    <li @click="autofillDemo('letandemo@gmail.com')" class="demo-account-item" title="Click để tự động điền">
                      <span><strong>Lễ tân:</strong> letandemo@gmail.com</span>
                      <i class="bi bi-file-earmark-arrow-down-fill autofill-icon"></i>
                    </li>
                    <li @click="autofillDemo('khachhangdemo@gmail.com')" class="demo-account-item" title="Click để tự động điền">
                      <span><strong>Khách hàng:</strong> khachhangdemo@gmail.com</span>
                      <i class="bi bi-file-earmark-arrow-down-fill autofill-icon"></i>
                    </li>
                    <li @click="autofillDemo('bacsi_test@gmail.com')" class="demo-account-item" title="Click để tự động điền">
                      <span><strong>Bác sĩ:</strong> bacsi_test@gmail.com</span>
                      <i class="bi bi-file-earmark-arrow-down-fill autofill-icon"></i>
                    </li>
                    <li class="password-info">
                      Mật khẩu chung: <strong>123456</strong>
                    </li>
                  </ul>
                </div>
              </div>

              <form @submit.prevent="handleLogin" class="auth-form">
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-envelope-fill"></i></span>
                  <input id="email" type="email" v-model="loginForm.email" class="input-field" placeholder="Email của bạn" required />
                  <label for="email" class="input-label">Địa chỉ Email</label>
                </div>

                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-lock-fill"></i></span>
                  <input id="password" :type="showPassword ? 'text' : 'password'" v-model="loginForm.password" class="input-field" placeholder="Mật khẩu" required />
                  <label for="password" class="input-label">Mật khẩu</label>
                  <button type="button" @click="showPassword = !showPassword" class="toggle-pw-btn">
                    <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
                  </button>
                </div>

                <div class="d-flex align-items-center justify-content-between mb-4 small">
                  <label class="remember-check">
                    <input type="checkbox" v-model="loginForm.rememberMe" />
                    <span>Ghi nhớ đăng nhập</span>
                  </label>
                  <a href="#" @click.prevent="goToForgotPassword" class="forgot-link">Quên mật khẩu?</a>
                </div>

                <button type="submit" :disabled="loading" class="btn-auth-submit">
                  <span v-if="!loading">Đăng nhập <i class="bi bi-box-arrow-in-right ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>

                <div class="auth-divider"><span>hoặc</span></div>

                <button type="button" @click="handleGoogleLogin" class="btn-google">
                  <img src="https://upload.wikimedia.org/wikipedia/commons/c/c1/Google_%22G%22_logo.svg" width="18" height="18" alt="G" />
                  Tiếp tục với Google
                </button>

                <p class="auth-switch-text">
                  Chưa có tài khoản?
                  <router-link to="/register">Đăng ký ngay <i class="bi bi-arrow-right"></i></router-link>
                </p>
              </form>
            </div>

            <div v-else-if="currentStep === 'otp'" key="otp">
              <div class="form-header">
                <div class="otp-icon-wrapper"><i class="bi bi-shield-check-fill"></i></div>
                <h3>Xác thực OTP</h3>
                <p>Mã xác thực đã gửi tới <strong class="text-warning">{{ targetEmail }}</strong></p>
              </div>
              <form @submit.prevent="handleVerifyOtp" class="auth-form">
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-123"></i></span>
                  <input id="otp" type="text" v-model="otpForm.otpCode" class="input-field otp-input" placeholder="• • • • • •" required maxlength="6" />
                  <label for="otp" class="input-label">Nhập mã OTP</label>
                </div>
                <button type="submit" :disabled="loading" class="btn-auth-submit">
                  <span v-if="!loading">Xác nhận <i class="bi bi-check-circle ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>
                <button type="button" @click="handleResendOtp" :disabled="resendCountdown > 0 || loading" class="btn-auth-secondary">
                  {{ resendCountdown > 0 ? `Gửi lại sau (${resendCountdown}s)` : 'Gửi lại mã OTP' }}
                </button>
                <button type="button" @click="goBackToLogin" class="btn-auth-link">← Quay lại đăng nhập</button>
              </form>
            </div>

            <div v-else-if="currentStep === 'forgot'" key="forgot">
              <div class="form-header">
                <div class="otp-icon-wrapper" style="background: linear-gradient(135deg,#fbbf24,#f59e0b)"><i class="bi bi-envelope-open-fill"></i></div>
                <h3>Quên mật khẩu</h3>
                <p>Nhập email để nhận mã OTP khôi phục</p>
              </div>
              <form @submit.prevent="handleForgotPassword" class="auth-form">
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-envelope-fill"></i></span>
                  <input id="forgot-email" type="email" v-model="forgotForm.email" class="input-field" placeholder="Email khôi phục" required />
                  <label for="forgot-email" class="input-label">Email của bạn</label>
                </div>
                <button type="submit" :disabled="loading" class="btn-auth-submit">
                  <span v-if="!loading">Gửi OTP <i class="bi bi-send ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>
                <button type="button" @click="goBackToLogin" class="btn-auth-link">← Quay lại đăng nhập</button>
              </form>
            </div>

            <div v-else-if="currentStep === 'reset'" key="reset">
              <div class="form-header">
                <div class="otp-icon-wrapper" style="background: linear-gradient(135deg,#10b981,#059669)"><i class="bi bi-key-fill"></i></div>
                <h3>Đặt lại mật khẩu</h3>
                <p>Tạo mật khẩu mới cho tài khoản của bạn</p>
              </div>
              <form @submit.prevent="handleResetPassword" class="auth-form">
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-123"></i></span>
                  <input id="reset-otp" type="text" v-model="resetForm.otpCode" class="input-field" placeholder="Mã OTP" required />
                  <label for="reset-otp" class="input-label">Mã OTP</label>
                </div>
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-lock-fill"></i></span>
                  <input id="new-password" type="password" v-model="resetForm.newPassword" class="input-field" placeholder="Mật khẩu mới" required />
                  <label for="new-password" class="input-label">Mật khẩu mới</label>
                </div>
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-shield-lock-fill"></i></span>
                  <input id="confirm-password" type="password" v-model="resetForm.confirmPassword" class="input-field" placeholder="Xác nhận mật khẩu" required />
                  <label for="confirm-password" class="input-label">Xác nhận mật khẩu</label>
                </div>
                <button type="submit" :disabled="loading" class="btn-auth-submit">
                  <span v-if="!loading">Đổi mật khẩu <i class="bi bi-check-circle ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>
                <button type="button" @click="goBackToLogin" class="btn-auth-link">← Quay lại đăng nhập</button>
              </form>
            </div>
          </Transition>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { 
  Info, 
  CheckCircle2, 
  AlertCircle 
} from '@lucide/vue';

const router = useRouter();

const currentStep = ref<'login' | 'otp' | 'forgot' | 'reset'>('login');
const showDemoBox = ref(false);
const autofillDemo = (email: string) => {
  loginForm.email = email;
  loginForm.password = '123456';
  showSuccessToast(`Đã tự động điền: ${email}`);
};
const loading = ref(false);
const showPassword = ref(false);
const targetEmail = ref('');
const resendCountdown = ref(0);
let countdownInterval: any = null;

// Toast Notifications matching home styles
interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
  icon: any;
}
const toasts = ref<Toast[]>([]);
let toastId = 0;

const showToast = (message: string, type: 'success' | 'error' | 'info' = 'info') => {
  // Loại bỏ thông báo cũ nếu trùng nội dung (tránh spam khi click liên tục)
  toasts.value = toasts.value.filter(t => t.message !== message);
  
  // Giới hạn tối đa 3 thông báo hiển thị cùng lúc
  if (toasts.value.length >= 3) {
    toasts.value.shift();
  }

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

// Form state
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
      // Tự động sửa lỗi CustomerId cho tài khoản cũ bị thiếu
      try { await api.post('/account/ensure-profile'); } catch { /* silent - chỉ customer mới cần */ }
      setTimeout(() => {
        router.push('/');
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
      type: 'Verification'
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
.login-page-container {
  width: 100%;
  min-height: 100vh;
  background-color: #fcfbf7;
  display: flex;
  align-items: center;
  justify-content: center;
}

.login-split-layout {
  display: flex;
  width: 100%;
  min-height: 100vh;
}

/* ===== LEFT HERO PANEL ===== */
.hero-panel {
  flex: 1.1;
  height: 100vh;
  position: sticky;
  top: 0;
  align-self: flex-start;
  background-size: cover;
  background-position: center;
  display: flex;
  align-items: flex-end;
  justify-content: flex-start;
  padding: 4rem;
  overflow: hidden;
}

.hero-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(15, 23, 42, 0.65) 0%, rgba(2, 6, 23, 0.85) 100%);
  z-index: 1;
}

@media (max-width: 991px) {
  .hero-panel {
    display: none; /* Ẩn ở mobile để tập trung vào form */
  }
}

.hero-panel-inner {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  align-items: flex-start;
  position: relative;
  z-index: 5;
}

.hero-brand-white {
  font-size: 2.2rem;
  font-weight: 800;
  color: white;
  text-decoration: none;
  display: flex;
  align-items: center;
}

.hero-brand-white i {
  color: #fbbf24;
}

.hero-brand-white span {
  color: #fbbf24;
}

.hero-content-bottom {
  max-width: 480px;
}

.hero-title {
  font-size: 2.8rem;
  font-weight: 800;
  color: white;
  margin-bottom: 1rem;
  line-height: 1.2;
}

.hero-desc {
  font-size: 1.1rem;
  color: #e2e8f0;
  margin-bottom: 0;
  line-height: 1.6;
}


/* ===== RIGHT FORM PANEL ===== */
.form-panel {
  flex: 0.9;
  height: 100%;
  overflow-y: auto;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 3rem;
  box-shadow: -10px 0 30px rgba(0, 0, 0, 0.02);
}

@media (max-width: 991px) {
  .form-panel {
    flex: 1;
    padding: 1.5rem;
  }
}

.form-panel-inner {
  max-width: 420px;
  width: 100%;
}

.back-home-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.9rem;
  font-weight: 600;
  color: #64748b;
  text-decoration: none;
  margin-bottom: 2.5rem;
  transition: all 0.2s ease;
}

.back-home-btn:hover {
  color: #d97706;
  transform: translateX(-4px);
}

.form-header {
  margin-bottom: 2rem;
}

.form-header h3 {
  font-size: 1.85rem;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 0.4rem;
}

.form-header p {
  color: #64748b;
  font-size: 0.95rem;
}

/* COLLAPSIBLE DEMO BOX REDESIGN */
.demo-box {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.3s ease;
}

.demo-toggle-btn {
  width: 100%;
  padding: 0.75rem 1rem;
  background: transparent;
  border: none;
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 700;
  color: #475569;
  cursor: pointer;
  outline: none;
  font-size: 0.9rem;
  transition: background-color 0.2s;
}

.demo-toggle-btn:hover {
  background-color: #f1f5f9;
}

.demo-content-wrapper {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.demo-content-wrapper.is-open {
  max-height: 300px;
  border-top: 1px dashed #e2e8f0;
}

.demo-list {
  list-style: none;
  padding: 0.75rem 1rem;
  margin: 0;
  font-size: 0.82rem;
  color: #64748b;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.demo-account-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.4rem 0.6rem;
  border-radius: 8px;
  cursor: pointer;
  background-color: #ffffff;
  border: 1px solid #e2e8f0;
  transition: all 0.2s ease;
}

.demo-account-item:hover {
  background-color: #fffbeb;
  border-color: #f59e0b;
  color: #d97706;
  transform: translateY(-1px);
}

.autofill-icon {
  color: #d97706;
  font-size: 1rem;
  opacity: 0.5;
  transition: opacity 0.2s;
}

.demo-account-item:hover .autofill-icon {
  opacity: 1;
}

.password-info {
  font-size: 0.82rem;
  border-top: 1px dashed #e2e8f0;
  padding-top: 0.5rem;
  margin-top: 0.25rem;
  text-align: center;
}

/* PREMIUM CUSTOM INPUT */
.input-group-custom {
  position: relative;
  margin-bottom: 1.25rem;
}

.input-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 1.1rem;
  pointer-events: none;
}

.input-field {
  width: 100%;
  padding: 1rem 1rem 1rem 2.8rem;
  font-size: 1rem; /* Prevent auto-zoom on iOS */
  color: #1e293b;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  background: #ffffff;
  outline: none;
  transition: all 0.2s ease;
}

.input-field::placeholder {
  color: transparent;
}

.input-label {
  position: absolute;
  left: 2.8rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 0.95rem;
  pointer-events: none;
  transition: all 0.2s ease;
}

/* Floating Label Logic */
.input-field:focus ~ .input-label,
.input-field:not(:placeholder-shown) ~ .input-label {
  top: 0.25rem;
  font-size: 0.75rem;
  color: #d97706;
  transform: translateY(0);
}

.input-field:focus {
  padding-top: 1.35rem;
  padding-bottom: 0.65rem;
  border-color: #f59e0b;
  box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.08);
}

.input-field:not(:placeholder-shown) {
  padding-top: 1.35rem;
  padding-bottom: 0.65rem;
}

.toggle-pw-btn {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  border: none;
  background: transparent;
  color: #94a3b8;
  cursor: pointer;
}

.toggle-pw-btn:hover {
  color: #64748b;
}

.remember-check {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  color: #64748b;
}

.remember-check input {
  width: 16px;
  height: 16px;
  accent-color: #d97706;
}

.forgot-link {
  color: #d97706;
  text-decoration: none;
  font-weight: 600;
}

.forgot-link:hover {
  text-decoration: underline;
}

/* BUTTONS */
.btn-auth-submit {
  width: 100%;
  padding: 0.95rem;
  background: linear-gradient(135deg, #fbbf24 0%, #d97706 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 8px 16px rgba(217, 119, 6, 0.15);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: all 0.2s ease;
}

.btn-auth-submit:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 20px rgba(217, 119, 6, 0.25);
}

.btn-auth-submit:active {
  transform: translateY(0);
}

.btn-auth-secondary {
  width: 100%;
  padding: 0.85rem;
  background: #f1f5f9;
  color: #475569;
  border: none;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  margin-top: 0.75rem;
}

.btn-auth-secondary:hover {
  background: #e2e8f0;
  color: #1e293b;
}

.btn-auth-link {
  width: 100%;
  background: transparent;
  border: none;
  color: #64748b;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  margin-top: 1rem;
  transition: all 0.2s ease;
}

.btn-auth-link:hover {
  color: #1e293b;
}

.auth-divider {
  display: flex;
  align-items: center;
  text-align: center;
  color: #cbd5e1;
  font-size: 0.85rem;
  margin: 1.5rem 0;
}

.auth-divider::before,
.auth-divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #f1f5f9;
}

.auth-divider span {
  padding: 0 10px;
  color: #94a3b8;
}

.btn-google {
  width: 100%;
  padding: 0.85rem;
  background: white;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 600;
  color: #475569;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  transition: all 0.2s ease;
}

.btn-google:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.02);
}

.auth-switch-text {
  text-align: center;
  margin-top: 2rem;
  font-size: 0.9rem;
  color: #64748b;
}

.auth-switch-text a {
  color: #d97706;
  text-decoration: none;
  font-weight: 700;
  margin-left: 4px;
}

.auth-switch-text a:hover {
  text-decoration: underline;
}

/* OTP Step Header Icon */
.otp-icon-wrapper {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  background: linear-gradient(135deg, #60a5fa, #2563eb);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
  margin-bottom: 1.25rem;
  box-shadow: 0 8px 16px rgba(37, 99, 235, 0.15);
}

.otp-input {
  letter-spacing: 0.4rem;
  font-size: 1.3rem;
  text-align: center;
  padding-left: 1rem;
}

.btn-spinner {
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

/* Transition effects */
.step-fade-enter-active,
.step-fade-leave-active {
  transition: all 0.25s ease;
}

.step-fade-enter-from {
  opacity: 0;
  transform: translateX(10px);
}

.step-fade-leave-to {
  opacity: 0;
  transform: translateX(-10px);
}

/* Toast Notifications */
.toast-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 1200;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.toast {
  background: white;
  border-radius: 12px;
  padding: 0.85rem 1.25rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
  display: flex;
  align-items: center;
  gap: 10px;
  border-left: 4px solid #cbd5e1;
  min-width: 280px;
}

.toast-success { border-left-color: #10b981; }
.toast-error { border-left-color: #ef4444; }
.toast-info { border-left-color: #f59e0b; }

.toast-icon {
  width: 18px;
  height: 18px;
}
.toast-success .toast-icon { color: #10b981; }
.toast-error .toast-icon { color: #ef4444; }
.toast-info .toast-icon { color: #f59e0b; }

.toast-message {
  font-size: 0.88rem;
  font-weight: 600;
  color: #1e293b;
}

.toast-fade-enter-active,
.toast-fade-leave-active {
  transition: all 0.3s ease;
}
.toast-fade-enter-from {
  opacity: 0;
  transform: translateY(-20px);
}
.toast-fade-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
</style>
