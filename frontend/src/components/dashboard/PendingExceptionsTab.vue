<template>
  <div class="pending-exceptions-tab bg-white border rounded p-4 mb-4">
    <h5 class="fw-bold text-dark mb-4 border-bottom pb-2">
      <i class="bi bi-inbox text-warning me-2"></i> Yêu Cầu Chờ Duyệt (Xin Nghỉ / Đổi Ca)
    </h5>

    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="exceptions.length === 0" class="text-center py-5 text-muted">
      <i class="bi bi-check-circle text-success fs-1 mb-3 d-block opacity-50"></i>
      <p class="mb-0">Hiện tại không có yêu cầu nào chờ duyệt.</p>
    </div>

    <div v-else class="d-flex flex-column gap-3">
      <div v-for="req in exceptions" :key="req.id" class="card shadow-sm border-0 bg-light">
        <div class="card-body d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-3">
          
          <div>
            <div class="d-flex align-items-center gap-2 mb-2">
              <span class="badge rounded-pill" :class="req.type === 'TimeOff' ? 'bg-danger' : 'bg-primary'">
                {{ req.type === 'TimeOff' ? 'Nghỉ Phép' : 'Đổi Ca' }}
              </span>
              <span class="fw-bold text-dark">Mã Bác sĩ: {{ req.doctorId }}</span>
            </div>
            
            <div class="text-muted small">
              <div class="mb-1">
                <i class="bi bi-calendar-range text-secondary me-1"></i> 
                Từ <strong>{{ formatDate(req.startDate) }}</strong> đến <strong>{{ formatDate(req.endDate) }}</strong>
              </div>
              <div v-if="req.type === 'ShiftSwap'" class="mb-1">
                <i class="bi bi-person-up text-secondary me-1"></i> 
                <strong>Bác sĩ trực thay:</strong> {{ req.substituteDoctorId }}
              </div>
              <div v-if="req.reason">
                <i class="bi bi-chat-left-text text-secondary me-1"></i> 
                <strong>Lý do:</strong> <span class="fst-italic">{{ req.reason }}</span>
              </div>
            </div>
          </div>

          <div class="d-flex gap-2 mt-3 mt-md-0">
            <button @click="processRequest(req.id, 'Approved')" class="btn btn-success btn-sm px-3 rounded-pill shadow-sm fw-bold">
              <i class="bi bi-check-lg"></i> Duyệt
            </button>
            <button @click="processRequest(req.id, 'Rejected')" class="btn btn-outline-danger btn-sm px-3 rounded-pill shadow-sm fw-bold">
              <i class="bi bi-x-lg"></i> Từ chối
            </button>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { scheduleExceptionService } from '../../services/scheduleException.service';
import Swal from 'sweetalert2';

const exceptions = ref([]);
const loading = ref(false);

const loadPending = async () => {
  try {
    loading.value = true;
    const data = await scheduleExceptionService.getPendingExceptions();
    exceptions.value = data || [];
  } catch (error) {
    console.error('Failed to load pending exceptions', error);
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  const d = new Date(dateString);
  const pad = (n) => n.toString().padStart(2, '0');
  return `${pad(d.getDate())}/${pad(d.getMonth()+1)}/${d.getFullYear()}`;
};

const processRequest = async (id, status) => {
  try {
    const actionText = status === 'Approved' ? 'duyệt' : 'từ chối';
    const confirm = await Swal.fire({
      title: `Xác nhận`,
      text: `Bạn có chắc muốn ${actionText} yêu cầu này?`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: status === 'Approved' ? '#198754' : '#dc3545',
      cancelButtonColor: '#6c757d',
      confirmButtonText: `Đồng ý ${actionText}`,
      cancelButtonText: 'Đóng'
    });

    if (confirm.isConfirmed) {
      await scheduleExceptionService.approveException(id, { status });
      Swal.fire({
        icon: 'success',
        title: 'Thành công',
        text: `Yêu cầu đã được ${actionText}.`,
        confirmButtonColor: '#ffc107'
      });
      loadPending();
    }
  } catch (error) {
    Swal.fire({
      icon: 'error',
      title: 'Lỗi',
      text: error.response?.data?.message || `Không thể ${actionText} yêu cầu`,
      confirmButtonColor: '#dc3545'
    });
  }
};

onMounted(() => {
  loadPending();
});
</script>
