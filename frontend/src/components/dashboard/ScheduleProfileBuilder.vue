<template>
  <div class="modal-overlay glass-overlay" @click.self="$emit('cancel')">
    <div class="modal-card glass-card">
      <div class="modal-header border-bottom px-4 py-3 bg-white bg-opacity-75 d-flex justify-content-between align-items-center w-100">
        <h5 class="fw-bold text-primary mb-0">
          <i class="bi bi-diagram-3-fill me-2 text-primary"></i> Tạo Mẫu Lịch Trực
        </h5>
        <button type="button" class="btn-close shadow-none m-0" aria-label="Close" @click="$emit('cancel')"></button>
      </div>
      
      <div class="modal-body p-4 bg-white bg-opacity-50 text-start">
        <div class="mb-3">
          <label class="form-label small fw-bold text-muted mb-1">Tên Mẫu Lịch <span class="text-danger">*</span></label>
          <input v-model="profileName" type="text" class="form-control form-control-sm border-primary-subtle fw-bold" placeholder="VD: Ca Sáng T2-T6" />
        </div>

        <div class="mb-4">
          <label class="form-label small fw-bold text-muted mb-1">Mô tả (Tùy chọn)</label>
          <textarea v-model="profileDescription" class="form-control form-control-sm border-primary-subtle" rows="2" placeholder="Ghi chú thêm..."></textarea>
        </div>

        <h6 class="fw-bold text-dark mb-3 border-bottom pb-2"><i class="bi bi-calendar-week me-2"></i>Chi tiết ca trong tuần</h6>
        
        <div class="d-flex flex-column gap-2 mb-2">
          <div v-for="day in 7" :key="day" class="d-flex flex-wrap align-items-center p-2 rounded border border-primary-subtle bg-white shadow-sm">
            <div class="fw-bold text-dark d-flex align-items-center" style="width: 100px;">
              {{ getDayName(day - 1) }}
            </div>
            
            <div class="d-flex align-items-center flex-grow-1 flex-wrap gap-2">
              <div class="form-check form-switch mb-0 d-flex align-items-center ms-2" style="min-width: 70px;">
                <input class="form-check-input" type="checkbox" role="switch" v-model="shifts[day - 1].isDayOff" :id="'switch_'+day">
                <label class="form-check-label small fw-bold ms-2" :class="shifts[day - 1].isDayOff ? 'text-danger' : 'text-success'" :for="'switch_'+day">
                  {{ shifts[day - 1].isDayOff ? 'Nghỉ' : 'Trực' }}
                </label>
              </div>
              
              <template v-if="!shifts[day - 1].isDayOff">
                <div class="d-flex align-items-center ms-auto gap-2">
                  <input type="time" v-model="shifts[day - 1].startTime" class="form-control form-control-sm border-primary-subtle text-center fw-bold" style="width: 110px;" />
                  <span class="text-muted small fw-bold">đến</span>
                  <input type="time" v-model="shifts[day - 1].endTime" class="form-control form-control-sm border-primary-subtle text-center fw-bold" style="width: 110px;" />
                </div>
              </template>
              <template v-else>
                <span class="text-muted small fst-italic ms-auto">Không có ca trực ngày này</span>
              </template>
            </div>
          </div>
        </div>

        <div class="mt-4 pt-3 border-top border-light d-flex justify-content-end gap-2">
          <button type="button" class="btn btn-sm btn-light fw-bold border px-4" @click="$emit('cancel')">Hủy</button>
          <button type="button" @click="saveProfile" :disabled="isSaving || !profileName" class="btn btn-sm btn-primary text-white fw-bold px-4 shadow-sm">
            <span v-if="isSaving" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            Lưu Mẫu Lịch
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { scheduleProfileService } from '../../services/scheduleProfile.service';
import Swal from 'sweetalert2';

const emit = defineEmits(['saved', 'cancel']);

const profileName = ref('');
const profileDescription = ref('');
const isSaving = ref(false);

const shifts = ref(Array.from({ length: 7 }, (_, i) => ({
  dayOfWeek: i,
  startTime: '08:00',
  endTime: '17:00',
  isDayOff: i === 0 || i === 6 // Sunday and Saturday default off
})));

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

    await scheduleProfileService.createProfile({
      name: profileName.value,
      description: profileDescription.value,
      shifts: formattedShifts
    });

    Swal.fire({
      icon: 'success',
      title: 'Thành công',
      text: 'Đã tạo mẫu lịch trực mới.',
      confirmButtonColor: '#0d6efd'
    });
    
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
  max-width: 650px;
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
.border-primary-subtle {
  border-color: #a3c4f3;
}
.border-primary-subtle:focus {
  border-color: #0d6efd;
  box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.15);
}
</style>
