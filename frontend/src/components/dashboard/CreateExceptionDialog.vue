<template>
  <Teleport to="body">
    <div class="modal-overlay zalo-modal-overlay" @click.self="$emit('close')">
      <div class="glass-modal-card animate-slide-up" style="max-width: 550px;">
        <div class="glass-modal-header bg-warning bg-opacity-10 border-bottom border-light">
          <h5 class="modal-title fw-bold text-warning-dark mb-0">
            <i class="bi bi-person-lines-fill me-2"></i> Đăng Ký Nghỉ Phép
          </h5>
          <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('close')"></button>
        </div>
        
        <div class="glass-modal-body text-start">
          <div class="mb-4">
            <label class="form-label small fw-bold text-muted mb-1">Loại Yêu Cầu <span class="text-danger">*</span></label>
            <select v-model="form.type" class="form-select glass-input fw-bold px-3 py-2 text-dark">
              <option value="TimeOff">Xin Nghỉ Phép (Time-Off)</option>
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

          <div class="mb-3 bg-white bg-opacity-50 p-3 rounded-3 border border-light shadow-sm">
            <div class="form-check form-switch mb-2 d-flex align-items-center gap-2">
              <input class="form-check-input" type="checkbox" role="switch" id="allDaySwitchDoctor" v-model="form.isAllDay" style="transform: scale(1.2); cursor: pointer;">
              <label class="form-check-label text-dark fw-bold small" for="allDaySwitchDoctor" style="cursor: pointer;">
                <i class="bi bi-clock-history text-primary me-1"></i> Nghỉ cả ngày (All day)
              </label>
            </div>
            <div class="text-muted small ms-4" style="font-size: 0.75rem" v-if="form.isAllDay">
              Hệ thống sẽ tự động gán thời gian nghỉ cả ngày (00:00 - 23:59).
            </div>
          </div>

          <div class="row g-3 mb-3" v-if="!form.isAllDay">
            <div class="col-6">
              <label class="form-label text-muted small fw-bold">
                <i class="bi bi-play-circle-fill text-success me-1"></i> Giờ bắt đầu *
              </label>
              <TimePicker
                v-model="form.startHour"
                :slots="timeSlotOptions"
                @change="validateEndTime"
              />
            </div>
            <div class="col-6">
              <label class="form-label text-muted small fw-bold">
                <i class="bi bi-stop-circle-fill text-danger me-1"></i> Giờ kết thúc *
              </label>
              <TimePicker
                v-model="form.endHour"
                :slots="endTimeSlotOptions"
              />
            </div>
          </div>

          <!-- Duration Badge -->
          <div v-if="form.isAllDay || (form.startHour && form.endHour)" class="mb-4">
            <div class="duration-badge d-inline-flex align-items-center gap-2 px-3 py-2 rounded-pill bg-light border border-white shadow-sm">
              <i class="bi bi-hourglass-split text-danger"></i>
              <span class="fw-bold small text-dark">Tổng thời gian: <strong class="text-danger">{{ blockDuration }}</strong></span>
            </div>
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
import TimePicker from '../shared/TimePicker.vue';

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
  isAllDay: true,
  startHour: '08:00',
  endHour: '12:00',
  substituteDoctorId: '',
  reason: ''
});

const timeSlotOptions = computed(() => {
  const slots = [];
  for (let h = 8; h <= 20; h++) {
    slots.push(`${String(h).padStart(2, '0')}:00`);
    if (h < 20) slots.push(`${String(h).padStart(2, '0')}:30`);
  }
  return slots;
});

const endTimeSlotOptions = computed(() => {
  if (form.value.startDate < form.value.endDate) {
    return timeSlotOptions.value;
  }
  return timeSlotOptions.value.filter(s => s > form.value.startHour);
});

const validateEndTime = () => {
  if (form.value.startDate === form.value.endDate && form.value.endHour <= form.value.startHour) {
    const idx = endTimeSlotOptions.value.findIndex(s => s > form.value.startHour);
    form.value.endHour = endTimeSlotOptions.value[idx >= 0 ? idx : 0] || '12:00';
  }
};

const blockDuration = computed(() => {
  if (!form.value.startDate || !form.value.endDate) return '';
  
  let startStr = `${form.value.startDate}T${form.value.isAllDay ? '00:00' : form.value.startHour}:00`;
  let endStr = `${form.value.endDate}T${form.value.isAllDay ? '23:59' : form.value.endHour}:00`;
  
  const start = new Date(startStr);
  const end = new Date(endStr);
  
  const totalMin = Math.round((end - start) / 60000);
  
  if (totalMin <= 0) return 'Không hợp lệ';
  
  if (form.value.isAllDay) {
    const days = Math.round(totalMin / (24 * 60));
    return days > 1 ? `${days} ngày` : '1 ngày (Cả ngày)';
  }

  const h = Math.floor(totalMin / 60);
  const m = totalMin % 60;
  
  if (h >= 24) {
    const d = Math.floor(h / 24);
    const remH = h % 24;
    let res = `${d} ngày`;
    if (remH > 0) res += ` ${remH} giờ`;
    if (m > 0) res += ` ${m} phút`;
    return res;
  }
  
  return h > 0 ? (m > 0 ? `${h} giờ ${m} phút` : `${h} giờ`) : `${m} phút`;
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
    const res = await api.get('/doctors');
    const allDoctors = res.data || [];
    
    const currentDoc = allDoctors.find(d => d.id === props.currentDoctorId);
    if (currentDoc && currentDoc.role) {
      doctors.value = allDoctors.filter(d => d.id !== props.currentDoctorId && d.role === currentDoc.role);
    } else {
      doctors.value = allDoctors.filter(d => d.id !== props.currentDoctorId);
    }
  } catch (error) {
    console.error('Failed to load doctors', error);
  }
});

const submitRequest = async () => {
  try {
    isSubmitting.value = true;
    
    if (form.value.isAllDay) {
      form.value.startHour = '00:00';
      form.value.endHour = '23:59';
    }

    const startDateTime = new Date(`${form.value.startDate}T${form.value.startHour}:00`).toISOString();
    const endDateTime = new Date(`${form.value.endDate}T${form.value.endHour}:00`).toISOString();
    
    await scheduleExceptionService.createException({
      doctorId: props.currentDoctorId,
      type: form.value.type,
      startDate: startDateTime,
      endDate: endDateTime,
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
