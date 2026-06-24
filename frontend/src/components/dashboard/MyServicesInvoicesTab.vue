<template>
  <div class="invoices-tab-container p-4">
    <!-- Summary Cards -->
    <div class="row g-4 mb-4">
      <div class="col-md-4">
        <div class="summary-card bg-warning text-dark p-4 h-100 rounded-4 shadow-sm position-relative overflow-hidden">
          <div class="d-flex align-items-center mb-2 position-relative z-1">
            <i class="bi bi-wallet2 me-2 fs-5 opacity-75"></i>
            <span class="fs-6 fw-medium opacity-75">Tổng chi tiêu</span>
          </div>
          <h2 class="fw-bold mb-0 position-relative z-1">{{ formatCurrency(totalSpent) }}</h2>
          <div class="position-absolute" style="right: -20px; bottom: -20px; opacity: 0.1; transform: scale(3);">
            <i class="bi bi-wallet2"></i>
          </div>
        </div>
      </div>
      <div class="col-md-4">
        <div class="summary-card bg-white p-4 h-100 rounded-4 shadow-sm border border-light">
          <div class="d-flex align-items-center mb-2 text-muted">
            <i class="bi bi-receipt me-2 fs-5"></i>
            <span class="fs-6 fw-medium">Số hóa đơn</span>
          </div>
          <h2 class="fw-bold text-dark mb-0">{{ invoiceCount }}</h2>
        </div>
      </div>
      <div class="col-md-4">
        <div class="summary-card bg-white p-4 h-100 rounded-4 shadow-sm border border-light">
          <div class="d-flex align-items-center mb-2 text-muted">
            <i class="bi bi-heptagon me-2 fs-5"></i>
            <span class="fs-6 fw-medium">Thú cưng</span>
          </div>
          <h2 class="fw-bold text-dark mb-0">{{ String(petCount).padStart(2, '0') }}</h2>
        </div>
      </div>
    </div>

    <!-- Main List Section -->
    <div class="card border-0 rounded-4 shadow-sm overflow-hidden bg-white">
      <div class="card-header bg-white border-bottom p-4 d-flex justify-content-between align-items-center">
        <h6 class="fw-bold text-dark mb-0 text-uppercase" style="letter-spacing: 0.5px; font-size: 0.85rem;">Danh sách hóa đơn</h6>
        <div class="d-flex gap-2">
          <button 
            class="btn btn-sm rounded-pill px-3 fw-medium" 
            :class="filterStatus === 'all' ? 'btn-warning text-dark fw-bold' : 'btn-light text-muted'"
            @click="filterStatus = 'all'"
          >Tất cả</button>
          <button 
            class="btn btn-sm rounded-pill px-3 fw-medium" 
            :class="filterStatus === 'paid' ? 'btn-warning text-dark fw-bold' : 'btn-light text-muted'"
            @click="filterStatus = 'paid'"
          >Đã thanh toán</button>
          <button 
            class="btn btn-sm rounded-pill px-3 fw-medium" 
            :class="filterStatus === 'cancelled' ? 'btn-warning text-dark fw-bold' : 'btn-light text-muted'"
            @click="filterStatus = 'cancelled'"
          >Đã hủy</button>
        </div>
      </div>
      <div class="card-body p-0">
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0 custom-table">
            <thead class="bg-light">
              <tr>
                <th class="text-muted fw-semibold py-3 px-4" style="font-size: 0.75rem; letter-spacing: 0.5px;">DỊCH VỤ</th>
                <th class="text-muted fw-semibold py-3" style="font-size: 0.75rem; letter-spacing: 0.5px;">NGÀY THỰC HIỆN</th>
                <th class="text-muted fw-semibold py-3" style="font-size: 0.75rem; letter-spacing: 0.5px;">THÚ CƯNG</th>
                <th class="text-muted fw-semibold py-3" style="font-size: 0.75rem; letter-spacing: 0.5px;">TRẠNG THÁI</th>
                <th class="text-muted fw-semibold py-3" style="font-size: 0.75rem; letter-spacing: 0.5px;">TỔNG TIỀN</th>
                <th class="py-3 px-4"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="inv in filteredInvoices" :key="inv.id" class="cursor-pointer" style="transition: all 0.2s;" @click="openInvoiceDetail(inv)">
                <td class="px-4 py-3">
                  <div class="d-flex align-items-center">
                    <div class="service-icon-box bg-warning-subtle text-warning rounded-3 d-flex align-items-center justify-content-center me-3" style="width: 40px; height: 40px;">
                      <i :class="inv.iconClass" class="fs-5"></i>
                    </div>
                    <span class="fw-medium text-dark">{{ inv.serviceName }}</span>
                  </div>
                </td>
                <td class="py-3">
                  <div class="text-dark">{{ formatDate(inv.date) }}</div>
                </td>
                <td class="py-3">
                  <div class="d-flex align-items-center gap-2">
                    <div class="avatar-circle bg-warning-subtle text-warning fw-bold d-flex align-items-center justify-content-center rounded-circle" style="width: 32px; height: 32px; font-size: 0.8rem;">
                      {{ getInitials(inv.petName) }}
                    </div>
                    <div>
                      <div class="fw-medium text-dark" style="font-size: 0.9rem;">{{ inv.petName }}</div>
                      <div class="text-muted" style="font-size: 0.8rem;">({{ inv.petSpecies }})</div>
                    </div>
                  </div>
                </td>
                <td class="py-3">
                  <span class="badge rounded-pill px-3 py-2 fw-medium" :class="getStatusBadgeClass(inv.status)">
                    {{ getStatusLabel(inv.status) }}
                  </span>
                </td>
                <td class="py-3">
                  <strong class="text-dark">{{ formatCurrency(inv.totalAmount) }}</strong>
                </td>
                <td class="px-4 py-3 text-end">
                  <i class="bi bi-chevron-right text-muted"></i>
                </td>
              </tr>
              <tr v-if="filteredInvoices.length === 0">
                <td colspan="6" class="text-center py-5 text-muted">
                  <div class="mb-3"><i class="bi bi-inbox fs-1 opacity-50"></i></div>
                  Không tìm thấy hóa đơn nào.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Invoice Detail Modal -->
    <div v-if="showDetailModal && selectedInvoice" class="invoice-modal-overlay" @click.self="closeInvoiceDetail">
      <div class="invoice-modal-card">
        <!-- Header -->
        <div class="d-flex justify-content-between align-items-center border-bottom pb-3 mb-4">
          <div>
            <h5 class="fw-bold text-dark mb-1">Hóa đơn {{ selectedInvoice.id }}</h5>
            <div class="text-muted small">Xuất ngày {{ formatDate(selectedInvoice.date) }}</div>
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-outline-secondary btn-sm rounded-pill px-3 fw-medium"><i class="bi bi-share me-1"></i> Chia sẻ</button>
            <button class="btn btn-dark btn-sm rounded-pill px-3 fw-medium"><i class="bi bi-download me-1"></i> Tải PDF</button>
            <button class="btn btn-light btn-sm rounded-circle ms-2" @click="closeInvoiceDetail"><i class="bi bi-x-lg"></i></button>
          </div>
        </div>

        <!-- Status Banner -->
        <div class="status-banner mb-4 p-3 rounded-3 d-flex justify-content-between align-items-center" :class="selectedInvoice.status === 'paid' ? 'bg-success-subtle border border-success-subtle' : 'bg-warning-subtle border border-warning-subtle'">
          <div class="d-flex align-items-center gap-3">
            <div class="icon-circle text-white d-flex align-items-center justify-content-center rounded-circle shadow-sm" :class="selectedInvoice.status === 'paid' ? 'bg-success' : 'bg-warning'" style="width: 44px; height: 44px;">
              <i :class="selectedInvoice.status === 'paid' ? 'bi bi-check-lg fs-4' : 'bi bi-hourglass-split fs-5'"></i>
            </div>
            <div>
              <h6 class="fw-bold mb-0" :class="selectedInvoice.status === 'paid' ? 'text-success' : 'text-warning-emphasis'">{{ selectedInvoice.status === 'paid' ? 'Thanh toán thành công' : 'Chờ thanh toán' }}</h6>
              <div class="small" :class="selectedInvoice.status === 'paid' ? 'text-success opacity-75' : 'text-warning-emphasis opacity-75'">Mã GD: {{ selectedInvoice.transactionId || 'N/A' }}</div>
            </div>
          </div>
          <div class="text-end">
            <div class="small fw-bold" :class="selectedInvoice.status === 'paid' ? 'text-success opacity-75' : 'text-warning-emphasis opacity-75'">{{ selectedInvoice.status === 'paid' ? 'ĐÃ THANH TOÁN' : 'SỐ TIỀN CẦN THANH TOÁN' }}</div>
            <h4 class="fw-bold mb-0" :class="selectedInvoice.status === 'paid' ? 'text-success' : 'text-warning-emphasis'">{{ formatCurrency(selectedInvoice.totalAmount) }}</h4>
          </div>
        </div>

        <!-- Details Row -->
        <div class="row g-3 mb-4">
          <div class="col-sm-6">
            <div class="p-3 border rounded-3 bg-light h-100">
              <div class="small text-muted fw-bold mb-2 text-uppercase" style="font-size: 0.75rem;">Bệnh nhân</div>
              <div class="d-flex align-items-center gap-3">
                <div class="avatar-circle bg-warning-subtle text-warning fw-bold d-flex align-items-center justify-content-center rounded-3" style="width: 48px; height: 48px; font-size: 1.2rem;">
                  {{ getInitials(selectedInvoice.petName) }}
                </div>
                <div>
                  <h6 class="fw-bold text-dark mb-0">{{ selectedInvoice.petName }}</h6>
                  <div class="small text-muted">{{ selectedInvoice.petSpecies }} <span v-if="selectedInvoice.petAge">• {{ selectedInvoice.petAge }}</span></div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-sm-6">
            <div class="p-3 border rounded-3 bg-light h-100">
              <div class="small text-muted fw-bold mb-2 text-uppercase" style="font-size: 0.75rem;">Bác sĩ phụ trách</div>
              <div class="d-flex align-items-center gap-3">
                <div class="avatar-circle bg-info-subtle text-info fw-bold d-flex align-items-center justify-content-center rounded-3" style="width: 48px; height: 48px; font-size: 1.2rem;">
                  <i class="bi bi-person-badge"></i>
                </div>
                <div>
                  <h6 class="fw-bold text-dark mb-0">{{ selectedInvoice.doctorName }}</h6>
                  <div class="small text-muted">{{ selectedInvoice.doctorTitle }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content -->
        <div class="row g-4">
          <!-- Left Col: Services -->
          <div class="col-md-8">
            <h6 class="fw-bold text-dark opacity-75 mb-3">Chi tiết dịch vụ</h6>
            <div class="border rounded-3 overflow-hidden">
              <table class="table mb-0 custom-table">
                <thead class="bg-light">
                  <tr>
                    <th class="small text-muted fw-semibold py-2 px-3 border-bottom-0">MÔ TẢ DỊCH VỤ</th>
                    <th class="small text-muted fw-semibold py-2 px-3 border-bottom-0 text-center" style="width: 80px;">SL</th>
                    <th class="small text-muted fw-semibold py-2 px-3 border-bottom-0 text-end" style="width: 120px;">ĐƠN GIÁ</th>
                    <th class="small text-muted fw-semibold py-2 px-3 border-bottom-0 text-end" style="width: 120px;">THÀNH TIỀN</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(item, idx) in selectedInvoice.items" :key="idx">
                    <td class="p-3 align-middle">
                      <div class="d-flex align-items-center gap-3">
                        <div class="bg-warning-subtle text-warning rounded d-flex align-items-center justify-content-center" style="width: 36px; height: 36px;">
                          <i :class="item.icon"></i>
                        </div>
                        <div>
                          <div class="fw-bold text-dark" style="font-size: 0.9rem;">{{ item.description }}</div>
                          <div class="small text-muted" style="font-size: 0.75rem;">{{ item.descNote }}</div>
                        </div>
                      </div>
                    </td>
                    <td class="p-3 align-middle text-center">{{ item.qty }}</td>
                    <td class="p-3 align-middle text-end">{{ formatCurrency(item.price) }}</td>
                    <td class="p-3 align-middle text-end fw-bold text-dark">{{ formatCurrency(item.total) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>


            
            <!-- VNPay QR (CREATIVE ADDITION 2) -->
            <div v-if="selectedInvoice.status === 'pending'" class="mt-4 p-3 border border-primary-subtle rounded-3 bg-primary-subtle d-flex gap-3 align-items-center justify-content-between">
              <div class="d-flex align-items-center gap-3">
                <i class="bi bi-qr-code-scan text-primary fs-2"></i>
                <div>
                  <h6 class="fw-bold text-primary mb-1">Thanh toán nhanh bằng mã QR</h6>
                  <p class="mb-0 small text-dark opacity-75">Dùng ứng dụng Ngân hàng hoặc Ví MoMo quét mã để thanh toán.</p>
                </div>
              </div>
              <button class="btn btn-primary btn-sm rounded-pill px-3 fw-bold shadow-sm">Mở mã QR</button>
            </div>
          </div>

          <!-- Right Col: Summary & Notes -->
          <div class="col-md-4">
            <!-- Payment Card -->
            <div class="text-white rounded-4 p-4 mb-3 shadow-sm position-relative overflow-hidden" style="background: linear-gradient(145deg, #0f172a 0%, #1e293b 100%);">
              <h6 class="fw-bold text-white mb-4">Tổng thanh toán</h6>
              
              <div class="d-flex justify-content-between mb-2 small opacity-75" v-if="selectedInvoice.serviceFee">
                <span>Tiền dịch vụ</span>
                <span>{{ formatCurrency(selectedInvoice.serviceFee) }}</span>
              </div>
              <div class="d-flex justify-content-between mb-2 small opacity-75" v-if="selectedInvoice.medicineFee">
                <span>Tiền thuốc</span>
                <span>{{ formatCurrency(selectedInvoice.medicineFee) }}</span>
              </div>
              
              <div class="border-top border-secondary border-opacity-50 pt-3 mb-4 d-flex justify-content-between align-items-center">
                <span class="small opacity-75 text-uppercase">Tổng cộng</span>
                <h3 class="fw-bold mb-0 text-white">{{ formatCurrency(selectedInvoice.totalAmount) }}</h3>
              </div>
              
              <div class="bg-white bg-opacity-10 rounded-3 p-3 d-flex align-items-center gap-3">
                <i class="bi fs-4" :class="getPaymentIcon(selectedInvoice.paymentMethod)"></i>
                <div>
                  <div class="small opacity-75" style="font-size: 0.7rem;">Phương thức thanh toán</div>
                  <div class="fw-bold small">{{ selectedInvoice.paymentMethod || 'Chưa có' }}</div>
                </div>
              </div>
            </div>
            
            <!-- Notes -->
            <div class="p-3 border rounded-3 bg-light">
              <div class="d-flex align-items-center gap-2 mb-2">
                <i class="bi bi-info-circle text-primary"></i>
                <span class="small fw-bold text-dark text-uppercase">Ghi chú lâm sàng</span>
              </div>
              <p class="small text-muted mb-4" style="line-height: 1.5;">{{ selectedInvoice.clinicalNotes || 'Không có ghi chú.' }}</p>
              
              <div class="border-top pt-3">
                <div class="small fw-bold text-dark text-uppercase mb-1" style="font-size: 0.7rem;">Địa chỉ phòng khám</div>
                <div class="small text-muted" style="line-height: 1.5;">
                  MyPet Clinic - Trụ sở chính<br>
                  123 Đường Thú Y, Quận 1<br>
                  Hồ Chí Minh, VN<br>
                  +84 (28) 3910 1234
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

// --- Types ---
interface InvoiceMock {
  id: string;
  serviceName: string;
  iconClass: string;
  date: string;
  petName: string;
  petSpecies: string;
  petAge?: string;
  doctorName?: string;
  doctorTitle?: string;
  status: 'paid' | 'cancelled' | 'pending';
  totalAmount: number;
  serviceFee?: number;
  medicineFee?: number;
  paymentMethod?: string;
  transactionId?: string;
  clinicalNotes?: string;
  nextAppointment?: string;
  items?: { description: string, descNote: string, icon: string, qty: number, price: number, total: number }[];
}

// --- State ---
const showDetailModal = ref(false);
const selectedInvoice = ref<InvoiceMock | null>(null);

const invoices = ref<InvoiceMock[]>([]);
const totalSpent = computed(() => invoices.value.filter(i => i.status === 'paid').reduce((sum, i) => sum + i.totalAmount, 0));
const invoiceCount = computed(() => invoices.value.length);
const petCount = computed(() => {
  const pets = new Set(invoices.value.map(i => i.petName));
  return pets.size;
});

const filterStatus = ref<'all' | 'paid' | 'cancelled'>('all');

// --- API ---
const fetchInvoices = async () => {
  try {
    const res = await api.get('/my-appointments/invoices');
    invoices.value = res.data.map((inv: any) => {
      const serviceItems = inv.items?.filter((i: any) => i.itemType === 'service') || [];
      const medicineItems = inv.items?.filter((i: any) => i.itemType === 'medicine') || [];
      
      const serviceFee = serviceItems.reduce((sum: number, i: any) => sum + i.totalPrice, 0);
      const medicineFee = medicineItems.reduce((sum: number, i: any) => sum + i.totalPrice, 0);
      
      const serviceName = serviceItems.length > 0 ? serviceItems[0].itemName : (medicineItems.length > 0 ? 'Mua thuốc' : 'Dịch vụ');
      const iconClass = serviceItems.length > 0 ? 'bi bi-heart-pulse' : 'bi bi-capsule';

      let status = 'pending';
      if (inv.paymentStatus === 'paid') status = 'paid';
      if (inv.paymentStatus === 'cancelled') status = 'cancelled';

      return {
        id: `INV-${inv.id}`,
        serviceName,
        iconClass,
        date: inv.createdAt,
        petName: inv.petName,
        petSpecies: inv.petSpecies || 'Thú cưng',
        doctorName: inv.doctorName || 'Bác sĩ',
        doctorTitle: 'Bác sĩ Thú y',
        status,
        totalAmount: inv.totalAmount,
        serviceFee,
        medicineFee,
        paymentMethod: inv.paymentMethod || (status === 'paid' ? 'Tiền mặt' : undefined),
        transactionId: status === 'paid' ? `TXN_${inv.id}` : undefined,
        items: inv.items?.map((i: any) => ({
          description: i.itemName,
          descNote: i.itemType === 'service' ? 'Dịch vụ y tế' : 'Thuốc/Vật tư',
          icon: i.itemType === 'service' ? 'bi bi-stethoscope' : 'bi bi-capsule',
          qty: i.quantity,
          price: i.unitPrice,
          total: i.totalPrice
        }))
      };
    });
  } catch (err) {
    console.error('Failed to fetch invoices', err);
  }
};

onMounted(() => {
  fetchInvoices();
});

// --- Mock Data ---


// --- Methods ---
const getPaymentIcon = (method: string | undefined): string => {
  if (!method) return 'bi-wallet2';
  const m = method.toLowerCase();
  if (m.includes('tiền mặt') || m.includes('cash')) return 'bi-cash-stack';
  if (m.includes('chuyển khoản') || m.includes('bank') || m.includes('vnpay') || m.includes('momo') || m.includes('ngân hàng')) return 'bi-bank';
  if (m.includes('thẻ') || m.includes('visa') || m.includes('mastercard')) return 'bi-credit-card';
  return 'bi-wallet2';
};

const openInvoiceDetail = (inv: InvoiceMock) => {
  selectedInvoice.value = inv;
  showDetailModal.value = true;
};

const closeInvoiceDetail = () => {
  showDetailModal.value = false;
  setTimeout(() => selectedInvoice.value = null, 300);
};

// --- Computed ---
const filteredInvoices = computed(() => {
  if (filterStatus.value === 'all') return invoices.value;
  return invoices.value.filter(inv => inv.status === filterStatus.value);
});

// --- Methods ---
const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const formatDate = (dateStr: string): string => {
  if (!dateStr) return '—';
  const finalDateStr = dateStr.endsWith('Z') ? dateStr : dateStr + 'Z';
  const d = new Date(finalDateStr);
  const time = d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  const day = d.getDate().toString().padStart(2, '0');
  const monthNames = ["Th01", "Th02", "Th03", "Th04", "Th05", "Th06", "Th07", "Th08", "Th09", "Th10", "Th11", "Th12"];
  const month = monthNames[d.getMonth()];
  const year = d.getFullYear();
  return `${time} • ${day} ${month}, ${year}`;
};

const getInitials = (name: string): string => {
  if (!name) return 'PT';
  return name.substring(0, 2).toUpperCase();
};

const getStatusBadgeClass = (status: string): string => {
  switch (status) {
    case 'paid': return 'bg-success-subtle text-success border border-success-subtle';
    case 'cancelled': return 'bg-danger-subtle text-danger border border-danger-subtle';
    default: return 'bg-secondary-subtle text-secondary';
  }
};

const getStatusLabel = (status: string): string => {
  switch (status) {
    case 'paid': return 'Đã thanh toán';
    case 'cancelled': return 'Đã hủy';
    default: return 'Chờ xử lý';
  }
};
</script>

<style scoped>
.invoices-tab-container {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.summary-card {
  transition: transform 0.2s, box-shadow 0.2s;
}
.summary-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 20px rgba(0,0,0,0.05) !important;
}

.custom-table th {
  text-transform: uppercase;
  border-bottom: 2px solid #f1f5f9;
}

.custom-table td {
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
}

.custom-table tbody tr:hover td {
  background-color: #f8fafc;
}

.cursor-pointer {
  cursor: pointer;
}

.btn-light {
  background-color: #f1f5f9;
  border-color: #f1f5f9;
}
.btn-light:hover {
  background-color: #e2e8f0;
  border-color: #e2e8f0;
}

/* Modal styles */
.invoice-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  z-index: 1050;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  animation: fadeInOverlay 0.2s ease-out;
}

.invoice-modal-card {
  background: white;
  border-radius: 1.25rem;
  width: 100%;
  max-width: 950px;
  max-height: 95vh;
  overflow-y: auto;
  padding: 2.5rem;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  animation: slideUpModal 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeInOverlay {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUpModal {
  from { opacity: 0; transform: translateY(20px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

/* Scrollbar hidden for modal */
.invoice-modal-card::-webkit-scrollbar {
  width: 6px;
}
.invoice-modal-card::-webkit-scrollbar-thumb {
  background-color: #cbd5e1;
  border-radius: 10px;
}
</style>