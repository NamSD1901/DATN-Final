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
        <div class="news-cards-grid">
          <!-- Article 1 -->
          <div class="news-card-col">
            <div class="card border-0 glass-card h-100 overflow-hidden shadow-sm article-card">
              <div class="position-relative img-wrapper">
                <img src="https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop" class="card-img-top article-img" alt="News Image 1" />
                <span class="position-absolute badge-category bg-warning text-dark text-uppercase">Khuyến Mãi</span>
              </div>
              <div class="card-body p-4 article-body">
                <div class="d-flex align-items-center gap-2 mb-2 text-muted small meta-date">
                  <CalendarDays class="meta-icon" /> 26/05/2026
                </div>
                <h5 class="card-title fw-bold mb-2">Chào Hè Rực Rỡ - Ưu Đãi 20% Dịch Vụ Cắt Tỉa Lông</h5>
                <p class="card-text small text-muted">
                  Giúp bé cưng giải nhiệt mùa hè với bộ lông gọn gàng mát mẻ. MyPetClinic giảm ngay 20% cho tất cả khách hàng đặt lịch dịch vụ Grooming...
                </p>
              </div>
            </div>
          </div>

          <!-- Article 2 -->
          <div class="news-card-col">
            <div class="card border-0 glass-card h-100 overflow-hidden shadow-sm article-card">
              <div class="position-relative img-wrapper">
                <img src="https://images.unsplash.com/photo-1583337130417-3346a1be7dee?q=80&w=400&auto=format&fit=crop" class="card-img-top article-img" alt="News Image 2" />
                <span class="position-absolute badge-category bg-info text-white text-uppercase">Nội Bộ</span>
              </div>
              <div class="card-body p-4 article-body">
                <div class="d-flex align-items-center gap-2 mb-2 text-muted small meta-date">
                  <CalendarDays class="meta-icon" /> 18/05/2026
                </div>
                <h5 class="card-title fw-bold mb-2">MyPetClinic Đón Nhận Chứng Chỉ Y Khoa ISO 9001</h5>
                <p class="card-text small text-muted">
                  Đánh dấu mốc quan trọng trong việc chuẩn hóa toàn bộ quy trình chăm sóc nội trú, phẫu thuật ngoại khoa và quy trình khử trùng...
                </p>
              </div>
            </div>
          </div>

          <!-- Article 3 -->
          <div class="news-card-col">
            <div class="card border-0 glass-card h-100 overflow-hidden shadow-sm article-card">
              <div class="position-relative img-wrapper">
                <img src="https://images.unsplash.com/photo-1518717758536-85ae29035b6d?q=80&w=400&auto=format&fit=crop" class="card-img-top article-img" alt="News Image 3" />
                <span class="position-absolute badge-category bg-success text-white text-uppercase">Cộng Đồng</span>
              </div>
              <div class="card-body p-4 article-body">
                <div class="d-flex align-items-center gap-2 mb-2 text-muted small meta-date">
                  <CalendarDays class="meta-icon" /> 10/05/2026
                </div>
                <h5 class="card-title fw-bold mb-2">Ngày Hội Nhận Nuôi Thú Cưng Mồ Côi - Tìm Mái Ấm Yêu Thương</h5>
                <p class="card-text small text-muted">
                  Phối hợp với Trạm Cứu Hộ Động Vật, MyPetClinic hỗ trợ kiểm tra sức khỏe, tiêm phòng dại miễn phí cho các bé cưng được nhận nuôi...
                </p>
              </div>
            </div>
          </div>
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
import { ref } from 'vue';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
import { Newspaper, CalendarDays, MapPin } from '@lucide/vue';

const showBookingModal = ref(false);

const handleBookingSuccess = (msg: string) => {
  alert(msg);
};

const handleBookingError = (msg: string) => {
  alert(msg);
};
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
