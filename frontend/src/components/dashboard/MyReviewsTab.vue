<template>
  <div class="container-fluid p-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <h4 class="fw-bold text-dark mb-1">Đánh giá của tôi</h4>
        <p class="text-muted small mb-0">Quản lý các nhận xét và đánh giá dịch vụ của bạn</p>
      </div>
      <!-- Nút làm mới danh sách -->
      <button @click="loadMyReviews" class="btn btn-outline-secondary rounded-pill btn-sm fw-bold">
        <i class="bi bi-arrow-clockwise me-1"></i> Làm mới
      </button>
    </div>

    <!-- Trạng thái loading -->
    <div v-if="reviewStore.loading" class="text-center py-5">
      <div class="spinner-border text-warning" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2 text-muted">Đang tải đánh giá...</p>
    </div>

    <!-- Error state -->
    <div v-else-if="reviewStore.error" class="alert alert-danger rounded-4">
      {{ reviewStore.error }}
    </div>

    <!-- Danh sách đánh giá -->
    <div v-else>
      <div v-if="reviewStore.myReviews.length === 0" class="text-center py-5 bg-white rounded-4 shadow-sm border-0">
        <div class="mb-3">
          <i class="bi bi-star-half text-warning opacity-25" style="font-size: 4rem;"></i>
        </div>
        <h5 class="fw-bold text-dark">Chưa có đánh giá nào</h5>
        <p class="text-muted mb-4">Bạn chưa thực hiện đánh giá nào sau các cuộc hẹn.</p>
        <button @click="openAppointments" class="btn btn-warning text-white rounded-pill px-4 fw-bold shadow-sm">
          Xem lịch sử hẹn
        </button>
      </div>

      <div v-else class="row g-4">
        <!-- Review List -->
        <div class="col-12" v-for="review in reviewStore.myReviews" :key="review.id">
          <ReviewCard :review="review">
            <!-- Inject nút Edit vào slot actions -->
            <template #actions>
              <button @click="openEditModal(review)" class="btn btn-sm btn-light border rounded-pill shadow-sm hover-text-warning transition-colors" title="Chỉnh sửa đánh giá">
                <i class="bi bi-pencil-square"></i> Chỉnh sửa
              </button>
            </template>
          </ReviewCard>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="reviewStore.myPagination.totalPages > 1" class="d-flex justify-content-center mt-5">
        <nav aria-label="Page navigation">
          <ul class="pagination pagination-sm gap-2">
            <li class="page-item" :class="{ disabled: reviewStore.myPagination.page === 1 }">
              <a class="page-link rounded-circle border-0 shadow-sm" href="#" @click.prevent="changePage(reviewStore.myPagination.page - 1)">
                <i class="bi bi-chevron-left"></i>
              </a>
            </li>
            <li v-for="p in reviewStore.myPagination.totalPages" :key="p" class="page-item" :class="{ active: p === reviewStore.myPagination.page }">
              <a class="page-link rounded-circle border-0 shadow-sm" href="#" @click.prevent="changePage(p)">
                {{ p }}
              </a>
            </li>
            <li class="page-item" :class="{ disabled: reviewStore.myPagination.page === reviewStore.myPagination.totalPages }">
              <a class="page-link rounded-circle border-0 shadow-sm" href="#" @click.prevent="changePage(reviewStore.myPagination.page + 1)">
                <i class="bi bi-chevron-right"></i>
              </a>
            </li>
          </ul>
        </nav>
      </div>
    </div>

    <!-- Review Modal for Editing -->
    <ReviewModal
      :is-open="isModalOpen"
      :is-edit="true"
      :initial-data="selectedReview"
      @close="closeModal"
      @submit="handleUpdateReview"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useReviewStore } from '../../stores/review.store';
import ReviewCard from '../shared/ReviewCard.vue';
import ReviewModal from '../shared/ReviewModal.vue';
import type { ReviewDto } from '../../services/review.service';
import { useRouter } from 'vue-router';

const reviewStore = useReviewStore();
const router = useRouter();

const isModalOpen = ref(false);
const selectedReview = ref<any>(null);

const loadMyReviews = async (page: number = 1) => {
  await reviewStore.fetchMyReviews(page);
};

const changePage = (page: number) => {
  if (page < 1 || page > reviewStore.myPagination.totalPages) return;
  loadMyReviews(page);
  window.scrollTo({ top: 0, behavior: 'smooth' });
};

const openEditModal = (review: ReviewDto) => {
  selectedReview.value = {
    reviewId: review.id,
    appointmentId: review.appointmentId,
    rating: review.rating,
    comment: review.comment,
    imageUrls: review.imageUrls
  };
  isModalOpen.value = true;
};

const closeModal = () => {
  isModalOpen.value = false;
  selectedReview.value = null;
};

const handleUpdateReview = async (data: any) => {
  if (!data.reviewId) return;
  try {
    await reviewStore.updateMyReview(data.reviewId, {
      rating: data.rating,
      comment: data.comment,
      imageUrls: data.imageUrls
    });
    closeModal();
    // Toast notification can be added here if needed
  } catch (error) {
    console.error("Cập nhật đánh giá thất bại:", error);
  }
};

const openAppointments = () => {
  // Option 1: Emit an event up to change the activeTab to 'my-appointments'
  // For simplicity, we just use DOM to trigger click on sidebar, or emit if we expose an emit.
  // Actually, since Dashboard manages activeTab, it's better to just emit. But we don't have emit from Tab.
  // So we use router or simple state. Wait, the dashboard components don't have direct access to Dashboard.vue's activeTab unless they emit.
  // Let's emit an event `switch-tab`
  emit('switch-tab', 'my-appointments');
};

const emit = defineEmits(['switch-tab']);

onMounted(() => {
  loadMyReviews(1);
});
</script>

<style scoped>
.page-link {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #495057;
  transition: all 0.2s;
}

.page-item.active .page-link {
  background-color: var(--primary-color, #2563EB);
  color: white;
}

.page-item:not(.active) .page-link:hover {
  background-color: #f8f9fa;
  color: var(--primary-color, #2563EB);
}
</style>
