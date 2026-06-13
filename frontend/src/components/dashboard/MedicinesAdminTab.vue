<template>
  <div class="medicines-admin-tab container-fluid p-0 animate-fade-in">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-box-seam-fill text-warning me-2"></i>Quản lý Kho thuốc & Dược phẩm</h4>
        <p class="text-muted small mb-0">Theo dõi tồn kho, hạn sử dụng và cấu hình đơn giá thuốc trong phòng khám</p>
      </div>
      <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill me-2"></i> Nhập Thuốc Mới
      </button>
    </div>

    <!-- Warnings Dashboard Cards -->
    <div class="row g-3 mb-4" v-if="!loadingWarnings">
      <!-- Sắp hết hàng -->
      <div class="col-md-6">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-danger bg-opacity-10 text-danger h-100">
          <div class="d-flex justify-content-between align-items-center">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Thuốc sắp hết hàng</h6>
              <h4 class="fw-extrabold mb-0">{{ lowStockList.length }} <span class="fs-6 fw-normal">loại</span></h4>
            </div>
            <i class="bi bi-exclamation-octagon-fill fs-1 opacity-50"></i>
          </div>
          <div class="mt-2 border-top border-danger border-opacity-25 pt-2" v-if="lowStockList.length > 0">
            <div class="small fw-bold mb-1">Cần nhập kho gấp:</div>
            <div class="d-flex flex-wrap gap-1">
              <span v-for="item in lowStockList.slice(0, 3)" :key="item.id" class="badge bg-danger text-white rounded-pill px-2">
                {{ item.name }} (còn {{ item.stockQuantity }})
              </span>
              <span v-if="lowStockList.length > 3" class="small text-muted align-self-center">... và {{ lowStockList.length - 3 }} loại khác</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Sắp hết hạn -->
      <div class="col-md-6">
        <div class="card border-0 shadow-sm rounded-4 p-3 bg-warning bg-opacity-10 text-dark-gold h-100">
          <div class="d-flex justify-content-between align-items-center">
            <div>
              <h6 class="text-uppercase small fw-bold mb-1 opacity-75">Thuốc sắp hết hạn</h6>
              <h4 class="fw-extrabold mb-0">{{ expiringList.length }} <span class="fs-6 fw-normal">loại</span></h4>
            </div>
            <i class="bi bi-calendar-x-fill fs-1 opacity-50"></i>
          </div>
          <div class="mt-2 border-top border-warning border-opacity-25 pt-2" v-if="expiringList.length > 0">
            <div class="small fw-bold mb-1">Cần thanh lý/huỷ lô:</div>
            <div class="d-flex flex-wrap gap-1">
              <span v-for="item in expiringList.slice(0, 3)" :key="item.id" class="badge bg-warning text-dark rounded-pill px-2">
                {{ item.name }} (HSD: {{ formatDate(item.expiryDate) }})
              </span>
              <span v-if="expiringList.length > 3" class="small text-muted align-self-center">... và {{ expiringList.length - 3 }} loại khác</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters & Table -->
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="row g-2 mb-3 align-items-center">
        <div class="col-md-6">
          <div class="input-group">
            <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
            <input 
              type="text" 
              v-model="searchKeyword" 
              class="form-control border-start-0 input-premium" 
              placeholder="Tìm kiếm thuốc theo tên..."
            />
          </div>
        </div>
        <div class="col-md-6 text-md-end text-muted small">
          Tổng số: <strong class="text-dark">{{ filteredMedicines.length }}</strong> loại thuốc
        </div>
      </div>

      <!-- Medicines Table -->
      <div class="table-responsive rounded-4 border overflow-hidden mt-3">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Tên dược phẩm</th>
              <th>Đơn vị tính</th>
              <th>Tồn kho</th>
              <th>Giá nhập</th>
              <th>Giá bán</th>
              <th>Hạn sử dụng</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="7" class="py-5">
                <div class="spinner-border text-warning spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải danh sách dược phẩm...</span>
              </td>
            </tr>
            <tr v-else-if="filteredMedicines.length === 0" class="text-center">
              <td colspan="7" class="py-5 text-muted">
                <i class="bi bi-box2 fs-2 mb-2 d-block"></i>
                Không tìm thấy loại thuốc nào trong kho.
              </td>
            </tr>
            <tr v-for="med in filteredMedicines" :key="med.id" v-else>
              <td class="ps-4 fw-bold text-dark">
                {{ med.name }}
                <div class="text-muted small fw-normal text-truncate" style="max-width: 200px;">{{ med.description || 'Không có mô tả' }}</div>
              </td>
              <td>{{ med.unit || 'Lọ/Viên' }}</td>
              <td>
                <span :class="['badge rounded-pill px-3 py-1.5 fw-bold', med.stockQuantity <= 10 ? 'bg-danger text-white' : 'bg-success bg-opacity-10 text-success']">
                  {{ med.stockQuantity }}
                </span>
              </td>
              <td class="text-muted">{{ formatCurrency(med.importPrice) }}</td>
              <td class="fw-bold text-success">{{ formatCurrency(med.sellPrice) }}</td>
              <td>
                <span :class="{'text-danger fw-bold': isExpiring(med.expiryDate)}">
                  {{ formatDate(med.expiryDate) }}
                </span>
              </td>
              <td class="text-center">
                <button class="btn btn-sm btn-outline-warning rounded-pill px-3 me-2" @click="openEditModal(med)">
                  <i class="bi bi-pencil-fill me-1"></i>Sửa
                </button>
                <button class="btn btn-sm btn-outline-danger rounded-pill px-3" @click="handleDelete(med.id)">
                  <i class="bi bi-trash-fill me-1"></i>Xoá
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="showModal = false">
      <div class="zalo-modal-card max-w-500">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-box-seam-fill me-2"></i> {{ isEdit ? 'Cập Nhật Dược Phẩm' : 'Nhập Kho Dược Phẩm Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Tên thuốc *</label>
              <input type="text" v-model="form.name" class="form-control input-premium" required placeholder="Nhập tên thuốc..." />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Đơn vị tính *</label>
                <input type="text" v-model="form.unit" class="form-control" required placeholder="VD: Viên, Lọ, Chai..." />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Số lượng nhập kho *</label>
                <input type="number" v-model="form.stockQuantity" class="form-control" required min="0" />
              </div>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giá nhập (VND)</label>
                <input type="number" v-model="form.importPrice" class="form-control" min="0" />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Giá bán lẻ (VND) *</label>
                <input type="number" v-model="form.sellPrice" class="form-control" required min="0" />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Hạn sử dụng (HSD)</label>
              <input type="date" v-model="form.expiryDate" class="form-control" />
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Mô tả chi tiết</label>
              <textarea v-model="form.description" class="form-control" rows="3" placeholder="Cách dùng, thành phần thuốc..."></textarea>
            </div>

            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-4">Lưu lại</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

const loading = ref(false);
const loadingWarnings = ref(false);
const medicinesList = ref<any[]>([]);
const searchKeyword = ref('');

const lowStockList = ref<any[]>([]);
const expiringList = ref<any[]>([]);

const showModal = ref(false);
const isEdit = ref(false);
const currentMedicineId = ref<number | null>(null);

const form = ref({
  name: '',
  unit: '',
  stockQuantity: 0,
  importPrice: 0 as number | null,
  sellPrice: 0,
  expiryDate: '',
  description: ''
});

const filteredMedicines = computed(() => {
  const keyword = searchKeyword.value.toLowerCase().trim();
  return medicinesList.value.filter(m => 
    m.name.toLowerCase().includes(keyword)
  );
});

const loadMedicines = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/medicines');
    medicinesList.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải kho thuốc:', err);
  } finally {
    loading.value = false;
  }
};

const loadWarnings = async () => {
  loadingWarnings.value = true;
  try {
    const res = await api.get('/admin/medicines/warnings');
    if (res.data) {
      lowStockList.value = res.data.lowStock || [];
      expiringList.value = res.data.expiring || [];
    }
  } catch (err) {
    console.error('Lỗi tải cảnh báo kho thuốc:', err);
  } finally {
    loadingWarnings.value = false;
  }
};

const openCreateModal = () => {
  isEdit.value = false;
  currentMedicineId.value = null;
  form.value = {
    name: '',
    unit: 'Viên',
    stockQuantity: 100,
    importPrice: 0,
    sellPrice: 0,
    expiryDate: '',
    description: ''
  };
  showModal.value = true;
};

const openEditModal = (med: any) => {
  isEdit.value = true;
  currentMedicineId.value = med.id;
  form.value = {
    name: med.name,
    unit: med.unit || '',
    stockQuantity: med.stockQuantity || 0,
    importPrice: med.importPrice || 0,
    sellPrice: med.sellPrice || 0,
    expiryDate: med.expiryDate ? med.expiryDate.split('T')[0] : '',
    description: med.description || ''
  };
  showModal.value = true;
};

const submitForm = async () => {
  try {
    const payload = {
      ...form.value,
      expiryDate: form.value.expiryDate ? form.value.expiryDate : null
    };

    if (isEdit.value && currentMedicineId.value) {
      await api.put(`/admin/medicines/${currentMedicineId.value}`, payload);
      alert('Cập nhật dược phẩm thành công!');
    } else {
      await api.post('/admin/medicines', payload);
      alert('Nhập kho dược phẩm thành công!');
    }
    showModal.value = false;
    await loadMedicines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi lưu thông tin thuốc.');
  }
};

const handleDelete = async (id: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá thuốc này khỏi kho không?')) return;
  try {
    await api.delete(`/admin/medicines/${id}`);
    alert('Xoá dược phẩm thành công!');
    await loadMedicines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá dược phẩm.');
  }
};

const isExpiring = (dateStr: string) => {
  if (!dateStr) return false;
  const expiry = new Date(dateStr);
  const today = new Date();
  const diffTime = expiry.getTime() - today.getTime();
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays <= 30;
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return 'Không thời hạn';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatCurrency = (val: number | null) => {
  if (!val) return '0 đ';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

onMounted(() => {
  loadMedicines();
  loadWarnings();
});
</script>

<style scoped>
.bg-light-gold {
  background-color: #fdfaf0;
}
.text-dark-gold {
  color: #b25e00;
}
.input-premium {
  border: 1px solid #ffeed1;
  transition: all 0.3s ease;
}
.input-premium:focus {
  border-color: #f59e0b;
  box-shadow: 0 0 0 0.25rem rgba(245, 158, 11, 0.15);
}
.zalo-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(5px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}
.zalo-modal-card {
  background: white;
  width: 100%;
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  overflow: hidden;
}
.max-w-500 {
  max-width: 500px;
}
.zalo-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.zalo-modal-body {
  padding: 2rem 1.5rem;
}
</style>
