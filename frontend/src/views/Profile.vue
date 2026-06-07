<template>
  <div class="profile-wrapper">
    <!-- Background elements -->
    <div class="bg-glow bg-glow-1"></div>
    <div class="bg-glow bg-glow-2"></div>

    <!-- Toast Notifications -->
    <TransitionGroup name="toast-fade" tag="div" class="toast-container">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
        <component :is="toast.icon" class="toast-icon" />
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </TransitionGroup>

    <div class="profile-container">
      <!-- Top Action bar -->
      <div class="action-bar">
        <button @click="goBack" class="btn-back">
          <ArrowLeft class="icon-btn" />
          <span>Quay lại Dashboard</span>
        </button>
      </div>

      <div class="profile-grid">
        <!-- Sidebar with Avatar card -->
        <div class="profile-sidebar">
          <div class="sidebar-card">
            <div class="avatar-section">
              <div class="avatar-container">
                <img :src="getAvatarUrl(profile.avatar)" alt="Avatar" class="avatar-img" />
                <label for="avatar-upload" class="avatar-upload-label" :class="{ uploading }">
                  <Camera class="camera-icon" v-if="!uploading" />
                  <div class="mini-spinner" v-else></div>
                  <input 
                    id="avatar-upload" 
                    type="file" 
                    accept="image/*" 
                    @change="handleAvatarUpload" 
                    :disabled="uploading"
                    class="hidden-input"
                  />
                </label>
              </div>
              <h3 class="user-name">{{ profile.fullName || 'Người dùng' }}</h3>
              <span class="user-role">{{ translateRole(profile.roleName) }}</span>
            </div>

            <div class="sidebar-meta">
              <div class="meta-item">
                <Mail class="meta-icon" />
                <span>{{ profile.email }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Main settings forms -->
        <div class="profile-content">
          <div class="content-card">
            <!-- Tabs Navigation -->
            <div class="tabs-nav">
              <button 
                @click="activeTab = 'info'" 
                :class="['tab-btn', { active: activeTab === 'info' }]"
              >
                <User class="tab-icon" />
                Thông tin cá nhân
              </button>
              <button 
                @click="activeTab = 'password'" 
                :class="['tab-btn', { active: activeTab === 'password' }]"
              >
                <Lock class="tab-icon" />
                Đổi mật khẩu
              </button>
            </div>

            <!-- Tab Content: Personal Info -->
            <div v-if="activeTab === 'info'" class="tab-pane">
              <form @submit.prevent="handleUpdateProfile" class="settings-form">
                <div class="form-grid">
                  <div class="input-group">
                    <label for="fullName">Họ và Tên</label>
                    <input 
                      id="fullName" 
                      type="text" 
                      v-model="profileForm.fullName" 
                      required 
                      class="form-input"
                    />
                  </div>

                  <div class="input-group">
                    <label for="phone">Số điện thoại</label>
                    <input 
                      id="phone" 
                      type="tel" 
                      v-model="profileForm.phone" 
                      required 
                      class="form-input"
                    />
                  </div>

                  <div class="input-group">
                    <label for="gender">Giới tính</label>
                    <select id="gender" v-model="profileForm.gender" class="form-select">
                      <option :value="1">Nam</option>
                      <option :value="0">Nữ</option>
                      <option :value="2">Khác</option>
                    </select>
                  </div>

                  <div class="input-group">
                    <label for="dob">Ngày sinh</label>
                    <input 
                      id="dob" 
                      type="date" 
                      v-model="profileForm.dateOfBirth" 
                      class="form-input"
                    />
                  </div>
                </div>

                <div class="input-group full-width">
                  <label for="address">Địa chỉ</label>
                  <textarea 
                    id="address" 
                    rows="3" 
                    v-model="profileForm.address" 
                    class="form-textarea"
                    placeholder="Nhập địa chỉ nhà của bạn..."
                  ></textarea>
                </div>

                <div class="form-actions">
                  <button type="submit" :disabled="saving" class="btn-primary">
                    <span v-if="!saving">Lưu Thay Đổi</span>
                    <div class="spinner" v-else></div>
                  </button>
                </div>
              </form>
            </div>

            <!-- Tab Content: Change Password -->
            <div v-else-if="activeTab === 'password'" class="tab-pane">
              <form @submit.prevent="handleChangePassword" class="settings-form">
                <div class="input-group">
                  <label for="currentPassword">Mật khẩu hiện tại</label>
                  <input 
                    id="currentPassword" 
                    type="password" 
                    v-model="passwordForm.currentPassword" 
                    required 
                    class="form-input"
                    placeholder="••••••••"
                  />
                </div>

                <div class="form-grid">
                  <div class="input-group">
                    <label for="newPassword">Mật khẩu mới</label>
                    <input 
                      id="newPassword" 
                      type="password" 
                      v-model="passwordForm.newPassword" 
                      required 
                      class="form-input"
                      placeholder="Tối thiểu 8 ký tự"
                    />
                  </div>

                  <div class="input-group">
                    <label for="confirmNewPassword">Xác nhận mật khẩu mới</label>
                    <input 
                      id="confirmNewPassword" 
                      type="password" 
                      v-model="passwordForm.confirmNewPassword" 
                      required 
                      class="form-input"
                      placeholder="Nhập lại mật khẩu mới"
                    />
                  </div>
                </div>

                <div class="form-actions">
                  <button type="submit" :disabled="saving" class="btn-primary">
                    <span v-if="!saving">Cập Nhật Mật Khẩu</span>
                    <div class="spinner" v-else></div>
                  </button>
                </div>
              </form>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import api from '../services/api';
import { 
  User, 
  Lock, 
  Mail, 
  ArrowLeft, 
  Camera, 
  AlertCircle, 
  CheckCircle2, 
  Info 
} from '@lucide/vue';

const router = useRouter();
const activeTab = ref<'info' | 'password'>('info');
const loading = ref(true);
const saving = ref(false);
const uploading = ref(false);

const backendUrl = 'https://localhost:7284';

// Profile state
const profile = reactive({
  id: '',
  fullName: '',
  email: '',
  phone: '',
  address: '',
  gender: 1 as number | null,
  dateOfBirth: '' as string | null,
  avatar: '',
  roleName: ''
});

// Editable Form state
const profileForm = reactive({
  fullName: '',
  phone: '',
  address: '',
  gender: 1 as number | null,
  dateOfBirth: '' as string | null
});

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: ''
});

// Toast State
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

const getAvatarUrl = (avatarPath: string) => {
  if (!avatarPath) return 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=150&h=150';
  if (avatarPath.startsWith('http')) return avatarPath;
  return `${backendUrl}${avatarPath}`;
};

const translateRole = (role: string) => {
  if (!role) return '';
  const r = role.toLowerCase();
  if (r === 'customer') return 'Khách hàng';
  if (r === 'doctor') return 'Bác sĩ';
  if (r === 'receptionist') return 'Lễ tân';
  if (r === 'admin') return 'Quản trị viên';
  return role;
};

// Fetch User Profile on load
const fetchProfile = async () => {
  try {
    const response = await api.get('/profile');
    Object.assign(profile, response.data);
    
    // Bind to edit form
    profileForm.fullName = response.data.fullName || '';
    profileForm.phone = response.data.phone || '';
    profileForm.address = response.data.address || '';
    profileForm.gender = response.data.gender;
    
    // Format Date for date input (yyyy-MM-dd)
    if (response.data.dateOfBirth) {
      profileForm.dateOfBirth = response.data.dateOfBirth.split('T')[0];
    } else {
      profileForm.dateOfBirth = null;
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Không thể tải hồ sơ người dùng.');
  } finally {
    loading.value = false;
  }
};

const handleUpdateProfile = async () => {
  saving.value = true;
  try {
    const response = await api.put('/profile', profileForm);
    if (response.data.success) {
      showSuccessToast(response.data.message || 'Cập nhật hồ sơ thành công!');
      // Refresh profile local state
      profile.fullName = profileForm.fullName;
      profile.phone = profileForm.phone;
      profile.address = profileForm.address;
      profile.gender = profileForm.gender;
      profile.dateOfBirth = profileForm.dateOfBirth;
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Có lỗi xảy ra khi cập nhật hồ sơ.');
  } finally {
    saving.value = false;
  }
};

const handleChangePassword = async () => {
  if (passwordForm.newPassword !== passwordForm.confirmNewPassword) {
    showErrorToast('Mật khẩu mới không khớp.');
    return;
  }
  saving.value = true;
  try {
    const response = await api.put('/profile/password', passwordForm);
    if (response.data.success) {
      showSuccessToast(response.data.message || 'Thay đổi mật khẩu thành công!');
      passwordForm.currentPassword = '';
      passwordForm.newPassword = '';
      passwordForm.confirmNewPassword = '';
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Đổi mật khẩu thất bại. Vui lòng kiểm tra lại mật khẩu hiện tại.');
  } finally {
    saving.value = false;
  }
};

const handleAvatarUpload = async (event: Event) => {
  const fileInput = event.target as HTMLInputElement;
  if (!fileInput.files || fileInput.files.length === 0) return;
  
  const file = fileInput.files[0];
  const formData = new FormData();
  formData.append('avatarFile', file);
  
  uploading.value = true;
  try {
    const response = await api.post('/profile/avatar', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    if (response.data.success) {
      profile.avatar = response.data.avatarUrl;
      showSuccessToast('Cập nhật ảnh đại diện thành công!');
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Không thể tải ảnh đại diện lên.');
  } finally {
    uploading.value = false;
  }
};

const goBack = () => {
  router.push('/dashboard');
};

onMounted(() => {
  fetchProfile();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700&display=swap');

.profile-wrapper {
  position: relative;
  min-height: 100vh;
  background: radial-gradient(circle at top right, #1e293b, #0f172a, #0b0f19);
  font-family: 'Outfit', sans-serif;
  color: #f8fafc;
  overflow: hidden;
  padding: 2.5rem 1.5rem;
}

.bg-glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.12;
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

.profile-container {
  position: relative;
  z-index: 1;
  max-width: 1100px;
  margin: 0 auto;
}

.action-bar {
  margin-bottom: 2rem;
}

.btn-back {
  display: flex;
  align-items: center;
  gap: 8px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  padding: 0.6rem 1.2rem;
  border-radius: 10px;
  color: #cbd5e1;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-back:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
  transform: translateX(-4px);
}

.icon-btn {
  width: 18px;
  height: 18px;
}

.profile-grid {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 2rem;
}

@media (max-width: 868px) {
  .profile-grid {
    grid-template-columns: 1fr;
  }
}

/* Sidebar Styling */
.profile-sidebar {
  display: flex;
  flex-direction: column;
}

.sidebar-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 20px;
  padding: 2.5rem 1.8rem;
  text-align: center;
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.25);
}

.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 2rem;
}

.avatar-container {
  position: relative;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  padding: 4px;
  background: linear-gradient(135deg, #14b8a6, #6366f1);
  box-shadow: 0 8px 24px rgba(20, 184, 166, 0.3);
  margin-bottom: 1.2rem;
}

.avatar-img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #0f172a;
}

.avatar-upload-label {
  position: absolute;
  bottom: 0;
  right: 0;
  background: #14b8a6;
  color: white;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.3);
  transition: all 0.3s;
}

.avatar-upload-label:hover {
  background: #0d9488;
  transform: scale(1.1);
}

.camera-icon {
  width: 18px;
  height: 18px;
}

.hidden-input {
  display: none;
}

.user-name {
  font-size: 1.4rem;
  font-weight: 600;
  color: #ffffff;
  margin-bottom: 0.3rem;
}

.user-role {
  font-size: 0.85rem;
  font-weight: 600;
  color: #14b8a6;
  text-transform: uppercase;
  letter-spacing: 1px;
  background: rgba(20, 184, 166, 0.1);
  padding: 0.3rem 0.8rem;
  border-radius: 20px;
}

.sidebar-meta {
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  padding-top: 1.5rem;
}

.meta-item {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: #94a3b8;
  font-size: 0.95rem;
}

.meta-icon {
  width: 18px;
  height: 18px;
  color: #64748b;
}

/* Content Area styling */
.profile-content {
  display: flex;
  flex-direction: column;
}

.content-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 20px;
  padding: 2.5rem;
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.25);
  min-height: 480px;
}

.tabs-nav {
  display: flex;
  gap: 1.5rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  margin-bottom: 2.5rem;
}

.tab-btn {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: #94a3b8;
  font-size: 1rem;
  font-weight: 600;
  padding-bottom: 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s;
}

.tab-btn:hover {
  color: #ffffff;
}

.tab-btn.active {
  color: #14b8a6;
  border-color: #14b8a6;
}

.tab-icon {
  width: 18px;
  height: 18px;
}

/* Forms layout inside tabs */
.settings-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
}

@media (max-width: 600px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.input-group label {
  color: #cbd5e1;
  font-size: 0.9rem;
  font-weight: 500;
}

.form-input, .form-select, .form-textarea {
  width: 100%;
  padding: 0.85rem 1rem;
  background: rgba(15, 23, 42, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  color: #ffffff;
  font-size: 0.95rem;
  font-family: inherit;
  transition: all 0.3s ease;
}

.form-input:focus, .form-select:focus, .form-textarea:focus {
  outline: none;
  border-color: #14b8a6;
  box-shadow: 0 0 0 3px rgba(20, 184, 166, 0.15);
  background: rgba(15, 23, 42, 0.8);
}

.form-select option {
  background: #1e293b;
  color: white;
}

.form-textarea {
  resize: vertical;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 1rem;
}

.btn-primary {
  background: linear-gradient(135deg, #14b8a6, #0d9488);
  color: #ffffff;
  border: none;
  border-radius: 10px;
  padding: 0.85rem 2rem;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  justify-content: center;
  align-items: center;
  min-width: 150px;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(20, 184, 166, 0.3);
  background: linear-gradient(135deg, #2dd4bf, #14b8a6);
}

.btn-primary:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 0.8s linear infinite;
}

.mini-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.4);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Toast styling matching Login/Register */
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
