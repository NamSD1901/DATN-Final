<template>
  <div class="myhistory-tab">
    
    <!-- Header -->
    <div class="history-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-journal-medical me-2" style="color: var(--primary-gold);"></i>
            {{ $t('history.title') }}
          </h3>
          <p class="text-muted mb-0 small">{{ $t('history.subtitle') }}</p>
        </div>
      </div>
    </div>

    <!-- Pet Selection Pills -->
    <div class="pet-selector-container mb-4">
      <h5 class="fw-semibold text-dark mb-3">
        <i class="bi bi-tag-fill me-2 text-warning"></i>{{ $t('history.yourPets') }}
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
      <p class="text-muted mt-3">{{ $t('history.syncing') }}</p>
    </div>

    <!-- Error state -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 border-0 shadow-sm py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- No Pet Selected -->
    <div v-else-if="selectedPetId === null" class="empty-history text-center py-5">
      <div class="empty-icon">🐶</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">{{ $t('history.noPetSelectedTitle') }}</h5>
      <p class="text-muted small mb-0">{{ $t('history.noPetSelectedDesc') }}</p>
    </div>

    <!-- Empty history for selected pet -->
    <div v-else-if="records.length === 0" class="empty-history text-center py-5">
      <div class="empty-icon">📁</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">{{ $t('history.noHistoryTitle') }}</h5>
      <p class="text-muted small mb-0">{{ $t('history.noHistoryDesc') }}</p>
    </div>

    <div v-else>
      <!-- Vitals Chart Section -->
      <div class="chart-container mb-5" v-if="chartData.labels.length > 1">
        <h5 class="fw-semibold mb-3"><i class="bi bi-graph-up-arrow me-2 text-primary"></i>Biểu đồ Sinh hiệu</h5>
        <div class="chart-wrapper p-3 bg-white rounded-4 shadow-sm border">
          <Line :data="chartData" :options="chartOptions" style="max-height: 250px;" />
        </div>
      </div>

      <!-- Timeline records (Accordion Style) -->
      <h5 class="fw-semibold mb-4"><i class="bi bi-clock-history me-2 text-success"></i>Dòng thời gian y khoa</h5>
      <div class="medical-history-timeline mt-2" id="medical-records-export-area">
        <div class="timeline-container accordion" id="historyAccordion">
          
          <div v-for="(record, index) in records" :key="record.recordId" class="timeline-item">
            <!-- Timeline dot -->
            <div class="timeline-badge" :class="record.recordType === 'Vaccination' ? 'bg-success' : 'bg-primary'">
              <div class="badge-inner">
                <i v-if="record.recordType === 'Vaccination'" class="bi bi-shield-plus text-white"></i>
                <i v-else class="bi bi-heart-pulse-fill text-white"></i>
              </div>
            </div>
            
            <!-- Timeline content card (Glassmorphism + Accordion) -->
            <div class="timeline-card accordion-item border-0 bg-transparent">
              <h2 class="accordion-header card-header-main" :id="'heading' + record.recordId">
                <button 
                  class="accordion-button shadow-none bg-white rounded-top-4" 
                  :class="{ 'collapsed': !expandedRecords.includes(record.recordId) }"
                  type="button" 
                  @click="toggleRecord(record.recordId)"
                >
                  <div class="d-flex flex-column w-100 pe-3">
                    <div class="d-flex justify-content-between align-items-center w-100 mb-1">
                      <span class="visit-date fw-bold text-dark">
                        {{ formatDateFull(record.visitDate) }}
                      </span>
                      <span class="badge rounded-pill" :class="record.recordType === 'Vaccination' ? 'bg-success' : 'bg-primary'">
                        {{ record.serviceName || (record.recordType === 'Vaccination' ? 'Tiêm phòng' : 'Khám bệnh') }}
                      </span>
                    </div>
                    <div class="d-flex justify-content-between align-items-center w-100">
                      <span class="doctor-badge mb-0 text-muted small">
                        <i class="bi bi-person-badge me-1"></i>BS: <span class="fw-semibold text-dark">{{ record.doctorName || 'Chưa rõ' }}</span>
                      </span>
                      <span v-if="record.diagnosis" class="text-truncate text-muted small ms-2" style="max-width: 200px;">
                        {{ record.diagnosis }}
                      </span>
                    </div>
                  </div>
                </button>
              </h2>

              <div 
                :id="'collapse' + record.recordId" 
                class="accordion-collapse collapse" 
                :class="{ 'show': expandedRecords.includes(record.recordId) }"
              >
                <div class="accordion-body card-body-main bg-white border-top border-light rounded-bottom-4 shadow-sm" :id="'record-content-' + record.recordId">
                  
                  <!-- Action Bar -->
                  <div class="d-flex justify-content-end mb-3 action-bar hide-on-print">
                    <button @click="downloadPDF(record.recordId)" class="btn btn-sm btn-outline-danger me-2">
                      <i class="bi bi-file-pdf me-1"></i>Xuất PDF
                    </button>
                    <div v-if="record.invoiceId" class="badge bg-light text-dark border d-flex align-items-center px-3">
                      <i class="bi bi-receipt me-2 text-secondary"></i>
                      <span class="me-2">Hóa đơn: <strong>#INV-{{ record.invoiceId }}</strong></span>
                      <span v-if="record.invoiceStatus === 'paid'" class="badge bg-success">Đã thanh toán</span>
                      <span v-else class="badge bg-warning text-dark">Chờ thanh toán</span>
                    </div>
                  </div>

                  <!-- PDF Header (Only visible in PDF) -->
                  <div class="pdf-header d-none mb-4 text-center">
                    <h2 class="text-primary fw-bold mb-1">MYPET CLINIC</h2>
                    <p class="mb-0">Hồ Sơ Bệnh Án Điện Tử</p>
                    <hr>
                  </div>

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
                  </div>

                  <!-- Simplified Medical Details -->
                  <div class="medical-details mb-4">
                    <div class="mb-4">
                      <div class="small fw-bold text-muted mb-2 text-uppercase" style="letter-spacing: 0.5px;">Chẩn đoán của bác sĩ:</div>
                      <div class="text-dark fw-bold p-3 bg-light rounded-3 border" style="font-size: 1.1rem;">{{ record.diagnosis || 'Chưa có chẩn đoán' }}</div>
                    </div>
                    
                    <div v-if="record.careInstructions || record.doctorNotes" class="mt-4 p-3 bg-warning bg-opacity-10 rounded-3 border-start border-warning border-3">
                      <div class="small fw-bold text-warning-emphasis mb-2"><i class="bi bi-info-circle-fill me-1"></i>Lời dặn dò chăm sóc:</div>
                      <div class="text-dark fw-medium" style="line-height: 1.6;">{{ record.careInstructions || record.doctorNotes }}</div>
                    </div>
                  </div>

                  <!-- Prescribed Medicine Detailed -->
                  <div v-if="record.prescribedMedicines && record.prescribedMedicines.length > 0" class="medicine-section mb-4">
                    <span class="detail-label mb-2"><i class="bi bi-capsule text-primary me-1"></i>Đơn thuốc chỉ định:</span>
                    <div class="table-responsive">
                      <table class="table table-sm table-bordered medicine-table">
                        <thead class="table-light">
                          <tr>
                            <th>Tên thuốc</th>
                            <th>Số lượng</th>
                            <th>Liều dùng</th>
                            <th>Cách dùng</th>
                            <th>Lời dặn</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="(med, mIndex) in record.prescribedMedicines" :key="mIndex">
                            <td class="fw-semibold text-primary">{{ med.medicineName }}</td>
                            <td>{{ med.quantity || '-' }}</td>
                            <td>{{ med.dosage || '-' }}</td>
                            <td>{{ med.frequency || '-' }} ({{ med.durationDays ? med.durationDays + ' ngày' : '-' }})</td>
                            <td class="text-muted small">{{ med.instruction || '-' }}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </div>

                  <!-- Note -->
                  <div v-if="record.doctorNotes" class="detail-block note-block mb-3">
                    <span class="detail-label text-muted"><i class="bi bi-journal-text me-1"></i>Ghi chú của bác sĩ:</span>
                    <p class="detail-content text-muted mb-0 small">{{ record.doctorNotes }}</p>
                  </div>

                  <!-- Follow Up -->
                  <div v-if="record.followUpDate" class="detail-block bg-warning bg-opacity-10 border-warning mt-3">
                    <span class="detail-label text-dark-gold"><i class="bi bi-calendar-event me-1"></i>Ngày hẹn tái khám:</span>
                    <p class="detail-content text-dark fw-bold mb-0 small">{{ formatDateFull(record.followUpDate) }}</p>
                  </div>
                  
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api';
// @ts-ignore
import html2pdf from 'html2pdf.js';

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js'
import { Line } from 'vue-chartjs'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

// ===== Types =====
interface Pet {
  id: number;
  name: string;
  species: string;
  breed?: string;
  weight?: number;
}

interface PrescribedMedicine {
  medicineName: string;
  dosage?: string;
  frequency?: string;
  durationDays?: number;
  quantity?: number;
  instruction?: string;
}

interface MedicalRecord {
  recordId: number;
  appointmentId: number;
  serviceName: string;
  recordType: string;
  visitDate: string;
  doctorName: string;
  weight?: number;
  temperature?: number;
  diagnosis: string;
  careInstructions: string;
  doctorNotes?: string;
  followUpDate?: string;
  prescribedMedicines: PrescribedMedicine[];
  invoiceId?: number;
  invoiceStatus?: string;
  invoiceTotalAmount?: number;
}

// ===== State =====
const myPets = ref<Pet[]>([]);
const selectedPetId = ref<number | null>(null);
const records = ref<MedicalRecord[]>([]);
const loading = ref(false);
const errorMsg = ref('');

const expandedRecords = ref<number[]>([]);
const toggleRecord = (recordId: number) => {
  const index = expandedRecords.value.indexOf(recordId);
  if (index > -1) {
    expandedRecords.value.splice(index, 1);
  } else {
    expandedRecords.value.push(recordId);
  }
};



const formatDiagnosis = (diag: string) => {
  if (!diag) return 'Không';
  try {
    if (diag.trim().startsWith('{')) {
      const parsed = JSON.parse(diag);
      const mainDiag = parsed.definitiveDiagnosis || parsed.tentativeDiagnosis;
      let result = mainDiag ? mainDiag : '';
      if (parsed.differentialDiagnosis && parsed.differentialDiagnosis !== 'Không') {
        result += result ? ` | Phân biệt: ${parsed.differentialDiagnosis}` : `Phân biệt: ${parsed.differentialDiagnosis}`;
      }
      if (parsed.diseaseSeverity && parsed.diseaseSeverity !== 'Nhẹ') {
        result += result ? ` | Mức độ: ${parsed.diseaseSeverity}` : `Mức độ: ${parsed.diseaseSeverity}`;
      }
      return result || 'Không';
    }
  } catch (e) {
    // Ignore error
  }
  return diag;
};

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

// ===== Chart Logic =====
const chartData = computed(() => {
  // Sort records chronologically for the chart
  const sorted = [...records.value].reverse();
  const labels = sorted.map(r => new Date(r.visitDate).toLocaleDateString('vi-VN'));
  const weights = sorted.map(r => r.weight || null);
  const temps = sorted.map(r => r.temperature || null);

  return {
    labels,
    datasets: [
      {
        label: 'Cân nặng (kg)',
        backgroundColor: '#3b82f6',
        borderColor: '#3b82f6',
        data: weights,
        yAxisID: 'y'
      },
      {
        label: 'Nhiệt độ (°C)',
        backgroundColor: '#ef4444',
        borderColor: '#ef4444',
        data: temps,
        yAxisID: 'y1'
      }
    ]
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index' as const,
    intersect: false,
  },
  plugins: {
    legend: { position: 'top' as const }
  },
  scales: {
    y: {
      type: 'linear' as const,
      display: true,
      position: 'left' as const,
      title: { display: true, text: 'Cân nặng (kg)' }
    },
    y1: {
      type: 'linear' as const,
      display: true,
      position: 'right' as const,
      grid: { drawOnChartArea: false },
      title: { display: true, text: 'Nhiệt độ (°C)' }
    }
  }
};

// ===== PDF Export =====
const downloadPDF = (recordId: number) => {
  const element = document.getElementById(`record-content-${recordId}`);
  if (!element) return;
  
  // Clone element to modify for print
  const clone = element.cloneNode(true) as HTMLElement;
  
  // Show PDF headers, hide action bars
  const pdfHeader = clone.querySelector('.pdf-header');
  if (pdfHeader) pdfHeader.classList.remove('d-none');
  
  const actionBar = clone.querySelector('.hide-on-print');
  if (actionBar) actionBar.remove();

  // Create temporary container
  const tempDiv = document.createElement('div');
  tempDiv.appendChild(clone);
  tempDiv.style.padding = '20px';
  tempDiv.style.backgroundColor = 'white';
  tempDiv.style.color = 'black';

  const opt = {
    margin:       0.5,
    filename:     `BenhAn_MyPetClinic_${recordId}.pdf`,
    image:        { type: 'jpeg', quality: 0.98 },
    html2canvas:  { scale: 2 },
    jsPDF:        { unit: 'in', format: 'letter', orientation: 'portrait' }
  };

  html2pdf().set(opt).from(tempDiv).save();
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
    day: '2-digit', month: '2-digit', year: 'numeric',
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
  background: linear-gradient(180deg, rgba(59, 130, 246, 0.4) 0%, rgba(16, 185, 129, 0.4) 100%);
  border-radius: 2px;
}

.timeline-item {
  position: relative;
  margin-bottom: 1.5rem;
}

.timeline-badge {
  position: absolute;
  left: -2.1rem;
  top: 0.8rem;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 0 4px #f3f4f6;
  z-index: 2;
}

.badge-inner {
  font-size: 0.8rem;
}

/* ===== Vitals ===== */
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

/* ===== Detail blocks ===== */
.detail-block {
  background: rgba(249, 250, 251, 0.8);
  padding: 0.85rem 1rem;
  border-radius: 12px;
  border: 1px solid rgba(229, 231, 235, 0.5);
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

/* ===== Medicine Table ===== */
.medicine-table {
  font-size: 0.85rem;
  border-radius: 8px;
  overflow: hidden;
}
.medicine-table th {
  background-color: #f8fafc;
  color: #475569;
  font-weight: 600;
  border-bottom-width: 2px;
}
.medicine-table td {
  vertical-align: middle;
}
</style>
