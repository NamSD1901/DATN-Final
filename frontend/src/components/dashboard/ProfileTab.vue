<template>
  <div class="container-fluid p-0">
    <!-- Toast Notifications -->
    <TransitionGroup name="toast-fade" tag="div" class="toast-container">
      <div v-for="toast in toasts" :key="toast.id" :class="['toast', `toast-${toast.type}`]">
        <component :is="toast.icon" class="toast-icon" />
        <span class="toast-message">{{ toast.message }}</span>
      </div>
    </TransitionGroup>

    <div class="profile-grid shadow-sm rounded-4">
      <!-- Sidebar with Avatar card -->
      <div class="profile-sidebar">
        <div class="sidebar-card">
          <div class="avatar-section">
            <div class="avatar-container">
              <img :src="getAvatarUrl(profile.avatar)" @error="handleAvatarError" alt="Avatar" class="avatar-img" />
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
                <button type="submit" :disabled="saving" class="btn btn-premium px-5 py-2.5 rounded-pill shadow-sm fw-bold">
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
                <button type="submit" :disabled="saving" class="btn btn-premium px-5 py-2.5 rounded-pill shadow-sm fw-bold">
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
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import api from '../../services/api';
import { 
  User, 
  Lock, 
  Mail, 
  Camera, 
  AlertCircle, 
  CheckCircle2, 
  Info 
} from '@lucide/vue';

const emit = defineEmits(['profile-updated']);

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
  if (!avatarPath) return `https://ui-avatars.com/api/?name=${encodeURIComponent(profile.fullName || 'User')}&background=f59e0b&color=fff&rounded=true`;
  if (avatarPath.startsWith('http')) return avatarPath;
  return `${backendUrl}${avatarPath}`;
};

const handleAvatarError = (event: Event) => {
  const target = event.target as HTMLImageElement;
  target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(profile.fullName || 'User')}&background=f59e0b&color=fff&rounded=true`;
};

const translateRole = (role: string) => {
  if (!role) return '';
  const r = role.toLowerCase();
  if (r === 'customer') return 'Khách hàng';
  if (r === 'doctor' || r === 'clinical_doctor' || r === 'vaccination_doctor') return 'Bác sĩ thú y';
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

      // Notify parent to update dashboard topbar
      emit('profile-updated');
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
      // Notify parent to update dashboard topbar
      emit('profile-updated');
    }
  } catch (error: any) {
    showErrorToast(error.response?.data?.message || 'Không thể tải ảnh đại diện lên.');
  } finally {
    uploading.value = false;
  }
};

onMounted(() => {
  fetchProfile();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700&subset=vietnamese&display=swap');

.profile-grid {
  background: white;
  display: grid;
  grid-template-columns: 320px 1fr;
  border: 1px solid #f1f5f9;
  overflow: hidden;
  min-height: 550px;
  font-family: 'Be Vietnam Pro', sans-serif;
}

@media (max-width: 868px) {
  .profile-grid {
    grid-template-columns: 1fr;
    border-radius: 20px;
  }
}

/* Sidebar Styling */
.profile-sidebar {
  display: flex;
  flex-direction: column;
}

.sidebar-card {
  background: #fdfaf0;
  border-right: 1px solid #f0f0f0;
  padding: 3rem 2rem;
  text-align: center;
  height: 100%;
}

.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 2rem;
}

.avatar-container {
  position: relative;
  width: 130px;
  height: 130px;
  border-radius: 50%;
  padding: 4px;
  background: linear-gradient(135deg, var(--primary-gold), #fff7e0);
  box-shadow: 0 8px 24px rgba(245, 158, 11, 0.15);
  margin-bottom: 1.5rem;
}

.avatar-img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
  border: 5px solid #ffffff;
}

.avatar-upload-label {
  position: absolute;
  bottom: 0;
  right: 0;
  background: var(--primary-gold);
  color: white;
  width: 38px;
  height: 38px;
  border-radius: 50%;
  display: flex;
  justify-content: center;
  align-items: center;
  cursor: pointer;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15);
  transition: all 0.3s;
}

.avatar-upload-label:hover {
  background: var(--primary-dark);
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
  font-size: 1.3rem;
  font-weight: 800;
  color: #0f172a;
  margin-bottom: 0.5rem;
  letter-spacing: -0.5px;
}

.user-role {
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--primary-dark);
  text-transform: uppercase;
  letter-spacing: 1px;
  background: rgba(245, 158, 11, 0.1);
  padding: 0.4rem 1.2rem;
  border-radius: 20px;
}

.sidebar-meta {
  border-top: 1px dashed #cbd5e1;
  padding-top: 2rem;
  margin-top: auto;
}

.meta-item {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: #64748b;
  font-size: 0.95rem;
  font-weight: 500;
}

.meta-icon {
  width: 18px;
  height: 18px;
  color: #94a3b8;
}

/* Content Area styling */
.profile-content {
  display: flex;
  flex-direction: column;
}

.content-card {
  background: white;
  padding: 2.5rem 3rem;
  height: 100%;
}

.tabs-nav {
  display: flex;
  gap: 2.5rem;
  border-bottom: 1px solid #f1f5f9;
  margin-bottom: 2.5rem;
}

.tab-btn {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: #64748b;
  font-size: 1.05rem;
  font-weight: 600;
  padding-bottom: 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.3s;
}

.tab-btn:hover {
  color: #0f172a;
}

.tab-btn.active {
  color: var(--primary-dark);
  border-color: var(--primary-gold);
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
  gap: 0.6rem;
}

.input-group label {
  color: #475569;
  font-size: 0.9rem;
  font-weight: 700;
}

.form-input, .form-select, .form-textarea {
  width: 100%;
  padding: 0.85rem 1.25rem;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  color: #0f172a;
  font-size: 0.95rem;
  font-family: inherit;
  transition: all 0.3s ease;
  box-shadow: none;
}

.form-input:focus, .form-select:focus, .form-textarea:focus {
  outline: none;
  border-color: var(--primary-gold);
  background: #ffffff;
  box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.15);
}

.form-select option {
  background: #ffffff;
  color: #0f172a;
}

.form-textarea {
  resize: vertical;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 1rem;
  padding-top: 1.5rem;
  border-top: 1px dashed #f1f5f9;
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

/* Toast styling */
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
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
  font-size: 0.9rem;
  font-weight: 500;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.toast-success {
  background: #10b981;
}

.toast-error {
  background: #ef4444;
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
