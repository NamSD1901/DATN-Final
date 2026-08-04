<template>
  <div class="blog-admin-container p-4">
    <!-- Header Tools -->
    <div class="glass-card p-4 mb-4">
      <div class="d-flex flex-wrap justify-content-between align-items-center gap-3 mb-4">
        <div>
          <h4 class="fw-bold text-dark mb-1"><i class="bi bi-journal-richtext text-warning me-2"></i>Quản Trị Bài Viết & Tin Tức</h4>
          <p class="text-muted small mb-0">Viết cẩm nang chăm sóc thú cưng và các thông báo khuyến mại của phòng khám</p>
        </div>
        
        <button v-if="activeTab === 'posts'" class="btn btn-warning rounded-pill px-4 py-2 fw-bold text-dark shadow-sm d-flex align-items-center gap-1" @click="openCreateModal">
          <i class="bi bi-plus-circle-fill"></i> Viết bài mới
        </button>
      </div>

      <!-- Tabs -->
      <ul class="nav nav-tabs nav-tabs-premium" role="tablist">
        <li class="nav-item" role="presentation">
          <button class="nav-link fw-bold px-4" :class="{ active: activeTab === 'posts' }" @click="activeTab = 'posts'">
            <i class="bi bi-list-ul me-2"></i> Danh sách Bài viết
          </button>
        </li>
        <li class="nav-item" role="presentation">
          <button class="nav-link fw-bold px-4" :class="{ active: activeTab === 'categories' }" @click="activeTab = 'categories'">
            <i class="bi bi-tags-fill me-2"></i> Danh mục Bài viết
          </button>
        </li>
      </ul>
    </div>

    <div v-if="activeTab === 'posts'" class="tab-pane-content">
      <!-- Error/Success Alerts -->
    <div v-if="successMsg" class="alert alert-success rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-check-circle-fill me-2"></i>{{ successMsg }}
    </div>
    <div v-if="postStore.error" class="alert alert-danger rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ postStore.error }}
    </div>
    <div v-if="validationError" class="alert alert-warning rounded-4 shadow-sm mb-4" role="alert">
      <i class="bi bi-shield-exclamation me-2"></i>{{ validationError }}
    </div>

    <!-- Filters -->
    <div class="card border-0 shadow-sm rounded-4 mb-4 bg-white p-3">
      <div class="row g-2">
        <div class="col-md-4">
          <input type="text" v-model="filterSearch" @input="debouncedFetch" class="form-control rounded-pill bg-light" placeholder="Tìm kiếm tiêu đề...">
        </div>
        <div class="col-md-3">
          <select v-model="filterStatus" @change="fetchPosts" class="form-select rounded-pill bg-light">
            <option value="">Tất cả trạng thái</option>
            <option value="published">Đã đăng</option>
            <option value="draft">Bản nháp</option>
          </select>
        </div>
        <div class="col-md-3">
          <select v-model="filterCategory" @change="fetchPosts" class="form-select rounded-pill bg-light">
            <option value="">Tất cả danh mục</option>
            <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Blog Posts Table List -->
    <div class="card border-0 shadow-sm rounded-4 overflow-hidden bg-white">
      <div class="table-responsive">
        <table class="table align-middle border-bottom mb-0">
          <thead class="table-light">
            <tr>
              <th class="py-3 ps-4 border-0 text-muted small" width="100">Ảnh bìa</th>
              <th class="py-3 border-0 text-muted small">Tiêu đề bài viết</th>
              <th class="py-3 border-0 text-muted small">Danh mục / Tác giả</th>
              <th class="py-3 border-0 text-muted small" width="130">Ngày tạo</th>
              <th class="py-3 border-0 text-muted small" width="130">Trạng thái</th>
              <th class="py-3 border-0 text-muted small text-end pe-4" width="150">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="post in posts" :key="post.id">
              <td class="ps-4 py-3 border-0">
                <img :src="post.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" 
                     alt="Post thumbnail" class="rounded-3 object-fit-cover shadow-sm border" width="65" height="45" />
              </td>
              <td class="py-3 border-0">
                <div class="fw-bold text-dark mb-1">{{ post.title }}</div>
                <span class="text-muted small font-monospace">/{{ post.slug }}</span>
                <div v-if="post.tags && post.tags.length" class="mt-1">
                  <span v-for="tag in post.tags" :key="tag" class="badge bg-light text-secondary border me-1 fw-normal" style="font-size: 0.65rem;">#{{ tag }}</span>
                </div>
              </td>
              <td class="py-3 border-0">
                <div class="text-primary fw-bold small mb-1"><i class="bi bi-folder2-open me-1"></i>{{ post.categoryName || 'Chưa phân loại' }}</div>
                <div class="text-muted small"><i class="bi bi-person me-1"></i>{{ post.authorName }}</div>
              </td>
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
            <tr v-if="postStore.loading && posts.length === 0">
              <td colspan="6" class="text-center py-5 text-muted">
                <div class="spinner-border spinner-border-sm text-warning me-2" role="status"></div>
                Đang tải danh sách bài viết...
              </td>
            </tr>
            <tr v-if="!postStore.loading && posts.length === 0">
              <td colspan="6" class="text-center py-5 text-muted">
                <i class="bi bi-journal-x fs-1 d-block mb-2 text-black-50 opacity-25"></i>
                Chưa có bài viết nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <!-- Pagination -->
      <div v-if="postStore.totalCount > 10" class="p-3 border-top d-flex justify-content-between align-items-center">
        <span class="text-muted small">Hiển thị {{ posts.length }} / {{ postStore.totalCount }} bài viết</span>
        <div class="d-flex gap-2">
          <button class="btn btn-sm btn-light border-0 shadow-sm" :disabled="pageIndex === 1" @click="pageIndex--; fetchPosts()">Trang trước</button>
          <button class="btn btn-sm btn-light border-0 shadow-sm" :disabled="posts.length < 10" @click="pageIndex++; fetchPosts()">Trang sau</button>
        </div>
      </div>
    </div>
    </div> <!-- End Posts Tab -->

    <div v-else-if="activeTab === 'categories'" class="tab-pane-content mt-3">
      <CategoriesAdminTab :hide-header="true" />
    </div>

    <!-- Create/Edit Post Modal Dialog -->
    <div v-if="showModal" class="zalo-modal-overlay" @click.self="closeModal">
      <div class="zalo-modal-card" style="max-width: 1000px; height: 95vh; display: flex; flex-direction: column;">
        <div class="zalo-modal-header bg-warning text-dark flex-shrink-0">
          <h5 class="modal-title fw-bold">
            <i class="bi bi-journal-plus me-2"></i>{{ isEditing ? 'Chỉnh Sửa Bài Viết' : 'Viết Bài Mới' }}
          </h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="closeModal"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start flex-grow-1 overflow-auto bg-light">
          <form @submit.prevent="savePost" class="bg-white p-4 rounded-4 shadow-sm border h-100" id="postForm">
            <div class="row g-4 h-100">
              <!-- Cột Trái -->
              <div class="col-lg-8 d-flex flex-column">
                <div class="mb-3">
                  <label class="form-label small text-muted fw-bold">Tiêu đề bài viết <span class="text-danger">*</span></label>
                  <input type="text" v-model="form.title" class="form-control form-control-lg fw-bold rounded-3 border-warning-subtle" placeholder="Nhập tiêu đề hấp dẫn..." required maxlength="255" />
                </div>

                <div class="mb-3">
                  <label class="form-label small text-muted fw-bold">Đường dẫn tĩnh (Slug) <span class="text-danger">*</span></label>
                  <div class="input-group">
                    <span class="input-group-text bg-light text-muted small border-end-0 rounded-start-3">/posts/</span>
                    <input type="text" v-model="form.slug" class="form-control rounded-end-3 border-start-0" placeholder="tieu-de-viet-tat-khong-dau" required pattern="^[a-z0-9-]+$" title="Chỉ chứa chữ thường, số và dấu gạch ngang" maxlength="255" />
                  </div>
                </div>

                <div class="mb-3">
                  <label class="form-label small text-muted fw-bold">Nội dung tóm tắt (Summary) <span class="text-danger">*</span></label>
                  <textarea v-model="form.summary" class="form-control rounded-3 bg-light border-0" rows="2" placeholder="Tóm tắt ngắn gọn nội dung bài viết..." required maxlength="500"></textarea>
                </div>

                <div class="mb-3 flex-grow-1 d-flex flex-column">
                  <label class="form-label small text-muted fw-bold">Nội dung bài viết (Rich Text) <span class="text-danger">*</span></label>
                  <div class="border rounded-3 flex-grow-1" style="min-height: 300px; display: flex; flex-direction: column;">
                    <QuillEditor theme="snow" v-model:content="form.content" contentType="html" class="flex-grow-1" />
                  </div>
                </div>
              </div>

              <!-- Cột Phải -->
              <div class="col-lg-4 border-start overflow-auto">
                <div class="mb-4">
                  <label class="form-label small text-muted fw-bold">Trạng thái phát hành</label>
                  <select v-model="form.status" class="form-select rounded-3 bg-light border-0">
                    <option value="draft">Bản nháp (Draft)</option>
                    <option value="published">Đăng công khai (Published)</option>
                  </select>
                </div>

                <div class="mb-4">
                  <label class="form-label small text-muted fw-bold">Danh mục <span class="text-danger">*</span></label>
                  <select v-model="form.categoryId" class="form-select rounded-3" required>
                    <option :value="null">-- Chọn danh mục --</option>
                    <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                  </select>
                </div>

                <div class="mb-4">
                  <label class="form-label small text-muted fw-bold">Ảnh bìa (Thumbnail URL) <span v-if="form.status === 'published'" class="text-danger">*</span></label>
                  <input type="text" v-model="form.thumbnail" class="form-control rounded-3 mb-2" placeholder="https://..." :required="form.status === 'published'" />
                  <img v-if="form.thumbnail" :src="form.thumbnail" class="img-fluid rounded-3 shadow-sm" alt="Preview" @error="handleImageError">
                </div>

                <div class="mb-4">
                  <label class="form-label small text-muted fw-bold">Thẻ Tag (Ngăn cách bởi dấu phẩy)</label>
                  <input type="text" v-model="formTagsInput" class="form-control rounded-3" placeholder="Ví dụ: Chó, Mèo, Dinh dưỡng" />
                </div>
                
                <div class="mb-4">
                  <label class="form-label small text-muted fw-bold">SEO Meta Title</label>
                  <input type="text" v-model="form.metaTitle" class="form-control rounded-3" placeholder="Tiêu đề hiển thị trên Google..." maxlength="60" />
                  <small class="text-muted">{{ form.metaTitle.length }}/60 ký tự</small>
                </div>
                <div class="mb-4">
                  <label class="form-label small text-muted fw-bold">SEO Meta Description</label>
                  <textarea v-model="form.metaDescription" class="form-control rounded-3" rows="3" placeholder="Mô tả SEO..." maxlength="160"></textarea>
                  <small class="text-muted">{{ form.metaDescription.length }}/160 ký tự</small>
                </div>
              </div>
            </div>
          </form>
        </div>
        <div class="zalo-modal-footer bg-white border-top p-3 d-flex justify-content-between align-items-center flex-shrink-0">
          <div class="text-danger small fw-bold">
             <span v-if="validationError"><i class="bi bi-shield-exclamation"></i> {{ validationError }}</span>
          </div>
          <div class="d-flex gap-2">
            <button type="button" class="btn btn-outline-secondary rounded-pill px-4 fw-bold" @click="closeModal">Hủy bỏ</button>
            <button type="submit" form="postForm" class="btn btn-warning text-dark rounded-pill px-4 fw-bold shadow-sm" :disabled="saving">
              {{ saving ? 'Đang lưu...' : (isEditing ? 'Cập nhật' : 'Đăng tải') }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { usePostStore } from '../../stores/post.store';
import CategoriesAdminTab from './CategoriesAdminTab.vue';
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';

const postStore = usePostStore();

const posts = computed(() => postStore.posts);
const categories = computed(() => postStore.categories);

const activeTab = ref('posts');
const saving = ref(false);
const successMsg = ref('');
const validationError = ref('');

const showModal = ref(false);
const isEditing = ref(false);
const currentId = ref<number | null>(null);

const filterSearch = ref('');
const filterStatus = ref('');
const filterCategory = ref<number | string>('');
const pageIndex = ref(1);

const formTagsInput = ref('');
const form = ref({
  title: '',
  slug: '',
  summary: '',
  thumbnail: '',
  content: '',
  status: 'draft',
  categoryId: null as number | null,
  metaTitle: '',
  metaDescription: '',
  tags: [] as string[]
});

let searchTimeout: any = null;
const debouncedFetch = () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    pageIndex.value = 1;
    fetchPosts();
  }, 500);
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
};

const triggerAlert = (msg: string) => {
  successMsg.value = msg;
  setTimeout(() => successMsg.value = '', 4000);
};

const handleImageError = (e: Event) => {
  (e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop';
};

const fetchPosts = async () => {
  await postStore.fetchAdminPosts(
    pageIndex.value, 
    10, 
    filterSearch.value, 
    filterStatus.value, 
    filterCategory.value ? Number(filterCategory.value) : undefined
  );
};

const generateSlug = (text: string) => {
  return text.toString().toLowerCase()
    .normalize('NFD').replace(/[\u0300-\u036f]/g, "") // Remove accents
    .replace(/đ/g, "d").replace(/Đ/g, "d")
    .replace(/\s+/g, '-') // Replace spaces with -
    .replace(/[^\w-]+/g, '') // Remove all non-word chars
    .replace(/--+/g, '-') // Replace multiple - with single -
    .replace(/^-+/, '') // Trim - from start of text
    .replace(/-+$/, ''); // Trim - from end of text
};

// Tự động generate slug khi gõ title (chỉ khi đang thêm mới)
watch(() => form.value.title, (newTitle) => {
  if (!isEditing.value && newTitle) {
    form.value.slug = generateSlug(newTitle);
  }
});

const openCreateModal = () => {
  isEditing.value = false;
  currentId.value = null;
  formTagsInput.value = '';
  validationError.value = '';
  form.value = {
    title: '',
    slug: '',
    summary: '',
    thumbnail: '',
    content: '',
    status: 'draft',
    categoryId: null,
    metaTitle: '',
    metaDescription: '',
    tags: []
  };
  showModal.value = true;
};

const openEditModal = (post: any) => {
  isEditing.value = true;
  currentId.value = post.id;
  formTagsInput.value = (post.tags || []).join(', ');
  validationError.value = '';
  form.value = {
    title: post.title,
    slug: post.slug || '',
    summary: post.summary || '',
    thumbnail: post.thumbnail || '',
    content: post.content || '',
    status: post.status,
    categoryId: post.categoryId || null,
    metaTitle: post.metaTitle || '',
    metaDescription: post.metaDescription || '',
    tags: []
  };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const validateForm = () => {
  // VR-01: Tiêu đề trống -> HTML5 required covers this
  // VR-02: Tiêu đề max 255 -> HTML5 maxlength covers this
  
  // VR-04: Slug trống -> HTML5 required covers this
  // VR-06: Slug format -> HTML5 pattern covers this
  
  // VR-08: Tóm tắt trống -> HTML5 required covers this
  
  // VR-10: Nội dung trống
  // Quill content is HTML, so it might be '<p><br></p>' or empty.
  const contentRaw = form.value.content.replace(/<[^>]*>?/gm, '').trim();
  if (!contentRaw && !form.value.content.includes('<img')) {
    validationError.value = 'Nội dung bài viết không được để trống (VR-10).';
    return false;
  }

  // VR-11: Ảnh đại diện trống khi xuất bản
  if (form.value.status === 'published' && !form.value.thumbnail) {
    validationError.value = 'Ảnh đại diện không được để trống khi Xuất bản (VR-11).';
    return false;
  }

  // VR-14: Chủ đề bắt buộc
  if (!form.value.categoryId) {
    validationError.value = 'Vui lòng chọn Chủ đề bài viết (VR-14).';
    return false;
  }

  // Check tags limit
  const processedTags = formTagsInput.value
      .split(',')
      .map(t => t.trim())
      .filter(t => t.length > 0);
  
  if (processedTags.length > 10) {
    validationError.value = 'Chỉ được phép gắn tối đa 10 tags.';
    return false;
  }

  return true;
};

const savePost = async () => {
  validationError.value = '';
  
  if (!validateForm()) {
    return;
  }

  saving.value = true;
  try {
    const processedTags = formTagsInput.value
      .split(',')
      .map(t => t.trim())
      .filter(t => t.length > 0);
    
    form.value.tags = processedTags;

    if (isEditing.value && currentId.value) {
      await postStore.updatePost(currentId.value, form.value);
      triggerAlert('Đã cập nhật bài viết thành công.');
    } else {
      await postStore.createPost(form.value);
      triggerAlert('Đã xuất bản bài viết mới thành công.');
    }
    closeModal();
    fetchPosts();
  } catch (err: any) {
    validationError.value = postStore.error || 'Lỗi khi lưu bài viết. Vui lòng kiểm tra lại.';
  } finally {
    saving.value = false;
  }
};

const confirmDelete = async (post: any) => {
  if (!confirm(`Bạn có chắc chắn muốn xóa bài viết "${post.title}"?`)) return;
  try {
    await postStore.deletePost(post.id);
    triggerAlert('Xóa bài viết thành công.');
    fetchPosts();
  } catch (err: any) {
    // Error is handled
  }
};

onMounted(() => {
  fetchPosts();
  if (categories.value.length === 0) {
    postStore.fetchCategories();
  }
});
</script>

<style scoped>
.nav-tabs-premium {
  border-bottom: 2px solid #f1f5f9;
}
.nav-tabs-premium .nav-link {
  color: #64748b;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  transition: all 0.3s ease;
}
.nav-tabs-premium .nav-link:hover {
  color: #f59e0b;
  border-color: transparent;
}
.nav-tabs-premium .nav-link.active {
  color: #d97706;
  background: transparent;
  border-color: #f59e0b;
}

.tab-pane-content {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}

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
}
.zalo-modal-header {
  padding: 1.2rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.zalo-modal-body {
  padding: 1.5rem;
}

/* Customize Quill */
:deep(.ql-container) {
  min-height: 250px;
  font-size: 1.05rem;
  font-family: inherit;
  border-bottom-left-radius: 0.5rem;
  border-bottom-right-radius: 0.5rem;
}
:deep(.ql-toolbar) {
  border-top-left-radius: 0.5rem;
  border-top-right-radius: 0.5rem;
  background-color: #f8f9fa;
}
</style>
