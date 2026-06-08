<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <BookOpen class="icon-book" /> Góc Y Khoa & Đời Sống
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">SỨC KHỎE THÚ CƯNG</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Trang bị kiến thức chăm sóc khoa học, nhận biết sớm các triệu chứng bệnh lý thường gặp dưới sự cố vấn chuyên môn từ bác sĩ.
        </p>
      </div>
    </section>

    <!-- Content Section -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <div class="grid-layout">
          <!-- Sidebar widgets -->
          <aside class="sidebar-wrapper">
            <!-- Search box widget -->
            <div class="card border-0 glass-card p-3 mb-4 widget-card">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2">
                <Search class="widget-icon" /> Tìm kiếm bài viết
              </h6>
              <div class="search-box">
                <input 
                  type="text" 
                  v-model="searchQuery" 
                  class="form-control input-premium search-input" 
                  placeholder="Nhập từ khóa..." 
                />
              </div>
            </div>

            <!-- Categories Widget -->
            <div class="card border-0 glass-card p-3 mb-4 widget-card">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2">
                <Tag class="widget-icon" /> Chuyên Mục
              </h6>
              <div class="nav flex-column gap-1">
                <a class="sidebar-link active" href="#" @click.prevent="selectedCategory = 'All'">
                  <ChevronRight class="chevron" /> Tất cả bài viết
                </a>
                <a class="sidebar-link" href="#" @click.prevent="selectedCategory = 'Dinh Dưỡng'">
                  <ChevronRight class="chevron" /> Dinh Dưỡng Thú Cưng
                </a>
                <a class="sidebar-link" href="#" @click.prevent="selectedCategory = 'Bệnh Học'">
                  <ChevronRight class="chevron" /> Bệnh Học Chó Mèo
                </a>
                <a class="sidebar-link" href="#" @click.prevent="selectedCategory = 'Kinh Nghiệm'">
                  <ChevronRight class="chevron" /> Kinh Nghiệm Nuôi Dạy
                </a>
                <a class="sidebar-link" href="#" @click.prevent="selectedCategory = 'Y Học Dự Phòng'">
                  <ChevronRight class="chevron" /> Y Học Dự Phòng
                </a>
              </div>
            </div>

            <!-- Popular Articles Widget -->
            <div class="card border-0 glass-card p-3 widget-card">
              <h6 class="fw-bold mb-3 d-flex align-items-center gap-2">
                <Star class="widget-icon text-warning" /> Đọc Nhiều Nhất
              </h6>
              <div class="popular-list">
                <a href="#" class="popular-item" @click.prevent>
                  <span class="popular-date">25/05/2026</span>
                  <strong>Nấm da ở chó mèo có lây sang người không?</strong>
                </a>
                <a href="#" class="popular-item" @click.prevent>
                  <span class="popular-date">24/05/2026</span>
                  <strong>Tại sao chó bị rụng lông & điều trị thế nào?</strong>
                </a>
                <a href="#" class="popular-item" @click.prevent>
                  <span class="popular-date">20/05/2026</span>
                  <strong>Chó bị táo bón: Cách chữa trị tại nhà</strong>
                </a>
              </div>
            </div>
          </aside>

          <!-- Articles Grid -->
          <main class="main-content">
            <div class="articles-grid">
              <!-- Article Card -->
              <div v-for="article in filteredArticles" :key="article.id" class="article-card glass-card">
                <div class="article-img-wrapper">
                  <img :src="article.image" class="article-img" :alt="article.title" />
                </div>
                <div class="article-body">
                  <span class="badge" :class="article.badgeClass">{{ article.category }}</span>
                  <h5 class="article-title">{{ article.title }}</h5>
                  <p class="article-desc">{{ article.excerpt }}</p>
                </div>
              </div>
            </div>
            
            <div v-if="filteredArticles.length === 0" class="text-center py-5 no-results">
              Không tìm thấy bài viết nào phù hợp.
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
import { ref, computed } from 'vue';
import Header from '../../components/layout/Header.vue';
import Footer from '../../components/layout/Footer.vue';
import BookingModal from '../../components/shared/BookingModal.vue';
import { BookOpen, Search, Tag, Star, ChevronRight } from '@lucide/vue';

const showBookingModal = ref(false);
const searchQuery = ref('');
const selectedCategory = ref('All');

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};

const articles = ref([
  {
    id: 1,
    title: 'Nấm da ở chó mèo có lây sang người không?',
    category: 'Bệnh Học',
    badgeClass: 'bg-danger',
    image: 'https://images.unsplash.com/photo-1596492784531-6e6eb5ea9993?q=80&w=400&auto=format&fit=crop',
    excerpt: 'Tìm hiểu nguyên nhân gây nấm da (Microsporum canis), các triệu chứng điển hình ở người khi bị lây nhiễm (vết đỏ tròn như đồng xu, ngứa ngáy) và biện pháp phòng ngừa triệt để tại nhà...'
  },
  {
    id: 2,
    title: 'Tại sao chó bị rụng lông và cách điều trị hiệu quả',
    category: 'Kinh Nghiệm',
    badgeClass: 'bg-warning text-dark',
    image: 'https://images.unsplash.com/photo-1581888227599-779811939961?q=80&w=400&auto=format&fit=crop',
    excerpt: 'Phân biệt rụng lông sinh lý tự nhiên và rụng lông bệnh lý do ký sinh trùng (ghẻ demodex, xà mâu, bọ chét) hoặc dị ứng thức ăn để có hướng can thiệp y khoa kịp thời...'
  },
  {
    id: 3,
    title: 'Chó bị táo bón: Biểu hiện và cách điều trị tại nhà',
    category: 'Dinh Dưỡng',
    badgeClass: 'bg-info',
    image: 'https://images.unsplash.com/photo-1544568100-847a948585b9?q=80&w=400&auto=format&fit=crop',
    excerpt: 'Hướng dẫn bổ sung chất xơ hòa tan, men vi sinh đường ruột và thay đổi thói quen cho chó uống nước nhằm điều trị dứt điểm chứng táo bón, khó tiêu ở thú cưng...'
  },
  {
    id: 4,
    title: 'Chế độ dinh dưỡng khoa học cho mèo dưới 1 năm tuổi',
    category: 'Dinh Dưỡng',
    badgeClass: 'bg-info',
    image: 'https://images.unsplash.com/photo-1533738363-b7f9aef128ce?q=80&w=400&auto=format&fit=crop',
    excerpt: 'Thời kỳ phát triển vàng quyết định tầm vóc và đề kháng của bé mèo. Những thành phần dinh dưỡng bắt buộc (Taurine, Canxi, Protein dễ tiêu) và các thực phẩm tuyệt đối cấm kỵ...'
  }
]);

const filteredArticles = computed(() => {
  return articles.value.filter(a => {
    const matchesSearch = a.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                          a.excerpt.toLowerCase().includes(searchQuery.value.toLowerCase());
    const matchesCategory = selectedCategory.value === 'All' || a.category === selectedCategory.value;
    return matchesSearch && matchesCategory;
  });
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

.icon-book {
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
  z-index: 1;
  pointer-events: none;
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
  padding: 0.5rem 0.8rem;
  border-radius: var(--radius-sm);
  color: var(--text-dark) !important;
  font-weight: 600;
  font-size: 0.85rem;
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

/* Articles layout */
.articles-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

@media (max-width: 576px) {
  .articles-grid {
    grid-template-columns: 1fr;
  }
}

.article-card {
  overflow: hidden;
  height: 100%;
  display: flex;
  flex-direction: column;
  background-color: white !important;
}

.article-img-wrapper {
  height: 180px;
}

.article-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.article-body {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.badge {
  align-self: start;
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
  text-transform: uppercase;
  margin-bottom: 8px;
  color: white;
}

.bg-danger { background-color: #ef4444; }
.bg-warning { background-color: var(--primary-gold); }
.bg-info { background-color: #0ea5e9; }

.article-title {
  font-size: 1.1rem;
  font-weight: 750;
  line-height: 1.4;
  margin-bottom: 8px;
  color: var(--text-dark);
}

.article-desc {
  font-size: 0.8rem;
  line-height: 1.6;
  color: var(--text-muted);
  margin: 0;
}

.no-results {
  font-size: 0.95rem;
  color: var(--text-muted);
}
</style>
