<template>
  <div class="doctor-queue-tab h-100 d-flex flex-column glass-panel rounded-4 p-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center border-bottom pb-3 mb-4">
      <div>
        <h4 class="fw-bold text-dark mb-1">
          <i class="bi bi-calendar-week-fill text-primary me-2"></i> Lịch làm việc
        </h4>
        <p class="text-muted mb-0 small">Hôm nay: {{ todayFormatted }}</p>
      </div>
      <div>
        <span class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-3 py-2 border border-danger border-opacity-25 shadow-sm">
          <i class="bi bi-circle-fill small me-1 pulse-icon"></i> Bác sĩ điều trị
        </span>
      </div>
    </div>

    <!-- Controls -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div class="d-flex align-items-center bg-white bg-opacity-50 backdrop-blur rounded-pill p-1 shadow-sm border-glass">
        <button class="btn btn-sm btn-white rounded-circle hover-lift" @click="prevWeek">
          <i class="bi bi-chevron-left text-dark"></i>
        </button>
        <div class="fw-bold text-dark px-4 font-monospace">
          Tuần: {{ weekStartStr }} – {{ weekEndStr }}
        </div>
        <button class="btn btn-sm btn-white rounded-circle hover-lift" @click="nextWeek">
          <i class="bi bi-chevron-right text-dark"></i>
        </button>
      </div>

      <div class="d-flex gap-2">
        <button v-if="!isAdmin" class="btn btn-outline-warning rounded-pill px-4 fw-bold shadow-sm bg-white me-2" @click="showCreateException = true">
          <i class="bi bi-person-lines-fill me-1"></i> Đăng ký nghỉ phép
        </button>
        <select v-if="isAdmin" v-model="selectedDoctor" @change="fetchWeeklySchedule" class="form-select border-glass bg-white bg-opacity-75 rounded-pill px-3 py-1 shadow-sm fw-medium" style="width: 180px;">
          <option value="ALL">Tất cả bác sĩ</option>
          <option v-for="doc in doctors" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
        </select>
        <button class="btn btn-outline-glass rounded-pill px-4 fw-bold" @click="goToToday">Hôm nay</button>
        <button class="btn btn-premium-neon rounded-pill px-4 fw-bold shadow-sm" @click="fetchWeeklySchedule" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
          <i v-else class="bi bi-arrow-clockwise me-1"></i> Làm mới
        </button>
      </div>
    </div>

    <!-- Calendar Grid -->
    <div class="calendar-grid-container flex-grow-1 overflow-auto border rounded-3 position-relative">
      
      <!-- Loading Overlay -->
      <div v-if="loading" class="position-absolute top-0 start-0 w-100 h-100 bg-white bg-opacity-50 backdrop-blur d-flex justify-content-center align-items-center z-3 rounded-3">
        <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status"></div>
      </div>

      <table class="table mb-0 calendar-table">
        <thead class="position-sticky top-0 z-2 glass-header">
          <tr>
            <th class="text-center text-secondary-muted small fw-bold py-3 align-middle border-bottom-glass" style="width: 80px; min-width: 80px;">GIỜ</th>
            <th v-for="(day, index) in weekDays" :key="index" 
                class="text-center py-3 border-bottom-glass border-start-glass"
                :class="{ 'current-day-col': isToday(day.date) }">
              <div class="d-flex flex-column align-items-center">
                <span class="text-secondary-muted small fw-bold mb-2">{{ day.name }}</span>
                <div class="day-circle shadow-sm" :class="isToday(day.date) ? 'today-circle pulse-gold' : 'bg-white text-dark border-glass'">
                  {{ day.date.getDate() }}
                </div>
              </div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="time in timeSlots" :key="time">
            <td class="text-center text-secondary-muted small fw-bold align-middle border-end-glass border-bottom-glass">{{ time }}</td>
            <td v-for="(day, index) in weekDays" :key="index" class="p-2 position-relative border-bottom-glass border-start-glass" :class="{ 'current-day-cell': isToday(day.date) }">
              <!-- Render Events for this cell -->
              <div class="d-flex flex-column gap-2">
                <template v-for="evt in getEventsForCell(day.date, time)" :key="evt.id">
                  
                  <!-- Appointment Card -->
                  <div class="appointment-card p-2"
                       :class="[getBorderClass(evt.extendedProps?.status), { 'emergency-pulse': evt.extendedProps?.isEmergency }]">
                    
                    <div class="d-flex justify-content-between align-items-start mb-1">
                      <strong class="text-primary d-block text-truncate fw-bolder fs-6" style="letter-spacing: -0.2px;" :title="'Giờ thực tế: ' + getActualTime(evt.start)">
                        <i class="bi bi-heptagon-fill text-warning me-1 small" style="font-size: 0.7rem;"></i><span class="text-danger opacity-75 small">[{{ getActualTime(evt.start) }}]</span> {{ evt.extendedProps?.petName || 'Thú cưng' }}
                      </strong>
                      <div class="d-flex gap-1">
                        <span v-if="evt.extendedProps?.isEmergency" class="badge bg-danger shadow-sm rounded-pill px-2 py-1" style="font-size: 0.6rem; letter-spacing: 0.5px;">CẤP CỨU</span>
                        <span v-if="['completed', 'ready_to_pay'].includes(evt.extendedProps?.status)" class="badge bg-success bg-opacity-10 text-success border border-success shadow-sm rounded-pill px-2 py-1" style="font-size: 0.6rem; letter-spacing: 0.5px;"><i class="bi bi-check2-circle me-1"></i>Đã hoàn tất</span>
                      </div>
                    </div>
                    
                    <div class="text-dark fw-semibold text-truncate mb-1" style="font-size: 0.75rem;">
                      <i class="bi bi-person-fill text-secondary me-1"></i>{{ evt.extendedProps?.customerName || 'Khách vãng lai' }}
                    </div>

                    <div class="text-dark fw-medium text-truncate" style="font-size: 0.75rem;">
                      <i class="bi bi-clipboard2-pulse text-secondary me-1"></i>{{ evt.extendedProps?.serviceName || evt.title }}
                    </div>

                    <!-- Action Button -->
                    <div class="mt-2" v-if="isToday(day.date) && evt.extendedProps?.status === 'in_progress'">
                      <button class="btn btn-sm btn-outline-primary w-100 rounded-pill py-1 d-flex justify-content-center align-items-center fw-bold shadow-sm" 
                              style="font-size: 0.75rem; background-color: #f8fbff;"
                              @click.stop="startTreatment(evt)">
                        <i class="bi bi-play-circle-fill me-1 fs-6"></i> Tiến hành khám
                      </button>
                    </div>

                  </div>

                </template>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Create Exception Modal -->
    <CreateExceptionDialog v-if="showCreateException" :currentDoctorId="currentDoctorId" @close="showCreateException = false" @submitted="handleExceptionSubmitted" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api';
import CreateExceptionDialog from './CreateExceptionDialog.vue';

const emit = defineEmits(['switch-tab']);

const events = ref<any[]>([]);
const loading = ref(false);
const currentDate = ref(new Date());

const isAdmin = computed(() => {
  const role = localStorage.getItem('user_role');
  return role === 'admin' || role === 'receptionist';
});

const doctors = ref<any[]>([]);
const selectedDoctor = ref('ALL');
const showCreateException = ref(false);
const currentDoctorId = ref('');

const handleExceptionSubmitted = () => {
  showCreateException.value = false;
  fetchWeeklySchedule();
};

// Constants
const timeSlots = [
  '08:00', '08:30', '09:00', '09:30', '10:00', '10:30', '11:00', '11:30',
  '13:30', '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00',
  '17:30', '18:00', '18:30', '19:00', '19:30', '20:00'
];

// Computed
const todayFormatted = computed(() => {
  return new Date().toLocaleDateString('en-GB', { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' });
});

const currentWeekStart = computed(() => {
  const d = new Date(currentDate.value);
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Adjust when day is Sunday
  return new Date(d.setDate(diff));
});

const weekDays = computed(() => {
  const days = [];
  const start = new Date(currentWeekStart.value);
  const dayNames = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];
  
  for (let i = 0; i < 7; i++) {
    const date = new Date(start);
    date.setDate(start.getDate() + i);
    days.push({ name: dayNames[i], date: date });
  }
  return days;
});

const weekStartStr = computed(() => {
  return weekDays.value[0].date.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit' });
});

const weekEndStr = computed(() => {
  return weekDays.value[6].date.toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit' });
});

// Helpers
const isToday = (date: Date) => {
  const today = new Date();
  return date.getDate() === today.getDate() &&
         date.getMonth() === today.getMonth() &&
         date.getFullYear() === today.getFullYear();
};

const isWaitingOrInProgress = (status: string) => {
  return status === 'waiting' || status === 'in_progress';
};

const getBorderClass = (status: string) => {
  switch (status) {
    case 'waiting': return 'status-waiting border-warning';
    case 'in_progress': return 'status-in-progress border-primary';
    case 'ready_to_pay': return 'status-ready border-success opacity-75';
    case 'completed': return 'status-completed border-success bg-success bg-opacity-10';
    case 'cancelled': return 'status-cancelled border-danger opacity-50 text-decoration-line-through';
    default: return 'status-default border-info';
  }
};

const getEventsForCell = (date: Date, timeStr: string) => {
  // Format target datetime string to match local YYYY-MM-DDTHH:mm format roughly
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const dateStr = `${year}-${month}-${day}`;
  
  return events.value.filter(evt => {
    if (!evt.start || !evt.start.includes('T')) return false;
    
    // Safely parse "YYYY-MM-DDTHH:mm:ss" manually to avoid browser timezone shifting
    const [datePart, timePart] = evt.start.split('T');
    const [hourStr, minStr] = timePart.split(':');
    
    const evtDateStr = datePart;
    let evtTimeStr = `${hourStr}:${minStr}`;
    
    // Làm tròn giờ khám lẻ xuống khe 30 phút gần nhất (ví dụ: 17:19 -> 17:00, 17:45 -> 17:30)
    if (!timeSlots.includes(evtTimeStr)) {
      const min = parseInt(minStr);
      const roundedMin = min >= 30 ? '30' : '00';
      evtTimeStr = `${hourStr}:${roundedMin}`;
    }

    return evtDateStr === dateStr && evtTimeStr === timeStr;
  });
};

const getActualTime = (isoString: string) => {
  if (!isoString || !isoString.includes('T')) return '';
  const timePart = isoString.split('T')[1];
  return timePart.substring(0, 5); // "HH:mm"
};

// Actions
const prevWeek = () => {
  const d = new Date(currentDate.value);
  d.setDate(d.getDate() - 7);
  currentDate.value = d;
  fetchWeeklySchedule();
};

const nextWeek = () => {
  const d = new Date(currentDate.value);
  d.setDate(d.getDate() + 7);
  currentDate.value = d;
  fetchWeeklySchedule();
};

const goToToday = () => {
  currentDate.value = new Date();
  fetchWeeklySchedule();
};

// API
const fetchWeeklySchedule = async () => {
  loading.value = true;
  try {
    const start = new Date(currentWeekStart.value);
    start.setHours(0, 0, 0, 0);
    
    const end = new Date(currentWeekStart.value);
    end.setDate(end.getDate() + 6);
    end.setHours(23, 59, 59, 999);

    const res = await api.get('/doctor/weekly-schedule', {
      params: {
        start: start.toISOString(),
        end: end.toISOString(),
        targetDoctorId: selectedDoctor.value !== 'ALL' ? selectedDoctor.value : undefined
      }
    });
    
    events.value = res.data || [];
    console.log("=== THÔNG TIN API TRẢ VỀ ===", events.value);
  } catch (err: any) {
    console.error('Lỗi fetch weekly schedule:', err);
    events.value = [{ 
      id: 'error', 
      title: 'LỖI API: ' + (err.response?.status || err.message), 
      start: new Date().toISOString() 
    }];
  } finally {
    loading.value = false;
  }
};

const startTreatment = async (evt: any) => {
  try {
    const appointmentId = evt.id; // Fix: Lấy id từ evt.id thay vì extendedProps
    if (!appointmentId) return;

    const res = await api.post('/doctor/start-treatment', appointmentId);
    if (res.data.success) {
      localStorage.setItem('active_treatment_appointment_id', appointmentId.toString());
      localStorage.setItem('active_treatment_pet_id', evt.extendedProps?.petId?.toString() || '0');
      localStorage.setItem('active_treatment_pet_name', evt.extendedProps?.petName || 'Bệnh nhi');
      localStorage.setItem('active_treatment_customer_name', evt.extendedProps?.customerName || 'Khách vãng lai');
      const sName = (evt.extendedProps?.serviceName || evt.title || '').toLowerCase();
      const sType = sName.includes('tiêm') || sName.includes('vaccin') ? 'Vaccination' : 'Consultation';
      localStorage.setItem('active_treatment_service_type', sType);
      
      emit('switch-tab', 'medical-records');
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Không thể bắt đầu ca khám.');
  }
};

const continueTreatment = (evt: any) => {
  const appointmentId = evt.id; // Fix: Lấy id từ evt.id
  if (!appointmentId) return;

  localStorage.setItem('active_treatment_appointment_id', appointmentId.toString());
  localStorage.setItem('active_treatment_pet_id', evt.extendedProps?.petId?.toString() || '0');
  localStorage.setItem('active_treatment_pet_name', evt.extendedProps?.petName || 'Bệnh nhi');
  localStorage.setItem('active_treatment_customer_name', evt.extendedProps?.customerName || 'Khách vãng lai');
  const sName = (evt.extendedProps?.serviceName || evt.title || '').toLowerCase();
  const sType = sName.includes('tiêm') || sName.includes('vaccin') ? 'Vaccination' : 'Consultation';
  localStorage.setItem('active_treatment_service_type', sType);
  
  emit('switch-tab', 'medical-records');
};

const loadDoctors = async () => {
  if (!isAdmin.value) return;
  try {
    const res = await api.get('/doctors');
    doctors.value = res.data;
  } catch (err) {
    console.error('Lỗi load danh sách bác sĩ:', err);
  }
};

const loadCurrentUser = async () => {
  try {
    const res = await api.get('/dashboard');
    currentDoctorId.value = res.data.userId || '';
  } catch (e) {
    console.error('Lỗi lấy thông tin user:', e);
  }
};

onMounted(() => {
  loadCurrentUser();
  loadDoctors();
  fetchWeeklySchedule();
});
</script>

<style scoped>
/* Glassmorphism Classes */
.glass-panel {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(245, 158, 11, 0.12);
  box-shadow: 0 8px 32px 0 rgba(217, 119, 6, 0.04);
}

.backdrop-blur {
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
}

.border-glass { border: 1px solid rgba(245, 158, 11, 0.15) !important; }
.border-bottom-glass { border-bottom: 1px solid rgba(245, 158, 11, 0.1) !important; }
.border-start-glass { border-left: 1px solid rgba(245, 158, 11, 0.1) !important; }

.text-secondary-muted { color: #64748b; }

/* Premium Buttons */
.btn-premium-neon {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  border: none;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
}
.btn-premium-neon:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.45);
  color: white;
}

.btn-outline-glass {
  background: rgba(255, 255, 255, 0.6);
  color: #1e293b;
  border: 1px solid rgba(217, 119, 6, 0.2);
  transition: all 0.3s ease;
}
.btn-outline-glass:hover {
  background: rgba(255, 255, 255, 0.9);
  border-color: #f59e0b;
  color: #d97706;
}

.hover-lift {
  transition: transform 0.2s ease, background-color 0.2s;
}
.hover-lift:hover {
  transform: translateY(-2px);
  background-color: #f1f5f9 !important;
}

/* Day Circle */
.day-circle {
  width: 42px;
  height: 42px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.15rem;
  transition: all 0.3s;
}

.today-circle {
  background: linear-gradient(135deg, #f59e0b, #d97706) !important;
  color: white !important;
  border: none !important;
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4) !important;
}

/* Calendar Grid */
.calendar-grid-container {
  scrollbar-width: thin;
  scrollbar-color: rgba(245, 158, 11, 0.3) transparent;
  background: rgba(255, 255, 255, 0.4);
}

.calendar-table {
  table-layout: fixed;
  min-width: 900px;
  border-collapse: separate;
  border-spacing: 0;
}

.glass-header th {
  background: rgba(255, 255, 255, 0.85) !important;
  backdrop-filter: blur(12px);
}

.calendar-table td {
  height: 95px;
  vertical-align: top;
  transition: background-color 0.2s;
}
.calendar-table td:hover {
  background-color: rgba(245, 158, 11, 0.02);
}

.current-day-col {
  background: linear-gradient(to bottom, rgba(245, 158, 11, 0.04), transparent) !important;
}

.current-day-cell {
  background-color: rgba(245, 158, 11, 0.02) !important;
}

/* Appointment Cards */
.appointment-card {
  background: rgba(255, 255, 255, 0.9) !important;
  backdrop-filter: blur(4px);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-left-width: 4px !important;
  border-left-style: solid !important;
  box-shadow: 0 4px 12px rgba(0,0,0,0.04);
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  cursor: pointer;
  position: relative;
  overflow: hidden;
}

.appointment-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: linear-gradient(135deg, rgba(255,255,255,0.4), transparent);
  pointer-events: none;
}

.appointment-card:hover {
  transform: translateY(-3px) scale(1.02);
  box-shadow: 0 12px 24px rgba(245, 158, 11, 0.12) !important;
  z-index: 10;
}

/* Custom Status Border Colors */
.status-waiting { border-left-color: #f59e0b !important; }
.status-in-progress { border-left-color: #3b82f6 !important; }
.status-ready { border-left-color: #10b981 !important; }
.status-completed { border-left-color: #64748b !important; }
.status-cancelled { border-left-color: #ef4444 !important; }
.status-default { border-left-color: #0ea5e9 !important; }

/* Animations */
.pulse-icon {
  animation: pulseOpacity 2s infinite;
}

@keyframes pulseOpacity {
  0% { opacity: 1; }
  50% { opacity: 0.4; }
  100% { opacity: 1; }
}

.emergency-pulse {
  animation: borderPulse 2s infinite;
}

@keyframes borderPulse {
  0% { border-color: #dc3545; box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.4); }
  70% { border-color: #dc3545; box-shadow: 0 0 0 8px rgba(220, 53, 69, 0); }
  100% { border-color: #dc3545; box-shadow: 0 0 0 0 rgba(220, 53, 69, 0); }
}
</style>
