<template>
  <div class="schedules-admin-tab container-fluid p-0 animate-fade-in" style="min-width: 0; max-width: 100%; overflow-x: hidden;">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-calendar-event-fill text-warning me-2"></i>Quản lý Lịch Làm Việc & Nghỉ</h4>
        <p class="text-muted small mb-0">Thiết lập ca trực với giao diện Timeline Grid Glassmorphism Độc Bản</p>
      </div>
      <div class="d-flex flex-wrap justify-content-end gap-2">
        <button class="btn btn-danger px-3 py-2 rounded-pill shadow-sm" @click="openCreateBlockModal">
          <i class="bi bi-calendar-x-fill me-1"></i> Khóa Lịch
        </button>
        <button class="btn btn-outline-primary px-3 py-2 rounded-pill shadow-sm" @click="showProfileManager = true">
          <i class="bi bi-diagram-3-fill me-1"></i> Quản lý Mẫu Lịch
        </button>
        <button class="btn btn-outline-info px-3 py-2 rounded-pill shadow-sm" @click="showAssignProfile = true">
          <i class="bi bi-people-fill me-1"></i> Gán Profile
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
                
                <!-- Render Holiday Overlay -->
                <div v-if="getHolidayForDay(day)" class="holiday-card p-2 rounded shadow-sm d-flex flex-column justify-content-center align-items-center text-center w-100 h-100 position-absolute top-0 start-0" style="background: rgba(220, 53, 69, 0.1); border: 1px dashed #dc3545; z-index: 5; backdrop-filter: blur(2px);">
                  <i class="bi bi-calendar-x text-danger fs-4 mb-1"></i>
                  <span class="fw-bold text-danger small" style="font-size: 0.75rem;">Nghỉ lễ/Đóng cửa</span>
                  <span class="text-danger mt-1 fw-medium" style="font-size: 0.65rem; word-break: break-word;" :title="getHolidayForDay(day).name">{{ getHolidayForDay(day).name }}</span>
                </div>

                <template v-else>
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
                       @click="!block.id.toString().startsWith('unavail_') ? openEditBlockModal(block) : null"
                       :title="block.reason">
                    <div class="striped-bg"></div>
                    <div class="fw-bold position-relative z-1 d-flex align-items-center gap-1" style="font-size: 0.8rem">
                      <i class="bi bi-slash-circle"></i> {{ block.id.toString().startsWith('unavail_') ? 'Nghỉ Phép/Đổi Ca' : 'Lịch Nghỉ' }}
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
                </template>

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
            <!-- Date Range Picker -->
            <div class="row g-3 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">
                  <i class="bi bi-calendar-event text-danger me-1"></i> Từ ngày *
                </label>
                <input type="date" v-model="blockForm.startDate" class="form-control glass-input fw-bold text-dark" required :disabled="isBlockEdit" :min="minDate" @change="validateDateRange" />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">
                  <i class="bi bi-calendar-check text-danger me-1"></i> Đến ngày *
                </label>
                <input type="date" v-model="blockForm.endDate" class="form-control glass-input fw-bold text-dark" required :disabled="isBlockEdit" :min="blockForm.startDate || minDate" />
              </div>
            </div>



            <!-- Time Range Picker -->
            <div class="mb-3 bg-white bg-opacity-50 p-3 rounded-3 border border-light shadow-sm">
              <div class="form-check form-switch mb-2 d-flex align-items-center gap-2">
                <input class="form-check-input" type="checkbox" role="switch" id="allDaySwitch" v-model="blockForm.isAllDay" :disabled="isBlockEdit" style="transform: scale(1.2); cursor: pointer;">
                <label class="form-check-label text-dark fw-bold small" for="allDaySwitch" style="cursor: pointer;">
                  <i class="bi bi-clock-history text-primary me-1"></i> Nghỉ cả ngày (All day)
                </label>
              </div>
              <div class="text-muted small ms-4" style="font-size: 0.75rem" v-if="blockForm.isAllDay">
                Hệ thống sẽ tự động gán thời gian nghỉ cả ngày (00:00 - 23:59).
              </div>
            </div>

            <div class="row g-3 mb-3" v-if="!blockForm.isAllDay">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">
                  <i class="bi bi-play-circle-fill text-success me-1"></i> Giờ bắt đầu *
                </label>
                <TimePicker
                  v-model="blockForm.startHour"
                  :slots="timeSlotOptions"
                  :disabled="isBlockEdit"
                  @change="validateEndTime"
                />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">
                  <i class="bi bi-stop-circle-fill text-danger me-1"></i> Giờ kết thúc *
                </label>
                <TimePicker
                  v-model="blockForm.endHour"
                  :slots="endTimeSlotOptions"
                  :disabled="isBlockEdit"
                />
              </div>
            </div>

            <!-- Duration Badge -->
            <div v-if="(blockForm.isAllDay || (blockForm.startHour && blockForm.endHour)) && !isBlockEdit" class="mb-3">
              <div class="duration-badge d-inline-flex align-items-center gap-2 px-3 py-2 rounded-pill">
                <i class="bi bi-hourglass-split text-danger"></i>
                <span class="fw-bold small text-dark">Tổng thời gian: <strong class="text-danger">{{ blockDuration }}</strong></span>
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
    <!-- Template Modals -->
    <ScheduleProfileManager v-if="showProfileManager" @close="showProfileManager = false" @changed="loadData" />
    
    <AssignProfileDialog v-if="showAssignProfile" @close="showAssignProfile = false" @assigned="handleProfileAssigned" />

    <!-- Pending Exceptions List (For Admin) -->
    <div class="mt-8">
      <PendingExceptionsTab />
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import Swal from 'sweetalert2';
import api from '../../services/api';
import ScheduleProfileManager from './ScheduleProfileManager.vue';
import AssignProfileDialog from './AssignProfileDialog.vue';
import PendingExceptionsTab from './PendingExceptionsTab.vue';
import TimePicker from '../shared/TimePicker.vue';

// --- STATE ---
const loading = ref(false);
const schedulesList = ref<any[]>([]);
const blockTimesList = ref<any[]>([]);
const clinicHolidaysList = ref<any[]>([]);
const doctorUsers = ref<any[]>([]);
const filterDoctorId = ref('all');

const showProfileManager = ref(false);
const showAssignProfile = ref(false);

const handleProfileAssigned = () => {
  showAssignProfile.value = false;
  loadData();
};

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

const toLocalDateStr = (d: Date) => {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}`;
};

const loadData = async () => {
  loading.value = true;
  try {
    const startStr = toLocalDateStr(currentWeekDays.value[0]);
    const endStr = toLocalDateStr(currentWeekDays.value[6]);
    
    const [schedulesRes, blocksRes, holidaysRes] = await Promise.all([
      api.get(`/doctor-schedules?startDate=${startStr}&endDate=${endStr}`).catch(() => ({ data: [] })),
      api.get(`/block-times?startDate=${startStr}&endDate=${endStr}`).catch(() => ({ data: [] })),
      api.get(`/OperatingHours/holidays`).catch(() => ({ data: [] }))
    ]);
    
    schedulesList.value = schedulesRes.data || [];
    blockTimesList.value = blocksRes.data || [];
    clinicHolidaysList.value = holidaysRes.data?.data || (Array.isArray(holidaysRes.data) ? holidaysRes.data : []);
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
      return (r === 'doctor' || r === 'clinical_doctor' || r === 'vaccination_doctor') && u.isActive === true;
    });
  } catch (err) {
    console.error('Lỗi tải danh sách bác sĩ:', err);
  }
};

// --- GRID HELPERS ---
const getHolidayForDay = (day: Date) => {
  const dateStr = toLocalDateStr(day);
  return clinicHolidaysList.value.find(h => {
    if (!h.isActive || !h.startDate || !h.endDate) return false;
    const startStrH = h.startDate.split('T')[0];
    const endStrH = h.endDate.split('T')[0];
    return dateStr >= startStrH && dateStr <= endStrH;
  });
};

const getSchedules = (doctorId: string, day: Date) => {
  const dateStr = toLocalDateStr(day);
  return schedulesList.value.filter(s => {
    if (!s.workDate || !s.isAvailable) return false;
    // API có thể trả về UTC (VD: 2026-07-18T17:00:00) nhưng thiếu chữ Z.
    // Thêm Z vào để đảm bảo trình duyệt hiểu đây là giờ UTC, sau đó Date sẽ tự quy đổi về giờ Local.
    const utcWorkDate = s.workDate.endsWith('Z') ? s.workDate : `${s.workDate}Z`;
    const localD = new Date(utcWorkDate);
    const wStr = toLocalDateStr(localD);
    return s.doctorId === doctorId && wStr === dateStr;
  }).sort((a,b) => (a.startTime || '').localeCompare(b.startTime || ''));
};

const getBlockTimes = (doctorId: string, day: Date) => {
  const dateStr = toLocalDateStr(day);
  const dayStart = new Date(`${dateStr}T00:00:00`);
  const dayEnd = new Date(`${dateStr}T23:59:59`);
  
  const blocks = blockTimesList.value.filter(b => {
    if (!b.startTime || !b.endTime) return false;
    const utcStartTime = b.startTime.endsWith('Z') ? b.startTime : `${b.startTime}Z`;
    const utcEndTime = b.endTime.endsWith('Z') ? b.endTime : `${b.endTime}Z`;
    const sD = new Date(utcStartTime);
    const eD = new Date(utcEndTime);
    
    return b.doctorId === doctorId && (sD <= dayEnd && eD >= dayStart);
  });

  const unavailableSchedules = schedulesList.value.filter(s => {
    if (!s.workDate || s.isAvailable !== false) return false;
    const utcWorkDate = s.workDate.endsWith('Z') ? s.workDate : `${s.workDate}Z`;
    const localD = new Date(utcWorkDate);
    return s.doctorId === doctorId && toLocalDateStr(localD) === dateStr;
  }).map(s => ({
    id: 'unavail_' + s.id,
    doctorId: s.doctorId,
    startTime: s.workDate.split('T')[0] + 'T' + s.startTime,
    endTime: s.workDate.split('T')[0] + 'T' + s.endTime,
    reason: s.notes || 'Nghỉ / Đổi ca (Exception)'
  }));

  return [...blocks, ...unavailableSchedules];
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

// --- TIME SLOT OPTIONS ---
const timeSlotOptions = computed(() => {
  const slots: string[] = [];
  for (let h = 8; h <= 20; h++) {
    slots.push(`${String(h).padStart(2, '0')}:00`);
    if (h < 20) slots.push(`${String(h).padStart(2, '0')}:30`);
  }
  return slots;
});

const endTimeSlotOptions = computed(() => {
  if (blockForm.value.startDate < blockForm.value.endDate) {
    return timeSlotOptions.value;
  }
  return timeSlotOptions.value.filter(s => s > blockForm.value.startHour);
});

const blockDuration = computed(() => {
  if (!blockForm.value.startDate || !blockForm.value.endDate) return '';
  
  let startStr = `${blockForm.value.startDate}T${blockForm.value.isAllDay ? '00:00' : blockForm.value.startHour}:00`;
  let endStr = `${blockForm.value.endDate}T${blockForm.value.isAllDay ? '23:59' : blockForm.value.endHour}:00`;
  
  const start = new Date(startStr);
  const end = new Date(endStr);
  
  const totalMin = Math.round((end - start) / 60000);
  
  if (totalMin <= 0) return 'Không hợp lệ';
  
  if (blockForm.value.isAllDay) {
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
      await api.put(`/doctor-schedules/${currentScheduleId.value}`, payload);
    } else {
      await api.post('/doctor-schedules', payload);
    }
    showModal.value = false;
    await loadData(); // Reload both schedules & blocks
  } catch (err: any) {
    Swal.fire('Lỗi', err.response?.data?.message || 'Lỗi khi lưu ca trực.', 'error');
  }
};

const handleDelete = async (id: number) => {
  const result = await Swal.fire({
    title: 'Xác nhận xóa',
    text: 'Bạn có chắc chắn muốn xoá ca trực này không?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Có, xóa',
    cancelButtonText: 'Hủy'
  });
  if (!result.isConfirmed) return;
  try {
    await api.delete(`/doctor-schedules/${id}`);
    showModal.value = false;
    await loadData();
  } catch (err: any) {
    Swal.fire('Lỗi', err.response?.data?.message || 'Lỗi khi xoá ca trực.', 'error');
  }
};

// --- BLOCK MODAL ---
const showBlockModal = ref(false);
const isBlockEdit = ref(false);
const currentBlockId = ref<string | null>(null);

const blockForm = ref({
  doctorId: '',
  startDate: '',
  endDate: '',
  isAllDay: true,
  startHour: '08:00',
  endHour: '12:00',
  blockType: 0,
  reason: ''
});

const selectedDaysCount = computed(() => {
  if (!blockForm.value.startDate || !blockForm.value.endDate) return 0;
  const s = new Date(blockForm.value.startDate);
  const e = new Date(blockForm.value.endDate);
  const diff = Math.round((e.getTime() - s.getTime()) / (1000 * 60 * 60 * 24)) + 1;
  return diff > 0 ? diff : 0;
});

const formatDisplayDate = (dateStr: string) => {
  if (!dateStr) return '';
  const [y, m, d] = dateStr.split('-');
  return `${d}/${m}/${y}`;
};

const validateDateRange = () => {
  if (blockForm.value.endDate && blockForm.value.endDate < blockForm.value.startDate) {
    blockForm.value.endDate = blockForm.value.startDate;
  }
};

const validateEndTime = () => {
  if (blockForm.value.startDate === blockForm.value.endDate && blockForm.value.endHour <= blockForm.value.startHour) {
    const idx = endTimeSlotOptions.value.findIndex(s => s > blockForm.value.startHour);
    blockForm.value.endHour = endTimeSlotOptions.value[idx >= 0 ? idx : 0] || '12:00';
  }
};

const openCreateBlockModal = () => {
  isBlockEdit.value = false;
  currentBlockId.value = null;
  blockForm.value = {
    doctorId: filterDoctorId.value !== 'all' ? filterDoctorId.value : (doctorUsers.value[0]?.id || ''),
    startDate: minDate.value,
    endDate: minDate.value,
    isAllDay: true,
    startHour: '08:00',
    endHour: '12:00',
    blockType: 0,
    reason: ''
  };
  showBlockModal.value = true;
};

const openEditBlockModal = (block: any) => {
  isBlockEdit.value = true;
  currentBlockId.value = block.id;
  const pad = (n: number) => n.toString().padStart(2, '0');

  const parseDt = (dtString: string) => {
    if (!dtString) return { date: '', hour: '08:00' };
    // Handle both 'Z' suffix and no suffix
    const safe = dtString.endsWith('Z') ? dtString : `${dtString}Z`;
    const d = new Date(safe);
    return {
      date: `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}`,
      hour: `${pad(d.getHours())}:${pad(d.getMinutes())}`
    };
  };

  const start = parseDt(block.startTime);
  const end = parseDt(block.endTime);
  const isAll = (start.hour === '00:00' && end.hour === '23:59') || (start.hour === '00:00' && end.hour === '00:00');

  blockForm.value = {
    doctorId: block.doctorId,
    startDate: start.date,
    endDate: start.date,
    isAllDay: isAll,
    startHour: isAll ? '08:00' : start.hour,
    endHour: isAll ? '12:00' : end.hour,
    blockType: block.blockType || 0,
    reason: block.reason || ''
  };
  showBlockModal.value = true;
};

const submitBlockForm = async () => {
  if (blockForm.value.isAllDay) {
    blockForm.value.startHour = '00:00';
    blockForm.value.endHour = '23:59';
  }

  if (!blockForm.value.startDate || !blockForm.value.endDate || !blockForm.value.startHour || !blockForm.value.endHour) {
    Swal.fire('Cảnh báo', 'Vui lòng chọn đầy đủ ngày và giờ.', 'warning');
    return;
  }
  if (blockForm.value.endHour <= blockForm.value.startHour) {
    Swal.fire('Cảnh báo', 'Giờ kết thúc phải sau giờ bắt đầu.', 'warning');
    return;
  }
  if (blockForm.value.endDate < blockForm.value.startDate) {
    Swal.fire('Cảnh báo', 'Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.', 'warning');
    return;
  }

  if (isBlockEdit.value) {
    Swal.fire('Thông báo', 'Chỉnh sửa trực tiếp chưa hỗ trợ, vui lòng xóa và tạo mới!', 'info');
    return;
  }

  try {
    let startStr = `${blockForm.value.startDate}T${blockForm.value.isAllDay ? '00:00' : blockForm.value.startHour}:00`;
    let endStr = `${blockForm.value.endDate}T${blockForm.value.isAllDay ? '23:59' : blockForm.value.endHour}:00`;
    
    const startDt = new Date(startStr);
    const endDt = new Date(endStr);

    await api.post('/block-times', {
      doctorId:  blockForm.value.doctorId,
      startTime: startDt.toISOString(),
      endTime:   endDt.toISOString(),
      blockType: blockForm.value.blockType,
      reason:    blockForm.value.reason
    });

    showBlockModal.value = false;
    await loadData();
  } catch (err: any) {
    Swal.fire('Lỗi', err.response?.data?.message || 'Lỗi lưu lịch nghỉ.', 'error');
  }
};

const handleDeleteBlock = async (id: string) => {
  const result = await Swal.fire({
    title: 'Xác nhận xóa',
    text: 'Bạn có chắc chắn muốn hủy lịch nghỉ này?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Có, xóa',
    cancelButtonText: 'Hủy'
  });
  if (!result.isConfirmed) return;
  try {
    await api.delete(`/block-times/${id}`);
    showBlockModal.value = false;
    await loadData();
  } catch (err: any) {
    Swal.fire('Lỗi', err.response?.data?.message || 'Lỗi khi hủy lịch nghỉ.', 'error');
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

/* --- Time Picker (Direction A) --- */
.time-select-wrapper {
  position: relative;
}
.time-select-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: #9ca3af;
  font-size: 0.85rem;
  z-index: 2;
  pointer-events: none;
  transition: color 0.2s;
}
.time-select {
  padding-left: 2.4rem !important;
  cursor: pointer;
  appearance: none;
  -webkit-appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 16 16'%3E%3Cpath fill='%236b7280' d='M7.247 11.14L2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z'/%3E%3C/svg%3E") !important;
  background-repeat: no-repeat !important;
  background-position: right 12px center !important;
  padding-right: 2rem !important;
  border-radius: 10px !important;
  transition: all 0.25s ease !important;
}
.time-select:focus {
  border-color: #dc3545 !important;
  box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.15) !important;
}
.time-select:focus + .time-select-icon,
.time-select-wrapper:focus-within .time-select-icon {
  color: #dc3545;
}
.time-select:not(:disabled):hover {
  border-color: rgba(220, 53, 69, 0.4) !important;
  background-color: rgba(255,255,255,0.9) !important;
}

/* Duration Badge */
.duration-badge {
  background: linear-gradient(135deg, rgba(220, 53, 69, 0.06), rgba(220, 53, 69, 0.12));
  border: 1px dashed rgba(220, 53, 69, 0.3);
  animation: fadeIn 0.3s ease;
}

/* Date Range Summary */
.date-range-summary {
  background: linear-gradient(135deg, rgba(220, 53, 69, 0.05), rgba(220, 53, 69, 0.10));
  border: 1px solid rgba(220, 53, 69, 0.2);
  animation: fadeIn 0.3s ease;
}
.date-range-invalid {
  background: rgba(220, 53, 69, 0.05);
  border-color: rgba(220, 53, 69, 0.35);
  border-style: dashed;
}
</style>
