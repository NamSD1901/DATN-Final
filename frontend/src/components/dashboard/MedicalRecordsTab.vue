<template>
  <div class="medical-records-tab">
    <div class="row g-4">
      <!-- Left Column: Form khám bệnh & Kê đơn -->
      <div class="col-lg-8">
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-glass bg-white mb-4">
          <div class="d-flex justify-content-between align-items-center mb-4 border-bottom pb-3">
            <div>
              <h5 class="fw-bold text-dark mb-1">
                <i class="bi bi-clipboard2-pulse-fill text-warning me-2"></i>
                Hồ Sơ Bệnh Án Chuẩn S.O.A.P
              </h5>
              <p class="text-muted mb-0 small">Bác sĩ hãy nhập chẩn đoán chi tiết và hướng điều trị theo từng bước.</p>
            </div>
            <div class="text-end">
              <span v-if="activePatient.petName" class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold text-uppercase shadow-sm mb-2 d-block">
                {{ activePatient.petName }} (Chủ: {{ activePatient.customerName }})
              </span>
              <div class="btn-group btn-group-sm" role="group">
                <input type="radio" class="btn-check" name="recordType" id="typeConsultation" value="Consultation" v-model="form.recordType">
                <label class="btn btn-outline-warning text-dark fw-bold" for="typeConsultation">Khám Bệnh</label>
                <input type="radio" class="btn-check" name="recordType" id="typeVaccination" value="Vaccination" v-model="form.recordType">
                <label class="btn btn-outline-warning text-dark fw-bold" for="typeVaccination">Tiêm Phòng</label>
              </div>
            </div>
          </div>

          <!-- Active Case Alert / Empty Warning -->
          <div v-if="!activePatient.appointmentId" class="alert alert-info rounded-4 border-0 p-4 mb-0 text-center">
            <i class="bi bi-exclamation-triangle-fill text-info fs-1 d-block mb-2"></i>
            <h6 class="fw-bold text-dark mb-2">Chưa chọn ca khám hoạt động</h6>
            <p class="text-muted small mb-3">Vui lòng quay lại tab "Hàng khám của tôi" để chọn bệnh nhi bắt đầu khám.</p>
            <button class="btn btn-warning text-dark fw-bold rounded-pill px-4" @click="$emit('switch-tab', 'doctor-cases')">
              <i class="bi bi-arrow-left me-1"></i> Xem hàng khám
            </button>
          </div>

          <!-- Main Diagnostic Form -->
          <form v-else @submit.prevent="submitForm">
            <div class="accordion mb-4 custom-soap-accordion" id="soapAccordion">
              
              <!-- S: Subjective -->
              <div class="accordion-item border-0 shadow-sm rounded-4 mb-3 overflow-hidden bg-white">
                <h2 class="accordion-header" id="headingS">
                  <button class="accordion-button fw-bold text-dark bg-light rounded-top-4" :class="{ 'collapsed': activeAccordion !== 'S' }" type="button" @click="toggleAccordion('S')">
                    <span class="soap-badge s-badge me-2">S</span> Subjective (Tiền sử & Lời khai)
                  </button>
                </h2>
                <div id="collapseS" class="accordion-collapse collapse" :class="{ 'show': activeAccordion === 'S' }">
                  <div class="accordion-body border-top">
                    <div class="mb-3">
                      <label class="form-label small fw-bold text-secondary">Bệnh sử / Lời khai chủ nuôi <span class="text-danger">*</span></label>
                      <textarea v-model="form.medicalHistory" class="form-control rounded-3" rows="3" placeholder="Ví dụ: Bé bỏ ăn 2 ngày nay, nôn mửa buổi sáng..." required></textarea>
                      <div class="form-text small text-muted">Ghi nhận thông tin chủ quan từ chủ nuôi trước khi tiến hành khám.</div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- O: Objective -->
              <div class="accordion-item border-0 shadow-sm rounded-4 mb-3 overflow-hidden bg-white">
                <h2 class="accordion-header" id="headingO">
                  <button class="accordion-button fw-bold text-dark bg-light" :class="{ 'collapsed': activeAccordion !== 'O' }" type="button" @click="toggleAccordion('O')">
                    <span class="soap-badge o-badge me-2">O</span> Objective (Khám lâm sàng)
                  </button>
                </h2>
                <div id="collapseO" class="accordion-collapse collapse" :class="{ 'show': activeAccordion === 'O' }">
                  <div class="accordion-body border-top">
                    <!-- Vital Signs Grid -->
                    <div class="row g-3 mb-3">
                      <div class="col-md-4">
                        <label class="form-label small fw-bold text-secondary">Cân nặng (kg) <span class="text-danger">*</span></label>
                        <div class="input-group">
                          <input type="number" step="0.1" v-model="form.weight" class="form-control rounded-start-3" placeholder="Ví dụ: 5.2" required>
                          <span class="input-group-text bg-light text-muted rounded-end-3">kg</span>
                        </div>
                      </div>
                      <div class="col-md-4">
                        <label class="form-label small fw-bold text-secondary">Nhiệt độ (°C) <span class="text-danger">*</span></label>
                        <div class="input-group">
                          <input type="number" step="0.1" v-model="form.temperature" class="form-control rounded-start-3" placeholder="Ví dụ: 38.5" required>
                          <span class="input-group-text bg-light text-muted rounded-end-3">°C</span>
                        </div>
                      </div>
                    </div>
                    <div class="mb-3">
                      <label class="form-label small fw-bold text-secondary">Dấu hiệu lâm sàng (Triệu chứng) <span class="text-danger">*</span></label>
                      <textarea v-model="form.clinicalSigns" class="form-control rounded-3" rows="2" placeholder="Ví dụ: Nhịp tim 120bpm, lông xơ xác, niêm mạc nhợt nhạt..." required></textarea>
                    </div>
                  </div>
                </div>
              </div>

              <!-- A: Assessment -->
              <div class="accordion-item border-0 shadow-sm rounded-4 mb-3 overflow-hidden bg-white">
                <h2 class="accordion-header" id="headingA">
                  <button class="accordion-button fw-bold text-dark bg-light" :class="{ 'collapsed': activeAccordion !== 'A' }" type="button" @click="toggleAccordion('A')">
                    <span class="soap-badge a-badge me-2">A</span> Assessment (Chẩn đoán)
                  </button>
                </h2>
                <div id="collapseA" class="accordion-collapse collapse" :class="{ 'show': activeAccordion === 'A' }">
                  <div class="accordion-body border-top">
                    <div class="mb-3">
                      <label class="form-label small fw-bold text-secondary">Chẩn đoán bệnh <span class="text-danger">*</span></label>
                      <input type="text" v-model="form.diagnosis" class="form-control rounded-3" placeholder="Ví dụ: Viêm phế quản cấp tính / Khỏe mạnh (nếu tiêm phòng)" required>
                    </div>
                  </div>
                </div>
              </div>

              <!-- P: Plan -->
              <div class="accordion-item border-0 shadow-sm rounded-4 mb-3 overflow-hidden bg-white">
                <h2 class="accordion-header" id="headingP">
                  <button class="accordion-button fw-bold text-dark bg-light" :class="{ 'collapsed': activeAccordion !== 'P' }" type="button" @click="toggleAccordion('P')">
                    <span class="soap-badge p-badge me-2">P</span> Plan (Kế hoạch điều trị)
                  </button>
                </h2>
                <div id="collapseP" class="accordion-collapse collapse" :class="{ 'show': activeAccordion === 'P' }">
                  <div class="accordion-body border-top">
                    <div class="mb-3">
                      <label class="form-label small fw-bold text-secondary">Phương pháp điều trị / Hướng xử lý <span class="text-danger">*</span></label>
                      <textarea v-model="form.treatmentPlan" class="form-control rounded-3" rows="3" placeholder="Ví dụ: Kê đơn kháng sinh, tiêm vắc xin dại, dặn dò kiêng nước..." required></textarea>
                    </div>
                    <div class="row g-3 mb-3">
                      <div class="col-md-6">
                        <label class="form-label small fw-bold text-secondary">Ngày hẹn tái khám (nếu có)</label>
                        <input type="date" v-model="form.followUpDate" class="form-control rounded-3">
                      </div>
                      <div class="col-md-6">
                        <label class="form-label small fw-bold text-secondary">Ghi chú bác sĩ (Nội bộ)</label>
                        <input type="text" v-model="form.doctorNotes" class="form-control rounded-3" placeholder="Ví dụ: Cần theo dõi thêm phản ứng sau tiêm...">
                      </div>
                    </div>
                  </div>
                </div>
              </div>

            </div>

            <!-- Prescription Area -->
            <div class="border-top pt-4 mb-4">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-capsule text-success me-2"></i>Kê đơn thuốc điều trị</h6>
                <button type="button" class="btn btn-outline-success btn-sm rounded-pill px-3" @click="addPrescriptionLine">
                  <i class="bi bi-plus-circle me-1"></i> Thêm thuốc
                </button>
              </div>

              <!-- Prescription Lines -->
              <div v-if="form.prescriptions.length === 0" class="text-center py-4 bg-light rounded-4 text-muted small">
                <i class="bi bi-prescription fs-3 d-block mb-1 text-black-50 opacity-50"></i>
                Chưa kê đơn thuốc cho ca khám này.
              </div>

              <div v-else class="prescription-list">
                <div v-for="(pres, idx) in form.prescriptions" :key="idx" class="prescription-line-card bg-light p-3 rounded-4 mb-3 border-0 shadow-sm position-relative">
                  <button type="button" class="btn-remove-line text-danger border-0 bg-transparent" @click="removePrescriptionLine(idx)">
                    <i class="bi bi-x-circle-fill fs-5"></i>
                  </button>

                  <div class="row g-2 align-items-end">
                    <!-- Medicine Select -->
                    <div class="col-md-4">
                      <label class="form-label small fw-bold text-muted" style="font-size:0.75rem;">Tên thuốc / Vật tư</label>
                      <select v-model="pres.medicineId" class="form-select rounded-3 small" @change="onMedicineChange(idx, pres.medicineId)">
                        <option :value="null" disabled>— Chọn thuốc —</option>
                        <option v-for="med in medicineOptions" :key="med.id" :value="med.id">
                          {{ med.name }} ({{ med.unit }}) — Giá: {{ formatPrice(med.sellPrice) }}
                        </option>
                      </select>
                    </div>

                    <!-- Quantity -->
                    <div class="col-md-2">
                      <label class="form-label small fw-bold text-muted" style="font-size:0.75rem;">Số lượng</label>
                      <input type="number" min="1" v-model.number="pres.quantity" class="form-control rounded-3 small" placeholder="SL">
                    </div>

                    <!-- Dosage -->
                    <div class="col-md-3">
                      <label class="form-label small fw-bold text-muted" style="font-size:0.75rem;">Liều lượng</label>
                      <input type="text" v-model="pres.dosage" class="form-control rounded-3 small" placeholder="Ví dụ: 1 viên">
                    </div>

                    <!-- Frequency -->
                    <div class="col-md-3">
                      <label class="form-label small fw-bold text-muted" style="font-size:0.75rem;">Tần suất / Thời lượng</label>
                      <input type="text" v-model="pres.frequency" class="form-control rounded-3 small" placeholder="Ví dụ: 2 lần/ngày, sau ăn">
                    </div>
                  </div>

                  <!-- Real-time Stock Warnings -->
                  <div v-if="pres.medicineId" class="mt-2 text-start">
                    <span v-if="pres.stockQuantity === 0" class="badge bg-danger text-white rounded-3 small">
                      <i class="bi bi-x-circle me-1"></i> HẾT HÀNG TRONG KHO (Không thể kê đơn)
                    </span>
                    <span v-else-if="pres.quantity > pres.stockQuantity" class="badge bg-danger text-white rounded-3 small">
                      <i class="bi bi-exclamation-triangle me-1"></i> Vượt quá tồn kho (Kho chỉ còn: {{ pres.stockQuantity }} {{ pres.unit }})
                    </span>
                    <span v-else-if="pres.stockQuantity <= 5" class="badge bg-warning text-dark rounded-3 small">
                      <i class="bi bi-exclamation-triangle-fill me-1"></i> Tồn kho sắp hết (Hiện tại còn: {{ pres.stockQuantity }} {{ pres.unit }})
                    </span>
                    <span v-else class="text-success small fw-semibold ms-1" style="font-size: 0.78rem;">
                      <i class="bi bi-check-circle me-1"></i> Sẵn sàng (Còn tồn: {{ pres.stockQuantity }} {{ pres.unit }})
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Error Banner -->
            <div v-if="errorMessage" class="alert alert-danger rounded-3 p-3 small">
              <i class="bi bi-x-circle-fill me-2"></i> {{ errorMessage }}
            </div>

            <!-- Actions -->
            <div class="d-flex justify-content-between align-items-center">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4" @click="cancelTreatment">
                <i class="bi bi-arrow-left me-1"></i> Trở về hàng khám
              </button>
              <button type="submit" class="btn btn-premium rounded-pill px-5 fw-bold text-white shadow-lg btn-save" :disabled="submitting || hasStockDeficit">
                <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
                <i class="bi bi-check2-circle me-1"></i> Lưu bệnh án & Hoàn thành
              </button>
            </div>
          </form>
        </div>
      </div>

      <!-- Right Column: Bệnh sử & Thông tin thú cưng -->
      <div class="col-lg-4">
        <!-- Active Pet Info Card -->
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-white mb-4 card-gradient-pet text-dark">
          <h6 class="fw-bold mb-3 border-bottom pb-2 text-dark"><i class="bi bi-info-circle-fill text-warning me-2"></i>Thông tin thú cưng</h6>
          <div v-if="activePatient.petId" class="pet-info-grid">
            <div class="d-flex align-items-center gap-3 mb-3">
              <span class="pet-avatar-large">🐾</span>
              <div>
                <h5 class="fw-bold text-dark mb-0">{{ activePatient.petName }}</h5>
                <span class="badge bg-light text-secondary border small">ID: {{ activePatient.petId }}</span>
              </div>
            </div>
            <div class="row g-2 small">
              <div class="col-6 text-muted">Chủ nuôi:</div>
              <div class="col-6 fw-bold">{{ activePatient.customerName }}</div>
              <div class="col-6 text-muted">ID Cuộc hẹn:</div>
              <div class="col-6 fw-semibold">#{{ activePatient.appointmentId }}</div>
            </div>
          </div>
          <div v-else class="text-center py-4 text-muted small">
            Chưa có thông tin thú cưng được tải.
          </div>
        </div>

        <!-- Medical Records Timeline -->
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
          <h6 class="fw-bold mb-3 border-bottom pb-2 text-dark"><i class="bi bi-clock-history text-warning me-2"></i>Lịch sử khám (Timeline)</h6>
          
          <div v-if="loadingHistory" class="text-center py-4">
            <div class="spinner-border spinner-border-sm text-warning" role="status"></div>
            <p class="text-muted small mt-2">Đang tải bệnh sử...</p>
          </div>

          <div v-else-if="medicalHistory.length === 0" class="text-center py-5 text-muted small">
            <i class="bi bi-folder-x fs-2 d-block mb-1 text-black-50 opacity-50"></i>
            Chưa có bệnh án cũ được ghi nhận.
          </div>

          <div v-else class="medical-timeline">
            <div v-for="record in medicalHistory" :key="record.recordId" class="timeline-item position-relative ps-4 pb-4">
              <div class="timeline-line"></div>
              <div class="timeline-circle bg-warning"></div>
              
              <div class="timeline-content p-3 bg-light rounded-4">
                <div class="d-flex justify-content-between align-items-center mb-2 flex-wrap">
                  <span class="text-dark fw-bold small"><i class="bi bi-calendar3 me-1"></i>{{ formatDate(record.visitDate) }}</span>
                  <span class="badge bg-secondary text-white small" style="font-size:0.65rem;">BS. {{ record.doctorName }}</span>
                </div>
                
                <div class="small mb-1"><strong class="text-secondary">Chẩn đoán:</strong> {{ record.diagnosis }}</div>
                <div class="small mb-1"><strong class="text-secondary">Điều trị:</strong> {{ record.treatment }}</div>
                <div v-if="record.symptoms" class="small mb-1"><strong class="text-secondary">Triệu chứng:</strong> {{ record.symptoms }}</div>
                <div v-if="record.note" class="small mb-2 text-muted italic">"{{ record.note }}"</div>
                
                <!-- Prescribed medicines list -->
                <div v-if="record.prescribedMedicines && record.prescribedMedicines.length > 0" class="mt-2 border-top pt-2">
                  <span class="fw-bold text-success d-block small mb-1" style="font-size: 0.72rem;"><i class="bi bi-capsule-pill me-1"></i>Thuốc kê đơn:</span>
                  <ul class="list-unstyled mb-0 px-2">
                    <li v-for="(medStr, mIdx) in record.prescribedMedicines" :key="mIdx" class="text-muted" style="font-size: 0.75rem;">
                      • {{ medStr }}
                    </li>
                  </ul>
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
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

const activeAccordion = ref('S');

const toggleAccordion = (section: string) => {
  if (activeAccordion.value === section) {
    activeAccordion.value = ''; // Collapse if clicking the same
  } else {
    activeAccordion.value = section; // Open the new one
  }
};

// Active patient details loaded from LocalStorage
const activePatient = ref({
  appointmentId: '',
  petId: '',
  petName: '',
  customerName: ''
});

// Form states
const form = ref({
  appointmentId: 0,
  petId: 0,
  recordType: 'Consultation',
  medicalHistory: '',
  weight: null as number | null,
  temperature: null as number | null,
  clinicalSigns: '',
  diagnosis: '',
  treatmentPlan: '',
  doctorNotes: '',
  followUpDate: '',
  prescriptions: [] as Array<{
    medicineId: number | null;
    quantity: number;
    dosage: string;
    frequency: string;
    durationDays: number;
    instruction: string;
    stockQuantity: number;
    unit: string;
  }>
});

const medicineOptions = ref<any[]>([]);
const medicalHistory = ref<any[]>([]);

const loadingHistory = ref(false);
const submitting = ref(false);
const errorMessage = ref('');

// Computed: Check if any medicine does not have enough stock
const hasStockDeficit = computed(() => {
  return form.value.prescriptions.some(
    p => p.medicineId !== null && (p.stockQuantity === 0 || p.quantity > p.stockQuantity)
  );
});

// Load patient context and medicines list
onMounted(async () => {
  const appointmentId = localStorage.getItem('active_treatment_appointment_id');
  const petId = localStorage.getItem('active_treatment_pet_id');
  const petName = localStorage.getItem('active_treatment_pet_name');
  const customerName = localStorage.getItem('active_treatment_customer_name');

  if (appointmentId && petId) {
    activePatient.value = {
      appointmentId,
      petId,
      petName: petName || 'Bệnh nhi',
      customerName: customerName || 'Khách vãng lai'
    };
    form.value.appointmentId = parseInt(appointmentId, 10);
    form.value.petId = parseInt(petId, 10);
    
    // Load Medical history of the pet
    fetchPetHistory(parseInt(petId, 10));
  }

  // Load list of medicines for prescription form
  fetchMedicines();
});

const fetchMedicines = async () => {
  try {
    const res = await api.get('/medicines');
    medicineOptions.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải danh mục thuốc:', err);
  }
};

const fetchPetHistory = async (petId: number) => {
  loadingHistory.value = true;
  try {
    const res = await api.get(`/medical-records/pet/${petId}`);
    medicalHistory.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải bệnh sử:', err);
  } finally {
    loadingHistory.value = false;
  }
};

// Prescription List Actions
const addPrescriptionLine = () => {
  form.value.prescriptions.push({
    medicineId: null,
    quantity: 1,
    dosage: '',
    frequency: '',
    durationDays: 5,
    instruction: '',
    stockQuantity: 9999,
    unit: 'đơn vị'
  });
};

const removePrescriptionLine = (index: number) => {
  form.value.prescriptions.splice(index, 1);
};

const onMedicineChange = (idx: number, medicineId: number | null) => {
  if (medicineId === null) return;
  const match = medicineOptions.value.find(m => m.id === medicineId);
  if (match) {
    form.value.prescriptions[idx].stockQuantity = match.stockQuantity;
    form.value.prescriptions[idx].unit = match.unit || 'đơn vị';
    form.value.prescriptions[idx].dosage = '1 viên';
    form.value.prescriptions[idx].frequency = '2 lần/ngày';
    form.value.prescriptions[idx].instruction = 'Sau ăn';
  }
};

// Form submission & cancellation
const submitForm = async () => {
  if (!form.value.appointmentId) {
    errorMessage.value = 'Không tìm thấy ID cuộc hẹn hoạt động.';
    return;
  }
  if (!form.value.diagnosis || !form.value.treatmentPlan) {
    errorMessage.value = 'Vui lòng điền đầy đủ Chẩn đoán bệnh và Phương pháp điều trị.';
    return;
  }

  submitting.value = true;
  errorMessage.value = '';

  try {
    const payload = {
      appointmentId: form.value.appointmentId,
      petId: form.value.petId,
      recordType: form.value.recordType,
      medicalHistory: form.value.medicalHistory || undefined,
      weight: form.value.weight,
      temperature: form.value.temperature,
      clinicalSigns: form.value.clinicalSigns,
      diagnosis: form.value.diagnosis,
      treatmentPlan: form.value.treatmentPlan,
      doctorNotes: form.value.doctorNotes || undefined,
      followUpDate: form.value.followUpDate ? new Date(form.value.followUpDate).toISOString() : undefined,
      prescriptions: form.value.prescriptions
        .filter(p => p.medicineId !== null)
        .map(p => ({
          medicineId: p.medicineId,
          quantity: p.quantity,
          dosage: p.dosage || '1 viên',
          frequency: p.frequency || '1 lần/ngày',
          durationDays: p.durationDays || 5,
          instruction: p.instruction || ''
        }))
    };

    const res = await api.post('/medical-records', payload);
    if (res.data.success) {
      // Clear active treatment context from LocalStorage
      localStorage.removeItem('active_treatment_appointment_id');
      localStorage.removeItem('active_treatment_pet_id');
      localStorage.removeItem('active_treatment_pet_name');
      localStorage.removeItem('active_treatment_customer_name');
      
      // Go back to cases queue
      emit('switch-tab', 'doctor-cases');
    }
  } catch (err: any) {
    errorMessage.value = err.response?.data?.message || 'Không thể lưu bệnh án. Vui lòng kiểm tra lại.';
  } finally {
    submitting.value = false;
  }
};

const cancelTreatment = () => {
  emit('switch-tab', 'doctor-cases');
};

// Format Helpers
const formatPrice = (price: number | null): string => {
  if (price === null) return '0đ';
  return price.toLocaleString('vi-VN') + 'đ';
};

const formatDate = (dateStr: string): string => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });
};
</script>

<style scoped>
.medical-records-tab {
  padding: 0;
}

.bg-glass {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
}

.pet-avatar-large {
  font-size: 3rem;
  background-color: #fff9e6;
  border-radius: 50%;
  width: 70px;
  height: 70px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 3px solid #ffc107;
  box-shadow: 0 4px 12px rgba(255, 193, 7, 0.2);
}

.card-gradient-pet {
  background: linear-gradient(135deg, #fffbeb 0%, #fff9e6 100%);
  border-left: 5px solid #ffc107 !important;
}

.prescription-line-card {
  position: relative;
  border: 1px solid #e9ecef;
  transition: all 0.2s;
}

.prescription-line-card:hover {
  box-shadow: 0 4px 10px rgba(0,0,0,0.05) !important;
}

.btn-remove-line {
  position: absolute;
  top: 10px;
  right: 10px;
  padding: 0;
  line-height: 1;
  opacity: 0.6;
  transition: opacity 0.2s;
}
.btn-remove-line:hover {
  opacity: 1;
}

.btn-premium {
  background: linear-gradient(135deg, #ffc107, #ff9800);
  border: none;
  transition: all 0.3s;
  box-shadow: 0 4px 12px rgba(255, 152, 0, 0.3);
}
.btn-premium:hover {
  transform: translateY(-1px);
  box-shadow: 0 6px 18px rgba(255, 152, 0, 0.4);
}

.btn-premium:disabled {
  background: #cbd5e1;
  box-shadow: none;
  cursor: not-allowed;
  transform: none;
}

.timeline-item {
  position: relative;
}

.timeline-line {
  position: absolute;
  top: 12px;
  left: 6px;
  bottom: 0;
  width: 2px;
  background-color: #e2e8f0;
}

.timeline-item:last-child .timeline-line {
  display: none;
}

.timeline-circle {
  position: absolute;
  top: 8px;
  left: 1px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid white;
  z-index: 1;
}

.timeline-content {
  box-shadow: 0 2px 8px rgba(0,0,0,0.02);
  border: 1px solid #f1f5f9;
}

/* SOAP Accordion Styles */
.custom-soap-accordion .accordion-item {
  border: 1px solid #e9ecef !important;
}
.custom-soap-accordion .accordion-button {
  background-color: #fdfaf0 !important;
  color: #333 !important;
}
.custom-soap-accordion .accordion-button:not(.collapsed) {
  background-color: #fff8e1 !important;
  color: #000 !important;
  box-shadow: inset 0 -1px 0 rgba(0,0,0,.125);
}
.soap-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  color: white;
  font-size: 0.8rem;
  font-weight: 800;
}
.s-badge { background-color: #3b82f6; }
.o-badge { background-color: #10b981; }
.a-badge { background-color: #f59e0b; }
.p-badge { background-color: #ef4444; }
</style>
