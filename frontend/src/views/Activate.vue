<template>
  <div class="activate-page-container">
    <div class="login-split-layout">
      <!-- ===== LEFT HERO PANEL ===== -->
      <div class="hero-panel" style="background-image: url('https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&q=80&w=1200');">
        <div class="hero-overlay"></div>
        <div class="hero-panel-inner">
          <router-link to="/" class="hero-brand-white">
            <i class="bi bi-heart-pulse-fill me-2"></i>
            MyPet<span>Clinic</span>
          </router-link>

          <div class="hero-content-bottom">
            <h2 class="hero-title">Bảo mật tài khoản của bạn.</h2>
            <p class="hero-desc">Vui lòng thiết lập mật khẩu mạnh để kích hoạt và bảo vệ tài khoản nhân viên của bạn tại hệ thống.</p>
          </div>
        </div>
      </div>

      <!-- ===== RIGHT FORM PANEL ===== -->
      <div class="form-panel">
        <div class="form-panel-inner w-100" style="max-width: 420px; margin: 0 auto;">
          <router-link to="/login" class="back-home-btn">
            <i class="bi bi-arrow-left me-1"></i> Trở về đăng nhập
          </router-link>

          <div class="form-header text-start mb-4">
            <div class="mb-3">
              <div class="brand-logo bg-warning text-dark rounded-circle d-inline-flex justify-content-center align-items-center shadow-sm" style="width: 56px; height: 56px;">
                <i class="bi bi-shield-lock-fill fs-3"></i>
              </div>
            </div>
            <h3 class="fw-bold text-dark mb-2">Thiết Lập Mật Khẩu</h3>
            <p class="text-muted small">Nhập mật khẩu mới để kích hoạt tài khoản nhân viên của bạn.</p>
          </div>

          <div v-if="loading || isCheckingToken" class="my-5 text-center">
            <div class="spinner-border text-warning" role="status"></div>
            <div class="mt-2 text-muted small fw-bold">{{ isCheckingToken ? 'Đang kiểm tra liên kết...' : 'Đang xử lý...' }}</div>
          </div>
          
          <div v-else-if="success" class="my-5 text-center">
            <i class="bi bi-check-circle-fill text-success" style="font-size: 4rem;"></i>
            <h4 class="text-success fw-bold mt-3">Kích hoạt thành công!</h4>
            <p class="text-muted small mt-2">Tài khoản của bạn đã sẵn sàng sử dụng.</p>
            <router-link to="/login" class="btn btn-warning fw-bold px-5 py-2 mt-3 rounded-pill shadow-sm">Đến Trang Đăng Nhập</router-link>
          </div>

          <div v-else-if="!isTokenValid" class="my-5 text-center">
            <i class="bi bi-x-circle-fill text-danger" style="font-size: 4rem;"></i>
            <h4 class="text-danger fw-bold mt-3">Lỗi Kích Hoạt</h4>
            <p class="text-muted mt-2">{{ errorMsg }}</p>
            <router-link to="/login" class="btn btn-outline-secondary fw-bold px-4 py-2 mt-3 rounded-pill shadow-sm">Về trang Đăng Nhập</router-link>
          </div>

          <form v-else @submit.prevent="handleActivate" class="text-start">
            <div v-if="errorMsg" class="alert alert-danger py-2 small mb-3">
              <i class="bi bi-exclamation-triangle-fill me-2"></i> {{ errorMsg }}
            </div>
            
            <div class="mb-3">
              <div class="input-group-custom">
                <span class="input-icon"><i class="bi bi-key-fill"></i></span>
                <input :type="showPassword ? 'text' : 'password'" v-model="password" id="newPassword" class="input-field pe-5" required placeholder="Mật khẩu mới" />
                <label for="newPassword" class="input-label">Mật khẩu mới</label>
                <span class="position-absolute end-0 top-50 translate-middle-y me-3 cursor-pointer text-muted" style="z-index: 10;" @click="showPassword = !showPassword">
                  <i :class="showPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </span>
              </div>
              
              <!-- Password Strength Meter -->
              <div class="password-strength-container mt-2" v-if="password">
                <div class="strength-bar-wrapper">
                  <div class="strength-bar" :style="{ width: passwordStrength.width, backgroundColor: passwordStrength.color }"></div>
                </div>
                <div class="strength-text" :style="{ color: passwordStrength.color }">
                  Độ mạnh: {{ passwordStrength.text }}
                </div>
                <ul class="password-hints mt-1" v-if="passwordErrors.length > 0">
                  <li v-for="(error, index) in passwordErrors" :key="index" class="text-danger small">
                    <i class="bi bi-x-circle"></i> {{ error }}
                  </li>
                </ul>
                <ul class="password-hints mt-1" v-else>
                  <li class="text-success small"><i class="bi bi-check-circle"></i> Mật khẩu đạt yêu cầu</li>
                </ul>
              </div>
            </div>
            
            <div class="mb-4">
              <div class="input-group-custom">
                <span class="input-icon"><i class="bi bi-shield-lock-fill"></i></span>
                <input :type="showConfirmPassword ? 'text' : 'password'" v-model="confirmPassword" id="confirmPassword" class="input-field pe-5" required placeholder="Xác nhận mật khẩu" />
                <label for="confirmPassword" class="input-label">Xác nhận mật khẩu</label>
                <span class="position-absolute end-0 top-50 translate-middle-y me-3 cursor-pointer text-muted" style="z-index: 10;" @click="showConfirmPassword = !showConfirmPassword">
                  <i :class="showConfirmPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
                </span>
              </div>
            </div>
            
            <button type="submit" class="btn-auth-submit">
              Kích Hoạt Tài Khoản <i class="bi bi-arrow-right"></i>
            </button>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import api from '../services/api';

const route = useRoute();
const token = ref('');
const password = ref('');
const confirmPassword = ref('');
const showPassword = ref(false);
const showConfirmPassword = ref(false);
const loading = ref(false);
const success = ref(false);
const errorMsg = ref('');
const isCheckingToken = ref(true);
const isTokenValid = ref(false);

onMounted(async () => {
  const queryToken = route.query.token as string;
  if (!queryToken) {
    errorMsg.value = 'Link kích hoạt không hợp lệ hoặc bị thiếu Token.';
    isCheckingToken.value = false;
  } else {
    token.value = queryToken;
    try {
      await api.get(`/account/check-invitation?token=${queryToken}`);
      isTokenValid.value = true;
    } catch (err: any) {
      errorMsg.value = err.response?.data?.message || 'Link kích hoạt không hợp lệ hoặc đã hết hạn.';
    } finally {
      isCheckingToken.value = false;
    }
  }
});

const passwordStrength = computed(() => {
  const pwd = password.value;
  let score = 0;
  if (!pwd) return { score: 0, text: '', color: '#e2e8f0', width: '0%' };

  if (pwd.length >= 8) score += 1;
  if (/[A-Z]/.test(pwd)) score += 1;
  if (/[a-z]/.test(pwd)) score += 1;
  if (/[0-9]/.test(pwd)) score += 1;
  if (/[^A-Za-z0-9]/.test(pwd)) score += 1;

  if (score <= 2) return { score, text: 'Yếu', color: '#ef4444', width: '33%' };
  if (score <= 4) return { score, text: 'Trung bình', color: '#f59e0b', width: '66%' };
  return { score, text: 'Mạnh', color: '#10b981', width: '100%' };
});

const passwordErrors = computed(() => {
  const pwd = password.value;
  const errors = [];
  if (pwd && pwd.length < 8) errors.push('Tối thiểu 8 ký tự');
  if (pwd && !/[A-Z]/.test(pwd)) errors.push('Cần ít nhất 1 chữ hoa');
  if (pwd && !/[a-z]/.test(pwd)) errors.push('Cần ít nhất 1 chữ thường');
  if (pwd && !/[0-9]/.test(pwd)) errors.push('Cần ít nhất 1 chữ số');
  if (pwd && !/[^A-Za-z0-9]/.test(pwd)) errors.push('Cần ít nhất 1 ký tự đặc biệt (@, $, #, ...)');
  return errors;
});

const handleActivate = async () => {
  if (passwordErrors.value.length > 0) {
    errorMsg.value = 'Mật khẩu chưa đủ mạnh. Vui lòng kiểm tra lại các yêu cầu.';
    return;
  }
  if (password.value !== confirmPassword.value) {
    errorMsg.value = 'Mật khẩu xác nhận không khớp.';
    return;
  }
  if (!token.value) {
    errorMsg.value = 'Token không tồn tại.';
    return;
  }

  loading.value = true;
  errorMsg.value = '';

  try {
    await api.post('/account/activate', {
      token: token.value,
      password: password.value,
      confirmPassword: confirmPassword.value
    });
    success.value = true;
  } catch (err: any) {
    if (err.response?.data?.errors) {
      const firstErrorKey = Object.keys(err.response.data.errors)[0];
      errorMsg.value = err.response.data.errors[firstErrorKey][0];
    } else {
      errorMsg.value = err.response?.data?.message || 'Có lỗi xảy ra trong quá trình kích hoạt.';
    }
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.activate-page-container {
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
  top: 0; left: 0; right: 0; bottom: 0;
  background: linear-gradient(135deg, rgba(15, 23, 42, 0.4) 0%, rgba(2, 6, 23, 0.7) 100%);
  z-index: 1;
}

@media (max-width: 991px) {
  .hero-panel { display: none; }
}

.hero-panel-inner {
  width: 100%; height: 100%;
  display: flex; flex-direction: column;
  justify-content: space-between; align-items: flex-start;
  position: relative; z-index: 5;
}

.hero-brand-white {
  font-size: 2.2rem; font-weight: 800; color: white; text-decoration: none; display: flex; align-items: center;
}
.hero-brand-white span { color: #fbbf24; }

.hero-content-bottom { max-width: 480px; }
.hero-title { font-size: 2.8rem; font-weight: 800; color: white; margin-bottom: 1rem; line-height: 1.2; }
.hero-desc { font-size: 1.1rem; color: #e2e8f0; margin-bottom: 0; line-height: 1.6; }

/* ===== RIGHT FORM PANEL ===== */
.form-panel {
  flex: 0.9;
  background: white;
  display: flex; align-items: center; justify-content: center;
  padding: 3rem;
  box-shadow: -10px 0 30px rgba(0, 0, 0, 0.02);
}

@media (max-width: 991px) {
  .form-panel { flex: 1; padding: 1.5rem; }
}

.back-home-btn {
  display: inline-flex; align-items: center; gap: 6px;
  font-size: 0.9rem; font-weight: 600; color: #64748b; text-decoration: none;
  margin-bottom: 2.5rem; transition: all 0.2s ease;
}
.back-home-btn:hover { color: #d97706; transform: translateX(-4px); }

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
  z-index: 2;
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
  z-index: 2;
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

.cursor-pointer { cursor: pointer; transition: all 0.2s ease; }
.cursor-pointer:hover { color: #f59e0b !important; }

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

.strength-bar-wrapper {
  height: 4px; background-color: #e2e8f0; border-radius: 2px; overflow: hidden; margin-bottom: 0.25rem;
}
.strength-bar { height: 100%; transition: all 0.3s ease; }
.strength-text { font-size: 0.75rem; font-weight: 600; margin-bottom: 0.5rem; }
.password-hints { list-style: none; padding: 0; margin: 0; }
</style>
