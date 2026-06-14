<template>
  <div class="blog-admin-container p-4">
    <!-- Header Tools -->
    <div class="glass-card p-4 mb-4 d-flex flex-wrap justify-content-between align-items-center gap-3">
      <div>
        <h4 class="fw-bold text-dark mb-1"><i class="bi bi-journal-richtext text-warning me-2"></i>Quản Trị Bài Viết & Tin Tức</h4>
        <p class="text-muted small mb-0">Viết cẩm nang chăm sóc thú cưng và các thông báo khuyến mại của phòng khám</p>
      </div>
      
      <button class="btn btn-warning rounded-pill px-4 py-2 fw-bold text-dark shadow-sm d-flex align-items-center gap-1" @click="openCreateModal">
        <i class="bi bi-plus-circle-fill"></i> Viết bài mới
      </button>
    </div>

    <!-- Error/Success Alerts -->
    <div v-if="successMsg" class="alert alert-success rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-check-circle-fill me-2"></i>{{ successMsg }}
    </div>
    <div v-if="errorMsg" class="alert alert-danger rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ errorMsg }}
    </div>

    <!-- Blog Posts Table List -->
    <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
      <div class="table-responsive">
        <table class="table align-middle border-bottom mb-0">
          <thead class="table-light">
            <tr>
              <th class="py-3 ps-4 border-0 text-muted small" width="100">Ảnh bìa</th>
              <th class="py-3 border-0 text-muted small">Tiêu đề bài viết</th>
              <th class="py-3 border-0 text-muted small">Tác giả</th>
              <th class="py-3 border-0 text-muted small" width="130">Ngày tạo</th>
              <th class="py-3 border-0 text-muted small" width="130">Trạng thái</th>
              <th class="py-3 border-0 text-muted small text-end pe-4" width="150">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="post in posts" :key="post.id">
              <td class="ps-4 py-3 border-0">
                <img :src="post.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" 
                     alt="Post thumbnail" class="rounded-3 object-fit-cover" width="65" height="45" />
              </td>
              <td class="py-3 border-0">
                <div class="fw-bold text-dark mb-1">{{ post.title }}</div>
                <span class="text-muted small font-monospace">/posts/{{ post.slug }}</span>
              </td>
              <td class="py-3 border-0 text-muted small">{{ post.authorName }}</td>
              <td class="py-3 border-0 text-muted small">{{ formatDate(post.createdAt) }}</td>
              <td class="py-3 border-0">
                <span class="badge px-3 py-2 rounded-pill fw-bold" 
                      :class="post.status === 'published' ? 'bg-success-subtle text-success' : 'bg-secondary-subtle text-secondary'">
                  {{ post.status === 'published' ? 'Đã đăng' : 'Bản nháp' }}
                </span>
              </td>
              <td class="py-3 border-0 text-end pe-4">
                <button class="btn btn-light btn-sm rounded-circle me-1 border-0 shadow-sm hover-text-warning" @click="openEditModal(post)">
                  <i class="bi bi-pencil-fill"></i>
                </button>
                <button class="btn btn-light btn-sm rounded-circle border-0 shadow-sm hover-text-danger" @click="confirmDelete(post)">
                  <i class="bi bi-trash-fill text-danger"></i>
                </button>
              </td>
            </tr>
            <tr v-if="loading && posts.length === 0">
              <td colspan="6" class="text-center py-5 text-muted">
                <div class="spinner-border spinner-border-sm text-warning me-2" role="status"></div>
                Đang tải danh sách bài viết...
              </td>
            </tr>
            <tr v-if="!loading && posts.length === 0">
              <td colspan="6" class="text-center py-5 text-muted">
                <i class="bi bi-journal-x fs-1 d-block mb-2 text-black-50 opacity-25"></i>
                Chưa có bài viết nào được đăng tải.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create/Edit Post Modal Dialog -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="closeModal">
      <div class="zalo-modal-card" style="max-width: 750px;">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-journal-plus me-2"></i>{{ isEditing ? 'Chỉnh Sửa Bài Viết' : 'Viết Bài Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="closeModal"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start">
          <form @submit.prevent="savePost">
            <div class="row g-3">
              <!-- Title -->
              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Tiêu đề bài viết <span class="text-danger">*</span></label>
                <input type="text" v-model="form.title" class="form-control rounded-3" placeholder="Nhập tiêu đề hấp dẫn..." required />
              </div>

              <!-- Slug -->
              <div class="col-md-6">
                <label class="form-label small text-muted fw-bold">URL Slug (Tùy chọn)</label>
                <input type="text" v-model="form.slug" class="form-control rounded-3" placeholder="tieu-de-viet-tat-viet-khong-dau" />
              </div>

              <!-- Status -->
              <div class="col-md-6">
                <label class="form-label small text-muted fw-bold">Trạng thái phát hành</label>
                <select v-model="form.status" class="form-select rounded-3">
                  <option value="draft">Bản nháp (Draft)</option>
                  <option value="published">Đăng công khai (Published)</option>
                </select>
              </div>

              <!-- Thumbnail URL -->
              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Ảnh bìa (URL Ảnh)</label>
                <input type="text" v-model="form.thumbnail" class="form-control rounded-3" placeholder="https://images.unsplash.com/photo-..." />
              </div>

              <!-- Content -->
              <div class="col-12">
                <label class="form-label small text-muted fw-bold">Nội dung bài viết <span class="text-danger">*</span></label>
                <textarea v-model="form.content" class="form-control rounded-3" rows="10" placeholder="Nội dung bài viết chuẩn markdown hoặc text thô..." required></textarea>
              </div>
            </div>

            <div class="d-flex justify-content-end gap-2 mt-4 pt-3 border-top">
              <button type="button" class="btn btn-outline-secondary rounded-pill px-4 fw-bold" @click="closeModal">Hủy bỏ</button>
              <button type="submit" class="btn btn-warning text-dark rounded-pill px-4 fw-bold shadow-sm" :disabled="saving">
                {{ saving ? 'Đang lưu...' : 'Lưu bài viết' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../services/api';

const posts = ref<any[]>([]);
const loading = ref(false);
const saving = ref(false);

const successMsg = ref('');
const errorMsg = ref('');

const showModal = ref(false);
const isEditing = ref(false);
const currentId = ref<number | null>(null);

const form = ref({
  title: '',
  slug: '',
  thumbnail: '',
  content: '',
  status: 'draft'
});

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN');
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

const fetchPosts = async () => {
  loading.value = true;
  try {
    const res = await api.get('/admin/posts');
    posts.value = res.data;
  } catch (err: any) {
    console.error('Không thể tải bài viết:', err);
    triggerAlert('error', 'Không thể tải danh sách bài viết.');
  } finally {
    loading.value = false;
  }
};

const openCreateModal = () => {
  isEditing.value = false;
  currentId.value = null;
  form.value = {
    title: '',
    slug: '',
    thumbnail: '',
    content: '',
    status: 'draft'
  };
  showModal.value = true;
};

const openEditModal = (post: any) => {
  isEditing.value = true;
  currentId.value = post.id;
  form.value = {
    title: post.title,
    slug: post.slug,
    thumbnail: post.thumbnail || '',
    content: post.content || '',
    status: post.status
  };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const savePost = async () => {
  saving.value = true;
  try {
    if (isEditing.value && currentId.value) {
      await api.put(`/admin/posts/${currentId.value}`, form.value);
      triggerAlert('success', 'Đã cập nhật bài viết thành công.');
    } else {
      await api.post('/admin/posts', form.value);
      triggerAlert('success', 'Đã xuất bản bài viết mới thành công.');
    }
    closeModal();
    fetchPosts();
  } catch (err: any) {
    console.error('Lỗi khi lưu bài viết:', err);
    triggerAlert('error', err.response?.data?.message || 'Không thể lưu bài viết.');
  } finally {
    saving.value = false;
  }
};

const confirmDelete = async (post: any) => {
  if (!confirm(`Bạn có chắc chắn muốn xóa bài viết "${post.title}"?`)) return;
  try {
    await api.delete(`/admin/posts/${post.id}`);
    triggerAlert('success', 'Xóa bài viết thành công.');
    fetchPosts();
  } catch (err: any) {
    console.error('Lỗi khi xóa bài viết:', err);
    triggerAlert('error', 'Không thể xóa bài viết.');
  }
};

onMounted(() => {
  fetchPosts();
});
</script>

<style scoped>
.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 16px;
}

/* Modal overlay styling */
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
