<template>
  <div class="myhistory-tab">

    <!-- Header -->
    <div class="history-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-clock-history me-2" style="color: var(--primary-gold);"></i>
            Lịch sử khám bệnh
          </h3>
          <p class="text-muted mb-0 small">Toàn bộ hồ sơ y tế và các lần thăm khám của thú cưng bạn.</p>
        </div>
        <!-- Summary stats -->
        <div class="d-flex gap-3">
          <div class="summary-stat-box">
            <span class="stat-num">{{ completedCount }}</span>
            <span class="stat-desc">Lần khám</span>
          </div>
          <div class="summary-stat-box accent-green">
            <span class="stat-num">{{ totalSpent }}</span>
            <span class="stat-desc">Đã chi</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Search & Filter Row -->
    <div class="filter-row mb-4">
      <div class="search-box">
        <i class="bi bi-search search-icon"></i>
        <input
          v-model="searchQuery"
          type="text"
          class="search-input"
          placeholder="Tìm theo thú cưng, dịch vụ, bác sĩ..."
        />
      </div>
      <select v-model="filterPet" class="filter-select">
        <option value="">Tất cả thú cưng</option>
        <option v-for="name in uniquePetNames" :key="name" :value="name">{{ name }}</option>
      </select>
      <select v-model="sortOrder" class="filter-select">
        <option value="desc">Mới nhất trước</option>
        <option value="asc">Cũ nhất trước</option>
      </select>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải lịch sử khám...</p>
    </div>

    <!-- Error -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 border-0 shadow-sm py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Empty State -->
    <div v-else-if="filteredHistory.length === 0" class="empty-history">
      <div class="empty-icon">🔍</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">
        {{ searchQuery || filterPet ? 'Không tìm thấy kết quả phù hợp' : 'Chưa có lịch sử khám bệnh' }}
      </h5>
      <p class="text-muted small mb-0">
        {{ searchQuery || filterPet ? 'Thử thay đổi từ khoá tìm kiếm hoặc bộ lọc.' : 'Sau khi hoàn thành buổi khám đầu tiên, lịch sử sẽ được hiển thị tại đây.' }}
      </p>
    </div>

    <!-- History Timeline -->
    <div v-else class="history-timeline">
      <!-- Group by month-year -->
      <div v-for="(group, monthKey) in groupedHistory" :key="monthKey" class="month-group">
        <div class="month-header">
          <i class="bi bi-calendar3 me-2"></i>{{ monthKey }}
          <span class="month-count">{{ group.length }} lần khám</span>
        </div>

        <div class="history-cards">
          <div
            v-for="record in group"
            :key="record.id"
            class="history-card"
            @click="openDetailModal(record)"
          >
            <!-- Pet Avatar -->
            <div class="history-card-left">
              <div class="history-pet-avatar" :style="{ background: getAvatarGradient(record.species) }">
                <span>{{ getSpeciesEmoji(record.species) }}</span>
              </div>
              <div class="history-timeline-dot"></div>
            </div>

            <!-- Card Content -->
            <div class="history-card-body">
              <div class="history-card-top">
                <div>
                  <span class="history-pet-name">{{ record.petName }}</span>
                  <span class="text-muted ms-2 small">{{ record.species }}</span>
                </div>
                <span class="history-date-badge">
                  {{ formatDateShort(record.appointmentDate) }}
                </span>
              </div>

              <div class="history-service-row">
                <span class="service-tag">
                  <i class="bi bi-clipboard2-pulse me-1"></i>{{ record.serviceName }}
                </span>
                <span v-if="record.doctorName" class="doctor-tag">
                  <i class="bi bi-person-badge me-1"></i>{{ record.doctorName }}
                </span>
              </div>

              <div v-if="record.symptom" class="history-symptom">
                <i class="bi bi-chat-right-text text-muted me-1"></i>
                <span>{{ truncate(record.symptom, 80) }}</span>
              </div>

              <!-- Invoice Info -->
              <div class="history-card-footer">
                <div class="d-flex align-items-center gap-2">
                  <span
                    v-if="record.invoiceStatus"
                    class="invoice-pill"
                    :class="`inv-${record.invoiceStatus}`"
                  >
                    <i class="bi bi-receipt me-1"></i>
                    {{ getInvoiceLabel(record.invoiceStatus) }}
                  </span>
                  <span v-if="record.invoiceTotalAmount" class="total-amount">
                    {{ formatCurrency(record.invoiceTotalAmount) }}
                  </span>
                </div>
                <button class="btn-view-history">
                  <i class="bi bi-eye me-1"></i> Xem chi tiết
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ===== DETAIL MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal && selectedRecord" class="history-modal-overlay" @click.self="showDetailModal = false">
          <div class="history-modal-card">

            <!-- Header -->
            <div class="history-modal-header" :style="{ background: getAvatarGradient(selectedRecord.species) }">
              <div class="text-center w-100">
                <div class="modal-pet-avatar">{{ getSpeciesEmoji(selectedRecord.species) }}</div>
                <h4 class="text-white fw-bold mt-2 mb-1">{{ selectedRecord.petName }}</h4>
                <p class="text-white opacity-75 small mb-0">
                  Ngày khám: {{ formatDateFull(selectedRecord.appointmentDate) }}
                </p>
              </div>
              <button class="modal-close-btn" @click="showDetailModal = false">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <!-- Body -->
            <div class="history-modal-body">

              <!-- Medical Summary -->
              <div class="medical-section">
                <h6 class="section-title"><i class="bi bi-clipboard2-pulse text-warning me-2"></i>Thông tin khám</h6>
                <div class="row g-2">
                  <div class="col-sm-6">
                    <div class="medi-item">
                      <span class="medi-label">Dịch vụ</span>
                      <span class="medi-value">{{ selectedRecord.serviceName }}</span>
                    </div>
                  </div>
                  <div class="col-sm-6">
                    <div class="medi-item">
                      <span class="medi-label">Bác sĩ phụ trách</span>
                      <span class="medi-value">{{ selectedRecord.doctorName || 'Không rõ' }}</span>
                    </div>
                  </div>
                  <div v-if="selectedRecord.symptom" class="col-12">
                    <div class="medi-item">
                      <span class="medi-label">Triệu chứng / Lý do khám</span>
                      <span class="medi-value">{{ selectedRecord.symptom }}</span>
                    </div>
                  </div>
                  <div v-if="selectedRecord.note" class="col-12">
                    <div class="medi-item">
                      <span class="medi-label">Ghi chú</span>
                      <span class="medi-value">{{ selectedRecord.note }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Status -->
              <div class="medical-section mt-3">
                <h6 class="section-title"><i class="bi bi-activity text-warning me-2"></i>Trạng thái</h6>
                <div class="d-flex align-items-center gap-3 flex-wrap">
                  <span class="status-pill" :class="`status-${selectedRecord.status}`">
                    <i :class="getStatusIcon(selectedRecord.status ?? '')" class="me-1"></i>
                    {{ getStatusLabel(selectedRecord.status ?? '') }}
                  </span>
                </div>
              </div>

              <!-- Invoice Section -->
              <div v-if="selectedRecord.invoiceId" class="medical-section mt-3">
                <h6 class="section-title"><i class="bi bi-receipt text-warning me-2"></i>Hóa đơn thanh toán</h6>
                <div class="invoice-summary-card" :class="selectedRecord.invoiceStatus === 'paid' ? 'inv-paid-bg' : 'inv-unpaid-bg'">
                  <div class="d-flex justify-content-between align-items-center">
                    <div>
                      <span class="fw-bold text-dark">Hóa đơn #{{ selectedRecord.invoiceId }}</span>
                      <br />
                      <span class="invoice-pill" :class="`inv-${selectedRecord.invoiceStatus}`">
                        {{ getInvoiceLabel(selectedRecord.invoiceStatus ?? '') }}
                      </span>
                    </div>
                    <div class="text-end">
                      <div v-if="selectedRecord.invoiceTotalAmount" class="invoice-total-display">
                        {{ formatCurrency(selectedRecord.invoiceTotalAmount) }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Quick stats for this record -->
              <div class="medical-section mt-3">
                <h6 class="section-title"><i class="bi bi-info-circle text-warning me-2"></i>Thông tin thú cưng tại thời điểm khám</h6>
                <div class="row g-2">
                  <div class="col-sm-6">
                    <div class="medi-item">
                      <span class="medi-label">Giống</span>
                      <span class="medi-value">{{ selectedRecord.breed || '—' }}</span>
                    </div>
                  </div>
                  <div class="col-sm-6">
                    <div class="medi-item">
                      <span class="medi-label">Cân nặng</span>
                      <span class="medi-value">{{ selectedRecord.weight ? `${selectedRecord.weight} kg` : '—' }}</span>
                    </div>
                  </div>
                  <div class="col-sm-6">
                    <div class="medi-item">
                      <span class="medi-label">Hung hăng</span>
                      <span class="medi-value">
                        <span v-if="selectedRecord.isAggressive" class="text-danger fw-semibold">⚠️ Có</span>
                        <span v-else class="text-success">Không</span>
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="d-flex justify-content-end mt-4">
                <button class="btn btn-outline-secondary rounded-pill px-5" @click="showDetailModal = false">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

// ===== Types =====
interface AppointmentDetail {
  id: number;
  petId: number;
  petName: string | null;
  species: string | null;
  breed: string | null;
  weight: number | null;
  isAggressive: boolean;
  customerId: string;
  customerName: string | null;
  customerPhone: string | null;
  serviceId: number;
  serviceName: string | null;
  servicePrice: number | null;
  doctorId: string | null;
  doctorName: string | null;
  appointmentDate: string;
  symptom: string | null;
  note: string | null;
  status: string | null;
  qrToken: string | null;
  invoiceId: number | null;
  invoiceStatus: string | null;
  invoiceTotalAmount: number | null;
}

// ===== State =====
const history = ref<AppointmentDetail[]>([]);
const loading = ref(false);
const errorMsg = ref('');

const searchQuery = ref('');
const filterPet = ref('');
const sortOrder = ref('desc');

const showDetailModal = ref(false);
const selectedRecord = ref<AppointmentDetail | null>(null);

// ===== API =====
const fetchHistory = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/my-appointments');
    // Only show completed appointments as history
    history.value = (res.data as AppointmentDetail[]).filter(a => a.status === 'completed');
  } catch (err: any) {
    errorMsg.value = 'Không thể tải lịch sử khám. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

// ===== Computed =====
const completedCount = computed(() => history.value.length);

const totalSpent = computed(() => {
  const total = history.value
    .filter(a => a.invoiceStatus === 'paid' && a.invoiceTotalAmount)
    .reduce((sum, a) => sum + (a.invoiceTotalAmount ?? 0), 0);
  if (total === 0) return '—';
  return formatCurrency(total);
});

const uniquePetNames = computed(() => {
  const names = history.value.map(a => a.petName ?? '').filter(n => n !== '');
  return [...new Set(names)];
});

const filteredHistory = computed(() => {
  let result = [...history.value];

  if (filterPet.value) {
    result = result.filter(a => a.petName === filterPet.value);
  }

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase();
    result = result.filter(a =>
      a.petName?.toLowerCase().includes(q) ||
      a.serviceName?.toLowerCase().includes(q) ||
      a.doctorName?.toLowerCase().includes(q) ||
      a.symptom?.toLowerCase().includes(q)
    );
  }

  result.sort((a, b) => {
    const dateA = new Date(a.appointmentDate).getTime();
    const dateB = new Date(b.appointmentDate).getTime();
    return sortOrder.value === 'desc' ? dateB - dateA : dateA - dateB;
  });

  return result;
});

const groupedHistory = computed((): Record<string, AppointmentDetail[]> => {
  const groups: Record<string, AppointmentDetail[]> = {};
  for (const record of filteredHistory.value) {
    const d = new Date(record.appointmentDate);
    const key = `Tháng ${d.getMonth() + 1}, ${d.getFullYear()}`;
    if (!groups[key]) groups[key] = [];
    groups[key].push(record);
  }
  return groups;
});

// ===== Modal =====
const openDetailModal = (record: AppointmentDetail) => {
  selectedRecord.value = record;
  showDetailModal.value = true;
};

// ===== Helpers =====
const getSpeciesEmoji = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species ?? ''] || '🐾';
};

const getAvatarGradient = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': 'linear-gradient(135deg, #f59e0b, #d97706)',
    'Mèo': 'linear-gradient(135deg, #8b5cf6, #6d28d9)',
    'Thỏ': 'linear-gradient(135deg, #ec4899, #be185d)',
    'Chim': 'linear-gradient(135deg, #06b6d4, #0891b2)',
    'Cá': 'linear-gradient(135deg, #3b82f6, #1d4ed8)',
    'Bò sát': 'linear-gradient(135deg, #10b981, #059669)',
  };
  return map[species ?? ''] || 'linear-gradient(135deg, #6b7280, #4b5563)';
};

const getStatusLabel = (status: string): string => {
  const map: Record<string, string> = {
    pending: 'Chờ xác nhận', confirmed: 'Đã xác nhận',
    in_progress: 'Đang khám', completed: 'Hoàn thành', cancelled: 'Đã huỷ',
  };
  return map[status] || status;
};

const getStatusIcon = (status: string): string => {
  const map: Record<string, string> = {
    pending: 'bi bi-hourglass-split', confirmed: 'bi bi-check-circle-fill',
    in_progress: 'bi bi-activity', completed: 'bi bi-check2-all', cancelled: 'bi bi-x-circle-fill',
  };
  return map[status] || 'bi bi-question-circle';
};

const getInvoiceLabel = (status: string): string => {
  const map: Record<string, string> = {
    unpaid: 'Chưa thanh toán', paid: 'Đã thanh toán', cancelled: 'Đã huỷ',
  };
  return map[status] || status;
};

const formatDateShort = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateFull = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

const formatCurrency = (amount: number | null | undefined): string => {
  if (!amount) return '';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const truncate = (str: string | null, maxLen: number): string => {
  if (!str) return '';
  return str.length > maxLen ? str.slice(0, maxLen) + '...' : str;
};

// ===== Lifecycle =====
onMounted(fetchHistory);
</script>

<style scoped>
/* ===== Layout ===== */
.myhistory-tab { padding: 0; }

/* ===== Hero ===== */
.history-hero {
  background: linear-gradient(135deg, #faf5ff 0%, #ede9fe 100%);
  border-radius: 16px;
  padding: 1.5rem 2rem;
  border: 1px solid #ddd6fe;
}

.summary-stat-box {
  background: white;
  border-radius: 12px;
  padding: 0.75rem 1.2rem;
  text-align: center;
  box-shadow: 0 2px 8px rgba(0,0,0,0.06);
  min-width: 80px;
}

.summary-stat-box.accent-green {
  background: linear-gradient(135deg, #d1fae5, #a7f3d0);
}

.stat-num {
  display: block;
  font-size: 1.3rem;
  font-weight: 800;
  color: #1a1a2e;
}

.stat-desc {
  display: block;
  font-size: 0.7rem;
  color: #6b7280;
  font-weight: 600;
  text-transform: uppercase;
}

/* ===== Filter Row ===== */
.filter-row {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: center;
}

.search-box {
  position: relative;
  flex: 1;
  min-width: 200px;
}

.search-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: #9ca3af;
  font-size: 0.9rem;
}

.search-input {
  width: 100%;
  padding: 0.55rem 0.9rem 0.55rem 2.5rem;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.88rem;
  outline: none;
  background: white;
  transition: border-color 0.2s;
}

.search-input:focus {
  border-color: #8b5cf6;
  box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.15);
}

.filter-select {
  padding: 0.55rem 1rem;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.85rem;
  background: white;
  outline: none;
  cursor: pointer;
  color: #374151;
  transition: border-color 0.2s;
}

.filter-select:focus {
  border-color: #8b5cf6;
}

/* ===== Empty State ===== */
.empty-history {
  background: white;
  border-radius: 20px;
  padding: 4rem 2rem;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0,0,0,0.06);
  border: 2px dashed #ddd6fe;
}

.empty-icon { font-size: 5rem; }

/* ===== Timeline ===== */
.history-timeline { display: flex; flex-direction: column; gap: 1.5rem; }

.month-group {}

.month-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  font-weight: 700;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  padding: 0.5rem 0;
  margin-bottom: 0.75rem;
  border-bottom: 2px solid #f0f0f0;
}

.month-count {
  margin-left: auto;
  background: #ede9fe;
  color: #7c3aed;
  padding: 2px 10px;
  border-radius: 10px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: none;
  letter-spacing: 0;
}

.history-cards {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-left: 10px;
  border-left: 2px solid #f0f0f0;
}

.history-card {
  display: flex;
  gap: 1rem;
  background: white;
  border-radius: 14px;
  padding: 1rem 1.1rem;
  box-shadow: 0 2px 10px rgba(0,0,0,0.06);
  cursor: pointer;
  transition: all 0.25s ease;
  border: 1px solid #f0f0f0;
  position: relative;
  margin-left: -10px;
}

.history-card:hover {
  transform: translateX(4px);
  box-shadow: 0 6px 20px rgba(0,0,0,0.1);
  border-color: #ddd6fe;
}

.history-card-left {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

.history-pet-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  border: 3px solid white;
  box-shadow: 0 3px 10px rgba(0,0,0,0.15);
}

.history-timeline-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #ddd6fe;
  flex-grow: 1;
  max-height: 20px;
}

.history-card-body {
  flex: 1;
  min-width: 0;
}

.history-card-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.5rem;
  gap: 0.5rem;
}

.history-pet-name {
  font-weight: 700;
  font-size: 0.95rem;
  color: #1a1a2e;
}

.history-date-badge {
  font-size: 0.75rem;
  color: #6b7280;
  font-weight: 600;
  white-space: nowrap;
  background: #f9fafb;
  padding: 2px 10px;
  border-radius: 10px;
  border: 1px solid #e5e7eb;
}

/* Service row */
.history-service-row {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 0.5rem;
}

.service-tag {
  background: #ede9fe;
  color: #5b21b6;
  padding: 3px 10px;
  border-radius: 10px;
  font-size: 0.78rem;
  font-weight: 600;
}

.doctor-tag {
  background: #f0fdf4;
  color: #065f46;
  padding: 3px 10px;
  border-radius: 10px;
  font-size: 0.78rem;
  font-weight: 600;
}

.history-symptom {
  font-size: 0.82rem;
  color: #6b7280;
  margin-bottom: 0.6rem;
  line-height: 1.4;
}

/* Footer */
.history-card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 6px;
}

.invoice-pill {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 0.72rem;
  font-weight: 700;
}

.inv-paid { background: #d1fae5; color: #065f46; }
.inv-unpaid { background: #fef3c7; color: #92400e; }
.inv-cancelled { background: #f3f4f6; color: #6b7280; }

.total-amount {
  font-size: 0.88rem;
  font-weight: 700;
  color: #059669;
}

.btn-view-history {
  background: transparent;
  border: 1.5px solid #ddd6fe;
  color: #7c3aed;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.78rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-view-history:hover {
  background: #ede9fe;
}

/* ===== Detail Modal ===== */
.history-modal-overlay {
  position: fixed;
  top: 0; left: 0;
  width: 100vw; height: 100vh;
  background: rgba(0,0,0,0.45);
  backdrop-filter: blur(6px);
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.history-modal-card {
  background: white;
  border-radius: 20px;
  width: 100%;
  max-width: 580px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 60px rgba(0,0,0,0.2);
}

.history-modal-header {
  padding: 2rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  border-radius: 20px 20px 0 0;
  position: relative;
}

.modal-pet-avatar {
  font-size: 3.5rem;
  filter: drop-shadow(0 4px 8px rgba(0,0,0,0.2));
}

.modal-close-btn {
  background: rgba(255,255,255,0.2);
  border: none;
  cursor: pointer;
  font-size: 1rem;
  color: white;
  padding: 0.4rem 0.6rem;
  border-radius: 8px;
  transition: all 0.2s;
  position: absolute;
  top: 1rem;
  right: 1rem;
}

.modal-close-btn:hover {
  background: rgba(255,255,255,0.35);
}

.history-modal-body { padding: 1.5rem; }

/* Sections */
.medical-section {
  border: 1px solid #f0f0f0;
  border-radius: 12px;
  padding: 1rem 1.1rem;
  background: #fafafa;
}

.section-title {
  font-weight: 700;
  color: #374151;
  margin-bottom: 0.75rem;
  font-size: 0.88rem;
}

.medi-item {
  background: white;
  border-radius: 8px;
  padding: 8px 12px;
  border: 1px solid #f0f0f0;
}

.medi-label {
  display: block;
  font-size: 0.7rem;
  font-weight: 600;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  margin-bottom: 2px;
}

.medi-value {
  font-size: 0.88rem;
  font-weight: 600;
  color: #1f2937;
}

/* Status pill */
.status-pill {
  display: inline-flex;
  align-items: center;
  padding: 5px 14px;
  border-radius: 20px;
  font-size: 0.82rem;
  font-weight: 700;
}

.status-completed { background: #d1fae5; color: #065f46; }
.status-cancelled { background: #f3f4f6; color: #4b5563; }
.status-in_progress { background: #ede9fe; color: #5b21b6; }

/* Invoice summary */
.invoice-summary-card {
  border-radius: 10px;
  padding: 12px 16px;
  border: 1px solid #e5e7eb;
}

.inv-paid-bg { background: #f0fdf4; border-color: #bbf7d0; }
.inv-unpaid-bg { background: #fffbeb; border-color: #fde68a; }

.invoice-total-display {
  font-size: 1.3rem;
  font-weight: 800;
  color: #059669;
}

/* Modal transition */
.modal-fade-enter-active, .modal-fade-leave-active { transition: all 0.3s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: scale(0.95); }
</style>
