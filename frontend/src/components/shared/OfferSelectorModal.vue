<template>
  <div v-if="show" class="voucher-modal-overlay animate__animated animate__fadeIn" @click.self="closeModal">
    <div class="voucher-modal-card animate__animated animate__slideInUp zoom-in">
      
      <!-- Header -->
      <div class="modal-header d-flex justify-content-between align-items-center">
        <h5 class="fw-bold mb-0 modal-title">
          <i class="bi bi-ticket-perforated-fill me-2 title-icon"></i>
          Chọn mã khuyến mãi
        </h5>
        <button class="close-btn" @click="closeModal">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <!-- Input box for custom code -->
      <div class="p-4 bg-light-surface">
        <div class="d-flex gap-2 align-items-center bg-white rounded-pill p-1 shadow-sm border focus-ring-container">
          <input type="text" class="form-control border-0 bg-transparent custom-input" placeholder="Nhập mã voucher (VD: SUMMER20)" v-model="manualCode" @keyup.enter="applyManualCode">
          <button class="btn btn-apply rounded-pill px-4 fw-bold flex-shrink-0" @click="applyManualCode" :disabled="!manualCode">Áp dụng</button>
        </div>
      </div>

      <!-- Voucher List -->
      <div class="modal-body p-4 custom-scrollbar bg-white" style="max-height: 55vh; overflow-y: auto;">
        
        <div v-if="offerStore.loading" class="text-center py-5">
          <div class="spinner-border text-primary" role="status"></div>
          <p class="text-muted mt-2 small fw-medium">Đang tìm mã giảm giá tốt nhất cho bạn...</p>
        </div>

        <div v-else-if="offerStore.publicOffers.length === 0" class="text-center py-5 empty-state">
          <div class="empty-icon-container mb-3">
            <i class="bi bi-ticket-detailed"></i>
          </div>
          <h6 class="text-secondary fw-bold">Chưa có mã khuyến mãi nào</h6>
          <p class="text-muted small">Vui lòng quay lại sau nhé!</p>
        </div>

        <div class="voucher-list d-flex flex-column gap-3" v-else>
          <div v-for="offer in offerStore.publicOffers" :key="offer.id" 
               class="voucher-item card border-0 shadow-sm rounded-4 position-relative overflow-hidden cursor-pointer"
               :class="{'selected-voucher': selectedCode === offer.code, 'disabled-voucher': !isOfferValid(offer)}"
               @click="isOfferValid(offer) ? selectOffer(offer) : null">
            
            <div class="d-flex h-100">
              <!-- Left Edge color -->
              <div class="voucher-edge d-flex align-items-center justify-content-center text-white" :class="isOfferValid(offer) ? 'bg-gradient-primary' : 'bg-secondary'">
                <div class="text-vertical fw-bold fs-5 tracking-widest px-2" style="writing-mode: vertical-rl; transform: rotate(180deg);">
                  VOUCHER
                </div>
              </div>

              <!-- Content -->
              <div class="card-body p-3 flex-grow-1 d-flex flex-column justify-content-between">
                <div>
                  <div class="d-flex justify-content-between align-items-start mb-1 gap-2">
                    <h6 class="fw-bold mb-0 text-dark" style="line-height: 1.4;">{{ offer.name }}</h6>
                    <span class="badge bg-light text-primary border border-primary fw-bold px-2 py-1 rounded-3">{{ offer.code }}</span>
                  </div>
                  <p class="text-muted small mb-2 text-truncate-2 mt-2" style="font-size: 0.85rem;">{{ offer.description }}</p>
                </div>
                
                <div class="d-flex justify-content-between align-items-end mt-2 pt-2 border-top border-light border-opacity-50">
                  <div>
                    <span class="text-danger fw-bold fs-5 d-block lh-1 mb-1">
                      Giảm {{ offer.discountType === 'PERCENTAGE' ? `${offer.discountValue}%` : formatCurrency(offer.discountValue) }}
                    </span>
                    <span class="text-secondary small" style="font-size: 0.8rem;">Đơn tối thiểu: <span class="fw-medium text-dark">{{ formatCurrency(offer.minOrderValue) }}</span></span>
                  </div>
                  <div class="text-end">
                    <span class="d-block text-secondary small mb-1" style="font-size: 0.8rem;">HSD: <span class="fw-medium text-dark">{{ formatDate(offer.endDate) }}</span></span>
                    <span v-if="!isOfferValid(offer)" class="text-danger small fw-bold px-2 py-1 bg-danger bg-opacity-10 rounded-2" style="font-size: 0.75rem;">Chưa đủ điều kiện</span>
                  </div>
                </div>
              </div>

              <!-- Selection Circle -->
              <div class="voucher-radio d-flex align-items-center justify-content-center p-3">
                <div class="radio-circle border d-flex align-items-center justify-content-center" 
                     :class="selectedCode === offer.code ? 'border-primary bg-primary' : 'border-secondary bg-light'">
                  <i class="bi bi-check2 text-white fs-5" v-if="selectedCode === offer.code"></i>
                </div>
              </div>

            </div>
          </div>
        </div>

      </div>

      <!-- Footer -->
      <div class="modal-footer bg-white p-4 justify-content-between border-top-0 shadow-sm-top">
        <button class="btn btn-light rounded-pill px-4 fw-medium text-secondary" @click="closeModal">Bỏ qua</button>
        <button class="btn btn-primary-gradient text-white fw-bold rounded-pill px-5 shadow" @click="confirmSelection" :disabled="!selectedCode">
          Áp dụng Voucher
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOfferStore } from '../../stores/offerStore';

const props = defineProps({
  show: Boolean,
  orderAmount: {
    type: Number,
    default: 0
  },
  currentSelectedCode: {
    type: String,
    default: ''
  },
  customerId: {
    type: String,
    default: null
  },
  serviceId: {
    type: Number,
    default: null
  },
  serviceIds: {
    type: Array as () => number[],
    default: () => []
  }
});

const emit = defineEmits(['close', 'apply']);

const offerStore = useOfferStore();
const selectedCode = ref('');
const manualCode = ref('');

onMounted(() => {
  offerStore.fetchPublicOffers(1, 20, props.customerId);
  if (props.currentSelectedCode) {
    selectedCode.value = props.currentSelectedCode;
  }
});

const isOfferValid = (offer: any) => {
  if (offer.minOrderValue > props.orderAmount) return false;
  
  if (offer.appliedServiceIds && offer.appliedServiceIds.length > 0) {
    // Combine serviceId and serviceIds, ensuring they are numbers
    const sIds = props.serviceIds.map(id => Number(id));
    if (props.serviceId && !sIds.includes(Number(props.serviceId))) {
      sIds.push(Number(props.serviceId));
    }
    
    // If no services provided but offer requires specific services, it's invalid
    if (sIds.length === 0) return false;
    
    // Check if ANY of the provided services match the offer's appliedServiceIds
    if (!offer.appliedServiceIds.some((id: any) => sIds.includes(Number(id)))) {
      return false;
    }
  }
  return true;
};

const selectOffer = (offer: any) => {
  if (selectedCode.value === offer.code) {
    selectedCode.value = ''; // toggle off
  } else {
    selectedCode.value = offer.code;
  }
};

const applyManualCode = () => {
  if (manualCode.value) {
    selectedCode.value = manualCode.value.trim();
    confirmSelection();
  }
};

const closeModal = () => {
  emit('close');
};

const confirmSelection = () => {
  emit('apply', selectedCode.value);
};

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  return new Date(dateString).toLocaleDateString('vi-VN');
};
</script>

<style scoped>
.voucher-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  z-index: 2050;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.voucher-modal-card {
  background: #ffffff;
  border-radius: 1.5rem;
  width: 100%;
  max-width: 520px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25), 0 0 0 1px rgba(0,0,0,0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.zoom-in {
  animation: zoomIn 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

@keyframes zoomIn {
  from {
    opacity: 0;
    transform: scale(0.95) translateY(10px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}

.modal-header {
  padding: 1.5rem 1.5rem 1.25rem;
  background: linear-gradient(to right, #ffffff, #f8fafc);
  border-bottom: 1px solid #f1f5f9;
}

.modal-title {
  color: #1e293b;
  font-size: 1.25rem;
  letter-spacing: -0.02em;
}

.title-icon {
  color: #3b82f6; /* Premium Blue */
  background: #eff6ff;
  padding: 0.4rem 0.5rem;
  border-radius: 0.5rem;
}

.close-btn {
  background: #f1f5f9;
  border: none;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b;
  transition: all 0.2s ease;
  cursor: pointer;
}

.close-btn:hover {
  background: #e2e8f0;
  color: #0f172a;
  transform: rotate(90deg);
}

.bg-light-surface {
  background-color: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.custom-input {
  box-shadow: none !important;
  font-size: 0.95rem;
}

.custom-input::placeholder {
  color: #94a3b8;
}

.focus-ring-container:focus-within {
  border-color: #93c5fd !important;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2) !important;
}

.btn-apply {
  background: #3b82f6;
  color: white;
  transition: all 0.2s ease;
}

.btn-apply:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-1px);
}

.btn-apply:disabled {
  background: #cbd5e1;
  color: #94a3b8;
  opacity: 0.8;
}

.voucher-item {
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  border: 2px solid transparent !important;
  background: #ffffff;
}

.voucher-item:not(.disabled-voucher):hover {
  transform: translateY(-3px) scale(1.01);
  box-shadow: 0 15px 30px -5px rgba(0, 0, 0, 0.08), 0 8px 10px -5px rgba(0, 0, 0, 0.04) !important;
  border-color: rgba(59, 130, 246, 0.1) !important;
}

.selected-voucher {
  border: 2px solid #3b82f6 !important;
  background-color: #eff6ff !important;
  box-shadow: 0 4px 6px -1px rgba(59, 130, 246, 0.1), 0 2px 4px -1px rgba(59, 130, 246, 0.06) !important;
}

.selected-voucher .badge {
  background-color: #3b82f6 !important;
  color: white !important;
}

.disabled-voucher {
  opacity: 0.65;
  filter: grayscale(0.6);
  cursor: not-allowed !important;
  background: #f8fafc;
}

.voucher-edge {
  width: 44px;
  border-right: 2px dashed rgba(255, 255, 255, 0.6);
  position: relative;
}

.voucher-edge::before, .voucher-edge::after {
  content: '';
  position: absolute;
  width: 20px;
  height: 20px;
  background-color: #ffffff;
  border-radius: 50%;
  right: -10px;
  z-index: 1;
}

.selected-voucher .voucher-edge::before, 
.selected-voucher .voucher-edge::after {
  background-color: #eff6ff;
}

.voucher-edge::before {
  top: -10px;
  box-shadow: inset 0 -2px 4px rgba(0,0,0,0.02);
}
.voucher-edge::after {
  bottom: -10px;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.02);
}

.bg-gradient-primary {
  background: linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%);
}

.radio-circle {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.selected-voucher .radio-circle {
  transform: scale(1.1);
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.2);
}

.shadow-sm-top {
  box-shadow: 0 -4px 6px -1px rgba(0, 0, 0, 0.05);
}

.btn-primary-gradient {
  background: linear-gradient(to right, #3b82f6, #2563eb);
  border: none;
  transition: all 0.2s ease;
}

.btn-primary-gradient:hover:not(:disabled) {
  background: linear-gradient(to right, #2563eb, #1d4ed8);
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(59, 130, 246, 0.4), 0 4px 6px -2px rgba(59, 130, 246, 0.2) !important;
}

.btn-primary-gradient:disabled {
  background: #94a3b8;
  opacity: 0.7;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.empty-icon-container {
  width: 80px;
  height: 80px;
  background: #f1f5f9;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  color: #94a3b8;
}

.text-truncate-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 10px;
  border: 2px solid #ffffff;
}
.custom-scrollbar:hover::-webkit-scrollbar-thumb {
  background-color: #94a3b8;
}
</style>
