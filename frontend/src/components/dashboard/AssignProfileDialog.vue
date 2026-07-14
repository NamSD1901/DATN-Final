<template>
  <div class="modal-overlay zalo-modal-overlay" @click.self="$emit('close')">
    <div class="glass-modal-card animate-slide-up" style="max-width: 600px;">
      <div class="glass-modal-header bg-info bg-opacity-10 border-bottom border-light">
        <h5 class="modal-title fw-bold text-info-dark mb-0">
          <i class="bi bi-people-fill me-2"></i> Gán Profile Cho Bác Sĩ
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
      </div>
      
      <div class="glass-modal-body text-start">
        <div class="mb-4">
          <label class="form-label small fw-bold text-muted mb-1">Chọn Mẫu Lịch (Profile) <span class="text-danger">*</span></label>
          <select v-model="form.profileId" class="form-select glass-input fw-bold px-3 py-2">
            <option value="">-- Chọn một mẫu --</option>
            <option v-for="profile in profiles" :key="profile.id" :value="profile.id">
              {{ profile.name }}
            </option>
          </select>
        </div>

        <div class="mb-4">
          <label class="form-label small fw-bold text-muted mb-1">Ngày áp dụng <span class="text-danger">*</span></label>
          <input type="date" v-model="form.effectiveDate" class="form-control glass-input fw-bold px-3 py-2" />
        </div>

        <div class="mb-3">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <label class="form-label small fw-bold text-muted mb-0">Chọn Bác sĩ <span class="text-danger">*</span></label>
            <div class="form-check mb-0 d-flex align-items-center">
              <input class="form-check-input glass-switch-info mt-0 me-2" type="checkbox" id="selectAllDocs" :checked="isAllSelected" @change="toggleSelectAll" style="cursor: pointer;">
              <label class="form-check-label small fw-bold text-info-dark" for="selectAllDocs" style="cursor: pointer;">
                Chọn tất cả
              </label>
            </div>
          </div>
          <div class="glass-list-container p-2 rounded-4 overflow-auto shadow-sm" style="max-height: 250px;">
            <div v-for="doctor in doctors" :key="doctor.id" 
                 class="doctor-list-item d-flex align-items-center mb-2 p-2 rounded-3 transition-all"
                 :class="{'bg-info bg-opacity-10': form.doctorIds.includes(doctor.id)}">
              <input class="form-check-input glass-check-info m-0 ms-2 me-3 flex-shrink-0" type="checkbox" :value="doctor.id" :id="'doc_' + doctor.id" v-model="form.doctorIds" style="width: 1.25rem; height: 1.25rem; cursor: pointer;">
              <label class="form-check-label d-flex align-items-center flex-grow-1 m-0" :for="'doc_' + doctor.id" style="cursor: pointer;">
                <div class="avatar-circle rounded-circle text-white d-flex align-items-center justify-content-center fw-bold shadow-sm" 
                     :class="form.doctorIds.includes(doctor.id) ? 'bg-gradient-info' : 'bg-secondary bg-opacity-50'"
                     style="width: 40px; height: 40px; font-size: 1rem; margin-right: 12px; transition: all 0.3s ease;">
                  {{ doctor.fullName.charAt(0) }}
                </div>
                <div class="d-flex flex-column">
                  <span class="fw-bold text-dark" style="font-size: 0.95rem;">{{ doctor.fullName }}</span>
                  <span class="text-muted" style="font-size: 0.8rem;"><i class="bi bi-envelope-at me-1"></i>{{ doctor.email }}</span>
                </div>
              </label>
            </div>
            <div v-if="doctors.length === 0" class="text-center text-muted small fst-italic py-4">
              <i class="bi bi-search fs-4 d-block mb-2 text-black-50"></i>
              Không tìm thấy bác sĩ nào.
            </div>
          </div>
        </div>

        <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end gap-2">
          <button type="button" class="btn btn-light rounded-pill px-4 glass-btn text-dark fw-bold shadow-sm" @click="$emit('close')">Hủy</button>
          <button type="button" @click="assignProfile" :disabled="isAssigning || !form.profileId || form.doctorIds.length === 0" class="btn btn-premium-info rounded-pill px-4 shadow-sm">
            <span v-if="isAssigning" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            Áp dụng Profile
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { scheduleProfileService } from '../../services/scheduleProfile.service';
import Swal from 'sweetalert2';
import api from '../../services/api';

const emit = defineEmits(['close', 'assigned']);

const profiles = ref([]);
const doctors = ref([]);
const isAssigning = ref(false);

const form = ref({
  profileId: '',
  doctorIds: [],
  effectiveDate: new Date().toISOString().split('T')[0]
});

const isAllSelected = computed(() => {
  return doctors.value.length > 0 && form.value.doctorIds.length === doctors.value.length;
});

const toggleSelectAll = (e) => {
  if (e.target.checked) {
    form.value.doctorIds = doctors.value.map(d => d.id);
  } else {
    form.value.doctorIds = [];
  }
};

onMounted(async () => {
  try {
    try {
        const profilesRes = await api.get('/ScheduleProfile');
        profiles.value = profilesRes.data || [];
    } catch {
        profiles.value = [{ id: 1, name: 'Mẫu Lịch Hành Chính' }];
    }
    
    const doctorsRes = await api.get('/admin/users');
    const allUsers = doctorsRes.data || [];
    doctors.value = allUsers.filter(u => {
      const r = u.role?.toLowerCase() || '';
      return r === 'doctor' || r === 'clinical_doctor' || r === 'vaccination_doctor';
    });
  } catch (error) {
    console.error('Failed to load initial data', error);
  }
});

const assignProfile = async () => {
  if (!form.value.profileId || form.value.doctorIds.length === 0) return;
  
  try {
    isAssigning.value = true;
    
    await scheduleProfileService.assignProfile({
      profileId: parseInt(form.value.profileId),
      doctorIds: form.value.doctorIds,
      effectiveDate: new Date(form.value.effectiveDate).toISOString()
    });

    Swal.fire({
      icon: 'success',
      title: 'Thành công',
      text: 'Đã gán mẫu lịch trực cho các bác sĩ được chọn.',
      confirmButtonColor: '#0dcaf0'
    });
    
    emit('assigned');
    emit('close');
  } catch (error) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: error.response?.data?.message || 'Không thể gán mẫu lịch',
      confirmButtonColor: '#dc3545'
    });
  } finally {
    isAssigning.value = false;
  }
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
  background: rgba(255, 255, 255, 0.95);
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
.text-info-dark {
  color: #0588a6;
}
.bg-gradient-info {
  background: linear-gradient(135deg, #0dcaf0 0%, #0588a6 100%);
}
.glass-input {
  background: rgba(255, 255, 255, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(5px);
  border-radius: 10px;
  transition: all 0.3s ease;
}
.glass-input:focus {
  background: rgba(255, 255, 255, 0.9);
  border-color: #0dcaf0;
  box-shadow: 0 0 0 0.25rem rgba(13, 202, 240, 0.25);
}
.glass-list-container {
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(5px);
}
.doctor-list-item {
  border: 1px solid transparent;
}
.doctor-list-item:hover {
  background-color: rgba(255, 255, 255, 0.8);
  border-color: rgba(13, 202, 240, 0.3);
}
.glass-check-info:checked {
  background-color: #0dcaf0;
  border-color: #0dcaf0;
}
.glass-switch-info:checked {
  background-color: #0dcaf0;
  border-color: #0dcaf0;
}
.btn-premium-info {
  background: linear-gradient(135deg, #0dcaf0 0%, #08a0c4 100%);
  color: #fff;
  border: none;
  font-weight: 600;
  transition: all 0.3s ease;
}
.btn-premium-info:hover {
  background: linear-gradient(135deg, #31d2f2 0%, #068ca8 100%);
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(13, 202, 240, 0.3) !important;
  color: white;
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
.transition-all {
  transition: all 0.2s ease;
}
/* Animations */
.animate-slide-up { animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
