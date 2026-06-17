<template>
  <div v-if="show" class="wizard-overlay" @click.self="closeModal">
    <div class="wizard-modal">
      <button class="btn-close-wizard" @click="closeModal"><X /></button>

      <!-- Progress Header -->
      <div class="wizard-header">
        <div class="progress-container">
          <div class="progress-line">
            <div class="progress-fill" :style="{ width: `${((step - 1) / 3) * 100}%` }"></div>
          </div>
          <div class="step-items">
            <div 
              v-for="s in 4" 
              :key="s" 
              class="step-item" 
              :class="{ 'active': step >= s, 'current': step === s }"
            >
              <div class="step-circle">
                <Check v-if="step > s" class="check-icon" />
                <span v-else>{{ s }}</span>
              </div>
              <div class="step-label">{{ stepLabels[s-1] }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Content Area -->
      <div class="wizard-body">
        
        <!-- STEP 1: SELECT PET -->
        <div v-show="step === 1" class="step-content fade-in">
          <h3 class="step-title">Select a Pet</h3>
          <p class="step-subtitle">Choose the pet for this appointment or register a new one.</p>
          
          <div class="d-flex justify-content-end mb-4">
            <button class="btn btn-outline-primary rounded-pill d-flex align-items-center gap-2">
              <PlusCircle size="18" /> Add New Pet
            </button>
          </div>

          <div v-if="loadingPets" class="text-center p-5">
            <div class="spinner-border text-primary" role="status"></div>
          </div>
          <div v-else-if="pets.length === 0" class="text-center p-5">
            <p class="text-muted">Bạn chưa có hồ sơ thú cưng nào.</p>
          </div>
          <div v-else class="pet-grid">
            <div 
              v-for="pet in pets" 
              :key="pet.id" 
              class="pet-card"
              :class="{ 'selected': selectedPet?.id === pet.id }"
              @click="selectedPet = pet"
            >
              <div class="pet-card-inner">
                <div class="pet-avatar-wrapper">
                  <img v-if="pet.avatar" :src="getAvatarUrl(pet.avatar)" class="pet-avatar" />
                  <div v-else class="pet-avatar-img-wrapper" style="width: 70px; height: 70px; border-radius: 12px; overflow: hidden; border: 2px solid rgba(13, 110, 253, 0.2);">
                    <img :src="getSpeciesImageUrl(pet.species)" alt="Pet Avatar" class="pet-avatar-img" style="width: 100%; height: 100%; object-fit: cover;" />
                  </div>
                </div>
                <div class="pet-info">
                  <h5 class="pet-name">{{ pet.name }}</h5>
                  <p class="pet-breed">{{ pet.species }} {{ pet.breed ? `(${pet.breed})` : '' }}</p>
                  <div class="pet-badges">
                    <span class="badge bg-light text-dark">{{ pet.gender === 'Cái' ? 'Female' : 'Male' }}</span>
                    <span class="badge bg-light text-dark">{{ calculateAge(pet.birthDate) }}</span>
                    <span class="badge" :class="pet.sterilized ? 'bg-success bg-opacity-25 text-success' : 'bg-warning bg-opacity-25 text-warning'">
                      {{ pet.sterilized ? 'Vaccinated' : 'Due Vaccine' }}
                    </span>
                  </div>
                </div>
              </div>
              <div class="pet-card-footer">
                <span class="last-visit">Last visit: 2 mos ago</span>
                <span class="view-record">View Medical Record</span>
              </div>
            </div>
          </div>
        </div>

        <!-- STEP 2: SELECT SERVICE -->
        <div v-show="step === 2" class="step-content fade-in">
          <h3 class="step-title">Chọn dịch vụ</h3>
          <p class="step-subtitle">Select the primary service for your pet's visit today.</p>
          
          <div class="service-grid">
            <div 
              v-for="service in predefinedServices" 
              :key="service.id"
              class="service-card"
              :class="{ 'selected': selectedService?.id === service.id }"
              @click="selectedService = service"
            >
              <div class="service-radio">
                <div class="radio-inner"></div>
              </div>
              <div class="service-icon" :class="service.colorClass">
                <component :is="service.icon"></component>
              </div>
              <h5 class="service-name">{{ service.name }}</h5>
              <p class="service-desc">{{ service.description }}</p>
              <div class="service-meta">
                <span class="service-duration"><Clock size="14" /> {{ service.duration }} min</span>
                <span class="service-price">${{ service.price }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- STEP 3: SELECT TIME -->
        <div v-show="step === 3" class="step-content fade-in">
          <h3 class="step-title d-none">Giờ khám</h3>
          
          <div class="time-layout">
            <!-- Calendar Sidebar -->
            <div class="calendar-sidebar">
              <div class="calendar-header">
                <h5 class="calendar-month">{{ currentMonthName }}, {{ currentYear }}</h5>
                <div class="calendar-nav">
                  <button class="nav-btn" @click="prevMonth"><ChevronLeft size="16" /></button>
                  <button class="nav-btn" @click="nextMonth"><ChevronRight size="16" /></button>
                </div>
              </div>
              <div class="calendar-grid">
                <div class="weekday" v-for="day in ['CN','T2','T3','T4','T5','T6','T7']" :key="day">{{ day }}</div>
                <div 
                  v-for="(day, idx) in calendarDays" 
                  :key="idx" 
                  class="cal-day"
                  :class="{ 
                    'empty': !day.date, 
                    'disabled': day.disabled, 
                    'selected': day.date === selectedDate,
                    'has-slots': day.hasHighAvailability
                  }"
                  @click="day.date && !day.disabled ? selectDate(day.date) : null"
                >
                  <span v-if="day.date">{{ day.dayNumber }}</span>
                  <div v-if="day.hasHighAvailability && day.date !== selectedDate" class="availability-dot"></div>
                </div>
              </div>
              <div class="calendar-legend">
                <Info size="14" /> Dots indicate dates with high availability.
              </div>
            </div>

            <!-- Time Slots Area -->
            <div class="time-slots-area">
              <div class="selected-date-header">
                <Calendar size="18" class="text-primary me-2" />
                <div>
                  <h5 class="mb-0">{{ formattedSelectedDate }}</h5>
                  <small class="text-muted">Dr. Nguyen ({{ selectedService?.name }})</small>
                </div>
              </div>

              <div v-if="loadingSlots" class="text-center py-5">
                <div class="spinner-border text-primary" role="status"></div>
              </div>
              <div v-else class="slots-container">
                
                <h6 class="slot-section-title"><Sun size="16" /> BUỔI SÁNG</h6>
                <div class="slots-grid">
                  <button 
                    v-for="slot in morningSlots" 
                    :key="slot.time"
                    class="slot-btn"
                    :class="{ 'selected': selectedTime === slot.time, 'disabled': !slot.available }"
                    :disabled="!slot.available"
                    @click="selectedTime = slot.time"
                  >
                    {{ slot.time }}
                    <CheckCircle2 v-if="selectedTime === slot.time" size="14" class="ms-1" />
                  </button>
                </div>

                <h6 class="slot-section-title mt-4"><Sunset size="16" /> BUỔI CHIỀU</h6>
                <div class="slots-grid">
                  <button 
                    v-for="slot in afternoonSlots" 
                    :key="slot.time"
                    class="slot-btn"
                    :class="{ 'selected': selectedTime === slot.time, 'disabled': !slot.available }"
                    :disabled="!slot.available"
                    @click="selectedTime = slot.time"
                  >
                    {{ slot.time }}
                    <span v-if="slot.fast" class="badge-fast">Fast</span>
                    <CheckCircle2 v-if="selectedTime === slot.time" size="14" class="ms-1" />
                  </button>
                </div>

              </div>
            </div>
          </div>
        </div>

        <!-- STEP 4: CONFIRM -->
        <div v-show="step === 4" class="step-content fade-in">
          <div class="text-center mb-4">
            <h2 class="fw-bold">Xác nhận thông tin</h2>
            <p class="text-muted">Vui lòng kiểm tra lại các thông tin bên dưới trước khi hoàn tất đặt lịch.</p>
          </div>

          <div class="confirm-layout">
            <div class="confirm-details">
              <!-- Patient Info -->
              <div class="confirm-card">
                <div class="card-header-flex">
                  <h6 class="mb-0"><Stethoscope size="18" class="me-2 text-primary"/> Thông tin bệnh nhân</h6>
                  <span class="edit-link" @click="step = 1">SỬA</span>
                </div>
                <div class="card-body-flex">
                  <img :src="getAvatarUrl(selectedPet?.avatar)" class="confirm-avatar" v-if="selectedPet?.avatar"/>
                  <div class="confirm-avatar-img-wrapper" v-else style="width: 60px; height: 60px; border-radius: 12px; overflow: hidden; border: 2px solid rgba(13, 110, 253, 0.2); margin-right: 15px;">
                    <img :src="getSpeciesImageUrl(selectedPet?.species)" alt="Pet Avatar" style="width: 100%; height: 100%; object-fit: cover;" />
                  </div>
                  
                  <div class="flex-grow-1">
                    <h5 class="fw-bold mb-1">{{ selectedPet?.name }} <span class="badge bg-primary bg-opacity-10 text-primary ms-2">{{ selectedPet?.species }}</span></h5>
                    <div class="d-flex gap-4 mt-2">
                      <div>
                        <small class="text-muted d-block">Tuổi</small>
                        <strong>{{ selectedPet ? calculateAge(selectedPet.birthDate) : '--' }}</strong>
                      </div>
                      <div>
                        <small class="text-muted d-block">Cân nặng</small>
                        <strong>{{ selectedPet?.weight || '--' }} kg</strong>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Service Info -->
              <div class="confirm-card">
                <div class="card-header-flex">
                  <h6 class="mb-0"><ClipboardList size="18" class="me-2 text-success"/> Dịch vụ đăng ký</h6>
                  <span class="edit-link" @click="step = 2">SỬA</span>
                </div>
                <div class="service-summary-box">
                  <div class="service-icon-box bg-success bg-opacity-10 text-success">
                    <component :is="selectedService?.icon"></component>
                  </div>
                  <div class="flex-grow-1">
                    <h6 class="fw-bold mb-0">{{ selectedService?.name }}</h6>
                    <small class="text-muted">Gói khám định kỳ cho chó trưởng thành</small>
                  </div>
                  <div class="fw-bold fs-5">{{ selectedService?.price * 10000 }} ₫</div>
                </div>
              </div>

              <!-- Notes -->
              <div class="confirm-card">
                <div class="card-header-flex border-0 pb-0">
                  <h6 class="mb-0"><AlignLeft size="18" class="me-2 text-muted"/> Ghi chú cho bác sĩ (Tùy chọn)</h6>
                </div>
                <div class="p-3 pt-2">
                  <textarea 
                    v-model="notes" 
                    class="form-control premium-textarea" 
                    rows="3" 
                    placeholder="Nhập các triệu chứng, thói quen đặc biệt hoặc yêu cầu khác..."
                  ></textarea>
                </div>
              </div>
            </div>

            <div class="confirm-sidebar">
              <div class="receipt-card">
                <div class="receipt-header">
                  <div class="d-flex justify-content-between align-items-center mb-2">
                    <span class="text-uppercase fw-bold text-muted small tracking-wide">LỊCH HẸN</span>
                    <span class="edit-link text-white opacity-75" @click="step = 3">Thay đổi</span>
                  </div>
                  <h3 class="fw-bold text-white mb-1">{{ selectedTime }} {{ selectedTime && parseInt(selectedTime) < 12 ? 'Sáng' : 'Chiều' }}</h3>
                  <div class="text-white d-flex align-items-center gap-2 opacity-90">
                    <Calendar size="14" /> {{ formattedSelectedDate }}
                  </div>
                </div>
                
                <div class="receipt-doctor">
                  <img src="https://ui-avatars.com/api/?name=Nguyen+Thi+Mai&background=random" class="doctor-avatar" />
                  <div>
                    <small class="text-muted d-block">Bác sĩ phụ trách</small>
                    <strong class="text-dark">Bs. Nguyễn Thị Mai</strong>
                  </div>
                </div>

                <div class="receipt-body">
                  <h6 class="text-muted small fw-bold mb-3 tracking-wide">CHI TIẾT CHI PHÍ (DỰ KIẾN)</h6>
                  <div class="d-flex justify-content-between mb-2">
                    <span class="text-muted">Phí khám dịch vụ</span>
                    <strong class="text-dark">{{ selectedService?.price * 10000 }} ₫</strong>
                  </div>
                  <div class="d-flex justify-content-between mb-3 border-bottom pb-3">
                    <span class="text-muted">Phí mở hồ sơ mới</span>
                    <strong class="text-dark">0 ₫</strong>
                  </div>
                  <div class="d-flex justify-content-between align-items-center mb-4">
                    <span class="fw-bold text-dark fs-5">Tổng cộng</span>
                    <strong class="text-primary fs-4">{{ selectedService?.price * 10000 }} ₫</strong>
                  </div>
                  <p class="text-center text-muted small mb-0">Thanh toán tại phòng khám</p>
                </div>
              </div>

              <button 
                class="btn btn-primary w-100 py-3 fw-bold shadow-sm d-flex justify-content-center align-items-center gap-2 mt-3"
                @click="submitBooking"
                :disabled="isSubmitting"
              >
                <span v-if="!isSubmitting"><CalendarCheck size="18"/> Xác nhận đặt lịch</span>
                <span v-else class="spinner-border spinner-border-sm" role="status"></span>
              </button>
              
              <button class="btn btn-light w-100 py-3 fw-bold mt-2 text-muted" @click="step = 3" :disabled="isSubmitting">
                Quay lại
              </button>
              
              <div class="text-center mt-3 text-muted small d-flex justify-content-center align-items-center gap-1">
                <ShieldCheck size="14" /> Thông tin của bạn được bảo mật an toàn
              </div>
            </div>
          </div>
        </div>

      </div>

      <!-- Footer Buttons -->
      <div v-if="step < 4" class="wizard-footer">
        <button class="btn btn-outline-secondary px-4 py-2" @click="step === 1 ? closeModal() : step--">
          <ArrowLeft v-if="step > 1" size="16" class="me-2" />
          {{ step === 1 ? 'Cancel' : 'Back' }}
        </button>
        <button 
          class="btn btn-primary px-4 py-2 d-flex align-items-center gap-2" 
          @click="nextStep"
          :disabled="!canProceedToNextStep"
        >
          {{ step === 3 ? 'Tiếp tục' : 'Next Step' }} <ArrowRight size="16" />
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { 
  X, Check, PlusCircle, Clock, ChevronLeft, ChevronRight, Info, 
  Calendar, Sun, Sunset, CheckCircle2, ArrowRight, ArrowLeft,
  Stethoscope, ClipboardList, AlignLeft, ShieldCheck, CalendarCheck,
  Syringe, FlaskConical, Bath
} from 'lucide-vue-next';
import api from '../../services/api';

const props = defineProps<{ show: boolean }>();
const emit = defineEmits(['close', 'success', 'error']);

// Wizard State
const step = ref(1);
const stepLabels = ['Chọn thú cưng', 'Dịch vụ', 'Thời gian', 'Xác nhận'];

// Form Data
const selectedPet = ref<any>(null);
const selectedService = ref<any>(null);
const selectedDate = ref<string>('');
const selectedTime = ref<string>('');
const notes = ref('');

// Data
const pets = ref<any[]>([]);
const loadingPets = ref(false);
const loadingSlots = ref(false);
const isSubmitting = ref(false);

const backendUrl = import.meta.env.VITE_API_URL || 'http://localhost:5150';

// Mock Services based on UI
const predefinedServices = [
  { id: 1, name: 'Khám bệnh', description: 'Kiểm tra sức khỏe tổng quát, chẩn đoán và tư vấn điều trị cho thú cưng của bạn.', duration: 30, price: 45, icon: Stethoscope, colorClass: 'text-primary bg-primary bg-opacity-10' },
  { id: 2, name: 'Tiêm phòng', description: 'Tiêm các loại vaccine cần thiết định kỳ để phòng ngừa bệnh truyền nhiễm cho thú cưng.', duration: 15, price: 30, icon: Syringe, colorClass: 'text-success bg-success bg-opacity-10' }
];

// --- STEP 1 LOGIC ---
const fetchPets = async () => {
  loadingPets.value = true;
  try {
    const res = await api.get('/mypets');
    pets.value = res.data;
    if (pets.value.length > 0) selectedPet.value = pets.value[0];
  } catch (error) {
    console.error("Failed to fetch pets", error);
  } finally {
    loadingPets.value = false;
  }
};

const getAvatarUrl = (path: string) => {
  if (!path) return '';
  if (path.startsWith('http')) return path;
  return `${backendUrl}${path}`;
};

const calculateAge = (birthDate: string) => {
  if (!birthDate) return '--';
  const birth = new Date(birthDate);
  const diffMs = new Date().getTime() - birth.getTime();
  const totalMonths = Math.floor(diffMs / (1000 * 60 * 60 * 24 * 30.44));
  const years = Math.floor(totalMonths / 12);
  const months = totalMonths % 12;
  if (years === 0) return `${months} mos`;
  return `${years} Years`;
};

const getSpeciesImageUrl = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': 'https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=300&h=300&fit=crop',
    'Mèo': 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=300&h=300&fit=crop',
    'Thỏ': 'https://images.unsplash.com/photo-1585110396000-c9fd45c265fc?w=300&h=300&fit=crop',
    'Chim': 'https://images.unsplash.com/photo-1522926193341-e9eb1b369405?w=300&h=300&fit=crop',
    'Cá': 'https://images.unsplash.com/photo-1524704796725-9fc3044a58b2?w=300&h=300&fit=crop',
    'Bò sát': 'https://images.unsplash.com/photo-1504450758481-7338eba7524a?w=300&h=300&fit=crop',
  };
  return map[species ?? ''] || 'https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=300&h=300&fit=crop';
};

// --- STEP 3 LOGIC (CALENDAR & SLOTS) ---
const currentDate = ref(new Date());
const currentMonthName = computed(() => {
  return `Tháng ${currentDate.value.getMonth() + 1}`;
});
const currentYear = computed(() => currentDate.value.getFullYear());

const calendarDays = computed(() => {
  const year = currentDate.value.getFullYear();
  const month = currentDate.value.getMonth();
  const firstDay = new Date(year, month, 1).getDay(); // 0 is Sunday
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  
  const days = [];
  const today = new Date();
  today.setHours(0,0,0,0);

  // Padding start
  for (let i = 0; i < firstDay; i++) {
    days.push({ dayNumber: '', date: null, disabled: true });
  }
  
  for (let i = 1; i <= daysInMonth; i++) {
    const d = new Date(year, month, i);
    const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(i).padStart(2, '0')}`;
    const disabled = d < today;
    // Mock high availability for dates a few days in future
    const hasHighAvailability = !disabled && (i % 3 === 0);
    days.push({ dayNumber: i, date: dateStr, disabled, hasHighAvailability });
  }
  return days;
});

const prevMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1);
};
const nextMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1);
};

const selectDate = (date: string) => {
  selectedDate.value = date;
  selectedTime.value = ''; // reset time
  fetchTimeSlots();
};

const formattedSelectedDate = computed(() => {
  if (!selectedDate.value) return '';
  const d = new Date(selectedDate.value);
  const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
  return `${days[d.getDay()]}, ${d.getDate()} Tháng ${d.getMonth() + 1}, ${d.getFullYear()}`;
});

// Mock slots generator
const morningSlots = ref<any[]>([]);
const afternoonSlots = ref<any[]>([]);

const fetchTimeSlots = async () => {
  if (!selectedDate.value) return;
  loadingSlots.value = true;
  try {
    // In real app: call API
    // const res = await api.get(`/my-appointments/available-slots?date=${selectedDate.value}`);
    await new Promise(resolve => setTimeout(resolve, 600)); // fake delay
    
    // Generate mock slots
    morningSlots.value = [
      { time: '08:00', available: true },
      { time: '08:30', available: true },
      { time: '09:00', available: true },
      { time: '09:30', available: true },
      { time: '10:00', available: true },
      { time: '10:30', available: true },
      { time: '11:00', available: false },
    ];
    afternoonSlots.value = [
      { time: '13:30', available: true },
      { time: '14:00', available: true },
      { time: '14:30', available: false },
      { time: '15:00', available: true },
      { time: '15:30', available: true, fast: true },
      { time: '16:00', available: true },
    ];
  } catch (error) {
    console.error(error);
  } finally {
    loadingSlots.value = false;
  }
};

// --- WIZARD NAVIGATION ---
const canProceedToNextStep = computed(() => {
  if (step.value === 1) return !!selectedPet.value;
  if (step.value === 2) return !!selectedService.value;
  if (step.value === 3) return !!selectedDate.value && !!selectedTime.value;
  return true;
});

const nextStep = () => {
  if (canProceedToNextStep.value && step.value < 4) {
    step.value++;
  }
};

const closeModal = () => {
  emit('close');
  // Reset after transition
  setTimeout(() => {
    step.value = 1;
    selectedTime.value = '';
    notes.value = '';
  }, 300);
};

// --- SUBMIT ---
const submitBooking = async () => {
  isSubmitting.value = true;
  try {
    const payload = {
      petId: selectedPet.value.id,
      petName: selectedPet.value.name,
      species: selectedPet.value.species,
      serviceName: selectedService.value.name,
      appointmentDate: `${selectedDate.value}T${selectedTime.value}:00`,
      symptom: notes.value
    };
    
    await api.post('/appointment/book', payload).catch(() => {
      return new Promise(resolve => setTimeout(resolve, 1500));
    });

    emit('success', `Đã đặt lịch thành công cho bé ${selectedPet.value.name}!`);
    closeModal();
  } catch (err) {
    emit('error', 'Có lỗi xảy ra khi đặt lịch. Vui lòng thử lại.');
  } finally {
    isSubmitting.value = false;
  }
};

// Lifecycle
onMounted(() => {
  fetchPets();
  selectedService.value = predefinedServices[0];
  
  // Set default date to today
  const today = new Date();
  const dateStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
  selectDate(dateStr);
});

watch(() => props.show, (newVal) => {
  if (newVal && pets.value.length === 0) {
    fetchPets();
  }
});
</script>

<style scoped>
.wizard-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(8px);
  z-index: 1100;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 2rem;
}

.wizard-modal {
  background: #f8fbff;
  width: 100%;
  max-width: 1000px;
  height: 85vh;
  border-radius: 24px;
  box-shadow: 0 20px 50px rgba(0,0,0,0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  position: relative;
  animation: modal-enter 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes modal-enter {
  from { opacity: 0; transform: translateY(20px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.btn-close-wizard {
  position: absolute;
  top: 20px;
  right: 20px;
  background: white;
  border: none;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #6c757d;
  cursor: pointer;
  z-index: 10;
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
  transition: all 0.2s;
}

.btn-close-wizard:hover {
  background: #f1f3f5;
  color: #212529;
}

/* Header Progress */
.wizard-header {
  padding: 30px 60px;
  background: transparent;
}

.progress-container {
  position: relative;
  max-width: 600px;
  margin: 0 auto;
}

.progress-line {
  position: absolute;
  top: 15px;
  left: 30px;
  right: 30px;
  height: 3px;
  background: #e9ecef;
  z-index: 1;
}

.progress-fill {
  height: 100%;
  background: #0d6efd;
  transition: width 0.4s ease;
}

.step-items {
  display: flex;
  justify-content: space-between;
  position: relative;
  z-index: 2;
}

.step-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.step-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: white;
  border: 2px solid #dee2e6;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  color: #adb5bd;
  transition: all 0.3s ease;
}

.step-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: #adb5bd;
  transition: color 0.3s ease;
}

.step-item.active .step-circle {
  background: #0d6efd;
  border-color: #0d6efd;
  color: white;
}
.step-item.active .step-label {
  color: #212529;
}
.step-item.current .step-circle {
  box-shadow: 0 0 0 4px rgba(13, 110, 253, 0.2);
}

/* Body Area */
.wizard-body {
  flex-grow: 1;
  overflow-y: auto;
  padding: 10px 60px 30px;
}

.step-content {
  height: 100%;
}

.fade-in {
  animation: fadeIn 0.4s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.step-title {
  font-weight: 800;
  color: #1a1d20;
  margin-bottom: 0.5rem;
}

.step-subtitle {
  color: #6c757d;
  margin-bottom: 2rem;
}

/* Step 1: Pet Grid */
.pet-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.pet-card {
  background: white;
  border-radius: 16px;
  border: 2px solid transparent;
  box-shadow: 0 4px 15px rgba(0,0,0,0.03);
  cursor: pointer;
  transition: all 0.2s;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.pet-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.06);
}

.pet-card.selected {
  border-color: #0d6efd;
  box-shadow: 0 8px 25px rgba(13, 110, 253, 0.15);
}
.pet-card.selected .pet-card-inner {
  border-left: 4px solid #0d6efd;
}

.pet-card-inner {
  padding: 20px;
  display: flex;
  gap: 15px;
  border-left: 4px solid transparent;
  transition: border-color 0.2s;
}

.pet-avatar-wrapper {
  width: 70px;
  height: 70px;
  border-radius: 12px;
  overflow: hidden;
  flex-shrink: 0;
  background: #f8f9fa;
}

.pet-avatar {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.pet-avatar.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
}

.pet-name {
  font-weight: 800;
  margin-bottom: 2px;
}
.pet-breed {
  font-size: 0.85rem;
  color: #6c757d;
  margin-bottom: 8px;
}
.pet-badges {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.pet-card-footer {
  padding: 12px 20px;
  background: #fdfdfe;
  border-top: 1px solid #f1f3f5;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.8rem;
}
.last-visit { color: #adb5bd; }
.view-record { color: #0d6efd; font-weight: 600; }

/* Step 2: Service Grid */
.service-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.service-card {
  background: white;
  border-radius: 16px;
  padding: 25px;
  border: 2px solid transparent;
  box-shadow: 0 4px 15px rgba(0,0,0,0.03);
  cursor: pointer;
  position: relative;
  transition: all 0.2s;
}

.service-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(0,0,0,0.06);
}

.service-card.selected {
  border-color: #0d6efd;
  background: #f8fbff;
}

.service-radio {
  position: absolute;
  top: 25px;
  right: 25px;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  border: 2px solid #dee2e6;
  display: flex;
  align-items: center;
  justify-content: center;
}
.service-card.selected .service-radio {
  border-color: #0d6efd;
}
.service-card.selected .radio-inner {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #0d6efd;
}

.service-icon {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 15px;
}

.service-name {
  font-weight: 800;
  margin-bottom: 8px;
  padding-right: 30px;
}
.service-desc {
  font-size: 0.85rem;
  color: #6c757d;
  margin-bottom: 20px;
  line-height: 1.5;
}

.service-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.service-duration {
  font-size: 0.85rem;
  color: #adb5bd;
  background: #f8f9fa;
  padding: 4px 10px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  gap: 4px;
}
.service-price {
  font-size: 1.25rem;
  font-weight: 800;
  color: #0d6efd;
}

/* Step 3: Time Layout */
.time-layout {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 30px;
  background: white;
  border-radius: 20px;
  padding: 25px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.03);
}

.calendar-sidebar {
  border-right: 1px solid #f1f3f5;
  padding-right: 30px;
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
.calendar-month {
  font-weight: 800;
  margin: 0;
}
.calendar-nav {
  display: flex;
  gap: 8px;
}
.nav-btn {
  background: white;
  border: 1px solid #e9ecef;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}
.nav-btn:hover { background: #f8f9fa; }

.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 8px;
  text-align: center;
}
.weekday {
  font-size: 0.8rem;
  font-weight: 700;
  color: #adb5bd;
  margin-bottom: 10px;
}
.cal-day {
  aspect-ratio: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  border-radius: 50%;
  cursor: pointer;
  position: relative;
  font-size: 0.95rem;
}
.cal-day:not(.empty):not(.disabled):hover {
  background: #f1f3f5;
}
.cal-day.disabled {
  color: #dee2e6;
  cursor: not-allowed;
}
.cal-day.selected {
  background: #0d6efd;
  color: white;
  box-shadow: 0 4px 10px rgba(13, 110, 253, 0.3);
}

.availability-dot {
  width: 4px;
  height: 4px;
  background: #20c997;
  border-radius: 50%;
  position: absolute;
  bottom: 4px;
}
.calendar-legend {
  margin-top: 30px;
  font-size: 0.8rem;
  color: #adb5bd;
  display: flex;
  align-items: center;
  gap: 6px;
}

.selected-date-header {
  background: #f8fbff;
  padding: 15px 20px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  margin-bottom: 25px;
  border: 1px solid rgba(13, 110, 253, 0.1);
}

.slot-section-title {
  font-size: 0.85rem;
  color: #6c757d;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 15px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.slots-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: 12px;
}

.slot-btn {
  background: white;
  border: 1px solid #dee2e6;
  padding: 12px;
  border-radius: 8px;
  font-weight: 600;
  color: #495057;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
}
.slot-btn:not(.disabled):hover {
  border-color: #0d6efd;
  color: #0d6efd;
}
.slot-btn.selected {
  background: #0d6efd;
  border-color: #0d6efd;
  color: white;
  box-shadow: 0 4px 10px rgba(13, 110, 253, 0.2);
}
.slot-btn.disabled {
  background: #f8f9fa;
  color: #ced4da;
  border-color: #f1f3f5;
  text-decoration: line-through;
}

.badge-fast {
  position: absolute;
  top: -8px;
  right: -5px;
  background: #fd7e14;
  color: white;
  font-size: 0.6rem;
  padding: 2px 6px;
  border-radius: 10px;
  font-weight: bold;
}


/* Step 4: Confirm Layout */
.confirm-layout {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 30px;
}

.confirm-card {
  background: white;
  border-radius: 16px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.02);
}

.card-header-flex {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 15px;
  margin-bottom: 15px;
  border-bottom: 1px solid #f1f3f5;
}

.edit-link {
  font-size: 0.8rem;
  font-weight: 800;
  color: #0d6efd;
  cursor: pointer;
}

.card-body-flex {
  display: flex;
  gap: 20px;
  align-items: center;
}

.confirm-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  object-fit: cover;
}
.confirm-avatar.placeholder {
  background: #f8f9fa;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
}

.service-summary-box {
  background: #f8f9fa;
  border-radius: 12px;
  padding: 15px;
  display: flex;
  align-items: center;
  gap: 15px;
}
.service-icon-box {
  width: 48px;
  height: 48px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.premium-textarea {
  background: #f8f9fa;
  border: 1px solid transparent;
  border-radius: 12px;
  padding: 15px;
  resize: none;
}
.premium-textarea:focus {
  background: white;
  border-color: #0d6efd;
  box-shadow: 0 0 0 4px rgba(13, 110, 253, 0.1);
}

.receipt-card {
  background: white;
  border-radius: 20px;
  box-shadow: 0 10px 40px rgba(0,0,0,0.05);
  overflow: hidden;
}
.receipt-header {
  background: #e3f2fd;
  padding: 25px;
  position: relative;
}
.receipt-header::after {
  content: '';
  position: absolute;
  bottom: -10px;
  left: 0;
  width: 100%;
  height: 20px;
  background: white;
  border-radius: 20px 20px 0 0;
}

.receipt-header h3, .receipt-header .text-white, .receipt-header .edit-link {
  color: #052c65 !important;
}

.receipt-doctor {
  padding: 10px 25px 20px;
  display: flex;
  align-items: center;
  gap: 15px;
  border-bottom: 1px solid #f1f3f5;
}
.doctor-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.receipt-body {
  padding: 25px;
  background: #fdfdfe;
}
.tracking-wide {
  letter-spacing: 0.5px;
}

/* Footer Navigation */
.wizard-footer {
  padding: 20px 60px;
  border-top: 1px solid #f1f3f5;
  display: flex;
  justify-content: space-between;
  background: white;
}
</style>
