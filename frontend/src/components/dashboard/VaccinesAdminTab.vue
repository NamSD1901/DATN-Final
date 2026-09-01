<template>
  <div class="vaccines-admin-tab container-fluid p-0 animate-fade-in-up">
    <!-- Header Area -->
    <div class="d-flex flex-wrap justify-content-between align-items-end mb-4 gap-3 premium-header">
      <div>
        <h4 class="fw-extrabold mb-2 text-dark gradient-text">
          <i class="bi bi-droplet-half text-warning me-2"></i>Quản lý Vắc-xin & Tiêm phòng
        </h4>
        <p class="text-muted small mb-0 fw-medium">
          Theo dõi tồn kho, cảnh báo hết hạn và quản lý các lô hàng vắc-xin một cách chuyên nghiệp.
        </p>
      </div>
      <div class="d-flex gap-2">
        <button class="btn btn-modern-primary px-4 py-2.5 rounded-pill shadow-hover transition-all" @click="openCreateVaccineModal">
          <i class="bi bi-plus-circle me-2"></i> Thêm Vắc-xin Mới
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
                <h3 class="fw-extrabold mb-0 text-dark">{{ expiringList.length }} <span class="fs-6 fw-normal text-muted">lô vắc-xin</span></h3>
              </div>
              <div class="icon-circle bg-warning bg-opacity-25 text-warning-dark">
                <i class="bi bi-calendar-x-fill fs-3"></i>
              </div>
            </div>
            
            <div class="mt-3" v-if="expiringList.length > 0">
              <div class="small fw-semibold mb-2 text-muted">Cần thanh lý/huỷ lô:</div>
              <div class="d-flex flex-wrap gap-2 custom-scrollbar" style="max-height: 90px; overflow-y: auto; padding-right: 5px;">
                <span v-for="item in expiringList" :key="item.id" class="modern-badge bg-warning-light text-warning-dark shadow-sm">
                  {{ item.name }} <span class="badge-qty text-muted ms-1">HSD: {{ formatDate(item.expirationDate || item.expiryDate) }}</span>
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
              placeholder="Tìm kiếm vắc-xin theo tên..."
            />
          </div>
        </div>
        <div class="col-md-7 text-md-end text-muted small fw-medium">
          Đang hiển thị <strong class="text-primary fs-6">{{ filteredVaccines.length }}</strong> loại vắc-xin
        </div>
      </div>

      <!-- Vaccines Table -->
      <div class="table-responsive rounded-3 border-0 overflow-visible">
        <table class="table modern-table align-middle mb-0">
          <thead>
            <tr>
              <th class="ps-4">Tên Vắc-xin</th>
              <th width="15%">Nhà sản xuất</th>
              <th width="12%">Loài chỉ định</th>
              <th width="12%">Tổng Tồn kho</th>
              <th width="12%">Số lượng lô</th>
              <th width="25%" class="text-end pe-4">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center bg-transparent">
              <td colspan="6" class="py-5 border-0">
                <div class="spinner-border text-primary spinner-border-sm me-2"></div>
                <span class="text-muted fw-medium">Đang tải dữ liệu vắc-xin...</span>
              </td>
            </tr>
            <tr v-else-if="filteredVaccines.length === 0" class="text-center bg-transparent">
              <td colspan="6" class="py-5 border-0 text-muted">
                <div class="empty-state-icon mx-auto mb-3"><i class="bi bi-box2"></i></div>
                <h6 class="fw-bold text-dark">Không tìm thấy vắc-xin</h6>
                <p class="small mb-0">Thử thay đổi từ khóa tìm kiếm hoặc thêm mới vắc-xin.</p>
              </td>
            </tr>
            <tr v-for="vac in filteredVaccines" :key="vac.id" class="table-row-hover transition-all" v-else>
              <td class="ps-4 border-0">
                <div class="d-flex align-items-center gap-3 py-2">
                  <div class="item-avatar bg-warning bg-opacity-10 text-warning-dark fw-bold rounded-circle d-flex align-items-center justify-content-center">
                    {{ vac.name.charAt(0).toUpperCase() }}
                  </div>
                  <div>
                    <h6 class="mb-1 fw-bold text-dark">{{ vac.name }}</h6>
                    <div class="text-muted small text-truncate" style="max-width: 250px;">{{ vac.description || 'Không có mô tả' }}</div>
                  </div>
                </div>
              </td>
              <td class="border-0 text-muted fw-medium">{{ vac.manufacturer || 'Không rõ' }}</td>
              <td class="border-0">
                <span class="badge bg-light text-dark border px-2 py-1">{{ vac.targetSpecies || 'Tất cả' }}</span>
              </td>
              <td class="border-0">
                <span :class="['status-badge fw-bold px-3 py-1.5', vac.stockQuantity <= 10 ? 'danger' : 'success']">
                  <i :class="['bi me-1', vac.stockQuantity <= 10 ? 'bi-exclamation-triangle-fill' : 'bi-check-circle-fill']"></i>
                  {{ vac.stockQuantity }} liều
                </span>
              </td>
              <td class="border-0 text-muted fw-medium">
                <span class="badge bg-light text-dark border px-2 py-1"><i class="bi bi-layers me-1"></i>{{ vac.batches ? vac.batches.length : 0 }} lô</span>
              </td>
              <td class="text-end pe-4 border-0">
                <div class="d-flex justify-content-end gap-2 action-buttons">
                  <button class="btn btn-sm btn-action btn-manage" @click="openBatchesModal(vac)" title="Quản lý lô">
                    <i class="bi bi-box-seam me-1"></i> <span>Quản lý Lô</span>
                  </button>
                  <button class="btn btn-sm btn-action btn-edit" @click="openEditVaccineModal(vac)" title="Sửa thông tin">
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                  <button class="btn btn-sm btn-action btn-delete" @click="handleDeleteVaccine(vac.id)" title="Xoá vắc-xin">
                    <i class="bi bi-trash-fill"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Vaccine Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showVaccineModal" class="premium-modal-overlay" @click.self="showVaccineModal = false">
          <div class="premium-modal-card max-w-500 animate-slide-up">
            <div class="modal-header-elegant">
              <h5 class="fw-extrabold mb-0 text-dark">
                <span class="icon-box bg-warning bg-opacity-10 text-warning-dark me-2"><i class="bi bi-droplet-half"></i></span>
                {{ isEditVaccine ? 'Cập Nhật Vắc-xin' : 'Thêm Vắc-xin Mới' }}
              </h5>
              <button class="btn-close-elegant" @click="showVaccineModal = false"><i class="bi bi-x"></i></button>
            </div>
            <div class="modal-body-elegant">
              <form @submit.prevent="submitVaccineForm">
                <div class="modern-form-group mb-4">
                  <input type="text" v-model="vaccineForm.name" class="modern-input" id="vacName" required placeholder=" " />
                  <label for="vacName" class="modern-label">Tên Vắc-xin <span class="text-danger">*</span></label>
                </div>
                
                <div class="row g-3 mb-4">
                  <div class="col-md-6">
                    <div class="modern-form-group">
                      <input type="text" v-model="vaccineForm.manufacturer" class="modern-input" id="vacManufacturer" placeholder=" " />
                      <label for="vacManufacturer" class="modern-label">Nhà sản xuất</label>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="modern-form-group">
                      <input type="text" v-model="vaccineForm.targetSpecies" class="modern-input" id="vacSpecies" placeholder=" " />
                      <label for="vacSpecies" class="modern-label">Loài chỉ định</label>
                    </div>
                  </div>
                </div>

                <div class="row g-3 mb-4">
                  <div class="col-md-6">
                    <div class="modern-form-group">
                      <input type="number" v-model="vaccineForm.minAgeWeeks" class="modern-input" id="vacMinAge" min="0" placeholder=" " />
                      <label for="vacMinAge" class="modern-label">Tuổi tối thiểu (Tuần)</label>
                    </div>
                  </div>
                  <div class="col-md-6">
                    <div class="modern-form-group">
                      <input type="number" v-model="vaccineForm.intervalDays" class="modern-input" id="vacInterval" min="0" placeholder=" " />
                      <label for="vacInterval" class="modern-label">Khoảng cách tiêm (Ngày)</label>
                    </div>
                  </div>
                </div>

                <div class="modern-form-group mb-4">
                  <textarea v-model="vaccineForm.description" class="modern-input" id="vacDesc" rows="2" placeholder=" "></textarea>
                  <label for="vacDesc" class="modern-label">Mô tả, hướng dẫn tiêm...</label>
                </div>

                <div class="d-flex justify-content-end gap-2 mt-5">
                  <button type="button" class="btn btn-light px-4 py-2 rounded-pill fw-bold" @click="showVaccineModal = false">Hủy</button>
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
                  Quản lý Lô Vắc-xin
                </h5>
                <p class="text-muted small mb-0 ms-5">{{ currentVaccine?.name }} • {{ currentVaccine?.manufacturer || 'Không rõ NSX' }}</p>
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
                      <div class="col-md-2">
                        <div class="modern-form-group-sm">
                          <input type="text" v-model="form.batchNumber" class="modern-input-sm" required placeholder=" " :id="'bNum'+index" />
                          <label :for="'bNum'+index">Mã Số Lô *</label>
                        </div>
                      </div>
                      <div class="col-md-3">
                        <div class="modern-form-group-sm">
                          <input type="date" v-model="form.expirationDate" class="modern-input-sm" required :id="'bExp'+index" />
                          <label :for="'bExp'+index">Hạn Sử Dụng *</label>
                        </div>
                      </div>
                      <div class="col-md-2">
                        <div class="modern-form-group-sm">
                          <input type="number" v-model="form.stockQuantity" class="modern-input-sm" required min="1" :id="'bQty'+index" placeholder=" " />
                          <label :for="'bQty'+index">Số Lượng *</label>
                        </div>
                      </div>
                      <div class="col-md-2">
                        <div class="modern-form-group-sm">
                          <input type="number" v-model="form.importPrice" class="modern-input-sm" required min="0" :id="'bImportPrice'+index" placeholder=" " />
                          <label :for="'bImportPrice'+index">Giá Nhập *</label>
                        </div>
                      </div>
                      <div class="col-md-2">
                        <div class="modern-form-group-sm">
                          <input type="number" v-model="form.sellingPrice" class="modern-input-sm" required min="0" :id="'bSellPrice'+index" placeholder=" " />
                          <label :for="'bSellPrice'+index">Giá Bán *</label>
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
                        <th>Ngày nhập</th>
                        <th>Hạn sử dụng</th>
                        <th>Tồn kho</th>
                        <th>Giá bán</th>
                        <th class="text-end pe-4">Thao tác</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-if="!currentVaccine?.batches || currentVaccine.batches.length === 0">
                        <td colspan="6" class="text-center text-muted py-5">
                          <i class="bi bi-inboxes text-light fs-1 d-block mb-2"></i>
                          Vắc-xin này chưa có lô nào trong kho.
                        </td>
                      </tr>
                      <tr v-for="batch in currentVaccine?.batches" :key="batch.id" v-else>
                        <td class="ps-4 fw-bold text-dark">{{ batch.batchNumber }}</td>
                        <td class="text-muted fw-medium">{{ formatDate(batch.importDate) }}</td>
                        <td>
                          <span :class="['status-badge px-2 py-1', isExpiring(batch.expirationDate) ? 'danger' : 'muted']">
                            <i :class="['bi me-1', isExpiring(batch.expirationDate) ? 'bi-exclamation-circle-fill' : 'bi-calendar-check']"></i>
                            {{ formatDate(batch.expirationDate) }}
                          </span>
                        </td>
                        <td>
                          <span class="badge bg-primary bg-opacity-10 text-primary border border-primary border-opacity-25 px-2 py-1 rounded-pill">
                            {{ batch.stockQuantity }} liều
                          </span>
                        </td>
                        <td class="fw-bold text-primary">{{ formatCurrency(batch.sellingPrice) }}</td>
                        <td class="text-end pe-4">
                          <div class="d-flex justify-content-end gap-2">
                            <button class="btn btn-sm btn-outline-primary rounded-pill px-3 py-1 fw-bold" @click="handleAddMoreStock(batch)" title="Cộng dồn thêm số lượng vào lô này" :disabled="isExpired(batch.expirationDate)">
                              <i class="bi bi-plus-lg"></i> Thêm
                            </button>
                            <button class="btn btn-sm btn-light text-danger rounded-circle action-icon-btn" @click="handleDeleteBatch(batch.id)" title="Xoá Lô">
                              <i class="bi bi-trash3-fill"></i>
                            </button>
                          </div>
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
        <div v-if="showAddMoreModal" class="premium-modal-overlay" @click.self="showAddMoreModal = false">
          <div class="premium-modal-card max-w-400 animate-slide-up">
            <div class="modal-header-elegant">
              <h5 class="fw-extrabold mb-0 text-dark">
                <i class="bi bi-plus-circle text-primary me-2"></i> Cộng Dồn Số Lượng
              </h5>
              <button class="btn-close-elegant" @click="showAddMoreModal = false"><i class="bi bi-x"></i></button>
            </div>
            <div class="modal-body-elegant">
              <div class="alert alert-primary bg-primary bg-opacity-10 border-0 rounded-3 mb-4">
                <div class="d-flex gap-2">
                  <i class="bi bi-info-circle-fill text-primary mt-1"></i>
                  <div>
                    <h6 class="fw-bold text-primary mb-1">Lô {{ batchToAddMore?.batchNumber }}</h6>
                    <p class="small mb-0 text-primary opacity-75">Tồn kho hiện tại: <strong>{{ batchToAddMore?.stockQuantity }} liều</strong></p>
                  </div>
                </div>
              </div>
              
              <div class="modern-form-group mb-4">
                <input type="number" v-model="addMoreQuantity" class="modern-input text-center fs-4 fw-bold text-primary" required min="1" id="addMoreQty" placeholder=" " />
                <label for="addMoreQty" class="modern-label">Số lượng nhập thêm *</label>
              </div>

              <div class="d-flex justify-content-end gap-2 mt-4">
                <button type="button" class="btn btn-light px-4 py-2 rounded-pill fw-bold" @click="showAddMoreModal = false">Hủy</button>
                <button type="button" class="btn btn-modern-primary px-4 py-2 rounded-pill fw-bold shadow-sm" @click="confirmAddMoreStock" :disabled="isSubmittingAddMore">
                  <span v-if="isSubmittingAddMore" class="spinner-border spinner-border-sm me-2"></span>
                  <i v-else class="bi bi-check2-circle me-1"></i> Xác nhận
                </button>
              </div>
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
const vaccinesList = ref<any[]>([]);
const searchKeyword = ref('');

const lowStockList = ref<any[]>([]);
const expiringList = ref<any[]>([]);

const showVaccineModal = ref(false);
const isEditVaccine = ref(false);
const currentVaccineId = ref<number | null>(null);

const showBatchesModal = ref(false);
const currentVaccine = ref<any>(null);

const showAddMoreModal = ref(false);
const isSubmittingAddMore = ref(false);
const batchToAddMore = ref<any>(null);
const addMoreQuantity = ref(10);

const vaccineForm = ref({
  name: '',
  manufacturer: '',
  description: '',
  targetSpecies: '',
  minAgeWeeks: null as number | null,
  intervalDays: null as number | null
});

const batchForms = ref([{
  batchNumber: '',
  expirationDate: '',
  importDate: new Date().toISOString().split('T')[0],
  stockQuantity: 10,
  importPrice: 0,
  sellingPrice: 0
}]);

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

const loadWarnings = async () => {
  loadingWarnings.value = true;
  try {
    // Attempt to fetch warnings if these endpoints exist, or fallback to local calculation
    // Since we don't have dedicated vaccine warning endpoints, we calculate locally
    const allBatches = vaccinesList.value.flatMap(v => v.batches || []).map(b => ({...b, vaccineName: v.name}));
    
    // Low stock: vacs with <= 10 total
    lowStockList.value = vaccinesList.value.filter(v => v.stockQuantity <= 10);
    
    // Expiring: batches expiring in <= 30 days
    const today = new Date();
    const thirtyDaysFromNow = new Date();
    thirtyDaysFromNow.setDate(today.getDate() + 30);
    
    expiringList.value = allBatches
      .filter(b => b.expirationDate && new Date(b.expirationDate) <= thirtyDaysFromNow && b.stockQuantity > 0)
      .map(b => ({
        id: b.id,
        name: b.vaccineName + ' (Lô ' + b.batchNumber + ')',
        expirationDate: b.expirationDate
      }));
      
  } catch (err) {
    console.error('Lỗi tải cảnh báo vắc-xin:', err);
  } finally {
    loadingWarnings.value = false;
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
    await loadWarnings();
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
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá vắc-xin.');
  }
};

const openBatchesModal = (vac: any) => {
  currentVaccine.value = vac;
  batchForms.value = [{
    batchNumber: '',
    expirationDate: '',
    importDate: new Date().toISOString().split('T')[0],
    stockQuantity: 10,
    importPrice: 0,
    sellingPrice: 0
  }];
  showBatchesModal.value = true;
};

const addBatchRow = () => {
  batchForms.value.push({
    batchNumber: '',
    expirationDate: '',
    importDate: new Date().toISOString().split('T')[0],
    stockQuantity: 10,
    importPrice: 0,
    sellingPrice: 0
  });
};

const removeBatchRow = (index: number) => {
  batchForms.value.splice(index, 1);
};

const submitBatchForm = async () => {
  if (!currentVaccine.value) return;
  try {
    const promises = batchForms.value.map(form => 
      api.post(`/admin/vaccines/${currentVaccine.value.id}/batches`, form)
    );
    await Promise.all(promises);
    alert('Thêm lô vắc-xin thành công!');
    batchForms.value = [{
      batchNumber: '',
      expirationDate: '',
      importDate: new Date().toISOString().split('T')[0],
      stockQuantity: 10,
      importPrice: 0,
      sellingPrice: 0
    }];
    await loadVaccines();
    await loadWarnings();
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
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xoá lô vắc-xin.');
  }
};

const handleAddMoreStock = (batch: any) => {
  batchToAddMore.value = batch;
  addMoreQuantity.value = 10;
  showAddMoreModal.value = true;
};

const confirmAddMoreStock = async () => {
  if (!batchToAddMore.value || !currentVaccine.value) return;
  if (addMoreQuantity.value <= 0) {
    alert('Số lượng nhập thêm không hợp lệ.');
    return;
  }
  
  isSubmittingAddMore.value = true;
  try {
    const payload = {
      batchNumber: batchToAddMore.value.batchNumber,
      expirationDate: batchToAddMore.value.expirationDate,
      importDate: new Date().toISOString(),
      stockQuantity: addMoreQuantity.value,
      importPrice: batchToAddMore.value.importPrice || 0,
      sellingPrice: batchToAddMore.value.sellingPrice || 0
    };
    await api.post(`/admin/vaccines/${currentVaccine.value.id}/batches`, payload);
    showAddMoreModal.value = false;
    alert('Nhập thêm thành công!');
    await loadVaccines();
    await loadWarnings();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Có lỗi xảy ra khi nhập thêm.');
  } finally {
    isSubmittingAddMore.value = false;
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
  if (!dateStr) return 'N/A';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
};

const formatCurrency = (val: number | null) => {
  if (!val) return '0 đ';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

onMounted(async () => {
  await loadVaccines();
  await loadWarnings();
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
