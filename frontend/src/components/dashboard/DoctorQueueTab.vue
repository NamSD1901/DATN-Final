<template>
  <div class="doctor-queue-tab h-100 d-flex flex-column bg-white rounded-4 shadow-sm p-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center border-bottom pb-3 mb-4">
      <div>
        <h4 class="fw-bold text-dark mb-1">
          <i class="bi bi-calendar-week-fill text-primary me-2"></i> Lịch làm việc
        </h4>
        <p class="text-muted mb-0 small">Hôm nay: {{ todayFormatted }}</p>
      </div>
      <div>
        <span class="badge bg-danger bg-opacity-10 text-danger rounded-pill px-3 py-2 border border-danger border-opacity-25 me-3">
          <i class="bi bi-circle-fill small me-1"></i> Bác sĩ điều trị
        </span>
      </div>
    </div>

    <!-- Controls -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div class="d-flex align-items-center bg-light rounded-pill p-1 shadow-sm border">
        <button class="btn btn-sm btn-white rounded-circle" @click="prevWeek">
          <i class="bi bi-chevron-left"></i>
        </button>
        <div class="fw-bold text-dark px-4">
          Tuần: {{ weekStartStr }} – {{ weekEndStr }}
        </div>
        <button class="btn btn-sm btn-white rounded-circle" @click="nextWeek">
          <i class="bi bi-chevron-right"></i>
        </button>
      </div>

      <div class="d-flex gap-2">
        <select v-if="isAdmin" v-model="selectedDoctor" @change="fetchWeeklySchedule" class="form-select border-warning rounded-pill px-3 py-1 shadow-sm" style="width: 180px;">
          <option value="ALL">Tất cả bác sĩ</option>
          <option v-for="doc in doctors" :key="doc.id" :value="doc.id">Bs. {{ doc.fullName }}</option>
        </select>
        <button class="btn btn-outline-secondary rounded-pill px-4 fw-bold bg-white" @click="goToToday">Hôm nay</button>
        <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" @click="fetchWeeklySchedule" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
          <i v-else class="bi bi-arrow-clockwise me-1"></i> Làm mới
        </button>
      </div>
    </div>

    <!-- Calendar Grid -->
    <div class="calendar-grid-container flex-grow-1 overflow-auto border rounded-3 position-relative">
      
      <!-- Loading Overlay -->
      <div v-if="loading" class="position-absolute top-0 start-0 w-100 h-100 bg-white bg-opacity-75 d-flex justify-content-center align-items-center z-3">
        <div class="spinner-border text-primary" style="width: 3rem; height: 3rem;" role="status"></div>
      </div>

      <div class="p-3 bg-light border-bottom text-danger fw-bold" v-if="events && events.length > 0">
        DEBUG: Có {{ events.length }} ca khám. Ca đầu tiên: {{ events[0].title }} lúc {{ events[0].start }}
      </div>
      <div class="p-3 bg-light border-bottom text-danger fw-bold" v-else>
        DEBUG: API trả về 0 ca khám trong tuần này!
      </div>

      <table class="table table-bordered mb-0 calendar-table">
        <thead class="bg-light position-sticky top-0 z-2">
          <tr>
            <th class="text-center text-muted small fw-bold py-3 align-middle bg-light" style="width: 80px; min-width: 80px;">GIỜ</th>
            <th v-for="(day, index) in weekDays" :key="index" 
                class="text-center py-3 bg-light"
                :class="{ 'current-day-col': isToday(day.date) }">
              <div class="d-flex flex-column align-items-center">
                <span class="text-muted small fw-bold mb-1">{{ day.name }}</span>
                <div class="day-circle shadow-sm" :class="isToday(day.date) ? 'bg-primary text-white' : 'bg-white text-dark border'">
                  {{ day.date.getDate() }}
                </div>
              </div>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="time in timeSlots" :key="time">
            <td class="text-center text-muted small fw-bold align-middle bg-light border-end">{{ time }}</td>
            <td v-for="(day, index) in weekDays" :key="index" class="p-1 position-relative" :class="{ 'current-day-cell': isToday(day.date) }">
              <!-- Render Events for this cell -->
              <div class="d-flex flex-column gap-1">
                <template v-for="evt in getEventsForCell(day.date, time)" :key="evt.id">
                  
                  <!-- Appointment Card -->
                  <div class="appointment-card p-2 rounded-3 border shadow-sm bg-white"
                       :class="[getBorderClass(evt.extendedProps?.status), { 'emergency-pulse': evt.extendedProps?.isEmergency }]">
                    
                    <div class="d-flex justify-content-between align-items-start mb-1">
                      <strong class="text-dark small d-block text-truncate">{{ evt.extendedProps?.petName || 'Thú cưng' }}</strong>
                      <span v-if="evt.extendedProps?.isEmergency" class="badge bg-danger p-1" style="font-size: 0.5rem;">CẤP CỨU</span>
                    </div>
                    
                    <div class="text-muted text-truncate" style="font-size: 0.7rem;">
                      {{ evt.extendedProps?.serviceName || evt.title }}
                    </div>

                    <!-- Action Button (Only show if status is waiting or in_progress, and date is today) -->
                    <div class="mt-2" v-if="isToday(day.date) && isWaitingOrInProgress(evt.extendedProps?.status)">
                      <button v-if="evt.extendedProps?.status === 'waiting'" 
                              class="btn btn-sm btn-primary w-100 rounded-pill py-1 d-flex justify-content-center align-items-center" 
                              style="font-size: 0.7rem;"
                              @click.stop="startTreatment(evt)">
                        <i class="bi bi-play-fill me-1"></i> Tiến hành khám
                      </button>
                      <button v-if="evt.extendedProps?.status === 'in_progress'" 
                              class="btn btn-sm btn-outline-primary w-100 rounded-pill py-1 d-flex justify-content-center align-items-center" 
                              style="font-size: 0.7rem;"
                              @click.stop="continueTreatment(evt)">
                        <i class="bi bi-pencil-square me-1"></i> Khám tiếp
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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api';

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
    case 'waiting': return 'border-warning border-start border-4';
    case 'in_progress': return 'border-primary border-start border-4';
    case 'ready_to_pay': return 'border-success border-start border-4 opacity-75';
    case 'completed': return 'border-secondary border-start border-4 opacity-50';
    case 'cancelled': return 'border-danger border-start border-4 opacity-50 text-decoration-line-through';
    default: return 'border-info border-start border-4';
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
    const evtTimeStr = `${hourStr}:${minStr}`;
    
    return evtDateStr === dateStr && evtTimeStr === timeStr;
  });
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

onMounted(() => {
  loadDoctors();
  fetchWeeklySchedule();
});
</script>

<style scoped>
.day-circle {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.1rem;
}

.calendar-grid-container {
  scrollbar-width: thin;
  scrollbar-color: #cbd5e1 transparent;
}

.calendar-table {
  table-layout: fixed;
  min-width: 900px;
}

.calendar-table th, .calendar-table td {
  border-color: #e9ecef;
}

.calendar-table td {
  height: 90px;
  vertical-align: top;
}

.current-day-col {
  background-color: #f8faff !important;
}

.current-day-cell {
  background-color: #f8faff !important;
}

.appointment-card {
  transition: all 0.2s ease;
  cursor: pointer;
}
.appointment-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 15px rgba(0,0,0,0.1) !important;
  z-index: 10;
}

.emergency-pulse {
  animation: borderPulse 2s infinite;
}

@keyframes borderPulse {
  0% { border-color: #dc3545; box-shadow: 0 0 0 0 rgba(220, 53, 69, 0.4); }
  70% { border-color: #dc3545; box-shadow: 0 0 0 6px rgba(220, 53, 69, 0); }
  100% { border-color: #dc3545; box-shadow: 0 0 0 0 rgba(220, 53, 69, 0); }
}
</style>
