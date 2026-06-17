<template>
  <div class="qr-checkin-page min-vh-100 d-flex flex-column" style="background-color: #f8fafc;">
    <!-- Loading State -->
    <div v-if="loading" class="d-flex justify-content-center align-items-center flex-grow-1">
      <div class="spinner-border text-primary" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="container mt-5 text-center">
      <div class="alert alert-danger d-inline-block shadow-sm">
        <i class="bi bi-exclamation-triangle-fill me-2"></i> {{ error }}
      </div>
      <div class="mt-3">
        <button class="btn btn-outline-secondary" @click="goBack">
          <i class="bi bi-arrow-left me-2"></i>Quay lại Lịch hẹn
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div v-else-if="appointment" class="container py-4 flex-grow-1 d-flex flex-column justify-content-center">
      
      <!-- Top Actions (Hide when printing) -->
      <div class="d-flex justify-content-center align-items-center mb-5 no-print position-relative">
        <h4 class="fw-bold text-dark mb-0">Chi tiết QR Check-in</h4>
      </div>

      <div class="row g-4 align-items-stretch justify-content-center">
        
        <!-- Left Column: Info -->
        <div class="col-lg-3 col-md-6 order-2 order-lg-1 d-flex flex-column gap-3">
          <div class="card border rounded-4 flex-grow-1 info-card" style="border-color: #e2e8f0 !important;">
            <div class="card-body p-4">
              <h6 class="text-muted small fw-bold mb-4 text-uppercase tracking-wider">Thông tin buổi khám</h6>
              
              <div class="info-item mb-4 d-flex">
                <div class="icon-box bg-primary bg-opacity-10 text-primary rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;">
                  <i class="bi bi-heptagon-fill"></i>
                </div>
                <div>
                  <div class="text-muted small">Thú cưng</div>
                  <div class="fw-bold text-dark">{{ appointment.petName }} <span class="fw-normal text-secondary">({{ appointment.species }})</span></div>
                </div>
              </div>

              <div class="info-item mb-4 d-flex">
                <div class="icon-box bg-success bg-opacity-10 text-success rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;">
                  <i class="bi bi-bag-plus-fill"></i>
                </div>
                <div>
                  <div class="text-muted small">Dịch vụ</div>
                  <div class="fw-bold text-dark">{{ appointment.serviceName }}</div>
                </div>
              </div>

              <div class="info-item mb-4 d-flex">
                <div class="icon-box bg-warning bg-opacity-10 text-warning rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;">
                  <i class="bi bi-clock-fill"></i>
                </div>
                <div>
                  <div class="text-muted small">Thời gian</div>
                  <div class="fw-bold text-dark">{{ formatTime(appointment.appointmentDate) }}, {{ formatDate(appointment.appointmentDate) }}</div>
                </div>
              </div>

              <div class="info-item d-flex">
                <div class="icon-box bg-info bg-opacity-10 text-info rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;">
                  <i class="bi bi-person-badge-fill"></i>
                </div>
                <div>
                  <div class="text-muted small">Bác sĩ phụ trách</div>
                  <div class="fw-bold text-dark">{{ appointment.doctorName || 'Chưa phân công' }}</div>
                </div>
              </div>

            </div>
          </div>
          
          <button class="btn btn-link text-decoration-none fw-bold text-dark d-flex align-items-center" @click="goBack">
            <i class="bi bi-arrow-left me-2"></i> Về Lịch Hẹn
          </button>
        </div>

        <!-- Middle Column: Phone Mockup & Actions -->
        <div class="col-lg-5 col-md-8 order-1 order-lg-2">
          <div class="card border rounded-4 h-100 qr-center-card" style="border-color: #e2e8f0 !important;">
            <div class="card-body p-5 d-flex flex-column align-items-center text-center justify-content-center">
              
              <div class="mb-4" id="qr-export-area">
                <div v-if="appointment.qrToken" class="qr-container bg-white p-3 rounded-4 shadow-sm border" style="display: inline-block;">
                  <qrcode-vue :value="appointment.qrToken" :size="200" level="M" :margin="3" />
                </div>
                <div v-else class="text-danger">Lỗi: Không tìm thấy mã QR.</div>
              </div>

              <div class="text-muted small fw-bold mb-1 text-uppercase">Mã định danh buổi khám</div>
              <div class="fs-4 fw-bolder text-primary font-monospace mb-4">{{ appointment.qrToken }}</div>

              <div class="w-100 px-4 no-print d-flex flex-column gap-2">
                <button class="btn btn-primary w-100 rounded-3 fw-bold py-2 shadow-sm" style="background-color: #034694; border: none;" @click="downloadQr">
                  <i class="bi bi-download me-2"></i> Tải xuống mã QR
                </button>
                <button class="btn w-100 rounded-3 fw-bold py-2" style="background-color: #e2e8f0; color: #0f172a; border: none;" @click="printQr">
                  <i class="bi bi-printer me-2"></i> In thẻ Check-in
                </button>
              </div>

            </div>
          </div>
        </div>

        <!-- Right Column: Guide -->
        <div class="col-lg-4 col-md-12 order-3 order-lg-3">
          <div class="d-flex flex-column h-100 gap-3">
            
            <div class="card border rounded-4 flex-grow-1 guide-card" style="border-color: #e2e8f0 !important;">
              <div class="card-body p-4">
                <h6 class="text-muted small fw-bold mb-4 text-uppercase tracking-wider">Hướng dẫn sử dụng</h6>
                
                <div class="d-flex mb-4 position-relative step-item">
                  <div class="step-number text-white rounded-circle fw-bold d-flex align-items-center justify-content-center me-3 z-1" style="background-color: #1e40af;">1</div>
                  <div>
                    <div class="fw-bold text-dark mb-1">Đến phòng khám</div>
                    <div class="text-muted small">Vui lòng có mặt tại sảnh MyPet Clinic ít nhất 10 phút trước giờ hẹn.</div>
                  </div>
                </div>

                <div class="d-flex mb-4 position-relative step-item">
                  <div class="step-number text-white rounded-circle fw-bold d-flex align-items-center justify-content-center me-3 z-1" style="background-color: #1e40af;">2</div>
                  <div>
                    <div class="fw-bold text-dark mb-1">Quét mã tại Kiosk</div>
                    <div class="text-muted small">Đưa mã QR này vào vùng nhận diện của máy Kiosk tự động hoặc cho Lễ tân xem.</div>
                  </div>
                </div>

                <div class="d-flex mb-4 position-relative step-item">
                  <div class="step-number text-white rounded-circle fw-bold d-flex align-items-center justify-content-center me-3 z-1" style="background-color: #1e40af;">3</div>
                  <div>
                    <div class="fw-bold text-dark mb-1">Nhận số thứ tự</div>
                    <div class="text-muted small">Hệ thống sẽ ghi nhận và thông báo cho bác sĩ phụ trách ngay lập tức.</div>
                  </div>
                </div>

                <div class="alert border-0 rounded-3 d-flex align-items-center p-3 mt-4 mb-0 no-print" style="background-color: #fef3c7;">
                  <i class="bi bi-info-circle-fill fs-5 me-3" style="color: #d97706;"></i>
                  <div class="small text-dark">
                    <strong>Cần hỗ trợ?</strong><br>Vui lòng liên hệ quầy lễ tân hoặc gọi Hotline <strong>1900-PETS</strong>.
                  </div>
                </div>
              </div>
            </div>

            <div class="card border rounded-4 clinic-banner overflow-hidden position-relative no-print" style="min-height: 140px; border-color: #e2e8f0 !important;">
              <img src="/clinic-banner.png" class="w-100 h-100 object-fit-cover position-absolute top-0 start-0" alt="Clinic" style="opacity: 0.9;" />
              <div class="position-absolute bottom-0 w-100 p-3 bg-gradient-dark text-white">
                <div class="fw-bold small shadow-text">Chi nhánh: MyPet Clinic Quận 1</div>
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
import { useRoute, useRouter } from 'vue-router';
import api from '../services/api';
import QrcodeVue from 'qrcode.vue';

const route = useRoute();
const router = useRouter();

const loading = ref(true);
const error = ref('');
const appointment = ref<any>(null);

const loadAppointment = async () => {
  const id = route.params.id;
  if (!id) {
    error.value = "Không tìm thấy mã lịch hẹn.";
    loading.value = false;
    return;
  }

  try {
    const res = await api.get(`/my-appointments/${id}`);
    appointment.value = res.data;
    
    // Safety check - only confirmed appointments should have QR shown usually, 
    // but since we arrived here, we just display it if it exists.
    if (!appointment.value.qrToken) {
      error.value = "Lịch hẹn này chưa được cấp Mã QR.";
    }
  } catch (err: any) {
    console.error('Lỗi khi tải lịch hẹn:', err);
    error.value = "Đã xảy ra lỗi khi tải dữ liệu lịch hẹn.";
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  loadAppointment();
});

const goBack = () => {
  router.push('/dashboard');
};

const printQr = () => {
  window.print();
};

const downloadQr = () => {
  const canvas = document.querySelector('.qr-container canvas') as HTMLCanvasElement;
  if (canvas) {
    const link = document.createElement('a');
    link.download = `QR-CheckIn-${appointment.value.qrToken}.png`;
    link.href = canvas.toDataURL('image/png');
    link.click();
  } else {
    alert("Không tìm thấy mã QR để tải xuống.");
  }
};

// Utils
const formatDate = (dateString: string) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatTime = (dateString: string) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};
</script>

<style scoped>
.qr-checkin-page {
  font-family: 'Inter', sans-serif;
}



.qr-container {
  transition: transform 0.3s ease;
}
.qr-container:hover {
  transform: scale(1.02);
}

/* Icons and Steps */
.icon-box {
  width: 48px;
  height: 48px;
  font-size: 1.25rem;
  flex-shrink: 0;
}

.step-number {
  width: 32px;
  height: 32px;
  font-size: 1rem;
  flex-shrink: 0;
}

.step-line {
  background-color: #e2e8f0 !important;
}

.bg-gradient-dark {
  background: linear-gradient(to top, rgba(0,0,0,0.8) 0%, rgba(0,0,0,0) 100%);
}
.shadow-text {
  text-shadow: 0 2px 4px rgba(0,0,0,0.5);
}

/* Tracking */
.tracking-wider {
  letter-spacing: 0.05em;
}

/* Print Styles */
@media print {
  body * {
    visibility: hidden;
  }
  .qr-checkin-page {
    background-color: white !important;
  }
  .qr-center-card, .qr-center-card * {
    visibility: visible;
  }
  .qr-center-card {
    position: absolute;
    left: 50%;
    top: 50%;
    transform: translate(-50%, -50%);
    width: 100%;
    max-width: 500px;
    box-shadow: none !important;
    border: none !important;
  }
  .no-print, .phone-mockup {
    display: none !important;
  }
  /* Show just the QR outside the mockup when printing */
  .qr-center-card .card-body::before {
    content: '';
    display: block;
    clear: both;
  }
}
</style>
