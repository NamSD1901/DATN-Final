<template>
  <div v-if="isOpen" class="zalo-modal-overlay" @click.self="closeModal">
    <div class="zalo-modal-card modal-xl" style="max-width: 900px;">
      <div class="zalo-modal-header bg-info text-dark">
        <h5 class="modal-title fw-bold"><i class="bi bi-journal-medical me-2"></i> Hồ Sơ Y Tế Toàn Diện & Biểu Đồ</h5>
        <button class="modal-close text-dark border-0 bg-transparent" @click="closeModal"><i class="bi bi-x-lg fs-5"></i></button>
      </div>
      <div class="zalo-modal-body p-0 text-start bg-light">
        <!-- Summary Header -->
        <div class="p-4 bg-white border-bottom shadow-sm">
          <div class="row align-items-center">
            <div class="col-md-6 d-flex align-items-center gap-3">
              <span class="pet-avatar-large shadow-sm" style="font-size: 2.5rem; background-color: #e0f2fe; border-color: #0ea5e9;">{{ getAnimalEmoji(pet?.species || '') }}</span>
              <div>
                <h4 class="fw-bold text-dark mb-0">{{ pet?.name }}</h4>
                <p class="text-muted small mb-0">{{ pet?.breed || pet?.species }} | Cân nặng HT: {{ pet?.weight ? pet.weight + ' kg' : '—' }}</p>
              </div>
            </div>
            <div class="col-md-6 text-md-end mt-3 mt-md-0">
              <div v-if="pet?.allergyNote" class="d-inline-block text-start p-2 bg-danger bg-opacity-10 text-danger rounded small fw-bold">
                <i class="bi bi-exclamation-triangle-fill me-1"></i> Dị ứng: {{ pet.allergyNote }}
              </div>
            </div>
          </div>
        </div>
        
        <!-- Tabs Navigation -->
        <div class="px-4 pt-3 bg-white border-bottom">
          <ul class="nav nav-tabs border-0">
            <li class="nav-item">
              <a class="nav-link fw-bold border-0" :class="{ 'active text-info border-bottom border-info border-3': activeHistoryTab === 'consultation', 'text-muted': activeHistoryTab !== 'consultation' }" href="#" @click.prevent="activeHistoryTab = 'consultation'">
                <i class="bi bi-file-medical-fill me-1"></i> Lịch Sử Khám Bệnh
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link fw-bold border-0" :class="{ 'active text-info border-bottom border-info border-3': activeHistoryTab === 'vaccination', 'text-muted': activeHistoryTab !== 'vaccination' }" href="#" @click.prevent="activeHistoryTab = 'vaccination'">
                <i class="bi bi-shield-fill-check me-1"></i> Sổ Tiêm Phòng
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link fw-bold border-0" :class="{ 'active text-info border-bottom border-info border-3': activeHistoryTab === 'weight-chart', 'text-muted': activeHistoryTab !== 'weight-chart' }" href="#" @click.prevent="activeHistoryTab = 'weight-chart'">
                <i class="bi bi-graph-up text-primary me-1"></i> Biểu Đồ Tăng Trưởng
              </a>
            </li>
          </ul>
        </div>

        <!-- Tabs Content -->
        <div class="p-4 overflow-auto" style="max-height: 60vh;">
          <div v-if="loading" class="text-center py-5">
            <div class="spinner-border text-info" role="status"></div>
            <p class="text-muted mt-2">Đang tải hồ sơ y tế...</p>
          </div>
          
          <template v-else>
            <!-- Consultation History Tab -->
            <div v-if="activeHistoryTab === 'consultation'">
              <div v-if="consultationHistory.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-folder-x fs-1 d-block mb-3 opacity-25"></i>
                Chưa có bệnh án lưu trữ.
              </div>
              <div v-else class="medical-timeline pe-2">
                <div v-for="record in consultationHistory" :key="record.recordId" class="timeline-item position-relative ps-4 pb-4">
                  <div class="timeline-line"></div>
                  <div class="timeline-circle bg-info shadow-sm"></div>
                  
                  <div class="timeline-content bg-white rounded-4 shadow-sm border overflow-hidden">
                    <!-- Collapsible Header -->
                    <div class="d-flex justify-content-between align-items-center p-3 cursor-pointer" 
                         :class="expandedConsultations.includes(record.recordId) ? 'bg-light border-bottom' : ''"
                         @click="toggleConsultation(record.recordId)" style="cursor: pointer;">
                      <div>
                        <span class="text-dark fw-bold fs-6 me-3"><i class="bi bi-calendar-check text-info me-2"></i> {{ formatDate(record.visitDate) }}</span>
                        <span class="badge bg-light border text-dark shadow-sm px-3 py-1 rounded-pill"><i class="bi bi-person-badge text-muted me-1"></i> Bs. {{ getLastWord(record.doctorName) }}</span>
                        <span v-if="!expandedConsultations.includes(record.recordId)" class="ms-3 small text-muted text-truncate d-inline-block" style="max-width: 250px; vertical-align: bottom;">
                          <i class="bi bi-stethoscope text-primary me-1"></i> {{ record.diagnosis || 'Khám bệnh' }}
                        </span>
                      </div>
                      <i class="bi text-muted fs-5" :class="expandedConsultations.includes(record.recordId) ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                    </div>
                    
                    <!-- Expanded Content -->
                    <div v-if="expandedConsultations.includes(record.recordId)" class="p-4 pt-3">
                      <div class="row g-3 small">
                        <div class="col-12 border-bottom pb-2 mb-2 d-flex justify-content-between">
                          <div>
                            <span class="text-muted fw-bold">Nhiệt độ:</span> {{ record.temperature || '—' }} °C
                          </div>
                          <div>
                            <span class="text-muted fw-bold">Cân nặng khi khám:</span> {{ record.weight || '—' }} kg
                          </div>
                        </div>
                        <div v-if="record.clinicalSigns" class="col-12 border-bottom pb-2">
                          <div class="text-muted mb-1 fw-bold">Khám lâm sàng:</div>
                          <div class="text-dark">{{ record.clinicalSigns }}</div>
                        </div>
                        <div class="col-md-6 border-end">
                          <div class="text-muted mb-1 fw-bold text-danger">Chẩn đoán:</div>
                          <div class="fw-bold text-dark">{{ record.diagnosis }}</div>
                        </div>
                        <div class="col-md-6">
                          <div class="text-muted mb-1 fw-bold text-primary">Phương pháp điều trị:</div>
                          <div class="text-dark">{{ record.treatmentPlan }}</div>
                        </div>
                        <div v-if="record.doctorNotes" class="col-12 mt-2">
                          <div class="p-3 bg-light rounded-3 fst-italic text-muted border-start border-3 border-info">"{{ record.doctorNotes }}"</div>
                        </div>
                      </div>
                      
                      <div v-if="record.prescribedMedicines && record.prescribedMedicines.length > 0" class="mt-3 bg-light p-3 rounded-4 border shadow-sm">
                        <span class="fw-bold text-success d-block small mb-2"><i class="bi bi-capsule-pill me-1"></i>Thuốc đã kê đơn:</span>
                        <ul class="list-unstyled mb-0 ps-2">
                          <li v-for="(medStr, mIdx) in record.prescribedMedicines" :key="mIdx" class="text-dark small mb-2 d-flex align-items-start">
                            <i class="bi bi-check-circle-fill text-success me-2 mt-1" style="font-size: 0.7rem;"></i> {{ medStr.medicineName }} - Số lượng: {{ medStr.quantity }}
                          </li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Vaccination History Tab -->
            <div v-if="activeHistoryTab === 'vaccination'">
              <div v-if="vaccinationHistory.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-shield-x fs-1 d-block mb-3 opacity-25"></i>
                Chưa có lịch sử tiêm phòng.
              </div>
              <div v-else class="medical-timeline pe-2">
                <div v-for="vac in vaccinationHistory" :key="vac.id" class="timeline-item position-relative ps-4 pb-4">
                  <div class="timeline-line"></div>
                  <div class="timeline-circle bg-success shadow-sm"></div>
                  
                  <div class="timeline-content bg-white rounded-4 shadow-sm border border-success border-opacity-25 overflow-hidden">
                    <!-- Collapsible Header -->
                    <div class="d-flex justify-content-between align-items-center p-3 cursor-pointer" 
                         :class="expandedVaccinations.includes(vac.id) ? 'bg-success bg-opacity-10 border-bottom border-success border-opacity-25' : ''"
                         @click="toggleVaccination(vac.id)" style="cursor: pointer;">
                      <div>
                        <span class="text-dark fw-bold fs-6 me-3"><i class="bi bi-calendar2-check text-success me-2"></i> {{ formatDate(vac.injectionDate) }}</span>
                        <span class="badge bg-light border text-dark shadow-sm px-3 py-1 rounded-pill"><i class="bi bi-person-badge text-muted me-1"></i> Bs. {{ getLastWord(vac.doctorName) }}</span>
                        <span v-if="!expandedVaccinations.includes(vac.id)" class="ms-3 small text-muted text-truncate d-inline-block" style="max-width: 250px; vertical-align: bottom;">
                          <i class="bi bi-shield-check text-success me-1"></i> {{ vac.vaccineName || 'Tiêm phòng' }}
                        </span>
                      </div>
                      <i class="bi text-muted fs-5" :class="expandedVaccinations.includes(vac.id) ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                    </div>

                    <!-- Expanded Content -->
                    <div v-if="expandedVaccinations.includes(vac.id)" class="p-4 pt-3">
                      <div class="row g-3 small">
                        <div class="col-12 border-bottom pb-2 mb-2 d-flex justify-content-between">
                          <div>
                            <span class="text-muted fw-bold">Nhiệt độ:</span> {{ vac.temperature || '—' }} °C
                          </div>
                          <div>
                            <span class="text-muted fw-bold">Cân nặng:</span> {{ vac.weight || '—' }} kg
                          </div>
                          <div>
                            <span class="text-muted fw-bold">Đánh giá:</span> 
                            <span :class="vac.clinicalAssessment === 'Đủ điều kiện' ? 'text-success fw-bold' : 'text-danger fw-bold'">{{ vac.clinicalAssessment || '—' }}</span>
                          </div>
                        </div>
                        
                        <div class="col-12 border-bottom pb-2">
                          <div class="d-flex align-items-start gap-2">
                            <i class="bi bi-shield-check text-success fs-5 mt-1"></i>
                            <div>
                              <div class="fw-bold text-dark fs-6">{{ vac.vaccineName || 'Không xác định' }}</div>
                              <div class="text-muted mt-1">
                                Lô: <span class="fw-bold">{{ vac.batchNumber || '—' }}</span> | 
                                Đường tiêm: {{ vac.route || '—' }} | 
                                Vị trí: {{ vac.injectionSite || '—' }}
                              </div>
                            </div>
                          </div>
                        </div>
                        
                        <div class="col-md-6 border-end">
                          <div class="text-muted mb-1 fw-bold text-warning"><i class="bi bi-alarm"></i> Lịch nhắc lại:</div>
                          <div class="fw-bold" :class="vac.nextDueDate ? 'text-danger' : 'text-dark'">
                            {{ vac.nextDueDate ? formatDate(vac.nextDueDate) : 'Không có' }}
                          </div>
                        </div>
                        
                        <div class="col-md-6">
                          <div class="text-muted mb-1 fw-bold text-info"><i class="bi bi-clipboard-pulse"></i> Tình trạng:</div>
                          <div class="text-dark">
                            {{ vac.isAllergic ? 'Có dị ứng' : 'Bình thường' }}
                            <span v-if="vac.hasPreviousReaction" class="text-danger fw-bold ms-1">(Từng sốc phản vệ)</span>
                          </div>
                        </div>
                        
                        <div v-if="vac.followUpInstructions || vac.doctorRemarks" class="col-12 mt-2">
                          <div class="p-3 bg-success bg-opacity-10 rounded-3 text-dark border-start border-3 border-success">
                            <div v-if="vac.followUpInstructions"><span class="fw-bold text-success"><i class="bi bi-chat-quote-fill"></i> Dặn dò:</span> {{ vac.followUpInstructions }}</div>
                            <div v-if="vac.doctorRemarks" :class="{'mt-2': vac.followUpInstructions}"><span class="fw-bold text-success"><i class="bi bi-journal-medical"></i> BS Ghi chú:</span> {{ vac.doctorRemarks }}</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Weight Chart Tab -->
            <div v-if="activeHistoryTab === 'weight-chart'" class="bg-white p-4 rounded-4 shadow-sm border">
              <h6 class="text-center fw-bold mb-4">Biểu Đồ Theo Dõi Cân Nặng (kg)</h6>
              <div v-if="chartData.labels.length > 0" style="height: 350px;">
                <Line :data="chartData" :options="chartOptions" />
              </div>
              <div v-else class="text-center py-5 text-muted">
                <i class="bi bi-graph-down text-secondary fs-1 d-block mb-3 opacity-25"></i>
                Chưa có đủ dữ liệu cân nặng từ các lần khám để vẽ biểu đồ.
              </div>
            </div>

          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import api from '../../../services/api';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import { Line } from 'vue-chartjs';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

const props = defineProps<{
  pet: any | null;
  isOpen: boolean;
}>();

const emit = defineEmits(['close']);

const activeHistoryTab = ref('consultation');
const loading = ref(false);
const consultationHistory = ref<any[]>([]);
const vaccinationHistory = ref<any[]>([]);

const expandedConsultations = ref<number[]>([]);
const toggleConsultation = (id: number) => {
  const index = expandedConsultations.value.indexOf(id);
  if (index > -1) expandedConsultations.value.splice(index, 1);
  else expandedConsultations.value.push(id);
};

const expandedVaccinations = ref<number[]>([]);
const toggleVaccination = (id: number) => {
  const index = expandedVaccinations.value.indexOf(id);
  if (index > -1) expandedVaccinations.value.splice(index, 1);
  else expandedVaccinations.value.push(id);
};

const closeModal = () => {
  emit('close');
};

const fetchPetHistory = async () => {
  if (!props.pet || !props.pet.id) return;
  loading.value = true;
  try {
    const [consultRes, vaccineRes] = await Promise.all([
      api.get(`/medical-records/pet/${props.pet.id}`),
      api.get(`/vaccinations/pet/${props.pet.id}`)
    ]);
    consultationHistory.value = consultRes.data || [];
    vaccinationHistory.value = vaccineRes.data || [];
  } catch (err: any) {
    console.error('Lỗi tải hồ sơ y tế toàn diện:', err);
    alert('Không thể tải đầy đủ hồ sơ y tế.');
  } finally {
    loading.value = false;
  }
};

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    activeHistoryTab.value = 'consultation';
    fetchPetHistory();
  }
});

// Chart.js Data and Options
const chartData = computed(() => {
  // Extract dates and weights from consultationHistory
  // Filter out records without weight
  const recordsWithWeight = consultationHistory.value
    .filter(r => r.weight && r.weight > 0)
    .sort((a, b) => new Date(a.visitDate).getTime() - new Date(b.visitDate).getTime());

  return {
    labels: recordsWithWeight.map(r => formatDate(r.visitDate)),
    datasets: [
      {
        label: 'Cân nặng (kg)',
        backgroundColor: '#0ea5e9',
        borderColor: '#0ea5e9',
        data: recordsWithWeight.map(r => r.weight),
        tension: 0.3,
        fill: false,
        pointBackgroundColor: '#ffffff',
        pointBorderColor: '#0ea5e9',
        pointBorderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6
      }
    ]
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      position: 'bottom' as const
    },
    tooltip: {
      callbacks: {
        label: function(context: any) {
          return `${context.parsed.y} kg`;
        }
      }
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      title: {
        display: true,
        text: 'Cân nặng (kg)'
      }
    },
    x: {
      title: {
        display: true,
        text: 'Ngày khám'
      }
    }
  }
};

// UI Helpers
const getAnimalEmoji = (species: string) => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const getLastWord = (name: string) => {
  if (!name) return '—';
  const parts = name.trim().split(' ');
  return parts[parts.length - 1];
};
</script>

<style scoped>
/* Timeline */
.medical-timeline {
  padding-left: 5px;
}
.timeline-item {
  position: relative;
}
.timeline-line {
  position: absolute;
  top: 15px;
  left: 9px;
  bottom: 0;
  width: 2px;
  background-color: #e2e8f0;
}
.timeline-item:last-child .timeline-line {
  display: none;
}
.timeline-circle {
  position: absolute;
  top: 12px;
  left: 4px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
  z-index: 1;
}

/* Modals overlays */
.zalo-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.zalo-modal-card {
  background: white;
  width: 100%;
  border-radius: var(--radius-md, 1rem);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.zalo-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.zalo-modal-body {
  padding: 1.5rem;
  max-height: 80vh;
  overflow-y: auto;
}
</style>
