<template>
  <div v-if="show" class="zalo-modal-overlay" @click.self="closeModal">
    <div class="zalo-modal-card modal-lg max-w-700 bg-light" style="background: rgba(255, 255, 255, 0.85) !important; backdrop-filter: blur(12px);">
      <div class="zalo-modal-header bg-primary text-white d-flex justify-content-between align-items-center px-4 py-3 border-0">
        <h5 class="modal-title fw-bold mb-0">
          <i class="bi bi-person-hearts me-2"></i> Điều Phối Bác Sĩ
        </h5>
        <button class="modal-close text-white border-0 bg-transparent" @click="closeModal"><i class="bi bi-x-lg fs-5"></i></button>
      </div>
      <div class="zalo-modal-body text-start p-4">
        <div class="card border-0 shadow-sm rounded-4 p-3 mb-4 bg-white bg-opacity-75">
          <div class="d-flex justify-content-between align-items-center">
             <div>
               <h6 class="fw-bold mb-1">Ca khám: <span class="text-primary">{{ time }}</span> - {{ petName }}</h6>
               <span class="text-muted small">Bác sĩ hiện tại: <span class="fw-bold">Bs. {{ currentDoctorName.replace(/^(Bs\.|BS\.|Bs|BS)\s*/i, '') }}</span></span>
             </div>
             <span class="badge bg-warning text-dark px-3 py-2 rounded-pill">Đang điều phối</span>
          </div>
        </div>

        <h6 class="fw-bold mb-3 text-dark"><i class="bi bi-list-stars text-warning me-2"></i>Danh sách bác sĩ đề xuất</h6>
        
        <div style="min-height: 160px;">
          <transition name="fade" mode="out-in">
            <div v-if="store.isLoadingDoctors" key="loading" class="d-flex flex-column align-items-center justify-content-center h-100 py-5">
              <div class="spinner-border text-primary" style="width: 2.5rem; height: 2.5rem;"></div>
              <div class="mt-3 fw-bold text-muted small">Đang phân tích và chấm điểm bác sĩ...</div>
            </div>
            
            <div v-else-if="filteredEligibleDoctors.length === 0" key="empty" class="alert alert-warning border-0 rounded-4 mt-2 shadow-sm">
              <i class="bi bi-exclamation-triangle-fill me-2"></i> Không tìm thấy bác sĩ nào khác phù hợp hoặc đang trực.
            </div>

            <div v-else key="list" class="row g-3 mt-1">
              <div class="col-md-6" v-for="(doc, index) in filteredEligibleDoctors" :key="doc.doctorId">
                <label 
                  class="card h-100 border rounded-4 p-3 cursor-pointer transition-all position-relative mb-0 overflow-visible"
                  :class="selectedDoctorId === doc.doctorId ? 'border-primary bg-primary bg-opacity-10 shadow-sm' : 'border-light bg-white hover-shadow'"
                  @click="selectedDoctorId = doc.doctorId"
                >
                  <!-- Badge for the best match -->
                  <div v-if="index === 0" class="position-absolute top-0 end-0 translate-middle-y me-3 z-1">
                     <span class="badge bg-danger rounded-pill px-3 py-1 shadow-sm"><i class="bi bi-fire me-1"></i>Đề xuất hàng đầu</span>
                  </div>

                  <div class="d-flex align-items-start gap-3">
                    <input type="radio" class="form-check-input mt-1 fs-5 flex-shrink-0" name="doctorSelect" :value="doc.doctorId" v-model="selectedDoctorId">
                    <div class="flex-grow-1">
                      <div class="d-flex flex-wrap justify-content-between align-items-center mb-2 gap-1">
                        <h6 class="mb-0 fw-bold" :class="selectedDoctorId === doc.doctorId ? 'text-primary' : 'text-dark'">Bs. {{ doc.fullName.replace(/^(Bs\.|BS\.|Bs|BS)\s*/i, '') }}</h6>
                        <span class="badge bg-success bg-opacity-10 text-success rounded-pill px-2 py-1">Điểm: {{ doc.totalScore }}</span>
                      </div>
                      <div class="d-flex flex-wrap gap-1 mt-1">
                        <span v-for="(reason, rIdx) in doc.tags" :key="rIdx" class="badge bg-secondary bg-opacity-10 text-secondary rounded-pill px-2 py-1" style="font-size: 0.7rem;">
                          <i v-if="reason.includes('sao')" class="bi bi-star-fill text-warning me-1"></i>
                          <i v-else class="bi bi-check-circle-fill text-success me-1"></i> 
                          {{ reason }}
                        </span>
                      </div>
                    </div>
                  </div>
                </label>
              </div>
            </div>
          </transition>
        </div>

        <div class="mt-4 form-group">
          <label class="form-label fw-bold small text-muted">Lý do đổi bác sĩ (Bắt buộc) <span class="text-danger">*</span></label>
          <textarea v-model="reason" class="form-control rounded-3 border-light bg-white bg-opacity-75 shadow-sm" rows="2" placeholder="VD: Khách hàng yêu cầu đổi bác sĩ..."></textarea>
        </div>

      </div>
      <div class="zalo-modal-footer bg-white border-top border-light p-3 d-flex justify-content-end gap-2" style="background: rgba(255, 255, 255, 0.6);">
        <button type="button" class="btn btn-light rounded-pill px-4 fw-bold" @click="closeModal" :disabled="store.isChangingDoctor">Hủy</button>
        <button type="button" class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm d-flex align-items-center gap-2" @click="confirmChange" :disabled="!selectedDoctorId || !reason || store.isChangingDoctor">
          <span v-if="store.isChangingDoctor" class="spinner-border spinner-border-sm"></span>
          <i v-else class="bi bi-check-circle"></i>
          Xác nhận đổi
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useAppointmentStore } from '../../stores/appointment.store';
import Swal from 'sweetalert2';

const props = defineProps<{
  show: boolean;
  appointmentId: number;
  time: string;
  petName: string;
  currentDoctorName: string;
  currentDoctorId: string;
}>();

const emit = defineEmits(['update:show', 'success']);
const store = useAppointmentStore();

const selectedDoctorId = ref('');
const reason = ref('');

const filteredEligibleDoctors = computed(() => {
  if (!props.currentDoctorId) return store.eligibleDoctors;
  // So sánh chuỗi không phân biệt hoa thường để an toàn
  return store.eligibleDoctors.filter(doc => doc.doctorId?.toLowerCase() !== props.currentDoctorId?.toLowerCase());
});

watch(() => props.show, async (newVal) => {
  if (newVal && props.appointmentId) {
    selectedDoctorId.value = '';
    reason.value = '';
    await store.fetchEligibleDoctors(props.appointmentId);
    // Auto select the top match if available
    if (filteredEligibleDoctors.value.length > 0) {
      selectedDoctorId.value = filteredEligibleDoctors.value[0].doctorId;
    }
  }
});

const closeModal = () => {
  if (store.isChangingDoctor) return;
  emit('update:show', false);
};

const confirmChange = async () => {
  if (!selectedDoctorId.value || !reason.value) return;

  try {
    const success = await store.changeDoctor(props.appointmentId, {
      newDoctorId: selectedDoctorId.value,
      reason: reason.value,
      force: false
    });

    if (success) {
      emit('success');
      closeModal();
    }
  } catch (err: any) {
    // Nếu store quăng lỗi ra, tức là do trùng lịch hoặc bác sĩ nghỉ phép
    const msg = err.response?.data?.message || err.message;
    const result = await Swal.fire({
      title: 'Cảnh báo Bận rộn',
      text: msg + ' Bạn có chắc chắn muốn ghi đè (Force Bypass) để nhét thêm lịch hẹn này không?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Đồng ý ghi đè',
      cancelButtonText: 'Hủy bỏ'
    });

    if (result.isConfirmed) {
      const forceSuccess = await store.changeDoctor(props.appointmentId, {
        newDoctorId: selectedDoctorId.value,
        reason: reason.value + ' (Đã ghi đè thủ công)',
        force: true
      });
      if (forceSuccess) {
        emit('success');
        closeModal();
      }
    }
  }
};
</script>

<style scoped>
.modal-lg {
  max-width: 750px !important;
  width: 95% !important;
}
.hover-shadow:hover {
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.05) !important;
  border-color: #dee2e6 !important;
}
.cursor-pointer {
  cursor: pointer;
}
.transition-all {
  transition: all 0.2s ease-in-out;
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.25s ease, transform 0.25s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(5px);
}
</style>
