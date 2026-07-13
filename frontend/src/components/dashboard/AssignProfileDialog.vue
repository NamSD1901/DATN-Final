<template>
  <div class="modal-overlay glass-overlay" @click.self="$emit('close')">
    <div class="modal-card glass-card">
      <div class="modal-header border-bottom px-4 py-3 bg-white bg-opacity-75 d-flex justify-content-between align-items-center w-100">
        <h5 class="fw-bold text-info mb-0">
          <i class="bi bi-people-fill me-2 text-info"></i> Gán Profile Cho Bác Sĩ
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
      </div>
      
      <div class="modal-body p-4 bg-white bg-opacity-50 text-start">
        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Chọn Mẫu Lịch (Profile) <span class="text-danger">*</span></label>
          <select v-model="form.profileId" class="form-select form-select-sm border-info-subtle fw-bold">
            <option value="">-- Chọn một mẫu --</option>
            <option v-for="profile in profiles" :key="profile.id" :value="profile.id">
              {{ profile.name }}
            </option>
          </select>
        </div>

        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Ngày áp dụng <span class="text-danger">*</span></label>
          <input type="date" v-model="form.effectiveDate" class="form-control form-control-sm border-info-subtle fw-bold" />
        </div>

        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Chọn Bác sĩ (Có thể chọn nhiều) <span class="text-danger">*</span></label>
          <div class="border border-info-subtle rounded p-3 bg-white overflow-auto shadow-sm" style="max-height: 250px;">
            <div v-for="doctor in doctors" :key="doctor.id" class="form-check d-flex align-items-center mb-2">
              <input class="form-check-input mt-0 me-3 border-info" type="checkbox" :value="doctor.id" :id="'doc_' + doctor.id" v-model="form.doctorIds" style="width: 1.2rem; height: 1.2rem;">
              <label class="form-check-label d-flex align-items-center flex-grow-1" :for="'doc_' + doctor.id" style="cursor: pointer;">
                <div class="avatar-circle rounded-circle bg-info text-white d-flex align-items-center justify-content-center fw-bold shadow-sm" style="width: 32px; height: 32px; font-size: 0.9rem; margin-right: 12px;">
                  {{ doctor.fullName.charAt(0) }}
                </div>
                <div class="d-flex flex-column">
                  <span class="fw-bold text-dark">{{ doctor.fullName }}</span>
                  <span class="text-muted" style="font-size: 0.75rem;">{{ doctor.email }}</span>
                </div>
              </label>
            </div>
            <div v-if="doctors.length === 0" class="text-center text-muted small fst-italic">
              Không tìm thấy bác sĩ nào.
            </div>
          </div>
        </div>

        <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end gap-2">
          <button type="button" class="btn btn-sm btn-light fw-bold border px-4" @click="$emit('close')">Hủy</button>
          <button type="button" @click="assignProfile" :disabled="isAssigning || !form.profileId || form.doctorIds.length === 0" class="btn btn-sm btn-info text-white fw-bold px-4 shadow-sm">
            <span v-if="isAssigning" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            Áp dụng
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
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
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 1050;
  display: flex;
  justify-content: center;
  align-items: center;
}
.glass-overlay {
  background: rgba(0, 0, 0, 0.45);
  backdrop-filter: blur(8px);
}
.modal-card {
  width: 100%;
  max-width: 550px;
  max-height: 90vh;
  border-radius: 16px;
  box-shadow: 0 15px 35px rgba(0,0,0,0.2);
  display: flex;
  flex-direction: column;
}
.modal-body {
  overflow-y: auto;
}
.glass-card {
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.5);
}
.border-info-subtle {
  border-color: #9eeaf9;
}
.border-info-subtle:focus {
  border-color: #0dcaf0;
  box-shadow: 0 0 0 0.2rem rgba(13, 202, 240, 0.15);
}
</style>
