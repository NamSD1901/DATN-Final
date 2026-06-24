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

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import api from '../../services/api';
import Swal from 'sweetalert2';

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
    Swal.fire({ icon: 'warning', title: 'Chưa đủ tiền!', text: 'Số tiền khách đưa chưa đủ để thanh toán.', confirmButtonColor: '#f59e0b' });
    return;
  }

  const result = await Swal.fire({
    title: 'Xác nhận thanh toán?',
    html: `Tổng cần thu: <strong class="text-primary">${finalTotal.value.toLocaleString('vi-VN')}đ</strong><br>Hình thức: <strong>${paymentMethod.value === 'cash' ? 'Tiền mặt' : (paymentMethod.value === 'qr' ? 'VietQR' : 'POS Thẻ')}</strong>`,
    icon: 'question',
    showCancelButton: true,
    confirmButtonText: '✅ Xác nhận & In hóa đơn',
    cancelButtonText: 'Huỷ',
    confirmButtonColor: '#f59e0b',
    cancelButtonColor: '#94a3b8',
  });
  if (!result.isConfirmed) return;

  try {
    const res = await api.post(`/invoice/${invoice.value.id}/process-payment`, {
      paymentMethod: paymentMethod.value,
      discountAmount: discountAmount.value
    });
    
    if (res.data.success) {
      printInvoiceWindow();
      Swal.fire({ icon: 'success', title: 'Thanh toán thành công!', text: 'Hóa đơn đang được in.', timer: 2000, showConfirmButton: false });
      selectedAppointmentId.value = null;
      invoice.value = null;
      await loadPendingCheckouts();
    } else {
      Swal.fire({ icon: 'error', title: 'Thất bại', text: 'Giao dịch thanh toán thất bại.', confirmButtonColor: '#f59e0b' });
    }
  } catch (err: any) {
    Swal.fire({ icon: 'error', title: 'Lỗi hệ thống', text: err.response?.data?.message || 'Có lỗi xảy ra khi xác nhận thanh toán.', confirmButtonColor: '#f59e0b' });
  }
};

// ─── Print: open isolated window with full invoice HTML ───────────────────
const printInvoiceWindow = () => {
  if (!invoice.value) return;
  const inv = invoice.value;
  const discount = discountAmount.value;
  const total = Math.max(0, inv.subtotal - discount);
  const payLabel = paymentMethod.value === 'cash' ? 'Tiền mặt' : (paymentMethod.value === 'qr' ? 'Chuyển khoản VietQR' : 'POS / Thẻ ngân hàng');
  const now = new Date().toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  const fmtCur = (v: number) => (v ?? 0).toLocaleString('vi-VN') + 'đ';

  const itemsHtml = (inv.items || []).map((item: any, idx: number) => `
    <tr>
      <td style="text-align:center;color:#94a3b8;padding:9px 12px">${idx + 1}</td>
      <td style="padding:9px 12px">
        <div style="font-weight:600">${item.itemName}</div>
        <div style="font-size:0.72rem;color:#94a3b8">${item.itemType === 'service' ? 'Dịch vụ y tế' : 'Thuốc / Vật tư'}</div>
      </td>
      <td style="text-align:center;padding:9px 12px">${item.quantity}</td>
      <td style="text-align:right;padding:9px 12px">${fmtCur(item.unitPrice)}</td>
      <td style="text-align:right;padding:9px 12px;font-weight:700">${fmtCur(item.totalPrice)}</td>
    </tr>`).join('');

  const discountRow = discount > 0 ? `<tr style="border-bottom:1px dashed #e2e8f0"><td style="padding:5px 0;font-size:0.82rem;color:#475569">Giảm giá</td><td style="padding:5px 0;text-align:right;color:#ef4444">- ${fmtCur(discount)}</td></tr>` : '';

  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <title>Hóa đơn #INV-${String(inv.id).padStart(5,'0')}</title>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700;800&display=swap');
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: 'Be Vietnam Pro', sans-serif; color: #0f172a; font-size: 13px; line-height: 1.5; padding: 20mm 18mm; background: white; }
    .header { display: flex; justify-content: space-between; align-items: center; padding-bottom: 14px; border-bottom: 3px solid #f59e0b; margin-bottom: 18px; }
    .logo-block { display: flex; align-items: center; gap: 14px; }
    .logo-circle { width: 54px; height: 54px; background: linear-gradient(135deg,#fef08a,#f59e0b); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 1.6rem; }
    .clinic-name { font-size: 1.25rem; font-weight: 800; letter-spacing: 1px; }
    .clinic-sub { font-size: 0.72rem; color: #64748b; margin-top: 3px; }
    .inv-title { font-size: 1.05rem; font-weight: 800; color: #f59e0b; text-transform: uppercase; letter-spacing: 1px; text-align: right; }
    .inv-meta { font-size: 0.78rem; color: #64748b; text-align: right; margin-top: 4px; }
    .info-row { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 12px; margin-bottom: 20px; }
    .info-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 10px 14px; }
    .info-label { font-size: 0.62rem; font-weight: 700; text-transform: uppercase; color: #94a3b8; letter-spacing: 0.5px; margin-bottom: 5px; }
    .info-line { font-size: 0.78rem; color: #334155; }
    table.items { width: 100%; border-collapse: collapse; margin-bottom: 16px; }
    table.items thead tr { background: #0f172a; color: white; }
    table.items thead th { padding: 9px 12px; font-weight: 600; text-align: left; font-size: 0.72rem; letter-spacing: 0.3px; }
    table.items tbody tr { border-bottom: 1px solid #f1f5f9; }
    table.items tbody tr:nth-child(even) { background: #f8fafc; }
    .totals { margin-left: auto; width: 280px; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px 16px; background: #f8fafc; margin-bottom: 24px; }
    .totals table { width: 100%; }
    .total-final td { font-weight: 800; font-size: 1rem; color: #0f172a; border-top: 2px solid #0f172a; padding-top: 8px !important; }
    .footer { display: flex; justify-content: space-between; align-items: flex-end; border-top: 1px dashed #cbd5e1; padding-top: 16px; margin-top: 8px; }
    .sign-line { border-bottom: 1px solid #0f172a; height: 40px; margin: 8px 0; }
    @page { size: A4 portrait; margin: 0; }
  </style>
</head>
<body>
  <div class="header">
    <div class="logo-block">
      <div class="logo-circle">🐾</div>
      <div>
        <div class="clinic-name">MYPET CLINIC</div>
        <div class="clinic-sub">Hệ thống phòng khám thú y cao cấp</div>
      </div>
    </div>
    <div>
      <div class="inv-title">HÓA ĐƠN DỊCH VỤ</div>
      <div class="inv-meta">Số: <strong>#INV-${String(inv.id).padStart(5,'0')}</strong></div>
      <div class="inv-meta">Ngày: ${now}</div>
    </div>
  </div>

  <div class="info-row">
    <div class="info-box">
      <div class="info-label">THÔNG TIN KHÁCH HÀNG</div>
      <div class="info-line"><strong>${inv.customerName || '—'}</strong></div>
      <div class="info-line">Điện thoại: ${inv.customerPhone || 'N/A'}</div>
    </div>
    <div class="info-box">
      <div class="info-label">THÔNG TIN BỆNH NHÂN</div>
      <div class="info-line"><strong>${inv.petName || '—'}</strong> (${inv.petSpecies || 'Thú cưng'})</div>
      <div class="info-line">Bác sĩ phụ trách: ${inv.doctorName || '—'}</div>
    </div>
    <div class="info-box">
      <div class="info-label">PHÒNG KHÁM</div>
      <div class="info-line">MyPet Clinic - 123 Đường Thú Y</div>
      <div class="info-line">Hà Nội | SĐT: 090 123 4567</div>
    </div>
  </div>

  <table class="items">
    <thead><tr>
      <th style="width:40px">STT</th>
      <th>Mô tả dịch vụ / sản phẩm</th>
      <th style="text-align:center;width:55px">SL</th>
      <th style="text-align:right;width:110px">Đơn giá</th>
      <th style="text-align:right;width:120px">Thành tiền</th>
    </tr></thead>
    <tbody>${itemsHtml}</tbody>
  </table>

  <div class="totals">
    <table>
      <tr style="border-bottom:1px dashed #e2e8f0"><td style="padding:5px 0;font-size:0.82rem;color:#475569">Tạm tính</td><td style="padding:5px 0;text-align:right;font-size:0.82rem">${fmtCur(inv.subtotal)}</td></tr>
      ${discountRow}
      <tr class="total-final"><td style="padding:5px 0">TỔNG CỘNG</td><td style="text-align:right;padding:5px 0">${fmtCur(total)}</td></tr>
      <tr><td style="padding:4px 0;font-size:0.75rem;color:#64748b" colspan="2">Hình thức: ${payLabel}</td></tr>
    </table>
  </div>

  <div class="footer">
    <div style="flex:1;font-size:0.78rem;color:#475569">
      <div style="font-weight:700;margin-bottom:4px">Ghi chú:</div>
      <div>Hóa đơn này là bằng chứng thanh toán hợp lệ tại MyPet Clinic.</div>
      <div>Cảm ơn quý khách đã tin tưởng sử dụng dịch vụ!</div>
    </div>
    <div style="width:160px;text-align:center">
      <div style="font-weight:700;margin-bottom:4px">Xác nhận của phòng khám</div>
      <div class="sign-line"></div>
      <div style="font-size:0.75rem;color:#64748b">Thu ngân</div>
    </div>
  </div>

  <script>window.onload = function(){ window.print(); window.onafterprint = function(){ window.close(); }; }<\/script>
</body>
</html>`;

  const pw = window.open('', '_blank', 'width=900,height=650');
  if (pw) {
    pw.document.write(html);
    pw.document.close();
  }
};
// ──────────────────────────────────────────────────────────────────────────

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

/* A4 Invoice Print Styles */
#printReceiptArea {
  display: none;
  font-family: 'Be Vietnam Pro', 'Inter', sans-serif;
  color: #0f172a;
  font-size: 0.85rem;
  line-height: 1.5;
}

@media print {
  body * { visibility: hidden; }
  #printReceiptArea, #printReceiptArea * { visibility: visible; }
  #printReceiptArea {
    display: block !important;
    position: absolute;
    left: 0; top: 0;
    width: 100%;
    padding: 16mm 18mm;
    background: white;
    color: #0f172a;
  }
  @page { size: A4 portrait; margin: 0; }
}

/* Invoice Layout Classes */
.inv-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 12px;
  border-bottom: 3px solid #f59e0b;
  margin-bottom: 16px;
}
.inv-logo-block {
  display: flex;
  align-items: center;
  gap: 12px;
}
.inv-logo-circle {
  width: 52px;
  height: 52px;
  background: linear-gradient(135deg, #fef08a, #f59e0b);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}
.inv-clinic-name {
  font-size: 1.3rem;
  font-weight: 800;
  letter-spacing: 1px;
  color: #0f172a;
}
.inv-clinic-sub {
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 2px;
}
.inv-title-block {
  text-align: right;
}
.inv-title {
  font-size: 1.1rem;
  font-weight: 800;
  color: #f59e0b;
  text-transform: uppercase;
  letter-spacing: 1px;
}
.inv-number, .inv-date {
  font-size: 0.8rem;
  color: #64748b;
  margin-top: 3px;
}

.inv-info-row {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 12px;
  margin-bottom: 20px;
}
.inv-info-box {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 10px 14px;
}
.inv-info-label {
  font-size: 0.65rem;
  font-weight: 700;
  text-transform: uppercase;
  color: #94a3b8;
  letter-spacing: 0.5px;
  margin-bottom: 5px;
}
.inv-info-line {
  font-size: 0.8rem;
  color: #334155;
  line-height: 1.5;
}

.inv-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 16px;
  font-size: 0.82rem;
}
.inv-table thead tr {
  background: #0f172a;
  color: white;
}
.inv-table thead th {
  padding: 9px 12px;
  font-weight: 600;
  text-align: left;
  font-size: 0.75rem;
  letter-spacing: 0.3px;
}
.inv-table tbody tr {
  border-bottom: 1px solid #f1f5f9;
}
.inv-table tbody tr:nth-child(even) {
  background: #f8fafc;
}
.inv-table tbody td {
  padding: 9px 12px;
  vertical-align: middle;
}

.inv-totals {
  margin-left: auto;
  width: 280px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 12px 16px;
  background: #f8fafc;
  margin-bottom: 24px;
}
.inv-totals-row {
  display: flex;
  justify-content: space-between;
  padding: 5px 0;
  border-bottom: 1px dashed #e2e8f0;
  font-size: 0.82rem;
  color: #475569;
}
.inv-totals-final {
  font-weight: 800;
  font-size: 1rem !important;
  color: #0f172a !important;
  border-top: 2px solid #0f172a;
  border-bottom: none;
  padding-top: 8px;
  margin-top: 4px;
}

.inv-footer {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  border-top: 1px dashed #cbd5e1;
  padding-top: 16px;
  margin-top: 8px;
}
.inv-footer-left {
  flex: 1;
  font-size: 0.8rem;
  color: #475569;
}
.inv-footer-right {
  width: 160px;
}
.inv-sign-line {
  border-bottom: 1px solid #0f172a;
  height: 40px;
  margin: 8px 0;
}
</style>
