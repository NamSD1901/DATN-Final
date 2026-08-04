<template>
  <div class="categories-admin-container p-4">
    <!-- Header Tools -->
    <div v-if="!hideHeader" class="glass-card p-4 mb-4 d-flex flex-wrap justify-content-between align-items-center gap-3">
      <div>
        <h4 class="fw-bold text-dark mb-1"><i class="bi bi-tags-fill text-warning me-2"></i>Quản Trị Danh Mục Bài Viết</h4>
        <p class="text-muted small mb-0">Tạo và phân loại các chủ đề tin tức, blog kiến thức</p>
      </div>
      
      <button class="btn btn-warning rounded-pill px-4 py-2 fw-bold text-dark shadow-sm d-flex align-items-center gap-1" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill"></i> Thêm danh mục
      </button>
    </div>
    
    <div v-else class="text-end mb-3">
      <button class="btn btn-warning rounded-pill px-4 py-2 fw-bold text-dark shadow-sm d-flex align-items-center gap-1 d-inline-flex" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill"></i> Thêm danh mục
      </button>
    </div>

    <!-- Error/Success Alerts -->
    <div v-if="successMsg" class="alert alert-success rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-check-circle-fill me-2"></i>{{ successMsg }}
    </div>
    <div v-if="errorMsg" class="alert alert-danger rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Categories Table List -->
    <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
      <div class="table-responsive">
        <table class="table align-middle border-bottom mb-0">
          <thead class="table-light">
            <tr>
              <th class="py-3 ps-4 border-0 text-muted small">Tên Danh Mục</th>
              <th class="py-3 border-0 text-muted small">Đường dẫn (Slug)</th>
              <th class="py-3 border-0 text-muted small">Mô tả</th>
              <th class="py-3 border-0 text-muted small" width="130">Trạng thái</th>
              <th class="py-3 border-0 text-muted small text-end pe-4" width="150">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="category in categories" :key="category.id">
              <!-- Cấp 1 -->
              <tr>
                <td class="ps-4 py-3 border-0 fw-bold text-dark">
                  <i class="bi bi-folder-fill text-warning me-2"></i> {{ category.name }}
                </td>
                <td class="py-3 border-0 text-muted font-monospace small">{{ category.slug }}</td>
                <td class="py-3 border-0 text-muted small">{{ category.description || '-' }}</td>
                <td class="py-3 border-0">
                  <span class="badge px-3 py-2 rounded-pill fw-bold" 
                        :class="category.isActive ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                    {{ category.isActive ? 'Hiển thị' : 'Đã ẩn' }}
                  </span>
                </td>
                <td class="py-3 border-0 text-end pe-4">
                  <button class="btn btn-light btn-sm rounded-circle me-1 border-0 shadow-sm hover-text-warning" @click="openEditModal(category)">
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                  <button class="btn btn-light btn-sm rounded-circle border-0 shadow-sm hover-text-danger" @click="confirmDelete(category)">
                    <i class="bi bi-trash-fill text-danger"></i>
                  </button>
                </td>
              </tr>
              <!-- Cấp 2 -->
              <tr v-for="child in category.children" :key="child.id" class="bg-light">
                <td class="ps-5 py-3 border-0 text-dark">
                  <i class="bi bi-arrow-return-right text-muted me-2"></i> {{ child.name }}
                </td>
                <td class="py-3 border-0 text-muted font-monospace small">{{ child.slug }}</td>
                <td class="py-3 border-0 text-muted small">{{ child.description || '-' }}</td>
                <td class="py-3 border-0">
                  <span class="badge px-3 py-2 rounded-pill fw-bold" 
                        :class="child.isActive ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                    {{ child.isActive ? 'Hiển thị' : 'Đã ẩn' }}
                  </span>
                </td>
                <td class="py-3 border-0 text-end pe-4">
                  <button class="btn btn-light btn-sm rounded-circle me-1 border-0 shadow-sm hover-text-warning" @click="openEditModal(child)">
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                  <button class="btn btn-light btn-sm rounded-circle border-0 shadow-sm hover-text-danger" @click="confirmDelete(child)">
                    <i class="bi bi-trash-fill text-danger"></i>
                  </button>
                </td>
              </tr>
            </template>
            <tr v-if="postStore.loading && categories.length === 0">
              <td colspan="5" class="text-center py-5 text-muted">
                <div class="spinner-border spinner-border-sm text-warning me-2" role="status"></div>
                Đang tải danh mục...
              </td>
            </tr>
            <tr v-if="!postStore.loading && categories.length === 0">
              <td colspan="5" class="text-center py-5 text-muted">
                <i class="bi bi-folder-x fs-1 d-block mb-2 text-black-50 opacity-25"></i>
                Chưa có danh mục nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="closeModal">
      <div class="zalo-modal-card" style="max-width: 500px;">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-tags me-2"></i>{{ isEditing ? 'Chỉnh Sửa Danh Mục' : 'Thêm Danh Mục Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="closeModal"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="saveCategory">
            <div class="row g-3">
              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Tên Danh Mục <span class="text-danger">*</span></label>
                <input type="text" v-model="form.name" class="form-control rounded-3" placeholder="Nhập tên..." required />
              </div>

              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Đường dẫn tĩnh (Slug)</label>
                <input type="text" v-model="form.slug" class="form-control rounded-3" placeholder="Để trống sẽ tự tạo..." />
              </div>

              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Danh mục cha</label>
                <select v-model="form.parentId" class="form-select rounded-3">
                  <option :value="null">-- Không có (Danh mục gốc) --</option>
                  <option v-for="cat in categories.filter(c => c.id !== currentId)" :key="cat.id" :value="cat.id">
                    {{ cat.name }}
                  </option>
                </select>
              </div>

              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Mô tả</label>
                <textarea v-model="form.description" class="form-control rounded-3" rows="3" placeholder="Mô tả danh mục..."></textarea>
              </div>

              <div class="col-12 d-flex align-items-center mt-3">
                <div class="form-check form-switch fs-5 mb-0">
                  <input class="form-check-input" type="checkbox" id="flexSwitchCheckDefault" v-model="form.isActive">
                </div>
                <label class="form-check-label ms-2 text-dark fw-bold" for="flexSwitchCheckDefault">Hiển thị công khai</label>
              </div>
            </div>

            <div class="d-flex justify-content-end gap-2 mt-4 pt-3 border-top">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 fw-bold" @click="closeModal">Hủy bỏ</button>
              <button type="submit" class="btn btn-warning text-dark rounded-pill px-4 fw-bold shadow-sm" :disabled="saving">
                {{ saving ? 'Đang lưu...' : 'Lưu danh mục' }}
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

const props = defineProps({
  hideHeader: {
    type: Boolean,
    default: false
  }
});

const postStore = usePostStore();
const categories = computed(() => postStore.categories);

const saving = ref(false);
const successMsg = ref('');
const errorMsg = ref('');

const showModal = ref(false);
const isEditing = ref(false);
const currentId = ref<number | null>(null);

const form = ref({
  name: '',
  slug: '',
  description: '',
  parentId: null as number | null,
  isActive: true
});

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
    name: '',
    slug: '',
    description: '',
    parentId: null,
    isActive: true
  };
  showModal.value = true;
};

const openEditModal = (category: any) => {
  isEditing.value = true;
  currentId.value = category.id;
  form.value = {
    name: category.name,
    slug: category.slug || '',
    description: category.description || '',
    parentId: category.parentId || null,
    isActive: category.isActive
  };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const saveCategory = async () => {
  saving.value = true;
  try {
    if (isEditing.value && currentId.value) {
      await postStore.updateCategory(currentId.value, form.value);
      triggerAlert('success', 'Cập nhật danh mục thành công.');
    } else {
      await postStore.createCategory(form.value);
      triggerAlert('success', 'Thêm mới danh mục thành công.');
    }
    closeModal();
  } catch (err: any) {
    triggerAlert('error', err.message || 'Lỗi khi lưu danh mục.');
  } finally {
    saving.value = false;
  }
};

const confirmDelete = async (category: any) => {
  if (!confirm(`Bạn có chắc chắn muốn xóa danh mục "${category.name}"?`)) return;
  try {
    await postStore.deleteCategory(category.id);
    triggerAlert('success', 'Xóa danh mục thành công.');
  } catch (err: any) {
    triggerAlert('error', err.message || 'Không thể xóa danh mục (có thể do đang chứa bài viết).');
  }
};

onMounted(() => {
  postStore.fetchCategories();
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
