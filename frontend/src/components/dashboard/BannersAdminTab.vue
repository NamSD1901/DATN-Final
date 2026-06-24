<template>
  <div class="banners-admin-container p-4">
    <!-- Header Tools -->
    <div class="glass-card p-4 mb-4 d-flex flex-wrap justify-content-between align-items-center gap-3">
      <div>
        <h4 class="fw-bold text-dark mb-1"><i class="bi bi-image text-warning me-2"></i>Quản Trị Banners Quảng Cáo</h4>
        <p class="text-muted small mb-0">Quản lý các banner trình chiếu trên trang chủ và khuyến mại</p>
      </div>
      
      <button class="btn btn-warning rounded-pill px-4 py-2 fw-bold text-dark shadow-sm d-flex align-items-center gap-1" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill"></i> Thêm Banner
      </button>
    </div>

    <!-- Error/Success Alerts -->
    <div v-if="successMsg" class="alert alert-success rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-check-circle-fill me-2"></i>{{ successMsg }}
    </div>
    <div v-if="errorMsg" class="alert alert-danger rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Banners Table List -->
    <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
      <div class="table-responsive">
        <table class="table align-middle border-bottom mb-0">
          <thead class="table-light">
            <tr>
              <th class="py-3 ps-4 border-0 text-muted small" width="160">Ảnh Banner</th>
              <th class="py-3 border-0 text-muted small">Tiêu đề / Đường dẫn</th>
              <th class="py-3 border-0 text-muted small">Thời gian hiệu lực</th>
              <th class="py-3 border-0 text-muted small text-center" width="100">Thứ tự</th>
              <th class="py-3 border-0 text-muted small" width="130">Trạng thái</th>
              <th class="py-3 border-0 text-muted small text-end pe-4" width="150">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="banner in banners" :key="banner.id">
              <td class="ps-4 py-3 border-0">
                <img :src="banner.imageUrl || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" 
                     alt="Banner image" class="rounded-3 object-fit-cover shadow-sm border" width="120" height="60" />
              </td>
              <td class="py-3 border-0">
                <div class="fw-bold text-dark mb-1">{{ banner.title }}</div>
                <a v-if="banner.linkUrl" :href="banner.linkUrl" target="_blank" class="text-primary small text-decoration-none">
                  <i class="bi bi-link-45deg"></i> {{ truncateUrl(banner.linkUrl) }}
                </a>
              </td>
              <td class="py-3 border-0 text-muted small">
                <div v-if="banner.startDate || banner.endDate">
                  <div><i class="bi bi-calendar-event me-1"></i>Từ: {{ formatDate(banner.startDate) || 'Không giới hạn' }}</div>
                  <div><i class="bi bi-calendar-x me-1"></i>Đến: {{ formatDate(banner.endDate) || 'Không giới hạn' }}</div>
                </div>
                <span v-else class="text-success fw-bold">Luôn hiển thị</span>
              </td>
              <td class="py-3 border-0 text-center fw-bold text-dark fs-5">
                {{ banner.order }}
              </td>
              <td class="py-3 border-0">
                <span class="badge px-3 py-2 rounded-pill fw-bold" 
                      :class="banner.isActive ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                  {{ banner.isActive ? 'Đang chạy' : 'Tạm dừng' }}
                </span>
              </td>
              <td class="py-3 border-0 text-end pe-4">
                <button class="btn btn-light btn-sm rounded-circle me-1 border-0 shadow-sm hover-text-warning" @click="openEditModal(banner)">
                  <i class="bi bi-pencil-fill"></i>
                </button>
                <button class="btn btn-light btn-sm rounded-circle border-0 shadow-sm hover-text-danger" @click="confirmDelete(banner)">
                  <i class="bi bi-trash-fill text-danger"></i>
                </button>
              </td>
            </tr>
            <tr v-if="postStore.loading && banners.length === 0">
              <td colspan="6" class="text-center py-5 text-muted">
                <div class="spinner-border spinner-border-sm text-warning me-2" role="status"></div>
                Đang tải Banners...
              </td>
            </tr>
            <tr v-if="!postStore.loading && banners.length === 0">
              <td colspan="6" class="text-center py-5 text-muted">
                <i class="bi bi-images fs-1 d-block mb-2 text-black-50 opacity-25"></i>
                Chưa có Banner nào được cấu hình.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="closeModal">
      <div class="zalo-modal-card" style="max-width: 650px;">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-image me-2"></i>{{ isEditing ? 'Chỉnh Sửa Banner' : 'Thêm Banner Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="closeModal"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="saveBanner">
            <div class="row g-3">
              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Tiêu đề Banner <span class="text-danger">*</span></label>
                <input type="text" v-model="form.title" class="form-control rounded-3" placeholder="Chiến dịch khuyến mại..." required />
              </div>

              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Link Ảnh URL <span class="text-danger">*</span></label>
                <input type="url" v-model="form.imageUrl" class="form-control rounded-3" placeholder="https://..." required />
              </div>

              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Đường dẫn khi click (Tùy chọn)</label>
                <input type="url" v-model="form.linkUrl" class="form-control rounded-3" placeholder="https://..." />
              </div>

              <div class="col-md-4">
                <label class="form-label small text-muted fw-bold">Thứ tự hiển thị</label>
                <input type="number" v-model="form.order" class="form-control rounded-3" placeholder="0" min="0" />
              </div>
              
              <div class="col-md-4">
                <label class="form-label small text-muted fw-bold">Ngày bắt đầu</label>
                <input type="datetime-local" v-model="form.startDate" class="form-control rounded-3" />
              </div>

              <div class="col-md-4">
                <label class="form-label small text-muted fw-bold">Ngày kết thúc</label>
                <input type="datetime-local" v-model="form.endDate" class="form-control rounded-3" />
              </div>

              <div class="col-12 d-flex align-items-center mt-3">
                <div class="form-check form-switch fs-5 mb-0">
                  <input class="form-check-input" type="checkbox" id="flexSwitchBanner" v-model="form.isActive">
                </div>
                <label class="form-check-label ms-2 text-dark fw-bold" for="flexSwitchBanner">Kích hoạt hiển thị</label>
              </div>
            </div>

            <div class="d-flex justify-content-end gap-2 mt-4 pt-3 border-top">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 fw-bold" @click="closeModal">Hủy bỏ</button>
              <button type="submit" class="btn btn-warning text-dark rounded-pill px-4 fw-bold shadow-sm" :disabled="saving">
                {{ saving ? 'Đang lưu...' : 'Lưu Banner' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { usePostStore } from '../../stores/post.store';

const postStore = usePostStore();
const banners = computed(() => postStore.banners);

const saving = ref(false);
const successMsg = ref('');
const errorMsg = ref('');

const showModal = ref(false);
const isEditing = ref(false);
const currentId = ref<number | null>(null);

const form = ref({
  title: '',
  imageUrl: '',
  linkUrl: '',
  order: 0,
  startDate: '',
  endDate: '',
  isActive: true
});

const formatDate = (dateStr: string | null | undefined) => {
  if (!dateStr) return null;
  return new Date(dateStr).toLocaleString('vi-VN', { dateStyle: 'short', timeStyle: 'short' });
};

const truncateUrl = (url: string) => {
  if (url.length > 30) return url.substring(0, 30) + '...';
  return url;
};

const triggerAlert = (type: 'success' | 'error', msg: string) => {
  if (type === 'success') {
    successMsg.value = msg;
    setTimeout(() => successMsg.value = '', 4000);
  } else {
    errorMsg.value = msg;
    setTimeout(() => errorMsg.value = '', 4000);
  }
};

const openCreateModal = () => {
  isEditing.value = false;
  currentId.value = null;
  form.value = {
    title: '',
    imageUrl: '',
    linkUrl: '',
    order: 0,
    startDate: '',
    endDate: '',
    isActive: true
  };
  showModal.value = true;
};

const openEditModal = (banner: any) => {
  isEditing.value = true;
  currentId.value = banner.id;
  form.value = {
    title: banner.title,
    imageUrl: banner.imageUrl,
    linkUrl: banner.linkUrl || '',
    order: banner.order,
    startDate: banner.startDate ? banner.startDate.slice(0, 16) : '',
    endDate: banner.endDate ? banner.endDate.slice(0, 16) : '',
    isActive: banner.isActive
  };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const saveBanner = async () => {
  saving.value = true;
  try {
    const payload = {
      ...form.value,
      startDate: form.value.startDate ? new Date(form.value.startDate).toISOString() : null,
      endDate: form.value.endDate ? new Date(form.value.endDate).toISOString() : null,
    };

    if (isEditing.value && currentId.value) {
      await postStore.updateBanner(currentId.value, payload);
      triggerAlert('success', 'Cập nhật Banner thành công.');
    } else {
      await postStore.createBanner(payload);
      triggerAlert('success', 'Thêm mới Banner thành công.');
    }
    closeModal();
  } catch (err: any) {
    triggerAlert('error', err.message || 'Lỗi khi lưu Banner.');
  } finally {
    saving.value = false;
  }
};

const confirmDelete = async (banner: any) => {
  if (!confirm(`Bạn có chắc chắn muốn xóa Banner "${banner.title}"?`)) return;
  try {
    await postStore.deleteBanner(banner.id);
    triggerAlert('success', 'Xóa Banner thành công.');
  } catch (err: any) {
    triggerAlert('error', err.message || 'Không thể xóa Banner.');
  }
};

onMounted(() => {
  postStore.fetchBanners(false);
});
</script>

<style scoped>
.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 16px;
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
  border-radius: 16px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  overflow: hidden;
}
.zalo-modal-header {
  padding: 1.2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.zalo-modal-body {
  padding: 1.5rem;
}
</style>
