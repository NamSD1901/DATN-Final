<template>
  <div class="modal-overlay zalo-modal-overlay" @click.self="$emit('cancel')">
    <div class="glass-modal-card animate-slide-up" style="max-width: 700px;">
      <div class="glass-modal-header bg-warning bg-opacity-25 border-bottom border-light">
        <h5 class="modal-title fw-bold text-dark-gold mb-0">
          <i class="bi bi-diagram-3-fill me-2"></i> {{ isEditMode ? 'Cập Nhật Mẫu Lịch Trực' : 'Tạo Mẫu Lịch Trực' }}
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('cancel')"></button>
      </div>
      
      <div class="glass-modal-body text-start">
        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Tên Mẫu Lịch <span class="text-danger">*</span></label>
          <input v-model="profileName" type="text" class="form-control glass-input fw-bold px-3 py-2" placeholder="VD: Ca Sáng T2-T6" />
        </div>

        <div class="mb-4">
          <label class="form-label small fw-bold text-muted mb-1">Mô tả (Tùy chọn)</label>
          <textarea v-model="profileDescription" class="form-control glass-input px-3 py-2" rows="2" placeholder="Ghi chú thêm..."></textarea>
        </div>

        <h6 class="fw-bold text-dark mb-3 border-bottom pb-2 d-flex align-items-center">
          <div class="icon-circle bg-warning bg-opacity-10 text-dark-gold d-flex justify-content-center align-items-center rounded-circle me-2" style="width: 32px; height: 32px;">
            <i class="bi bi-calendar-week fs-6"></i>
          </div>
          Chi tiết ca trong tuần
        </h6>
        
        <div class="d-flex flex-column gap-2 mb-2">
          <div v-for="day in 7" :key="day" class="glass-shift-item d-flex flex-wrap align-items-center p-2 rounded-4 shadow-sm position-relative overflow-hidden">
            <div class="fw-bold text-dark d-flex align-items-center ps-2 position-relative z-1" style="width: 100px;">
              {{ getDayName(day - 1) }}
            </div>
            
            <div class="d-flex align-items-center flex-grow-1 flex-wrap gap-2 position-relative z-1">
              <div class="form-check form-switch mb-0 d-flex align-items-center ms-2" style="min-width: 70px;">
                <input class="form-check-input glass-switch" type="checkbox" role="switch" v-model="shifts[day - 1].isDayOff" :id="'switch_'+day">
                <label class="form-check-label small fw-bold ms-2" :class="shifts[day - 1].isDayOff ? 'text-danger' : 'text-success'" :for="'switch_'+day">
                  {{ shifts[day - 1].isDayOff ? 'Nghỉ' : 'Trực' }}
                </label>
              </div>
              
              <template v-if="!shifts[day - 1].isDayOff">
                <div class="d-flex align-items-center ms-auto gap-2 pe-2">
                  <input type="time" v-model="shifts[day - 1].startTime" class="form-control form-control-sm glass-input text-center fw-bold" style="width: 110px;" />
                  <span class="text-muted small fw-bold">đến</span>
                  <input type="time" v-model="shifts[day - 1].endTime" class="form-control form-control-sm glass-input text-center fw-bold" style="width: 110px;" />
                </div>
              </template>
              <template v-else>
                <span class="text-muted small fst-italic ms-auto pe-2">Không có ca trực ngày này</span>
              </template>
            </div>
          </div>
        </div>

        <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end gap-2">
          <button type="button" class="btn btn-light rounded-pill px-4 glass-btn text-dark fw-bold shadow-sm" @click="$emit('cancel')">Hủy</button>
          <button type="button" @click="saveProfile" :disabled="isSaving || !profileName" class="btn btn-premium rounded-pill px-4 shadow-sm">
            <span v-if="isSaving" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            {{ isEditMode ? 'Lưu Cập Nhật' : 'Lưu Mẫu Lịch' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { scheduleProfileService } from '../../services/scheduleProfile.service';
import Swal from 'sweetalert2';

const props = defineProps({
  editProfileId: {
    type: Number,
    default: null
  }
});

const emit = defineEmits(['saved', 'cancel']);

const isEditMode = computed(() => !!props.editProfileId);
const profileName = ref('');
const profileDescription = ref('');
const isSaving = ref(false);
const isLoading = ref(false);

const shifts = ref(Array.from({ length: 7 }, (_, i) => ({
  dayOfWeek: i,
  startTime: '08:00',
  endTime: '17:00',
  isDayOff: i === 0 || i === 6 // Sunday and Saturday default off
})));

onMounted(async () => {
  if (isEditMode.value) {
    isLoading.value = true;
    try {
      const profile = await scheduleProfileService.getProfile(props.editProfileId);
      profileName.value = profile.name;
      profileDescription.value = profile.description || '';
      
      // Map shifts
      profile.shifts.forEach(s => {
        const index = shifts.value.findIndex(sh => sh.dayOfWeek === s.dayOfWeek);
        if (index !== -1) {
          shifts.value[index].startTime = s.startTime.substring(0, 5);
          shifts.value[index].endTime = s.endTime.substring(0, 5);
          shifts.value[index].isDayOff = s.isDayOff;
        }
      });
    } catch (err) {
      console.error(err);
      Swal.fire('Lỗi', 'Không tải được dữ liệu mẫu lịch.', 'error');
    } finally {
      isLoading.value = false;
    }
  }
});

const getDayName = (dayIndex) => {
  const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
  return days[dayIndex];
};

const saveProfile = async () => {
  if (!profileName.value) return;
  
  try {
    isSaving.value = true;
    
    // Format times for backend (HH:mm:ss)
    const formattedShifts = shifts.value.map(s => ({
      dayOfWeek: s.dayOfWeek,
      isDayOff: s.isDayOff,
      startTime: s.startTime.length === 5 ? `${s.startTime}:00` : s.startTime,
      endTime: s.endTime.length === 5 ? `${s.endTime}:00` : s.endTime
    }));

    const payload = {
      name: profileName.value,
      description: profileDescription.value,
      shifts: formattedShifts
    };

    if (isEditMode.value) {
      await scheduleProfileService.updateProfile(props.editProfileId, payload);
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Đã cập nhật mẫu lịch trực.',
        confirmButtonColor: '#0d6efd'
      });
    } else {
      await scheduleProfileService.createProfile(payload);
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: 'Đã tạo mẫu lịch trực mới.',
        confirmButtonColor: '#0d6efd'
      });
    }
    
    emit('saved');
  } catch (error) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: error.response?.data?.message || 'Không thể tạo mẫu lịch',
      confirmButtonColor: '#dc3545'
    });
  } finally {
    isSaving.value = false;
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
  padding: 1.2rem;
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
.glass-input {
  background: rgba(255, 255, 255, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(5px);
  border-radius: 10px;
  transition: all 0.3s ease;
}
.glass-input:focus {
  background: rgba(255, 255, 255, 0.9);
  border-color: #ffc107;
  box-shadow: 0 0 0 0.25rem rgba(255, 193, 7, 0.25);
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
.glass-shift-item {
  background: rgba(255, 255, 255, 0.6);
  border: 1px solid rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(5px);
  transition: all 0.2s ease;
}
.glass-shift-item:hover {
  background: rgba(255, 255, 255, 0.9);
  border-color: #ffc107;
}
.glass-switch:checked {
  background-color: #dc3545;
  border-color: #dc3545;
}
.glass-switch:not(:checked) {
  background-color: #198754;
  border-color: #198754;
}
/* Animations */
.animate-slide-up { animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
