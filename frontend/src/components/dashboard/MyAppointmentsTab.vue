<template>
  <div class="myappts-tab">

    <!-- Header -->
    <div class="appts-hero mb-4">
      <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
        <div>
          <h3 class="fw-bold text-dark mb-1">
            <i class="bi bi-calendar-check-fill me-2" style="color: var(--primary-gold);"></i>
            Lịch hẹn của tôi
          </h3>
          <p class="text-muted mb-0 small">Theo dõi và quản lý các buổi hẹn khám bệnh cho thú cưng.</p>
        </div>
        <button class="btn btn-premium-appt" @click="openBookModal">
          <i class="bi bi-plus-circle-fill me-2"></i> Đặt lịch mới
        </button>
      </div>
    </div>

    <!-- Filter Tabs -->
    <div class="filter-tabs mb-4">
      <button
        v-for="tab in filterOptions"
        :key="tab.value"
        class="filter-tab-btn"
        :class="{ active: activeFilter === tab.value }"
        @click="activeFilter = tab.value"
      >
        <i :class="tab.icon" class="me-1"></i> {{ tab.label }}
        <span v-if="getCountByStatus(tab.value) > 0" class="tab-count">{{ getCountByStatus(tab.value) }}</span>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;"></div>
      <p class="text-muted mt-3">Đang tải lịch hẹn...</p>
    </div>

    <!-- Error -->
    <div v-else-if="errorMsg" class="alert alert-danger rounded-4 border-0 shadow-sm py-3 px-4">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Empty -->
    <div v-else-if="filteredAppointments.length === 0" class="empty-state-appt">
      <div class="empty-icon">📅</div>
      <h5 class="fw-bold text-dark mt-3 mb-2">
        {{ activeFilter === 'all' ? 'Chưa có lịch hẹn nào' : `Không có lịch hẹn ${getStatusLabel(activeFilter)}` }}
      </h5>
      <p class="text-muted small mb-4">Đặt lịch ngay để được phục vụ nhanh hơn, không cần chờ đợi.</p>
      <button class="btn btn-premium-appt" @click="openBookModal">
        <i class="bi bi-plus-circle-fill me-2"></i> Đặt lịch ngay
      </button>
    </div>

    <!-- Appointment Cards -->
    <div v-else>
      <TransitionGroup name="appt-card" tag="div" class="appts-list">
        <div
          v-for="appt in filteredAppointments"
          :key="appt.id"
          class="appt-card"
          :class="`status-${appt.status}`"
        >
          <!-- Left accent bar -->
          <div class="appt-accent-bar"></div>

          <!-- Content -->
          <div class="appt-card-content">
            <!-- Top row: date + status -->
            <div class="appt-top-row">
              <div class="appt-date-block">
                <div class="appt-date-day">{{ formatDay(appt.appointmentDate) }}</div>
                <div class="appt-date-rest">{{ formatMonthYear(appt.appointmentDate) }}</div>
                <div class="appt-date-time">🕐 {{ formatTime(appt.appointmentDate) }}</div>
              </div>
              <div class="d-flex flex-column align-items-end gap-2">
                <span class="appt-status-badge" :class="`badge-${appt.status}`">
                  <i :class="getStatusIcon(appt.status)" class="me-1"></i>
                  {{ getStatusLabel(appt.status) }}
                </span>
                <span v-if="appt.invoiceStatus" class="invoice-badge">
                  <i class="bi bi-receipt me-1"></i>
                  {{ getInvoiceStatusLabel(appt.invoiceStatus) }}
                </span>
              </div>
            </div>

            <!-- Middle: pet & service info -->
            <div class="appt-info-grid">
              <div class="appt-info-item">
                <span class="appt-info-label">Thú cưng</span>
                <span class="appt-info-value">
                  <span class="pet-inline-badge">{{ appt.petName || '—' }}</span>
                  <small class="text-muted ms-1">{{ appt.species }}</small>
                </span>
              </div>
              <div class="appt-info-item">
                <span class="appt-info-label">Dịch vụ</span>
                <span class="appt-info-value">{{ appt.serviceName }}</span>
              </div>
              <div class="appt-info-item">
                <span class="appt-info-label">Bác sĩ phụ trách</span>
                <span class="appt-info-value">{{ appt.doctorName || 'Chưa phân công' }}</span>
              </div>
              <div v-if="appt.symptom" class="appt-info-item">
                <span class="appt-info-label">Lý do khám</span>
                <span class="appt-info-value appt-symptom">{{ appt.symptom }}</span>
              </div>
            </div>

            <!-- Note -->
            <div v-if="appt.note" class="appt-note">
              <i class="bi bi-chat-left-text-fill me-1 text-muted"></i>
              <span>{{ appt.note }}</span>
            </div>

            <!-- Actions -->
            <div class="appt-actions">
              <button class="btn-appt-detail" @click="openDetailModal(appt)">
                <i class="bi bi-eye me-1"></i> Xem chi tiết
              </button>
              <button
                v-if="canCancel(appt.status)"
                class="btn-appt-cancel"
                @click="confirmCancel(appt)"
              >
                <i class="bi bi-x-circle me-1"></i> Huỷ lịch
              </button>
              <div v-if="appt.invoiceTotalAmount && appt.invoiceTotalAmount > 0" class="appt-price-tag">
                {{ formatCurrency(appt.invoiceTotalAmount) }}
              </div>
            </div>
          </div>
        </div>
      </TransitionGroup>
    </div>

    <!-- ===== BOOKING MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showBookModal" class="appt-modal-overlay" @click.self="closeBookModal">
          <div class="appt-modal-card">
            <div class="appt-modal-header">
              <h5 class="fw-bold mb-0">
                <i class="bi bi-calendar-plus-fill me-2 text-warning"></i>
                Đặt lịch hẹn mới
              </h5>
              <button class="modal-close-btn" @click="closeBookModal">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="appt-modal-body">
              <!-- Progress steps -->
              <div class="booking-steps mb-4">
                <div v-for="(step, idx) in bookingSteps" :key="idx" class="step-item" :class="{ active: currentStep >= idx, done: currentStep > idx }">
                  <div class="step-circle">
                    <i v-if="currentStep > idx" class="bi bi-check-lg"></i>
                    <span v-else>{{ idx + 1 }}</span>
                  </div>
                  <span class="step-label">{{ step }}</span>
                  <div v-if="idx < bookingSteps.length - 1" class="step-line"></div>
                </div>
              </div>

              <div v-if="bookingSuccess" class="text-center py-4">
                <div style="font-size: 4rem;">✅</div>
                <h5 class="fw-bold text-success mt-3 mb-2">Đặt lịch thành công!</h5>
                <p class="text-muted small">Chúng tôi sẽ xác nhận lịch hẹn của bạn sớm nhất có thể.</p>
                <button class="btn btn-premium-appt mt-3" @click="closeBookModal">
                  <i class="bi bi-check2-circle me-2"></i> Hoàn tất
                </button>
              </div>

              <form v-else @submit.prevent="submitBooking">
                <!-- Step 1: Choose Pet -->
                <div v-if="currentStep === 0">
                  <p class="text-muted small mb-3">Chọn thú cưng bạn muốn đặt lịch khám:</p>
                  <div v-if="myPets.length === 0" class="alert alert-warning border-0 rounded-3">
                    <i class="bi bi-exclamation-triangle me-2"></i>Bạn chưa có thú cưng. <a href="#" @click.prevent="$emit('switch-tab', 'my-pets')">Thêm thú cưng ngay</a>
                  </div>
                  <div v-else class="pet-select-grid">
                    <div
                      v-for="pet in myPets"
                      :key="pet.id"
                      class="pet-select-card"
                      :class="{ selected: bookForm.petId === pet.id }"
                      @click="bookForm.petId = pet.id"
                    >
                      <div class="pet-select-emoji">{{ getSpeciesEmoji(pet.species) }}</div>
                      <div class="pet-select-name">{{ pet.name }}</div>
                      <div class="pet-select-species text-muted">{{ pet.species }}</div>
                      <div v-if="bookForm.petId === pet.id" class="pet-select-check">
                        <i class="bi bi-check-circle-fill text-success"></i>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Step 2: Choose Service & Date -->
                <div v-else-if="currentStep === 1">
                  <div class="row g-3">
                    <div class="col-12">
                      <label class="form-label-custom">Dịch vụ <span class="text-danger">*</span></label>
                      <select v-model="bookForm.serviceId" class="form-control-custom" required>
                        <option value="">-- Chọn dịch vụ --</option>
                        <option v-for="svc in services" :key="svc.id" :value="svc.id">
                          {{ svc.name }} {{ svc.price ? `— ${formatCurrency(svc.price)}` : '' }}
                        </option>
                      </select>
                    </div>
                    <div class="col-12">
                      <label class="form-label-custom">Ngày & Giờ hẹn <span class="text-danger">*</span></label>
                      <input
                        v-model="bookForm.appointmentDate"
                        type="datetime-local"
                        class="form-control-custom"
                        :min="minDateStr"
                        required
                      />
                    </div>
                    <div class="col-12">
                      <label class="form-label-custom">Triệu chứng / Lý do khám <span class="text-danger">*</span></label>
                      <textarea
                        v-model="bookForm.symptom"
                        class="form-control-custom"
                        rows="3"
                        placeholder="Mô tả triệu chứng của thú cưng, lý do muốn khám..."
                        required
                      ></textarea>
                    </div>
                    <div class="col-12">
                      <label class="form-label-custom">Ghi chú thêm</label>
                      <textarea
                        v-model="bookForm.note"
                        class="form-control-custom"
                        rows="2"
                        placeholder="VD: thú cưng hay cắn, cần bác sĩ nữ..."
                      ></textarea>
                    </div>
                  </div>
                </div>

                <!-- Step 3: Confirm -->
                <div v-else-if="currentStep === 2">
                  <div class="booking-confirm-card">
                    <h6 class="fw-bold mb-3 text-dark">
                      <i class="bi bi-clipboard-check text-warning me-2"></i>Xác nhận thông tin đặt lịch
                    </h6>
                    <div class="confirm-row">
                      <span class="confirm-label">Thú cưng</span>
                      <span class="confirm-value">{{ getSelectedPetName() }}</span>
                    </div>
                    <div class="confirm-row">
                      <span class="confirm-label">Dịch vụ</span>
                      <span class="confirm-value">{{ getSelectedServiceName() }}</span>
                    </div>
                    <div class="confirm-row">
                      <span class="confirm-label">Thời gian</span>
                      <span class="confirm-value">{{ formatDatetimeLocal(bookForm.appointmentDate) }}</span>
                    </div>
                    <div class="confirm-row">
                      <span class="confirm-label">Lý do khám</span>
                      <span class="confirm-value">{{ bookForm.symptom }}</span>
                    </div>
                    <div v-if="bookForm.note" class="confirm-row">
                      <span class="confirm-label">Ghi chú</span>
                      <span class="confirm-value">{{ bookForm.note }}</span>
                    </div>
                  </div>
                  <div v-if="bookingError" class="alert alert-danger border-0 rounded-3 mt-3 py-2 px-3">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ bookingError }}
                  </div>
                </div>

                <!-- Navigation buttons -->
                <div class="booking-nav-btns mt-4">
                  <button
                    v-if="currentStep > 0"
                    type="button"
                    class="btn btn-outline-secondary rounded-pill px-4"
                    @click="currentStep--"
                    :disabled="bookingLoading"
                  >
                    <i class="bi bi-arrow-left me-1"></i> Quay lại
                  </button>
                  <div class="ms-auto d-flex gap-2">
                    <button
                      v-if="currentStep < 2"
                      type="button"
                      class="btn btn-premium-appt"
                      @click="nextStep"
                      :disabled="!canProceed"
                    >
                      Tiếp theo <i class="bi bi-arrow-right ms-1"></i>
                    </button>
                    <button
                      v-else
                      type="submit"
                      class="btn btn-premium-appt"
                      :disabled="bookingLoading"
                    >
                      <span v-if="bookingLoading" class="spinner-border spinner-border-sm me-2"></span>
                      <i v-else class="bi bi-check2-circle me-2"></i>
                      Xác nhận đặt lịch
                    </button>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ===== DETAIL MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showDetailModal && detailAppt" class="appt-modal-overlay" @click.self="showDetailModal = false">
          <div class="appt-modal-card">
            <div class="appt-modal-header" :class="`detail-header-${detailAppt.status}`">
              <div>
                <h5 class="fw-bold mb-1">
                  <i class="bi bi-calendar-event-fill me-2"></i>Chi tiết lịch hẹn #{{ detailAppt.id }}
                </h5>
                <span class="appt-status-badge" :class="`badge-${detailAppt.status}`">
                  <i :class="getStatusIcon(detailAppt.status)" class="me-1"></i>
                  {{ getStatusLabel(detailAppt.status) }}
                </span>
              </div>
              <button class="modal-close-btn" @click="showDetailModal = false">
                <i class="bi bi-x-lg"></i>
              </button>
            </div>

            <div class="appt-modal-body">
              <div class="row g-3">
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Thú cưng</div>
                    <div class="detail-value">{{ detailAppt.petName }} <small class="text-muted">({{ detailAppt.species }})</small></div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Bác sĩ phụ trách</div>
                    <div class="detail-value">{{ detailAppt.doctorName || 'Chưa phân công' }}</div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Dịch vụ</div>
                    <div class="detail-value">{{ detailAppt.serviceName }}</div>
                  </div>
                </div>
                <div class="col-sm-6">
                  <div class="detail-item">
                    <div class="detail-label">Thời gian</div>
                    <div class="detail-value">{{ formatDateFull(detailAppt.appointmentDate) }}</div>
                  </div>
                </div>
                <div v-if="detailAppt.symptom" class="col-12">
                  <div class="detail-item">
                    <div class="detail-label">Triệu chứng / Lý do khám</div>
                    <div class="detail-value">{{ detailAppt.symptom }}</div>
                  </div>
                </div>
                <div v-if="detailAppt.note" class="col-12">
                  <div class="detail-item">
                    <div class="detail-label">Ghi chú</div>
                    <div class="detail-value">{{ detailAppt.note }}</div>
                  </div>
                </div>
                <div v-if="detailAppt.invoiceId" class="col-12">
                  <div class="detail-item" :class="detailAppt.invoiceStatus === 'paid' ? 'item-success' : 'item-warning'">
                    <div class="detail-label"><i class="bi bi-receipt me-1"></i>Hóa đơn</div>
                    <div class="detail-value d-flex justify-content-between align-items-center">
                      <span>{{ getInvoiceStatusLabel(detailAppt.invoiceStatus) }}</span>
                      <strong v-if="detailAppt.invoiceTotalAmount" class="text-dark">{{ formatCurrency(detailAppt.invoiceTotalAmount) }}</strong>
                    </div>
                  </div>
                </div>
              </div>

              <div class="d-flex gap-2 mt-4">
                <button
                  v-if="canCancel(detailAppt.status)"
                  class="btn btn-outline-danger rounded-pill fw-semibold flex-fill"
                  @click="confirmCancelFromDetail(detailAppt)"
                >
                  <i class="bi bi-x-circle me-2"></i>Huỷ lịch hẹn này
                </button>
                <button class="btn btn-outline-secondary rounded-pill px-4" @click="showDetailModal = false">Đóng</button>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- ===== CANCEL CONFIRM MODAL ===== -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showCancelModal && apptToCancel" class="appt-modal-overlay" @click.self="showCancelModal = false">
          <div class="appt-modal-card" style="max-width: 420px;">
            <div class="appt-modal-header">
              <h5 class="fw-bold mb-0"><i class="bi bi-x-circle-fill text-danger me-2"></i>Huỷ lịch hẹn</h5>
              <button class="modal-close-btn" @click="showCancelModal = false"><i class="bi bi-x-lg"></i></button>
            </div>
            <div class="appt-modal-body text-center">
              <div style="font-size: 3.5rem; margin-bottom: 1rem;">🗓️</div>
              <p class="text-muted mb-4">Bạn có chắc muốn huỷ lịch hẹn <strong>{{ formatDateFull(apptToCancel.appointmentDate) }}</strong> không?<br>
              <small>Lưu ý: lịch hẹn đã huỷ không thể khôi phục.</small></p>
              <div class="d-flex gap-2 justify-content-center">
                <button class="btn btn-outline-secondary rounded-pill px-4" @click="showCancelModal = false" :disabled="cancelLoading">Giữ lịch</button>
                <button class="btn btn-danger rounded-pill px-4 fw-bold" @click="cancelAppointment" :disabled="cancelLoading">
                  <span v-if="cancelLoading" class="spinner-border spinner-border-sm me-2"></span>
                  <i v-else class="bi bi-x-circle me-2"></i>Xác nhận huỷ
                </button>
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

// ===== Emits =====
const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

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

interface Pet {
  id: number;
  name: string;
  species: string;
}

interface Service {
  id: number;
  name: string;
  price: number | null;
}

// ===== State =====
const appointments = ref<AppointmentDetail[]>([]);
const myPets = ref<Pet[]>([]);
const services = ref<Service[]>([]);
const loading = ref(false);
const errorMsg = ref('');

const activeFilter = ref('all');

// Detail modal
const showDetailModal = ref(false);
const detailAppt = ref<AppointmentDetail | null>(null);

// Cancel modal
const showCancelModal = ref(false);
const apptToCancel = ref<AppointmentDetail | null>(null);
const cancelLoading = ref(false);

// Book modal
const showBookModal = ref(false);
const currentStep = ref(0);
const bookingLoading = ref(false);
const bookingError = ref('');
const bookingSuccess = ref(false);

const bookForm = ref({
  petId: 0,
  serviceId: 0,
  appointmentDate: '',
  symptom: '',
  note: '',
});

const bookingSteps = ['Chọn thú cưng', 'Dịch vụ & Thời gian', 'Xác nhận'];

// ===== Filter Options =====
const filterOptions = [
  { value: 'all', label: 'Tất cả', icon: 'bi bi-list-ul' },
  { value: 'pending', label: 'Chờ xác nhận', icon: 'bi bi-hourglass-split' },
  { value: 'confirmed', label: 'Đã xác nhận', icon: 'bi bi-check-circle' },
  { value: 'in_progress', label: 'Đang khám', icon: 'bi bi-activity' },
  { value: 'completed', label: 'Hoàn thành', icon: 'bi bi-check2-all' },
  { value: 'cancelled', label: 'Đã huỷ', icon: 'bi bi-x-circle' },
];

// ===== Computed =====
const filteredAppointments = computed(() => {
  if (activeFilter.value === 'all') return appointments.value;
  return appointments.value.filter(a => a.status === activeFilter.value);
});

const minDateStr = computed(() => {
  const now = new Date();
  now.setMinutes(now.getMinutes() + 30); // At least 30 min from now
  return now.toISOString().slice(0, 16);
});

const canProceed = computed(() => {
  if (currentStep.value === 0) return bookForm.value.petId > 0;
  if (currentStep.value === 1) return bookForm.value.serviceId > 0 && bookForm.value.appointmentDate !== '' && bookForm.value.symptom.trim() !== '';
  return true;
});

// ===== API =====
const fetchAppointments = async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await api.get('/my-appointments');
    appointments.value = res.data;
  } catch (err: any) {
    errorMsg.value = 'Không thể tải lịch hẹn. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
};

const fetchPets = async () => {
  try {
    const res = await api.get('/mypets');
    myPets.value = res.data;
  } catch { /* silent */ }
};

const fetchServices = async () => {
  try {
    const res = await api.get('/my-appointments/services');
    services.value = res.data;
  } catch { /* silent */ }
};

const submitBooking = async () => {
  bookingLoading.value = true;
  bookingError.value = '';
  try {
    await api.post('/my-appointments', {
      petId: bookForm.value.petId,
      serviceId: bookForm.value.serviceId,
      appointmentDate: bookForm.value.appointmentDate,
      symptom: bookForm.value.symptom,
      note: bookForm.value.note,
    });
    bookingSuccess.value = true;
    await fetchAppointments();
  } catch (err: any) {
    bookingError.value = err?.response?.data?.message || 'Đặt lịch thất bại. Vui lòng thử lại.';
  } finally {
    bookingLoading.value = false;
  }
};

const cancelAppointment = async () => {
  if (!apptToCancel.value) return;
  cancelLoading.value = true;
  try {
    await api.put(`/my-appointments/${apptToCancel.value.id}/cancel`);
    await fetchAppointments();
    showCancelModal.value = false;
    showDetailModal.value = false;
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Huỷ lịch thất bại. Vui lòng thử lại.');
  } finally {
    cancelLoading.value = false;
  }
};

// ===== Modal controls =====
const openBookModal = async () => {
  bookForm.value = { petId: 0, serviceId: 0, appointmentDate: '', symptom: '', note: '' };
  currentStep.value = 0;
  bookingError.value = '';
  bookingSuccess.value = false;
  await fetchPets();
  await fetchServices();
  showBookModal.value = true;
};

const closeBookModal = () => {
  showBookModal.value = false;
};

const nextStep = () => {
  if (currentStep.value < 2) currentStep.value++;
};

const openDetailModal = (appt: AppointmentDetail) => {
  detailAppt.value = appt;
  showDetailModal.value = true;
};

const confirmCancel = (appt: AppointmentDetail) => {
  apptToCancel.value = appt;
  showCancelModal.value = true;
};

const confirmCancelFromDetail = (appt: AppointmentDetail) => {
  showDetailModal.value = false;
  apptToCancel.value = appt;
  showCancelModal.value = true;
};

// ===== Helpers =====
const getCountByStatus = (status: string): number => {
  if (status === 'all') return 0;
  return appointments.value.filter(a => a.status === status).length;
};

const canCancel = (status: string | null): boolean => {
  return status === 'pending' || status === 'confirmed';
};

const getStatusLabel = (status: string | null): string => {
  const map: Record<string, string> = {
    pending: 'Chờ xác nhận',
    confirmed: 'Đã xác nhận',
    in_progress: 'Đang khám',
    completed: 'Hoàn thành',
    cancelled: 'Đã huỷ',
  };
  return map[status ?? ''] || (status ?? 'Không rõ');
};

const getStatusIcon = (status: string | null): string => {
  const map: Record<string, string> = {
    pending: 'bi bi-hourglass-split',
    confirmed: 'bi bi-check-circle-fill',
    in_progress: 'bi bi-activity',
    completed: 'bi bi-check2-all',
    cancelled: 'bi bi-x-circle-fill',
  };
  return map[status ?? ''] || 'bi bi-question-circle';
};

const getInvoiceStatusLabel = (status: string | null | undefined): string => {
  const map: Record<string, string> = {
    unpaid: 'Chưa thanh toán',
    paid: 'Đã thanh toán',
    cancelled: 'Đã huỷ HĐ',
  };
  return map[status ?? ''] || (status ?? '');
};

const getSpeciesEmoji = (species: string | null): string => {
  const map: Record<string, string> = {
    'Chó': '🐕', 'Mèo': '🐈', 'Thỏ': '🐇', 'Chim': '🦜', 'Cá': '🐟', 'Bò sát': '🦎',
  };
  return map[species ?? ''] || '🐾';
};

const getSelectedPetName = (): string => {
  const pet = myPets.value.find(p => p.id === bookForm.value.petId);
  return pet ? `${getSpeciesEmoji(pet.species)} ${pet.name}` : '—';
};

const getSelectedServiceName = (): string => {
  const svc = services.value.find(s => s.id === bookForm.value.serviceId);
  return svc?.name ?? '—';
};

const formatDay = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).getDate().toString().padStart(2, '0');
};

const formatMonthYear = (dateStr: string): string => {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return `Th${d.getMonth() + 1}/${d.getFullYear()}`;
};

const formatTime = (dateStr: string): string => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const formatDateFull = (dateStr: string): string => {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleString('vi-VN', {
    weekday: 'long', day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  });
};

const formatDatetimeLocal = (val: string): string => {
  if (!val) return '—';
  return new Date(val).toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
};

const formatCurrency = (amount: number | null | undefined): string => {
  if (!amount) return '';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

// ===== Lifecycle =====
onMounted(fetchAppointments);
</script>

<style scoped>
/* ===== Layout ===== */
.myappts-tab { padding: 0; }

/* ===== Hero ===== */
.appts-hero {
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  border-radius: 16px;
  padding: 1.5rem 2rem;
  border: 1px solid #bbf7d0;
}

/* ===== Premium Button ===== */
.btn-premium-appt {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
  border: none;
  padding: 0.6rem 1.4rem;
  border-radius: 50px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 15px rgba(16, 185, 129, 0.35);
  display: inline-flex;
  align-items: center;
}

.btn-premium-appt:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(16, 185, 129, 0.45);
}

/* ===== Filter Tabs ===== */
.filter-tabs {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.filter-tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 6px 14px;
  border-radius: 20px;
  border: 1.5px solid #e5e7eb;
  background: white;
  font-size: 0.82rem;
  font-weight: 500;
  color: #6b7280;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.filter-tab-btn:hover {
  border-color: #10b981;
  color: #059669;
}

.filter-tab-btn.active {
  background: #10b981;
  border-color: #10b981;
  color: white;
  font-weight: 700;
}

.tab-count {
  background: rgba(255,255,255,0.3);
  border-radius: 10px;
  padding: 1px 7px;
  font-size: 0.75rem;
  font-weight: 700;
  margin-left: 2px;
}

.filter-tab-btn:not(.active) .tab-count {
  background: #f3f4f6;
  color: #374151;
}

/* ===== Empty State ===== */
.empty-state-appt {
  background: white;
  border-radius: 20px;
  padding: 4rem 2rem;
  text-align: center;
  box-shadow: 0 4px 20px rgba(0,0,0,0.06);
  border: 2px dashed #bbf7d0;
}
.empty-icon { font-size: 5rem; }

/* ===== Appointment Cards ===== */
.appts-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.appt-card {
  background: white;
  border-radius: 16px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.07);
  display: flex;
  overflow: hidden;
  transition: all 0.25s ease;
  border: 1px solid #f0f0f0;
}

.appt-card:hover {
  transform: translateX(4px);
  box-shadow: 0 6px 20px rgba(0,0,0,0.1);
}

/* Status colored left bar */
.appt-accent-bar {
  width: 5px;
  flex-shrink: 0;
}

.status-pending .appt-accent-bar { background: linear-gradient(180deg, #f59e0b, #d97706); }
.status-confirmed .appt-accent-bar { background: linear-gradient(180deg, #3b82f6, #1d4ed8); }
.status-in_progress .appt-accent-bar { background: linear-gradient(180deg, #8b5cf6, #6d28d9); }
.status-completed .appt-accent-bar { background: linear-gradient(180deg, #10b981, #059669); }
.status-cancelled .appt-accent-bar { background: linear-gradient(180deg, #6b7280, #4b5563); }

.appt-card-content {
  flex: 1;
  padding: 1.1rem 1.25rem;
}

/* Top row */
.appt-top-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 0.9rem;
  gap: 1rem;
}

.appt-date-block {
  display: flex;
  align-items: baseline;
  gap: 6px;
  flex-wrap: wrap;
}

.appt-date-day {
  font-size: 1.6rem;
  font-weight: 800;
  color: #1a1a2e;
  line-height: 1;
}

.appt-date-rest {
  font-size: 0.82rem;
  font-weight: 600;
  color: #6b7280;
}

.appt-date-time {
  font-size: 0.78rem;
  color: #9ca3af;
  width: 100%;
  margin-top: 2px;
}

/* Status badge */
.appt-status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.78rem;
  font-weight: 700;
  white-space: nowrap;
}

.badge-pending { background: #fef3c7; color: #92400e; }
.badge-confirmed { background: #dbeafe; color: #1e40af; }
.badge-in_progress { background: #ede9fe; color: #5b21b6; }
.badge-completed { background: #d1fae5; color: #065f46; }
.badge-cancelled { background: #f3f4f6; color: #4b5563; }

.invoice-badge {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 0.72rem;
  font-weight: 600;
  background: #fffbeb;
  color: #92400e;
  border: 1px solid #fde68a;
}

/* Info grid */
.appt-info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 8px;
  margin-bottom: 0.75rem;
}

.appt-info-item {
  display: flex;
  flex-direction: column;
}

.appt-info-label {
  font-size: 0.7rem;
  color: #9ca3af;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.appt-info-value {
  font-size: 0.88rem;
  font-weight: 600;
  color: #374151;
}

.pet-inline-badge {
  background: #fef3c7;
  color: #92400e;
  padding: 1px 8px;
  border-radius: 10px;
  font-size: 0.8rem;
}

.appt-symptom {
  font-weight: 400;
  color: #6b7280;
  font-size: 0.85rem;
}

/* Note */
.appt-note {
  font-size: 0.8rem;
  color: #6b7280;
  background: #f9fafb;
  border-radius: 8px;
  padding: 6px 10px;
  margin-bottom: 0.75rem;
}

/* Actions */
.appt-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.btn-appt-detail {
  background: transparent;
  border: 1.5px solid #e5e7eb;
  border-radius: 20px;
  padding: 5px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  color: #4b5563;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-appt-detail:hover {
  border-color: #10b981;
  color: #059669;
  background: #f0fdf4;
}

.btn-appt-cancel {
  background: transparent;
  border: 1.5px solid #fee2e2;
  border-radius: 20px;
  padding: 5px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  color: #dc2626;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-appt-cancel:hover {
  background: #fee2e2;
}

.appt-price-tag {
  margin-left: auto;
  font-size: 0.9rem;
  font-weight: 700;
  color: #059669;
  background: #d1fae5;
  padding: 4px 12px;
  border-radius: 20px;
}

/* ===== Cards animation ===== */
.appt-card-enter-active, .appt-card-leave-active { transition: all 0.35s ease; }
.appt-card-enter-from { opacity: 0; transform: translateX(-20px); }
.appt-card-leave-to { opacity: 0; transform: translateX(20px); }

/* ===== Modal ===== */
.appt-modal-overlay {
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

.appt-modal-card {
  background: white;
  border-radius: 20px;
  width: 100%;
  max-width: 640px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 25px 60px rgba(0,0,0,0.2);
}

.appt-modal-header {
  padding: 1.25rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f0f0f0;
  position: sticky;
  top: 0;
  background: white;
  z-index: 1;
  border-radius: 20px 20px 0 0;
}

.appt-modal-body { padding: 1.5rem; }

.modal-close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 1.1rem;
  color: #6b7280;
  padding: 0.25rem 0.5rem;
  border-radius: 6px;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: #f3f4f6;
  color: #111;
}

/* Booking Steps */
.booking-steps {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  gap: 0;
}

.step-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
  flex: 1;
}

.step-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: #f3f4f6;
  color: #9ca3af;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.85rem;
  transition: all 0.3s;
  z-index: 1;
  border: 2px solid #e5e7eb;
}

.step-item.active .step-circle {
  background: #10b981;
  color: white;
  border-color: #10b981;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
}

.step-item.done .step-circle {
  background: #d1fae5;
  color: #059669;
  border-color: #10b981;
}

.step-label {
  font-size: 0.7rem;
  font-weight: 600;
  color: #9ca3af;
  margin-top: 6px;
  text-align: center;
  white-space: nowrap;
}

.step-item.active .step-label, .step-item.done .step-label {
  color: #059669;
}

.step-line {
  position: absolute;
  top: 18px;
  left: 50%;
  width: 100%;
  height: 2px;
  background: #e5e7eb;
  z-index: 0;
}

.step-item.done .step-line {
  background: #10b981;
}

/* Pet select grid */
.pet-select-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(130px, 1fr));
  gap: 10px;
}

.pet-select-card {
  border: 2px solid #e5e7eb;
  border-radius: 14px;
  padding: 1rem 0.75rem;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
  background: #fafafa;
}

.pet-select-card:hover {
  border-color: #10b981;
  background: #f0fdf4;
}

.pet-select-card.selected {
  border-color: #10b981;
  background: #f0fdf4;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
}

.pet-select-emoji { font-size: 2.5rem; margin-bottom: 4px; }
.pet-select-name { font-weight: 700; font-size: 0.88rem; color: #1a1a2e; }
.pet-select-species { font-size: 0.72rem; }
.pet-select-check {
  position: absolute;
  top: 6px;
  right: 6px;
  font-size: 1.1rem;
}

/* Form controls */
.form-label-custom {
  font-size: 0.82rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 5px;
  display: block;
}

.form-control-custom {
  width: 100%;
  padding: 0.55rem 0.9rem;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.9rem;
  outline: none;
  transition: border-color 0.2s;
  background: #fafafa;
}

.form-control-custom:focus {
  border-color: #10b981;
  box-shadow: 0 0 0 3px rgba(16, 185, 129, 0.15);
  background: white;
}

textarea.form-control-custom { resize: vertical; min-height: 80px; }

/* Booking confirm card */
.booking-confirm-card {
  background: #f9fafb;
  border-radius: 14px;
  padding: 1.25rem;
  border: 1px solid #e5e7eb;
}

.confirm-row {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.confirm-row:last-child { border-bottom: none; }

.confirm-label {
  font-size: 0.8rem;
  color: #9ca3af;
  font-weight: 600;
}

.confirm-value {
  font-size: 0.88rem;
  font-weight: 600;
  color: #374151;
  max-width: 60%;
  text-align: right;
}

/* Navigation buttons */
.booking-nav-btns {
  display: flex;
  align-items: center;
  gap: 10px;
  padding-top: 1rem;
  border-top: 1px solid #f0f0f0;
}

/* Detail modal items */
.detail-item {
  background: #f9fafb;
  border-radius: 10px;
  padding: 10px 14px;
  border: 1px solid #f0f0f0;
}

.detail-label {
  font-size: 0.72rem;
  font-weight: 600;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.4px;
  margin-bottom: 3px;
}

.detail-value {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1f2937;
}

.item-success { background: #f0fdf4; border-color: #bbf7d0; }
.item-warning { background: #fffbeb; border-color: #fde68a; }

/* Modal transition */
.modal-fade-enter-active, .modal-fade-leave-active { transition: all 0.3s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; transform: scale(0.95); }
</style>
