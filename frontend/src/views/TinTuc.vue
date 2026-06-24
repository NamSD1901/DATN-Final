<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4 position-relative z-index-1">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <Newspaper class="icon-news" /> Bản Tin MyPetClinic
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">TIN TỨC &amp; SỰ KIỆN</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto mb-4">
          Cập nhật các chương trình ưu đãi, sự kiện cộng đồng và kiến thức chăm sóc thú cưng mới nhất từ hệ thống bệnh viện của chúng tôi.
        </p>

        <!-- Search Bar -->
        <div class="max-w-2xl mx-auto">
          <div class="input-group input-group-lg shadow-sm rounded-pill overflow-hidden bg-white border">
            <span class="input-group-text bg-transparent border-0 pe-1 text-muted">
              <i class="bi bi-search"></i>
            </span>
            <input type="text" v-model="searchQuery" @input="debouncedSearch" class="form-control border-0 bg-transparent fs-6" placeholder="Tìm kiếm bài viết..." />
          </div>
        </div>
      </div>
    </section>

    <!-- Content -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        
        <!-- Filter Pills -->
        <div class="d-flex flex-wrap gap-2 mb-5 justify-content-center">
          <button class="btn rounded-pill px-4" 
                  :class="!selectedCategory ? 'btn-warning fw-bold shadow-sm' : 'btn-outline-secondary'"
                  @click="selectCategory('')">
            Tất cả
          </button>
          <button v-for="cat in categories" :key="cat.id" 
                  class="btn rounded-pill px-4"
                  :class="selectedCategory === cat.slug ? 'btn-warning fw-bold shadow-sm' : 'btn-outline-secondary'"
                  @click="selectCategory(cat.slug)">
            {{ cat.name }}
          </button>
        </div>

        <div v-if="postStore.loading && posts.length === 0" class="text-center py-5">
          <div class="spinner-border text-warning" role="status"></div>
          <p class="text-muted mt-2 small">Đang tải tin tức...</p>
        </div>

        <div v-else class="news-cards-grid">
          <!-- Dynamic Articles -->
          <div v-for="post in posts" :key="post.id" class="news-card-col">
            <router-link :to="`/news/${post.slug}`" class="text-decoration-none text-dark">
              <div class="card border-0 glass-card h-100 overflow-hidden shadow-sm article-card">
                <div class="position-relative img-wrapper">
                  <img :src="post.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" class="card-img-top article-img" :alt="post.title" />
                  <span v-if="post.categoryName" class="position-absolute badge-category bg-warning text-dark text-uppercase shadow-sm">{{ post.categoryName }}</span>
                </div>
                <div class="card-body p-4 article-body">
                  <div class="d-flex align-items-center justify-content-between mb-2">
                    <div class="text-muted small meta-date">
                      <CalendarDays class="meta-icon" /> {{ formatDate(post.publishedAt || post.createdAt) }}
                    </div>
                    <div class="text-muted small meta-date">
                      <i class="bi bi-eye"></i> {{ post.viewCount || 0 }}
                    </div>
                  </div>
                  
                  <h5 class="card-title fw-bold mb-2 text-dark line-clamp-2">{{ post.title }}</h5>
                  <p class="card-text small text-muted line-clamp-3">
                    {{ post.summary || truncateText(post.content || '', 120) }}
                  </p>
                </div>
                <div class="card-footer bg-transparent border-0 p-4 pt-0 text-warning fw-bold small d-flex align-items-center">
                  Đọc tiếp <i class="bi bi-arrow-right ms-2"></i>
                </div>
              </div>
            </router-link>
          </div>

          <!-- Fallback when no posts -->
          <div v-if="posts.length === 0" class="col-12 text-center py-5 text-muted" style="grid-column: 1 / -1;">
            <i class="bi bi-journal-x fs-1 d-block mb-2 text-warning opacity-50"></i>
            Không tìm thấy bài viết nào.
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="postStore.totalCount > 0" class="d-flex justify-content-center mt-5">
          <nav>
            <ul class="pagination pagination-lg">
              <li class="page-item" :class="{ disabled: currentPage === 1 }">
                <button class="page-link rounded-start-pill text-dark" @click="changePage(currentPage - 1)">Trước</button>
              </li>
              <li class="page-item disabled">
                <span class="page-link text-muted">Trang {{ currentPage }} / {{ totalPages }}</span>
              </li>
              <li class="page-item" :class="{ disabled: currentPage >= totalPages }">
                <button class="page-link rounded-end-pill text-dark" @click="changePage(currentPage + 1)">Sau</button>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </section>

    <Footer />

    <BookingModal 
      :show="showBookingModal" 
      @close="showBookingModal = false" 
      @success="handleBookingSuccess" 
      @error="handleBookingError" 
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usePostStore } from '../stores/post.store';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
import { Newspaper, CalendarDays } from 'lucide-vue-next';

const route = useRoute();
const router = useRouter();
const postStore = usePostStore();

const showBookingModal = ref(false);
const searchQuery = ref('');
const selectedCategory = ref('');
const currentPage = ref(1);

const posts = computed(() => postStore.posts);
const categories = computed(() => postStore.categories.filter((c: any) => c.isActive));
const totalPages = computed(() => Math.ceil(postStore.totalCount / 10) || 1);

let searchTimeout: any = null;

const debouncedSearch = () => {
  clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => {
    currentPage.value = 1;
    updateQueryParams();
    fetchPosts();
  }, 500);
};

const selectCategory = (slug: string) => {
  selectedCategory.value = slug;
  currentPage.value = 1;
  updateQueryParams();
  fetchPosts();
};

const changePage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page;
    updateQueryParams();
    fetchPosts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
};

const updateQueryParams = () => {
  router.replace({
    query: {
      ...route.query,
      q: searchQuery.value || undefined,
      cat: selectedCategory.value || undefined,
      p: currentPage.value > 1 ? currentPage.value : undefined
    }
  });
};

const fetchPosts = async () => {
  await postStore.fetchPublicPosts(currentPage.value, 12, searchQuery.value, selectedCategory.value);
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
};

const truncateText = (text: string, length: number) => {
  if (!text) return '';
  if (text.length <= length) return text;
  return text.substring(0, length) + '...';
};

const handleBookingSuccess = (msg: string) => alert(msg);
const handleBookingError = (msg: string) => alert(msg);

onMounted(async () => {
  // Sync state from URL
  if (route.query.q) searchQuery.value = route.query.q as string;
  if (route.query.cat) selectedCategory.value = route.query.cat as string;
  if (route.query.p) currentPage.value = parseInt(route.query.p as string) || 1;

  await postStore.fetchCategories();
  fetchPosts();
});
</script>

<style scoped>
.page-wrapper {
  background-color: var(--bg-light);
  color: var(--text-dark);
}

.hero-section {
  padding: 5rem 0;
  overflow: hidden;
}

.icon-news {
  width: 16px;
  height: 16px;
  display: inline-block;
  vertical-align: middle;
}

.hero-shape-1 {
  position: absolute;
  top: -20%;
  right: -10%;
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, rgba(254, 243, 199, 0.7) 0%, rgba(254, 243, 199, 0) 70%);
  z-index: 0;
  pointer-events: none;
}

.z-index-1 {
  z-index: 1;
}

.max-w-2xl {
  max-width: 650px;
}

/* News cards grid */
.news-cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 2rem;
}

.article-card {
  display: flex;
  flex-direction: column;
  background-color: white !important;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.article-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 .5rem 1.5rem rgba(0,0,0,.08) !important;
}

.img-wrapper {
  height: 220px;
  overflow: hidden;
}

.article-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--transition-speed);
}

.article-card:hover .article-img {
  transform: scale(1.05);
}

.badge-category {
  top: 15px;
  left: 15px;
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.4rem 0.8rem;
  border-radius: 6px;
}

.article-body {
  flex-grow: 1;
}

.meta-date {
  font-size: 0.8rem;
  display: flex;
  align-items: center;
  gap: 6px;
}

.meta-icon {
  width: 16px;
  height: 16px;
  color: var(--primary-gold);
}

.card-title {
  font-size: 1.15rem;
  line-height: 1.4;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
