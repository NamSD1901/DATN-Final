<template>
  <div class="customer-overview-tab">
    <!-- Quick Stats -->
    <div class="row g-4 mb-4">
      <div class="col-lg-4 col-md-6">
        <div class="stat-card glass-panel h-100">
          <div class="stat-icon bg-warning-glass text-warning">
            <i class="bi bi-calendar-event-fill"></i>
          </div>
          <div class="stat-details">
            <h6 class="stat-title text-secondary-muted">Lịch hẹn sắp tới</h6>
            <h4 class="stat-value text-dark fw-bold mb-0">
              <span v-if="loadingAppointments">...</span>
              <span v-else-if="nextAppointment">
                {{ formatDateTimeShort(nextAppointment.appointmentDate) }}
              </span>
              <span v-else class="fs-6 fw-normal">Chưa có lịch</span>
            </h4>
            <p v-if="nextAppointment" class="stat-subtitle small text-muted mb-0 mt-1 truncate-text">
              Cho bé: <strong>{{ nextAppointment.petName }}</strong>
            </p>
          </div>
        </div>
      </div>

      <div class="col-lg-4 col-md-6">
        <div class="stat-card glass-panel h-100">
          <div class="stat-icon bg-success-glass text-success">
            <i class="bi bi-heptagon-fill"></i>
          </div>
          <div class="stat-details">
            <h6 class="stat-title text-secondary-muted">Thú cưng đang quản lý</h6>
            <h4 class="stat-value text-dark fw-bold mb-0">
              <span v-if="loadingPets">...</span>
              <span v-else>{{ pets.length }} <span class="fs-6 fw-normal">bé</span></span>
            </h4>
            <p class="stat-subtitle small text-muted mb-0 mt-1">Đã đăng ký trong hệ thống</p>
          </div>
        </div>
      </div>

      <div class="col-lg-4 col-md-12">
        <div class="stat-card glass-panel h-100">
          <div class="stat-icon bg-info-glass text-info">
            <i class="bi bi-heart-pulse-fill"></i>
          </div>
          <div class="stat-details">
            <h6 class="stat-title text-secondary-muted">Hoạt động y tế</h6>
            <h4 class="stat-value text-dark fw-bold mb-0">
              <span v-if="loadingAppointments">...</span>
              <span v-else>{{ totalAppointments }} <span class="fs-6 fw-normal">lần khám</span></span>
            </h4>
            <p class="stat-subtitle small text-muted mb-0 mt-1">Lịch sử thăm khám</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Grid -->
    <div class="row g-4">
      <!-- Left Column: Upcoming Appointments (60%) -->
      <div class="col-xl-7 col-lg-6">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h5 class="fw-bold text-dark mb-0"><i class="bi bi-calendar2-heart-fill text-warning me-2"></i>Chi tiết lịch khám sắp tới</h5>
          <button class="btn btn-sm btn-outline-glass rounded-pill px-3" @click="$emit('switch-tab', 'my-appointments')">
            Quản lý lịch hẹn <i class="bi bi-arrow-right ms-1"></i>
          </button>
        </div>

        <div v-if="loadingAppointments" class="glass-panel p-4 text-center">
          <div class="spinner-border text-warning" role="status"></div>
        </div>
        <div v-else-if="!nextAppointment" class="glass-panel p-5 text-center h-100 d-flex flex-column justify-content-center align-items-center">
          <div style="font-size: 3rem; opacity: 0.7;">📅</div>
          <h5 class="fw-bold text-dark mt-3 mb-2">Chưa có lịch khám nào sắp tới</h5>
          <p class="text-muted mb-4">Hãy đặt lịch khám định kỳ hoặc tiêm phòng để bảo vệ sức khoẻ cho thú cưng của bạn.</p>
          <button class="btn btn-premium-neon px-4" @click="$emit('switch-tab', 'my-appointments')">
            <i class="bi bi-calendar-plus-fill me-2"></i> Đặt lịch ngay
          </button>
        </div>
        <div v-else class="glass-panel p-4 h-100 position-relative overflow-hidden d-flex flex-column">
          <div class="position-absolute top-0 end-0 p-3 opacity-10">
             <i class="bi bi-calendar-check-fill" style="font-size: 8rem;"></i>
          </div>
          <div class="d-flex align-items-center gap-3 mb-4 position-relative z-index-1">
             <div class="pet-avatar-img-wrapper" style="width: 60px; height: 60px; border-radius: 12px; overflow: hidden; border: 2px solid rgba(245, 158, 11, 0.2);">
                <img :src="getSpeciesImageUrl(nextAppointment.petSpecies)" alt="Pet Avatar" style="width: 100%; height: 100%; object-fit: cover;" />
             </div>
             <div>
                <h4 class="fw-bold text-dark mb-1">{{ nextAppointment.serviceName }}</h4>
                <p class="text-secondary-muted mb-0">Thú cưng: <strong>{{ nextAppointment.petName }}</strong></p>
             </div>
          </div>
          
          <div class="row g-3 mb-4 position-relative z-index-1">
             <div class="col-sm-6">
                <div class="p-3 rounded bg-light border border-warning border-opacity-25 h-100">
                   <p class="small text-muted mb-1"><i class="bi bi-clock-history me-1"></i>Thời gian</p>
                   <strong class="text-dark">{{ formatDateTime(nextAppointment.appointmentDate) }}</strong>
                </div>
             </div>
             <div class="col-sm-6">
                <div class="p-3 rounded bg-light border border-warning border-opacity-25 h-100">
                   <p class="small text-muted mb-1"><i class="bi bi-person-badge me-1"></i>Bác sĩ phụ trách</p>
                   <strong class="text-dark">{{ nextAppointment.doctorName || 'Sắp xếp khi đến khám' }}</strong>
                </div>
             </div>
             <div class="col-12" v-if="nextAppointment.symptom || nextAppointment.note">
                <div class="p-3 rounded bg-warning bg-opacity-10 border border-warning border-opacity-25">
                   <p class="small text-warning fw-bold mb-1"><i class="bi bi-info-circle-fill me-1"></i>Triệu chứng / Ghi chú</p>
                   <span class="text-dark">{{ nextAppointment.symptom || nextAppointment.note }}</span>
                </div>
             </div>
          </div>
          
          <div class="d-flex gap-3 position-relative z-index-1 mt-auto">
             <button class="btn btn-outline-glass flex-grow-1" @click="$emit('switch-tab', 'my-appointments')">Quản lý lịch</button>
             <button class="btn btn-premium-neon flex-grow-1" @click="$emit('switch-tab', 'my-pets')">Xem hồ sơ thú cưng</button>
          </div>
        </div>
      </div>

      <!-- Right Column: Recent Activity Timeline (40%) -->
      <div class="col-xl-5 col-lg-6">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <h5 class="fw-bold text-dark mb-0"><i class="bi bi-clock-history text-warning me-2"></i>Hoạt động gần đây</h5>
        </div>

        <div class="glass-panel p-4 h-100 timeline-container">
          <div v-if="loadingAppointments" class="text-center py-4">
            <div class="spinner-border text-warning" role="status"></div>
          </div>
          <div v-else-if="recentAppointments.length === 0" class="text-center py-4 text-muted">
            <i class="bi bi-inbox fs-1 d-block mb-2 opacity-50"></i>
            Chưa có hoạt động y tế nào gần đây.
          </div>
          <div v-else class="timeline">
            <div v-for="(appt, index) in recentAppointments" :key="appt.id" class="timeline-item">
              <div class="timeline-marker" :class="getStatusColor(appt.status)">
                <i :class="getStatusIcon(appt.status)"></i>
              </div>
              <div class="timeline-content">
                <h6 class="fw-bold mb-1 text-dark">{{ getStatusTitle(appt.status) }}</h6>
                <p class="small text-secondary-muted mb-1">{{ formatDateTime(appt.appointmentDate) }}</p>
                <div class="timeline-card p-2 rounded mt-2">
                  <strong class="d-block text-dark">{{ appt.serviceName }}</strong>
                  <span class="small text-muted">Bé: {{ appt.petName }} · Bác sĩ: {{ appt.doctorName || 'Chưa xếp' }}</span>
                </div>
              </div>
            </div>
          </div>
          <div v-if="recentAppointments.length > 0" class="text-center mt-3 pt-3 border-top-glass">
            <button class="btn btn-link text-warning fw-bold text-decoration-none p-0" @click="$emit('switch-tab', 'my-history')">
              Xem tất cả lịch sử <i class="bi bi-arrow-right"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Auto Review Modal -->
    <ReviewModal
      :is-open="showAutoReviewModal"
      :initial-data="{ appointmentId: autoReviewApptId }"
      @close="onAutoReviewClosed"
      @submit="onAutoReviewSubmitted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api';
import { useReviewStore } from '../../stores/review.store';
import ReviewModal from '../shared/ReviewModal.vue';
import confetti from 'canvas-confetti';

const backendUrl = import.meta.env.VITE_API_URL || 'http://localhost:5150';

const emit = defineEmits(['switch-tab']);
const reviewStore = useReviewStore();

// Auto review state
const showAutoReviewModal = ref(false);
const autoReviewApptId = ref(0);
const autoReviewServiceName = ref('');

// State
const pets = ref<any[]>([]);
const appointments = ref<any[]>([]);
const loadingPets = ref(true);
const loadingAppointments = ref(true);

const fetchPets = async () => {
  try {
    const res = await api.get('/mypets');
    pets.value = res.data;
  } catch (error) {
    console.error("Failed to fetch pets", error);
  } finally {
    loadingPets.value = false;
  }
};

const fetchAppointments = async () => {
  try {
    const res = await api.get('/my-appointments?page=1&pageSize=50');
    // Assuming API returns { items: [...] } or just an array. Based on controller it might return paginated result.
    appointments.value = res.data.items || res.data || [];
  } catch (error) {
    console.error("Failed to fetch appointments", error);
  } finally {
    loadingAppointments.value = false;
  }
};

// Computed
const totalAppointments = computed(() => {
  return appointments.value.filter(a => a.status === 'completed').length;
});

const nextAppointment = computed(() => {
  const futureAppts = appointments.value.filter(a => 
    (a.status === 'confirmed' || a.status === 'pending') && 
    new Date(a.appointmentDate).getTime() > new Date().getTime()
  );
  if (futureAppts.length === 0) return null;
  
  return futureAppts.sort((a, b) => new Date(a.appointmentDate).getTime() - new Date(b.appointmentDate).getTime())[0];
});

const recentAppointments = computed(() => {
  return [...appointments.value]
    .sort((a, b) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime())
    .slice(0, 5);
});

// Helpers
const getPetAvatarUrl = (avatarPath: string) => {
  if (avatarPath.startsWith('http')) return avatarPath;
  return `${backendUrl}${avatarPath}`;
};

const calculateAge = (birthDate: string): string => {
  if (!birthDate) return 'Chưa rõ tuổi';
  const birth = new Date(birthDate);
  const now = new Date();
  const diffMs = now.getTime() - birth.getTime();
  const totalMonths = Math.floor(diffMs / (1000 * 60 * 60 * 24 * 30.44));
  const years = Math.floor(totalMonths / 12);
  const months = totalMonths % 12;
  if (years === 0) return months === 0 ? 'Sơ sinh' : `${months} tháng`;
  return `${years} tuổi`;
};

const getSpeciesEmoji = (species: string): string => {
  const map: Record<string, string> = { 'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎' };
  return map[species] || '🐾';
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

const getSpeciesClass = (species: string): string => {
  const map: Record<string, string> = { 'Chó': 'species-dog', 'Mèo': 'species-cat' };
  return map[species] || 'species-other';
};

const getPetAvatarColor = (species: string): string => {
  const map: Record<string, string> = {
    'Chó': 'linear-gradient(135deg, hsl(35, 95%, 60%), hsl(35, 95%, 45%))',
    'Mèo': 'linear-gradient(135deg, hsl(265, 85%, 65%), hsl(265, 85%, 50%))'
  };
  return map[species] || 'linear-gradient(135deg, hsl(210, 10%, 50%), hsl(210, 10%, 35%))';
};

const formatDateTimeShort = (dateStr: string) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return `${date.getHours().toString().padStart(2, '0')}:${date.getMinutes().toString().padStart(2, '0')} - ${date.getDate()}/${date.getMonth() + 1}`;
};

const formatDateTime = (dateStr: string) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  return date.toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' });
};

const getStatusColor = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'bg-warning text-dark';
    case 'confirmed': return 'bg-info text-white';
    case 'completed': return 'bg-success text-white';
    case 'cancelled': return 'bg-danger text-white';
    default: return 'bg-secondary text-white';
  }
};

const getStatusIcon = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'bi-hourglass-split';
    case 'confirmed': return 'bi-calendar-check';
    case 'completed': return 'bi-check2-circle';
    case 'cancelled': return 'bi-x-circle';
    default: return 'bi-info-circle';
  }
};

const getStatusTitle = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'Chờ xác nhận';
    case 'confirmed': return 'Đã xác nhận';
    case 'completed': return 'Đã khám xong';
    case 'cancelled': return 'Đã huỷ';
    default: return 'Không xác định';
  }
};

const triggerConfetti = () => {
  const duration = 2.5 * 1000;
  const animationEnd = Date.now() + duration;
  const defaults = { startVelocity: 30, spread: 360, ticks: 60, zIndex: 1200 };

  function randomInRange(min: number, max: number) {
    return Math.random() * (max - min) + min;
  }

  const interval: any = setInterval(function() {
    const timeLeft = animationEnd - Date.now();
    if (timeLeft <= 0) {
      return clearInterval(interval);
    }
    const particleCount = 40 * (timeLeft / duration);
    // Pháo giấy bắn ra từ 2 bên cạnh màn hình
    confetti(Object.assign({}, defaults, { particleCount, origin: { x: randomInRange(0.1, 0.3), y: Math.random() - 0.2 } }));
    confetti(Object.assign({}, defaults, { particleCount, origin: { x: randomInRange(0.7, 0.9), y: Math.random() - 0.2 } }));
  }, 250);
};

const onAutoReviewClosed = () => {
  showAutoReviewModal.value = false;
  try {
    const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
    const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];
    if (!dismissedReviews.includes(autoReviewApptId.value)) {
      dismissedReviews.push(autoReviewApptId.value);
      localStorage.setItem('dismissedReviews', JSON.stringify(dismissedReviews));
    }
  } catch (err) {
    console.error("Lỗi lưu trạng thái dismiss review", err);
  }
};

const onAutoReviewSubmitted = async (data: any) => {
  try {
    await reviewStore.submitReview({
      appointmentId: data.appointmentId,
      rating: data.rating,
      comment: data.comment
    });
    showAutoReviewModal.value = false;
    
    // Đánh dấu đã review thành công vào dismiss (để chắc chắn ko popup lại dù DB có delay)
    try {
      const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
      const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];
      if (!dismissedReviews.includes(data.appointmentId)) {
        dismissedReviews.push(data.appointmentId);
        localStorage.setItem('dismissedReviews', JSON.stringify(dismissedReviews));
      }
    } catch (e) {}

    // Bắn pháo giấy chúc mừng lớn ở giữa khi Đánh giá thành công
    confetti({
      particleCount: 150,
      spread: 70,
      origin: { y: 0.6 },
      zIndex: 1200
    });
  } catch (err: any) {
    console.error('Lỗi khi gửi đánh giá', err);
    // Nếu lỗi do đã đánh giá rồi thì cũng đóng luôn popup
    if (err.response?.data?.message?.includes("đã được đánh giá")) {
      showAutoReviewModal.value = false;
      try {
        const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
        const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];
        if (!dismissedReviews.includes(data.appointmentId)) {
          dismissedReviews.push(data.appointmentId);
          localStorage.setItem('dismissedReviews', JSON.stringify(dismissedReviews));
        }
      } catch (e) {}
    }
  }
};

onMounted(async () => {
  fetchPets();
  await fetchAppointments();
  
  // Logic kiểm tra xem có lịch khám nào đã hoàn thành mà chưa được đánh giá không
  try {
    await reviewStore.fetchMyReviews(1);
    
    const dismissedReviewsStr = localStorage.getItem('dismissedReviews');
    const dismissedReviews: number[] = dismissedReviewsStr ? JSON.parse(dismissedReviewsStr) : [];

    // Tìm các lịch hẹn đã hoàn thành và sắp xếp theo ngày khám gần nhất (mới nhất lên đầu)
    const completedAppts = appointments.value
      .filter(a => a.status === 'completed')
      .sort((a, b) => new Date(b.appointmentDate).getTime() - new Date(a.appointmentDate).getTime());
      
    // Chỉ kiểm tra LỊCH HẸN MỚI NHẤT (gần đây nhất) để không làm phiền khách với các lịch hẹn cũ.
    if (completedAppts.length > 0) {
      const latestAppt = completedAppts[0];
      const hasReviewed = reviewStore.myReviews.some(r => r.appointmentId == latestAppt.id);
      
      // Nếu chưa review và chưa từng bấm "Huỷ/Đóng"
      if (!hasReviewed && !dismissedReviews.includes(latestAppt.id)) {
        // Đặt dữ liệu và hiển thị Popup tự động
        autoReviewApptId.value = latestAppt.id;
        autoReviewServiceName.value = latestAppt.serviceName || 'Dịch vụ khám';
        
        // Timeout một chút để UI load xong mới hiện popup và nổ sao
        setTimeout(() => {
          showAutoReviewModal.value = true;
          triggerConfetti(); // Bắn pháo giấy sao trời!
        }, 800);
      }
    }
  } catch (e) {
    console.error("Lỗi khi tải lịch sử đánh giá auto:", e);
  }
});
</script>

<style scoped>
/* Glassmorphism Classes */
.glass-panel {
  background: rgba(255, 255, 255, 0.75);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(245, 158, 11, 0.15);
  border-radius: 20px;
  box-shadow: 0 8px 32px 0 rgba(217, 119, 6, 0.05);
}

.border-top-glass {
  border-top: 1px solid rgba(217, 119, 6, 0.1);
}

.text-secondary-muted {
  color: #6b7280 !important;
}

.truncate-text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Stat Cards */
.stat-card {
  display: flex;
  align-items: center;
  padding: 1.5rem;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 12px 40px rgba(217, 119, 6, 0.1);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  margin-right: 1.2rem;
  flex-shrink: 0;
}

.bg-warning-glass { background: rgba(245, 158, 11, 0.15); }
.bg-success-glass { background: rgba(16, 185, 129, 0.15); }
.bg-info-glass { background: rgba(14, 165, 233, 0.15); }

/* Featured Pet Cards */
.featured-pet-card {
  transition: all 0.3s ease;
  border: 1px solid rgba(245, 158, 11, 0.1);
}

.featured-pet-card:hover {
  border-color: rgba(245, 158, 11, 0.3);
  box-shadow: 0 10px 25px rgba(245, 158, 11, 0.1);
  transform: translateY(-3px);
}

.pet-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  color: white;
  overflow: hidden;
  flex-shrink: 0;
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.btn-premium-neon {
  background: linear-gradient(135deg, var(--primary-gold), var(--primary-dark));
  color: white;
  border: none;
  border-radius: 50px;
  font-weight: 600;
  padding: 0.5rem;
  transition: all 0.3s;
}

.btn-premium-neon:hover {
  box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3);
  filter: brightness(1.05);
  color: white;
}

.btn-outline-glass {
  border: 1px solid rgba(245, 158, 11, 0.3);
  color: var(--primary-dark);
  font-weight: 600;
}

.btn-outline-glass:hover {
  background: var(--primary-gold);
  color: white;
}

/* Timeline Styles */
.timeline-container {
  overflow-y: auto;
  max-height: 400px;
}
.timeline-container::-webkit-scrollbar {
  width: 6px;
}
.timeline-container::-webkit-scrollbar-thumb {
  background-color: rgba(245, 158, 11, 0.2);
  border-radius: 4px;
}

.timeline {
  position: relative;
  padding-left: 20px;
}

.timeline::before {
  content: '';
  position: absolute;
  top: 10px;
  bottom: 0;
  left: 31px;
  width: 2px;
  background: rgba(245, 158, 11, 0.2);
}

.timeline-item {
  position: relative;
  margin-bottom: 1.5rem;
  padding-left: 45px;
}

.timeline-item:last-child {
  margin-bottom: 0;
}

.timeline-marker {
  position: absolute;
  left: 0;
  top: 0;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.8rem;
  z-index: 1;
  box-shadow: 0 2px 5px rgba(0,0,0,0.1);
  border: 2px solid white;
}

.timeline-card {
  background: rgba(245, 158, 11, 0.05);
  border: 1px solid rgba(245, 158, 11, 0.1);
}
</style>
