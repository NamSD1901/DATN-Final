<template>
  <div class="schedules-admin-tab container-fluid p-0 animate-fade-in">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-calendar-event-fill text-warning me-2"></i>Quản lý Lịch Làm Việc & Nghỉ</h4>
        <p class="text-muted small mb-0">Thiết lập ca trực với giao diện Timeline Grid Glassmorphism Độc Bản</p>
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

    <!-- Filter & Week Navigator -->
    <div class="bg-white border rounded p-3 mb-4 d-flex flex-wrap align-items-center justify-content-between gap-3">
      <div class="d-flex align-items-center gap-3">
        <label class="form-label text-muted small fw-bold mb-0">Lọc theo Bác sĩ:</label>
        <select v-model="filterDoctorId" class="form-select glass-input rounded-pill px-3" style="width: 200px">
          <option value="all">Tất cả bác sĩ</option>
          <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
            👨‍⚕️ {{ doc.fullName }}
          </option>
        </select>
      </div>

      <!-- Week Navigator -->
      <div class="d-flex align-items-center bg-white bg-opacity-75 p-1 rounded-pill shadow-sm border border-light">
        <button class="btn btn-sm btn-light rounded-circle shadow-sm fw-bold text-dark" style="width: 32px; height: 32px" @click="prevWeek">
          <i class="bi bi-chevron-left"></i>
        </button>
        <div class="fw-bold px-3 text-dark text-uppercase" style="font-size: 0.85rem; letter-spacing: 0.5px">
          Tuần: {{ formatDateNum(currentWeekDays[0]) }} - {{ formatDateNum(currentWeekDays[6]) }}
        </div>
        <button class="btn btn-sm btn-light rounded-circle shadow-sm fw-bold text-dark" style="width: 32px; height: 32px" @click="nextWeek">
          <i class="bi bi-chevron-right"></i>
        </button>
        <button class="btn btn-sm btn-outline-warning rounded-pill px-3 ms-2 fw-bold" @click="goToday">Hôm nay</button>
      </div>
    </div>

    <!-- NATIVE VUE GRID -->
    <div class="schedule-grid bg-white border rounded mb-4 overflow-auto position-relative" style="max-height: 75vh;">
      <div v-if="loading" class="position-absolute w-100 h-100 d-flex justify-content-center align-items-center bg-white bg-opacity-50" style="z-index: 100;">
        <div class="spinner-border text-warning" style="width: 3rem; height: 3rem;"></div>
      </div>

      <div class="grid-container" style="min-width: 1000px;">
        <!-- Header Row -->
        <div class="grid-header d-flex bg-white bg-opacity-90 border-bottom border-light sticky-top" style="z-index: 20; backdrop-filter: blur(10px);">
          <div class="grid-cell doctor-col fw-bold text-dark text-uppercase small d-flex align-items-center justify-content-center bg-white bg-opacity-90 position-sticky start-0" style="z-index: 21;">
            Bác sĩ
          </div>
          <div class="grid-cell day-col text-center py-3" v-for="day in currentWeekDays" :key="day.toISOString()"
               :class="{'bg-warning bg-opacity-10': isToday(day)}">
            <div class="fw-bold text-dark" :class="{'text-warning': isToday(day)}">{{ formatDayName(day) }}</div>
            <div class="small text-muted fw-medium">{{ formatDateNum(day) }}</div>
          </div>
        </div>

        <!-- Body Rows (1 Row per Doctor) -->
        <div class="grid-body">
          <div v-if="filteredDoctors.length === 0" class="p-5 text-center text-muted">
            Không có bác sĩ nào phù hợp với bộ lọc.
          </div>
          <div class="grid-row d-flex border-bottom border-light" v-for="doc in filteredDoctors" :key="doc.id">
            <!-- Doctor Column -->
            <div class="grid-cell doctor-col d-flex flex-column align-items-center justify-content-center text-center p-3 bg-white bg-opacity-75 position-sticky start-0" style="z-index: 15; backdrop-filter: blur(5px);">
              <div class="avatar-circle shadow-sm fw-bold d-flex justify-content-center align-items-center rounded-circle mb-2" 
                   :style="{ backgroundColor: getDoctorColor(doc.id, 0.2), color: getDoctorColor(doc.id, 1), width: '45px', height: '45px', fontSize: '1.2rem' }">
                {{ doc.fullName.charAt(0) }}
              </div>
              <div class="small fw-bold text-dark" style="line-height: 1.2">{{ doc.fullName }}</div>
              <div class="text-muted" style="font-size: 0.7rem; margin-top: 2px">Bác sĩ thú y</div>
            </div>
            
            <!-- Day Columns -->
            <div class="grid-cell day-col p-2" v-for="day in currentWeekDays" :key="day.toISOString()"
                 :class="{'bg-warning bg-opacity-10': isToday(day)}">
              <div class="cell-content h-100 rounded p-1 position-relative d-flex flex-column gap-1">
                
                <!-- Render Schedules -->
                <div v-for="sched in getSchedules(doc.id, day)" :key="'s'+sched.id" 
                     class="schedule-card p-2 rounded shadow-sm pointer"
                     :style="{ backgroundColor: getDoctorColor(doc.id, 0.1), borderLeft: `4px solid ${getDoctorColor(doc.id, 1)}` }"
                     @click="openEditModal(sched)">
                  <div class="fw-bold d-flex align-items-center gap-1" :style="{ color: getDoctorColor(doc.id, 1), fontSize: '0.8rem' }">
                    <i class="bi bi-clock-history"></i>
                    {{ formatTime(sched.startTime) }} - {{ formatTime(sched.endTime) }}
                  </div>
                  <div class="text-muted mt-1 text-truncate" style="font-size: 0.7rem" v-if="sched.notes" :title="sched.notes">{{ sched.notes }}</div>
                </div>
                
                <!-- Render Block Times -->
                <div v-for="block in getBlockTimes(doc.id, day)" :key="'b'+block.id"
                     class="block-card p-2 rounded shadow-sm pointer border-0 position-relative overflow-hidden"
                     style="background-color: #dc3545; color: white;"
                     @click="openEditBlockModal(block)">
                  <div class="striped-bg"></div>
                  <div class="fw-bold position-relative z-1 d-flex align-items-center gap-1" style="font-size: 0.8rem">
                    <i class="bi bi-slash-circle"></i> Lịch Nghỉ
                  </div>
                  <div class="position-relative z-1 mt-1 text-white-50" style="font-size: 0.7rem">
                    {{ formatTime(block.startTime) }} - {{ formatTime(block.endTime) }}
                  </div>
                </div>

                <!-- Add Button Overlay -->
                <div class="add-btn-small text-center mt-auto pt-2 pb-1">
                  <button class="btn btn-sm btn-light rounded-circle shadow-sm text-warning add-btn-hover" 
                          style="width: 28px; height: 28px; padding: 0"
                          @click="openCreateModalFor(doc.id, day)" title="Thêm ca trực">
                    <i class="bi bi-plus fs-5" style="line-height: 0;"></i>
                  </button>
                </div>

              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Schedule Modal -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="showModal = false">
      <div class="glass-modal-card max-w-500 animate-slide-up">
        <div class="glass-modal-header bg-warning bg-opacity-25 border-bottom border-light">
          <h5 class="modal-title fw-bold text-dark-gold">
            <i class="bi bi-calendar-plus-fill me-2"></i> {{ isEdit ? 'Cập Nhật Ca Trực' : 'Phân Ca Trực Mới' }}
          </h5>
          <button class="btn-close" @click="showModal = false"></button>
        </div>
        <div class="glass-modal-body text-start">
          <form @submit.prevent="submitForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Chọn Bác sĩ trực *</label>
              <select v-model="form.doctorId" class="form-select glass-input fw-bold" required :disabled="isEdit">
                <option value="">-- Chọn bác sĩ --</option>
                <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
                  👨‍⚕️ {{ doc.fullName }}
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Ngày trực *</label>
              <input type="date" v-model="form.workDate" class="form-control glass-input fw-bold" required :min="minDate" :disabled="isEdit" />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giờ bắt đầu *</label>
                <input type="time" v-model="form.startTime" class="form-control glass-input fw-bold" required />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giờ kết thúc *</label>
                <input type="time" v-model="form.endTime" class="form-control glass-input fw-bold" required />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Ghi chú (Tùy chọn)</label>
              <input type="text" v-model="form.notes" class="form-control glass-input" placeholder="Ví dụ: Khám tổng quát..." />
            </div>

            <div class="mt-4 pt-3 border-top border-light d-flex justify-content-between">
              <button v-if="isEdit" type="button" class="btn btn-outline-danger rounded-pill px-4 shadow-sm" @click="handleDelete(currentScheduleId as number)">
                <i class="bi bi-trash"></i> Xoá
              </button>
              <div v-else></div>
              <div>
                <button type="button" class="btn btn-light rounded-pill px-4 me-2 glass-btn text-dark fw-bold" @click="showModal = false">Hủy</button>
                <button type="submit" class="btn btn-premium rounded-pill px-4 shadow-sm">Lưu ca trực</button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Create/Edit Block Modal -->
    <div v-if="showBlockModal" class="zalo-modal-overlay" @click.self="showBlockModal = false">
      <div class="glass-modal-card max-w-500 animate-slide-up">
        <div class="glass-modal-header bg-danger bg-opacity-25 border-bottom border-light">
          <h5 class="modal-title fw-bold text-danger">
            <i class="bi bi-calendar-x-fill me-2"></i> {{ isBlockEdit ? 'Chi Tiết Lịch Nghỉ' : 'Thêm Lịch Nghỉ/Bận' }}
          </h5>
          <button class="btn-close" @click="showBlockModal = false"></button>
        </div>
        <div class="glass-modal-body text-start">
          <form @submit.prevent="submitBlockForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Chọn Bác sĩ *</label>
              <select v-model="blockForm.doctorId" class="form-select glass-input fw-bold" required :disabled="isBlockEdit">
                <option value="">-- Chọn bác sĩ --</option>
                <option v-for="doc in doctorUsers" :key="doc.id" :value="doc.id">
                  👨‍⚕️ {{ doc.fullName }}
                </option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Loại Nghỉ/Bận *</label>
              <select v-model.number="blockForm.blockType" class="form-select glass-input fw-bold" :disabled="isBlockEdit">
                <option :value="0">Nghỉ phép (Leave)</option>
                <option :value="3">Họp (Meeting)</option>
                <option :value="2">Nghỉ trưa (Break)</option>
                <option :value="9">Lý do khác</option>
              </select>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Bắt đầu *</label>
                <input type="datetime-local" v-model="blockForm.startTime" class="form-control glass-input fw-bold" required :disabled="isBlockEdit" />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Kết thúc *</label>
                <input type="datetime-local" v-model="blockForm.endTime" class="form-control glass-input fw-bold" required :disabled="isBlockEdit" />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Lý do cụ thể</label>
              <textarea v-model="blockForm.reason" class="form-control glass-input" rows="2" placeholder="Ví dụ: Xin nghỉ phép có việc gia đình..." :disabled="isBlockEdit"></textarea>
            </div>

            <div class="mt-4 pt-3 border-top border-light d-flex justify-content-between">
              <button v-if="isBlockEdit" type="button" class="btn btn-outline-danger rounded-pill px-4 shadow-sm" @click="handleDeleteBlock(currentBlockId as string)">
                <i class="bi bi-trash"></i> Hủy lịch nghỉ
              </button>
              <div v-else></div>
              <div>
                <button type="button" class="btn btn-light rounded-pill px-4 me-2 glass-btn text-dark fw-bold" @click="showBlockModal = false">Đóng</button>
                <button v-if="!isBlockEdit" type="submit" class="btn btn-danger rounded-pill px-4 shadow-sm">Lưu lịch nghỉ</button>
              </div>
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

// --- STATE ---
const loading = ref(false);
const schedulesList = ref<any[]>([]);
const blockTimesList = ref<any[]>([]);
const doctorUsers = ref<any[]>([]);
const filterDoctorId = ref('all');

// --- WEEK MANAGEMENT ---
const currentDate = ref(new Date());

const currentWeekDays = computed(() => {
  const dates: Date[] = [];
  const d = new Date(currentDate.value);
  const day = d.getDay() || 7; // Convert Sun (0) to 7
  d.setDate(d.getDate() - day + 1); // Set to Monday
  
  for (let i = 0; i < 7; i++) {
    dates.push(new Date(d));
    d.setDate(d.getDate() + 1);
  }
  return dates;
});

const nextWeek = () => {
  const d = new Date(currentDate.value);
  d.setDate(d.getDate() + 7);
  currentDate.value = d;
  loadData();
};

const prevWeek = () => {
  const d = new Date(currentDate.value);
  d.setDate(d.getDate() - 7);
  currentDate.value = d;
  loadData();
};

const goToday = () => {
  currentDate.value = new Date();
  loadData();
};

// --- DATA FETCHING ---
const filteredDoctors = computed(() => {
  if (filterDoctorId.value === 'all') return doctorUsers.value;
  return doctorUsers.value.filter(u => u.id === filterDoctorId.value);
});

const loadData = async () => {
  loading.value = true;
  try {
    const startStr = currentWeekDays.value[0].toISOString().split('T')[0];
    const endStr = currentWeekDays.value[6].toISOString().split('T')[0];
    
    const [schedRes, blockRes] = await Promise.all([
      api.get(`/admin/schedules?startDate=${startStr}&endDate=${endStr}`).catch(() => ({ data: [] })),
      api.get(`/admin/block-times?startDate=${startStr}&endDate=${endStr}`).catch(() => ({ data: [] }))
    ]);
    
    schedulesList.value = schedRes.data || [];
    blockTimesList.value = blockRes.data || [];
  } catch (err) {
    console.error('Lỗi tải dữ liệu lịch:', err);
  } finally {
    loading.value = false;
  }
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

// --- GRID HELPERS ---
const getSchedules = (doctorId: string, day: Date) => {
  const dateStr = day.toISOString().split('T')[0];
  return schedulesList.value.filter(s => {
    if (!s.workDate) return false;
    return s.doctorId === doctorId && s.workDate.split('T')[0] === dateStr;
  }).sort((a,b) => (a.startTime || '').localeCompare(b.startTime || ''));
};

const getBlockTimes = (doctorId: string, day: Date) => {
  const dateStr = day.toISOString().split('T')[0];
  return blockTimesList.value.filter(b => {
    if (!b.startTime) return false;
    return b.doctorId === doctorId && b.startTime.split('T')[0] === dateStr;
  });
};

const getDoctorColor = (doctorId: string | undefined, opacity: number = 1) => {
  if (!doctorId) return `rgba(78, 115, 223, ${opacity})`;
  const colors = [
    { r: 78,  g: 115, b: 223 }, // #4e73df
    { r: 28,  g: 200, b: 138 }, // #1cc88a
    { r: 54,  g: 185, b: 204 }, // #36b9cc
    { r: 246, g: 194, b: 62  }, // #f6c23e
    { r: 231, g: 74,  b: 59  }, // #e74a3b
    { r: 111, g: 66,  b: 193 }, // #6f42c1
    { r: 253, g: 126, b: 20  }, // #fd7e14
    { r: 32,  g: 201, b: 151 }  // #20c997
  ];
  let hash = 0;
  for (let i = 0; i < doctorId.length; i++) {
    hash = doctorId.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % colors.length;
  const c = colors[index];
  return `rgba(${c.r}, ${c.g}, ${c.b}, ${opacity})`;
};

// --- UTILS ---
const formatDayName = (d: Date) => {
  const days = ['CN', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
  return days[d.getDay()];
};
const formatDateNum = (d: Date) => {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${pad(d.getDate())}/${pad(d.getMonth()+1)}`;
};
const formatTime = (timeStr: string) => {
  if (!timeStr) return '';
  // HH:mm:ss -> HH:mm
  if (timeStr.length >= 5) return timeStr.substring(0, 5);
  // ISO -> HH:mm
  if (timeStr.includes('T')) {
    const d = new Date(timeStr);
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
  return timeStr;
};
const isToday = (d: Date) => {
  const today = new Date();
  return d.getDate() === today.getDate() && 
         d.getMonth() === today.getMonth() && 
         d.getFullYear() === today.getFullYear();
};

const minDate = computed(() => {
  const today = new Date();
  return today.toISOString().split('T')[0];
});

// --- MODALS STATE & LOGIC ---
const showModal = ref(false);
const isEdit = ref(false);
const currentScheduleId = ref<number | null>(null);

const form = ref({
  doctorId: '',
  workDate: '',
  startTime: '08:00',
  endTime: '12:00',
  maxAppointments: 10,
  isAvailable: true,
  notes: ''
});

const openCreateScheduleModal = () => {
  isEdit.value = false;
  currentScheduleId.value = null;
  form.value = {
    doctorId: filterDoctorId.value !== 'all' ? filterDoctorId.value : (doctorUsers.value[0]?.id || ''),
    workDate: minDate.value,
    startTime: '08:00',
    endTime: '12:00',
    maxAppointments: 10,
    isAvailable: true,
    notes: ''
  };
  showModal.value = true;
};

const openCreateModalFor = (doctorId: string, day: Date) => {
  isEdit.value = false;
  currentScheduleId.value = null;
  const pad = (n: number) => n.toString().padStart(2, '0');
  form.value = {
    doctorId: doctorId,
    workDate: `${day.getFullYear()}-${pad(day.getMonth()+1)}-${pad(day.getDate())}`,
    startTime: '08:00',
    endTime: '12:00',
    maxAppointments: 10,
    isAvailable: true,
    notes: ''
  };
  showModal.value = true;
};

const openEditModal = (sched: any) => {
  isEdit.value = true;
  currentScheduleId.value = sched.id;
  form.value = {
    doctorId: sched.doctorId,
    workDate: sched.workDate ? sched.workDate.split('T')[0] : '',
    startTime: sched.startTime ? sched.startTime.substring(0, 5) : '',
    endTime: sched.endTime ? sched.endTime.substring(0, 5) : '',
    maxAppointments: sched.maxAppointments || 10,
    isAvailable: sched.isAvailable,
    notes: sched.notes || ''
  };
  showModal.value = true;
};

const submitForm = async () => {
  try {
    const payload = {
      ...form.value,
      startTime: form.value.startTime.length === 5 ? `${form.value.startTime}:00` : form.value.startTime,
      endTime: form.value.endTime.length === 5 ? `${form.value.endTime}:00` : form.value.endTime
    };

    if (isEdit.value && currentScheduleId.value) {
      await api.put(`/admin/schedules/${currentScheduleId.value}`, payload);
    } else {
      await api.post('/admin/schedules', payload);
    }
    showModal.value = false;
    await loadData(); // Reload both schedules & blocks
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi lưu ca trực.');
  }
};

const handleDelete = async (id: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá ca trực này không?')) return;
  try {
    await api.delete(`/admin/schedules/${id}`);
    showModal.value = false;
    await loadData();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá ca trực.');
  }
};

// --- BLOCK MODAL ---
const showBlockModal = ref(false);
const isBlockEdit = ref(false);
const currentBlockId = ref<string | null>(null);

const blockForm = ref({
  doctorId: '',
  startTime: '',
  endTime: '',
  blockType: 0,
  reason: ''
});

const openCreateBlockModal = () => {
  isBlockEdit.value = false;
  currentBlockId.value = null;
  blockForm.value = {
    doctorId: filterDoctorId.value !== 'all' ? filterDoctorId.value : (doctorUsers.value[0]?.id || ''),
    startTime: '',
    endTime: '',
    blockType: 0,
    reason: ''
  };
  showBlockModal.value = true;
};

const openEditBlockModal = (block: any) => {
  isBlockEdit.value = true;
  currentBlockId.value = block.id;
  const toLocalDT = (dtString: string) => {
      const d = new Date(dtString);
      const pad = (n: number) => n.toString().padStart(2, '0');
      return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  };
  blockForm.value = {
    doctorId: block.doctorId,
    startTime: block.startTime ? toLocalDT(block.startTime) : '',
    endTime: block.endTime ? toLocalDT(block.endTime) : '',
    blockType: block.blockType || 0,
    reason: block.reason || ''
  };
  showBlockModal.value = true;
};

const submitBlockForm = async () => {
  try {
    const payload = {
      ...blockForm.value,
      startTime: new Date(blockForm.value.startTime).toISOString(),
      endTime: new Date(blockForm.value.endTime).toISOString()
    };
    
    // Fallback to simple create/delete if put is not available
    if (isBlockEdit.value) {
      alert("Chỉnh sửa trực tiếp chưa hỗ trợ, vui lòng xóa và tạo mới!");
    } else {
      await api.post('/admin/block-times', payload);
    }
    showBlockModal.value = false;
    await loadData();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi lưu lịch nghỉ.');
  }
};

const handleDeleteBlock = async (id: string) => {
  if (!confirm('Bạn có chắc chắn muốn hủy lịch nghỉ này?')) return;
  try {
    await api.delete(`/admin/block-times/${id}`);
    showBlockModal.value = false;
    await loadData();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi hủy lịch nghỉ.');
  }
};

// --- INIT ---
onMounted(() => {
  loadDoctors().then(() => {
    loadData();
  });
});
</script>

<style scoped>
/* Glassmorphism Styles */
.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: 16px;
}
.glass-modal-card {
  background: rgba(255, 255, 255, 0.9);
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
  border-color: var(--primary-color, #ffc107);
  box-shadow: 0 0 0 0.25rem rgba(255, 193, 7, 0.25);
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
.text-dark-gold {
  color: #b25e00;
}
.zalo-modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(8px);
  z-index: 1200;
  display: flex; justify-content: center; align-items: center;
  padding: 1rem;
}
.max-w-500 { max-width: 500px; }

/* Grid Styles */
.schedule-grid {
  border: 1px solid rgba(0,0,0,0.05);
}
.grid-container {
  display: flex;
  flex-direction: column;
}
.grid-cell {
  border-right: 1px solid rgba(0,0,0,0.05);
}
.grid-cell:last-child {
  border-right: none;
}
.doctor-col {
  width: 140px;
  flex-shrink: 0;
  background: rgba(255,255,255,0.4);
}
.day-col {
  flex: 1;
  min-width: 120px;
}
.cell-content {
  min-height: 100px;
  transition: background 0.2s;
}
.cell-content:hover {
  background: rgba(255, 193, 7, 0.05);
}

/* Add Button Hover */
.add-btn-small {
  opacity: 0;
  transition: opacity 0.2s, transform 0.2s;
  transform: translateY(10px);
}
.cell-content:hover .add-btn-small {
  opacity: 1;
  transform: translateY(0);
}
.add-btn-hover:hover {
  background-color: var(--primary-color, #ffc107) !important;
  color: white !important;
  transform: scale(1.1);
}

/* Card Styles */
.schedule-card {
  transition: transform 0.2s, box-shadow 0.2s;
}
.schedule-card:hover {
  transform: translateY(-2px) scale(1.02);
  box-shadow: 0 6px 12px rgba(0,0,0,0.1) !important;
  z-index: 2;
}

.block-card {
  transition: transform 0.2s;
}
.block-card:hover {
  transform: translateY(-2px) scale(1.02);
}
.striped-bg {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background-image: linear-gradient(45deg, rgba(255,255,255,0.2) 25%, transparent 25%, transparent 50%, rgba(255,255,255,0.2) 50%, rgba(255,255,255,0.2) 75%, transparent 75%, transparent);
  background-size: 1rem 1rem;
  opacity: 0.5;
  z-index: 0;
}

/* Animations */
.animate-fade-in { animation: fadeIn 0.4s ease-out forwards; }
.animate-slide-up { animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
