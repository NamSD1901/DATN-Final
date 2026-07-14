<template>
  <Teleport to="body">
    <div class="modal-overlay zalo-modal-overlay" @click.self="$emit('close')">
      <div class="glass-modal-card animate-slide-up" style="max-width: 550px;">
        <div class="glass-modal-header bg-warning bg-opacity-10 border-bottom border-light">
          <h5 class="modal-title fw-bold text-warning-dark mb-0">
            <i class="bi bi-person-lines-fill me-2"></i> Yêu Cầu Xin Nghỉ / Đổi Ca
          </h5>
          <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
        </div>
        
        <div class="glass-modal-body text-start">
          <div class="mb-4">
            <label class="form-label small fw-bold text-muted mb-1">Loại Yêu Cầu <span class="text-danger">*</span></label>
            <select v-model="form.type" class="form-select glass-input fw-bold px-3 py-2 text-dark">
              <option value="TimeOff">Xin Nghỉ Phép (Time-Off)</option>
              <option value="ShiftSwap">Xin Đổi Ca (Shift Swap)</option>
            </select>
          </div>

          <div class="row g-3 mb-4">
            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Từ ngày <span class="text-danger">*</span></label>
              <input type="date" v-model="form.startDate" class="form-control glass-input fw-bold px-3 py-2 text-dark" />
            </div>
            <div class="col-md-6">
              <label class="form-label small fw-bold text-muted mb-1">Đến ngày <span class="text-danger">*</span></label>
              <input type="date" v-model="form.endDate" class="form-control glass-input fw-bold px-3 py-2 text-dark" />
            </div>
          </div>

          <div v-if="form.type === 'ShiftSwap'" class="mb-4">
            <label class="form-label small fw-bold text-muted mb-1">Bác sĩ trực thay <span class="text-danger">*</span></label>
            <select v-model="form.substituteDoctorId" class="form-select glass-input fw-bold px-3 py-2 text-dark">
              <option value="">-- Chọn bác sĩ trực thay --</option>
              <option v-for="doc in doctors" :key="doc.id" :value="doc.id">
                👨‍⚕️ Bs. {{ doc.fullName }}
              </option>
            </select>
          </div>

          <div class="mb-3">
            <label class="form-label small fw-bold text-muted mb-1">Lý do (Tùy chọn)</label>
            <textarea v-model="form.reason" class="form-control glass-input px-3 py-2 text-dark" rows="3" placeholder="Ví dụ: Nghỉ ốm, Việc gia đình..."></textarea>
          </div>

          <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end gap-2">
            <button type="button" class="btn btn-light rounded-pill px-4 glass-btn text-dark fw-bold shadow-sm" @click="$emit('close')">Hủy</button>
            <button type="button" @click="submitRequest" :disabled="isSubmitting || !isValid" class="btn btn-premium-warning rounded-pill px-4 shadow-sm">
              <span v-if="isSubmitting" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
              Gửi Yêu Cầu
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
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
.text-warning-dark {
  color: #b25e00;
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
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.25rem rgba(245, 158, 11, 0.25);
}
.btn-premium-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: #fff;
  border: none;
  font-weight: 600;
  transition: all 0.3s ease;
}
.btn-premium-warning:hover {
  background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(245, 158, 11, 0.3) !important;
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
/* Animations */
.animate-slide-up { animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
