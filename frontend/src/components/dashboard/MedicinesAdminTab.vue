<template>
  <div class="medicines-admin-tab container-fluid p-0 animate-fade-in-up">
    <!-- Header Area -->
    <div class="d-flex flex-wrap justify-content-between align-items-end mb-4 gap-3 premium-header">
      <div>
        <h4 class="fw-extrabold mb-2 text-dark gradient-text">
          <i class="bi bi-box-seam text-primary me-2"></i>Quản lý Kho thuốc & Dược phẩm
        </h4>
        <p class="text-muted small mb-0 fw-medium">
          Theo dõi tồn kho, cảnh báo hết hạn và quản lý các lô hàng dược phẩm một cách chuyên nghiệp.
        </p>
      </div>
      <div class="d-flex gap-2">
        <button class="btn btn-modern-primary px-4 py-2.5 rounded-pill shadow-hover transition-all" @click="openCreateModal">
          <i class="bi bi-plus-circle me-2"></i> Thêm Dược Phẩm Mới
        </button>
      </div>
    </div>

    <!-- Warnings Dashboard Cards -->
    <div class="row g-4 mb-4" v-if="!loadingWarnings">
      <!-- Sắp hết hàng -->
      <div class="col-md-6">
        <div class="modern-card warning-card danger-theme h-100 p-4 rounded-4 position-relative overflow-hidden">
          <div class="bg-shape bg-shape-danger"></div>
          <div class="position-relative z-1">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <div>
                <h6 class="text-uppercase small fw-bold mb-1 text-danger opacity-75 letter-spacing-1">Sắp hết hàng</h6>
                <h3 class="fw-extrabold mb-0 text-dark">{{ lowStockList.length }} <span class="fs-6 fw-normal text-muted">sản phẩm</span></h3>
              </div>
              <div class="icon-circle bg-danger bg-opacity-10 text-danger">
                <i class="bi bi-exclamation-octagon-fill fs-3"></i>
              </div>
            </div>
            
            <div class="mt-3" v-if="lowStockList.length > 0">
              <div class="small fw-semibold mb-2 text-muted">Cần nhập kho ngay:</div>
              <div class="d-flex flex-wrap gap-2 custom-scrollbar" style="max-height: 90px; overflow-y: auto; padding-right: 5px;">
                <span v-for="item in lowStockList" :key="item.id" class="modern-badge bg-danger text-white shadow-sm">
                  {{ item.name }} <span class="badge-qty ms-1">còn {{ item.stockQuantity }}</span>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Sắp hết hạn -->
      <div class="col-md-6">
        <div class="modern-card warning-card warning-theme h-100 p-4 rounded-4 position-relative overflow-hidden">
          <div class="bg-shape bg-shape-warning"></div>
          <div class="position-relative z-1">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <div>
                <h6 class="text-uppercase small fw-bold mb-1 text-warning-dark opacity-75 letter-spacing-1">Sắp hết hạn</h6>
                <h3 class="fw-extrabold mb-0 text-dark">{{ expiringList.length }} <span class="fs-6 fw-normal text-muted">lô thuốc</span></h3>
              </div>
              <div class="icon-circle bg-warning bg-opacity-25 text-warning-dark">
                <i class="bi bi-calendar-x-fill fs-3"></i>
              </div>
            </div>
            
            <div class="mt-3" v-if="expiringList.length > 0">
              <div class="small fw-semibold mb-2 text-muted">Cần thanh lý/huỷ lô:</div>
              <div class="d-flex flex-wrap gap-2 custom-scrollbar" style="max-height: 90px; overflow-y: auto; padding-right: 5px;">
                <span v-for="item in expiringList" :key="item.batchId" class="modern-badge bg-warning-light text-warning-dark shadow-sm">
                  {{ item.medicineName }} (Lô {{ item.batchNumber }}) <span class="badge-qty text-muted ms-1">HSD: {{ formatDate(item.expiryDate) }}</span>
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters & Table Section -->
    <div class="modern-card bg-white p-4 rounded-4 shadow-sm mb-4">
      <div class="row g-3 mb-4 align-items-center">
        <div class="col-md-5">
          <div class="modern-search-bar">
            <i class="bi bi-search search-icon"></i>
            <input 
              type="text" 
              v-model="searchKeyword" 
              class="form-control form-control-lg border-0 shadow-none bg-transparent" 
              placeholder="Tìm kiếm thuốc theo tên..."
            />
          </div>
        </div>
        <div class="col-md-7 text-md-end text-muted small fw-medium">
          Đang hiển thị <strong class="text-primary fs-6">{{ filteredMedicines.length }}</strong> danh mục thuốc
        </div>
      </div>

      <!-- Medicines Table -->
      <div class="table-responsive rounded-3 border-0 overflow-visible">
        <table class="table modern-table align-middle mb-0">
          <thead>
            <tr>
              <th class="ps-4">Tên dược phẩm</th>
              <th width="10%">Đơn vị</th>
              <th width="12%">Giá bán</th>
              <th width="15%">Tổng Tồn kho</th>
              <th width="12%">Số lượng lô</th>
              <th width="25%" class="text-end pe-4">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center bg-transparent">
              <td colspan="6" class="py-5 border-0">
                <div class="spinner-border text-primary spinner-border-sm me-2"></div>
                <span class="text-muted fw-medium">Đang tải dữ liệu kho thuốc...</span>
              </td>
            </tr>
            <tr v-else-if="filteredMedicines.length === 0" class="text-center bg-transparent">
              <td colspan="6" class="py-5 border-0 text-muted">
                <div class="empty-state-icon mx-auto mb-3"><i class="bi bi-box2"></i></div>
                <h6 class="fw-bold text-dark">Không tìm thấy dược phẩm</h6>
                <p class="small mb-0">Thử thay đổi từ khóa tìm kiếm hoặc thêm mới dược phẩm.</p>
              </td>
            </tr>
            <tr v-for="med in filteredMedicines" :key="med.id" class="table-row-hover transition-all" v-else>
              <td class="ps-4 border-0">
                <div class="d-flex align-items-center gap-3 py-2">
                  <div class="item-avatar bg-primary bg-opacity-10 text-primary fw-bold rounded-circle d-flex align-items-center justify-content-center">
                    {{ med.name.charAt(0).toUpperCase() }}
                  </div>
                  <div>
                    <h6 class="mb-1 fw-bold text-dark">{{ med.name }}</h6>
                    <div class="text-muted small text-truncate" style="max-width: 250px;">{{ med.description || 'Không có mô tả' }}</div>
                  </div>
                </div>
              </td>
              <td class="border-0 text-muted fw-medium">{{ med.unit || 'Lọ/Viên' }}</td>
              <td class="border-0">
                <div class="text-primary fw-bold">{{ formatCurrency(med.sellPrice) }}</div>
                <div class="small text-muted">Nhập: {{ formatCurrency(med.importPrice) }}</div>
              </td>
              <td class="border-0">
                <span :class="['status-badge fw-bold px-3 py-1.5', med.stockQuantity <= 10 ? 'danger' : 'success']">
                  <i :class="['bi me-1', med.stockQuantity <= 10 ? 'bi-exclamation-triangle-fill' : 'bi-check-circle-fill']"></i>
                  {{ med.stockQuantity }}
                </span>
              </td>
              <td class="border-0 text-muted fw-medium">
                <span class="badge bg-light text-dark border px-2 py-1"><i class="bi bi-layers me-1"></i>{{ med.batches ? med.batches.length : 0 }} lô</span>
              </td>
              <td class="text-end pe-4 border-0">
                <div class="d-flex justify-content-end gap-2 action-buttons">
                  <button class="btn btn-sm btn-action btn-manage" @click="openBatchesModal(med)" title="Quản lý lô">
                    <i class="bi bi-box-seam me-1"></i> <span>Quản lý Lô</span>
                  </button>
                  <button class="btn btn-sm btn-action btn-edit" @click="openEditModal(med)" title="Sửa thông tin">
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                  <button class="btn btn-sm btn-action btn-delete" @click="handleDelete(med.id)" title="Xoá thuốc">
                    <i class="bi bi-trash-fill"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showModal" class="premium-modal-overlay" @click.self="showModal = false">
          <div class="premium-modal-card max-w-500 animate-slide-up">
            <div class="modal-header-elegant">
              <h5 class="fw-extrabold mb-0 text-dark">
                <span class="icon-box bg-primary bg-opacity-10 text-primary me-2"><i class="bi bi-capsule"></i></span>
                {{ isEdit ? 'Cập Nhật Dược Phẩm' : 'Thêm Dược Phẩm Mới' }}
              </h5>
              <button class="btn-close-elegant" @click="showModal = false"><i class="bi bi-x"></i></button>
            </div>
            <div class="modal-body-elegant">
              <form @submit.prevent="submitForm">
                <div class="modern-form-group mb-4">
                  <input type="text" v-model="form.name" class="modern-input" id="medName" required placeholder=" " />
                  <label for="medName" class="modern-label">Tên dược phẩm <span class="text-danger">*</span></label>
                </div>
                <div class="modern-form-group mb-4">
                  <input type="text" v-model="form.unit" class="modern-input" id="medUnit" required placeholder=" " />
                  <label for="medUnit" class="modern-label">Đơn vị tính (Viên, Lọ...) <span class="text-danger">*</span></label>
                </div>
                
                <div class="row g-3 mb-4">
                  <div class="col-md-6">
                    <div class="modern-form-group">
                      <input type="number" v-model="form.importPrice" class="modern-input" id="medImportPrice" required min="0" placeholder=" " />
                      <label for="medImportPrice" class="modern-label">Giá nhập (VNĐ) <span class="text-danger">*</span></label>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="modern-form-group">
                      <input type="number" v-model="form.sellPrice" class="modern-input" id="medSellPrice" required min="0" placeholder=" " />
                      <label for="medSellPrice" class="modern-label">Giá bán (VNĐ) <span class="text-danger">*</span></label>
                    </div>
                  </div>
                </div>

                <div class="modern-form-group mb-4">
                  <textarea v-model="form.description" class="modern-input" id="medDesc" rows="3" placeholder=" "></textarea>
                  <label for="medDesc" class="modern-label">Mô tả, thành phần, cách dùng...</label>
                </div>

                <div class="d-flex justify-content-end gap-2 mt-5">
                  <button type="button" class="btn btn-light px-4 py-2 rounded-pill fw-bold" @click="showModal = false">Hủy</button>
                  <button type="submit" class="btn btn-modern-primary px-4 py-2 rounded-pill fw-bold shadow-sm">
                    <i class="bi bi-check2-circle me-1"></i> Lưu Dữ Liệu
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Manage Batches Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showBatchesModal" class="premium-modal-overlay" @click.self="showBatchesModal = false">
          <div class="premium-modal-card max-w-900 animate-slide-up">
            <div class="modal-header-elegant border-bottom border-light">
              <div>
                <h5 class="fw-extrabold mb-1 text-dark">
                  <span class="icon-box bg-success bg-opacity-10 text-success me-2"><i class="bi bi-boxes"></i></span>
                  Quản lý Lô Dược phẩm
                </h5>
                <p class="text-muted small mb-0 ms-5">{{ currentMedicine?.name }} • {{ currentMedicine?.unit || 'Chưa cập nhật đơn vị' }}</p>
              </div>
              <button class="btn-close-elegant" @click="showBatchesModal = false"><i class="bi bi-x"></i></button>
            </div>
            
            <div class="modal-body-elegant p-0 bg-light-soft">
              <!-- Add New Batch Section -->
              <div class="p-4 bg-white shadow-sm rounded-bottom-0 position-relative z-1">
                <div class="d-flex justify-content-between align-items-center mb-3">
                  <h6 class="fw-bold text-dark mb-0"><i class="bi bi-plus-circle-dotted text-primary me-2"></i>Thêm Lô Mới (Nhập Kho)</h6>
                  <button type="button" class="btn btn-sm btn-outline-primary rounded-pill" @click="addBatchRow">
                    <i class="bi bi-plus-lg me-1"></i>Thêm dòng
                  </button>
                </div>
                
                <form @submit.prevent="submitBatchForm" class="bg-light p-3 rounded-3 border border-light-subtle">
                  <div class="custom-scrollbar" style="max-height: 320px; overflow-y: auto; overflow-x: hidden; padding-right: 5px;">
                    <div v-for="(form, index) in batchForms" :key="index" class="row g-3 align-items-center mb-2 pb-2" :class="{'border-bottom border-light-subtle': index < batchForms.length - 1}">
                      <div class="col-md-3">
                        <div class="modern-form-group-sm">
                          <input type="text" v-model="form.batchNumber" class="modern-input-sm" required placeholder=" " :id="'bNum'+index" />
                          <label :for="'bNum'+index">Mã Số Lô *</label>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="modern-form-group-sm">
                          <input type="date" v-model="form.manufactureDate" class="modern-input-sm" required :id="'bMfx'+index" />
                          <label :for="'bMfx'+index">Ngày Sản Xuất *</label>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="modern-form-group-sm">
                          <input type="date" v-model="form.expiryDate" class="modern-input-sm" required :id="'bExp'+index" />
                          <label :for="'bExp'+index">Hạn Sử Dụng *</label>
                        </div>
                      </div>
                      <div class="col-md-2">
                        <div class="modern-form-group-sm">
                          <input type="number" v-model="form.quantity" class="modern-input-sm" required min="1" :id="'bQty'+index" placeholder=" " />
                          <label :for="'bQty'+index">Số Lượng *</label>
                        </div>
                      </div>
                      <div class="col-md-1 d-flex justify-content-end">
                        <button v-if="batchForms.length > 1" type="button" class="btn btn-sm btn-light text-danger rounded-circle action-icon-btn" @click="removeBatchRow(index)" title="Xoá dòng">
                          <i class="bi bi-x-lg"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <div class="d-flex justify-content-end mt-3 border-top border-light-subtle pt-3">
                    <button type="submit" class="btn btn-modern-primary px-4 py-2 rounded-pill shadow-sm">
                      <i class="bi bi-check2-circle me-2"></i>Lưu các lô
                    </button>
                  </div>
                </form>
              </div>
              
              <!-- Batches List Section -->
              <div class="p-4">
                <h6 class="fw-bold mb-3 text-dark"><i class="bi bi-list-task text-primary me-2"></i>Danh Sách Các Lô Hiện Tại</h6>
                <div class="table-responsive bg-white rounded-3 border shadow-sm">
                  <table class="table modern-table table-hover align-middle mb-0">
                    <thead class="table-light text-uppercase small text-muted letter-spacing-1">
                      <tr>
                        <th class="ps-4">Số lô</th>
                        <th>Ngày sản xuất</th>
                        <th>Hạn sử dụng</th>
                        <th>Tồn kho lô</th>
                        <th class="text-end pe-4">Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-if="!currentMedicine?.batches || currentMedicine.batches.length === 0">
                        <td colspan="5" class="text-center text-muted py-5">
                          <i class="bi bi-inboxes text-light fs-1 d-block mb-2"></i>
                          Dược phẩm này chưa có lô nào trong kho.
                        </td>
                      </tr>
                      <tr v-for="batch in currentMedicine?.batches" :key="batch.id" v-else>
                        <td class="ps-4 fw-bold text-dark">{{ batch.batchNumber }}</td>
                        <td class="text-muted fw-medium">{{ formatDate(batch.manufactureDate) }}</td>
                        <td>
                          <span :class="['status-badge px-2 py-1', isExpiring(batch.expiryDate) ? 'danger' : 'muted']">
                            <i :class="['bi me-1', isExpiring(batch.expiryDate) ? 'bi-exclamation-circle-fill' : 'bi-calendar-check']"></i>
                            {{ formatDate(batch.expiryDate) }}
                          </span>
                        </td>
                        <td>
                          <span class="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 px-2 py-1 rounded-pill">
                            {{ batch.currentQuantity }} {{ currentMedicine.unit }}
                          </span>
                        </td>
                        <td class="text-end pe-4 d-flex justify-content-end gap-2">
                          <button class="btn btn-sm btn-light text-success rounded-circle action-icon-btn" @click="handleAddMoreStock(batch)" title="Nhập thêm (Cộng dồn)" :disabled="isExpired(batch.expiryDate)">
                            <i class="bi bi-plus-lg"></i>
                          </button>
                          <button class="btn btn-sm btn-light text-danger rounded-circle action-icon-btn" @click="handleDeleteBatch(batch.id)" title="Xoá Lô">
                            <i class="bi bi-trash3-fill"></i>
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
      </Transition>
    </Teleport>
    <!-- Add More Stock Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showAddMoreModal" class="premium-modal-overlay" style="z-index: 1300;" @click.self="showAddMoreModal = false">
          <div class="premium-modal-card max-w-500 animate-slide-up">
            <div class="modal-header-elegant border-bottom border-light">
              <h5 class="fw-extrabold mb-0 text-dark">
                <span class="icon-box bg-success bg-opacity-10 text-success me-2"><i class="bi bi-plus-circle"></i></span>
                Nhập Thêm Số Lượng
              </h5>
              <button class="btn-close-elegant" @click="showAddMoreModal = false"><i class="bi bi-x"></i></button>
            </div>
            <div class="modal-body-elegant text-center py-4">
              <div class="mb-3">
                <p class="text-muted mb-1">Đang nhập thêm cho lô:</p>
                <h5 class="fw-bold text-primary">{{ batchToAddMore?.batchNumber }}</h5>
              </div>
              <div class="modern-form-group mx-auto" style="max-width: 200px;">
                <input type="number" v-model="addMoreQuantity" class="modern-input text-center fs-4 fw-bold" required min="1" placeholder="0" />
                <label class="modern-label text-center w-100">Số lượng ({{ currentMedicine?.unit || 'viên' }})</label>
              </div>
            </div>
            <div class="modal-footer border-top-0 d-flex justify-content-center pb-4 gap-2">
              <button type="button" class="btn btn-light px-4 py-2 rounded-pill fw-bold" @click="showAddMoreModal = false">Hủy</button>
              <button type="button" class="btn btn-modern-primary px-4 py-2 rounded-pill fw-bold shadow-sm" @click="confirmAddMoreStock" :disabled="!addMoreQuantity || addMoreQuantity <= 0 || isSubmittingAddMore">
                <span v-if="isSubmittingAddMore" class="spinner-border spinner-border-sm me-2"></span>
                <i v-else class="bi bi-check2-circle me-1"></i> Xác Nhận
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
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

const showBatchesModal = ref(false);
const currentMedicine = ref<any>(null);

const showAddMoreModal = ref(false);
const addMoreQuantity = ref(0);
const batchToAddMore = ref<any>(null);
const isSubmittingAddMore = ref(false);

const form = ref({
  name: '',
  unit: '',
  description: '',
  importPrice: 0,
  sellPrice: 0
});

const batchForms = ref([{
  batchNumber: '',
  manufactureDate: new Date().toISOString().split('T')[0],
  expiryDate: '',
  quantity: 10
}]);

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
    
    if (showBatchesModal.value && currentMedicine.value) {
      currentMedicine.value = medicinesList.value.find(m => m.id === currentMedicine.value.id);
    }
  } catch (err) {
    console.error('Lỗi tải kho thuốc:', err);
  } finally {
    loading.value = false;
  }
};

const loadWarnings = async () => {
  loadingWarnings.value = true;
  try {
    const [lowStockRes, expiringRes] = await Promise.all([
      api.get('/medicines/low-stock'),
      api.get('/medicines/expiring')
    ]);
    lowStockList.value = lowStockRes.data || [];
    expiringList.value = expiringRes.data || [];
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
    description: '',
    importPrice: 0,
    sellPrice: 0
  };
  showModal.value = true;
};

const openEditModal = (med: any) => {
  isEdit.value = true;
  currentMedicineId.value = med.id;
  form.value = {
    name: med.name,
    unit: med.unit || '',
    description: med.description || '',
    importPrice: med.importPrice || 0,
    sellPrice: med.sellPrice || 0
  };
  showModal.value = true;
};

const submitForm = async () => {
  try {
    const payload = { ...form.value };

    if (isEdit.value && currentMedicineId.value) {
      await api.put(`/admin/medicines/${currentMedicineId.value}`, payload);
      alert('Cập nhật dược phẩm thành công!');
    } else {
      await api.post('/admin/medicines', payload);
      alert('Thêm mới danh mục dược phẩm thành công!');
    }
    showModal.value = false;
    await loadMedicines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi lưu thông tin thuốc.');
  }
};

const openBatchesModal = (med: any) => {
  currentMedicine.value = med;
  batchForms.value = [{
    batchNumber: '',
    manufactureDate: new Date().toISOString().split('T')[0],
    expiryDate: '',
    quantity: 10
  }];
  showBatchesModal.value = true;
};

const addBatchRow = () => {
  batchForms.value.push({
    batchNumber: '',
    manufactureDate: new Date().toISOString().split('T')[0],
    expiryDate: '',
    quantity: 10
  });
};

const removeBatchRow = (index: number) => {
  batchForms.value.splice(index, 1);
};

const submitBatchForm = async () => {
  if (!currentMedicine.value) return;
  try {
    const promises = batchForms.value.map(form => {
      const payload = {
        medicineId: currentMedicine.value.id,
        batchNumber: form.batchNumber,
        manufactureDate: new Date(form.manufactureDate).toISOString(),
        expiryDate: new Date(form.expiryDate).toISOString(),
        quantity: form.quantity,
        notes: "Nhập kho từ Quản lý Lô"
      };
      return api.post('/medicines/import', payload);
    });

    await Promise.all(promises);

    alert('Thêm lô dược phẩm thành công!');
    batchForms.value = [{
      batchNumber: '',
      manufactureDate: new Date().toISOString().split('T')[0],
      expiryDate: '',
      quantity: 10
    }];
    await loadMedicines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi thêm lô dược phẩm.');
  }
};

const handleAddMoreStock = (batch: any) => {
  batchToAddMore.value = batch;
  addMoreQuantity.value = 10;
  showAddMoreModal.value = true;
};

const confirmAddMoreStock = async () => {
  if (!batchToAddMore.value) return;
  if (addMoreQuantity.value <= 0) {
    alert('Số lượng nhập thêm không hợp lệ.');
    return;
  }
  
  isSubmittingAddMore.value = true;
  try {
    const payload = {
      medicineId: currentMedicine.value.id,
      batchNumber: batchToAddMore.value.batchNumber,
      manufactureDate: batchToAddMore.value.manufactureDate,
      expiryDate: batchToAddMore.value.expiryDate,
      quantity: addMoreQuantity.value,
      notes: "Nhập thêm (Cộng dồn) trực tiếp từ danh sách"
    };
    await api.post('/medicines/import', payload);
    showAddMoreModal.value = false;
    alert('Nhập thêm thành công!');
    await loadMedicines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi nhập thêm.');
  } finally {
    isSubmittingAddMore.value = false;
  }
};

const handleDeleteBatch = async (batchId: number) => {
  if (!confirm('Bạn có chắc chắn muốn xoá lô này không? (Chỉ có thể xoá nếu tồn kho lô = 0)')) return;
  try {
    await api.delete(`/admin/medicines/batches/${batchId}`);
    alert('Xoá lô thành công!');
    await loadMedicines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá lô dược phẩm.');
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

const isExpired = (dateStr: string) => {
  if (!dateStr) return false;
  const expiry = new Date(dateStr);
  const today = new Date();
  expiry.setHours(0,0,0,0);
  today.setHours(0,0,0,0);
  return expiry.getTime() < today.getTime();
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return 'Không thời hạn';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount || 0);
};

onMounted(() => {
  loadMedicines();
  loadWarnings();
});
</script>

<style scoped>
/* Animations */
.animate-fade-in-up {
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-slide-up {
  animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes slideUp {
  from { opacity: 0; transform: translateY(40px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

/* Transitions for Vue <Transition> */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

/* Premium Utilities */
.letter-spacing-1 { letter-spacing: 0.5px; }
.transition-all { transition: all 0.3s ease; }
.bg-light-soft { background-color: #f8fafc; }

/* Text Gradients */
.gradient-text {
  background: linear-gradient(135deg, #1e293b 0%, #334155 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

/* Modern Buttons */
.btn-modern-primary {
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border: none;
  font-weight: 600;
  box-shadow: 0 4px 14px 0 rgba(37, 99, 235, 0.39);
}
.btn-modern-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(37, 99, 235, 0.23);
  color: white;
}

/* Dashboard Warning Cards with Glassmorphism / Modern Shapes */
.modern-card {
  border: 1px solid rgba(0,0,0,0.04);
  background: #ffffff;
}
.warning-card {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}
.warning-card:hover {
  transform: translateY(-5px);
}
.danger-theme {
  background: linear-gradient(135deg, #fff0f0 0%, #fff 100%);
  border: 1px solid #ffe1e1;
}
.danger-theme:hover {
  box-shadow: 0 10px 30px rgba(239, 68, 68, 0.1);
}
.warning-theme {
  background: linear-gradient(135deg, #fffbeb 0%, #fff 100%);
  border: 1px solid #fef3c7;
}
.warning-theme:hover {
  box-shadow: 0 10px 30px rgba(245, 158, 11, 0.1);
}
.bg-shape {
  position: absolute;
  top: -20px;
  right: -20px;
  width: 150px;
  height: 150px;
  border-radius: 50%;
  opacity: 0.1;
  z-index: 0;
}
.bg-shape-danger { background: #ef4444; }
.bg-shape-warning { background: #f59e0b; }

.icon-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}
.text-warning-dark { color: #b45309; }
.bg-warning-light { background-color: #fef3c7; }

/* Modern Badges */
.modern-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
}
.badge-qty {
  font-weight: 400;
  opacity: 0.9;
}

/* Status Badges */
.status-badge {
  display: inline-flex;
  align-items: center;
  border-radius: 6px;
  font-size: 0.85rem;
}
.status-badge.success { background: #ecfdf5; color: #059669; }
.status-badge.danger { background: #fef2f2; color: #dc2626; }
.status-badge.muted { background: #f3f4f6; color: #4b5563; }

/* Search Bar */
.modern-search-bar {
  position: relative;
  background: #f8fafc;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  transition: all 0.3s;
}
.modern-search-bar:focus-within {
  background: #ffffff;
  border-color: #3b82f6;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}
.search-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 1.1rem;
}
.modern-search-bar input {
  padding-left: 45px;
  font-size: 0.95rem;
}

/* Table Design */
.modern-table th {
  border-bottom: 2px solid #f1f5f9;
  color: #64748b;
  font-weight: 600;
  padding-bottom: 1rem;
}
.table-row-hover td {
  border-bottom: 1px solid #f1f5f9;
  padding: 1rem 0;
  vertical-align: middle;
}
.table-row-hover:hover td {
  background-color: #f8fafc;
}
.item-avatar {
  width: 40px;
  height: 40px;
  font-size: 1.2rem;
}
.empty-state-icon {
  width: 80px;
  height: 80px;
  background: #f8fafc;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  color: #cbd5e1;
}

/* Table Action Buttons */
.btn-action {
  border: 1px solid transparent;
  background: #f8fafc;
  color: #64748b;
  border-radius: 8px;
  padding: 6px 12px;
  font-weight: 500;
  transition: all 0.2s;
}
.btn-action:hover {
  background: white;
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
}
.btn-action.btn-manage:hover { border-color: #3b82f6; color: #3b82f6; }
.btn-action.btn-edit:hover { border-color: #f59e0b; color: #f59e0b; }
.btn-action.btn-delete:hover { border-color: #ef4444; color: #ef4444; }

.action-icon-btn {
  width: 32px;
  height: 32px;
  padding: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.action-icon-btn:hover {
  background: #fee2e2 !important;
}

/* Modals */
.premium-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  z-index: 1200;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}
.premium-modal-card {
  background: white;
  width: 100%;
  border-radius: 20px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  position: relative;
}
.max-w-500 { max-width: 500px; }
.max-w-900 { max-width: 900px; }
.modal-header-elegant {
  padding: 1.5rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  background: white;
}
.modal-body-elegant {
  padding: 0 2rem 2rem 2rem;
}
.icon-box {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
}
.btn-close-elegant {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  background: #f1f5f9;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  transition: all 0.2s;
}
.btn-close-elegant:hover {
  background: #e2e8f0;
  color: #0f172a;
  transform: rotate(90deg);
}

/* Floating Label Forms */
.modern-form-group {
  position: relative;
}
.modern-input {
  width: 100%;
  padding: 1rem 1rem 0.5rem 1rem;
  font-size: 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #f8fafc;
  transition: all 0.2s;
}
.modern-input:focus {
  outline: none;
  border-color: #3b82f6;
  background: white;
  box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
}
.modern-label {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  font-size: 1rem;
  transition: all 0.2s;
  pointer-events: none;
  font-weight: 500;
}
textarea.modern-input + .modern-label {
  top: 1.2rem;
  transform: none;
}
.modern-input:focus + .modern-label,
.modern-input:not(:placeholder-shown) + .modern-label {
  top: 0.4rem;
  transform: none;
  font-size: 0.75rem;
  color: #3b82f6;
}

/* Small Form Group */
.modern-form-group-sm {
  position: relative;
}
.modern-input-sm {
  width: 100%;
  padding: 1.2rem 0.75rem 0.4rem;
  font-size: 0.9rem;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  transition: all 0.2s;
}
.modern-input-sm:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}
.modern-form-group-sm label {
  position: absolute;
  left: 0.75rem;
  top: 0.3rem;
  font-size: 0.7rem;
  color: #64748b;
  font-weight: 600;
  pointer-events: none;
}
</style>
