<template>
  <div class="settings-admin-tab container-fluid p-0">
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="mb-4">
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-clock-fill text-warning me-2"></i>Cấu hình Khung giờ làm việc</h4>
        <p class="text-muted small mb-0">Cấu hình thời gian làm việc hàng ngày, độ dài các ca khám và sức chứa tối đa mỗi khung giờ</p>
      </div>

      <div v-if="loading" class="text-center py-5">
        <div class="spinner-border text-warning me-2"></div>
        <span class="text-muted">Đang tải cấu hình hiện tại...</span>
      </div>

      <form v-else @submit.prevent="saveConfig" class="max-w-600">
        <div class="row g-3">
          <div class="col-md-6">
            <label class="form-label text-muted small fw-bold">Giờ bắt đầu làm việc *</label>
            <input type="time" v-model="config.startTime" class="form-control input-premium" required />
          </div>
          <div class="col-md-6">
            <label class="form-label text-muted small fw-bold">Giờ kết thúc làm việc *</label>
            <input type="time" v-model="config.endTime" class="form-control input-premium" required />
          </div>
          <div class="col-md-6">
            <label class="form-label text-muted small fw-bold">Độ dài mỗi ca khám (phút) *</label>
            <input type="number" v-model="config.durationMinutes" class="form-control input-premium" required min="10" max="180" />
          </div>
          <div class="col-md-6">
            <label class="form-label text-muted small fw-bold">Số lượng thú cưng tối đa mỗi Slot *</label>
            <input type="number" v-model="config.maxAppointmentsPerSlot" class="form-control input-premium" required min="1" max="20" />
          </div>
        </div>

        <div class="mt-4 pt-3 border-top text-end">
          <button type="submit" class="btn btn-premium rounded-pill px-5">
            <i class="bi bi-save-fill me-2"></i> Lưu Cấu Hình
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../services/api';

const loading = ref(false);
const config = ref({
  startTime: '08:00:00',
  endTime: '17:00:00',
  durationMinutes: 30,
  maxAppointmentsPerSlot: 3
});

const loadConfig = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/slots/config');
    if (res.data) {
      config.value = {
        startTime: res.data.startTime || '08:00:00',
        endTime: res.data.endTime || '17:00:00',
        durationMinutes: res.data.durationMinutes || 30,
        maxAppointmentsPerSlot: res.data.maxAppointmentsPerSlot || 3
      };
    }
  } catch (err) {
    console.error('Lỗi khi tải cấu hình slot:', err);
  } finally {
    loading.value = false;
  }
};

const saveConfig = async () => {
  try {
    const res = await api.put('/admin/slots/config', config.value);
    if (res.data.success) {
      alert('Cập nhật cấu hình khung giờ làm việc thành công!');
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi lưu cấu hình.');
  }
};

onMounted(() => {
  loadConfig();
});
</script>

<style scoped>
.max-w-600 {
  max-width: 600px;
}
.input-premium {
  border: 1px solid #ffeed1;
  transition: all 0.3s ease;
}
.input-premium:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.25rem rgba(245, 158, 11, 0.15);
}
</style>
