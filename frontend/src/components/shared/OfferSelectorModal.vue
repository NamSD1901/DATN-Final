<template>
  <div v-if="show" class="voucher-modal-overlay animate__animated animate__fadeIn" @click.self="closeModal">
    <div class="voucher-modal-card animate__animated animate__slideInUp">
      
      <!-- Header -->
      <div class="modal-header bg-white sticky-top pt-4 pb-3 px-4 border-bottom d-flex justify-content-between align-items-center">
        <h5 class="fw-bold text-dark mb-0"><i class="bi bi-ticket-perforated-fill text-warning me-2"></i>Chọn mã khuyến mãi</h5>
        <button class="btn btn-light rounded-circle shadow-sm" style="width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;" @click="closeModal">
          <i class="bi bi-x-lg"></i>
        </button>
      </div>

      <!-- Input box for custom code -->
      <div class="p-4 bg-light">
        <div class="d-flex gap-2">
          <input type="text" class="form-control rounded-pill px-4 border-0 shadow-sm custom-input" placeholder="Nhập mã voucher (VD: SUMMER20)" v-model="manualCode">
          <button class="btn btn-warning rounded-pill px-4 text-white fw-bold shadow-sm flex-shrink-0" @click="applyManualCode" :disabled="!manualCode">Áp dụng</button>
        </div>
      </div>

      <!-- Voucher List -->
      <div class="modal-body p-4 custom-scrollbar bg-white" style="max-height: 50vh; overflow-y: auto;">
        
        <div v-if="offerStore.loading" class="text-center py-5">
          <div class="spinner-border text-warning" role="status"></div>
          <p class="text-muted mt-2 small">Đang tải mã giảm giá...</p>
        </div>

        <div v-else-if="offerStore.publicOffers.length === 0" class="text-center py-5">
          <i class="bi bi-ticket-detailed text-muted opacity-50" style="font-size: 4rem;"></i>
          <h6 class="text-muted mt-3">Hiện chưa có khuyến mãi nào</h6>
        </div>

        <div class="voucher-list d-flex flex-column gap-3" v-else>
          <div v-for="offer in offerStore.publicOffers" :key="offer.id" 
               class="voucher-item card border-0 shadow-sm rounded-4 position-relative overflow-hidden cursor-pointer"
               :class="{'selected-voucher': selectedCode === offer.code, 'disabled-voucher': !isOfferValid(offer)}"
               @click="isOfferValid(offer) ? selectOffer(offer) : null">
            
            <div class="d-flex h-100">
              <!-- Left Edge color -->
              <div class="voucher-edge d-flex align-items-center justify-content-center text-white" :class="isOfferValid(offer) ? 'bg-gradient-warning' : 'bg-secondary'">
                <div class="text-vertical fw-bold fs-5 tracking-widest px-2" style="writing-mode: vertical-rl; transform: rotate(180deg);">
                  VOUCHER
                </div>
              </div>

              <!-- Content -->
              <div class="card-body p-3 flex-grow-1">
                <div class="d-flex justify-content-between align-items-start mb-1">
                  <h6 class="fw-bold mb-0 text-dark">{{ offer.name }}</h6>
                  <span class="badge bg-light text-dark border fw-bold px-2 py-1 rounded-3 ms-2">{{ offer.code }}</span>
                </div>
                
                <p class="text-muted small mb-2 text-truncate-2" style="font-size: 0.8rem;">{{ offer.description }}</p>
                
                <div class="d-flex justify-content-between align-items-end mt-3">
                  <div>
                    <span class="text-danger fw-bold fs-6 d-block">
                      Giảm {{ offer.discountType === 'PERCENTAGE' ? `${offer.discountValue}%` : formatCurrency(offer.discountValue) }}
                    </span>
                    <span class="text-muted small" style="font-size: 0.75rem;">Đơn tối thiểu: {{ formatCurrency(offer.minOrderValue) }}</span>
                  </div>
                  <div class="text-end">
                    <span class="d-block text-muted small" style="font-size: 0.75rem;">HSD: {{ formatDate(offer.endDate) }}</span>
                    <span v-if="!isOfferValid(offer)" class="text-danger small fw-bold" style="font-size: 0.75rem;">Chưa đủ điều kiện</span>
                  </div>
                </div>
              </div>

              <!-- Selection Circle -->
              <div class="voucher-radio d-flex align-items-center justify-content-center p-3 border-start">
                <div class="radio-circle border rounded-circle d-flex align-items-center justify-content-center" 
                     :class="selectedCode === offer.code ? 'border-warning bg-warning' : 'border-secondary'">
                  <i class="bi bi-check text-white fs-5" v-if="selectedCode === offer.code"></i>
                </div>
              </div>

            </div>
          </div>
        </div>

      </div>

      <!-- Footer -->
      <div class="modal-footer bg-white border-top p-4">
        <button class="btn btn-outline-secondary rounded-pill px-4" @click="closeModal">Bỏ qua</button>
        <button class="btn btn-warning text-white fw-bold rounded-pill px-5 shadow-sm" @click="confirmSelection" :disabled="!selectedCode">
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
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(5px);
  z-index: 2050;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.voucher-modal-card {
  background: white;
  border-radius: 1.5rem;
  width: 100%;
  max-width: 500px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.voucher-edge {
  width: 40px;
  border-right: 2px dashed rgba(255,255,255,0.5);
  position: relative;
}
.voucher-edge::before, .voucher-edge::after {
  content: '';
  position: absolute;
  width: 16px;
  height: 16px;
  background-color: #fff;
  border-radius: 50%;
  right: -8px;
}
.voucher-edge::before {
  top: -8px;
}
.voucher-edge::after {
  bottom: -8px;
}

.bg-gradient-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.voucher-item {
  transition: all 0.2s;
  border: 2px solid transparent !important;
}

.voucher-item:not(.disabled-voucher):hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1) !important;
}

.selected-voucher {
  border: 2px solid #f59e0b !important;
  background-color: #fffbeb;
}

.disabled-voucher {
  opacity: 0.6;
  filter: grayscale(0.8);
  cursor: not-allowed !important;
}

.radio-circle {
  width: 24px;
  height: 24px;
  transition: all 0.2s;
}

.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 10px;
}
</style>
