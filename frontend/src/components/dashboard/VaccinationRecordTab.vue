<template>
  <div class="medical-records-tab h-100 d-flex flex-column">
    <!-- Inner Tabs Navigation -->
    <ul class="nav nav-pills custom-pills mb-4">
      <li class="nav-item">
        <a class="nav-link fw-bold" :class="{ 'active': internalTab === 'treatment' }" href="#" @click.prevent="internalTab = 'treatment'">
          <i class="bi bi-shield-plus me-1"></i> Bệnh Án Tiêm Chủng
        </a>
      </li>
      <li class="nav-item">
        <a class="nav-link fw-bold" :class="{ 'active': internalTab === 'pet-history' }" href="#" @click.prevent="internalTab = 'pet-history'">
          <i class="bi bi-folder2-open me-1"></i> Lịch Sử Tiêm Phòng
        </a>
      </li>
    </ul>

    <!-- Tab 1: Phiếu Tiêm Chủng SOAP -->
    <div v-if="internalTab === 'treatment'" class="flex-grow-1 overflow-auto pe-2 pb-5">
      <!-- Header -->
      <div class="d-flex flex-column mb-3">
        <h4 class="fw-bold text-dark mb-0">Bệnh Án Tiêm Chủng (S.O.A.P)</h4>
        <span class="text-muted small">Hôm nay: {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: '2-digit', day: '2-digit' }) }}</span>
      </div>

      <div class="card border-0 shadow-sm rounded-4 p-4 bg-glass bg-white border-top border-4 border-success">
        <div class="d-flex justify-content-between align-items-center mb-4 border-bottom pb-3">
          <div>
            <a href="#" class="text-muted text-decoration-none fw-bold" @click.prevent="cancelTreatment">
              <i class="bi bi-arrow-left me-1"></i> Trở về hàng khám
            </a>
            <span class="mx-2 text-muted">/</span>
            <span class="fw-bold text-dark fs-5">{{ activePatient.petName }}</span>
          </div>
          <div class="badge bg-success bg-opacity-10 text-success rounded-pill px-4 py-2 fs-6 shadow-sm border border-success border-opacity-25">
            <i class="bi bi-shield-check me-1"></i> S.O.A.P
          </div>
        </div>

        <div v-if="!activePatient.appointmentId" class="alert alert-info rounded-4 border-0 p-4 text-center">
          <i class="bi bi-exclamation-triangle-fill text-info fs-1 d-block mb-2"></i>
          <h6 class="fw-bold text-dark">Chưa chọn ca khám hoạt động</h6>
        </div>

        <form v-else @submit.prevent="submitForm">
          <!-- Quick Info Row -->
          <div class="row mb-4 bg-light rounded-4 p-3 mx-0">
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Chủ nuôi</div>
              <div class="fw-bold text-dark">{{ activePatient.customerName }}</div>
            </div>
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Thú cưng</div>
              <div class="fw-bold text-dark">{{ activePatient.petName }} <span class="small fw-normal text-muted">(ID: {{ activePatient.petId }})</span></div>
            </div>
            <div class="col-md-3 border-end">
              <div class="small text-muted mb-1">Cân nặng (kg) <span class="text-danger">*</span></div>
              <input type="number" step="0.1" v-model="form.weight" class="form-control form-control-sm border-warning rounded-3 bg-white" placeholder="VD: 5.2" required>
            </div>
            <div class="col-md-3">
              <div class="small text-muted mb-1">Nhiệt độ (°C)</div>
              <input type="number" step="0.1" v-model="form.temperature" class="form-control form-control-sm border-warning rounded-3 bg-white" placeholder="VD: 38.5">
            </div>
          </div>

          <!-- Split-pane Layout -->
          <div class="row g-4">
            
            <!-- CỘT TRÁI: S (Subjective) & O (Objective) -->
            <div class="col-lg-6 d-flex flex-column gap-4">
              
              <!-- S - Subjective -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-primary fs-5" style="width:35px;height:35px;line-height:22px">S</span>
                <h6 class="fw-bold text-primary mb-3 ms-2 border-bottom pb-2">Thông tin chủ quan (Khách hàng)</h6>
                
                <div class="mb-3">
                  <label class="form-label small fw-bold text-secondary">Lý do tiêm <span class="text-danger">*</span></label>
                  <select v-model="form.reasonForVisit" class="form-select rounded-3 shadow-sm" required>
                    <option value="" disabled>-- Chọn lý do --</option>
                    <option value="Tiêm cơ bản">Tiêm cơ bản mũi mới</option>
                    <option value="Tiêm nhắc lại">Tiêm nhắc lại theo lịch</option>
                    <option value="Tư vấn tiêm chủng">Tư vấn tiêm chủng</option>
                  </select>
                </div>
                
                <div class="row g-3 mb-3">
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Tình trạng ăn uống <span class="text-danger">*</span></label>
                    <select v-model="form.eatingStatus" class="form-select rounded-3 shadow-sm" required>
                      <option value="Bình thường">Bình thường</option>
                      <option value="Biếng ăn">Biếng ăn</option>
                      <option value="Bỏ ăn">Bỏ ăn</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Tiền sử vắc-xin</label>
                    <input type="text" v-model="form.previousVaccineHistory" class="form-control rounded-3 shadow-sm" placeholder="VD: Đã tiêm dại cách đây 1 năm">
                  </div>
                </div>

                <div class="row g-2 mb-3">
                  <div class="col-6">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="form.hasVomitingOrDiarrhea" id="vomit">
                      <label class="form-check-label small" for="vomit">Nôn mửa / Tiêu chảy</label>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="form.hasCoughOrSneeze" id="cough">
                      <label class="form-check-label small" for="cough">Ho / Hắt hơi</label>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="form.isAllergic" id="allergy">
                      <label class="form-check-label small text-danger fw-bold" for="allergy">Dị ứng</label>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" v-model="form.hasPreviousReaction" id="reaction">
                      <label class="form-check-label small text-danger fw-bold" for="reaction">Sốc/Phản ứng tiêm cũ</label>
                    </div>
                  </div>
                </div>

                <!-- Fields conditional on switches -->
                <div v-if="form.isAllergic" class="mb-3">
                  <input type="text" v-model="form.allergyDetails" class="form-control form-control-sm border-danger text-danger bg-danger bg-opacity-10" placeholder="Chi tiết tình trạng dị ứng..." required>
                </div>
                <div v-if="form.hasPreviousReaction" class="mb-3">
                  <input type="text" v-model="form.previousReactionDetails" class="form-control form-control-sm border-danger text-danger bg-danger bg-opacity-10" placeholder="Triệu chứng phản vệ lần trước..." required>
                </div>
              </div>

              <!-- O - Objective -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-info fs-5" style="width:35px;height:35px;line-height:22px">O</span>
                <h6 class="fw-bold text-info mb-3 ms-2 border-bottom pb-2 text-dark">Khám lâm sàng (Bác sĩ)</h6>
                
                <div class="row g-3 mb-3">
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Tinh thần <span class="text-danger">*</span></label>
                    <select v-model="form.mentalStatus" class="form-select rounded-3 shadow-sm" required>
                      <option value="Linh hoạt">Linh hoạt</option>
                      <option value="Lờ đờ">Lờ đờ</option>
                      <option value="Hưng phấn">Hưng phấn</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Niêm mạc <span class="text-danger">*</span></label>
                    <select v-model="form.mucosaStatus" class="form-select rounded-3 shadow-sm" required>
                      <option value="Hồng hào">Hồng hào</option>
                      <option value="Nhợt nhạt">Nhợt nhạt</option>
                      <option value="Vàng">Vàng</option>
                    </select>
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Nhịp tim (lần/p)</label>
                    <input type="number" v-model="form.heartRate" class="form-control rounded-3 shadow-sm">
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Nhịp thở (lần/p)</label>
                    <input type="number" v-model="form.respiratoryRate" class="form-control rounded-3 shadow-sm">
                  </div>
                  <div class="col-md-4">
                    <label class="form-label small fw-bold text-secondary">Mất nước (%)</label>
                    <input type="number" v-model="form.dehydrationPercent" class="form-control rounded-3 shadow-sm" placeholder="< 5%">
                  </div>
                </div>
              </div>
            </div>

            <!-- CỘT PHẢI: A (Assessment) & P (Plan) -->
            <div class="col-lg-6 d-flex flex-column gap-4">
              
              <!-- A - Assessment -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-success fs-5" style="width:35px;height:35px;line-height:22px">A</span>
                <h6 class="fw-bold text-success mb-3 ms-2 border-bottom pb-2">Đánh giá & Vắc-xin</h6>

                <div class="mb-4">
                  <label class="form-label small fw-bold text-secondary">Kết luận lâm sàng <span class="text-danger">*</span></label>
                  <div class="d-flex gap-3">
                    <div class="form-check form-check-inline border p-2 rounded-3 bg-white" :class="{'border-success bg-success bg-opacity-10': form.clinicalAssessment === 'Đủ điều kiện'}">
                      <input class="form-check-input" type="radio" v-model="form.clinicalAssessment" value="Đủ điều kiện" id="assess1" required>
                      <label class="form-check-label fw-bold text-success" for="assess1">Đủ điều kiện tiêm</label>
                    </div>
                    <div class="form-check form-check-inline border p-2 rounded-3 bg-white" :class="{'border-danger bg-danger bg-opacity-10': form.clinicalAssessment === 'Hoãn tiêm'}">
                      <input class="form-check-input" type="radio" v-model="form.clinicalAssessment" value="Hoãn tiêm" id="assess2" required>
                      <label class="form-check-label fw-bold text-danger" for="assess2">Hoãn tiêm</label>
                    </div>
                  </div>
                </div>

                <div v-if="form.clinicalAssessment === 'Hoãn tiêm'" class="mb-3">
                  <label class="form-label small fw-bold text-danger">Lý do hoãn tiêm (Chỉ tạo Phí Khám) <span class="text-danger">*</span></label>
                  <textarea v-model="form.doctorRemarks" class="form-control rounded-3 border-danger shadow-sm" rows="2" placeholder="Sốt cao, cần theo dõi thêm..." required></textarea>
                </div>

                <div v-if="form.clinicalAssessment === 'Đủ điều kiện'" class="vaccine-selector border border-success border-opacity-50 p-3 rounded-4 bg-white mb-3">
                  <div class="mb-3">
                    <label class="form-label small fw-bold text-secondary">Chọn Vắc-xin <span class="text-danger">*</span></label>
                    <select v-model="form.vaccineId" @change="onVaccineChange" class="form-select border-success rounded-3 shadow-sm fw-bold" required>
                      <option :value="null" disabled>-- Danh mục Vắc-xin --</option>
                      <option v-for="v in availableVaccines" :key="v.id" :value="v.id">
                        {{ v.name }} ({{ v.targetSpecies }})
                      </option>
                    </select>
                  </div>

                  <div v-if="form.vaccineId" class="mb-0">
                    <label class="form-label small fw-bold text-secondary">Chọn Lô & Hạn sử dụng <span class="text-danger">*</span></label>
                    <select v-model="form.vaccineBatchId" class="form-select rounded-3 shadow-sm" required>
                      <option :value="null" disabled>-- Chọn Lô --</option>
                      <option v-for="b in selectedVaccineBatches" :key="b.id" :value="b.id">
                        Lô: {{ b.batchNumber }} - HSD: {{ formatDate(b.expirationDate) }} (Tồn: {{ b.stockQuantity }}) - Giá: {{ b.sellingPrice.toLocaleString() }}đ
                      </option>
                    </select>
                    <div v-if="selectedVaccineBatches.length === 0" class="text-danger small mt-1">
                      <i class="bi bi-x-circle"></i> Vắc-xin này hiện đang hết hàng / Hết hạn sử dụng lô.
                    </div>
                  </div>
                </div>

                <div class="row g-3" v-if="form.clinicalAssessment === 'Đủ điều kiện'">
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Đường tiêm</label>
                    <select v-model="form.route" class="form-select rounded-3 shadow-sm">
                      <option value="Dưới da (SC)">Dưới da (SC)</option>
                      <option value="Tiêm bắp (IM)">Tiêm bắp (IM)</option>
                      <option value="Nhỏ mũi">Nhỏ mũi</option>
                    </select>
                  </div>
                  <div class="col-md-6">
                    <label class="form-label small fw-bold text-secondary">Vị trí tiêm</label>
                    <input type="text" v-model="form.injectionSite" class="form-control rounded-3 shadow-sm" placeholder="VD: Đùi phải">
                  </div>
                </div>
              </div>

              <!-- P - Plan -->
              <div class="p-4 rounded-4 bg-light border shadow-sm position-relative">
                <span class="position-absolute top-0 start-0 translate-middle badge rounded-pill bg-warning text-dark fs-5" style="width:35px;height:35px;line-height:22px">P</span>
                <h6 class="fw-bold text-warning mb-3 ms-2 border-bottom pb-2">Kế hoạch & Dặn dò</h6>
                
                <div class="mb-3">
                  <label class="form-label small fw-bold text-secondary"><i class="bi bi-calendar-event text-warning me-1"></i> Ngày nhắc lại dự kiến</label>
                  <input type="date" v-model="form.nextDueDate" class="form-control rounded-pill shadow-sm border px-3">
                </div>
                
                <div class="mb-0">
                  <label class="form-label small fw-bold text-secondary">Lời dặn dò về nhà (In lên phiếu) <span class="text-danger">*</span></label>
                  <textarea v-model="form.followUpInstructions" class="form-control rounded-3 shadow-sm" rows="2" placeholder="Kiêng tắm 7 ngày, theo dõi nếu sốt..." required></textarea>
                </div>
              </div>

            </div>
          </div>

          <!-- Error Banner -->
          <div v-if="errorMessage" class="alert alert-danger rounded-4 p-3 small my-4 shadow-sm border-0 d-flex align-items-center">
            <i class="bi bi-exclamation-circle-fill fs-5 me-2"></i> {{ errorMessage }}
          </div>

          <!-- Actions -->
          <div class="d-flex justify-content-end align-items-center border-top pt-4 mt-4">
            <button type="button" class="btn btn-light rounded-pill px-4 fw-bold text-dark me-3 shadow-sm border" @click="cancelTreatment">
              Hủy bỏ
            </button>
            <button type="submit" class="btn btn-success rounded-pill px-5 fw-bold text-white shadow-lg btn-save" :disabled="submitting || isSubmitDisabled">
              <span v-if="submitting" class="spinner-border spinner-border-sm me-1"></span>
              <i class="bi bi-check2-all me-1"></i> Lưu Bệnh Án SOAP
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Tab 2: Lịch Sử Tiêm Phòng -->
    <div v-if="internalTab === 'pet-history'" class="flex-grow-1 overflow-auto">
      <div class="card border-0 shadow-sm rounded-4 p-4 bg-white h-100 border">
        <h6 class="fw-bold mb-4 pb-3 text-dark border-bottom"><i class="bi bi-shield-check text-success me-2"></i>Lịch sử tiêm chủng của bé</h6>
        
        <div v-if="loadingHistory" class="text-center py-5">
          <div class="spinner-border text-success" role="status"></div>
          <p class="text-muted small mt-3">Đang tải lịch sử tiêm...</p>
        </div>

        <div v-else-if="vaccinationHistory.length === 0" class="text-center py-5 text-muted small">
          <i class="bi bi-shield-x fs-1 d-block mb-3 text-black-50 opacity-25"></i>
          Bé chưa có lịch sử tiêm chủng nào trong hệ thống.
        </div>

        <div v-else class="table-responsive">
          <table class="table table-hover align-middle">
            <thead class="bg-light text-secondary small">
              <tr>
                <th>Ngày Tiêm</th>
                <th>Vắc-xin & Lô</th>
                <th>Bác sĩ</th>
                <th>Đánh giá</th>
                <th>Ngày Nhắc Lại</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="record in vaccinationHistory" :key="record.id">
                <td class="fw-bold text-dark">{{ formatDate(record.injectionDate) }}</td>
                <td>
                  <span v-if="record.vaccineName" class="badge bg-primary rounded-pill px-3 py-1 mb-1">{{ record.vaccineName }}</span><br>
                  <span v-if="record.batchNumber" class="small text-muted">Lô: {{ record.batchNumber }}</span>
                </td>
                <td>Bs. {{ record.doctorName }}</td>
                <td>
                  <span class="badge" :class="record.clinicalAssessment === 'Đủ điều kiện' ? 'bg-success' : 'bg-danger'">{{ record.clinicalAssessment }}</span>
                </td>
                <td class="fw-bold text-warning">{{ record.nextDueDate ? formatDate(record.nextDueDate) : '---' }}</td>
              </tr>
            </tbody>
          </table>
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

const internalTab = ref('treatment');
const activePatient = ref({
  appointmentId: '',
  petId: '',
  petName: '',
  customerName: ''
});

// SOAP Form
const form = ref({
  appointmentId: 0,
  petId: 0,
  weight: null as number | null,
  temperature: null as number | null,
  reasonForVisit: '',
  previousVaccineHistory: '',
  isAllergic: false,
  allergyDetails: '',
  hasPreviousReaction: false,
  previousReactionDetails: '',
  isUnderTreatment: false,
  treatmentDetails: '',
  eatingStatus: 'Bình thường',
  hasVomitingOrDiarrhea: false,
  hasCoughOrSneeze: false,
  ownerNotes: '',
  heartRate: null as number | null,
  respiratoryRate: null as number | null,
  mentalStatus: 'Linh hoạt',
  mucosaStatus: 'Hồng hào',
  eyeNoseEarStatus: '',
  lymphNodeStatus: '',
  dehydrationPercent: null as number | null,
  vaccineId: null as number | null,
  vaccineBatchId: null as number | null,
  dose: 1,
  route: 'Dưới da (SC)',
  injectionSite: '',
  clinicalAssessment: 'Đủ điều kiện',
  doctorRemarks: '',
  nextDueDate: '',
  followUpInstructions: '',
  reactionNote: ''
});

const availableVaccines = ref<any[]>([]);
const vaccinationHistory = ref<any[]>([]);

const loadingHistory = ref(false);
const submitting = ref(false);
const errorMessage = ref('');

const selectedVaccineBatches = computed(() => {
  if (!form.value.vaccineId) return [];
  const vaccine = availableVaccines.value.find(v => v.id === form.value.vaccineId);
  return vaccine ? vaccine.batches : [];
});

const isSubmitDisabled = computed(() => {
  if (form.value.clinicalAssessment === 'Đủ điều kiện') {
    return !form.value.vaccineId || !form.value.vaccineBatchId;
  }
  return false;
});

onMounted(async () => {
  const appointmentId = localStorage.getItem('active_treatment_appointment_id');
  const petId = localStorage.getItem('active_treatment_pet_id');
  
  if (appointmentId && petId) {
    activePatient.value = {
      appointmentId,
      petId,
      petName: localStorage.getItem('active_treatment_pet_name') || 'Bệnh nhi',
      customerName: localStorage.getItem('active_treatment_customer_name') || 'Khách vãng lai'
    };
    form.value.appointmentId = parseInt(appointmentId, 10);
    form.value.petId = parseInt(petId, 10);
    
    fetchPetVaccinationHistory(parseInt(petId, 10));
  }

  fetchVaccines();
});

const fetchVaccines = async () => {
  try {
    const res = await api.get('/vaccinations/vaccines');
    if (res.data.success) {
      availableVaccines.value = res.data.data || [];
    }
  } catch (err) {
    console.error('Lỗi tải danh mục vắc-xin:', err);
  }
};

const fetchPetVaccinationHistory = async (petId: number) => {
  loadingHistory.value = true;
  try {
    const res = await api.get(`/vaccinations/pet/${petId}`);
    vaccinationHistory.value = res.data || res.data?.data || [];
  } catch (err) {
    console.error('Lỗi tải lịch sử tiêm chủng:', err);
  } finally {
    loadingHistory.value = false;
  }
};

const onVaccineChange = () => {
  form.value.vaccineBatchId = null; // reset batch when vaccine changes
  
  // Auto calculate nextDueDate based on vaccine intervalDays
  const vaccine = availableVaccines.value.find(v => v.id === form.value.vaccineId);
  if (vaccine && vaccine.intervalDays) {
    const nextDate = new Date();
    nextDate.setDate(nextDate.getDate() + vaccine.intervalDays);
    form.value.nextDueDate = nextDate.toISOString().split('T')[0];
  } else {
    form.value.nextDueDate = '';
  }
};

const submitForm = async () => {
  submitting.value = true;
  errorMessage.value = '';

  try {
    // Xử lý logic VR03, VR04, VR06 trước khi gửi
    const payload = { ...form.value };
    
    if (payload.clinicalAssessment === 'Hoãn tiêm') {
      payload.vaccineId = null;
      payload.vaccineBatchId = null;
    }

    if (payload.nextDueDate) {
      payload.nextDueDate = new Date(payload.nextDueDate).toISOString();
    } else {
      payload.nextDueDate = undefined as any;
    }

    const res = await api.post(`/vaccinations/appointments/${payload.appointmentId}`, payload);
    if (res.data.success) {
      // Tự động chuyển sang trạng thái chờ thanh toán
      try {
        await api.put(`/receptionist/queue/${payload.appointmentId}/status`, { status: 'ready_to_pay' });
      } catch (e) {
        console.warn('Không thể tự chuyển trạng thái ready_to_pay:', e);
      }

      localStorage.removeItem('active_treatment_appointment_id');
      localStorage.removeItem('active_treatment_pet_id');
      localStorage.removeItem('active_treatment_pet_name');
      localStorage.removeItem('active_treatment_customer_name');
      localStorage.removeItem('active_treatment_service_type');
      
      emit('switch-tab', 'doctor-cases');
    }
  } catch (err: any) {
    // Map backend FluentValidation/DataAnnotations errors if available
    if (err.response?.data?.errors) {
      const errors = err.response.data.errors;
      const firstError = Object.values(errors)[0] as string[];
      errorMessage.value = firstError[0];
    } else {
      errorMessage.value = err.response?.data?.message || 'Không thể lưu bệnh án. Vui lòng kiểm tra lại dữ liệu.';
    }
  } finally {
    submitting.value = false;
  }
};

const cancelTreatment = () => {
  emit('switch-tab', 'doctor-cases');
};

const formatDate = (dateStr: string | null): string => {
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

.custom-pills .nav-link {
  color: #6b7280;
  border-radius: 50px;
  padding: 10px 24px;
  margin-right: 10px;
  background-color: #f3f4f6;
  transition: all 0.3s ease;
}

.custom-pills .nav-link:hover {
  background-color: #e5e7eb;
}

.custom-pills .nav-link.active {
  background: linear-gradient(135deg, #198754, #20c997);
  color: white;
  box-shadow: 0 4px 10px rgba(32, 201, 151, 0.3);
}

.btn-save {
  transition: all 0.3s;
}
.btn-save:hover {
  transform: translateY(-1px);
}
.btn-save:disabled {
  cursor: not-allowed;
  transform: none;
}
</style>
