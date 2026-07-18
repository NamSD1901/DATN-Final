<template>
  <div class="register-page-container">
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
            <p class="hero-desc">Create your account to start booking professional services for your beloved companions.</p>
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
            <div v-if="currentStep === 'register'" key="register">
              <div class="form-header">
                <h3>Đăng ký tài khoản</h3>
                <p>Cùng chăm sóc tốt nhất cho người bạn nhỏ</p>
              </div>

              <form @submit.prevent="handleRegister" class="auth-form">
                <!-- Full Name -->
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-person-fill"></i></span>
                  <input id="fullName" type="text" v-model="registerForm.fullName" class="input-field" placeholder="Họ và tên" required />
                  <label for="fullName" class="input-label">Họ và tên</label>
                </div>

                <!-- Email -->
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-envelope-fill"></i></span>
                  <input id="email" type="email" v-model="registerForm.email" class="input-field" placeholder="Email" required />
                  <label for="email" class="input-label">Địa chỉ Email</label>
                </div>

                <!-- Phone -->
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-telephone-fill"></i></span>
                  <input id="phoneNumber" type="tel" v-model="registerForm.phone" class="input-field" placeholder="Số điện thoại" required />
                  <label for="phoneNumber" class="input-label">Số điện thoại</label>
                </div>

                <!-- Password -->
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-lock-fill"></i></span>
                  <input id="password" type="password" v-model="registerForm.password" class="input-field" placeholder="Mật khẩu" required />
                  <label for="password" class="input-label">Mật khẩu</label>
                </div>

                <!-- Confirm Password -->
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-shield-lock-fill"></i></span>
                  <input id="confirmPassword" type="password" v-model="registerForm.confirmPassword" class="input-field" placeholder="Xác nhận" required />
                  <label for="confirmPassword" class="input-label">Xác nhận mật khẩu</label>
                </div>

                <button type="submit" :disabled="loading" class="btn-auth-submit mt-2">
                  <span v-if="!loading">Đăng ký thành viên <i class="bi bi-person-plus-fill ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>

                <p class="auth-switch-text">
                  Đã có tài khoản?
                  <router-link to="/login">Đăng nhập ngay <i class="bi bi-arrow-right"></i></router-link>
                </p>
              </form>
            </div>

            <div v-else-if="currentStep === 'otp'" key="otp">
              <div class="form-header">
                <div class="otp-icon-wrapper"><i class="bi bi-shield-check-fill"></i></div>
                <h3>Xác thực tài khoản</h3>
                <p>Mã xác thực đã gửi tới <strong class="text-warning">{{ targetEmail }}</strong></p>
              </div>
              <form @submit.prevent="handleVerifyOtp" class="auth-form">
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-123"></i></span>
                  <input id="otp" type="text" v-model="otpForm.otpCode" class="input-field otp-input" placeholder="• • • • • •" required maxlength="6" />
                  <label for="otp" class="input-label">Nhập mã OTP</label>
                </div>
                <button type="submit" :disabled="loading" class="btn-auth-submit">
                  <span v-if="!loading">Kích hoạt tài khoản <i class="bi bi-check-circle ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>
                <button type="button" @click="handleResendOtp" :disabled="resendCountdown > 0 || loading" class="btn-auth-secondary">
                  {{ resendCountdown > 0 ? `Gửi lại sau (${resendCountdown}s)` : 'Gửi lại mã OTP' }}
                </button>
                <button type="button" @click="goBackToRegister" class="btn-auth-link">← Quay lại đăng ký</button>
              </form>
            </div>

            <div v-else-if="currentStep === 'claim'" key="claim">
              <div class="form-header">
                <div class="otp-icon-wrapper" style="background: linear-gradient(135deg, #fbbf24, #d97706);"><i class="bi bi-person-bounding-box"></i></div>
                <h3>Xác minh Hồ sơ</h3>
                <p>Số điện thoại này đã từng khám tại phòng khám. Vui lòng xác nhận để đồng bộ hồ sơ cũ.</p>
              </div>
              <form @submit.prevent="handleClaimProfile" class="auth-form">
                <div class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-upc-scan"></i></span>
                  <input id="customerCode" type="text" v-model="claimForm.customerCode" class="input-field" placeholder="Mã khách hàng (VD: CUS...)" required />
                  <label for="customerCode" class="input-label">Mã khách hàng</label>
                </div>
                
                <div v-if="hasPets" class="input-group-custom">
                  <span class="input-icon"><i class="bi bi-suit-heart-fill"></i></span>
                  <input id="petName" type="text" v-model="claimForm.petName" class="input-field" placeholder="Tên một bé thú cưng" required />
                  <label for="petName" class="input-label">Tên thú cưng</label>
                </div>

                <button type="submit" :disabled="loading" class="btn-auth-submit">
                  <span v-if="!loading">Đồng bộ hồ sơ <i class="bi bi-arrow-repeat ms-1"></i></span>
                  <span v-else class="btn-spinner"></span>
                </button>
                <button type="button" @click="handleSkipClaim" :disabled="loading" class="btn-auth-secondary">
                  Tôi là khách mới / Bỏ qua
                </button>
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
const currentStep = ref<'register' | 'otp' | 'claim'>('register');
const loading = ref(false);
const targetEmail = ref('');
const hasPets = ref(false);
const tempToken = ref('');
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
const registerForm = reactive({
  email: '',
  password: '',
  confirmPassword: '',
  fullName: '',
  phone: ''
});

const otpForm = reactive({
  otpCode: ''
});

const claimForm = reactive({
  customerCode: '',
  petName: ''
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
      if (response.data.requiresClaiming) {
        hasPets.value = response.data.hasPets;
        tempToken.value = response.data.tempToken;
        currentStep.value = 'claim';
        showInfoToast(response.data.message || 'Vui lòng xác minh hồ sơ vãng lai của bạn.');
      } else {
        showSuccessToast('Kích hoạt tài khoản thành công! Đang chuyển đến trang Đăng nhập...');
        setTimeout(() => {
          router.push('/login');
        }, 1500);
      }
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

const handleClaimProfile = async () => {
  loading.value = true;
  try {
    const response = await api.post('/account/claim-profile', {
      email: targetEmail.value,
      tempToken: tempToken.value,
      customerCode: claimForm.customerCode,
      petName: hasPets.value ? claimForm.petName : null
    });
    if (response.data.success) {
      showSuccessToast(response.data.message || 'Đồng bộ hồ sơ thành công! Đang chuyển đến trang Đăng nhập...');
      setTimeout(() => {
        router.push('/login');
      }, 1500);
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Xác minh hồ sơ thất bại. Kiểm tra lại Mã KH và Tên thú cưng.');
  } finally {
    loading.value = false;
  }
};

const handleSkipClaim = async () => {
  if (!confirm('Bạn có chắc chắn muốn bỏ qua? Bạn sẽ được tạo 1 hồ sơ mới hoàn toàn và dữ liệu cũ sẽ không được liên kết.')) return;
  
  loading.value = true;
  try {
    const response = await api.post('/account/skip-claim', {
      email: targetEmail.value,
      tempToken: tempToken.value
    });
    if (response.data.success) {
      showSuccessToast(response.data.message || 'Tạo hồ sơ mới thành công! Đang chuyển đến trang Đăng nhập...');
      setTimeout(() => {
        router.push('/login');
      }, 1500);
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Có lỗi xảy ra.');
  } finally {
    loading.value = false;
  }
};

onUnmounted(() => {
  clearInterval(countdownInterval);
});
</script>

<style scoped>
.register-page-container {
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
  overflow: hidden;
}

/* ===== LEFT HERO PANEL ===== */
.hero-panel {
  flex: 1.1;
  background-size: cover;
  background-position: center;
  display: flex;
  align-items: flex-end;
  justify-content: flex-start;
  padding: 4rem;
  position: relative;
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
  font-size: 0.95rem;
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
