<template>
  <div class="offers-admin-tab animate__animated animate__fadeIn">
    <!-- Header Area -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <h4 class="fw-bold mb-1 text-dark">Quản lý Mã giảm giá (Voucher)</h4>
        <p class="text-muted small mb-0">Thiết lập các chương trình khuyến mãi, flash sale, quà tặng khách hàng</p>
      </div>
      <button class="btn btn-warning fw-bold px-4 py-2 rounded-pill shadow-sm text-white" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill me-2"></i>Tạo Mã Giảm Giá
      </button>
    </div>

    <!-- Filters & Stats -->
    <div class="card border-0 shadow-sm rounded-4 mb-4 bg-white overflow-hidden glassmorphism-card">
      <div class="card-body p-4 d-flex flex-wrap gap-3 align-items-center">
        <div class="search-box position-relative flex-grow-1" style="max-width: 400px;">
          <i class="bi bi-search position-absolute top-50 start-0 translate-middle-y ms-3 text-muted"></i>
          <input type="text" class="form-control rounded-pill ps-5 bg-light border-0 py-2" placeholder="Tìm theo mã code hoặc tên..." v-model="searchQuery" @input="debouncedFetch">
        </div>
        <select class="form-select w-auto border-0 bg-light rounded-pill px-4 py-2" v-model="statusFilter" @change="fetchOffers">
          <option value="">Tất cả trạng thái</option>
          <option value="ACTIVE">Đang hoạt động</option>
          <option value="LOCKED">Đã khóa</option>
          <option value="EXPIRED">Đã hết hạn</option>
        </select>
        <button class="btn btn-light rounded-pill px-4" @click="fetchOffers">
          <i class="bi bi-arrow-clockwise"></i> Làm mới
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="offerStore.loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="!offerStore.loading && offerStore.offers.length === 0" class="text-center py-5 bg-white rounded-4 shadow-sm">
      <i class="bi bi-ticket-perforated text-muted" style="font-size: 4rem; opacity: 0.5;"></i>
      <h5 class="mt-3 text-muted">Chưa có mã giảm giá nào</h5>
      <p class="text-muted small">Bắt đầu tạo mã giảm giá đầu tiên để thu hút khách hàng.</p>
      <button class="btn btn-outline-warning rounded-pill px-4 mt-2" @click="openCreateModal">Tạo Voucher</button>
    </div>

    <!-- Data Grid -->
    <div v-else class="row g-4">
      <div class="col-xl-4 col-md-6" v-for="offer in offerStore.offers" :key="offer.id">
        <div class="card border-0 shadow-sm rounded-4 h-100 overflow-hidden offer-card" :class="{'locked': offer.status !== 'ACTIVE'}">
          <div class="offer-header p-3 text-white d-flex justify-content-between align-items-center"
               :class="offer.status === 'ACTIVE' ? 'bg-gradient-warning' : 'bg-secondary'">
            <div class="badge bg-white text-dark rounded-pill fw-bold fs-6 shadow-sm px-3">
              {{ offer.code }}
            </div>
            <span class="badge" :class="offer.status === 'ACTIVE' ? 'bg-success' : 'bg-dark'">{{ getStatusLabel(offer.status) }}</span>
          </div>
          <div class="card-body p-4 position-relative">
            <h5 class="fw-bold mb-2 text-dark">{{ offer.name }}</h5>
            <p class="text-muted small mb-3 text-truncate-2">{{ offer.description }}</p>
            
            <div class="d-flex align-items-center justify-content-between bg-light p-3 rounded-3 mb-3">
              <div>
                <span class="text-muted small d-block mb-1">Mức giảm</span>
                <span class="fw-bold text-danger fs-5">
                  {{ offer.discountType === 'PERCENTAGE' ? `${offer.discountValue}%` : formatCurrency(offer.discountValue) }}
                </span>
              </div>
              <div class="text-end">
                <span class="text-muted small d-block mb-1">Đơn tối thiểu</span>
                <span class="fw-bold text-dark">{{ formatCurrency(offer.minOrderValue) }}</span>
              </div>
            </div>

            <div class="progress mb-2" style="height: 8px;">
              <div class="progress-bar bg-warning" role="progressbar" 
                   :style="{ width: getUsagePercentage(offer) + '%' }" 
                   :aria-valuenow="getUsagePercentage(offer)" aria-valuemin="0" aria-valuemax="100"></div>
            </div>
            <div class="d-flex justify-content-between text-muted small mb-3">
              <span>Đã dùng: <b>{{ offer.usedQuantity }}</b></span>
              <span>Tổng: <b>{{ offer.totalQuantity || 'Vô hạn' }}</b></span>
            </div>

            <div class="text-muted small mb-3">
              <i class="bi bi-calendar-event me-1"></i> Hạn: {{ formatDate(offer.endDate) }}
            </div>
          </div>
          <div class="card-footer bg-white border-top-0 p-3 d-flex justify-content-end gap-2">
            <button class="btn btn-light btn-sm rounded-pill px-3" @click="openEditModal(offer)">
              <i class="bi bi-pencil-square"></i> Sửa
            </button>
            <button v-if="offer.status === 'ACTIVE'" class="btn btn-outline-danger btn-sm rounded-pill px-3" @click="lockOffer(offer)">
              <i class="bi bi-lock-fill"></i> Khóa
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Pagination -->
    <div class="d-flex justify-content-center mt-4" v-if="offerStore.totalPages > 1">
      <nav aria-label="Page navigation">
        <ul class="pagination pagination-rounded shadow-sm">
          <li class="page-item" :class="{ disabled: offerStore.currentPage === 1 }">
            <a class="page-link border-0" href="#" @click.prevent="changePage(offerStore.currentPage - 1)"><i class="bi bi-chevron-left"></i></a>
          </li>
          <li class="page-item" v-for="page in offerStore.totalPages" :key="page" :class="{ active: offerStore.currentPage === page }">
            <a class="page-link border-0" href="#" @click.prevent="changePage(page)">{{ page }}</a>
          </li>
          <li class="page-item" :class="{ disabled: offerStore.currentPage === offerStore.totalPages }">
            <a class="page-link border-0" href="#" @click.prevent="changePage(offerStore.currentPage + 1)"><i class="bi bi-chevron-right"></i></a>
          </li>
        </ul>
      </nav>
    </div>

    <!-- Modal Form (Create/Edit) -->
    <div v-if="showOfferModal" class="modal fade show d-block" tabindex="-1" style="background: rgba(0,0,0,0.5);" @click.self="closeModal">
      <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content border-0 rounded-4 shadow-lg">
          <div class="modal-header border-bottom-0 bg-light rounded-top-4 p-4">
            <h5 class="modal-title fw-bold"><i class="bi bi-ticket-detailed-fill text-warning me-2"></i>{{ isEditing ? 'Cập nhật Mã Giảm Giá' : 'Tạo Mã Giảm Giá Mới' }}</h5>
            <button type="button" class="btn-close" @click="closeModal" aria-label="Close"></button>
          </div>
          <div class="modal-body p-4">
            <form @submit.prevent="saveOffer">
              <div class="row g-3">
                <div class="col-md-6">
                  <label class="form-label fw-bold">Mã Code (VD: SUMMER20)</label>
                  <input type="text" class="form-control rounded-3" v-model="formData.code" required :disabled="isEditing">
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-bold">Tên chương trình</label>
                  <input type="text" class="form-control rounded-3" v-model="formData.name" required>
                </div>
                <div class="col-12">
                  <label class="form-label fw-bold">Mô tả ngắn</label>
                  <textarea class="form-control rounded-3" v-model="formData.description" rows="2"></textarea>
                </div>
                
                <div class="col-md-6">
                  <label class="form-label fw-bold">Loại giảm giá</label>
                  <select class="form-select rounded-3" v-model="formData.discountType">
                    <option value="PERCENTAGE">Phần trăm (%)</option>
                    <option value="FIXED_AMOUNT">Số tiền cố định (VNĐ)</option>
                  </select>
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-bold">Mức giảm</label>
                  <input type="number" class="form-control rounded-3" v-model.number="formData.discountValue" required min="1">
                </div>

                <div class="col-md-6" v-if="formData.discountType === 'PERCENTAGE'">
                  <label class="form-label fw-bold">Giảm tối đa (VNĐ)</label>
                  <input type="number" class="form-control rounded-3" v-model.number="formData.maxDiscount" placeholder="Để trống nếu không giới hạn">
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-bold">Giá trị đơn tối thiểu (VNĐ)</label>
                  <input type="number" class="form-control rounded-3" v-model.number="formData.minOrderValue" required min="0">
                </div>

                <div class="col-md-4">
                  <label class="form-label fw-bold">Tổng số lượt dùng</label>
                  <input type="number" class="form-control rounded-3" v-model.number="formData.totalQuantity" placeholder="Để trống = vô hạn">
                </div>
                <div class="col-md-4">
                  <label class="form-label fw-bold">Lượt dùng/khách</label>
                  <input type="number" class="form-control rounded-3" v-model.number="formData.usageLimitPerUser" required min="1">
                </div>
                <div class="col-md-4 d-flex align-items-end pb-2">
                  <div class="form-check form-switch">
                    <input class="form-check-input" type="checkbox" role="switch" id="isPublicSwitch" v-model="formData.isPublic">
                    <label class="form-check-label ms-2" for="isPublicSwitch">Hiển thị Public</label>
                  </div>
                </div>

                <div class="col-md-6">
                  <label class="form-label fw-bold">Ngày bắt đầu</label>
                  <input type="datetime-local" class="form-control rounded-3" v-model="formData.startDate" required>
                </div>
                <div class="col-md-6">
                  <label class="form-label fw-bold">Ngày kết thúc</label>
                  <input type="datetime-local" class="form-control rounded-3" v-model="formData.endDate" required>
                </div>

                <!-- Add Service Selection -->
                <div class="col-12 mt-2">
                  <label class="form-label fw-bold">Dịch vụ áp dụng</label>
                  <div class="card border-0 bg-light p-3 rounded-3">
                    <div class="row g-2">
                      <div class="col-md-6" v-for="svc in availableServices" :key="svc.id">
                        <div class="form-check">
                          <input class="form-check-input" type="checkbox" :value="svc.id" :id="'svc-'+svc.id" v-model="formData.appliedServiceIds">
                          <label class="form-check-label text-dark" :for="'svc-'+svc.id">
                            {{ svc.name }}
                          </label>
                        </div>
                      </div>
                      <div v-if="availableServices.length === 0" class="text-muted small w-100">
                        Đang tải danh sách dịch vụ...
                      </div>
                    </div>
                  </div>
                  <small class="text-muted mt-1 d-block"><i class="bi bi-info-circle me-1"></i> Bỏ trống tất cả nếu muốn mã này có thể áp dụng cho mọi dịch vụ.</small>
                </div>
              </div>
            </form>
          </div>
          <div class="modal-footer border-top-0 bg-light rounded-bottom-4 p-4">
            <button type="button" class="btn btn-secondary rounded-pill px-4" @click="closeModal">Hủy</button>
            <button type="button" class="btn btn-warning text-white fw-bold rounded-pill px-5" @click="saveOffer" :disabled="offerStore.loading">
              <span v-if="offerStore.loading" class="spinner-border spinner-border-sm me-2"></span>
              {{ isEditing ? 'Lưu thay đổi' : 'Tạo mới' }}
            </button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOfferStore } from '../../stores/offerStore';
import api from '../../services/api';

const offerStore = useOfferStore();
const searchQuery = ref('');
const statusFilter = ref('');
let searchTimeout: any = null;

const showOfferModal = ref(false);
const isEditing = ref(false);
const currentEditId = ref('');
const availableServices = ref<any[]>([]);

const formData = ref({
  code: '',
  name: '',
  description: '',
  discountType: 'PERCENTAGE' as 'PERCENTAGE' | 'FIXED_AMOUNT',
  discountValue: 0,
  maxDiscount: null as number | null,
  minOrderValue: 0,
  totalQuantity: null as number | null,
  usageLimitPerUser: 1,
  startDate: '',
  endDate: '',
  isPublic: true,
  appliedServiceIds: [] as number[]
});

onMounted(() => {
  fetchOffers();
  fetchServices();
});

const fetchServices = async () => {
  try {
    const res = await api.get('/service');
    availableServices.value = res.data;
  } catch (error) {
    console.error("Failed to fetch services", error);
  }
};

const closeModal = () => {
  showOfferModal.value = false;
};

const fetchOffers = () => {
  offerStore.fetchOffers(1, 10, statusFilter.value, searchQuery.value);
};

const debouncedFetch = () => {
  if (searchTimeout) clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    fetchOffers();
  }, 500);
};

const changePage = (page: number) => {
  if (page >= 1 && page <= offerStore.totalPages) {
    offerStore.fetchOffers(page, 10, statusFilter.value, searchQuery.value);
  }
};

const toLocalDatetimeString = (date: string | Date) => {
  const d = new Date(date);
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const openCreateModal = () => {
  isEditing.value = false;
  formData.value = {
    code: '',
    name: '',
    description: '',
    discountType: 'PERCENTAGE',
    discountValue: 0,
    maxDiscount: null,
    minOrderValue: 0,
    totalQuantity: null,
    usageLimitPerUser: 1,
    startDate: toLocalDatetimeString(new Date()),
    endDate: toLocalDatetimeString(new Date(Date.now() + 30 * 24 * 60 * 60 * 1000)),
    isPublic: true,
    appliedServiceIds: []
  };
  showOfferModal.value = true;
};

const openEditModal = (offer: any) => {
  isEditing.value = true;
  currentEditId.value = offer.id;
  formData.value = {
    code: offer.code,
    name: offer.name,
    description: offer.description,
    discountType: offer.discountType,
    discountValue: offer.discountValue,
    maxDiscount: offer.maxDiscount,
    minOrderValue: offer.minOrderValue,
    totalQuantity: offer.totalQuantity,
    usageLimitPerUser: offer.usageLimitPerUser,
    startDate: toLocalDatetimeString(offer.startDate),
    endDate: toLocalDatetimeString(offer.endDate),
    isPublic: offer.isPublic,
    appliedServiceIds: offer.appliedServiceIds || []
  };
  showOfferModal.value = true;
};

const saveOffer = async () => {
  try {
    // Sanitize payload: convert empty strings to null for nullable number fields
    const payload: any = { ...formData.value };
    if (!payload.totalQuantity || payload.totalQuantity <= 0) payload.totalQuantity = null;
    if (!payload.maxDiscount || payload.maxDiscount <= 0) payload.maxDiscount = null;
    
    // Convert local datetime-local string to UTC ISO string before sending
    payload.startDate = new Date(formData.value.startDate).toISOString();
    payload.endDate = new Date(formData.value.endDate).toISOString();

    if (isEditing.value) {
      await offerStore.updateOffer(currentEditId.value, payload);
    } else {
      await offerStore.createOffer(payload as any);
    }
    closeModal();
  } catch (e: any) {
    if (e.response?.data?.errors) {
      const errorMsgs = Object.values(e.response.data.errors).flat().join('\n');
      alert('Lỗi dữ liệu nhập:\n' + errorMsgs);
    } else {
      alert(e.response?.data?.message || 'Có lỗi xảy ra');
    }
  }
};

const lockOffer = async (offer: any) => {
  if (confirm(`Bạn có chắc muốn khóa mã ${offer.code} không? Khách hàng sẽ không thể sử dụng mã này nữa.`)) {
    try {
      await offerStore.lockOffer(offer.id);
    } catch (e: any) {
      alert(e.response?.data?.message || 'Không thể khóa mã');
    }
  }
};

// Helpers
const getStatusLabel = (status: string) => {
  switch (status) {
    case 'ACTIVE': return 'Đang chạy';
    case 'LOCKED': return 'Bị khóa';
    case 'EXPIRED': return 'Hết hạn';
    default: return status;
  }
};

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleDateString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const getUsagePercentage = (offer: any) => {
  if (!offer.totalQuantity) return 0;
  return Math.min(100, Math.round((offer.usedQuantity / offer.totalQuantity) * 100));
};

</script>

<style scoped>
.glassmorphism-card {
  background: rgba(255, 255, 255, 0.85) !important;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
}

.offer-card {
  transition: transform 0.2s, box-shadow 0.2s;
}

.offer-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px rgba(0,0,0,0.1) !important;
}

.offer-card.locked {
  opacity: 0.75;
  filter: grayscale(0.5);
}

.bg-gradient-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.text-truncate-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.pagination-rounded .page-link {
  border-radius: 50%;
  margin: 0 5px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #4a4a4a;
}

.pagination-rounded .page-item.active .page-link {
  background-color: var(--primary-gold, #f59e0b);
  color: white;
}
</style>
