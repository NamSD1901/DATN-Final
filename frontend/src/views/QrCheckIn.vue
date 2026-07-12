<template>
  <div class="qr-checkin-page min-vh-100 d-flex flex-column bg-light-mesh">
    <!-- Loading State -->
    <div v-if="loading" class="d-flex justify-content-center align-items-center flex-grow-1">
      <div class="spinner-border text-warning" role="status" style="width: 3rem; height: 3rem;">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="container mt-5 text-center">
      <div class="alert alert-danger d-inline-block shadow-sm">
        <i class="bi bi-exclamation-triangle-fill me-2"></i> {{ error }}
      </div>
      <div class="mt-3">
        <button class="btn btn-outline-secondary rounded-pill" @click="goBack">
          <i class="bi bi-arrow-left me-2"></i>Quay lại Lịch hẹn
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div v-else-if="appointment" class="container py-4 flex-grow-1 d-flex flex-column justify-content-center">
      
      <!-- Top Actions (Hide when printing) -->
      <div class="d-flex justify-content-center align-items-center mb-4 no-print position-relative">
        <h3 class="fw-bold text-dark mb-0 d-flex align-items-center gap-2">
          <i class="bi bi-qr-code-scan text-warning"></i> Thẻ Check-in Thông Minh
        </h3>
      </div>

      <div class="row g-4 align-items-stretch justify-content-center">
        
        <!-- Left Column: Info -->
        <div class="col-lg-3 col-md-6 order-2 order-lg-1 d-flex flex-column gap-3">
          <div class="card glass-panel border-glass rounded-4 flex-grow-1 info-card shadow-sm">
            <div class="card-body p-4">
              <h6 class="text-muted small fw-bold mb-4 text-uppercase tracking-wider">Thông tin buổi khám</h6>
              
              <div class="info-item mb-4 d-flex">
                <div class="icon-box bg-warning bg-opacity-10 text-warning rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;">
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
                <div class="icon-box bg-danger bg-opacity-10 text-danger rounded-circle me-3 d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;">
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
          
          <button class="btn btn-outline-glass rounded-pill fw-bold text-dark d-flex align-items-center justify-content-center w-100" @click="goBack">
            <i class="bi bi-arrow-left me-2"></i> Trở Về Lịch Hẹn
          </button>
        </div>

        <!-- Middle Column: QR Code & Actions -->
        <div class="col-lg-5 col-md-8 order-1 order-lg-2">
          <div class="card glass-panel border-glass rounded-4 h-100 qr-center-card shadow-sm position-relative overflow-hidden">
            <!-- Decorative gradient element -->
            <div class="position-absolute top-0 start-50 translate-middle-x rounded-circle" style="width: 250px; height: 250px; background: radial-gradient(circle, rgba(245,158,11,0.15) 0%, rgba(255,255,255,0) 70%); z-index: 0;"></div>
            
            <div class="card-body p-5 d-flex flex-column align-items-center text-center justify-content-center position-relative z-1">
              
              <div class="mb-4" id="qr-export-area">
                <div v-if="appointment.qrToken" class="qr-container bg-white p-3 rounded-4 shadow border-warning-subtle border" style="display: inline-block;">
                  <qrcode-vue :value="appointment.qrToken" :size="220" level="M" :margin="2" />
                </div>
                <div v-else class="text-danger">Lỗi: Không tìm thấy mã QR.</div>
              </div>

              <div class="text-muted small fw-bold mb-1 text-uppercase tracking-wider">Mã định danh buổi khám</div>
              <div class="fs-4 fw-bolder text-warning font-monospace mb-4 d-inline-block px-3 py-1 rounded bg-warning bg-opacity-10 border border-warning border-opacity-25">{{ appointment.qrToken }}</div>

              <div class="w-100 px-4 no-print d-flex flex-column gap-2 mt-auto">
                <button class="btn btn-premium-neon w-100 rounded-pill fw-bold py-2 shadow-sm" @click="downloadQr">
                  <i class="bi bi-download me-2"></i> Tải xuống mã QR
                </button>
              </div>

            </div>
          </div>
        </div>

        <!-- Right Column: Guide -->
        <div class="col-lg-4 col-md-12 order-3 order-lg-3">
          <div class="d-flex flex-column h-100 gap-3">
            
            <div class="card glass-panel border-glass rounded-4 flex-grow-1 guide-card shadow-sm">
              <div class="card-body p-4">
                <h6 class="text-muted small fw-bold mb-4 text-uppercase tracking-wider">Hướng dẫn sử dụng</h6>
                
                <div class="d-flex mb-4 position-relative step-item">
                  <div class="step-number text-white rounded-circle fw-bold d-flex align-items-center justify-content-center me-3 z-1 shadow-sm" style="background: linear-gradient(135deg, #f59e0b, #d97706);">1</div>
                  <div>
                    <div class="fw-bold text-dark mb-1">Đến phòng khám</div>
                    <div class="text-muted small">Vui lòng có mặt tại sảnh MyPet Clinic ít nhất 10 phút trước giờ hẹn.</div>
                  </div>
                </div>

                <div class="d-flex mb-4 position-relative step-item">
                  <div class="step-number text-white rounded-circle fw-bold d-flex align-items-center justify-content-center me-3 z-1 shadow-sm" style="background: linear-gradient(135deg, #f59e0b, #d97706);">2</div>
                  <div>
                    <div class="fw-bold text-dark mb-1">Quét mã tại Kiosk</div>
                    <div class="text-muted small">Đưa mã QR này vào vùng nhận diện của máy Kiosk tự động hoặc cho Lễ tân xem.</div>
                  </div>
                </div>

                <div class="d-flex mb-4 position-relative step-item">
                  <div class="step-number text-white rounded-circle fw-bold d-flex align-items-center justify-content-center me-3 z-1 shadow-sm" style="background: linear-gradient(135deg, #f59e0b, #d97706);">3</div>
                  <div>
                    <div class="fw-bold text-dark mb-1">Nhận số thứ tự</div>
                    <div class="text-muted small">Hệ thống sẽ ghi nhận và thông báo cho bác sĩ phụ trách ngay lập tức.</div>
                  </div>
                </div>

                <div class="alert border border-warning border-opacity-25 rounded-3 d-flex align-items-center p-3 mt-4 mb-0 no-print bg-warning bg-opacity-10">
                  <i class="bi bi-info-circle-fill fs-5 me-3 text-warning"></i>
                  <div class="small text-dark">
                    <strong>Cần hỗ trợ?</strong><br>Vui lòng liên hệ quầy lễ tân hoặc gọi Hotline <strong>1900-PETS</strong>.
                  </div>
                </div>
              </div>
            </div>

            <div class="card border rounded-4 clinic-banner overflow-hidden position-relative no-print shadow-sm" style="min-height: 120px; border-color: rgba(245, 158, 11, 0.2) !important;">
              <img src="/clinic-banner.png" class="w-100 h-100 object-fit-cover position-absolute top-0 start-0" alt="Clinic" style="opacity: 0.85;" />
              <div class="position-absolute bottom-0 w-100 p-2 text-white" style="background: linear-gradient(to top, rgba(0,0,0,0.8), transparent);">
                <div class="fw-bold small shadow-text ms-2"><i class="bi bi-geo-alt-fill text-warning me-1"></i> Chi nhánh MyPet Clinic Quận 1</div>
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

.bg-light-mesh {
  background-color: #f8fafc;
  background-image: radial-gradient(rgba(245, 158, 11, 0.05) 1px, transparent 1px);
  background-size: 20px 20px;
}

/* Glassmorphism Classes */
.glass-panel {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
}

.border-glass { 
  border: 1px solid rgba(245, 158, 11, 0.15) !important; 
}

/* Premium Buttons */
.btn-premium-neon {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  border: none;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.btn-premium-neon:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(245, 158, 11, 0.4);
  color: white;
}

.btn-outline-glass {
  background: rgba(255, 255, 255, 0.6);
  color: #1e293b;
  border: 1px solid rgba(217, 119, 6, 0.2);
  transition: all 0.3s ease;
}
.btn-outline-glass:hover {
  background: rgba(255, 255, 255, 0.9);
  border-color: #f59e0b;
  color: #d97706;
}

.qr-container {
  transition: transform 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.qr-container:hover {
  transform: scale(1.05);
  box-shadow: 0 10px 25px rgba(245, 158, 11, 0.2) !important;
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

.shadow-text {
  text-shadow: 0 1px 3px rgba(0,0,0,0.8);
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
    background-image: none !important;
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
    background: transparent !important;
  }
  .no-print {
    display: none !important;
  }
  .qr-center-card .card-body::before {
    content: '';
    display: block;
    clear: both;
  }
}
</style>
