<template>
  <div class="card border-0 shadow-sm rounded-4 bg-white p-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <div>
        <h4 class="fw-bold text-dark mb-1">
          <i class="bi bi-star-fill text-warning me-2"></i>Quản lý Đánh giá
        </h4>
        <p class="text-muted small mb-0">Theo dõi và quản lý phản hồi từ khách hàng</p>
      </div>
    </div>

    <!-- Statistics Cards -->
    <div v-if="reviewStore.statistics" class="row g-4 mb-5">
      <!-- Total Reviews -->
      <div class="col-md-4">
        <div class="card border-0 rounded-4 p-4 h-100 shadow-sm" style="background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);">
          <div class="d-flex align-items-center">
            <div class="flex-shrink-0 bg-white rounded-circle d-flex align-items-center justify-content-center shadow-sm" style="width: 56px; height: 56px;">
              <i class="bi bi-chat-square-text-fill fs-3" style="color: #16a34a;"></i>
            </div>
            <div class="ms-3">
              <h6 class="text-success mb-1 fw-bold">Tổng đánh giá</h6>
              <h2 class="fw-bold text-dark mb-0">{{ reviewStore.statistics.total }}</h2>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Average Rating -->
      <div class="col-md-4">
        <div class="card border-0 rounded-4 p-4 h-100 shadow-sm" style="background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);">
          <div class="d-flex align-items-center">
            <div class="flex-shrink-0 bg-white rounded-circle d-flex align-items-center justify-content-center shadow-sm" style="width: 56px; height: 56px;">
              <i class="bi bi-star-fill fs-3 text-warning"></i>
            </div>
            <div class="ms-3">
              <h6 class="text-warning mb-1 fw-bold">Điểm trung bình</h6>
              <div class="d-flex align-items-baseline">
                <h2 class="fw-bold text-dark mb-0 me-1">{{ reviewStore.statistics.avgRating.toFixed(1) }}</h2>
                <span class="text-muted small">/ 5.0</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Rating Distribution (Progress Bars) -->
      <div class="col-md-4">
        <div class="card border-0 rounded-4 p-3 h-100 shadow-sm bg-light">
          <h6 class="text-muted mb-2 fw-bold fs-7 text-uppercase tracking-wider">Phân bổ điểm</h6>
          <div class="d-flex flex-column gap-1">
            <div v-for="i in 5" :key="i" class="d-flex align-items-center">
              <span class="text-muted small me-2" style="width: 35px;">{{ 6 - i }} <i class="bi bi-star-fill text-warning" style="font-size: 0.7rem;"></i></span>
              <div class="progress flex-grow-1" style="height: 6px;">
                <div class="progress-bar bg-warning" role="progressbar" 
                     :style="{ width: getPercentage(reviewStore.statistics.ratingDistribution[6 - i] || 0) + '%' }"></div>
              </div>
              <span class="text-muted small ms-2" style="width: 20px; text-align: right;">{{ reviewStore.statistics.ratingDistribution[6 - i] || 0 }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Data Table -->
    <div class="table-responsive rounded-4 border">
      <table class="table table-hover table-borderless align-middle mb-0">
        <thead class="table-light border-bottom">
          <tr>
            <th class="py-3 px-4 text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Khách hàng</th>
            <th class="py-3 text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Lịch hẹn</th>
            <th class="py-3 text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Đánh giá</th>
            <th class="py-3 text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Nội dung</th>
            <th class="py-3 text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Ngày tạo</th>
            <th class="py-3 text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Trạng thái</th>
            <th class="py-3 px-4 text-end text-muted fw-semibold text-uppercase" style="font-size: 0.75rem; letter-spacing: 0.5px;">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="reviewStore.loading">
            <td colspan="7" class="text-center py-5">
              <div class="spinner-border text-primary" role="status"></div>
              <p class="text-muted mt-2 mb-0">Đang tải dữ liệu...</p>
            </td>
          </tr>
          <tr v-else-if="reviewStore.adminReviews.length === 0">
            <td colspan="7" class="text-center py-5">
              <div class="text-muted">
                <i class="bi bi-inbox fs-1 d-block mb-3 opacity-50"></i>
                Chưa có đánh giá nào.
              </div>
            </td>
          </tr>
          <tr v-for="review in reviewStore.adminReviews" :key="review.id" 
              class="border-bottom transition-all" 
              :class="{'opacity-50 bg-light': review.deletedAt}">
            
            <!-- Khách hàng -->
            <td class="px-4 py-3">
              <div class="d-flex align-items-center">
                <div class="me-3 bg-primary bg-opacity-10 text-primary fw-bold d-flex align-items-center justify-content-center rounded-circle" style="width: 40px; height: 40px;">
                  {{ review.customerName.charAt(0).toUpperCase() }}
                </div>
                <div>
                  <h6 class="mb-0 fw-bold text-dark">{{ review.customerName }}</h6>
                </div>
              </div>
            </td>
            
            <!-- Lịch hẹn -->
            <td class="py-3">
              <span class="badge bg-light text-dark border mb-1">#{{ review.appointmentId }}</span>
              <div class="small text-muted text-truncate" style="max-width: 150px;">{{ review.serviceName }}</div>
            </td>
            
            <!-- Đánh giá -->
            <td class="py-3">
              <div class="d-flex text-warning">
                <i v-for="s in 5" :key="s" class="bi" :class="s <= review.rating ? 'bi-star-fill' : 'bi-star text-muted opacity-25'" style="font-size: 0.9rem; margin-right: 2px;"></i>
              </div>
            </td>
            
            <!-- Nội dung -->
            <td class="py-3">
              <div v-if="hasBadWords(review.comment)" class="mb-1">
                <span class="badge bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25" style="font-size: 0.65rem;">
                  <i class="bi bi-exclamation-triangle-fill me-1"></i>Từ ngữ nhạy cảm
                </span>
              </div>
              <div v-if="review.comment" 
                   @click="toggleComment(review.id)"
                   :class="{'text-truncate': !expandedComments.includes(review.id)}" 
                   style="max-width: 250px; cursor: pointer; transition: all 0.2s ease;"
                   title="Bấm để xem đầy đủ / thu gọn">
                {{ review.comment }}
              </div>
              <div v-else class="text-muted fst-italic small">Không có bình luận</div>
            </td>
            
            <!-- Ngày tạo -->
            <td class="py-3 text-muted small">
              {{ formatDate(review.createdAt) }}
            </td>
            
            <!-- Trạng thái -->
            <td class="py-3">
              <span v-if="review.deletedAt" class="badge rounded-pill bg-danger bg-opacity-10 text-danger border border-danger border-opacity-25 px-3 py-1">Đã ẩn</span>
              <span v-else class="badge rounded-pill bg-success bg-opacity-10 text-success border border-success border-opacity-25 px-3 py-1">Công khai</span>
            </td>
            
            <!-- Hành động -->
            <td class="px-4 py-3 text-end">
              <button v-if="!review.deletedAt" @click="softDelete(review.id)" class="btn btn-sm btn-light text-danger rounded-circle action-btn" title="Ẩn đánh giá">
                <i class="bi bi-eye-slash-fill"></i>
              </button>
              <button v-else @click="restore(review.id)" class="btn btn-sm btn-light text-success rounded-circle action-btn" title="Khôi phục">
                <i class="bi bi-arrow-counterclockwise"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    
    <!-- Pagination -->
    <div class="d-flex justify-content-between align-items-center mt-4" v-if="reviewStore.adminPagination.totalPages > 1">
      <span class="text-muted small">Hiển thị trang {{ reviewStore.adminPagination.page }} / {{ reviewStore.adminPagination.totalPages }}</span>
      <div class="d-flex gap-1">
        <button v-for="p in reviewStore.adminPagination.totalPages" :key="p"
                @click="loadPage(p)"
                :class="p === reviewStore.adminPagination.page ? 'btn-primary shadow-sm' : 'btn-light text-muted'"
                class="btn btn-sm rounded-3 fw-bold" style="width: 32px; height: 32px;">
          {{ p }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.action-btn {
  width: 32px;
  height: 32px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.action-btn:hover {
  background-color: var(--bs-gray-200) !important;
  transform: translateY(-2px);
}
.tracking-wider {
  letter-spacing: 0.05em;
}
.fs-7 {
  font-size: 0.85rem;
}
</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useReviewStore } from '../../stores/review.store';

const reviewStore = useReviewStore();
const expandedComments = ref<number[]>([]);

const toggleComment = (id: number) => {
  if (expandedComments.value.includes(id)) {
    expandedComments.value = expandedComments.value.filter(x => x !== id);
  } else {
    expandedComments.value.push(id);
  }
};

const badWordsList = ['cứt', 'vcl', 'địt', 'đụ', 'lồn', 'buồi', 'cặc', 'chó đẻ', 'đĩ', 'ngu', 'fuck', 'shit'];

const hasBadWords = (text: string | undefined | null) => {
  if (!text) return false;
  const lowerText = text.toLowerCase();
  return badWordsList.some(word => lowerText.includes(word));
};

onMounted(async () => {
  await reviewStore.fetchStatistics();
  await reviewStore.fetchAdminReviews(1);
});

const loadPage = async (page: number) => {
  await reviewStore.fetchAdminReviews(page);
};

const getPercentage = (count: number) => {
  if (!reviewStore.statistics || reviewStore.statistics.total === 0) return 0;
  return (count / reviewStore.statistics.total) * 100;
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN');
};

const softDelete = async (id: number) => {
  if (confirm('Bạn có chắc chắn muốn ẩn đánh giá này khỏi trang chủ?')) {
    await reviewStore.softDeleteReview(id);
    await reviewStore.fetchStatistics();
  }
};

const restore = async (id: number) => {
  if (confirm('Khôi phục hiển thị cho đánh giá này?')) {
    await reviewStore.restoreReview(id);
    await reviewStore.fetchStatistics();
  }
};
</script>
