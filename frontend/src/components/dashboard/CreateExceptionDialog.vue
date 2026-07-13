<template>
  <div class="modal-overlay glass-overlay" @click.self="$emit('close')">
    <div class="modal-card glass-card">
      <div class="modal-header border-bottom px-4 py-3 bg-white bg-opacity-75 d-flex justify-content-between align-items-center w-100">
        <h5 class="fw-bold text-warning mb-0">
          <i class="bi bi-person-lines-fill me-2 text-warning"></i> Yêu Cầu Xin Nghỉ / Đổi Ca
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
      </div>
      
      <div class="modal-body p-4 bg-white bg-opacity-50 text-start">
        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Loại Yêu Cầu <span class="text-danger">*</span></label>
          <select v-model="form.type" class="form-select form-select-sm border-warning-subtle fw-bold text-dark">
            <option value="TimeOff">Xin Nghỉ Phép (Time-Off)</option>
            <option value="ShiftSwap">Xin Đổi Ca (Shift Swap)</option>
          </select>
        </div>

        <div class="row g-3 mb-3">
          <div class="col-md-6">
            <label class="form-label small fw-bold text-muted mb-1">Từ ngày <span class="text-danger">*</span></label>
            <input type="date" v-model="form.startDate" class="form-control form-control-sm border-warning-subtle fw-bold" />
          </div>
          <div class="col-md-6">
            <label class="form-label small fw-bold text-muted mb-1">Đến ngày <span class="text-danger">*</span></label>
            <input type="date" v-model="form.endDate" class="form-control form-control-sm border-warning-subtle fw-bold" />
          </div>
        </div>

        <div v-if="form.type === 'ShiftSwap'" class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Bác sĩ trực thay <span class="text-danger">*</span></label>
          <select v-model="form.substituteDoctorId" class="form-select form-select-sm border-warning-subtle fw-bold text-dark">
            <option value="">-- Chọn bác sĩ trực thay --</option>
            <option v-for="doc in doctors" :key="doc.id" :value="doc.id">
              {{ doc.fullName }}
            </option>
          </select>
        </div>

        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Lý do (Tùy chọn)</label>
          <textarea v-model="form.reason" class="form-control form-control-sm border-warning-subtle" rows="3" placeholder="Ví dụ: Nghỉ ốm, Việc gia đình..."></textarea>
        </div>

        <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end gap-2">
          <button type="button" class="btn btn-sm btn-light fw-bold border px-4" @click="$emit('close')">Hủy</button>
          <button type="button" @click="submitRequest" :disabled="isSubmitting || !isValid" class="btn btn-sm btn-warning text-dark fw-bold px-4 shadow-sm">
            <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            Gửi Yêu Cầu
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { scheduleExceptionService } from '../../services/scheduleException.service';
import Swal from 'sweetalert2';
import api from '../../services/api';

const props = defineProps({
  currentDoctorId: {
    type: String,
    required: true
  }
});

const emit = defineEmits(['close', 'submitted']);

const form = ref({
  type: 'TimeOff',
  startDate: new Date().toISOString().split('T')[0],
  endDate: new Date().toISOString().split('T')[0],
  substituteDoctorId: '',
  reason: ''
});

const doctors = ref([]);
const isSubmitting = ref(false);

const isValid = computed(() => {
  if (!form.value.startDate || !form.value.endDate) return false;
  if (form.value.type === 'ShiftSwap' && !form.value.substituteDoctorId) return false;
  return true;
});

onMounted(async () => {
  try {
    const res = await api.get('/admin/users');
    const allUsers = res.data || [];
    doctors.value = allUsers.filter(u => {
      const r = u.role?.toLowerCase() || '';
      return (r === 'doctor' || r === 'clinical_doctor' || r === 'vaccination_doctor') && u.id !== props.currentDoctorId;
    });
  } catch (error) {
    console.error('Failed to load doctors', error);
  }
});

const submitRequest = async () => {
  try {
    isSubmitting.value = true;
    
    await scheduleExceptionService.createException({
      doctorId: props.currentDoctorId,
      type: form.value.type,
      startDate: new Date(form.value.startDate).toISOString(),
      endDate: new Date(form.value.endDate).toISOString(),
      substituteDoctorId: form.value.type === 'ShiftSwap' ? form.value.substituteDoctorId : undefined,
      reason: form.value.reason
    });

    Swal.fire({
      icon: 'success',
      title: 'Thành công',
      text: 'Yêu cầu của bạn đã được gửi cho Admin phê duyệt.',
      confirmButtonColor: '#ffc107'
    });
    
    emit('submitted');
    emit('close');
  } catch (error) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: error.response?.data?.message || 'Không thể gửi yêu cầu',
      confirmButtonColor: '#dc3545'
    });
  } finally {
    isSubmitting.value = false;
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
.border-warning-subtle {
  border-color: #ffda85;
}
.border-warning-subtle:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.2rem rgba(245, 158, 11, 0.15);
}
</style>
