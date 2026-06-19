<template>
  <div class="doctor-queue-tab">
    <!-- Header Summary Cards -->
    <div class="row g-4 mb-4">
      <div class="col-md-6 col-lg-4">
        <div class="card border-0 shadow-sm rounded-4 p-3 d-flex flex-row align-items-center bg-white card-glow-green">
          <div class="bg-success bg-opacity-10 text-success p-3 rounded-circle me-3">
            <i class="bi bi-people-fill fs-3"></i>
          </div>
          <div>
            <h6 class="text-muted mb-1 small fw-bold">Bệnh nhi đang chờ</h6>
            <h4 class="fw-bold text-dark mb-0">{{ waitingList.length }} <span class="fs-6 fw-normal text-muted">bé</span></h4>
          </div>
        </div>
      </div>
      <div class="col-md-6 col-lg-4">
        <div class="card border-0 shadow-sm rounded-4 p-3 d-flex flex-row align-items-center bg-white card-glow-amber">
          <div class="bg-warning bg-opacity-10 text-warning p-3 rounded-circle me-3">
            <i class="bi bi-activity fs-3"></i>
          </div>
          <div>
            <h6 class="text-muted mb-1 small fw-bold">Đang tiếp nhận khám</h6>
            <h4 class="fw-bold text-dark mb-0">{{ inProgressList.length }} <span class="fs-6 fw-normal text-muted">bé</span></h4>
          </div>
        </div>
      </div>
    </div>

    <!-- Active Cases / Clinical Board -->
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
        <div>
          <h5 class="fw-bold text-dark mb-1">
            <i class="bi bi-clipboard-pulse text-warning me-2"></i>
            Danh sách bệnh nhi được phân công hôm nay
          </h5>
          <p class="text-muted mb-0 small">Bác sĩ hãy chọn ca khám và bấm "Bắt đầu khám" để tiến hành khám lâm sàng & kê đơn thuốc.</p>
        </div>
        <button class="btn btn-outline-warning rounded-pill px-3 py-1.5 shadow-sm small text-dark fw-bold btn-refresh" @click="fetchQueue" :disabled="loading">
          <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
          <i v-else class="bi bi-arrow-clockwise me-1"></i> Làm mới
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading && queueList.length === 0" class="text-center py-5">
        <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
        <p class="text-muted mt-3">Đang tải danh sách ca khám...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="queueList.length === 0" class="empty-state-doctor">
        <div class="empty-icon-doc">🏥</div>
        <h5 class="fw-bold text-dark mt-3 mb-2">Không có ca khám nào được phân công</h5>
        <p class="text-muted small">Hiện tại chưa có bệnh nhi nào check-in chỉ định cho bác sĩ trong ngày hôm nay.</p>
      </div>

      <!-- Main Queue Table -->
      <div v-else class="table-responsive">
        <table class="table align-middle border-bottom mb-0 table-hover">
          <thead class="table-light">
            <tr>
              <th class="border-0 text-muted small py-3 px-3">STT</th>
              <th class="border-0 text-muted small py-3">Thú cưng</th>
              <th class="border-0 text-muted small py-3">Chủ nuôi</th>
              <th class="border-0 text-muted small py-3">Thời gian vào</th>
              <th class="border-0 text-muted small py-3">Triệu chứng lâm sàng</th>
              <th class="border-0 text-muted small py-3">Trạng thái</th>
              <th class="border-0 text-muted text-end small py-3 px-3">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in queueList" :key="item.appointmentId" class="queue-row" :class="{ 'row-in-progress': item.status === 'in_progress' }">
              <td class="px-3">
                <span class="badge badge-queue-num" :class="item.isEmergency ? 'bg-danger text-white' : 'bg-light text-secondary border'">
                  {{ formatQueueNumber(item.queueNumber) }}
                </span>
                <span v-if="item.isEmergency" class="badge bg-danger ms-1 text-white text-uppercase" style="font-size: 0.65rem;">Cấp cứu</span>
              </td>
              <td>
                <div class="d-flex align-items-center gap-2">
                  <span class="pet-avatar-mini">{{ getSpeciesEmoji(item.species) }}</span>
                  <div>
                    <strong class="text-dark d-block">{{ item.petName }}</strong>
                    <small class="text-muted">{{ item.species }} {{ item.weight ? `— ${item.weight} kg` : '' }}</small>
                  </div>
                </div>
              </td>
              <td>
                <span class="fw-semibold text-dark">{{ item.customerName || 'Khách vãng lai' }}</span>
              </td>
              <td>
                <span class="text-muted small"><i class="bi bi-clock me-1"></i>{{ formatTime(item.checkInTime) }}</span>
              </td>
              <td>
                <div class="symptom-text" :title="item.symptom">{{ item.symptom || '—' }}</div>
              </td>
              <td>
                <span class="status-pill" :class="`status-${item.status}`">
                  <i :class="getStatusIcon(item.status)" class="me-1"></i>
                  {{ getStatusLabel(item.status) }}
                </span>
              </td>
              <td class="text-end px-3">
                <button 
                  v-if="item.status === 'waiting'" 
                  class="btn btn-premium btn-sm rounded-pill px-4 fw-bold" 
                  @click="startTreatment(item)"
                  :disabled="actionLoading === item.appointmentId"
                >
                  <span v-if="actionLoading === item.appointmentId" class="spinner-border spinner-border-sm me-1"></span>
                  <i class="bi bi-play-fill me-1"></i> Tiến hành khám
                </button>
                <button 
                  v-else-if="item.status === 'in_progress'" 
                  class="btn btn-warning text-dark btn-sm rounded-pill px-4 fw-bold shadow-sm" 
                  @click="continueTreatment(item)"
                >
                  <i class="bi bi-arrow-right-circle-fill me-1"></i> Tiến hành khám
                </button>
                <span v-else class="text-muted small fw-semibold">Chờ thu ngân</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';
import api from '../../services/api';

const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

// State
const queueList = ref<any[]>([]);
const loading = ref(false);
const actionLoading = ref<number | null>(null);
let pollInterval: any = null;

// Computed
const waitingList = computed(() => queueList.value.filter(item => item.status === 'waiting'));
const inProgressList = computed(() => queueList.value.filter(item => item.status === 'in_progress'));

// Helpers
const formatQueueNumber = (num: number | string) => {
  if (!num) return 'Q-000';
  const n = parseInt(num.toString(), 10);
  if (isNaN(n)) return num;
  return `Q-${String(n).padStart(3, '0')}`;
};

const getSpeciesEmoji = (species: string | null): string => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const formatTime = (timeStr: string | null): string => {
  if (!timeStr) return '—';
  return new Date(timeStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const getStatusLabel = (status: string | null): string => {
  if (status === 'waiting') return 'Đang chờ';
  if (status === 'in_progress') return 'Đang khám';
  if (status === 'ready_to_pay') return 'Chờ thanh toán';
  return status || 'Không rõ';
};

const getStatusIcon = (status: string | null): string => {
  if (status === 'waiting') return 'bi-hourglass-split';
  if (status === 'in_progress') return 'bi-activity';
  return 'bi-check2-circle';
};

// API
const fetchQueue = async () => {
  loading.value = true;
  try {
    const res = await api.get('/doctor/queue');
    queueList.value = res.data || [];
  } catch (err) {
    console.error('Lỗi fetch doctor queue:', err);
  } finally {
    loading.value = false;
  }
};

const startTreatment = async (item: any) => {
  actionLoading.value = item.appointmentId;
  try {
    const res = await api.post('/doctor/start-treatment', item.appointmentId);
    if (res.data.success) {
      // Lưu thông tin ca khám hiện tại vào LocalStorage để các tab khám bệnh (Sprint 12) đọc
      localStorage.setItem('active_treatment_appointment_id', item.appointmentId.toString());
      localStorage.setItem('active_treatment_pet_id', item.petId.toString());
      localStorage.setItem('active_treatment_pet_name', item.petName || 'Bệnh nhi');
      localStorage.setItem('active_treatment_customer_name', item.customerName || 'Khách vãng lai');
      
      // Chuyển sang tab Bệnh án
      emit('switch-tab', 'medical-records');
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Không thể bắt đầu ca khám.');
  } finally {
    actionLoading.value = null;
  }
};

const continueTreatment = (item: any) => {
  localStorage.setItem('active_treatment_appointment_id', item.appointmentId.toString());
  localStorage.setItem('active_treatment_pet_id', item.petId.toString());
  localStorage.setItem('active_treatment_pet_name', item.petName || 'Bệnh nhi');
  localStorage.setItem('active_treatment_customer_name', item.customerName || 'Khách vãng lai');
  
  emit('switch-tab', 'medical-records');
};

// Lifecycle
onMounted(() => {
  fetchQueue();
  // Tự động làm mới sau mỗi 10 giây
  pollInterval = setInterval(fetchQueue, 10000);
});

onUnmounted(() => {
  if (pollInterval) clearInterval(pollInterval);
});
</script>

<style scoped>
.doctor-queue-tab {
  padding: 0;
}

.card-glow-green {
  border-left: 4px solid #10b981 !important;
}

.card-glow-amber {
  border-left: 4px solid #fbbf24 !important;
}

.badge-queue-num {
  font-size: 0.9rem;
  font-weight: 700;
  padding: 6px 12px;
  border-radius: 50px;
}

.pet-avatar-mini {
  font-size: 1.8rem;
}

.symptom-text {
  max-width: 250px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.88rem;
  color: #6b7280;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  border-radius: 50px;
  font-size: 0.78rem;
  font-weight: 700;
}
.status-waiting {
  background-color: #f3f4f6;
  color: #4b5563;
}
.status-in_progress {
  background-color: #fffbeb;
  color: #d97706;
  animation: pulse-border 2s infinite;
}
.status-ready_to_pay {
  background-color: #ecfdf5;
  color: #059669;
}

@keyframes pulse-border {
  0% { box-shadow: 0 0 0 0 rgba(217, 119, 6, 0.2); }
  70% { box-shadow: 0 0 0 6px rgba(217, 119, 6, 0); }
  100% { box-shadow: 0 0 0 0 rgba(217, 119, 6, 0); }
}

.btn-premium {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
  border: none;
  transition: all 0.3s;
  box-shadow: 0 4px 10px rgba(16, 185, 129, 0.25);
}
.btn-premium:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 15px rgba(16, 185, 129, 0.35);
  color: white;
}

.btn-refresh {
  transition: all 0.2s;
}
.btn-refresh:hover {
  transform: rotate(15deg);
}

.empty-state-doctor {
  text-align: center;
  padding: 4rem 2rem;
  background-color: #fafafa;
  border-radius: 16px;
  border: 2px dashed #e5e7eb;
}
.empty-icon-doc {
  font-size: 4rem;
}

.queue-row {
  transition: all 0.2s;
}
.queue-row:hover {
  background-color: #fafbfd;
}
.row-in-progress {
  background-color: rgba(251, 191, 36, 0.03);
}
</style>
