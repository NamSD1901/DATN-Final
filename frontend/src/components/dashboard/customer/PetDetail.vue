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
                      <div class="d-flex justify-content-between align-items-center mb-3">
                        <h6 class="fw-bold mb-0 text-info">Chi tiết bệnh án</h6>
                        <button class="btn btn-sm btn-outline-info rounded-pill px-3 shadow-sm" @click="printRecord(record)">
                          <i class="bi bi-printer me-1"></i> In hồ sơ
                        </button>
                      </div>
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
                          <div class="text-muted mb-2 fw-bold"><i class="bi bi-activity text-info me-1"></i>Khám lâm sàng:</div>
                          <div class="d-flex flex-wrap gap-2">
                            <span v-for="(sign, sIdx) in (record.clinicalSigns || '').split(',').map(s => s.trim()).filter(s => s)" :key="sIdx" 
                                  class="badge bg-light text-dark border p-2 shadow-sm rounded-3 d-inline-block" 
                                  style="font-size: 0.8rem; text-align: left; max-width: 100%; white-space: normal;">
                              <span v-if="sign.includes(':')" class="fw-bold text-secondary">{{ sign.split(':')[0] }}:</span>
                              <span v-if="sign.includes(':')">{{ sign.substring(sign.indexOf(':') + 1) }}</span>
                              <span v-else>{{ sign }}</span>
                            </span>
                          </div>
                        </div>

                        <!-- Cận lâm sàng / Hình ảnh -->
                        <div v-if="record.attachments && record.attachments.length > 0" class="col-12 border-bottom pb-3">
                          <div class="text-muted mb-2 fw-bold"><i class="bi bi-images text-primary me-1"></i>Cận lâm sàng (Hình ảnh):</div>
                          <div class="d-flex gap-2 flex-wrap">
                            <img v-for="(img, idx) in record.attachments" :key="idx" :src="img.startsWith('http') ? img : baseUrl + img" class="rounded shadow-sm" style="width: 100px; height: 100px; object-fit: cover; border: 1px solid #cbd5e1;" />
                          </div>
                        </div>
                        
                        <div class="col-md-6 border-end">
                          <div class="text-muted mb-1 fw-bold text-danger"><i class="bi bi-exclamation-square text-danger me-1"></i>Chẩn đoán:</div>
                          <div class="fw-bold text-dark">{{ record.diagnosis }}</div>
                        </div>
                        <div class="col-md-6">
                          <div class="text-muted mb-1 fw-bold text-primary"><i class="bi bi-heart-pulse text-primary me-1"></i>Phương pháp điều trị:</div>
                          <div class="text-dark">{{ record.treatmentPlan }}</div>
                        </div>
                        <div v-if="record.doctorNotes" class="col-12 mt-2">
                          <div class="p-3 bg-light rounded-3 fst-italic text-muted border-start border-3 border-info">"{{ record.doctorNotes }}"</div>
                        </div>
                      </div>
                      
                      <div v-if="(record.prescribedMedicines || record.prescriptions) && (record.prescribedMedicines || record.prescriptions).length > 0" class="mt-3 bg-light p-3 rounded-4 border shadow-sm">
                        <span class="fw-bold text-success d-block small mb-2"><i class="bi bi-capsule-pill me-1"></i>Thuốc đã kê đơn:</span>
                        <ul class="list-unstyled mb-0 ps-2">
                          <li v-for="(med, mIdx) in (record.prescribedMedicines || record.prescriptions)" :key="mIdx" class="text-dark small mb-3 d-flex align-items-start pb-2 border-bottom border-secondary border-opacity-10">
                            <i class="bi bi-check-circle-fill text-success me-2 mt-1" style="font-size: 0.7rem;"></i> 
                            <div>
                                <span class="fw-bold fs-6 text-dark">{{ med.medicineName }}</span> 
                                <span class="badge bg-secondary ms-2 bg-opacity-10 text-secondary border">SL: {{ med.quantity }}</span>
                                <div class="text-muted mt-1" style="font-size: 0.78rem;">
                                    <span v-if="med.dosage"><i class="bi bi-droplet-half me-1"></i>Liều: <strong class="text-dark">{{ med.dosage }}</strong></span>
                                    <span v-if="med.durationDays" class="ms-2"><i class="bi bi-calendar-range me-1"></i>Liệu trình: <strong class="text-dark">{{ med.durationDays }} ngày</strong></span>
                                </div>
                                <div v-if="med.instruction" class="mt-1 fst-italic text-secondary" style="font-size: 0.75rem;">
                                    * HDSD: {{ med.instruction }}
                                </div>
                            </div>
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

const baseUrl = api.defaults.baseURL?.replace('/api', '') || 'http://localhost:5031';
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

const printRecord = async (record: any) => {
  if (!record.appointmentId) {
    alert('Hồ sơ bệnh án cũ không hỗ trợ in theo mẫu mới.');
    return;
  }
  
  try {
    let soap: any = null;
    try {
      const soapRes = await api.get(`/medical-records/soap/appointment/${record.appointmentId}`);
      soap = soapRes.data;
    } catch (e) {
      console.warn('No SOAP record found, falling back to basic record data');
      // Construct fallback SOAP object from basic record
      soap = {
        subjective: { 
          chiefComplaint: record.clinicalSigns || 'Không ghi nhận',
          petOwnerNotes: record.doctorNotes
        },
        objective: {
          weight: record.weight,
          temperature: record.temperature,
          bodyConditionScore: 5,
          mentation: 'Bình thường',
          hydration: 'Bình thường'
        },
        assessment: {
          definitiveDiagnosis: record.diagnosis
        },
        plan: {
          careInstructions: record.treatmentPlan,
          prescriptions: record.prescribedMedicines || record.prescriptions || []
        }
      };
    }
    
    let appt: any = null;
    // Fetch appointment to get more details if needed
    try {
      const apptRes = await api.get(`/appointment/${record.appointmentId}`);
      appt = apptRes.data;
    } catch (e) {
      console.warn('No appointment found');
    }
    
    const win = window.open('', '_blank');
    if (!win) return;
    
    const now = new Date(record.visitDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    const baseUrl = api.defaults.baseURL?.replace('/api', '') || '';
    
    // Services HTML (mock based on what we have, or empty if not needed since it's history)
    const servicesHtml = ''; 
    const medicinesHtml = (soap.plan?.prescriptions || []).map((m: any, idx: number) => {
      let detailsHtml = '';
      if (m) {
        detailsHtml = `
          <div style="font-size: 0.78rem; color: #475569; margin-top: 6px; border-top: 1px dashed #e2e8f0; padding-top: 6px;">
            ${m.dosage ? `<div style="margin-bottom: 2px;">Liều dùng: <strong style="color: #0f172a;">${m.dosage}</strong></div>` : ''}
            ${m.durationDays ? `<div style="margin-bottom: 2px;">Liệu trình: <strong style="color: #0f172a;">${m.durationDays} ngày</strong></div>` : ''}
            ${m.instruction ? `<div><em>* HDSD: ${m.instruction}</em></div>` : ''}
          </div>
        `;
      }
      return `
      <div class="rx-item">
        <div class="rx-name">${idx + 1}. ${m.medicineName || 'Thuốc'}</div>
        <div class="rx-qty">SL: <strong>${m.quantity || 1}</strong></div>
        ${detailsHtml}
      </div>`;
    }).join('');

    let imagesHtml = '';
    if (soap.objective?.attachments && soap.objective.attachments.length > 0) {
      const imgs = soap.objective.attachments.map((url: string) => `
        <img src="${baseUrl}${url}" style="width: 140px; height: 140px; object-fit: cover; border-radius: 8px; border: 1px solid #cbd5e1; box-shadow: 0 2px 4px rgba(0,0,0,0.05);" />
      `).join('');
      imagesHtml = `
      <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
        <div class="clinical-label" style="margin-bottom: 8px;"><i class="bi bi-images"></i> Hình ảnh Cận lâm sàng:</div>
        <div style="display: flex; gap: 10px; flex-wrap: wrap;">${imgs}</div>
      </div>
      `;
    }

    let sysExamsHtml = '';
    const exams = [
      { key: 'eyes', label: 'Mắt' }, { key: 'ears', label: 'Tai' }, { key: 'nose', label: 'Mũi' },
      { key: 'mouth', label: 'Miệng/Răng' }, { key: 'skinCoat', label: 'Da & Lông' },
      { key: 'gastrointestinal', label: 'Tiêu hóa' }, { key: 'respiratory', label: 'Hô hấp' }
    ];
    const abnormalExams = exams.filter(e => soap.objective && soap.objective[e.key] && !soap.objective[e.key].isNormal);
    if (abnormalExams.length > 0) {
      sysExamsHtml = `
        <div style="margin-top: 8px; font-size: 0.85rem;">
          <div class="clinical-label" style="font-size: 0.75rem; margin-bottom: 4px;">Phát hiện bất thường:</div>
          <ul style="margin: 0; padding-left: 20px; color: #b91c1c;">
            ${abnormalExams.map(e => `<li><strong>${e.label}:</strong> ${soap.objective[e.key].note || 'Có bất thường'}</li>`).join('')}
          </ul>
        </div>
      `;
    }

    const html = `
      <!DOCTYPE html>
      <html>
      <head>
        <title>Hồ Sơ Bệnh Án - ${props.pet?.name}</title>
        <style>
          @import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');
          
          /* Web Preview Styles */
          body { font-family: 'Plus Jakarta Sans', sans-serif; background: #f8fafc; margin: 0; padding: 20px 40px; color: #1e293b; line-height: 1.5; }
          .medical-record-page { max-width: 800px; margin: 0 auto; background: #fff; padding: 40px; box-shadow: 0 10px 25px -5px rgba(0,0,0,0.1); border-radius: 16px; }
          
          .header { display: flex; justify-content: space-between; align-items: flex-end; border-bottom: 3px solid #0ea5e9; padding-bottom: 15px; margin-bottom: 25px; }
          .logo-block { display: flex; align-items: center; gap: 12px; }
          .logo-circle { width: 50px; height: 50px; background: #0ea5e9; color: white; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 26px; }
          .clinic-name { font-size: 24px; font-weight: 800; color: #0ea5e9; letter-spacing: -0.5px; line-height: 1.2; text-transform: uppercase; }
          .clinic-sub { font-size: 13px; color: #64748b; font-weight: 500; }
          .inv-title { font-size: 20px; font-weight: 800; color: #0284c7; text-align: right; letter-spacing: 0.5px; text-transform: uppercase; }
          .inv-meta { font-size: 13px; color: #64748b; text-align: right; margin-top: 2px; }
          
          .premium-box { background: white; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; margin-bottom: 20px; }
          .patient-info-box { display: flex; gap: 20px; background: #f8fafc; border-color: #cbd5e1; }
          .info-label { font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.5px; font-weight: 700; color: #64748b; margin-bottom: 4px; }
          .info-value { font-weight: 800; color: #0f172a; }
          .info-value.text-xl { font-size: 1.35rem; }
          .info-value.text-lg { font-size: 1.15rem; }
          .species-badge { font-size: 0.75rem; padding: 3px 8px; background: #e2e8f0; border-radius: 6px; margin-left: 8px; vertical-align: middle; color: #475569; font-weight: 600; }
          
          .clinical-box { border-left: 5px solid #0ea5e9; }
          .section-title { font-size: 1.05rem; font-weight: 800; color: #0284c7; margin-bottom: 15px; display: flex; align-items: center; gap: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
          .clinical-grid { display: flex; flex-direction: column; gap: 12px; }
          .clinical-item { background: #f8fafc; padding: 12px 15px; border-radius: 8px; border: 1px solid #f1f5f9; }
          .clinical-label { font-size: 0.82rem; font-weight: 700; color: #334155; margin-bottom: 2px; text-transform: uppercase; letter-spacing: 0.5px; }
          .clinical-text { font-size: 0.95rem; color: #0f172a; }
          
          .rx-box { border-left: 5px solid #8b5cf6; }
          .rx-list { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
          .rx-item { border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; background: #f8fafc; }
          .rx-name { font-weight: 700; color: #4c1d95; font-size: 0.95rem; margin-bottom: 6px; }
          .rx-qty { font-size: 0.8rem; color: #64748b; }
          
          .footer { margin-top: 40px; }
          .sign-area { display: flex; justify-content: space-between; margin-top: 50px; }
          .sign-line { border-bottom: 1px dashed #cbd5e1; width: 100%; height: 60px; margin-bottom: 10px; }
          
          /* Print Styles */
          @media print {
            @page { size: A4; margin: 12mm 15mm; }
            body { background: #fff; padding: 0; margin: 0; font-size: 12pt; color: #000; }
            .medical-record-page { max-width: 100%; padding: 0; box-shadow: none; border-radius: 0; }
            
            /* Allow breaking inside the page to prevent huge gaps */
            .premium-box { 
              page-break-inside: auto; 
              break-inside: auto; 
              border: none !important; 
              padding: 0 !important; 
              margin-bottom: 25px !important; 
              background: transparent !important;
            }
            .patient-info-box { border-bottom: 2px dashed #cbd5e1 !important; padding-bottom: 15px !important; margin-bottom: 25px !important; }
            
            /* Remove thick left borders in print, use underlines for sections instead */
            .section-title { border-bottom: 2px solid #0ea5e9; padding-bottom: 6px; margin-bottom: 15px; color: #000 !important; }
            .rx-box .section-title { border-bottom-color: #8b5cf6; }
            
            /* Ensure items don't break inside themselves */
            .clinical-item, .rx-item, .sign-area {
              page-break-inside: avoid;
              break-inside: avoid;
              border: 1px solid #cbd5e1 !important;
              background: #fff !important;
              margin-bottom: 12px;
            }
            
            .header { border-bottom: 2px solid #000; margin-bottom: 25px; padding-bottom: 15px; }
            .clinic-name { color: #000 !important; }
            .inv-title { color: #000 !important; }
            
            /* Ensure images print well */
            img { max-width: 100%; page-break-inside: avoid; break-inside: avoid; }
            
            /* Force background colors to print if needed */
            * { -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }
          }
        </style>
      </head>
      <body>
        <div class="medical-record-page">
          <div class="header">
            <div class="logo-block">
              <div class="logo-circle">🐾</div>
              <div>
                <div class="clinic-name">MYPET CLINIC</div>
                <div class="clinic-sub">Hệ thống phòng khám thú y cao cấp</div>
              </div>
            </div>
            <div>
              <div class="inv-title">HỒ SƠ BỆNH ÁN</div>
              <div class="inv-meta">Ngày khám: ${now}</div>
            </div>
          </div>

          <div class="premium-box patient-info-box">
            <div style="flex:1;">
              <div class="info-label">THÔNG TIN BỆNH NHÂN</div>
              <div class="info-value text-xl">${props.pet?.name || '—'} <span class="species-badge">${props.pet?.species || 'Khác'}</span></div>
              <div style="display:flex; flex-wrap: wrap; gap: 15px; font-size: 0.85rem; color: #475569; margin-top: 6px;">
                <div style="white-space: nowrap;">Giống: <strong>${props.pet?.breed || '—'}</strong></div>
                <div style="white-space: nowrap;">Cân nặng: <strong>${soap?.objective?.weight || appt?.weight ? (soap?.objective?.weight || appt?.weight) + ' kg' : '—'}</strong></div>
                ${soap?.objective?.temperature ? `<div style="white-space: nowrap;">Nhiệt độ: <strong>${soap.objective.temperature}°C</strong></div>` : ''}
                ${soap?.objective?.heartRate ? `<div style="white-space: nowrap;">Nhịp tim: <strong>${soap.objective.heartRate} bpm</strong></div>` : ''}
              </div>
              <div class="info-meta" style="margin-top: 8px; font-size: 0.85rem; color: #64748b;">Mã Bệnh Án: <strong>#${record.appointmentId}</strong></div>
            </div>
            <div style="text-align:right; border-left: 1px dashed #bfdbfe; padding-left: 20px;">
              <div class="info-label">BÁC SĨ ĐIỀU TRỊ</div>
              <div class="info-value text-lg" style="color:#2563eb; margin-bottom: 8px;">${record.doctorName || '—'}</div>
              ${appt?.serviceName ? `<div style="font-size: 0.75rem; background: white; padding: 4px 8px; border-radius: 6px; display: inline-block; color: #0f172a; border: 1px solid #e2e8f0;">Dịch vụ: <strong>${appt.serviceName}</strong></div>` : ''}
            </div>
          </div>

          <div class="premium-box clinical-box">
            <div class="section-title"><i class="bi bi-clipboard-pulse"></i> 1. KẾT QUẢ KHÁM LÂM SÀNG (SOAP)</div>
            
            <div class="clinical-grid">
              <div class="clinical-item">
                <div class="clinical-label">S - Chủ quan (Triệu chứng & Lý do khám):</div>
                <div class="clinical-text">${soap.subjective?.chiefComplaint || appt?.symptom || 'Không ghi nhận'}</div>
                <div style="font-size: 0.8rem; color: #475569; margin-top: 4px; display: flex; gap: 16px; flex-wrap: wrap;">
                  ${soap.subjective?.appetite && soap.subjective.appetite !== 'Bình thường' ? `<div><span>Ăn uống:</span> <strong>${soap.subjective.appetite}</strong></div>` : ''}
                  ${soap.subjective?.hasVomiting ? `<div style="color: #b91c1c;"><span>Nôn mửa:</span> <strong>Có</strong> (${soap.subjective.vomitingDetails || ''})</div>` : ''}
                  ${soap.subjective?.hasDiarrhea ? `<div style="color: #b91c1c;"><span>Tiêu chảy:</span> <strong>Có</strong> (${soap.subjective.diarrheaDetails || ''})</div>` : ''}
                  ${soap.subjective?.activityLevel && soap.subjective.activityLevel !== 'Bình thường' ? `<div><span>Vận động:</span> <strong>${soap.subjective.activityLevel}</strong></div>` : ''}
                </div>
                ${soap.subjective?.petOwnerNotes ? `<div class="clinical-text" style="font-size: 0.8rem; color:#64748b; margin-top: 6px; font-style:italic;">* Ghi chú từ chủ: ${soap.subjective.petOwnerNotes}</div>` : ''}
              </div>
              <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
                <div class="clinical-label">O - Khách quan (Khám thực thể):</div>
                <div class="clinical-text" style="font-size: 0.85rem;">
                  Thể trạng (BCS): <strong>${soap.objective?.bodyConditionScore || 5}/9</strong> | Tri giác: <strong>${soap.objective?.mentation || 'Bình thường'}</strong> | Mức mất nước: <strong>${soap.objective?.hydration || 'Bình thường'}</strong>
                </div>
                ${sysExamsHtml}
              </div>
              
              ${imagesHtml}

              <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
                <div class="clinical-label">A - Chẩn đoán:</div>
                <div class="clinical-text" style="font-weight:600; color:#b91c1c; font-size: 1rem;">${soap.assessment?.definitiveDiagnosis || soap.assessment?.tentativeDiagnosis || record.diagnosis || 'Chưa có chẩn đoán cuối cùng'}</div>
                ${soap.assessment?.differentialDiagnosis ? `<div style="font-size: 0.8rem; color: #475569; margin-top: 2px;">Chẩn đoán phân biệt: <em>${soap.assessment.differentialDiagnosis}</em></div>` : ''}
                <div class="clinical-text" style="font-size: 0.8rem; color:#64748b; margin-top: 6px;">Tiên lượng: <strong style="color: #0f172a;">${soap.assessment?.prognosis || 'Tốt'}</strong> &nbsp;|&nbsp; Mức độ bệnh: <strong style="color: #0f172a;">${soap.assessment?.diseaseSeverity || 'Nhẹ'}</strong></div>
              </div>
            </div>
          </div>

          <div class="premium-box rx-box">
            <div class="section-title"><i class="bi bi-capsule"></i> 2. KẾT QUẢ ĐIỀU TRỊ & KÊ ĐƠN THUỐC (PLAN)</div>
            
            <div style="margin-bottom: 12px;">
              <div class="clinical-label" style="margin-bottom: 6px;">Đơn thuốc:</div>
              ${medicinesHtml ? `<div class="rx-list">${medicinesHtml}</div>` : '<div style="color:#64748b; font-style:italic;">Không có chỉ định thuốc mang về.</div>'}
            </div>

            <div style="border-top: 1px dashed #e2e8f0; padding-top: 12px; margin-top: 12px;">
              <div class="clinical-label" style="margin-bottom: 4px;">Dặn dò chăm sóc:</div>
              <div style="color:#334155; line-height: 1.6; font-size: 0.85rem;">
                ${soap.plan?.careInstructions ? soap.plan.careInstructions.replace(/\\n/g, '<br>') : `
                - Vui lòng cho thú cưng uống thuốc đúng liều lượng (nếu có).<br>
                - Tái khám ngay nếu thú cưng có biểu hiện bất thường (nôn mửa, bỏ ăn).<br>
                - Đảm bảo môi trường sống sạch sẽ, thoáng mát.`}
              </div>
              
              ${soap.plan?.followUpDate ? `
              <div style="margin-top: 12px;">
                <span style="background-color: #fef3c7; padding: 4px 10px; border-radius: 6px; border: 1px solid #f59e0b; color: #b45309; font-size: 0.85rem;">
                  <i class="bi bi-calendar-event me-1"></i> Lịch tái khám: <strong>${new Date(soap.plan.followUpDate).toLocaleDateString('vi-VN')}</strong> 
                  ${soap.plan.followUpNote ? `(${soap.plan.followUpNote})` : ''}
                </span>
              </div>` : ''}
            </div>
          </div>

          <div class="footer sign-area">
            <div style="flex:1;"></div>
            <div style="width:200px;text-align:center;">
              <div style="font-weight:700;margin-bottom:4px; font-size: 0.9rem;">Chữ ký Bác sĩ</div>
              <div class="sign-line"></div>
              <div style="font-size:0.9rem;color:#0f172a;font-weight:700;">${record.doctorName || ''}</div>
            </div>
          </div>
          <div style="text-align:center; font-size:0.75rem; color:#94a3b8; margin-top:30px; border-top:1px solid #f1f5f9; padding-top:10px;">
            * Phiếu khám bệnh điện tử — Hệ thống MyPetClinic *
          </div>
        </div>
      </body>
      </html>
    `;
    
    win.document.write(html);
    win.document.close();
    win.focus();
    setTimeout(() => {
      win.print();
      win.close();
    }, 250);
  } catch (e) {
    console.error('Lỗi khi tạo bản in:', e);
    alert('Đã xảy ra lỗi khi tạo bản in hồ sơ bệnh án.');
  }
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
