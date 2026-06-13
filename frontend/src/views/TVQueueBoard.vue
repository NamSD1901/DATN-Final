<template>
  <div class="tv-board-wrapper">
    <!-- Background glowing accents -->
    <div class="glow-bg glow-purple"></div>
    <div class="glow-bg glow-amber"></div>

    <!-- Header bar -->
    <header class="tv-header">
      <div class="brand-logo">
        <i class="bi bi-heart-pulse-fill brand-icon"></i>
        <span>MyPet<span class="brand-accent">Clinic</span></span>
      </div>
      <div class="board-title">BẢNG GỌI SỐ HÀNG KHÁM</div>
      <div class="tv-clock">
        <i class="bi bi-clock-fill me-2"></i>{{ currentTime }}
      </div>
    </header>

    <!-- Main board container -->
    <main class="tv-main-container">
      <div class="row h-100 g-4">
        <!-- Serving column (InProgress) -->
        <div class="col-md-7 h-100 d-flex flex-column">
          <div class="glass-card flex-grow-1 d-flex flex-column p-4 overflow-hidden border-serving">
            <h2 class="column-title text-success">
              <span class="pulse-indicator bg-success"></span>
              ĐANG KHÁM
            </h2>
            
            <div class="serving-list flex-grow-1 overflow-auto mt-3">
              <div v-if="servingList.length === 0" class="empty-state">
                <div class="empty-icon">🩺</div>
                <div class="empty-text">Hiện tại không có ca nào đang khám.</div>
              </div>
              <div v-else class="serving-grid">
                <div 
                  v-for="item in servingList" 
                  :key="item.appointmentId" 
                  class="serving-card glass-inner-card"
                >
                  <div class="serving-left">
                    <div class="serving-queue">{{ item.queueNumber }}</div>
                    <div class="serving-pet">
                      <span class="pet-avatar">🐾</span>
                      {{ item.petName }}
                    </div>
                  </div>
                  <div class="serving-right">
                    <div class="doctor-badge">Bác sĩ phụ trách</div>
                    <div class="doctor-name">Bs. {{ item.doctorName }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Waiting column -->
        <div class="col-md-5 h-100 d-flex flex-column">
          <div class="glass-card flex-grow-1 d-flex flex-column p-4 overflow-hidden border-waiting">
            <h2 class="column-title text-warning">
              <span class="pulse-indicator bg-warning"></span>
              DANH SÁCH CHỜ
            </h2>

            <div class="waiting-list flex-grow-1 overflow-auto mt-3">
              <div v-if="waitingList.length === 0" class="empty-state">
                <div class="empty-icon">🗓️</div>
                <div class="empty-text">Không có thú cưng nào trong hàng chờ.</div>
              </div>
              <div v-else class="waiting-grid">
                <div 
                  v-for="item in waitingList" 
                  :key="item.appointmentId" 
                  class="waiting-card glass-inner-card"
                >
                  <div class="waiting-queue">{{ item.queueNumber }}</div>
                  <div class="waiting-info">
                    <div class="waiting-pet-name">🐾 {{ item.petName }}</div>
                    <div class="waiting-doctor-name">Bs. {{ item.doctorName }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Footer marquee bar -->
    <footer class="tv-footer">
      <div class="marquee-content">
        <i class="bi bi-megaphone-fill text-warning me-2"></i>
        <span>Chào mừng quý khách đến với phòng khám thú y MyPetClinic. Vui lòng theo dõi bảng điện tử để vào phòng khám đúng số thứ tự của bé. Xin cảm ơn quý khách!</span>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import api from '../services/api';

interface QueueItem {
  appointmentId: number;
  queueNumber: string;
  petName: string;
  doctorName: string;
  status: string;
}

const waitingList = ref<QueueItem[]>([]);
const servingList = ref<QueueItem[]>([]);
const currentTime = ref('');
let pollInterval: any = null;
let clockInterval: any = null;

// Track previously seen serving IDs to identify new calls
const previousServingIds = ref<Set<number>>(new Set());

// Update time
const updateClock = () => {
  const now = new Date();
  currentTime.value = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
};

// Speak Voice
const speakNumber = (queueNumber: string, petName: string, doctorName: string) => {
  if ('speechSynthesis' in window) {
    window.speechSynthesis.cancel(); // Cancel any ongoing speech
    
    // Format queue digits to Vietnamese words: Q-001 -> "Q không không một"
    const cleanNum = queueNumber
      .replace('-', ' ')
      .replace(/0/g, 'không ')
      .replace(/1/g, 'một ')
      .replace(/2/g, 'hai ')
      .replace(/3/g, 'ba ')
      .replace(/4/g, 'bốn ')
      .replace(/5/g, 'năm ')
      .replace(/6/g, 'sáu ')
      .replace(/7/g, 'bảy ')
      .replace(/8/g, 'tám ')
      .replace(/9/g, 'chín ');

    // Extract doctor's last word name for friendly speaking
    const docParts = doctorName.split(' ');
    const docNameOnly = docParts[docParts.length - 1] || doctorName;

    const msg = new SpeechSynthesisUtterance();
    msg.text = `Xin mời thú cưng ${petName}, số thứ tự ${cleanNum}, vào phòng khám của bác sĩ ${docNameOnly}.`;
    msg.lang = 'vi-VN';
    msg.rate = 0.85; // Natural speaking rate
    msg.volume = 1;
    window.speechSynthesis.speak(msg);
  }
};

// Fetch data
const fetchLobbyBoard = async (isFirstLoad = false) => {
  try {
    const res = await api.get('/queue/lobby-board');
    const newWaiting = res.data.waiting || [];
    const newServing = res.data.serving || [];

    waitingList.value = newWaiting;
    servingList.value = newServing;

    // Detect new calls (items added to serving)
    if (!isFirstLoad) {
      for (const item of newServing) {
        if (!previousServingIds.value.has(item.appointmentId)) {
          speakNumber(item.queueNumber, item.petName, item.doctorName);
        }
      }
    }

    // Update tracked IDs
    previousServingIds.value = new Set(newServing.map((item: QueueItem) => item.appointmentId));
  } catch (err) {
    console.error('Lỗi fetch TV Board data:', err);
  }
};

onMounted(async () => {
  updateClock();
  clockInterval = setInterval(updateClock, 1000);

  // Initial load
  await fetchLobbyBoard(true);
  
  // Poll every 4 seconds for instant sync
  pollInterval = setInterval(() => fetchLobbyBoard(false), 4000);
});

onUnmounted(() => {
  if (clockInterval) clearInterval(clockInterval);
  if (pollInterval) clearInterval(pollInterval);
  if ('speechSynthesis' in window) {
    window.speechSynthesis.cancel();
  }
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@400;600;800&display=swap');

/* Main layouts */
.tv-board-wrapper {
  font-family: 'Outfit', 'Inter', sans-serif;
  background-color: #0f172a;
  color: #f8fafc;
  min-height: 100vh;
  width: 100vw;
  display: flex;
  flex-direction: column;
  position: relative;
  overflow: hidden;
}

/* Glowing backgrounds */
.glow-bg {
  position: absolute;
  width: 600px;
  height: 600px;
  border-radius: 50%;
  filter: blur(180px);
  opacity: 0.15;
  pointer-events: none;
  z-index: 0;
}
.glow-purple {
  top: -10%;
  left: -10%;
  background: #a855f7;
}
.glow-amber {
  bottom: -10%;
  right: -10%;
  background: #f59e0b;
}

/* Header bar styling */
.tv-header {
  height: 80px;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(12px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 3rem;
  z-index: 10;
}

.brand-logo {
  font-size: 1.8rem;
  font-weight: 800;
  display: flex;
  align-items: center;
  gap: 10px;
  color: white;
}
.brand-icon {
  color: #f59e0b;
}
.brand-accent {
  color: #f59e0b;
}

.board-title {
  font-size: 1.8rem;
  font-weight: 800;
  letter-spacing: 2px;
  background: linear-gradient(135deg, #fff 0%, #cbd5e1 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.tv-clock {
  font-size: 1.6rem;
  font-weight: 600;
  color: #f59e0b;
  font-variant-numeric: tabular-nums;
  background: rgba(245, 158, 11, 0.1);
  padding: 6px 18px;
  border-radius: 50px;
  border: 1px solid rgba(245, 158, 11, 0.2);
}

/* Main Container Grid */
.tv-main-container {
  flex: 1;
  padding: 2rem 3rem;
  z-index: 10;
  overflow: hidden;
}

/* Glassmorphism Cards */
.glass-card {
  background: rgba(30, 41, 59, 0.45);
  backdrop-filter: blur(20px);
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.06);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
}

.border-serving {
  border-top: 5px solid #10b981;
}

.border-waiting {
  border-top: 5px solid #f59e0b;
}

.column-title {
  font-size: 1.6rem;
  font-weight: 800;
  letter-spacing: 1px;
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 0.5rem;
}

.pulse-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
  animation: beacon 1.8s infinite;
}

@keyframes beacon {
  0% { transform: scale(0.9); opacity: 0.4; }
  50% { transform: scale(1.2); opacity: 1; }
  100% { transform: scale(0.9); opacity: 0.4; }
}

/* Inner Glass Cards */
.glass-inner-card {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-radius: 18px;
  transition: all 0.3s;
}

/* Serving Cards styling */
.serving-list {
  display: flex;
  flex-direction: column;
}
.serving-grid {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.serving-card {
  padding: 1.5rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 10px 30px rgba(16, 185, 129, 0.05);
}

.serving-card:hover {
  background: rgba(255, 255, 255, 0.07);
  transform: translateX(4px);
  border-color: rgba(16, 185, 129, 0.4);
}

.serving-left {
  display: flex;
  align-items: center;
  gap: 2.5rem;
}

.serving-queue {
  font-size: 3.5rem;
  font-weight: 800;
  color: #10b981;
  text-shadow: 0 0 20px rgba(16, 185, 129, 0.35);
  line-height: 1;
}

.serving-pet {
  font-size: 1.8rem;
  font-weight: 600;
  color: white;
}

.pet-avatar {
  margin-right: 8px;
}

.serving-right {
  text-align: right;
}

.doctor-badge {
  font-size: 0.8rem;
  text-transform: uppercase;
  color: #94a3b8;
  font-weight: 600;
  letter-spacing: 1px;
  margin-bottom: 4px;
}

.doctor-name {
  font-size: 1.6rem;
  font-weight: 700;
  color: #e2e8f0;
}

/* Waiting Cards styling */
.waiting-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 1rem;
}

.waiting-card {
  padding: 1.25rem;
  text-align: center;
}

.waiting-card:hover {
  background: rgba(255, 255, 255, 0.07);
  transform: scale(1.03);
  border-color: rgba(245, 158, 11, 0.4);
}

.waiting-queue {
  font-size: 2.2rem;
  font-weight: 800;
  color: #f59e0b;
  text-shadow: 0 0 15px rgba(245, 158, 11, 0.3);
  margin-bottom: 0.5rem;
}

.waiting-info {
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  padding-top: 0.5rem;
}

.waiting-pet-name {
  font-size: 1rem;
  font-weight: 600;
  color: white;
  margin-bottom: 2px;
}

.waiting-doctor-name {
  font-size: 0.82rem;
  color: #94a3b8;
}

/* Empty State */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 4rem 2rem;
  color: #64748b;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.empty-text {
  font-size: 1.1rem;
}

/* Footer Marquee bar */
.tv-footer {
  height: 50px;
  background: #f59e0b;
  color: #0f172a;
  font-weight: 700;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
  overflow: hidden;
  z-index: 10;
}

.marquee-content {
  white-space: nowrap;
  padding-left: 100%;
  animation: marquee 25s linear infinite;
}

@keyframes marquee {
  0% { transform: translate(0, 0); }
  100% { transform: translate(-100%, 0); }
}
</style>
