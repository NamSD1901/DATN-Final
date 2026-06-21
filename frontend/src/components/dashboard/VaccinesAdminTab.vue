<template>
  <div class="vaccines-admin-tab container-fluid p-0 animate-fade-in">
    <!-- Header -->
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
      <div>
        <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-droplet-half text-warning me-2"></i>Quản lý Vắc-xin</h4>
        <p class="text-muted small mb-0">Theo dõi thông tin vắc-xin và quản lý các lô nhập hàng</p>
      </div>
      <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateVaccineModal">
        <i class="bi bi-plus-circle-fill me-2"></i> Thêm Vắc-xin Mới
      </button>
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
              placeholder="Tìm kiếm vắc-xin theo tên..."
            />
          </div>
        </div>
        <div class="col-md-6 text-md-end text-muted small">
          Tổng số: <strong class="text-dark">{{ filteredVaccines.length }}</strong> loại vắc-xin
        </div>
      </div>

      <!-- Vaccines Table -->
      <div class="table-responsive rounded-4 border overflow-hidden mt-3">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Tên Vắc-xin</th>
              <th>Nhà sản xuất</th>
              <th>Loài chỉ định</th>
              <th>Tổng Tồn kho</th>
              <th>Số lượng lô</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="6" class="py-5">
                <div class="spinner-border text-warning spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải danh sách vắc-xin...</span>
              </td>
            </tr>
            <tr v-else-if="filteredVaccines.length === 0" class="text-center">
              <td colspan="6" class="py-5 text-muted">
                <i class="bi bi-box2 fs-2 mb-2 d-block"></i>
                Không tìm thấy vắc-xin nào.
              </td>
            </tr>
            <tr v-for="vac in filteredVaccines" :key="vac.id" v-else>
              <td class="ps-4 fw-bold text-dark">
                {{ vac.name }}
              </td>
              <td>{{ vac.manufacturer || 'Không rõ' }}</td>
              <td>
                <span class="badge bg-secondary rounded-pill">{{ vac.targetSpecies || 'Tất cả' }}</span>
              </td>
              <td>
                <span :class="['badge rounded-pill px-3 py-1.5 fw-bold', vac.stockQuantity <= 10 ? 'bg-danger text-white' : 'bg-success bg-opacity-10 text-success']">
                  {{ vac.stockQuantity }} liều
                </span>
              </td>
              <td class="text-muted">{{ vac.batches ? vac.batches.length : 0 }} lô</td>
              <td class="text-center">
                <button class="btn btn-sm btn-outline-primary rounded-pill px-3 me-2" @click="openBatchesModal(vac)">
                  <i class="bi bi-box-seam me-1"></i>Quản lý Lô
                </button>
                <button class="btn btn-sm btn-outline-warning rounded-pill px-3 me-2" @click="openEditVaccineModal(vac)">
                  <i class="bi bi-pencil-fill me-1"></i>Sửa
                </button>
                <button class="btn btn-sm btn-outline-danger rounded-pill px-3" @click="handleDeleteVaccine(vac.id)">
                  <i class="bi bi-trash-fill"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Vaccine Modal -->
    <div v-if="showVaccineModal" class="zalo-modal-overlay" @click.self="showVaccineModal = false">
      <div class="zalo-modal-card max-w-500">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-droplet-half me-2"></i> {{ isEditVaccine ? 'Cập Nhật Vắc-xin' : 'Thêm Vắc-xin Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showVaccineModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitVaccineForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Tên Vắc-xin *</label>
              <input type="text" v-model="vaccineForm.name" class="form-control input-premium" required placeholder="Nhập tên vắc-xin..." />
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Nhà sản xuất</label>
              <input type="text" v-model="vaccineForm.manufacturer" class="form-control" placeholder="Tên hãng sản xuất" />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-12">
                <label class="form-label text-muted small fw-bold">Loài chỉ định</label>
                <input type="text" v-model="vaccineForm.targetSpecies" class="form-control" placeholder="VD: Chó, Mèo..." />
              </div>
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Tuổi tối thiểu (Tuần)</label>
                <input type="number" v-model="vaccineForm.minAgeWeeks" class="form-control" min="0" />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Khoảng cách tiêm (Ngày)</label>
                <input type="number" v-model="vaccineForm.intervalDays" class="form-control" min="0" placeholder="VD: 365" />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Mô tả</label>
              <textarea v-model="vaccineForm.description" class="form-control" rows="2" placeholder="Ghi chú thêm..."></textarea>
            </div>
            <div class="mt-4 pt-3 border-top text-end">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 me-2" @click="showVaccineModal = false">Hủy</button>
              <button type="submit" class="btn btn-premium rounded-pill px-4">Lưu lại</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Manage Batches Modal -->
    <div v-if="showBatchesModal" class="zalo-modal-overlay" @click.self="showBatchesModal = false">
      <div class="zalo-modal-card" style="max-width: 800px; width: 90%;">
        <div class="zalo-modal-header bg-primary text-white">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-box-seam me-2"></i> Lô Vắc-xin: {{ currentVaccine?.name }}
          </h5>
          <button class="modal-close text-white border-0 bg-transparent" @click="showBatchesModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body p-0">
          <div class="p-4 bg-light border-bottom">
            <h6 class="fw-bold mb-3">Thêm Lô Mới</h6>
            <form @submit.prevent="submitBatchForm" class="row g-2 align-items-end">
              <div class="col-md-3">
                <label class="small text-muted mb-1">Số lô *</label>
                <input type="text" v-model="batchForm.batchNumber" class="form-control form-control-sm" required placeholder="Mã lô" />
              </div>
              <div class="col-md-3">
                <label class="small text-muted mb-1">Hạn sử dụng *</label>
                <input type="date" v-model="batchForm.expirationDate" class="form-control form-control-sm" required />
              </div>
              <div class="col-md-2">
                <label class="small text-muted mb-1">Số lượng *</label>
                <input type="number" v-model="batchForm.stockQuantity" class="form-control form-control-sm" required min="1" />
              </div>
              <div class="col-md-2">
                <label class="small text-muted mb-1">Giá bán *</label>
                <input type="number" v-model="batchForm.sellingPrice" class="form-control form-control-sm" required min="0" />
              </div>
              <div class="col-md-2">
                <button type="submit" class="btn btn-primary btn-sm w-100 fw-bold">Thêm Lô</button>
              </div>
            </form>
          </div>
          
          <div class="p-4">
            <h6 class="fw-bold mb-3">Danh sách Lô hiện tại</h6>
            <div class="table-responsive">
              <table class="table table-sm table-bordered align-middle">
                <thead class="table-light">
                  <tr>
                    <th>Số lô</th>
                    <th>Ngày nhập</th>
                    <th>Hạn sử dụng</th>
                    <th>Tồn kho</th>
                    <th>Giá bán</th>
                    <th class="text-center">Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="!currentVaccine?.batches || currentVaccine.batches.length === 0">
                    <td colspan="6" class="text-center text-muted py-3">Chưa có lô nào.</td>
                  </tr>
                  <tr v-for="batch in currentVaccine?.batches" :key="batch.id" v-else>
                    <td class="fw-bold">{{ batch.batchNumber }}</td>
                    <td>{{ formatDate(batch.importDate) }}</td>
                    <td>
                      <span :class="{'text-danger fw-bold': isExpiring(batch.expirationDate)}">
                        {{ formatDate(batch.expirationDate) }}
                      </span>
                    </td>
                    <td>
                      <span class="badge bg-success bg-opacity-10 text-success">{{ batch.stockQuantity }}</span>
                    </td>
                    <td>{{ formatCurrency(batch.sellingPrice) }}</td>
                    <td class="text-center">
                      <button class="btn btn-sm btn-outline-danger" @click="handleDeleteBatch(batch.id)">
                        <i class="bi bi-trash"></i>
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

  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import api from '../../services/api';

const loading = ref(false);
const vaccinesList = ref<any[]>([]);
const searchKeyword = ref('');

const showVaccineModal = ref(false);
const isEditVaccine = ref(false);
const currentVaccineId = ref<number | null>(null);

const showBatchesModal = ref(false);
const currentVaccine = ref<any>(null);

const vaccineForm = ref({
  name: '',
  manufacturer: '',
  description: '',
  targetSpecies: '',
  minAgeWeeks: null as number | null,
  intervalDays: null as number | null
});

const batchForm = ref({
  batchNumber: '',
  expirationDate: '',
  importDate: new Date().toISOString().split('T')[0],
  stockQuantity: 10,
  importPrice: 0,
  sellingPrice: 0
});

const filteredVaccines = computed(() => {
  const keyword = searchKeyword.value.toLowerCase().trim();
  return vaccinesList.value.filter(v => 
    v.name.toLowerCase().includes(keyword) || 
    (v.manufacturer && v.manufacturer.toLowerCase().includes(keyword))
  );
});

const loadVaccines = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/vaccines');
    vaccinesList.value = res.data || [];
    
    // Update currentVaccine if batches modal is open
    if (showBatchesModal.value && currentVaccine.value) {
      currentVaccine.value = vaccinesList.value.find(v => v.id === currentVaccine.value.id);
    }
  } catch (err) {
    console.error('Lỗi tải danh sách vắc-xin:', err);
  } finally {
    loading.value = false;
  }
};

const openCreateVaccineModal = () => {
  isEditVaccine.value = false;
  currentVaccineId.value = null;
  vaccineForm.value = {
    name: '',
    manufacturer: '',
    description: '',
    targetSpecies: '',
    minAgeWeeks: null,
    intervalDays: null
  };
  showVaccineModal.value = true;
};

const openEditVaccineModal = (vac: any) => {
  isEditVaccine.value = true;
  currentVaccineId.value = vac.id;
  vaccineForm.value = {
    name: vac.name,
    manufacturer: vac.manufacturer || '',
    description: vac.description || '',
    targetSpecies: vac.targetSpecies || '',
    minAgeWeeks: vac.minAgeWeeks,
    intervalDays: vac.intervalDays
  };
  showVaccineModal.value = true;
};

const submitVaccineForm = async () => {
  try {
    if (isEditVaccine.value && currentVaccineId.value) {
      await api.put(`/admin/vaccines/${currentVaccineId.value}`, vaccineForm.value);
      alert('Cập nhật vắc-xin thành công!');
    } else {
      await api.post('/admin/vaccines', vaccineForm.value);
      alert('Thêm vắc-xin thành công!');
    }
    showVaccineModal.value = false;
    await loadVaccines();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi lưu vắc-xin.');
  }
};

const handleDeleteVaccine = async (id: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá vắc-xin này không? Lưu ý không thể xoá vắc-xin đang còn tồn kho.')) return;
  try {
    await api.delete(`/admin/vaccines/${id}`);
    alert('Xoá vắc-xin thành công!');
    await loadVaccines();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá vắc-xin.');
  }
};

const openBatchesModal = (vac: any) => {
  currentVaccine.value = vac;
  batchForm.value = {
    batchNumber: '',
    expirationDate: '',
    importDate: new Date().toISOString().split('T')[0],
    stockQuantity: 10,
    importPrice: 0,
    sellingPrice: 0
  };
  showBatchesModal.value = true;
};

const submitBatchForm = async () => {
  if (!currentVaccine.value) return;
  try {
    await api.post(`/admin/vaccines/${currentVaccine.value.id}/batches`, batchForm.value);
    alert('Thêm lô vắc-xin thành công!');
    batchForm.value = {
      batchNumber: '',
      expirationDate: '',
      importDate: new Date().toISOString().split('T')[0],
      stockQuantity: 10,
      importPrice: 0,
      sellingPrice: 0
    };
    await loadVaccines();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi thêm lô vắc-xin.');
  }
};

const handleDeleteBatch = async (batchId: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá lô này không? (Chỉ có thể xoá nếu tồn kho = 0)')) return;
  try {
    await api.delete(`/admin/vaccines/batches/${batchId}`);
    alert('Xoá lô thành công!');
    await loadVaccines();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá lô vắc-xin.');
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
  if (!dateStr) return 'N/A';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatCurrency = (val: number | null) => {
  if (!val) return '0 đ';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

onMounted(() => {
  loadVaccines();
});
</script>

<style scoped>
.bg-light-gold {
  background-color: #fdfaf0;
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
