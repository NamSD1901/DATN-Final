<template>
  <div class="min-vh-100 position-relative font-sans d-flex flex-column" style="background-color: #f8fafc;">
    <Header />
    <div class="flex-grow-1 py-5 position-relative">
      
      <div class="container position-relative z-1 py-4">
        
        <!-- Header -->
        <div class="text-center mb-5 reveal active">
          <h1 class="display-5 fw-bold mb-3 text-dark tracking-tight" style="letter-spacing: -1px;">
            Đánh Giá Từ Khách Hàng
          </h1>
          <p class="lead mx-auto" style="max-width: 650px; color: var(--text-muted); font-size: 1.1rem;">
            Cảm nhận thực tế và chân thực nhất từ những khách hàng đã trải nghiệm dịch vụ tại MyPetClinic.
          </p>
        </div>
        
        <div class="row g-4 align-items-start reveal active delay-100">
        
        <!-- Left Sidebar: Filters (Sticky) -->
        <ReviewFilters v-model="filters" @change="applyFilters" />

        <!-- Middle Content: Reviews List -->
        <main class="col-lg-6 d-flex flex-column gap-4">
          <div v-if="reviewStore.loading" class="d-flex justify-content-center py-5">
            <div class="spinner-border" style="width: 3.5rem; height: 3.5rem; color: var(--primary-gold);" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
          </div>
          
          <div v-else-if="reviewStore.publicReviews.length === 0" class="text-center py-5 glass-card rounded-4 border-0">
            <i class="bi bi-inbox-fill mb-3" style="font-size: 4.5rem; color: var(--primary-light);"></i>
            <h4 class="fw-bold mb-2" style="color: var(--text-dark);">Không tìm thấy đánh giá nào</h4>
            <p class="mb-4" style="color: var(--text-muted);">Thử thay đổi hoặc xóa bộ lọc để xem thêm kết quả.</p>
            <button @click="clearFilters" class="btn-premium px-5 py-2 rounded-pill shadow-sm">
              Xóa bộ lọc
            </button>
          </div>

          <div v-else class="d-flex flex-column gap-4">
            <ReviewCard v-for="review in reviewStore.publicReviews" :key="review.id" :review="review" />
          </div>

          <!-- Pagination -->
          <div class="d-flex justify-content-center mt-5 gap-2" v-if="reviewStore.publicPagination.totalPages > 1">
            <button v-for="p in reviewStore.publicPagination.totalPages" :key="p"
                    @click="loadPage(p)"
                    class="btn btn-sm d-flex align-items-center justify-content-center fw-bold transition-all shadow-sm"
                    :style="p === reviewStore.publicPagination.page ? 'background: linear-gradient(135deg, var(--primary-gold), #f59e0b); color: white; border: none;' : 'background-color: white; color: var(--text-muted); border: 1px solid var(--border-color);'"
                    style="width: 45px; height: 45px; border-radius: 14px; font-size: 1.1rem;">
              {{ p }}
            </button>
          </div>
        </main>

        <!-- Right Sidebar: Statistics -->
        <aside class="col-lg-3 d-flex flex-column gap-4">
          <ReviewSummary :statistics="reviewStore.statistics" />
          
          <!-- Banner or Info card -->
          <div class="glass-card rounded-4 p-4 border-0 position-relative overflow-hidden">
            <div class="position-absolute" style="top: -20px; left: -20px; font-size: 8rem; color: rgba(245, 158, 11, 0.05); z-index: 0;">
              <i class="bi bi-chat-heart-fill"></i>
            </div>
            <div class="position-relative z-1">
              <h6 class="fw-bold mb-3" style="color: var(--primary-dark); font-size: 1.1rem;">Đánh giá của bạn rất quan trọng!</h6>
              <p class="small mb-0" style="color: var(--text-muted); line-height: 1.7;">Chúng tôi luôn lắng nghe để cải thiện chất lượng dịch vụ mỗi ngày. Hãy để lại đánh giá sau mỗi lần đến phòng khám nhé!</p>
            </div>
          </div>
        </aside>

      </div>
    </div>
    </div>
    <Footer />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useReviewStore } from '../stores/review.store';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import ReviewCard from '../components/shared/ReviewCard.vue';
import ReviewFilters from '../components/shared/ReviewFilters.vue';
import ReviewSummary from '../components/shared/ReviewSummary.vue';

const reviewStore = useReviewStore();

const filters = ref({
  rating: null as number | null,
  petType: '',
  sortBy: 'date_desc',
  hasImages: false
});

onMounted(async () => {
  await reviewStore.fetchStatistics();
  applyFilters();
});

const applyFilters = async () => {
  await reviewStore.fetchPublicReviews(
    1, 
    filters.value.sortBy, 
    filters.value.rating || undefined, 
    filters.value.petType || undefined, 
    undefined, // serviceId
    undefined, // doctorId
    filters.value.hasImages ? true : undefined
  );
};

const clearFilters = async () => {
  filters.value = {
    rating: null,
    petType: '',
    sortBy: 'date_desc',
    hasImages: false
  };
  await applyFilters();
};

const loadPage = async (page: number) => {
  await reviewStore.fetchPublicReviews(
    page, 
    filters.value.sortBy, 
    filters.value.rating || undefined,
    filters.value.petType || undefined,
    undefined,
    undefined,
    filters.value.hasImages ? true : undefined
  );
  window.scrollTo({ top: 0, behavior: 'smooth' });
};
</script>

<style scoped>
.pointer-events-none {
  pointer-events: none;
}
.z-1 {
  z-index: 1;
}
.tracking-tight {
  letter-spacing: -0.025em;
}
</style>
