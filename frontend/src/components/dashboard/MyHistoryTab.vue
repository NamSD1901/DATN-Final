<template>
  <div class="myhistory-tab">
    
    <!-- Header -->
    <div class="history-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-journal-medical me-2" style="color: var(--primary-gold);"></i>
            Hồ sơ & Lịch sử bệnh án
          </h3>
          <p class="text-muted mb-0 small">Chọn thú cưng bên dưới để theo dõi chi tiết dòng thời gian bệnh án và điều trị y khoa.</p>
        </div>
      </div>
    </div>

    <!-- Pet Selection Pills -->
    <div class="pet-selector-container mb-4">
      <h5 class="fw-semibold text-dark mb-3">
        <i class="bi bi-tag-fill me-2 text-warning"></i>Thú cưng của bạn:
      </h5>
      <div class="d-flex gap-3 flex-wrap">
        <button
          v-for="pet in myPets"
          :key="pet.id"
          class="pet-pill-btn d-flex align-items-center gap-2"
          :class="{ active: selectedPetId === pet.id }"
          @click="selectPet(pet.id)"
        >
          <span class="pet-emoji">{{ getSpeciesEmoji(pet.species) }}</span>
          <span class="pet-name">{{ pet.name }}</span>
        </button>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải bệnh án...</p>
    </div>

    <!-- Error state -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 border-0 shadow-sm py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- No Pet Selected -->
    <div v-else-if="selectedPetId === null" class="empty-history text-center py-5">
      <div class="empty-icon">🐾</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">Chưa chọn thú cưng</h5>
      <p class="text-muted small mb-0">Vui lòng chọn một thú cưng ở trên để xem lịch sử khám và hồ sơ bệnh án.</p>
    </div>

    <!-- Empty history for selected pet -->
    <div v-else-if="records.length === 0" class="empty-history text-center py-5">
      <div class="empty-icon">🔍</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">Chưa có lịch sử bệnh án</h5>
      <p class="text-muted small mb-0">Thú cưng này chưa hoàn thành đợt điều trị hay khám bệnh nào có bệnh án được lưu lại.</p>
    </div>

    <!-- Timeline records -->
    <div v-else class="medical-history-timeline mt-4">
      <div class="timeline-container">
        <div v-for="record in records" :key="record.recordId" class="timeline-item">
          <!-- Timeline dot -->
          <div class="timeline-badge">
            <div class="badge-inner">
              <i class="bi bi-heartpulse-fill text-white"></i>
            </div>
          </div>
          
          <!-- Timeline content card (Glassmorphism) -->
          <div class="timeline-card">
            <div class="card-header-main d-flex justify-content-between align-items-center flex-wrap gap-2">
              <div class="d-flex align-items-center gap-2">
                <span class="visit-date fw-bold">
                  <i class="bi bi-calendar-check me-1"></i>{{ formatDateFull(record.visitDate) }}
                </span>
              </div>
              <div class="doctor-badge">
                <i class="bi bi-person-badge me-1"></i>Bác sĩ: <span class="fw-semibold">{{ record.doctorName || 'Chưa rõ' }}</span>
              </div>
            </div>

            <div class="card-body-main mt-3">
              <!-- Vitals Row -->
              <div class="vitals-grid mb-3">
                <div class="vital-card">
                  <span class="vital-icon">⚖️</span>
                  <span class="vital-label">Cân nặng</span>
                  <span class="vital-val">{{ record.weight ? `${record.weight} kg` : '—' }}</span>
                </div>
                <div class="vital-card">
                  <span class="vital-icon">🌡️</span>
                  <span class="vital-label">Nhiệt độ</span>
                  <span class="vital-val">{{ record.temperature ? `${record.temperature} °C` : '—' }}</span>
                </div>
                <div class="vital-card">
                  <span class="vital-icon">💓</span>
                  <span class="vital-label">Nhịp tim</span>
                  <span class="vital-val">{{ record.heartRate ? `${record.heartRate} bpm` : '—' }}</span>
                </div>
              </div>

              <!-- Diagnosis & Symptoms -->
              <div class="medical-details mb-3">
                <div class="detail-block mb-2">
                  <span class="detail-label"><i class="bi bi-chat-left-dots-fill text-warning me-1"></i>Triệu chứng lâm sàng:</span>
                  <p class="detail-content">{{ record.symptoms || 'Không ghi nhận' }}</p>
                </div>
                
                <div class="detail-block diagnosis-block mb-2">
                  <span class="detail-label"><i class="bi bi-activity text-danger me-1"></i>Chẩn đoán y khoa:</span>
                  <p class="detail-content fw-bold text-dark">{{ record.diagnosis || 'Chưa ghi nhận' }}</p>
                </div>

                <div class="detail-block treatment-block mb-2">
                  <span class="detail-label"><i class="bi bi-file-earmark-medical-fill text-success me-1"></i>Phác đồ & Kế hoạch điều trị:</span>
                  <p class="detail-content text-dark">{{ record.treatment || 'Chưa ghi nhận' }}</p>
                </div>
              </div>

              <!-- Prescribed Medicine -->
              <div v-if="record.prescribedMedicines && record.prescribedMedicines.length > 0" class="medicine-section mb-3">
                <span class="detail-label"><i class="bi bi-capsule text-primary me-1"></i>Đơn thuốc chỉ định:</span>
                <div class="medicine-pills mt-2">
                  <span v-for="med in record.prescribedMedicines" :key="med" class="medicine-badge">
                    💊 {{ med }}
                  </span>
                </div>
              </div>

              <!-- Note -->
              <div v-if="record.note" class="detail-block note-block">
                <span class="detail-label text-muted"><i class="bi bi-journal-text me-1"></i>Ghi chú thêm:</span>
                <p class="detail-content text-muted mb-0 small">{{ record.note }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../services/api';

// ===== Types =====
interface Pet {
  id: number;
  name: string;
  species: string;
  breed?: string;
  weight?: number;
}

interface MedicalRecord {
  recordId: number;
  appointmentId: number;
  petId: number;
  petName: string;
  visitDate: string;
  diagnosis: string;
  treatment: string;
  doctorName: string;
  doctorId: string;
  weight?: number;
  temperature?: number;
  heartRate?: number;
  symptoms: string;
  note: string;
  prescribedMedicines: string[];
}

// ===== State =====
const myPets = ref<Pet[]>([]);
const selectedPetId = ref<number | null>(null);
const records = ref<MedicalRecord[]>([]);
const loading = ref(false);
const errorMsg = ref('');

// ===== API =====
const fetchPets = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/mypets');
    myPets.value = res.data;
    if (myPets.value.length > 0) {
      selectedPetId.value = myPets.value[0].id;
      await fetchMedicalHistory(myPets.value[0].id);
    }
  } catch (err: any) {
    errorMsg.value = 'Không thể tải danh sách thú cưng. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const fetchMedicalHistory = async (petId: number) => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get(`/my-appointments/pets/${petId}/medical-history`);
    records.value = res.data;
  } catch (err: any) {
    errorMsg.value = 'Không thể tải lịch sử bệnh án. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const selectPet = async (petId: number) => {
  selectedPetId.value = petId;
  await fetchMedicalHistory(petId);
};

// ===== Helpers =====
const getSpeciesEmoji = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species ?? ''] || '🐾';
};

const formatDateFull = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

// ===== Lifecycle =====
onMounted(fetchPets);
</script>

<style scoped>
/* ===== Layout ===== */
.myhistory-tab {
  padding: 0;
  color: #1f2937;
}

/* ===== Hero ===== */
.history-hero {
  background: linear-gradient(135deg, rgba(251, 191, 36, 0.08) 0%, rgba(16, 185, 129, 0.08) 100%);
  backdrop-filter: blur(12px);
  border-radius: 20px;
  padding: 1.75rem 2rem;
  border: 1px solid rgba(251, 191, 36, 0.15);
  box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.03);
}

/* ===== Pet Selector ===== */
.pet-pill-btn {
  background: rgba(255, 255, 255, 0.65);
  backdrop-filter: blur(8px);
  border: 1.5px solid #e5e7eb;
  padding: 0.6rem 1.2rem;
  border-radius: 30px;
  font-size: 0.92rem;
  font-weight: 600;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.02);
}

.pet-pill-btn:hover {
  transform: translateY(-2px);
  background: white;
  border-color: #fbbf24;
  box-shadow: 0 4px 12px rgba(251, 191, 36, 0.15);
}

.pet-pill-btn.active {
  background: linear-gradient(135deg, #fbbf24, #d97706);
  border-color: transparent;
  color: white;
  box-shadow: 0 8px 20px rgba(217, 119, 6, 0.3);
}

.pet-emoji {
  font-size: 1.2rem;
}

/* ===== Empty State ===== */
.empty-history {
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(12px);
  border-radius: 24px;
  padding: 4.5rem 2rem;
  box-shadow: 0 8px 32px rgba(31, 38, 135, 0.04);
  border: 2px dashed rgba(229, 231, 235, 0.8);
}

.empty-icon {
  font-size: 3.5rem;
  animation: float 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-8px); }
}

/* ===== Timeline ===== */
.medical-history-timeline {
  position: relative;
  padding-left: 2rem;
}

.timeline-container {
  position: relative;
}

.timeline-container::before {
  content: '';
  position: absolute;
  top: 0;
  bottom: 0;
  left: -1.25rem;
  width: 3px;
  background: linear-gradient(180deg, rgba(251, 191, 36, 0.6) 0%, rgba(16, 185, 129, 0.6) 100%);
  border-radius: 2px;
}

.timeline-item {
  position: relative;
  margin-bottom: 2rem;
}

.timeline-badge {
  position: absolute;
  left: -2.1rem;
  top: 0.5rem;
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, #10b981, #059669);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 0 4px white, 0 4px 10px rgba(16, 185, 129, 0.3);
  z-index: 2;
}

.badge-inner {
  font-size: 0.8rem;
}

.timeline-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.04);
  transition: all 0.3s ease;
}

.timeline-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.08);
  border-color: rgba(251, 191, 36, 0.3);
}

.card-header-main {
  border-bottom: 1px dashed rgba(229, 231, 235, 0.8);
  padding-bottom: 0.75rem;
}

.visit-date {
  font-size: 1.05rem;
  color: #111827;
}

.doctor-badge {
  background: rgba(16, 185, 129, 0.1);
  color: #065f46;
  padding: 0.35rem 0.85rem;
  border-radius: 20px;
  font-size: 0.82rem;
}

/* Vitals */
.vitals-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
  gap: 12px;
}

.vital-card {
  background: rgba(243, 244, 246, 0.6);
  border: 1px solid rgba(229, 231, 235, 0.5);
  border-radius: 12px;
  padding: 0.6rem 0.8rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.vital-icon {
  font-size: 1.1rem;
  margin-bottom: 2px;
}

.vital-label {
  font-size: 0.75rem;
  color: #6b7280;
  font-weight: 500;
}

.vital-val {
  font-size: 0.95rem;
  font-weight: 700;
  color: #1f2937;
  margin-top: 2px;
}

/* Detail blocks */
.detail-block {
  background: rgba(255, 255, 255, 0.4);
  padding: 0.85rem 1rem;
  border-radius: 12px;
  border: 1px solid rgba(229, 231, 235, 0.3);
}

.detail-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: #4b5563;
  margin-bottom: 4px;
}

.detail-content {
  font-size: 0.92rem;
  color: #1f2937;
  line-height: 1.5;
  margin-bottom: 0;
}

.diagnosis-block {
  background: rgba(239, 68, 68, 0.03);
  border-left: 3px solid #ef4444;
}

.treatment-block {
  background: rgba(16, 185, 129, 0.03);
  border-left: 3px solid #10b981;
}

.note-block {
  background: rgba(107, 114, 128, 0.03);
  border-left: 3px solid #6b7280;
}

/* Medicine */
.medicine-badge {
  display: inline-block;
  background: rgba(59, 130, 246, 0.1);
  color: #1e40af;
  padding: 0.3rem 0.8rem;
  border-radius: 20px;
  font-size: 0.82rem;
  font-weight: 600;
  margin-right: 6px;
  margin-bottom: 6px;
}
</style>
