<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <!-- Hero Section -->
    <section class="py-5 bg-gold-gradient position-relative text-center hero-section">
      <div class="hero-shape-1"></div>
      <div class="container py-4">
        <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold mb-3 shadow-sm text-uppercase">
          <Newspaper class="icon-news" /> Bản Tin MyPetClinic
        </span>
        <h1 class="display-4 fw-bold mb-3 gradient-text-gold">TIN TỨC & SỰ KIỆN</h1>
        <p class="fs-5 text-muted max-w-2xl mx-auto">
          Cập nhật các chương trình ưu đãi, sự kiện cộng đồng và những tin tức mới nhất từ hệ thống bệnh viện của chúng tôi.
        </p>
      </div>
    </section>

    <!-- Tin Tuc Content -->
    <section class="py-5 bg-white content-section">
      <div class="container">
        <!-- Main Highlight Event -->
        <div class="card border-0 glass-card p-4 mb-5 highlight-event-card">
          <div class="row-highlight">
            <div class="highlight-image-wrapper">
              <img src="https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?q=80&w=600&auto=format&fit=crop" class="img-fluid rounded-4 shadow-sm highlight-banner" alt="Main Event Banner" />
            </div>
            <div class="highlight-content">
              <span class="badge bg-danger mb-2">HOT EVENT</span>
              <h3 class="fw-bold mb-3 text-dark">Chiến Dịch Tiêm Vaccine Phòng Dại Miễn Phí Vì Cộng Đồng</h3>
              <p class="text-muted mb-4">
                Nhằm chung tay bảo vệ sức khỏe cộng đồng và đẩy lùi bệnh dại tại TP. Hồ Chí Minh, MyPetClinic tổ chức chiến dịch tiêm phòng vaccine dại hoàn toàn miễn phí cho 1000 chú chó mèo tại cả 3 cơ sở chính. 
              </p>
              <div class="highlight-meta-details mb-4">
                <span class="meta-item"><CalendarDays class="meta-icon" /> Thời gian: 01/06 - 15/06/2026</span>
                <span class="meta-item"><MapPin class="meta-icon" /> Toàn hệ thống</span>
              </div>
              <button class="btn-premium btn-sm" @click="showBookingModal = true">Đăng Ký Tham Gia Ngay</button>
            </div>
          </div>
        </div>

        <!-- Secondary News Grid -->
        <h4 class="fw-bold mb-4 block-title-news">Bản Tin Gần Đây</h4>
        
        <div v-if="loading" class="text-center py-5">
          <div class="spinner-border text-warning" role="status"></div>
          <p class="text-muted mt-2 small">Đang tải tin tức...</p>
        </div>

        <div v-else class="news-cards-grid">
          <!-- Dynamic Articles -->
          <div v-for="post in posts" :key="post.id" class="news-card-col" @click="selectPost(post)" style="cursor: pointer;">
            <div class="card border-0 glass-card h-100 overflow-hidden shadow-sm article-card">
              <div class="position-relative img-wrapper">
                <img :src="post.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" class="card-img-top article-img" :alt="post.title" />
                <span class="position-absolute badge-category bg-warning text-dark text-uppercase">Cẩm Nang</span>
              </div>
              <div class="card-body p-4 article-body">
                <div class="d-flex align-items-center gap-2 mb-2 text-muted small meta-date">
                  <CalendarDays class="meta-icon" /> {{ formatDate(post.createdAt) }}
                </div>
                <h5 class="card-title fw-bold mb-2 text-dark">{{ post.title }}</h5>
                <p class="card-text small text-muted line-clamp">
                  {{ truncateText(post.content || '', 100) }}
                </p>
              </div>
            </div>
          </div>

          <!-- Fallback when no posts -->
          <div v-if="posts.length === 0" class="col-12 text-center py-5 text-muted">
            <i class="bi bi-journal-x fs-1 d-block mb-2 text-warning opacity-50"></i>
            Hiện tại chưa có bài viết mới. Vui lòng quay lại sau!
          </div>
        </div>
      </div>
    </section>

    <!-- Detail Article Modal -->
    <div v-if="selectedPost" class="zalo-modal-overlay" @click.self="selectedPost = null">
      <div class="zalo-modal-card" style="max-width: 650px;">
        <div class="zalo-modal-header bg-warning text-dark">
          <h5 class="modal-title fw-bold">{{ selectedPost.title }}</h5>
          <button class="modal-close text-dark border-0 bg-transparent" @click="selectedPost = null"><i class="bi bi-x-lg fs-5"></i></button>
        </div>
        <div class="zalo-modal-body text-start" style="max-height: 70vh; overflow-y: auto;">
          <img :src="selectedPost.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" class="img-fluid rounded-4 w-100 mb-4 object-fit-cover" style="max-height: 250px;" />
          <div class="d-flex align-items-center gap-2 mb-3 text-muted small">
            <CalendarDays class="meta-icon" /> Đăng ngày: {{ formatDate(selectedPost.createdAt) }} | Tác giả: {{ selectedPost.authorName }}
          </div>
          <div class="text-dark" style="white-space: pre-line; line-height: 1.8;">
            {{ selectedPost.content }}
          </div>
        </div>
      </div>
    </div>

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
import { ref, onMounted } from 'vue';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
import { Newspaper, CalendarDays, MapPin } from '@lucide/vue';
import api from '../services/api';

const showBookingModal = ref(false);
const posts = ref<any[]>([]);
const loading = ref(false);
const selectedPost = ref<any>(null);

const fetchPosts = async () => {
  loading.value = true;
  try {
    const res = await api.get('/posts');
    posts.value = res.data;
  } catch (err) {
    console.error('Không thể tải bài viết:', err);
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

const selectPost = (post: any) => {
  selectedPost.value = post;
};

const truncateText = (text: string, length: number) => {
  if (text.length <= length) return text;
  return text.substring(0, length) + '...';
};

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};

onMounted(() => {
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
  z-index: 1;
  pointer-events: none;
}

/* Highlight Event layout */
.highlight-event-card {
  background-color: white !important;
}

.row-highlight {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  align-items: center;
}

@media (max-width: 991px) {
  .row-highlight {
    grid-template-columns: 1fr;
  }
}

.highlight-image-wrapper {
  width: 100%;
}

.highlight-banner {
  width: 100%;
  max-height: 350px;
  object-fit: cover;
  border-radius: var(--radius-md);
}

.highlight-content {
  display: flex;
  flex-direction: column;
  align-items: start;
}

.badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
  text-transform: uppercase;
  color: white;
}

.bg-danger { background-color: #ef4444; }
.bg-warning { background-color: var(--primary-gold); }
.bg-info { background-color: #0ea5e9; }
.bg-success { background-color: #10b981; }

.highlight-meta-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 0.85rem;
  color: var(--text-muted);
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.meta-icon {
  width: 16px;
  height: 16px;
  color: var(--primary-gold);
}

.block-title-news {
  font-size: 1.4rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
}

/* Secondary news cards grid */
.news-cards-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 2rem;
}

.article-card {
  display: flex;
  flex-direction: column;
  background-color: white !important;
}

.img-wrapper {
  height: 200px;
  overflow: hidden;
}

.article-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform var(--transition-speed);
}

.article-card:hover .article-img {
  transform: scale(1.03);
}

.badge-category {
  position: absolute;
  top: 15px;
  left: 15px;
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.3rem 0.6rem;
  border-radius: 4px;
}

.article-body {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  flex-grow: 1;
}

.meta-date {
  font-size: 0.75rem;
  color: var(--text-muted);
  display: flex;
  align-items: center;
  gap: 4px;
}

.card-title {
  font-size: 1.05rem;
  font-weight: 750;
  line-height: 1.4;
  margin-top: 4px;
  margin-bottom: 8px;
}

.card-text {
  font-size: 0.8rem;
  line-height: 1.6;
  color: var(--text-muted);
  margin: 0;
}
</style>
