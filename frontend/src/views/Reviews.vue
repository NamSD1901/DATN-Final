<template>
  <div class="min-h-screen bg-gray-50 py-12 relative overflow-hidden">
    <!-- Background Design -->
    <div class="absolute inset-0 z-0">
      <div class="absolute -top-40 -right-40 w-96 h-96 bg-[var(--primary-color)] rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob"></div>
      <div class="absolute top-40 -left-40 w-96 h-96 bg-purple-300 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>
    </div>

    <div class="container mx-auto px-4 relative z-10">
      <div class="text-center mb-12">
        <h1 class="text-4xl md:text-5xl font-bold text-gray-900 mb-4 bg-clip-text text-transparent bg-gradient-to-r from-[var(--primary-color)] to-purple-600">Đánh Giá Từ Khách Hàng</h1>
        <p class="text-lg text-gray-600 max-w-2xl mx-auto">Cảm nhận và phản hồi từ những khách hàng đã sử dụng dịch vụ tại MyPetClinic. Sự hài lòng của bạn là niềm vinh hạnh của chúng tôi.</p>
      </div>

      <!-- Thống kê tổng quan -->
      <div v-if="reviewStore.statistics" class="max-w-4xl mx-auto mb-16 glassmorphism rounded-3xl p-8 flex flex-col md:flex-row items-center gap-12">
        <div class="flex flex-col items-center justify-center text-center">
          <span class="text-6xl font-black text-gray-900">{{ reviewStore.statistics.avgRating.toFixed(1) }}</span>
          <div class="flex gap-1 my-3">
            <svg v-for="i in 5" :key="i"
                 :class="i <= Math.round(reviewStore.statistics.avgRating) ? 'text-yellow-400' : 'text-gray-300'"
                 class="w-6 h-6 fill-current" viewBox="0 0 20 20">
              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
            </svg>
          </div>
          <span class="text-sm text-gray-500">Dựa trên {{ reviewStore.statistics.total }} đánh giá</span>
        </div>

        <div class="flex-1 w-full flex flex-col gap-2">
          <div v-for="i in 5" :key="i" class="flex items-center gap-3">
            <span class="w-12 text-sm text-gray-600 font-medium">{{ 6 - i }} Sao</span>
            <div class="flex-1 h-3 bg-gray-200 rounded-full overflow-hidden">
              <div class="h-full bg-yellow-400 rounded-full" 
                   :style="{ width: getPercentage(6 - i) + '%' }"></div>
            </div>
            <span class="w-8 text-right text-xs text-gray-500">{{ reviewStore.statistics.ratingDistribution[6 - i] || 0 }}</span>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="flex justify-between items-center mb-8 max-w-6xl mx-auto">
        <div class="flex gap-4">
          <button @click="setFilter(null)" :class="!selectedRating ? 'bg-[var(--primary-color)] text-white' : 'bg-white text-gray-700'" class="px-4 py-2 rounded-xl shadow-sm transition-colors font-medium hover:opacity-90">Tất cả</button>
          <button v-for="i in [5,4,3,2,1]" :key="i" 
                  @click="setFilter(i)" 
                  :class="selectedRating === i ? 'bg-[var(--primary-color)] text-white' : 'bg-white text-gray-700'"
                  class="px-4 py-2 rounded-xl shadow-sm transition-colors font-medium hover:opacity-90 flex items-center gap-1">
            {{ i }} <svg class="w-4 h-4 text-yellow-400 fill-current" viewBox="0 0 20 20"><path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" /></svg>
          </button>
        </div>
      </div>

      <!-- Reviews Grid -->
      <div v-if="reviewStore.loading" class="flex justify-center py-20">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-[var(--primary-color)]"></div>
      </div>
      
      <div v-else-if="reviewStore.publicReviews.length === 0" class="text-center py-20">
        <p class="text-gray-500 text-lg">Chưa có đánh giá nào.</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-6xl mx-auto">
        <ReviewCard v-for="review in reviewStore.publicReviews" :key="review.id" :review="review" />
      </div>

      <!-- Pagination -->
      <div class="flex justify-center mt-12 gap-2" v-if="reviewStore.publicPagination.totalPages > 1">
        <button v-for="p in reviewStore.publicPagination.totalPages" :key="p"
                @click="loadPage(p)"
                :class="p === reviewStore.publicPagination.page ? 'bg-[var(--primary-color)] text-white' : 'bg-white text-gray-700 hover:bg-gray-100'"
                class="w-10 h-10 rounded-xl shadow-sm flex items-center justify-center font-bold transition-colors">
          {{ p }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useReviewStore } from '../stores/review.store';
import ReviewCard from '../components/shared/ReviewCard.vue';

const reviewStore = useReviewStore();
const selectedRating = ref<number | null>(null);

onMounted(async () => {
  await reviewStore.fetchStatistics();
  await reviewStore.fetchPublicReviews(1, 'date_desc');
});

const getPercentage = (stars: number) => {
  if (!reviewStore.statistics || reviewStore.statistics.total === 0) return 0;
  const count = reviewStore.statistics.ratingDistribution[stars] || 0;
  return (count / reviewStore.statistics.total) * 100;
};

const setFilter = async (rating: number | null) => {
  selectedRating.value = rating;
  await reviewStore.fetchPublicReviews(1, 'date_desc', rating || undefined);
};

const loadPage = async (page: number) => {
  await reviewStore.fetchPublicReviews(page, 'date_desc', selectedRating.value || undefined);
};
</script>

<style scoped>
.glassmorphism {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
}
</style>
