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
                
                <div class="mb-3">
                  <div class="input-group input-group-sm">
                    <input type="text" class="form-control" placeholder="Mã giảm giá (nếu có)" v-model="voucherCode">
                    <button class="btn btn-outline-warning" @click="applyVoucher" :disabled="!voucherCode || loadingVoucher">
                      <span v-if="loadingVoucher" class="spinner-border spinner-border-sm"></span>
                      <span v-else>Áp dụng</span>
                    </button>
                  </div>
                  <div v-if="voucherMessage" class="small mt-1" :class="voucherError ? 'text-danger' : 'text-success'">
                    {{ voucherMessage }}
                  </div>
                </div>

                <div v-if="discountAmount > 0" class="d-flex justify-content-between mb-2 small text-danger">
                  <span>Giảm giá (Voucher):</span>
                  <span class="fw-bold">-{{ formatCurrency(discountAmount) }}</span>
                </div>
                
                <hr class="my-3">

                <div class="d-flex justify-content-between align-items-center mb-3">
                  <span class="fw-bold text-dark">TỔNG CẦN THU:</span>
                  <span class="fw-extrabold text-primary fs-4">{{ formatCurrency(finalTotal) }}</span>
                </div>

                <!-- Chọn phương thức thanh toán -->
                <div class="mb-3 d-flex gap-2">
                  <button 
                    class="btn flex-fill fw-bold rounded-pill" 
                    :class="paymentMethod === 'cash' ? 'btn-success' : 'btn-outline-secondary'"
                    @click="paymentMethod = 'cash'"
                  >
                    <i class="bi bi-cash-coin me-1"></i> Tiền mặt
                  </button>
                  <button 
                    class="btn flex-fill fw-bold rounded-pill" 
                    :class="paymentMethod === 'qr' ? 'btn-primary' : 'btn-outline-secondary'"
                    @click="paymentMethod = 'qr'"
                  >
                    <i class="bi bi-qr-code-scan me-1"></i> Chuyển khoản
                  </button>
                </div>

                <!-- Cash received thối tiền -->
                <div v-if="paymentMethod === 'cash'" class="p-3 bg-white rounded-3 border mb-4 text-start animate-fade-in">
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

                <!-- Giao diện quét mã QR tự động -->
                <div v-if="paymentMethod === 'qr'" class="p-3 bg-white rounded-4 mb-4 text-center animate-fade-in shadow-sm position-relative overflow-hidden"
                  :class="notificationStore.isConnected ? 'border border-success' : 'border border-warning'"
                >
                  <!-- Header LIVE indicator -->
                  <div class="position-absolute top-0 start-0 w-100 py-1 fw-bold small d-flex align-items-center justify-content-center gap-2"
                    :class="notificationStore.isConnected ? 'bg-success bg-opacity-10 text-success' : 'bg-warning bg-opacity-10 text-warning'"
                  >
                    <span v-if="notificationStore.isConnected" class="d-flex align-items-center gap-1">
                      <span class="live-dot"></span>
                      <span>LIVE — Đang chờ thanh toán tự động</span>
                    </span>
                    <span v-else class="d-flex align-items-center gap-1">
                      <i class="bi bi-exclamation-triangle-fill"></i>
                      <span>Mất kết nối realtime — Đang thử lại...</span>
                    </span>
                  </div>
                  <div class="mt-4 mb-2 mx-auto bg-light rounded-4 p-2" style="width: 220px; height: 220px; border: 2px dashed #93c5fd;">
                    <img v-if="vietQrUrl" :src="vietQrUrl" alt="VietQR" class="w-100 h-100 object-fit-contain rounded-3" />
                  </div>
                  <div class="small fw-bold text-dark mb-1">Ngân hàng: <span class="text-primary">TPBank</span></div>
                  <div class="small fw-bold text-dark mb-1">STK: <span class="text-primary fw-bolder">00001562694</span></div>
                  <div class="small fw-bold text-dark mb-1">Chủ tài khoản: <span class="text-primary">CAO HA PHUONG</span></div>
                  <div class="small text-muted mt-2" style="font-size: 0.75rem;">
                    Mã GD: <strong class="text-primary bg-primary bg-opacity-10 px-1 rounded">MPC{{ invoice.id }}</strong><br>
                    Khách quét xong hệ thống sẽ tự động xuất hóa đơn.
                  </div>

                  <!-- Nút giả lập - chỉ hiện khi test local (Development) -->
                  <div v-if="isDev" class="mt-3 pt-3 border-top border-dashed">
                    <button 
                      class="btn btn-sm btn-outline-secondary w-100 rounded-pill fw-bold" 
                      style="font-size: 0.75rem; border-style: dashed;"
                      @click="simulatePayment"
                      :disabled="simulatingPayment"
                    >
                      <span v-if="simulatingPayment"><i class="bi bi-arrow-repeat spin me-1"></i> Đang xử lý...</span>
                      <span v-else><i class="bi bi-bug me-1"></i> [🧪 Dev] Giả lập thanh toán thành công</span>
                    </button>
                    <div class="text-muted mt-1" style="font-size: 0.65rem;">Chỉ hiện ở chế độ lập trình (localhost)</div>
                  </div>
                </div>

                <!-- Print Options -->
                <div class="mb-4">
                  <h6 class="fw-bold text-dark mb-2" style="font-size: 0.85rem;">Tùy chọn in ấn</h6>
                  <div class="d-flex flex-column gap-2">
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" id="printInvoiceCb" v-model="printInvoiceOpt">
                      <label class="form-check-label small fw-bold text-dark cursor-pointer" for="printInvoiceCb">In Hóa đơn thanh toán</label>
                    </div>
                    <div class="form-check form-switch">
                      <input class="form-check-input" type="checkbox" id="printMedicalCb" v-model="printMedicalRecordOpt">
                      <label class="form-check-label small fw-bold text-dark cursor-pointer" for="printMedicalCb">In Bệnh án & Đơn thuốc (Premium)</label>
                    </div>
                  </div>
                </div>

                <!-- Action Button -->
                <button v-if="paymentMethod === 'cash'" class="btn btn-premium btn-lg w-100 rounded-pill py-2.5 fw-bold shadow-sm" @click="confirmPayment">
                  <i class="bi bi-wallet-fill me-2"></i> Xác nhận & Thu tiền
                </button>
                <button v-else class="btn btn-primary btn-lg w-100 rounded-pill py-2.5 fw-bold shadow-sm animate-pulse" style="pointer-events: none;">
                  <i class="bi bi-arrow-repeat spin me-2"></i> Đang chờ khách quét QR...
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
import { useNotificationStore } from '../../stores/notification.store';

const props = defineProps<{
  initialAppointmentId?: number
}>();

const notificationStore = useNotificationStore();

// Chỉ hiện nút giả lập khi chạy local (localhost)
const isDev = ref(window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1');
const simulatingPayment = ref(false);

// State
const loadingQueue = ref(false);
const queueList = ref<any[]>([]);
const selectedAppointmentId = ref<number | null>(null);

const loadingInvoice = ref(false);
const invoice = ref<any>(null);

const catalogQuery = ref('');
const catalogResult = ref<any[]>([]);

const discountAmount = ref<number>(0);
const paymentMethod = ref<'cash' | 'qr'>('cash');
const cashReceived = ref<number>(0);

const voucherCode = ref('');
const voucherMessage = ref('');
const voucherError = ref(false);
const loadingVoucher = ref(false);

const printInvoiceOpt = ref<boolean>(true);
const printMedicalRecordOpt = ref<boolean>(false);

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
  const bankId = "TPB"; // TPBank
  const accountNo = "00001562694";
  const accountName = "CAO HA PHUONG";
  const addInfo = encodeURIComponent(`MPC${invoice.value.id}`);
  return `https://img.vietqr.io/image/${bankId}-${accountNo}-compact2.png?amount=${finalTotal.value}&addInfo=${addInfo}&accountName=${encodeURIComponent(accountName)}`;
});

// Watch for automatic SePay payment events
watch(() => notificationStore.lastSePayEvent, async (newVal) => {
  if (newVal && invoice.value && newVal.invoiceId === invoice.value.id) {
    // 1. Lưu snapshot toàn bộ dữ liệu TRƯỚC KHI Swal mở (tránh race condition khi invoice.value bị null sau đó)
    const invoiceSnapshot = { ...invoice.value, items: [...(invoice.value.items || [])] };
    const apptIdSnapshot = selectedAppointmentId.value;

    // 2. Fetch dữ liệu bổ sung ngay bây giờ (khi dữ liệu còn đầy đủ)
    let apptData = null;
    let soapData = null;
    if (printMedicalRecordOpt.value && apptIdSnapshot) {
      try {
        const apptRes = await api.get(`/appointment/${apptIdSnapshot}`);
        apptData = apptRes.data;
        const soapRes = await api.get(`/medical-records/soap/appointment/${apptIdSnapshot}`);
        soapData = soapRes.data;
      } catch (err) { }
    }

    // 3. Hiển thị Swal thông báo thành công với nút xác nhận - chờ người dùng bấm
    const result = await Swal.fire({ 
      title: '🎉 Thanh toán thành công!', 
      html: `
        <div style="font-size:1rem; color:#374151;">
          ${newVal.message || 'Khách hàng vừa thanh toán thành công qua chuyển khoản QR!'}
        </div>
        <div style="margin-top:12px; padding:10px; background:#f0fdf4; border-radius:8px; font-size:0.85rem; color:#166534;">
          <i class="bi bi-check-circle-fill me-1"></i>
          Hóa đơn đã được ghi nhận tự động vào hệ thống
        </div>
      `,
      icon: 'success', 
      showConfirmButton: true,
      confirmButtonText: 'In hóa đơn & Hoàn tất',
      confirmButtonColor: '#3b82f6',
      showCancelButton: true,
      cancelButtonText: 'Bỏ qua, không in',
      cancelButtonColor: '#9ca3af',
      timer: 30000,
      timerProgressBar: true,
      allowOutsideClick: false,
      backdrop: `
        rgba(0,0,123,0.4)
        url("/fireworks.gif")
        left top
        no-repeat
      `
    });

    // 4. In hóa đơn nếu người dùng bấm xác nhận (không bấm Cancel / không để timeout)
    if (result.isConfirmed) {
      printInvoiceWindow(invoiceSnapshot, apptData, soapData, printInvoiceOpt.value, printMedicalRecordOpt.value);
    }
    
    // 5. Dọn dẹp UI sau khi người dùng đã bấm (dù Confirm hay Cancel)
    selectedAppointmentId.value = null;
    invoice.value = null;
    await loadPendingCheckouts();
  }
}, { deep: true });

// Giả lập thanh toán (chỉ môi trường Development)
const simulatePayment = async () => {
  if (!invoice.value) return;
  simulatingPayment.value = true;
  try {
    await api.post(`/Webhooks/simulate/${invoice.value.id}`);
    // SignalR sẽ tự khởi động watch và xử lý tiếp — không cần làm gì thêm
  } catch (err: any) {
    Swal.fire({ icon: 'error', title: 'Lỗi giả lập', text: err.response?.data?.message || 'Không thể gọi simulate endpoint.', confirmButtonColor: '#f59e0b' });
  } finally {
    simulatingPayment.value = false;
  }
};

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
  voucherCode.value = '';
  voucherMessage.value = '';
  voucherError.value = false;
  
  try {
    const res = await api.get(`/invoice/${apptId}`);
    invoice.value = res.data;
    discountAmount.value = invoice.value.discountAmount || 0;
    
    // Auto-extract voucher from Appointment note
    try {
      const apptRes = await api.get(`/appointment/${apptId}`);
      if (apptRes.data?.note) {
        const match = apptRes.data.note.match(/\[Áp dụng voucher:\s*([^\]]+)\]/i);
        if (match) {
          voucherCode.value = match[1].trim();
          await applyVoucher();
        }
      }
    } catch (e) { }

    cashReceived.value = finalTotal.value;
  } catch (err: any) {
    console.error(err);
    Swal.fire({ 
      icon: 'error', 
      title: 'Lỗi', 
      text: err.response?.data?.message || 'Không thể lập hóa đơn cho ca này.',
      confirmButtonColor: '#3b82f6'
    });
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
    Swal.fire({
      icon: 'warning',
      title: 'Không thể thêm sản phẩm',
      text: err.response?.data?.message || 'Có lỗi xảy ra khi thêm sản phẩm.',
      confirmButtonColor: '#f59e0b'
    });
  }
};

let qtyTimeouts: Record<number, any> = {};

const changeQty = (itemId: number, newQty: number) => {
  if (newQty < 1) return;

  // Optimistic UI update
  if (invoice.value && invoice.value.items) {
    const item = invoice.value.items.find((i: any) => i.id === itemId);
    if (item) {
      item.quantity = newQty;
      item.totalPrice = newQty * item.unitPrice;
      recalculateTotal();
    }
  }

  if (qtyTimeouts[itemId]) clearTimeout(qtyTimeouts[itemId]);

  qtyTimeouts[itemId] = setTimeout(async () => {
    try {
      const res = await api.put(`/invoice/items/${itemId}`, { quantity: newQty });
      if (res.data.success) {
        invoice.value = res.data.invoice;
        recalculateTotal();
      }
    } catch (err: any) {
      console.error(err);
      Swal.fire({
        icon: 'warning',
        title: 'Không thể cập nhật số lượng',
        text: err.response?.data?.message || 'Có lỗi xảy ra khi cập nhật số lượng.',
        confirmButtonColor: '#f59e0b'
      });
      if (selectedAppointmentId.value) {
        selectAppointment(selectedAppointmentId.value); // rollback
      }
    }
  }, 400);
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
  
  // Re-validate voucher if items changed
  if (voucherCode.value && !voucherError.value) {
    applyVoucher();
  }
};

const applyVoucher = async () => {
  if (!voucherCode.value || !invoice.value) return;
  loadingVoucher.value = true;
  voucherMessage.value = 'Đang kiểm tra...';
  voucherError.value = false;
  try {
    const serviceItems = invoice.value.items.filter((i: any) => i.itemType === 'service');
    const serviceIds = serviceItems.map((i: any) => i.itemId);
    
    // Create a dictionary of ServiceId -> TotalPrice for that service
    const servicePrices: Record<number, number> = {};
    serviceItems.forEach((i: any) => {
      servicePrices[i.itemId] = i.totalPrice;
    });

    const res = await api.post('/offers/validate', {
      code: voucherCode.value.trim(),
      orderAmount: invoice.value.subtotal,
      serviceIds: serviceIds,
      servicePrices: servicePrices
    });
    
    if (res.data.success && res.data.data.isValid) {
      discountAmount.value = res.data.data.discountAmount;
      voucherMessage.value = `Áp dụng thành công (-${formatCurrency(discountAmount.value)})`;
    } else {
      voucherError.value = true;
      discountAmount.value = 0;
      voucherMessage.value = res.data.data?.message || res.data.message || 'Mã giảm giá không hợp lệ.';
    }
  } catch (err: any) {
    voucherError.value = true;
    discountAmount.value = 0;
    voucherMessage.value = err.response?.data?.message || 'Lỗi khi kiểm tra mã.';
  } finally {
    loadingVoucher.value = false;
    cashReceived.value = finalTotal.value;
  }
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
    html: `Tổng cần thu: <strong class="text-primary">${finalTotal.value.toLocaleString('vi-VN')}đ</strong><br>Hình thức: <strong>${paymentMethod.value === 'cash' ? 'Tiền mặt' : 'VietQR'}</strong>`,
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
      discountAmount: discountAmount.value,
      voucherCode: voucherCode.value ? voucherCode.value.trim() : null
    });
    
    if (res.data.success) {
      // Lấy dữ liệu bệnh án nếu người dùng có chọn in bệnh án
      let apptData = null;
      let soapData = null;
      let isVaccinationRecord = false;
      if (printMedicalRecordOpt.value && selectedAppointmentId.value) {
        try {
          const apptRes = await api.get(`/appointment/${selectedAppointmentId.value}`);
          apptData = apptRes.data;
        } catch (err) {
          console.error('Không thể lấy chi tiết bệnh án', err);
        }
        // Thử lấy SOAP khám bệnh trước
        try {
          const soapRes = await api.get(`/medical-records/soap/appointment/${selectedAppointmentId.value}`);
          soapData = soapRes.data;
        } catch {
          // Nếu không có SOAP khám bệnh → thử SOAP tiêm chủng
          try {
            const vaccRes = await api.get(`/vaccinations/appointments/${selectedAppointmentId.value}`);
            if (vaccRes.data) {
              soapData = vaccRes.data;
              isVaccinationRecord = true;
            }
          } catch {
            console.warn('Không tìm thấy dữ liệu SOAP (cả khám bệnh lẫn tiêm chủng)');
          }
        }
      }

      printInvoiceWindow(invoice.value, apptData, soapData, printInvoiceOpt.value, printMedicalRecordOpt.value, isVaccinationRecord);
      
      Swal.fire({ icon: 'success', title: 'Thanh toán thành công!', text: 'Đang xử lý in tài liệu.', timer: 2000, showConfirmButton: false });
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

// ─── Print: open isolated window with full HTML ─────────────────────
const printInvoiceWindow = (inv: any, appt: any, soap: any, optInvoice: boolean, optMedical: boolean, isVaccination: boolean = false) => {
  if (!inv) return;
  const discount = discountAmount.value;
  const total = Math.max(0, inv.subtotal - discount);
  const payLabel = paymentMethod.value === 'cash' ? 'Tiền mặt' : 'Chuyển khoản VietQR';
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

  // Generate Medical Record Section HTML
  let medicalRecordHtml = '';
  if (optMedical && appt) {
    const medicines = (inv.items || []).filter((i: any) => i.itemType === 'medicine');
    const services = (inv.items || []).filter((i: any) => i.itemType === 'service');
    
    const prescriptionsHtml = medicines.map((m: any, idx: number) => {
      const pDetail = soap?.plan?.prescriptions?.find((p: any) => p.medicineId === m.itemId);
      let detailsHtml = '';
      if (pDetail) {
        detailsHtml = `
          <div style="font-size: 0.78rem; color: #475569; margin-top: 6px; border-top: 1px dashed #e2e8f0; padding-top: 6px;">
            ${pDetail.dosage ? `<div style="margin-bottom: 2px;">Liều dùng: <strong style="color: #0f172a;">${pDetail.dosage}</strong></div>` : ''}
            ${pDetail.frequency ? `<div style="margin-bottom: 2px;">Tần suất: <strong style="color: #0f172a;">${pDetail.frequency}</strong></div>` : ''}
            ${pDetail.durationDays ? `<div style="margin-bottom: 2px;">Liệu trình: <strong style="color: #0f172a;">${pDetail.durationDays} ngày</strong></div>` : ''}
            ${pDetail.instruction ? `<div><em>${pDetail.instruction}</em></div>` : ''}
          </div>
        `;
      }
      return `
      <div class="rx-item">
        <div class="rx-name">${idx + 1}. ${m.itemName}</div>
        <div class="rx-qty">Cấp phát: <strong>${m.quantity}</strong></div>
        ${detailsHtml}
      </div>`;
    }).join('');

    const servicesHtml = services.map((s: any, idx: number) => `
      <div style="font-size: 0.85rem; color: #334155; margin-bottom: 4px;">
        <i class="bi bi-check2-circle text-success me-1"></i> ${s.itemName}
      </div>`).join('');

    // Attachments (Images)
    let imagesHtml = '';
    if (soap?.objective?.attachments && soap.objective.attachments.length > 0) {
      const baseUrl = api.defaults.baseURL?.replace('/api', '') || '';
      const imgs = soap.objective.attachments.map((url: string) => `
        <img src="${baseUrl}${url}" style="width: 140px; height: 140px; object-fit: cover; border-radius: 8px; border: 1px solid #cbd5e1; box-shadow: 0 2px 4px rgba(0,0,0,0.05);" />
      `).join('');
      imagesHtml = `
      <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
        <div class="clinical-label" style="margin-bottom: 8px;"><i class="bi bi-images"></i> Hình ảnh Cận lâm sàng:</div>
        <div style="display: flex; gap: 10px; flex-wrap: wrap;">${imgs}</div>
      </div>
      `;
    }

    // System Exams (Objective)
    let sysExamsHtml = '';
    if (soap?.objective) {
      const exams = [
        { key: 'eyes', label: 'Mắt' }, { key: 'ears', label: 'Tai' }, { key: 'nose', label: 'Mũi' },
        { key: 'mouth', label: 'Miệng/Răng' }, { key: 'skinCoat', label: 'Da & Lông' },
        { key: 'gastrointestinal', label: 'Tiêu hóa' }, { key: 'respiratory', label: 'Hô hấp' }
      ];
      const abnormalExams = exams.filter(e => soap.objective[e.key] && !soap.objective[e.key].isNormal);
      if (abnormalExams.length > 0) {
        sysExamsHtml = `
          <div style="margin-top: 8px; font-size: 0.85rem;">
            <div class="clinical-label" style="font-size: 0.75rem; margin-bottom: 4px;">Phát hiện bất thường:</div>
            <ul style="margin: 0; padding-left: 20px; color: #b91c1c;">
              ${abnormalExams.map(e => `<li><strong>${e.label}:</strong> ${soap.objective[e.key].note || 'Có bất thường'}</li>`).join('')}
            </ul>
          </div>
        `;
      }
    }

    // =================== VACCINATION RECORD TEMPLATE ===================
    if (isVaccination && soap) {
      const baseUrl = api.defaults.baseURL?.replace('/api', '') || '';
      const vaccWeight = soap.weight || appt?.weight;
      const vaccAttachments = (soap.attachments || []);
      const vaccImagesHtml = vaccAttachments.length > 0 ? `
        <div class="premium-box" style="border-left: 4px solid #6366f1; background: #f5f3ff;">
          <div class="section-title" style="color:#4f46e5;">&#128247; Hình Ảnh Cận Lâm Sàng</div>
          <div style="display:flex; gap:10px; flex-wrap:wrap;">
            ${vaccAttachments.map((u: string) => `<img src="${baseUrl}${u}" style="width:140px;height:140px;object-fit:cover;border-radius:8px;border:1px solid #c7d2fe;box-shadow:0 2px 6px rgba(99,102,241,0.15);" />`).join('')}
          </div>
        </div>` : '';

      const allergySummary = soap.isAllergic ? `<div style="color:#b91c1c; font-weight:600; margin-top:4px;">&#9888; Dị ứng: ${soap.allergyDetails || 'Có'}</div>` : '';
      const reactionSummary = soap.hasPreviousReaction ? `<div style="color:#b91c1c; font-size:0.82rem; margin-top:4px;">Phản ứng tiêm cũ: ${soap.previousReactionDetails || 'Có'}</div>` : '';
      const vomitingSummary = soap.hasVomitingOrDiarrhea ? `<span style="background:#fee2e2;color:#b91c1c;border-radius:4px;padding:2px 8px;font-size:0.75rem;margin-right:4px;">Nôn / Tiêu chảy</span>` : '';
      const coughSummary = soap.hasCoughOrSneeze ? `<span style="background:#fee2e2;color:#b91c1c;border-radius:4px;padding:2px 8px;font-size:0.75rem;">Ho / Hắt hơi</span>` : '';

      medicalRecordHtml = `
      <div class="medical-record-page" style="${optInvoice ? 'page-break-after: always;' : ''}">
        <div class="header">
          <div class="logo-block">
            <div class="logo-circle">&#128062;</div>
            <div>
              <div class="clinic-name">MYPET CLINIC</div>
              <div class="clinic-sub">Hệ thống phòng khám thú y cao cấp</div>
            </div>
          </div>
          <div>
            <div class="inv-title" style="color:#6366f1;">PHIẼU TIÊM CHỦNG</div>
            <div class="inv-meta">Ngày tiêm: ${soap.injectionDate ? new Date(soap.injectionDate).toLocaleDateString('vi-VN') : now}</div>
          </div>
        </div>

        <!-- Patient Info -->
        <div class="premium-box patient-info-box" style="border-color:#c7d2fe; background:#eef2ff;">
          <div style="flex:1;">
            <div class="info-label">THÔNG TIN BỆNH NHÂN</div>
            <div class="info-value text-xl">${inv.petName || '—'} <span class="species-badge" style="background:#e0e7ff;color:#4338ca;">${inv.petSpecies || 'Khác'}</span></div>
            <div style="display:flex; flex-wrap:wrap; gap:14px; font-size:0.85rem; color:#475569; margin-top:6px;">
              <div>Giống: <strong>${appt?.breed || '—'}</strong></div>
              ${vaccWeight ? `<div>Cân nặng: <strong>${vaccWeight} kg</strong></div>` : ''}
              ${soap.temperature ? `<div>Nhiệt độ: <strong>${soap.temperature}°C</strong></div>` : ''}
              ${soap.heartRate ? `<div>Nhịp tim: <strong>${soap.heartRate} bpm</strong></div>` : ''}
              ${soap.respiratoryRate ? `<div>Nhịp thở: <strong>${soap.respiratoryRate} l/p</strong></div>` : ''}
            </div>
            <div class="info-meta" style="margin-top:8px;">Chủ nuôi: <strong>${inv.customerName || '—'}</strong> (${inv.customerPhone || 'N/A'})</div>
          </div>
          <div style="text-align:right; border-left:1px dashed #c7d2fe; padding-left:20px;">
            <div class="info-label">BÁC SĨ THỰC HIỆN</div>
            <div class="info-value text-lg" style="color:#4338ca; margin-bottom:8px;">Bs. ${(soap.doctorName || inv.doctorName || '—').replace(/^Bs\.?\s*/i,'')}</div>
            <div style="font-size:0.75rem; background:white; padding:4px 10px; border-radius:6px; display:inline-block; color:#0f172a; border:1px solid #e0e7ff;">Dịch vụ: <strong>Tiêm phòng</strong></div>
          </div>
        </div>

        <!-- S: Subjective -->
        <div class="premium-box" style="border-left:4px solid #3b82f6; background:#eff6ff;">
          <div class="section-title" style="color:#1d4ed8;">S — Thông tin chủ quan (Khách hàng cung cấp)</div>
          <div class="clinical-grid">
            <div class="clinical-item">
              <div class="clinical-label">Lý do tiêm:</div>
              <div class="clinical-text">${soap.reasonForVisit || 'Tiêm định kỳ'}</div>
            </div>
            ${soap.eatingStatus ? `<div class="clinical-item" style="margin-top:8px;">
              <div class="clinical-label">Tình trạng ăn uống:</div>
              <div class="clinical-text">${soap.eatingStatus}</div>
            </div>` : ''}
            ${(soap.hasVomitingOrDiarrhea || soap.hasCoughOrSneeze) ? `<div class="clinical-item" style="margin-top:8px;">
              <div class="clinical-label">Triệu chứng hiện tại:</div>
              <div style="margin-top:4px;">${vomitingSummary}${coughSummary}</div>
            </div>` : ''}
            ${soap.previousVaccineHistory ? `<div class="clinical-item" style="margin-top:8px;">
              <div class="clinical-label">Tiền sử vắc-xin:</div>
              <div class="clinical-text" style="font-style:italic;color:#475569;">${soap.previousVaccineHistory}</div>
            </div>` : ''}
            ${allergySummary || reactionSummary ? `<div class="clinical-item" style="margin-top:8px;">${allergySummary}${reactionSummary}</div>` : ''}
            ${soap.ownerNotes ? `<div class="clinical-item" style="margin-top:8px;">
              <div class="clinical-label">Ghi chú của chủ nuôi:</div>
              <div class="clinical-text" style="font-style:italic;color:#475569;">${soap.ownerNotes}</div>
            </div>` : ''}
          </div>
        </div>

        <!-- O: Objective -->
        <div class="premium-box" style="border-left:4px solid #06b6d4; background:#ecfeff;">
          <div class="section-title" style="color:#0891b2;">O — Khám lâm sàng (Bác sĩ)</div>
          <div style="display:grid; grid-template-columns:1fr 1fr; gap:10px;">
            <div>
              <div class="clinical-label">Tinh thần:</div>
              <div class="clinical-text">${soap.mentalStatus || 'Linh hoạt'}</div>
            </div>
            <div>
              <div class="clinical-label">Niêm mạc:</div>
              <div class="clinical-text">${soap.mucosaStatus || 'Hồng hào'}</div>
            </div>
            ${soap.eyeNoseEarStatus ? `<div style="grid-column:span 2;">
              <div class="clinical-label">Mắt / Mũi / Tai:</div>
              <div class="clinical-text">${soap.eyeNoseEarStatus}</div>
            </div>` : ''}
            ${soap.lymphNodeStatus ? `<div style="grid-column:span 2;">
              <div class="clinical-label">Hạch bạch huyết:</div>
              <div class="clinical-text">${soap.lymphNodeStatus}</div>
            </div>` : ''}
            ${soap.dehydrationPercent != null ? `<div>
              <div class="clinical-label">Mất nước:</div>
              <div class="clinical-text">${soap.dehydrationPercent}%</div>
            </div>` : ''}
          </div>
        </div>

        ${vaccImagesHtml}

        <!-- A: Assessment + Vaccine -->
        <div class="premium-box" style="border-left:4px solid #10b981; background:#ecfdf5;">
          <div class="section-title" style="color:#047857;">A — Đánh giá &amp; Thông tin Vắc-xin</div>
          <div class="clinical-grid">
            <div class="clinical-item">
              <div class="clinical-label">Kết luận lâm sàng:</div>
              <div>
                <span style="display:inline-block; padding:3px 12px; border-radius:20px; font-weight:600; font-size:0.82rem;
                  background:${soap.clinicalAssessment === 'Đủ điều kiện' ? '#d1fae5' : '#fee2e2'};
                  color:${soap.clinicalAssessment === 'Đủ điều kiện' ? '#065f46' : '#b91c1c'};
                  border:1px solid ${soap.clinicalAssessment === 'Đủ điều kiện' ? '#6ee7b7' : '#fca5a5'};
                ">${soap.clinicalAssessment || 'Đủ điều kiện'}</span>
              </div>
              ${soap.doctorRemarks ? `<div class="clinical-text" style="margin-top:6px;font-style:italic;color:#475569;">${soap.doctorRemarks}</div>` : ''}
            </div>
            ${soap.vaccineName ? `<div class="clinical-item" style="margin-top:10px;padding-top:10px;border-top:1px dashed #a7f3d0;">
              <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;">
                <div>
                  <div class="clinical-label">Vắc-xin đã tiêm:</div>
                  <div class="clinical-text" style="font-weight:700;color:#065f46;">${soap.vaccineName}</div>
                  ${soap.batchNumber ? `<div style="font-size:0.75rem;color:#64748b;">Lô: ${soap.batchNumber}</div>` : ''}
                </div>
                <div>
                  <div class="clinical-label">Đường tiêm / Vị trí:</div>
                  <div class="clinical-text">${[soap.route, soap.injectionSite].filter(Boolean).join(' — ') || '—'}</div>
                  ${soap.dose != null ? `<div style="font-size:0.75rem;color:#64748b;">Liều: ${soap.dose} ml</div>` : ''}
                </div>
              </div>
            </div>` : ''}
          </div>
        </div>

        <!-- P: Plan -->
        <div class="premium-box" style="border-left:4px solid #f59e0b; background:#fffbeb;">
          <div class="section-title" style="color:#b45309;">P — Kế hoạch &amp; Dặn dò</div>
          <div class="clinical-grid">
            ${soap.nextDueDate ? `<div class="clinical-item">
              <div class="clinical-label">Ngày tiêm nhắc lại dự kiến:</div>
              <div>
                <span style="display:inline-block;background:#fef3c7;padding:4px 12px;border-radius:6px;border:1px solid #f59e0b;color:#b45309;font-size:0.85rem;font-weight:600;">
                  &#128197; ${new Date(soap.nextDueDate).toLocaleDateString('vi-VN')}
                </span>
              </div>
            </div>` : ''}
            <div class="clinical-item" style="margin-top:10px;padding-top:10px;border-top:1px dashed #fde68a;">
              <div class="clinical-label">Lời dặn dò:</div>
              <div class="clinical-text" style="line-height:1.7;">${soap.followUpInstructions || '- Kiêng tắm 7 ngày sau tiêm.<br>- Theo dõi nhiệt độ và biểu hiện dị ứng trong 24 giờ đầu.<br>- Tái khám ngay nếu có sốt, sưng nơi tiêm, bỏ ăn.'}</div>
            </div>
            ${soap.reactionNote ? `<div class="clinical-item" style="margin-top:8px;padding:8px;background:#fef2f2;border-radius:8px;border:1px solid #fca5a5;">
              <div class="clinical-label" style="color:#b91c1c;">&#9888; Ghi chú phản ứng sau tiêm:</div>
              <div class="clinical-text" style="color:#b91c1c;">${soap.reactionNote}</div>
            </div>` : ''}
          </div>
        </div>

        <div class="footer sign-area">
          <div style="flex:1;"></div>
          <div style="width:200px;text-align:center;">
            <div style="font-weight:700;margin-bottom:4px;">Chữ ký Bác sĩ</div>
            <div class="sign-line"></div>
            <div style="font-size:0.85rem;color:#0f172a;font-weight:700;">Bs. ${(soap.doctorName || inv.doctorName || '').replace(/^Bs\.?\s*/i,'')}</div>
          </div>
        </div>
        <div style="text-align:center; font-size:0.7rem; color:#94a3b8; margin-top:30px; border-top:1px solid #f1f5f9; padding-top:10px;">
          * Phiếu tiêm chủng điện tử — Hệ thống MyPetClinic. Mã hồ sơ: #${String(appt?.id || inv.id).padStart(5,'0')} *
        </div>
      </div>`;

    } else {
    // =================== MEDICAL RECORD TEMPLATE ===================
    medicalRecordHtml = `
    <div class="medical-record-page" style="${optInvoice ? 'page-break-after: always;' : ''}">
      <div class="header">
        <div class="logo-block">
          <div class="logo-circle">🐾</div>
          <div>
            <div class="clinic-name">MYPET CLINIC</div>
            <div class="clinic-sub">Hệ thống phòng khám thú y cao cấp</div>
          </div>
        </div>
        <div>
          <div class="inv-title">HỒ SƠ BỆNH ÁN</div>
          <div class="inv-meta">Ngày khám: ${now}</div>
        </div>
      </div>

      <div class="premium-box patient-info-box">
        <div style="flex:1;">
          <div class="info-label">THÔNG TIN BỆNH NHÂN</div>
          <div class="info-value text-xl">${inv.petName || '—'} <span class="species-badge">${inv.petSpecies || 'Khác'}</span></div>
          <div style="display:flex; flex-wrap: wrap; gap: 15px; font-size: 0.85rem; color: #475569; margin-top: 6px;">
            <div style="white-space: nowrap;">Giống: <strong>${appt?.breed || '—'}</strong></div>
            <div style="white-space: nowrap;">Cân nặng: <strong>${soap?.objective?.weight || appt?.weight ? (soap?.objective?.weight || appt?.weight) + ' kg' : '—'}</strong></div>
            ${soap?.objective?.temperature ? `<div style="white-space: nowrap;">Nhiệt độ: <strong>${soap.objective.temperature}°C</strong></div>` : ''}
            ${soap?.objective?.heartRate ? `<div style="white-space: nowrap;">Nhịp tim: <strong>${soap.objective.heartRate} bpm</strong></div>` : ''}
          </div>
          <div class="info-meta" style="margin-top: 8px;">Chủ nuôi: <strong>${inv.customerName || '—'}</strong> (${inv.customerPhone || 'N/A'})</div>
        </div>
        <div style="text-align:right; border-left: 1px dashed #bfdbfe; padding-left: 20px;">
          <div class="info-label">BÁC SĨ ĐIỀU TRỊ</div>
          <div class="info-value text-lg" style="color:#2563eb; margin-bottom: 8px;">${inv.doctorName || '—'}</div>
          ${appt?.serviceName ? `<div style="font-size: 0.75rem; background: white; padding: 4px 8px; border-radius: 6px; display: inline-block; color: #0f172a; border: 1px solid #e2e8f0;">Dịch vụ: <strong>${appt.serviceName}</strong></div>` : ''}
        </div>
      </div>

      <div class="premium-box clinical-box">
        <div class="section-title"><i class="bi bi-clipboard-pulse"></i> 1. KẾT QUẢ KHÁM LÂM SÀNG (SOAP)</div>
        
        <div class="clinical-grid">
          ${soap ? `
          <div class="clinical-item">
            <div class="clinical-label">S - Chủ quan (Triệu chứng & Lý do khám):</div>
            <div class="clinical-text">${soap.subjective?.chiefComplaint || 'Không ghi nhận'}</div>
            <div style="font-size: 0.8rem; color: #475569; margin-top: 4px; display: flex; gap: 16px; flex-wrap: wrap;">
              ${soap.subjective?.appetite && soap.subjective.appetite !== 'Bình thường' ? `<div><span>Ăn uống:</span> <strong>${soap.subjective.appetite}</strong></div>` : ''}
              ${soap.subjective?.hasVomiting ? `<div style="color: #b91c1c;"><span>Nôn mửa:</span> <strong>Có</strong> (${soap.subjective.vomitingDetails || ''})</div>` : ''}
              ${soap.subjective?.hasDiarrhea ? `<div style="color: #b91c1c;"><span>Tiêu chảy:</span> <strong>Có</strong> (${soap.subjective.diarrheaDetails || ''})</div>` : ''}
              ${soap.subjective?.activityLevel && soap.subjective.activityLevel !== 'Bình thường' ? `<div><span>Vận động:</span> <strong>${soap.subjective.activityLevel}</strong></div>` : ''}
            </div>
            ${soap.subjective?.petOwnerNotes ? `<div class="clinical-text" style="font-size: 0.8rem; color:#64748b; margin-top: 6px; font-style:italic;">* Ghi chú từ chủ: ${soap.subjective.petOwnerNotes}</div>` : ''}
          </div>
          <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
            <div class="clinical-label">O - Khách quan (Khám thực thể):</div>
            <div class="clinical-text" style="font-size: 0.85rem;">
              Thể trạng (BCS): <strong>${soap.objective?.bodyConditionScore || 5}/9</strong> | Tri giác: <strong>${soap.objective?.mentation || 'Bình thường'}</strong> | Mức mất nước: <strong>${soap.objective?.hydration || 'Bình thường'}</strong>
            </div>
            ${sysExamsHtml}
            ${services.length > 0 ? `
            <div style="margin-top: 10px;">
              <div class="clinical-label" style="font-size: 0.75rem; margin-bottom: 4px;">Chỉ định cận lâm sàng:</div>
              ${servicesHtml}
            </div>` : ''}
          </div>
          
          ${imagesHtml}

          <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
            <div class="clinical-label">A - Chẩn đoán:</div>
            <div class="clinical-text" style="font-weight:600; color:#b91c1c; font-size: 1rem;">${soap.assessment?.definitiveDiagnosis || soap.assessment?.tentativeDiagnosis || 'Chưa có chẩn đoán cuối cùng'}</div>
            ${soap.assessment?.differentialDiagnosis ? `<div style="font-size: 0.8rem; color: #475569; margin-top: 2px;">Chẩn đoán phân biệt: <em>${soap.assessment.differentialDiagnosis}</em></div>` : ''}
            <div class="clinical-text" style="font-size: 0.8rem; color:#64748b; margin-top: 6px;">Tiên lượng: <strong style="color: #0f172a;">${soap.assessment?.prognosis || 'Tốt'}</strong> &nbsp;|&nbsp; Mức độ bệnh: <strong style="color: #0f172a;">${soap.assessment?.diseaseSeverity || 'Nhẹ'}</strong></div>
          </div>
          ` : `
          <div class="clinical-item">
            <div class="clinical-label">Lý do khám & Triệu chứng:</div>
            <div class="clinical-text">${appt?.symptom || '<span style="color:#94a3b8;font-style:italic;">Không ghi nhận triệu chứng bất thường</span>'}</div>
          </div>
          
          ${services.length > 0 ? `
          <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
            <div class="clinical-label" style="margin-bottom: 6px;">Chỉ định cận lâm sàng:</div>
            <div>${servicesHtml}</div>
          </div>` : ''}

          <div class="clinical-item" style="margin-top: 10px; padding-top: 10px; border-top: 1px dashed #e2e8f0;">
            <div class="clinical-label">Kết luận chẩn đoán:</div>
            <div class="clinical-text" style="font-weight:600; color:#b91c1c; font-size: 1rem;">${(appt?.note ? appt.note.replace(/\[(Á|A)p d(ụ|\?)ng voucher:\s*.*?\]/i, '').trim() : '') || '<span style="color:#94a3b8;font-weight:400;font-style:italic;">Đang theo dõi thêm</span>'}</div>
          </div>
          `}
        </div>
      </div>

      <div class="premium-box rx-box">
        <div class="section-title"><i class="bi bi-capsule"></i> 2. KẾT QUẢ ĐIỀU TRỊ & KÊ ĐƠN THUỐC (PLAN)</div>
        
        <div style="margin-bottom: 12px;">
          <div class="clinical-label" style="margin-bottom: 6px;">Đơn thuốc:</div>
          ${medicines.length > 0 ? `<div class="rx-list">${prescriptionsHtml}</div>` : '<div style="color:#64748b; font-style:italic;">Không có chỉ định thuốc mang về.</div>'}
        </div>

        <div style="border-top: 1px dashed #e2e8f0; padding-top: 12px; margin-top: 12px;">
          <div class="clinical-label" style="margin-bottom: 4px;">Dặn dò chăm sóc:</div>
          <div style="color:#334155; line-height: 1.6; font-size: 0.85rem;">
            ${soap?.plan?.careInstructions ? soap.plan.careInstructions.replace(/\\n/g, '<br>') : `
            - Vui lòng cho thú cưng uống thuốc đúng liều lượng (nếu có).<br>
            - Tái khám ngay nếu thú cưng có biểu hiện bất thường (nôn mửa, bỏ ăn).<br>
            - Đảm bảo môi trường sống sạch sẽ, thoáng mát.`}
          </div>
          
          ${soap?.plan?.followUpDate ? `
          <div style="margin-top: 12px;">
            <span style="background-color: #fef3c7; padding: 4px 10px; border-radius: 6px; border: 1px solid #f59e0b; color: #b45309; font-size: 0.85rem;">
              <i class="bi bi-calendar-event me-1"></i> Lịch tái khám: <strong>${new Date(soap.plan.followUpDate).toLocaleDateString('vi-VN')}</strong> 
              ${soap.plan.followUpNote ? `(${soap.plan.followUpNote})` : ''}
            </span>
          </div>` : ''}
        </div>
      </div>

      <div class="footer sign-area">
        <div style="flex:1;">
        </div>
        <div style="width:200px;text-align:center">
          <div style="font-weight:700;margin-bottom:4px">Chữ ký Bác sĩ</div>
          <div class="sign-line"></div>
          <div style="font-size:0.85rem;color:#0f172a;font-weight:700;">${inv.doctorName || ''}</div>
        </div>
      </div>
      
      <div style="text-align:center; font-size: 0.7rem; color: #94a3b8; margin-top: 40px; border-top: 1px solid #f1f5f9; padding-top: 10px;">
        * Tài liệu được trích xuất tự động từ hệ thống quản lý MyPetClinic. Mã hồ sơ: #${String(appt?.id || inv.id).padStart(5, '0')} *
      </div>
      </div>
    `; // end medicalRecordHtml (medical record)
    } // end else (not vaccination)
    } // end if (optMedical && appt)

  // Generate Invoice Section HTML
  let invoiceHtml = '';
  if (optInvoice) {
    invoiceHtml = `
    <div class="invoice-page">
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
          <div class="info-line">MyPet Clinic - 124A Xuân Thủy</div>
          <div class="info-line">P. An Khánh, TP. HCM | Hotline: 0905 090 629</div>
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
    </div>
    `;
  }

  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <title>In Tài Liệu - MyPetClinic</title>
  <style>
    @import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700;800&display=swap');
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: 'Be Vietnam Pro', sans-serif; color: #0f172a; font-size: 13px; line-height: 1.5; padding: 0; background: #e2e8f0; }
    
    .medical-record-page, .invoice-page { 
      background: white; 
      margin: 0 auto 20px auto; 
      padding: 20mm 18mm; 
      width: 210mm;
      min-height: 297mm;
      box-shadow: 0 4px 6px rgba(0,0,0,0.1);
    }
    
    .header { display: flex; justify-content: space-between; align-items: center; padding-bottom: 14px; border-bottom: 3px solid #f59e0b; margin-bottom: 18px; }
    .logo-block { display: flex; align-items: center; gap: 14px; }
    .logo-circle { width: 54px; height: 54px; background: linear-gradient(135deg,#fef08a,#f59e0b); border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 1.6rem; }
    .clinic-name { font-size: 1.25rem; font-weight: 800; letter-spacing: 1px; }
    .clinic-sub { font-size: 0.72rem; color: #64748b; margin-top: 3px; }
    .inv-title { font-size: 1.05rem; font-weight: 800; color: #f59e0b; text-transform: uppercase; letter-spacing: 1px; text-align: right; }
    .inv-meta { font-size: 0.78rem; color: #64748b; text-align: right; margin-top: 4px; }
    
    /* Invoice Specific */
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
    
    /* Medical Record Specific (Premium) */
    .premium-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 18px; margin-bottom: 16px; page-break-inside: avoid; }
    .patient-info-box { display: flex; justify-content: space-between; align-items: center; border-left: 4px solid #3b82f6; background: #eff6ff; border-color: #bfdbfe; }
    .info-value.text-xl { font-size: 1.25rem; font-weight: 700; color: #1e3a8a; margin: 4px 0; }
    .info-value.text-lg { font-size: 1.1rem; font-weight: 700; }
    .species-badge { background: #dbeafe; color: #1d4ed8; padding: 2px 8px; border-radius: 12px; font-size: 0.7rem; vertical-align: middle; }
    .section-title { font-size: 0.95rem; font-weight: 800; color: #0f172a; margin-bottom: 12px; border-bottom: 1px dashed #cbd5e1; padding-bottom: 8px; }
    .clinical-grid { display: flex; flex-direction: column; gap: 12px; }
    .clinical-item { page-break-inside: avoid; }
    .clinical-label { font-size: 0.75rem; font-weight: 600; color: #64748b; }
    .clinical-text { font-size: 0.9rem; color: #0f172a; margin-top: 2px; }
    .rx-list { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .rx-item { background: white; border: 1px solid #e2e8f0; padding: 10px 14px; border-radius: 8px; box-shadow: 0 1px 2px rgba(0,0,0,0.05); page-break-inside: avoid; }
    .rx-name { font-weight: 700; color: #0f172a; font-size: 0.85rem; }
    .rx-qty { font-size: 0.75rem; color: #475569; margin-top: 4px; }
    .sign-area { margin-top: 30px; }
    
    .footer { display: flex; justify-content: space-between; align-items: flex-end; border-top: 1px dashed #cbd5e1; padding-top: 16px; margin-top: 8px; }
    .sign-line { border-bottom: 1px solid #0f172a; height: 60px; margin: 8px 0; }
    
    @media print {
      body { background: white; }
      .medical-record-page, .invoice-page { 
        margin: 0; padding: 0; box-shadow: none; width: 100%; min-height: auto;
      }
      @page { size: A4 portrait; margin: 15mm; }
    }
  </style>
</head>
<body>
  ${medicalRecordHtml}
  ${invoiceHtml}
  <script>window.onload = function(){ setTimeout(() => { window.print(); window.onafterprint = function(){ window.close(); }; }, 500); }<\/script>
</body>
</html>`;

  const pw = window.open('', '_blank', 'width=950,height=800');
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

/* ── LIVE dot realtime indicator ── */
.live-dot {
  display: inline-block;
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background-color: #16a34a;
  box-shadow: 0 0 0 0 rgba(22, 163, 74, 0.6);
  animation: live-pulse 1.6s ease-out infinite;
  flex-shrink: 0;
}
@keyframes live-pulse {
  0%   { box-shadow: 0 0 0 0 rgba(22, 163, 74, 0.7); }
  70%  { box-shadow: 0 0 0 8px rgba(22, 163, 74, 0); }
  100% { box-shadow: 0 0 0 0 rgba(22, 163, 74, 0); }
}
</style>
