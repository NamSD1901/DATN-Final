<template>
  <div class="activate-container min-vh-100 d-flex justify-content-center align-items-center">
    <div class="activate-card p-5 rounded-4 shadow-lg text-center">
      <div class="mb-4">
        <div class="brand-logo bg-warning text-dark rounded-circle d-inline-flex justify-content-center align-items-center shadow-sm" style="width: 70px; height: 70px;">
          <i class="bi bi-shield-lock-fill fs-1"></i>
        </div>
      </div>
      
      <h3 class="fw-bold mb-2">Thiết Lập Mật Khẩu</h3>
      <p class="text-muted small mb-4">Nhập mật khẩu mới để kích hoạt tài khoản nhân viên của bạn.</p>
      
      <div v-if="loading" class="my-5">
        <div class="spinner-border text-warning" role="status"></div>
        <div class="mt-2 text-muted small fw-bold">Đang xử lý...</div>
      </div>
      
      <div v-else-if="success" class="my-5">
        <i class="bi bi-check-circle-fill text-success" style="font-size: 4rem;"></i>
        <h4 class="text-success fw-bold mt-3">Kích hoạt thành công!</h4>
        <p class="text-muted small mt-2">Tài khoản của bạn đã sẵn sàng sử dụng.</p>
        <router-link to="/login" class="btn btn-warning fw-bold px-5 py-2 mt-3 rounded-pill shadow-sm">Đến Trang Đăng Nhập</router-link>
      </div>

      <form v-else @submit.prevent="handleActivate" class="text-start">
        <div v-if="errorMsg" class="alert alert-danger py-2 small mb-3">
          <i class="bi bi-exclamation-triangle-fill me-2"></i> {{ errorMsg }}
        </div>
        
        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Mật khẩu mới</label>
          <div class="input-group">
            <span class="input-group-text bg-light border-end-0"><i class="bi bi-key"></i></span>
            <input type="password" v-model="password" class="form-control border-start-0 bg-light" required placeholder="Nhập mật khẩu an toàn..." minlength="8" />
          </div>
          <div class="form-text text-muted mt-1" style="font-size: 0.75rem;">
            Tối thiểu 8 ký tự, phải có chữ hoa, chữ thường, số và ký tự đặc biệt (@$!%*?&).
          </div>
        </div>
        
        <div class="mb-4">
          <label class="form-label small fw-bold text-muted mb-1">Nhập lại mật khẩu</label>
          <div class="input-group">
            <span class="input-group-text bg-light border-end-0"><i class="bi bi-key-fill"></i></span>
            <input type="password" v-model="confirmPassword" class="form-control border-start-0 bg-light" required placeholder="Xác nhận mật khẩu" />
          </div>
        </div>
        
        <button type="submit" class="btn btn-warning w-100 fw-bold py-2 rounded-pill shadow-sm">Kích Hoạt Tài Khoản</button>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import api from '../services/api';

const route = useRoute();
const token = ref('');
const password = ref('');
const confirmPassword = ref('');
const loading = ref(false);
const success = ref(false);
const errorMsg = ref('');

onMounted(() => {
  const queryToken = route.query.token as string;
  if (!queryToken) {
    errorMsg.value = 'Link kích hoạt không hợp lệ hoặc bị thiếu Token.';
  } else {
    token.value = queryToken;
  }
});

const handleActivate = async () => {
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
.activate-container {
  background: linear-gradient(135deg, #fdfaf0 0%, #f4f6f9 100%);
}
.activate-card {
  width: 100%;
  max-width: 420px;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.5);
}
.input-group-text, .form-control {
  border-color: #e2e8f0;
}
.form-control:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.2rem rgba(245, 158, 11, 0.15);
  background: #fff !important;
}
</style>
