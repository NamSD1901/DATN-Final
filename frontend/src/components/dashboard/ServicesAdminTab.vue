<template>
  <div class="services-admin-tab container-fluid p-0">
    <div class="card border-0 shadow-sm rounded-4 p-4 bg-white">
      <div class="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
        <div>
          <h4 class="fw-bold mb-1 text-dark"><i class="bi bi-box-seam-fill text-warning me-2"></i>Quản lý Dịch vụ & Giá cả</h4>
          <p class="text-muted small mb-0">Cấu hình danh mục dịch vụ y tế, giá cả khám bệnh và thời gian thực hiện</p>
        </div>
        <button class="btn btn-premium px-4 py-2.5 rounded-pill shadow-sm" @click="openCreateModal">
          <i class="bi bi-plus-circle-fill me-2"></i> Thêm Dịch Vụ Mới
        </button>
      </div>

      <!-- Filters & Table -->
      <div class="row g-2 mb-3 align-items-center">
        <div class="col-md-6">
          <div class="input-group">
            <span class="input-group-text bg-white border-end-0"><i class="bi bi-search text-muted"></i></span>
            <input 
              type="text" 
              v-model="searchKeyword" 
              class="form-control border-start-0 input-premium" 
              placeholder="Tìm kiếm dịch vụ..."
            />
          </div>
        </div>
        <div class="col-md-6 text-md-end text-muted small">
          Tổng số: <strong class="text-dark">{{ filteredServices.length }}</strong> dịch vụ
        </div>
      </div>

      <!-- Services Table -->
      <div class="table-responsive rounded-4 border overflow-hidden mt-3">
        <table class="table table-hover align-middle mb-0">
          <thead class="bg-light-gold">
            <tr>
              <th class="ps-4">Tên dịch vụ</th>
              <th>Đơn giá</th>
              <th>Thời gian khám</th>
              <th>Mô tả</th>
              <th class="text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="text-center">
              <td colspan="5" class="py-5">
                <div class="spinner-border text-warning spinner-border-sm me-2"></div>
                <span class="text-muted">Đang tải danh sách dịch vụ...</span>
              </td>
            </tr>
            <tr v-else-if="filteredServices.length === 0" class="text-center">
              <td colspan="5" class="py-5 text-muted">
                <i class="bi bi-folder-x fs-2 mb-2 d-block"></i>
                Không tìm thấy dịch vụ nào.
              </td>
            </tr>
            <tr v-for="service in filteredServices" :key="service.id" v-else>
              <td class="ps-4 fw-bold text-dark">{{ service.name }}</td>
              <td>
                <span class="badge bg-success bg-opacity-10 text-success px-3 py-1.5 rounded-pill fw-bold">
                  {{ formatCurrency(service.price) }}
                </span>
              </td>
              <td>{{ service.durationMinutes }} phút</td>
              <td class="text-muted small text-truncate" style="max-width: 300px;">{{ service.description || '—' }}</td>
              <td class="text-center">
                <button class="btn btn-sm btn-outline-warning rounded-pill px-3 me-2" @click="openEditModal(service)">
                  <i class="bi bi-pencil-fill me-1"></i>Sửa
                </button>
                <button class="btn btn-sm btn-outline-danger rounded-pill px-3" @click="handleDelete(service.id)">
                  <i class="bi bi-trash-fill me-1"></i>Xóa
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
            <i class="bi bi-box-seam-fill me-2"></i> {{ isEdit ? 'Cập Nhật Dịch Vụ' : 'Thêm Mới Dịch Vụ' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="showModal = false"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="submitForm">
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Tên dịch vụ *</label>
              <input type="text" v-model="form.name" class="form-control input-premium" required placeholder="Tên dịch vụ y tế..." />
            </div>
            <div class="row g-2 mb-3">
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Đơn giá (VND) *</label>
                <input type="number" v-model="form.price" class="form-control" required placeholder="Nhập giá tiền..." />
              </div>
              <div class="col-6">
                <label class="form-label text-muted small fw-bold">Thời lượng (phút) *</label>
                <input type="number" v-model="form.durationMinutes" class="form-control" required placeholder="Phút..." />
              </div>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Nhóm danh mục *</label>
              <select v-model="form.categoryId" class="form-select border-warning" required>
                <option :value="1">Khám chữa bệnh</option>
                <option :value="2">Tiêm vắc xin</option>
                <option :value="3">Spa & Làm đẹp</option>
              </select>
            </div>
            <div class="mb-3">
              <label class="form-label text-muted small fw-bold">Mô tả chi tiết</label>
              <textarea v-model="form.description" class="form-control" rows="3" placeholder="Thông tin dịch vụ..."></textarea>
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
const servicesList = ref<any[]>([]);
const searchKeyword = ref('');

const showModal = ref(false);
const isEdit = ref(false);
const currentServiceId = ref<number | null>(null);

const form = ref({
  name: '',
  price: 0,
  categoryId: 1,
  durationMinutes: 30,
  description: ''
});

const filteredServices = computed(() => {
  const keyword = searchKeyword.value.toLowerCase().trim();
  return servicesList.value.filter(s => 
    s.isActive && (s.name.toLowerCase().includes(keyword) || (s.description && s.description.toLowerCase().includes(keyword)))
  );
});

const loadServices = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/services');
    servicesList.value = res.data || [];
  } catch (err) {
    console.error('Lỗi tải danh sách dịch vụ:', err);
  } finally {
    loading.value = false;
  }
};

const openCreateModal = () => {
  isEdit.value = false;
  currentServiceId.value = null;
  form.value = {
    name: '',
    price: 0,
    categoryId: 1,
    durationMinutes: 30,
    description: ''
  };
  showModal.value = true;
};

const openEditModal = (service: any) => {
  isEdit.value = true;
  currentServiceId.value = service.id;
  form.value = {
    name: service.name,
    price: service.price,
    categoryId: service.categoryId || 1,
    durationMinutes: service.durationMinutes || 30,
    description: service.description || ''
  };
  showModal.value = true;
};

const submitForm = async () => {
  try {
    if (isEdit.value && currentServiceId.value) {
      await api.put(`/admin/services/${currentServiceId.value}`, form.value);
      alert('Cập nhật dịch vụ thành công!');
    } else {
      await api.post('/admin/services', form.value);
      alert('Tạo dịch vụ thành công!');
    }
    showModal.value = false;
    await loadServices();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi lưu thông tin dịch vụ.');
  }
};

const handleDelete = async (id: number) => {
  if (!confirm('Bạn có chắc muốn ngừng kích hoạt dịch vụ này?')) return;
  try {
    await api.delete(`/admin/services/${id}`);
    alert('Ngừng kích hoạt thành công!');
    await loadServices();
  } catch (err: any) {
    alert(err.response?.data?.message || 'Lỗi khi xóa dịch vụ.');
  }
};

const formatCurrency = (val: number) => {
  if (!val) return '0 đ';
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val);
};

onMounted(() => {
  loadServices();
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
