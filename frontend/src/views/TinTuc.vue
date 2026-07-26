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
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Cập nhật các chương trình ưu đãi, sự kiện cộng đồng và kiến thức chăm sóc thú cưng mới nhất từ hệ thống bệnh viện của chúng tôi.
        </p>
      </div>
    </section>

    <!-- Content -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <div class="grid-layout">
          <!-- Sidebar widgets -->
          <aside class="sidebar-wrapper">
            <!-- Search box widget -->
            <div class="card border-0 glass-card p-3 mb-4 widget-card shadow-sm">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2">
                <Search class="widget-icon" /> Tìm kiếm bài viết
              </h6>
              <div class="search-box">
                <input 
                  type="text" 
                  v-model="searchQuery" 
                  @input="debouncedSearch"
                  class="form-control input-premium search-input" 
                  placeholder="Nhập từ khóa..." 
                />
              </div>
            </div>

            <!-- Categories Widget -->
            <div class="card border-0 glass-card p-3 mb-4 widget-card shadow-sm">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2">
                <Tag class="widget-icon" /> Chuyên Mục
              </h6>
              <div class="nav flex-column gap-1">
                <a 
                  class="sidebar-link" 
                  :class="{ active: !selectedCategory }" 
                  href="#" 
                  @click.prevent="selectCategory('')"
                >
                  <ChevronRight class="chevron" /> Tất cả bài viết
                </a>
                <a 
                  v-for="cat in categories" 
                  :key="cat.id" 
                  class="sidebar-link" 
                  :class="{ active: selectedCategory === cat.slug }" 
                  href="#" 
                  @click.prevent="selectCategory(cat.slug)"
                >
                  <ChevronRight class="chevron" /> {{ cat.name }}
                </a>
              </div>
            </div>

            <!-- Popular Articles Widget -->
            <div class="card border-0 glass-card p-3 widget-card shadow-sm">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2">
                <Star class="widget-icon text-warning" /> Đọc Nhiều Nhất
              </h6>
              <div v-if="popularPosts.length > 0" class="popular-list">
                <router-link 
                  v-for="post in popularPosts" 
                  :key="post.id" 
                  :to="`/news/${post.slug}`" 
                  class="popular-item"
                >
                  <span class="popular-date">{{ formatDate(post.publishedAt || post.createdAt) }}</span>
                  <strong>{{ post.title }}</strong>
                </router-link>
              </div>
              <div v-else class="text-muted small">Chưa có bài viết nổi bật.</div>
            </div>
          </aside>

          <!-- Main Content / Articles Grid -->
          <main class="main-content">
            <div v-if="postStore.loading && posts.length === 0" class="text-center py-5">
              <div class="spinner-border text-warning" role="status"></div>
              <p class="text-muted mt-2 small">Đang tải tin tức &amp; sự kiện...</p>
            </div>

            <div v-else-if="posts.length > 0" class="news-cards-grid">
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
            </div>

            <!-- Fallback when no posts -->
            <div v-else class="text-center py-5 no-results card border-0 glass-card p-4 shadow-sm">
              <i class="bi bi-journal-x fs-1 d-block mb-2 text-warning opacity-50"></i>
              Không tìm thấy bài viết nào phù hợp.
            </div>

            <!-- Pagination -->
            <div v-if="postStore.totalCount > 0" class="d-flex justify-content-center mt-5">
              <nav>
                <ul class="pagination pagination-md">
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
          </main>
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
import { Newspaper, CalendarDays, Search, Tag, Star, ChevronRight } from 'lucide-vue-next';

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

const popularPosts = computed(() => {
  return [...postStore.posts]
    .sort((a: any, b: any) => (b.viewCount || 0) - (a.viewCount || 0))
    .slice(0, 3);
});

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
  await postStore.fetchPublicPosts(currentPage.value, 10, searchQuery.value, selectedCategory.value);
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
  const cleanText = text.replace(/<[^>]*>?/gm, '');
  if (cleanText.length <= length) return cleanText;
  return cleanText.substring(0, length) + '...';
};

const handleBookingSuccess = (msg: string) => alert(msg);
const handleBookingError = (msg: string) => alert(msg);

onMounted(async () => {
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

.grid-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 2.5rem;
}

@media (max-width: 991px) {
  .grid-layout {
    grid-template-columns: 1fr;
  }
  .sidebar-wrapper {
    margin-bottom: 2rem;
  }
}

.widget-card {
  background-color: white !important;
  border-radius: var(--radius-md);
}

.widget-icon {
  width: 18px;
  height: 18px;
  color: var(--primary-gold);
}

.search-box {
  display: flex;
}

.search-input {
  width: 100%;
  padding: 0.6rem 0.8rem;
  border-radius: var(--radius-sm);
  border: 1px solid rgba(0, 0, 0, 0.08);
}

.sidebar-link {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 0.6rem 0.8rem;
  border-radius: var(--radius-sm);
  color: var(--text-dark) !important;
  font-weight: 600;
  font-size: 0.88rem;
  text-decoration: none;
  transition: all var(--transition-speed);
}

.sidebar-link .chevron {
  width: 14px;
  height: 14px;
  transition: transform 0.2s;
}

.sidebar-link:hover, .sidebar-link.active {
  background-color: var(--primary-cream);
  color: var(--primary-dark) !important;
  transform: translateX(4px);
}

.popular-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.popular-item {
  text-decoration: none;
  color: var(--text-dark);
  font-size: 0.85rem;
  display: block;
}

.popular-item:hover strong {
  color: var(--primary-gold);
}

.popular-date {
  display: block;
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-bottom: 2px;
}

.popular-item strong {
  display: block;
  line-height: 1.4;
  font-weight: 700;
  transition: color 0.2s;
}

/* News cards grid */
.news-cards-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.8rem;
}

@media (max-width: 768px) {
  .news-cards-grid {
    grid-template-columns: 1fr;
  }
}

.article-card {
  display: flex;
  flex-direction: column;
  background-color: white !important;
  border-radius: var(--radius-md);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.article-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 .5rem 1.5rem rgba(0,0,0,.08) !important;
}

.img-wrapper {
  height: 200px;
  overflow: hidden;
}

.article-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
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
  font-size: 1.1rem;
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

.no-results {
  font-size: 0.95rem;
  color: var(--text-muted);
}
</style>
