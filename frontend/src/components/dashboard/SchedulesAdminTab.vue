<template>
  <div class="schedules-admin-tab container-fluid p-0 animate-fade-in">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-calendar-event-fill text-warning me-2"></i>Quản lý Ca trực Bác sĩ</h4>
        <p class="text-muted small mb-0">Thiết lập ca trực làm việc hàng ngày của bác sĩ thú y để khách hàng đặt lịch khám</p>
      </div>
      <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill me-2"></i> Phân Ca Trực Mới
      </button>
    </div>

    <!-- Filters & Table -->
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="row g-3 mb-3 align-items-center">
        <div class="col-md-4">
          <label class="form-label text-muted small fw-bold">Lọc theo Bác sĩ</label>
          <select v-model="filterDoctorId" class="form-select border-warning rounded-pill px-3">
            <option value="all">Tất cả bác sĩ</option>
            <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
              👨‍⚕️ {{ doc.fullName }}
            </option>
          </select>
        </div>
        <div class="col-md-8 text-md-end text-muted small pt-4">
          Tổng số ca trực trong tuần: <strong class="text-dark">{{ filteredSchedules.length }}</strong> ca trực
        </div>
      </div>

      <!-- Schedules Table -->
      <div class="table-responsive rounded-4 border overflow-hidden mt-3">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Bác sĩ</th>
              <th>Ngày trực</th>
              <th>Thời gian ca trực</th>
              <th>Max ca khám/slot</th>
              <th>Trạng thái trực</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="6" class="py-5">
                <div class="spinner-border text-warning spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải danh sách ca trực...</span>
              </td>
            </tr>
            <tr v-else-if="filteredSchedules.length === 0" class="text-center">
              <td colspan="6" class="py-5 text-muted">
                <i class="bi bi-calendar-x fs-2 mb-2 d-block"></i>
                Không có lịch trực nào được ghi nhận.
              </td>
            </tr>
            <tr v-for="sched in filteredSchedules" :key="sched.id" v-else>
              <td class="ps-4 fw-bold text-dark">
                👨‍⚕️ {{ sched.doctorName }}
              </td>
              <td>{{ formatDate(sched.workDate) }}</td>
              <td>
                <span class="badge bg-warning bg-opacity-10 text-dark-gold border border-warning border-opacity-20 px-3 py-1.5 rounded-pill fw-bold">
                  <i class="bi bi-clock-fill me-1"></i>{{ sched.startTime }} - {{ sched.endTime }}
                </span>
              </td>
              <td>{{ sched.maxAppointments || 10 }} ca</td>
              <td>
                <span :class="['badge rounded-pill px-3 py-1.5 fw-bold', sched.isAvailable ? 'bg-success bg-opacity-10 text-success' : 'bg-secondary bg-opacity-10 text-secondary']">
                  {{ sched.isAvailable ? 'Sẵn sàng' : 'Tạm ngưng' }}
                </span>
              </td>
              <td class="text-center">
                <button class="btn btn-sm btn-outline-warning rounded-pill px-3 me-2" @click="openEditModal(sched)">
                  <i class="bi bi-pencil-fill me-1"></i>Sửa
                </button>
                <button class="btn btn-sm btn-outline-danger rounded-pill px-3" @click="handleDelete(sched.id)">
                  <i class="bi bi-trash-fill me-1"></i>Xoá
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="showModal = false">
      <div class="zalo-modal-card max-w-500">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-calendar-plus-fill me-2"></i> {{ isEdit ? 'Cập Nhật Ca Trực' : 'Phân Ca Trực Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Chọn Bác sĩ trực *</label>
              <select v-model="form.doctorId" class="form-select border-warning" required :disabled="isEdit">
                <option value="">-- Chọn bác sĩ --</option>
                <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
                  👨‍⚕️ {{ doc.fullName }}
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Ngày trực *</label>
              <input type="date" v-model="form.workDate" class="form-control" required :min="minDate" />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giờ bắt đầu *</label>
                <input type="time" v-model="form.startTime" class="form-control" required />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giờ kết thúc *</label>
                <input type="time" v-model="form.endTime" class="form-control" required />
              </div>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Số ca khám tối đa/slot</label>
                <input type="number" v-model="form.maxAppointments" class="form-control" min="1" max="50" />
              </div>
              <div class="col-6 pt-4">
                <div class="form-check form-switch mt-2">
                  <input class="form-check-input" type="checkbox" role="switch" id="scheduleAvailable" v-model="form.isAvailable">
                  <label class="form-check-label text-muted small" for="scheduleAvailable">Cho phép đặt ca trực này</label>
                </div>
              </div>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-4">Lưu ca trực</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

const loading = ref(false);
const schedulesList = ref<any[]>([]);
const doctorUsers = ref<any[]>([]);
const filterDoctorId = ref('all');

const showModal = ref(false);
const isEdit = ref(false);
const currentScheduleId = ref<number | null>(null);

const minDate = computed(() => {
  const today = new Date();
  return today.toISOString().split('T')[0];
});

const form = ref({
  doctorId: '',
  workDate: '',
  startTime: '08:00',
  endTime: '12:00',
  maxAppointments: 10,
  isAvailable: true
});

const filteredSchedules = computed(() => {
  if (filterDoctorId.value === 'all') {
    return schedulesList.value;
  }
  return schedulesList.value.filter(s => s.doctorId === filterDoctorId.value);
});

const loadSchedules = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/schedules');
    schedulesList.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải danh sách ca trực:', err);
  } finally {
    loading.value = false;
  }
};

const loadDoctors = async () => {
  try {
    const res = await api.get('/admin/users');
    const allUsers = res.data || [];
    doctorUsers.value = allUsers.filter((u: any) => u.role?.toLowerCase() === 'doctor');
  } catch (err) {
    console.error('Lỗi tải danh sách bác sĩ:', err);
  }
};

const openCreateModal = () => {
  isEdit.value = false;
  currentScheduleId.value = null;
  form.value = {
    doctorId: doctorUsers.value[0]?.id || '',
    workDate: minDate.value,
    startTime: '08:00',
    endTime: '12:00',
    maxAppointments: 10,
    isAvailable: true
  };
  showModal.value = true;
};

const openEditModal = (sched: any) => {
  isEdit.value = true;
  currentScheduleId.value = sched.id;
  form.value = {
    doctorId: sched.doctorId,
    workDate: sched.workDate,
    startTime: sched.startTime,
    endTime: sched.endTime,
    maxAppointments: sched.maxAppointments || 10,
    isAvailable: sched.isAvailable
  };
  showModal.value = true;
};

const submitForm = async () => {
  try {
    const payload = {
      ...form.value,
      workDate: form.value.workDate
    };

    if (isEdit.value && currentScheduleId.value) {
      await api.put(`/admin/schedules/${currentScheduleId.value}`, payload);
      alert('Cập nhật ca trực thành công!');
    } else {
      await api.post('/admin/schedules', payload);
      alert('Phân ca trực thành công!');
    }
    showModal.value = false;
    await loadSchedules();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi lưu ca trực.');
  }
};

const handleDelete = async (id: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá ca trực này không?')) return;
  try {
    await api.delete(`/admin/schedules/${id}`);
    alert('Xoá ca trực thành công!');
    await loadSchedules();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá ca trực.');
  }
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

onMounted(() => {
  loadSchedules();
  loadDoctors();
});
</script>

<style scoped>
.bg-light-gold {
  background-color: #fdfaf0;
}
.text-dark-gold {
  color: #b25e00;
}
.zalo-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}
.zalo-modal-card {
  background: white;
  width: 100%;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
}
.max-w-500 {
  max-width: 500px;
}
.zalo-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.zalo-modal-body {
  padding: 2rem 1.5rem;
}
</style>
