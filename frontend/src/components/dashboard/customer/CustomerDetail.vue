<template>
  <div class="card border-0 shadow-sm rounded-4 p-4 bg-white animate-fade-in">
    <!-- Back Button -->
    <div class="d-flex align-items-center gap-2 mb-4">
      <button class="btn btn-light rounded-circle p-2 border shadow-sm" @click="$emit('back')">
        <i class="bi bi-arrow-left fs-5"></i>
      </button>
      <span class="fw-bold text-muted">Quay lại danh sách khách hàng</span>
    </div>

    <!-- Loading Details state -->
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-warning mb-3"></div>
      <div>Đang tải hồ sơ bệnh án khách hàng...</div>
    </div>

    <!-- Details Content -->
    <!-- Layout: 3/4 and 1/4 -->
    <div v-else-if="detailData" class="row g-4">
      <!-- Sidebar: Customer Info -->
      <div class="col-lg-4">
        <div class="card border-0 shadow-sm bg-light rounded-4 p-4 text-center position-sticky" style="top: 1.5rem;">
          <div class="avatar-circle-gold fs-2 mx-auto mb-3" style="width: 100px; height: 100px;">
            {{ getAvatarLetters(detailData.customer.fullName) }}
          </div>
          <h4 class="fw-bold text-dark mb-1">{{ detailData.customer.fullName }}</h4>
          <span class="badge bg-warning text-dark px-3 py-1.5 rounded-pill fw-bold text-uppercase small shadow-sm mb-4">
            Khách hàng thân thiết
          </span>

          <div class="text-start mt-3 border-top pt-3">
            <div class="mb-3">
              <label class="text-muted small d-block mb-1">Mã khách hàng</label>
              <strong class="text-dark"><i class="bi bi-upc-scan text-warning me-1"></i>{{ detailData.customer.customerCode || '—' }}</strong>
            </div>
            <div class="mb-3">
              <label class="text-muted small d-block mb-1">Số điện thoại</label>
              <strong class="text-dark"><i class="bi bi-telephone text-warning me-1"></i>{{ detailData.customer.phone || 'Chưa cung cấp' }}</strong>
            </div>
            <div class="mb-3">
              <label class="text-muted small d-block mb-1">Địa chỉ Email</label>
              <strong class="text-dark"><i class="bi bi-envelope text-warning me-1"></i>{{ detailData.customer.email || 'Chưa cung cấp' }}</strong>
            </div>
            <div class="mb-3">
              <label class="text-muted small d-block mb-1">Địa chỉ liên hệ</label>
              <strong class="text-dark"><i class="bi bi-geo-alt text-warning me-1"></i>{{ detailData.customer.address || 'Chưa cung cấp' }}</strong>
            </div>
            <div class="mb-0">
              <label class="text-muted small d-block mb-1">Ngày tham gia hệ thống</label>
              <strong class="text-dark"><i class="bi bi-calendar-check text-warning me-1"></i>{{ formatDate(detailData.customer.createdAt) }}</strong>
            </div>
          </div>
        </div>
      </div>

      <!-- Customer Right panel (Stats + Pets + Medical Timeline) -->
      <div class="col-lg-8">
        <!-- KPI Widgets Grid -->
        <div class="row g-3 mb-4">
          <div class="col-sm-6 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 p-3 bg-primary bg-opacity-10 text-primary h-100">
              <div class="d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Số ca khám</h6>
                  <h4 class="fw-extrabold mb-0">{{ detailData.totalVisits }}</h4>
                </div>
                <i class="bi bi-stethoscope fs-2 opacity-50"></i>
              </div>
            </div>
          </div>
          <div class="col-sm-6 col-md-6">
            <div class="card border-0 shadow-sm rounded-4 p-3 bg-success bg-opacity-10 text-success h-100">
              <div class="d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Đã chi tiêu</h6>
                  <h4 class="fw-extrabold mb-0">{{ formatCurrency(detailData.totalSpent) }}</h4>
                </div>
                <i class="bi bi-wallet2 fs-2 opacity-50"></i>
              </div>
            </div>
          </div>
        </div>

        <!-- Pets Management block -->
        <div class="card border-0 shadow-sm rounded-4 p-4 mb-4 bg-light">
          <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="fw-bold text-dark mb-0"><i class="bi bi-heptagon-fill text-warning me-2"></i>Thú cưng đăng ký</h5>
            <button class="btn btn-sm btn-premium rounded-pill px-3" @click="$emit('add-pet', customerId)">
              <i class="bi bi-plus-lg me-1"></i> Đăng ký thêm bé
            </button>
          </div>

          <div class="row g-3">
            <div v-if="detailData.pets.length === 0" class="col-12 text-center text-muted py-4">
              Chưa có thú cưng nào được đăng ký cho chủ này.
            </div>
            <div v-for="pet in detailData.pets" :key="pet.id" class="col-md-6">
              <div class="card border-0 rounded-4 shadow-sm p-3 bg-white h-100 position-relative">
                <div class="position-absolute top-0 end-0 p-3 d-flex gap-2">
                  <button class="btn btn-sm btn-outline-info rounded-pill shadow-sm fw-bold" @click="$emit('view-pet-history', pet)" title="Hồ sơ y tế toàn diện">
                    <i class="bi bi-file-medical me-1"></i> Hồ sơ y tế
                  </button>
                  <button class="btn btn-sm btn-light rounded-circle shadow-sm text-primary" @click="$emit('edit-pet', pet)" title="Sửa thông tin">
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                </div>
                <div class="d-flex align-items-center gap-3">
                  <span class="fs-1 bg-light p-2 rounded-circle d-inline-block">{{ getAnimalEmoji(pet.species) }}</span>
                  <div>
                    <h6 class="fw-bold text-dark mb-1">{{ pet.name }}</h6>
                    <p class="text-muted small mb-0">{{ pet.breed || pet.species }} | {{ getGenderText(pet.gender) }}</p>
                    <small class="text-muted">Cân nặng: {{ pet.weight ? pet.weight + ' kg' : '—' }}</small>
                  </div>
                </div>
                <div v-if="pet.allergyNote" class="mt-3 p-2 bg-danger bg-opacity-10 text-danger rounded small fw-bold">
                  <i class="bi bi-exclamation-triangle-fill me-1"></i> Dị ứng: {{ pet.allergyNote }}
                </div>
                <div class="d-flex justify-content-end align-items-center mt-3 pt-2 border-top small text-muted">
                  <span class="badge rounded-pill" :class="pet.sterilized ? 'bg-success' : 'bg-secondary'">
                    {{ pet.sterilized ? 'Đã triệt sản' : 'Chưa triệt sản' }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Appointments / Clinical records timeline -->
        <div class="card border-0 shadow-sm rounded-4 p-4 bg-light">
          <h5 class="fw-bold text-dark mb-3"><i class="bi bi-clock-history text-warning me-2"></i>Lịch sử khám & Điều trị</h5>
          <div class="table-responsive">
            <table class="table table-hover align-middle bg-white rounded-4 overflow-hidden mb-0">
              <thead class="table-light">
                <tr>
                  <th>Thời gian</th>
                  <th>Thú cưng</th>
                  <th>Dịch vụ</th>
                  <th>Bác sĩ</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="detailData.appointments.length === 0" class="text-center text-muted">
                  <td colspan="5" class="py-4">Chưa có lịch sử khám bệnh nào ghi nhận.</td>
                </tr>
                <tr v-for="appt in paginatedAppointments" :key="appt.id">
                  <td class="small text-muted">{{ formatDateFull(appt.appointmentDate) }}</td>
                  <td class="fw-bold text-dark">{{ appt.petName }}</td>
                  <td><span class="badge bg-success bg-opacity-10 text-success rounded px-2.5 py-1 fw-bold">{{ appt.serviceName }}</span></td>
                  <td class="small">{{ appt.doctorName }}</td>
                  <td>
                    <span class="badge rounded-pill" :class="getStatusBadgeClass(appt.status)">
                      {{ getStatusLabel(appt.status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          
          <!-- Pagination -->
          <div v-if="totalPages > 1" class="d-flex justify-content-between align-items-center mt-3">
            <span class="text-muted small">
              Hiển thị {{ (currentPage - 1) * itemsPerPage + 1 }} - {{ Math.min(currentPage * itemsPerPage, detailData.appointments.length) }} trong số {{ detailData.appointments.length }} ca khám
            </span>
            <div class="btn-group">
              <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === 1" @click="prevPage">
                <i class="bi bi-chevron-left"></i> Trước
              </button>
              <button 
                v-for="page in totalPages" 
                :key="page" 
                class="btn btn-sm" 
                :class="page === currentPage ? 'btn-secondary text-white' : 'btn-outline-secondary'"
                @click="currentPage = page"
              >
                {{ page }}
              </button>
              <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === totalPages" @click="nextPage">
                Sau <i class="bi bi-chevron-right"></i>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import api from '../../../services/api';

const props = defineProps<{
  customerId: string;
}>();

const emit = defineEmits(['back', 'add-pet', 'edit-pet', 'view-pet-history']);

const loading = ref(false);
const detailData = ref<any>(null);

// Pagination
const currentPage = ref(1);
const itemsPerPage = 5;

const paginatedAppointments = computed(() => {
  if (!detailData.value || !detailData.value.appointments) return [];
  const start = (currentPage.value - 1) * itemsPerPage;
  const end = start + itemsPerPage;
  return detailData.value.appointments.slice(start, end);
});

const totalPages = computed(() => {
  if (!detailData.value || !detailData.value.appointments) return 0;
  return Math.ceil(detailData.value.appointments.length / itemsPerPage);
});

const nextPage = () => {
  if (currentPage.value < totalPages.value) currentPage.value++;
};

const prevPage = () => {
  if (currentPage.value > 1) currentPage.value--;
};

const fetchCustomerDetail = async () => {
  if (!props.customerId) return;
  loading.value = true;
  detailData.value = null;
  currentPage.value = 1;
  try {
    const res = await api.get(`/receptionist/customers/${props.customerId}`);
    detailData.value = res.data;
  } catch (err) {
    console.error(err);
    alert('Không thể tải hồ sơ chi tiết khách hàng.');
    emit('back');
  } finally {
    loading.value = false;
  }
};

watch(() => props.customerId, fetchCustomerDetail);
onMounted(fetchCustomerDetail);

// UI Helpers
const getAvatarLetters = (name: string) => {
  if (!name) return '?';
  const parts = name.trim().split(/\s+/);
  if (parts.length >= 2) {
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }
  return name.slice(0, 1).toUpperCase();
};

const getAnimalEmoji = (species: string) => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const getGenderText = (g: number) => {
  if (g === 1) return 'Đực';
  if (g === 2) return 'Cái';
  return 'Khác';
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatDateFull = (dateStr: string) => {
  if (!dateStr) return '—';
  const d = new Date(dateStr);
  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

const formatCurrency = (val: number) => {
  if (val === undefined || val === null) return '0đ';
  return val.toLocaleString('vi-VN') + 'đ';
};

const getStatusBadgeClass = (status: string) => {
  const s = (status || '').toLowerCase();
  if (s === 'completed') return 'bg-success text-white';
  if (s === 'ready_to_pay') return 'bg-info text-dark';
  if (s === 'in_progress') return 'bg-warning text-dark';
  if (s === 'cancelled') return 'bg-danger text-white';
  return 'bg-secondary text-white';
};

const getStatusLabel = (status: string) => {
  const s = (status || '').toLowerCase();
  if (s === 'completed') return 'Hoàn thành';
  if (s === 'ready_to_pay') return 'Chờ thanh toán';
  if (s === 'in_progress') return 'Đang khám';
  if (s === 'cancelled') return 'Đã hủy';
  if (s === 'waiting') return 'Đang chờ';
  return status;
};

// Expose a method to refresh data (called by parent when pet is added/edited)
defineExpose({
  refresh: fetchCustomerDetail
});
</script>

<style scoped>
.avatar-circle-gold {
  border-radius: 50%;
  background: linear-gradient(135deg, var(--primary-gold, #f59e0b), #d97706);
  color: white;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}
.animate-fade-in {
  animation: fadeIn 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
