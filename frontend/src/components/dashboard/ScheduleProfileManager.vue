<template>
  <div class="modal-overlay zalo-modal-overlay" @click.self="$emit('close')">
    <div class="glass-modal-card animate-slide-up" style="max-width: 650px;">
      <div class="glass-modal-header bg-warning bg-opacity-25 border-bottom border-light">
        <h5 class="modal-title fw-bold text-dark-gold mb-0">
          <i class="bi bi-diagram-3-fill me-2"></i> Quản Lý Mẫu Lịch Trực
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
      </div>
      
      <div class="glass-modal-body text-start">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h6 class="fw-bold text-dark mb-1">Danh sách Mẫu Lịch</h6>
            <p class="text-muted small mb-0">Thiết lập các khung giờ trực cố định trong tuần</p>
          </div>
          <button class="btn btn-premium rounded-pill px-4 shadow-sm" @click="openCreate">
            <i class="bi bi-plus-lg me-1"></i> Tạo Mới
          </button>
        </div>

        <div v-if="isLoading" class="text-center py-5">
          <div class="spinner-border text-warning" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
        </div>
        
        <div v-else class="list-group gap-3 mb-4">
          <div v-if="profiles.length === 0" class="text-center text-muted fst-italic py-5 bg-white bg-opacity-50 rounded-4 border border-light">
            <i class="bi bi-calendar-x fs-1 text-black-50 mb-3 d-block"></i>
            Chưa có mẫu lịch nào. Hãy tạo mới ngay!
          </div>
          
          <div v-for="profile in profiles" :key="profile.id" 
               class="glass-list-item d-flex justify-content-between align-items-center p-3 rounded-4 shadow-sm position-relative overflow-hidden">
            <div class="d-flex align-items-center gap-3 position-relative z-1">
              <div class="icon-circle bg-warning bg-opacity-10 text-dark-gold d-flex justify-content-center align-items-center rounded-circle" style="width: 48px; height: 48px;">
                <i class="bi bi-calendar-check fs-5"></i>
              </div>
              <div>
                <h6 class="mb-1 fw-bold text-dark">{{ profile.name }}</h6>
                <p class="mb-0 small text-muted"><i class="bi bi-journal-text me-1"></i> {{ profile.description || 'Không có mô tả' }}</p>
              </div>
            </div>
            <div class="d-flex gap-2 position-relative z-1">
              <button class="btn btn-sm btn-light text-primary rounded-pill px-3 shadow-sm hover-elevate" @click="openEdit(profile.id)">
                <i class="bi bi-pencil-fill me-1"></i> Sửa
              </button>
              <button class="btn btn-sm btn-light text-danger rounded-pill px-3 shadow-sm hover-elevate" @click="confirmDelete(profile.id)">
                <i class="bi bi-trash-fill me-1"></i> Xóa
              </button>
            </div>
          </div>
        </div>

        <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end">
          <button type="button" class="btn btn-light rounded-pill px-5 py-2 glass-btn text-dark fw-bold shadow-sm" @click="$emit('close')">Đóng</button>
        </div>
      </div>
    </div>

    <!-- Builder Modal -->
    <Teleport to="body">
      <ScheduleProfileBuilder
        v-if="showBuilder"
        :editProfileId="currentEditId"
        @cancel="showBuilder = false"
        @saved="handleBuilderSaved"
      />
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { scheduleProfileService } from '../../services/scheduleProfile.service';
import Swal from 'sweetalert2';
import ScheduleProfileBuilder from './ScheduleProfileBuilder.vue';

const emit = defineEmits(['close', 'changed']);

const profiles = ref([]);
const isLoading = ref(false);

const showBuilder = ref(false);
const currentEditId = ref(null);

const loadProfiles = async () => {
  isLoading.value = true;
  try {
    const data = await scheduleProfileService.getProfiles();
    profiles.value = data.filter(p => p.isActive !== false); // Hide soft-deleted
  } catch (error) {
    console.error(error);
    Swal.fire('Lỗi', 'Không thể tải danh sách mẫu lịch.', 'error');
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  loadProfiles();
});

const openCreate = () => {
  currentEditId.value = null;
  showBuilder.value = true;
};

const openEdit = (id) => {
  currentEditId.value = id;
  showBuilder.value = true;
};

const handleBuilderSaved = () => {
  showBuilder.value = false;
  loadProfiles();
  emit('changed'); // notify parent to reload schedules if needed
};

const confirmDelete = (id) => {
  Swal.fire({
    title: 'Xóa mẫu lịch?',
    text: "Bạn không thể phục hồi lại thao tác này!",
    icon: 'warning',
    showCancelButton: true,
    confirmButtonColor: '#dc3545',
    cancelButtonColor: '#6c757d',
    confirmButtonText: 'Đồng ý, Xóa',
    cancelButtonText: 'Hủy'
  }).then(async (result) => {
    if (result.isConfirmed) {
      try {
        await scheduleProfileService.deleteProfile(id);
        Swal.fire('Đã xóa!', 'Mẫu lịch đã được xóa.', 'success');
        loadProfiles();
        emit('changed');
      } catch (error) {
        Swal.fire('Lỗi', 'Không thể xóa mẫu lịch.', 'error');
      }
    }
  });
};
</script>

<style scoped>
.zalo-modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(8px);
  z-index: 1200;
  display: flex; justify-content: center; align-items: center;
  padding: 1rem;
}
.glass-modal-card {
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: 20px;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.15);
  overflow: hidden;
  width: 100%;
}
.glass-modal-header {
  padding: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.glass-modal-body {
  padding: 2rem 1.5rem;
  max-height: 80vh;
  overflow-y: auto;
}
.text-dark-gold {
  color: #b25e00;
}
.btn-premium {
  background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%);
  color: #fff;
  border: none;
  font-weight: 600;
  transition: all 0.3s ease;
}
.btn-premium:hover {
  background: linear-gradient(135deg, #ffb300 0%, #f57c00 100%);
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(255, 152, 0, 0.3) !important;
  color: white;
}
.glass-list-item {
  background: rgba(255, 255, 255, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(10px);
  transition: all 0.3s ease;
}
.glass-list-item:hover {
  background: rgba(255, 255, 255, 0.95);
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.08) !important;
  border-color: #ffc107;
}
.glass-btn {
  background: rgba(255, 255, 255, 0.5);
  backdrop-filter: blur(5px);
  border: 1px solid rgba(255, 255, 255, 0.6);
  transition: all 0.3s ease;
}
.glass-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}
.hover-elevate {
  transition: transform 0.2s, box-shadow 0.2s;
}
.hover-elevate:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.15) !important;
}
/* Animations */
.animate-slide-up { animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
