<template>
  <div class="schedules-admin-tab container-fluid p-0 animate-fade-in">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-calendar-event-fill text-warning me-2"></i>Quản lý Lịch Làm Việc & Nghỉ</h4>
        <p class="text-muted small mb-0">Thiết lập ca trực và thời gian nghỉ của bác sĩ với giao diện Glassmorphism</p>
      </div>
      <div class="d-flex gap-2">
        <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateScheduleModal">
          <i class="bi bi-calendar-plus-fill me-2"></i> Phân Ca Trực
        </button>
        <button class="btn btn-danger px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateBlockModal">
          <i class="bi bi-calendar-x-fill me-2"></i> Thêm Lịch Nghỉ
        </button>
      </div>
    </div>

    <!-- Filter & Tabs -->
    <div class="glass-card p-4 mb-4">
      <div class="row g-3 align-items-center mb-3">
        <div class="col-md-4">
          <label class="form-label text-muted small fw-bold">Lọc theo Bác sĩ</label>
          <select v-model="filterDoctorId" class="form-select glass-input rounded-pill px-3">
            <option value="all">Tất cả bác sĩ</option>
            <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
              👨‍⚕️ {{ doc.fullName }}
            </option>
          </select>
        </div>
        <div class="col-md-8 text-md-end pt-4">
          <ul class="nav nav-pills justify-content-md-end custom-tabs">
            <li class="nav-item">
              <a class="nav-link" :class="{ active: activeTab === 'schedules' }" @click.prevent="activeTab = 'schedules'" href="#">Ca Trực</a>
            </li>
            <li class="nav-item">
              <a class="nav-link" :class="{ active: activeTab === 'blocks' }" @click.prevent="activeTab = 'blocks'" href="#">Lịch Nghỉ/Bận</a>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <!-- Tab Ca Trực -->
    <div v-if="activeTab === 'schedules'" class="glass-card p-0 overflow-hidden">
      <div class="table-responsive">
        <table class="table table-hover align-middle mb-0 glass-table">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Bác sĩ</th>
              <th>Ngày trực</th>
              <th>Thời gian ca trực</th>
              <th>Ghi chú</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="scheduleStore.isLoading" class="text-center">
              <td colspan="5" class="py-5">
                <div class="spinner-border text-warning spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải danh sách ca trực...</span>
              </td>
            </tr>
            <tr v-else-if="filteredSchedules.length === 0" class="text-center">
              <td colspan="5" class="py-5 text-muted">
                <i class="bi bi-calendar-check fs-2 mb-2 d-block"></i>
                Chưa có ca trực nào.
              </td>
            </tr>
            <tr v-for="sched in filteredSchedules" :key="sched.id" v-else>
              <td class="ps-4 fw-bold text-dark">👨‍⚕️ {{ sched.doctorName }}</td>
              <td>{{ formatDate(sched.workDate) }}</td>
              <td>
                <span class="badge glass-badge-warning px-3 py-1.5 rounded-pill fw-bold">
                  <i class="bi bi-clock-fill me-1"></i>{{ sched.startTime }} - {{ sched.endTime }}
                </span>
              </td>
              <td><span class="text-muted small">{{ sched.notes || '-' }}</span></td>
              <td class="text-center">
                <button class="btn btn-sm glass-btn-outline-warning rounded-pill px-3 me-2" @click="openEditScheduleModal(sched)">
                  <i class="bi bi-pencil-fill me-1"></i>Sửa
                </button>
                <button class="btn btn-sm glass-btn-outline-danger rounded-pill px-3" @click="handleDeleteSchedule(sched.id)">
                  <i class="bi bi-trash-fill me-1"></i>Xoá
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Tab Lịch Nghỉ -->
    <div v-if="activeTab === 'blocks'" class="glass-card p-0 overflow-hidden">
      <div class="table-responsive">
        <table class="table table-hover align-middle mb-0 glass-table">
          <thead class="bg-light-danger">
            <tr>
              <th class="ps-4">Bác sĩ</th>
              <th>Loại Nghỉ/Bận</th>
              <th>Bắt đầu</th>
              <th>Kết thúc</th>
              <th>Lý do</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="blockTimeStore.isLoading" class="text-center">
              <td colspan="6" class="py-5">
                <div class="spinner-border text-danger spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải lịch nghỉ...</span>
              </td>
            </tr>
            <tr v-else-if="filteredBlockTimes.length === 0" class="text-center">
              <td colspan="6" class="py-5 text-muted">
                <i class="bi bi-calendar-x fs-2 mb-2 d-block"></i>
                Không có lịch nghỉ/bận nào.
              </td>
            </tr>
            <tr v-for="block in filteredBlockTimes" :key="block.id" v-else>
              <td class="ps-4 fw-bold text-dark">👨‍⚕️ {{ block.doctorName }}</td>
              <td>
                <span class="badge glass-badge-danger px-3 py-1.5 rounded-pill fw-bold">
                  {{ block.blockType }}
                </span>
              </td>
              <td>{{ formatDateTime(block.startTime) }}</td>
              <td>{{ formatDateTime(block.endTime) }}</td>
              <td><span class="text-muted small">{{ block.reason || '-' }}</span></td>
              <td class="text-center">
                <button class="btn btn-sm glass-btn-outline-danger rounded-pill px-3" @click="handleDeleteBlock(block.id)">
                  <i class="bi bi-trash-fill me-1"></i>Hủy lịch nghỉ
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Schedule Modal -->
    <div v-if="showScheduleModal" class="zalo-modal-overlay" @click.self="showScheduleModal = false">
      <div class="glass-modal-card max-w-500 animate-slide-up">
        <div class="glass-modal-header bg-warning bg-opacity-25">
          <h5 class="modal-title fw-bold text-dark-gold">
            <i class="bi bi-calendar-plus-fill me-2"></i> {{ isEdit ? 'Cập Nhật Ca Trực' : 'Phân Ca Trực Mới' }}
          </h5>
          <button class="btn-close" @click="showScheduleModal = false"></button>
        </div>
        <div class="glass-modal-body text-start">
          <form @submit.prevent="submitScheduleForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Chọn Bác sĩ trực *</label>
              <select v-model="scheduleForm.doctorId" class="form-select glass-input" required :disabled="isEdit">
                <option value="">-- Chọn bác sĩ --</option>
                <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
                  👨‍⚕️ {{ doc.fullName }}
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Ngày trực *</label>
              <input type="date" v-model="scheduleForm.workDate" class="form-control glass-input" required :min="minDate" :disabled="isEdit" />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giờ bắt đầu *</label>
                <input type="time" v-model="scheduleForm.startTime" class="form-control glass-input" required />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giờ kết thúc *</label>
                <input type="time" v-model="scheduleForm.endTime" class="form-control glass-input" required />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Ghi chú ca trực</label>
              <input type="text" v-model="scheduleForm.notes" class="form-control glass-input" placeholder="Ví dụ: Ca trực thay thế..." />
            </div>

            <div class="mt-4 pt-3 text-end">
              <button type="button" class="btn btn-light rounded-pill px-4 me-2 glass-btn" @click="showScheduleModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-4 shadow-sm" :disabled="scheduleStore.isLoading">Lưu ca trực</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- BlockTime Modal -->
    <div v-if="showBlockModal" class="zalo-modal-overlay" @click.self="showBlockModal = false">
      <div class="glass-modal-card max-w-500 animate-slide-up">
        <div class="glass-modal-header bg-danger bg-opacity-25">
          <h5 class="modal-title fw-bold text-danger">
            <i class="bi bi-calendar-x-fill me-2"></i> Thêm Lịch Nghỉ/Bận
          </h5>
          <button class="btn-close" @click="showBlockModal = false"></button>
        </div>
        <div class="glass-modal-body text-start">
          <form @submit.prevent="submitBlockForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Chọn Bác sĩ *</label>
              <select v-model="blockForm.doctorId" class="form-select glass-input" required>
                <option value="">-- Chọn bác sĩ --</option>
                <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
                  👨‍⚕️ {{ doc.fullName }}
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Loại Nghỉ/Bận *</label>
              <select v-model.number="blockForm.blockType" class="form-select glass-input">
                <option :value="0">Nghỉ phép (Leave)</option>
                <option :value="3">Họp (Meeting)</option>
                <option :value="2">Nghỉ trưa (Break)</option>
                <option :value="9">Lý do khác</option>
              </select>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Bắt đầu *</label>
                <input type="datetime-local" v-model="blockForm.startTime" class="form-control glass-input" required />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Kết thúc *</label>
                <input type="datetime-local" v-model="blockForm.endTime" class="form-control glass-input" required />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Lý do cụ thể</label>
              <textarea v-model="blockForm.reason" class="form-control glass-input" rows="2" placeholder="Ví dụ: Nghỉ thai sản..."></textarea>
            </div>

            <div class="mt-4 pt-3 text-end">
              <button type="button" class="btn btn-light rounded-pill px-4 me-2 glass-btn" @click="showBlockModal = false">Hủy</button>
              <button type="submit" class="btn btn-danger rounded-pill px-4 shadow-sm" :disabled="blockTimeStore.isLoading">Xác nhận</button>
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
import { useDoctorScheduleStore } from '../../stores/doctorSchedule.store';
import { useBlockTimeStore } from '../../stores/blockTime.store';

const scheduleStore = useDoctorScheduleStore();
const blockTimeStore = useBlockTimeStore();

const activeTab = ref('schedules');
const doctorUsers = ref<any[]>([]);
const filterDoctorId = ref('all');

// Modals state
const showScheduleModal = ref(false);
const showBlockModal = ref(false);
const isEdit = ref(false);
const currentScheduleId = ref<number | null>(null);

const minDate = computed(() => {
  const today = new Date();
  return today.toISOString().split('T')[0];
});

// Forms
const scheduleForm = ref({
  doctorId: '',
  workDate: '',
  startTime: '08:00',
  endTime: '12:00',
  maxAppointments: 10,
  isAvailable: true,
  notes: ''
});

const blockForm = ref({
  doctorId: '',
  startTime: '',
  endTime: '',
  blockType: 0,
  reason: ''
});

// Computed properties
const filteredSchedules = computed(() => {
  if (filterDoctorId.value === 'all') return scheduleStore.schedules;
  return scheduleStore.schedules.filter(s => s.doctorId === filterDoctorId.value);
});

const filteredBlockTimes = computed(() => {
  if (filterDoctorId.value === 'all') return blockTimeStore.blockTimes;
  return blockTimeStore.blockTimes.filter(b => b.doctorId === filterDoctorId.value);
});

// Load init data
const loadData = async () => {
  await Promise.all([
    scheduleStore.fetchSchedules(),
    blockTimeStore.fetchBlockTimes(),
    loadDoctors()
  ]);
};

const loadDoctors = async () => {
  try {
    const res = await api.get('/admin/users');
    const allUsers = res.data || [];
    doctorUsers.value = allUsers.filter((u: any) => {
      const r = u.role?.toLowerCase() || '';
      return r === 'doctor' || r === 'clinical_doctor' || r === 'vaccination_doctor';
    });
  } catch (err) {
    console.error('Lỗi tải danh sách bác sĩ:', err);
  }
};

// Handlers for Schedule
const openCreateScheduleModal = () => {
  isEdit.value = false;
  currentScheduleId.value = null;
  scheduleForm.value = {
    doctorId: doctorUsers.value[0]?.id || '',
    workDate: minDate.value,
    startTime: '08:00',
    endTime: '12:00',
    maxAppointments: 10,
    isAvailable: true,
    notes: ''
  };
  showScheduleModal.value = true;
};

const openEditScheduleModal = (sched: any) => {
  isEdit.value = true;
  currentScheduleId.value = sched.id;
  scheduleForm.value = {
    doctorId: sched.doctorId,
    workDate: sched.workDate.split('T')[0],
    startTime: sched.startTime.substring(0, 5),
    endTime: sched.endTime.substring(0, 5),
    maxAppointments: sched.maxAppointments || 10,
    isAvailable: sched.isAvailable,
    notes: sched.notes || ''
  };
  showScheduleModal.value = true;
};

const submitScheduleForm = async () => {
  const payload = {
    ...scheduleForm.value,
    startTime: scheduleForm.value.startTime.length === 5 ? `${scheduleForm.value.startTime}:00` : scheduleForm.value.startTime,
    endTime: scheduleForm.value.endTime.length === 5 ? `${scheduleForm.value.endTime}:00` : scheduleForm.value.endTime,
    maxAppointments: scheduleForm.value.maxAppointments ? Number(scheduleForm.value.maxAppointments) : 10
  };

  let success = false;
  if (isEdit.value && currentScheduleId.value) {
    success = await scheduleStore.updateSchedule(currentScheduleId.value, payload);
  } else {
    success = await scheduleStore.createSchedule(payload as any);
  }

  if (success) {
    showScheduleModal.value = false;
    await scheduleStore.fetchSchedules();
  }
};

const handleDeleteSchedule = async (id: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá ca trực này không?')) return;
  const success = await scheduleStore.deleteSchedule(id);
  if (success) {
    await scheduleStore.fetchSchedules();
  }
};

// Handlers for BlockTime
const openCreateBlockModal = () => {
  blockForm.value = {
    doctorId: doctorUsers.value[0]?.id || '',
    startTime: '',
    endTime: '',
    blockType: 0,
    reason: ''
  };
  showBlockModal.value = true;
};

const submitBlockForm = async () => {
  const payload = {
    ...blockForm.value,
    startTime: new Date(blockForm.value.startTime).toISOString(),
    endTime: new Date(blockForm.value.endTime).toISOString()
  };
  
  const success = await blockTimeStore.createBlockTime(payload as any);
  if (success) {
    showBlockModal.value = false;
    await blockTimeStore.fetchBlockTimes();
  }
};

const handleDeleteBlock = async (id: string) => {
  if (!confirm('Bạn có chắc chắn muốn hủy lịch nghỉ này?')) return;
  const success = await blockTimeStore.deleteBlockTime(id);
  if (success) {
    await blockTimeStore.fetchBlockTimes();
  }
};

// Utils
const formatDate = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateTime = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
};

onMounted(() => {
  loadData();
});
</script>

<style scoped>
/* Glassmorphism Styles */
.glass-card {
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.05);
}

.glass-modal-card {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: 20px;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.1);
  overflow: hidden;
  width: 100%;
}

.glass-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid rgba(255, 255, 255, 0.4);
}

.glass-modal-body {
  padding: 2rem 1.5rem;
}

.glass-input {
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(5px);
  border-radius: 10px;
  transition: all 0.3s ease;
}

.glass-input:focus {
  background: rgba(255, 255, 255, 0.8);
  border-color: var(--primary-color, #ffc107);
  box-shadow: 0 0 0 0.25rem rgba(255, 193, 7, 0.25);
}

.glass-btn {
  background: rgba(255, 255, 255, 0.4);
  backdrop-filter: blur(5px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  transition: all 0.3s ease;
}

.glass-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.glass-table {
  background: transparent;
}

.glass-table th {
  border-bottom: 2px solid rgba(0, 0, 0, 0.05);
  font-weight: 600;
  color: #555;
}

.glass-table td {
  border-bottom: 1px solid rgba(0, 0, 0, 0.03);
  background: rgba(255, 255, 255, 0.3);
}

.glass-badge-warning {
  background: rgba(255, 193, 7, 0.2);
  color: #b25e00;
  border: 1px solid rgba(255, 193, 7, 0.4);
}

.glass-badge-success {
  background: rgba(25, 135, 84, 0.2);
  color: #146c43;
  border: 1px solid rgba(25, 135, 84, 0.4);
}

.glass-badge-danger {
  background: rgba(220, 53, 69, 0.2);
  color: #b02a37;
  border: 1px solid rgba(220, 53, 69, 0.4);
}

.glass-badge-secondary {
  background: rgba(108, 117, 125, 0.2);
  color: #5c636a;
  border: 1px solid rgba(108, 117, 125, 0.4);
}

.glass-btn-outline-warning {
  color: #b25e00;
  background: rgba(255, 193, 7, 0.1);
  border: 1px solid rgba(255, 193, 7, 0.5);
}

.glass-btn-outline-warning:hover {
  background: rgba(255, 193, 7, 0.3);
  color: #b25e00;
}

.glass-btn-outline-danger {
  color: #b02a37;
  background: rgba(220, 53, 69, 0.1);
  border: 1px solid rgba(220, 53, 69, 0.5);
}

.glass-btn-outline-danger:hover {
  background: rgba(220, 53, 69, 0.3);
  color: #b02a37;
}

.custom-tabs .nav-link {
  color: #6c757d;
  font-weight: 600;
  border-radius: 50px;
  padding: 0.5rem 1.25rem;
  margin-left: 0.5rem;
  transition: all 0.3s ease;
}

.custom-tabs .nav-link:hover {
  background: rgba(0, 0, 0, 0.05);
}

.custom-tabs .nav-link.active {
  background: #ffc107;
  color: #212529;
  box-shadow: 0 4px 10px rgba(255, 193, 7, 0.3);
}

.bg-light-gold {
  background-color: rgba(253, 250, 240, 0.8) !important;
}

.bg-light-danger {
  background-color: rgba(255, 240, 240, 0.8) !important;
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
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(8px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.max-w-500 {
  max-width: 500px;
}

.animate-fade-in {
  animation: fadeIn 0.4s ease-out forwards;
}

.animate-slide-up {
  animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
