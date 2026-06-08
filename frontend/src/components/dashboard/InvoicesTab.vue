<template>
  <div class="invoices-tab container-fluid p-0">
    <div class="row g-4">
      <!-- Left Column: Checkout Queue -->
      <div class="col-md-4 col-lg-3">
        <div class="card border-0 shadow-sm rounded-4 h-100 bg-white">
          <div class="card-body p-3">
            <div class="d-flex justify-content-between align-items-center mb-3 px-1">
              <h6 class="fw-bold mb-0 text-dark">
                <i class="bi bi-hourglass-split text-warning me-1"></i> Chờ Thanh Toán
              </h6>
              <button class="btn btn-light border rounded-circle p-1" @click="loadPendingCheckouts" title="Làm mới">
                <i class="bi bi-arrow-clockwise"></i>
              </button>
            </div>
            
            <div class="checkout-queue-list d-flex flex-column gap-2" style="max-height: 600px; overflow-y: auto;">
              <div v-if="loadingQueue" class="text-center py-5 text-muted">
                <div class="spinner-border spinner-border-sm text-warning mb-2"></div>
                <div class="small">Đang tải hàng chờ...</div>
              </div>
              <div v-else-if="queueList.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-check-circle fs-3 text-success d-block mb-2"></i>
                <div class="small fw-bold">Hàng chờ trống!</div>
                <div class="small opacity-75">Không có ca chờ thanh toán.</div>
              </div>
              
              <div 
                v-for="item in queueList" 
                :key="item.appointmentId"
                class="card checkout-queue-card p-3 rounded-3 border-0 bg-light-hover"
                :class="{ 'active-card': selectedAppointmentId === item.appointmentId }"
                @click="selectAppointment(item.appointmentId)"
              >
                <div class="d-flex justify-content-between align-items-start mb-2">
                  <h6 class="fw-bold mb-0 text-dark">
                    {{ getAnimalEmoji(item.species) }} {{ item.petName || 'Không tên' }}
                    <span v-if="item.isEmergency" class="badge bg-danger ms-1 text-white" style="font-size: 0.6rem;">CẤP CỨU</span>
                  </h6>
                  <span class="badge bg-light text-secondary border rounded-pill">#{{ item.queueNumber }}</span>
                </div>
                <div class="text-muted small mb-2"><i class="bi bi-person-circle text-warning me-1"></i>{{ item.customerName || 'Khách vãng lai' }}</div>
                <div class="d-flex justify-content-between align-items-center mt-2 pt-2 border-top">
                  <span class="badge rounded-pill" :class="item.status === 'completed' ? 'bg-success text-white' : 'bg-warning text-dark'" style="font-size: 0.7rem;">
                    {{ item.status === 'completed' ? 'Chờ in lại HĐ' : 'Chờ thanh toán' }}
                  </span>
                  <span class="small fw-bold text-muted">Bs. {{ getLastWord(item.doctorName) }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Right Column: Invoice Details & Action Panel -->
      <div class="col-md-8 col-lg-9">
        <!-- Empty State -->
        <div v-if="!selectedAppointmentId" class="card border-0 shadow-sm rounded-4 h-100 text-center py-5 bg-white">
          <div class="card-body py-5 d-flex flex-column align-items-center justify-content-center">
            <div class="rounded-circle bg-warning bg-opacity-10 d-flex align-items-center justify-content-center mb-4 shadow-sm" style="width: 90px; height: 90px;">
              <i class="bi bi-credit-card-2-front fs-1 text-warning animate-pulse"></i>
            </div>
            <h4 class="fw-bold text-dark mb-2">Chưa chọn ca thanh toán</h4>
            <p class="text-muted text-wrap mb-0" style="max-width: 400px;">
              Vui lòng chọn một ca khám từ danh sách chờ thanh toán bên trái để lập hóa đơn và xử lý giao dịch.
            </p>
          </div>
        </div>

        <!-- Detail Panel -->
        <div v-else class="card border-0 shadow-sm rounded-4 p-4 bg-white" v-loading="loadingInvoice">
          <div v-if="loadingInvoice" class="text-center py-5 text-muted">
            <div class="spinner-border text-warning mb-2"></div>
            <div>Đang lập hóa đơn chi phí...</div>
          </div>
          <div v-else-if="invoice" class="row g-4 animate-fade-in">
            <!-- Left panel: Itemized Cost details -->
            <div class="col-lg-8">
              <!-- Patient Owner Header card -->
              <div class="p-3 bg-light rounded-4 border mb-4">
                <div class="row align-items-center">
                  <div class="col-sm-7 mb-2 mb-sm-0">
                    <small class="text-muted text-uppercase fw-bold" style="font-size: 0.65rem;">Thú cưng & Khách hàng</small>
                    <h5 class="fw-bold text-primary mb-1">🐾 {{ invoice.petName }} ({{ invoice.petSpecies || 'Khác' }})</h5>
                    <div class="text-dark small">
                      <i class="bi bi-person-circle text-warning me-1"></i> Chủ nuôi: <strong>{{ invoice.customerName }}</strong> - {{ invoice.customerPhone }}
                    </div>
                  </div>
                  <div class="col-sm-5 text-sm-end border-start-md ps-sm-4">
                    <small class="text-muted text-uppercase fw-bold" style="font-size: 0.65rem;">Bác sĩ phụ trách</small>
                    <div class="fw-bold text-dark"><i class="bi bi-heart-pulse-fill text-danger me-1"></i>{{ invoice.doctorName || 'Chưa phân công' }}</div>
                    <small class="text-muted"><i class="bi bi-calendar-event me-1"></i>{{ formatDate(invoice.createdAt) }}</small>
                  </div>
                </div>
              </div>

              <!-- Catalog Search Autocomplete -->
              <div class="position-relative mb-4">
                <label class="form-label fw-bold text-dark mb-1">Tìm kiếm & thêm nhanh Sản phẩm / Thuốc / Dịch vụ</label>
                <div class="input-group">
                  <span class="input-group-text bg-white"><i class="bi bi-search text-muted"></i></span>
                  <input 
                    type="text" 
                    v-model="catalogQuery" 
                    @input="searchCatalog"
                    class="form-control border-start-0 input-premium" 
                    placeholder="Gõ tên thuốc, dịch vụ khám, thức ăn, phụ kiện..."
                    autocomplete="off"
                  />
                </div>
                <!-- Autocomplete Dropdown list -->
                <div v-if="catalogResult.length > 0" class="position-absolute bg-white shadow-lg border rounded-4 mt-2 p-2 w-100 catalog-dropdown" style="z-index: 1050; max-height: 250px; overflow-y: auto;">
                  <div 
                    v-for="item in catalogResult" 
                    :key="item.id"
                    class="p-2 border-bottom hover-bg-light cursor-pointer rounded-3 d-flex justify-content-between align-items-center"
                    @click="addCatalogItem(item.type, item.id)"
                  >
                    <div>
                      <span class="fs-5 me-2">{{ item.type === 'service' ? '🩺' : '💊' }}</span>
                      <span class="fw-bold text-dark">{{ item.name }}</span>
                      <small v-if="item.type === 'medicine'" class="badge bg-secondary text-white ms-2">Kho: {{ item.stockQuantity }}</small>
                    </div>
                    <span class="fw-bold text-primary">{{ formatCurrency(item.price) }}</span>
                  </div>
                </div>
              </div>

              <!-- Invoice Details List -->
              <h6 class="fw-bold text-dark mb-3"><i class="bi bi-list-check me-1 text-warning"></i>Danh sách Chi tiết Chi phí</h6>
              <div class="table-responsive rounded-4 border overflow-hidden">
                <table class="table table-hover align-middle mb-0">
                  <thead class="table-light">
                    <tr>
                      <th class="text-center" style="width: 50px;">STT</th>
                      <th>Mặt hàng</th>
                      <th>Số lượng</th>
                      <th class="text-end">Đơn giá</th>
                      <th class="text-end">Thành tiền</th>
                      <th class="text-center" style="width: 50px;">Xóa</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="invoice.items.length === 0" class="text-center text-muted">
                      <td colspan="6" class="py-4">Hóa đơn chưa có chi tiết chi phí nào. Hãy tìm thêm sản phẩm ở ô phía trên!</td>
                    </tr>
                    <tr v-for="(item, idx) in invoice.items" :key="item.id">
                      <td class="text-center fw-bold text-secondary">{{ Number(idx) + 1 }}</td>
                      <td>
                        <div class="fw-bold text-dark small">{{ item.itemName }}</div>
                        <span class="badge bg-opacity-10 px-2 py-0.5 rounded" :class="item.itemType === 'service' ? 'bg-primary text-primary' : 'bg-success text-success'" style="font-size: 0.65rem;">
                          {{ item.itemType === 'service' ? 'Dịch vụ' : 'Thuốc / Vật tư' }}
                        </span>
                      </td>
                      <td style="width: 130px;">
                        <div class="input-group input-group-sm">
                          <button class="btn btn-outline-secondary px-2" type="button" @click="changeQty(item.id, item.quantity - 1)" :disabled="item.quantity <= 1">-</button>
                          <input type="number" class="form-control text-center fw-bold px-1" :value="item.quantity" @change="handleManualQtyChange(item.id, ($event.target as HTMLInputElement).value)" style="width: 45px;" />
                          <button class="btn btn-outline-secondary px-2" type="button" @click="changeQty(item.id, item.quantity + 1)">+</button>
                        </div>
                      </td>
                      <td class="text-end fw-bold text-secondary small">{{ formatCurrency(item.unitPrice) }}</td>
                      <td class="text-end fw-bold text-dark small">{{ formatCurrency(item.totalPrice) }}</td>
                      <td class="text-center">
                        <button class="btn btn-link text-danger p-0 fs-5" @click="removeItem(item.id)">
                          <i class="bi bi-trash3-fill"></i>
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Right panel: Cost summaries & payment processor -->
            <div class="col-lg-4">
              <div class="card border-0 shadow-sm rounded-4 bg-light p-3">
                <h5 class="fw-bold text-dark mb-3 border-bottom pb-2">
                  <i class="bi bi-wallet2 text-warning me-2"></i>Thanh Toán Hóa Đơn
                </h5>

                <div class="d-flex justify-content-between mb-2 small text-muted">
                  <span>Tổng tạm tính:</span>
                  <span class="fw-bold text-dark">{{ formatCurrency(invoice.subtotal) }}</span>
                </div>
                
                <div class="d-flex justify-content-between mb-3 align-items-center">
                  <span class="small text-muted">Giảm giá (đ):</span>
                  <input 
                    type="number" 
                    v-model.number="discountAmount" 
                    @input="recalculateTotal"
                    class="form-control form-control-sm text-end fw-bold text-danger border-danger border-opacity-25" 
                    style="width: 130px;"
                  />
                </div>

                <hr class="my-3">

                <div class="d-flex justify-content-between align-items-center mb-4">
                  <span class="fw-bold text-dark">TỔNG CẦN THU:</span>
                  <span class="fw-extrabold text-primary fs-4">{{ formatCurrency(finalTotal) }}</span>
                </div>

                <!-- Payment Method Selectors -->
                <h6 class="fw-bold text-dark mb-2" style="font-size: 0.85rem;">Hình thức thanh toán</h6>
                <div class="row g-2 mb-3">
                  <div class="col-4">
                    <div 
                      class="payment-method-card p-2 text-center" 
                      :class="{ 'active-method': paymentMethod === 'cash' }"
                      @click="paymentMethod = 'cash'"
                    >
                      <i class="bi bi-cash-stack fs-4 text-success d-block mb-1"></i>
                      <span class="small fw-bold">Tiền mặt</span>
                    </div>
                  </div>
                  <div class="col-4">
                    <div 
                      class="payment-method-card p-2 text-center" 
                      :class="{ 'active-method': paymentMethod === 'qr' }"
                      @click="paymentMethod = 'qr'"
                    >
                      <i class="bi bi-qr-code-scan fs-4 text-primary d-block mb-1"></i>
                      <span class="small fw-bold">VietQR</span>
                    </div>
                  </div>
                  <div class="col-4">
                    <div 
                      class="payment-method-card p-2 text-center" 
                      :class="{ 'active-method': paymentMethod === 'pos' }"
                      @click="paymentMethod = 'pos'"
                    >
                      <i class="bi bi-credit-card fs-4 text-info d-block mb-1"></i>
                      <span class="small fw-bold">POS Thẻ</span>
                    </div>
                  </div>
                </div>

                <!-- Dynamic helper content based on selected method -->
                <div class="p-3 bg-white rounded-3 border mb-4 text-center">
                  <!-- Cash received thối tiền -->
                  <div v-if="paymentMethod === 'cash'" class="text-start">
                    <label class="form-label small fw-bold text-success mb-1">Số tiền khách đưa (đ):</label>
                    <input 
                      type="number" 
                      v-model.number="cashReceived" 
                      class="form-control text-center fw-bold text-success fs-5 border-success mb-2" 
                    />
                    <div class="d-flex justify-content-between p-2 bg-success bg-opacity-10 text-success rounded fw-bold small">
                      <span>Tiền thừa thối khách:</span>
                      <span>{{ formatCurrency(cashChange) }}</span>
                    </div>
                  </div>

                  <!-- VietQR Dynamic image -->
                  <div v-else-if="paymentMethod === 'qr'">
                    <p class="text-muted small mb-2">Quét mã QR để chuyển khoản nhanh:</p>
                    <div class="qr-code-border p-2 bg-white d-inline-block shadow-sm rounded-3">
                      <img :src="vietQrUrl" class="img-fluid" style="max-height: 160px; width: auto;" alt="VietQR" />
                    </div>
                    <div class="mt-2 text-primary small fw-bold"><i class="bi bi-bank me-1"></i>Vietcombank - 990123456789</div>
                  </div>

                  <!-- POS Card -->
                  <div v-else-if="paymentMethod === 'pos'" class="py-3 text-center">
                    <div class="spinner-grow spinner-grow-sm text-info mb-2"></div>
                    <p class="mb-0 small fw-bold text-dark">Kết nối với thiết bị POS...</p>
                    <small class="text-muted text-xs">Vui lòng quẹt hoặc chạm thẻ ngân hàng.</small>
                  </div>
                </div>

                <!-- Action Button -->
                <button class="btn btn-premium btn-lg w-100 rounded-pill py-2.5 fw-bold shadow-sm" @click="confirmPayment">
                  <i class="bi bi-printer-fill me-2"></i> Thanh Toán & In Hóa Đơn
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Hidden K80 Print Receipt Layout -->
    <div id="printReceiptArea" v-if="invoice">
      <div style="text-align: center; margin-bottom: 4mm;">
        <h4 style="margin: 0; font-weight: 800; font-size: 1.1rem; text-transform: uppercase;">MYPET CLINIC</h4>
        <p style="margin: 1mm 0 0; font-size: 0.75rem;">Đ/C: 123 Đường Thú Y, Hà Nội<br>SĐT: 090 123 4567</p>
        <h5 style="margin: 3mm 0 0; font-weight: 800; font-size: 0.95rem; border-top: 1px dashed #000; border-bottom: 1px dashed #000; padding: 1.5mm 0; text-transform: uppercase;">Hóa Đơn Thanh Toán</h5>
      </div>

      <div style="font-size: 0.75rem; margin-bottom: 3mm; line-height: 1.4;">
        <div style="display: flex; justify-content: space-between;">
          <span>HĐ số: <strong>#{{ invoice.id }}</strong></span>
          <span>{{ new Date().toLocaleString('vi-VN') }}</span>
        </div>
        <div>Khách hàng: <strong>{{ invoice.customerName }}</strong></div>
        <div>SĐT: <span>{{ invoice.customerPhone }}</span></div>
        <div>Thú cưng: <strong>{{ invoice.petName }}</strong> ({{ invoice.petSpecies }})</div>
        <div>Bác sĩ: <span>{{ invoice.doctorName }}</span></div>
      </div>

      <table style="width: 100%; border-collapse: collapse; font-size: 0.75rem; margin-bottom: 4mm;">
        <thead>
          <tr style="border-bottom: 1px dashed #000; font-weight: 800;">
            <th style="text-align: left; padding: 1mm 0;">Tên</th>
            <th style="text-align: center; padding: 1mm 0; width: 10mm;">SL</th>
            <th style="text-align: right; padding: 1mm 0; width: 20mm;">Đơn giá</th>
            <th style="text-align: right; padding: 1mm 0; width: 22mm;">T.Tiền</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in invoice.items" :key="item.id" style="border-bottom: 1px dotted #ccc;">
            <td style="padding: 1.5mm 0;">{{ item.itemName }}</td>
            <td style="text-align: center; padding: 1.5mm 0;">{{ item.quantity }}</td>
            <td style="text-align: right; padding: 1.5mm 0;">{{ formatCurrency(item.unitPrice) }}</td>
            <td style="text-align: right; padding: 1.5mm 0;">{{ formatCurrency(item.totalPrice) }}</td>
          </tr>
        </tbody>
      </table>

      <div style="font-size: 0.75rem; border-top: 1px dashed #000; padding-top: 2mm; line-height: 1.5;">
        <div style="display: flex; justify-content: space-between;">
          <span>Tạm tính:</span>
          <span>{{ formatCurrency(invoice.subtotal) }}</span>
        </div>
        <div style="display: flex; justify-content: space-between;">
          <span>Giảm giá:</span>
          <span>{{ formatCurrency(discountAmount) }}</span>
        </div>
        <div style="display: flex; justify-content: space-between; font-weight: 800; font-size: 0.85rem; border-top: 1px solid #000; padding-top: 1mm; margin-top: 1mm;">
          <span>TỔNG CỘNG:</span>
          <span>{{ formatCurrency(finalTotal) }}</span>
        </div>
        <div style="display: flex; justify-content: space-between; font-size: 0.7rem; margin-top: 1mm;">
          <span>Hình thức:</span>
          <span>{{ paymentMethod === 'cash' ? 'Tiền mặt' : (paymentMethod === 'qr' ? 'Chuyển khoản VietQR' : 'POS Thẻ') }}</span>
        </div>
      </div>

      <div style="text-align: center; margin-top: 6mm; font-size: 0.7rem; border-top: 1px dashed #000; padding-top: 3mm;">
        <p style="margin: 0; font-style: italic;">Cảm ơn Quý khách & bé yêu!<br>Hẹn gặp lại!</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import api from '../../services/api';

const props = defineProps<{
  initialAppointmentId?: number
}>();

// State
const loadingQueue = ref(false);
const queueList = ref<any[]>([]);
const selectedAppointmentId = ref<number | null>(null);

const loadingInvoice = ref(false);
const invoice = ref<any>(null);

const catalogQuery = ref('');
const catalogResult = ref<any[]>([]);

const discountAmount = ref<number>(0);
const paymentMethod = ref<'cash' | 'qr' | 'pos'>('cash');
const cashReceived = ref<number>(0);

// Computed properties
const finalTotal = computed(() => {
  if (!invoice.value) return 0;
  return Math.max(0, invoice.value.subtotal - discountAmount.value);
});

const cashChange = computed(() => {
  return Math.max(0, cashReceived.value - finalTotal.value);
});

const vietQrUrl = computed(() => {
  if (!invoice.value) return '';
  const bankId = "VCB";
  const accountNo = "990123456789";
  const accountName = "MYPET CLINIC";
  const addInfo = encodeURIComponent(`THANH TOAN HD ${invoice.value.id}`);
  return `https://img.vietqr.io/image/${bankId}-${accountNo}-compact2.png?amount=${finalTotal.value}&addInfo=${addInfo}&accountName=${encodeURIComponent(accountName)}`;
});

// Methods
const loadPendingCheckouts = async () => {
  loadingQueue.value = true;
  try {
    const res = await api.get('/invoice/pending');
    queueList.value = res.data || [];
  } catch (err) {
    console.error(err);
  } finally {
    loadingQueue.value = false;
  }
};

const selectAppointment = async (apptId: number) => {
  selectedAppointmentId.value = apptId;
  loadingInvoice.value = true;
  invoice.value = null;
  catalogQuery.value = '';
  catalogResult.value = [];
  try {
    const res = await api.get(`/invoice/${apptId}`);
    invoice.value = res.data;
    discountAmount.value = invoice.value.discountAmount || 0;
    cashReceived.value = invoice.value.subtotal - discountAmount.value;
  } catch (err: any) {
    console.error(err);
    alert(err.response?.data?.message || 'Không thể lập hóa đơn cho ca này.');
    selectedAppointmentId.value = null;
  } finally {
    loadingInvoice.value = false;
  }
};

// Catalog quick search
let catalogDebounce: any = null;
const searchCatalog = () => {
  clearTimeout(catalogDebounce);
  catalogDebounce = setTimeout(async () => {
    const q = catalogQuery.value.trim();
    if (q.length < 2) {
      catalogResult.value = [];
      return;
    }
    try {
      const res = await api.get(`/invoice/catalog?query=${encodeURIComponent(q)}`);
      catalogResult.value = res.data || [];
    } catch (err) {
      console.error(err);
    }
  }, 300);
};

const addCatalogItem = async (type: string, itemId: number) => {
  if (!invoice.value) return;
  catalogQuery.value = '';
  catalogResult.value = [];
  try {
    const res = await api.post('/invoice/items', {
      invoiceId: invoice.value.id,
      itemType: type,
      itemId: itemId,
      quantity: 1
    });
    if (res.data.success) {
      invoice.value = res.data.invoice;
      recalculateTotal();
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Không thể thêm sản phẩm.');
  }
};

const changeQty = async (itemId: number, newQty: number) => {
  if (newQty < 1) return;
  try {
    const res = await api.put(`/invoice/items/${itemId}`, { quantity: newQty });
    if (res.data.success) {
      invoice.value = res.data.invoice;
      recalculateTotal();
    }
  } catch (err) {
    console.error(err);
  }
};

const handleManualQtyChange = (itemId: number, valStr: string) => {
  const qty = parseInt(valStr);
  if (!isNaN(qty) && qty > 0) {
    changeQty(itemId, qty);
  }
};

const removeItem = async (itemId: number) => {
  try {
    const res = await api.delete(`/invoice/items/${itemId}`);
    if (res.data.success) {
      invoice.value = res.data.invoice;
      recalculateTotal();
    }
  } catch (err) {
    console.error(err);
  }
};

const recalculateTotal = () => {
  if (discountAmount.value < 0) discountAmount.value = 0;
  if (invoice.value && discountAmount.value > invoice.value.subtotal) {
    discountAmount.value = invoice.value.subtotal;
  }
  cashReceived.value = finalTotal.value;
};

// Process transaction
const confirmPayment = async () => {
  if (!invoice.value) return;

  if (paymentMethod.value === 'cash' && cashReceived.value < finalTotal.value) {
    alert('Số tiền khách đưa chưa đủ!');
    return;
  }

  const ok = confirm(`Xác nhận thanh toán hóa đơn trị giá ${finalTotal.value.toLocaleString('vi-VN')}đ?`);
  if (!ok) return;

  try {
    const res = await api.post(`/invoice/${invoice.value.id}/process-payment`, {
      paymentMethod: paymentMethod.value,
      discountAmount: discountAmount.value
    });
    
    if (res.data.success) {
      // Trigger browser K80 print
      setTimeout(() => {
        window.print();
      }, 300);

      alert('Thanh toán hoàn tất & đã in hóa đơn!');
      selectedAppointmentId.value = null;
      invoice.value = null;
      await loadPendingCheckouts();
    } else {
      alert('Giao dịch thanh toán thất bại.');
    }
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi xác nhận thanh toán.');
  }
};

// Helpers
const getAnimalEmoji = (species: string) => {
  const s = (species || '').toLowerCase();
  if (s.includes('chó') || s.includes('dog')) return '🐶';
  if (s.includes('mèo') || s.includes('cat')) return '🐱';
  return '🐾';
};

const getLastWord = (name: string) => {
  if (!name) return '—';
  const parts = name.trim().split(/\s+/);
  return parts[parts.length - 1];
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleDateString('vi-VN');
};

const formatCurrency = (val: number) => {
  if (val === undefined || val === null) return '0đ';
  return val.toLocaleString('vi-VN') + 'đ';
};

// Watch initial id prop for auto-selection
watch(() => props.initialAppointmentId, (newVal) => {
  if (newVal) {
    selectAppointment(newVal);
  }
}, { immediate: true });

onMounted(() => {
  loadPendingCheckouts();
  if (props.initialAppointmentId) {
    selectAppointment(props.initialAppointmentId);
  }
});
</script>

<script lang="ts">
export default {
  name: 'InvoicesTab'
}
</script>

<style scoped>
.checkout-queue-card {
  transition: all 0.2s ease;
  cursor: pointer;
  border: 1px solid #edf2f7 !important;
}
.checkout-queue-card:hover {
  background-color: #fdfaf0;
  border-color: var(--primary-gold) !important;
}
.active-card {
  background-color: #fffbeb !important;
  border-color: var(--primary-gold) !important;
  border-left: 5px solid var(--primary-gold) !important;
}

.input-premium {
  border: 1px solid rgba(0,0,0,0.08);
}
.input-premium:focus {
  border-color: var(--primary-gold);
  box-shadow: 0 0 0 3px rgba(245, 158, 11, 0.15);
}

.payment-method-card {
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.payment-method-card:hover {
  border-color: #cbd5e1;
  background-color: #f8fafc;
}
.active-method {
  border-color: var(--primary-gold) !important;
  background-color: #fffbeb !important;
}

.qr-code-border {
  border: 2px dashed var(--primary-gold);
}

.border-start-md {
  border-left: 1px solid #edf2f7;
}

@media (max-width: 576px) {
  .border-start-md {
    border-left: none;
    border-top: 1px solid #edf2f7;
    padding-top: 10px;
    margin-top: 10px;
  }
}

.animate-fade-in {
  animation: fadeIn 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* K80 Print CSS styles */
#printReceiptArea {
  display: none;
}

@media print {
  body * {
    visibility: hidden;
  }
  #printReceiptArea, #printReceiptArea * {
    visibility: visible;
  }
  #printReceiptArea {
    display: block !important;
    position: absolute;
    left: 0;
    top: 0;
    width: 80mm;
    padding: 3mm;
    background: white;
    color: black;
  }
  @page {
    size: 80mm auto;
    margin: 0;
  }
}
</style>
