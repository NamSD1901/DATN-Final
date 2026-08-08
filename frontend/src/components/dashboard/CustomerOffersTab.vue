<template>
  <div class="customer-offers-tab animate__animated animate__fadeIn p-4">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <h4 class="fw-bold text-dark mb-1"><i class="bi bi-gift-fill text-warning me-2"></i>Kho Voucher của tôi</h4>
        <p class="text-muted small mb-0">Thu thập mã giảm giá để tiết kiệm chi phí chăm sóc thú cưng</p>
      </div>
      <div class="d-flex gap-2">
        <input type="text" class="form-control rounded-pill px-4 border-0 shadow-sm" placeholder="Nhập mã voucher (VD: SUMMER)" v-model="manualCode" style="text-transform: uppercase;">
        <button class="btn btn-warning rounded-pill px-4 text-white fw-bold shadow-sm" @click="saveManualCode" :disabled="!manualCode">Lưu mã</button>
      </div>
    </div>

    <!-- Active Offers Grid -->
    <div v-if="offerStore.loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status"></div>
    </div>

    <div v-else-if="offerStore.publicOffers.length === 0" class="text-center py-5 bg-white rounded-4 shadow-sm">
      <i class="bi bi-balloon-heart text-muted" style="font-size: 4rem; opacity: 0.5;"></i>
      <h5 class="mt-3 text-muted">Hiện tại chưa có khuyến mãi nào</h5>
      <p class="text-muted small">Hãy quay lại sau để săn voucher hấp dẫn nhé!</p>
    </div>

    <div v-else class="row g-4">
      <div class="col-lg-4 col-md-6" v-for="offer in offerStore.publicOffers" :key="offer.id">
        <div class="voucher-card bg-white rounded-4 shadow-sm overflow-hidden d-flex flex-column h-100">
          <div class="bg-gradient-warning text-white p-3 text-center position-relative">
            <h5 class="fw-bold mb-0">Giảm {{ offer.discountType === 'PERCENTAGE' ? `${offer.discountValue}%` : formatCurrency(offer.discountValue) }}</h5>
            <div class="cut-circle-left"></div>
            <div class="cut-circle-right"></div>
          </div>
          
          <div class="p-4 flex-grow-1 d-flex flex-column">
            <h6 class="fw-bold text-dark mb-1">{{ offer.name }}</h6>
            <p class="text-muted small mb-3 flex-grow-1">{{ offer.description }}</p>
            
            <div class="d-flex justify-content-between text-muted small mb-3">
              <span><i class="bi bi-bag-check me-1"></i> Đơn tối thiểu:</span>
              <span class="fw-bold text-dark">{{ formatCurrency(offer.minOrderValue) }}</span>
            </div>
            
            <div class="d-flex justify-content-between text-muted small mb-4">
              <span><i class="bi bi-calendar-event me-1"></i> HSD:</span>
              <span class="fw-bold text-danger">{{ formatDate(offer.endDate) }}</span>
            </div>
            
            <div class="mt-auto pt-3 border-top d-flex justify-content-between align-items-center">
              <span class="badge bg-light text-dark border px-3 py-2 fw-bold" style="font-size: 0.85rem; letter-spacing: 1px;">
                {{ offer.code }}
              </span>
              <button class="btn btn-outline-warning rounded-pill btn-sm px-3 fw-bold" @click="copyCode(offer.code)">
                Sao chép
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useOfferStore } from '../../stores/offerStore';

const offerStore = useOfferStore();
const manualCode = ref('');

onMounted(() => {
  offerStore.fetchPublicOffers(1, 20);
});

const saveManualCode = () => {
  if (manualCode.value) {
    alert(`Đã lưu mã: ${manualCode.value.toUpperCase()}`);
    manualCode.value = '';
  }
};

const copyCode = (code: string) => {
  navigator.clipboard.writeText(code);
  alert('Đã sao chép mã: ' + code);
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
.bg-gradient-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.voucher-card {
  transition: transform 0.2s, box-shadow 0.2s;
}

.voucher-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 30px rgba(0,0,0,0.1) !important;
}

.cut-circle-left, .cut-circle-right {
  position: absolute;
  width: 20px;
  height: 20px;
  background-color: #f4f6f9;
  border-radius: 50%;
  bottom: -10px;
  z-index: 10;
}

.cut-circle-left {
  left: -10px;
}

.cut-circle-right {
  right: -10px;
}
</style>
