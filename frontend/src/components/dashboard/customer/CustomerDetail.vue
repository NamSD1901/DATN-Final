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

        <!-- Tabs Nav -->
        <ul class="nav nav-pills mb-3 gap-2" id="customer-tabs" role="tablist">
          <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill fw-bold" :class="{'active': activeDetailTab === 'appointments', 'bg-warning text-dark': activeDetailTab === 'appointments', 'text-muted': activeDetailTab !== 'appointments'}" @click="activeDetailTab = 'appointments'">Lịch sử Cuộc hẹn</button>
          </li>

          <li class="nav-item" role="presentation">
            <button class="nav-link rounded-pill fw-bold" :class="{'active': activeDetailTab === 'invoices', 'bg-warning text-dark': activeDetailTab === 'invoices', 'text-muted': activeDetailTab !== 'invoices'}" @click="activeDetailTab = 'invoices'">Lịch sử Hóa đơn</button>
          </li>
        </ul>

        <!-- Appointments / Clinical records timeline -->
        <div v-if="activeDetailTab === 'appointments'" class="card border-0 shadow-sm rounded-4 p-4 bg-light animate-fade-in">
          <h5 class="fw-bold text-dark mb-3"><i class="bi bi-clock-history text-warning me-2"></i>Lịch sử Cuộc hẹn</h5>
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
          <div v-if="totalPages > 1" class="d-flex flex-column flex-md-row justify-content-between align-items-center gap-3 mt-3">
            <span class="text-muted small text-center text-md-start">
              Hiển thị {{ (currentPage - 1) * itemsPerPage + 1 }} - {{ Math.min(currentPage * itemsPerPage, detailData.appointments.length) }} trong số {{ detailData.appointments.length }} ca khám
            </span>
            <div class="btn-group flex-wrap justify-content-center">
              <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === 1" @click="prevPage">
                <i class="bi bi-chevron-left"></i> Trước
              </button>
              <template v-for="(page, index) in visiblePages" :key="index">
                <button 
                  v-if="typeof page === 'number'"
                  class="btn btn-sm" 
                  :class="page === currentPage ? 'btn-secondary text-white' : 'btn-outline-secondary'"
                  @click="currentPage = page"
                >
                  {{ page }}
                </button>
                <button v-else class="btn btn-sm btn-outline-secondary" disabled>...</button>
              </template>
              <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === totalPages" @click="nextPage">
                Sau <i class="bi bi-chevron-right"></i>
              </button>
            </div>
          </div>
        </div>


        <!-- Invoices Tab -->
        <div v-if="activeDetailTab === 'invoices'" class="card border-0 shadow-sm rounded-4 p-4 bg-light animate-fade-in">
          <div v-if="loadingInvoices" class="text-center py-4 text-muted"><div class="spinner-border spinner-border-sm text-warning mb-2"></div><br>Đang tải dữ liệu...</div>
          <div v-else-if="invoices.length === 0" class="text-center py-4 text-muted">Chưa có hóa đơn nào ghi nhận.</div>
          <div v-else class="table-responsive">
            <table class="table table-hover align-middle bg-white rounded-4 overflow-hidden mb-0">
              <thead class="table-light">
                <tr>
                  <th>Mã HĐ</th>
                  <th>Ngày tạo</th>
                  <th>Dịch vụ chính</th>
                  <th>Tổng tiền</th>
                  <th>Trạng thái</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="inv in invoices" :key="inv.id">
                  <td class="fw-bold text-primary small">#{{ inv.id }}</td>
                  <td class="small text-muted">{{ formatDateFull(inv.createdAt) }}</td>
                  <td class="small fw-bold text-dark">{{ inv.items?.find(i => i.itemType === 'service')?.itemName || inv.items?.[0]?.itemName || '—' }}</td>
                  <td class="fw-bold text-danger">{{ formatCurrency(inv.totalAmount) }}</td>
                  <td>
                    <span class="badge rounded-pill" :class="inv.paymentStatus === 'paid' ? 'bg-success' : 'bg-warning text-dark'">
                      {{ inv.paymentStatus === 'paid' ? 'Đã thanh toán' : (inv.paymentStatus === 'pending' ? 'Chờ thanh toán' : inv.paymentStatus) }}
                    </span>
                  </td>
                  <td>
                    <button class="btn btn-sm btn-outline-secondary rounded-pill fw-bold" @click="printInvoice(inv)" :disabled="inv.paymentStatus !== 'paid'">
                      <i class="bi bi-printer"></i> In lại
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
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

const activeDetailTab = ref('appointments');
const invoices = ref<any[]>([]);
const loadingInvoices = ref(false);



const fetchInvoices = async () => {
  if (invoices.value.length > 0) return;
  loadingInvoices.value = true;
  try {
    const res = await api.get(`/receptionist/customers/${props.customerId}/invoices`);
    invoices.value = res.data;
  } catch (err) {
    console.error(err);
  } finally {
    loadingInvoices.value = false;
  }
};

watch(activeDetailTab, (newVal) => {
  if (newVal === 'invoices') fetchInvoices();
});

const printInvoice = async (invInfo: any) => {
  try {
    const res = await api.get(`/invoice/${invInfo.appointmentId}`);
    const invData = res.data;
    
    // Simple Print Window
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      alert("Trình duyệt đã chặn popup. Vui lòng cho phép popup để in hóa đơn.");
      return;
    }
    const fmtCur = (v: number) => (v ?? 0).toLocaleString('vi-VN') + 'đ';
    const now = new Date().toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
    
    const total = Math.max(0, invData.subtotal - (invData.discountAmount || 0));
    const payLabel = invData.paymentMethod === 'qr' ? 'Chuyển khoản VietQR' : 'Tiền mặt';

    const itemsHtml = (invData.items || []).map((item: any, idx: number) => `
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

    const discountRow = (invData.discountAmount > 0) ? `<tr style="border-bottom:1px dashed #e2e8f0"><td style="padding:5px 0;font-size:0.82rem;color:#475569">Giảm giá</td><td style="padding:5px 0;text-align:right;color:#ef4444">- ${fmtCur(invData.discountAmount)}</td></tr>` : '';

    const invoiceHtml = `
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
          <div class="inv-title">HÓA ĐƠN DỊCH VỤ (IN LẠI)</div>
          <div class="inv-meta">Số: <strong>#INV-${String(invData.id).padStart(5,'0')}</strong></div>
          <div class="inv-meta">Ngày: ${now}</div>
        </div>
      </div>

      <div class="info-row">
        <div class="info-box">
          <div class="info-label">THÔNG TIN KHÁCH HÀNG</div>
          <div class="info-line"><strong>${invData.customerName || invInfo.customerName || '—'}</strong></div>
          <div class="info-line">Điện thoại: ${invData.customerPhone || invInfo.customerPhone || 'N/A'}</div>
        </div>
        <div class="info-box">
          <div class="info-label">THÔNG TIN BỆNH NHÂN</div>
          <div class="info-line"><strong>${invData.petName || '—'}</strong> (${invData.petSpecies || 'Thú cưng'})</div>
          <div class="info-line">Bác sĩ phụ trách: ${invData.doctorName || '—'}</div>
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
          <tr style="border-bottom:1px dashed #e2e8f0"><td style="padding:5px 0;font-size:0.82rem;color:#475569">Tạm tính</td><td style="padding:5px 0;text-align:right;font-size:0.82rem">${fmtCur(invData.subtotal)}</td></tr>
          ${discountRow}
          <tr class="total-final"><td style="padding:5px 0">TỔNG CỘNG</td><td style="text-align:right;padding:5px 0">${fmtCur(total)}</td></tr>
          <tr><td style="padding:4px 0;font-size:0.75rem;color:#64748b" colspan="2">Hình thức: ${payLabel}</td></tr>
        </table>
      </div>

      <div class="footer">
        <div style="flex:1;font-size:0.78rem;color:#475569">
          <div style="font-weight:700;margin-bottom:4px">Ghi chú:</div>
          <div>Hóa đơn này là bản in lại hợp lệ tại MyPet Clinic.</div>
        </div>
        <div style="width:160px;text-align:center">
          <div style="font-weight:700;margin-bottom:4px">Xác nhận của phòng khám</div>
          <div class="sign-line"></div>
          <div style="font-size:0.75rem;color:#64748b">Thu ngân</div>
        </div>
      </div>
    </div>
    `;

    printWindow.document.write(`
      <!DOCTYPE html>
      <html lang="vi">
      <head>
        <meta charset="UTF-8">
        <title>In Hóa Đơn #${invData.id}</title>
        <style>
          @import url('https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@300;400;500;600;700;800&display=swap');
          * { box-sizing: border-box; margin: 0; padding: 0; }
          body { font-family: 'Be Vietnam Pro', sans-serif; color: #0f172a; font-size: 13px; line-height: 1.5; padding: 0; background: #e2e8f0; }
          
          .invoice-page { 
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
          .sign-line { border-bottom: 1px solid #0f172a; height: 60px; margin: 8px 0; }
          
          @media print {
            body { background: white; }
            .invoice-page { margin: 0; padding: 0; box-shadow: none; width: 100%; min-height: auto; }
            @page { size: A4 portrait; margin: 15mm; }
          }
        </style>
      </head>
      <body>
        ${invoiceHtml}
        <script>window.onload = function(){ setTimeout(() => { window.print(); window.onafterprint = function(){ window.close(); }; }, 500); }<\/script>
      </body>
      </html>
    `);
    printWindow.document.close();
  } catch (err) {
    console.error('Lỗi khi lấy dữ liệu in hóa đơn', err);
    alert('Có lỗi xảy ra khi lấy chi tiết hóa đơn để in.');
  }
};

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

const visiblePages = computed(() => {
  const pages: (number | string)[] = [];
  const current = currentPage.value;
  const total = totalPages.value;

  if (total <= 7) {
    for (let i = 1; i <= total; i++) pages.push(i);
  } else {
    pages.push(1);
    if (current > 3) pages.push('...');
    
    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    if (current < total - 2) pages.push('...');
    pages.push(total);
  }
  return pages;
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
