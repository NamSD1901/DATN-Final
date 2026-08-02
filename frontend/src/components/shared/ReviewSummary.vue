<template>
  <div class="bg-white rounded-4 p-4 text-center border shadow-sm">
    <h6 class="fw-bold mb-4 text-dark fs-6 text-uppercase" style="letter-spacing: 0.5px;">Điểm trung bình</h6>
    
    <div class="d-flex align-items-end justify-content-center gap-2 mb-2">
      <span class="fw-bold lh-1 text-dark" style="font-size: 3.5rem; letter-spacing: -1px;">{{ safeStatistics.avgRating.toFixed(1) }}</span>
      <span class="fs-4 fw-bold pb-1 text-muted">/ 5</span>
    </div>
    
    <div class="d-flex justify-content-center gap-1 mb-2">
      <i v-for="i in 5" :key="i"
         class="bi"
         :class="i <= Math.round(safeStatistics.avgRating) ? 'bi-star-fill text-warning' : 'bi-star-fill text-black-50 opacity-25'"
         style="font-size: 1.2rem;">
      </i>
    </div>
    
    <p class="small fw-medium mb-4 text-muted">Dựa trên {{ safeStatistics.total }} đánh giá</p>
    
    <div class="d-flex flex-column gap-2 mt-3">
      <div v-for="i in 5" :key="i" class="d-flex align-items-center gap-3">
        <span class="text-end fw-bold text-dark" style="width: 45px; font-size: 0.8rem;">{{ 6 - i }} Sao</span>
        <div class="progress flex-grow-1 shadow-none" style="height: 8px; border-radius: 4px; background-color: #f1f5f9;">
          <div class="progress-bar bg-warning" role="progressbar" 
               :style="{ width: getPercentage(6 - i) + '%', transition: 'width 1s ease' }"></div>
        </div>
        <span class="text-start fw-medium text-muted" style="width: 35px; font-size: 0.85rem;">{{ safeStatistics.ratingDistribution[6 - i] || 0 }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { ReviewStatisticsDto } from '../../services/review.service';

const props = defineProps<{
  statistics: ReviewStatisticsDto | null
}>();

const safeStatistics = computed(() => {
  return props.statistics || {
    avgRating: 0,
    total: 0,
    ratingDistribution: { 1: 0, 2: 0, 3: 0, 4: 0, 5: 0 }
  };
});

const getPercentage = (stars: number) => {
  if (safeStatistics.value.total === 0) return 0;
  const count = safeStatistics.value.ratingDistribution[stars] || 0;
  return (count / safeStatistics.value.total) * 100;
};
</script>

<style scoped>
.fw-black {
  font-weight: 900;
}
.progress-bar {
  background: linear-gradient(90deg, var(--primary-gold) 0%, #fbbf24 100%);
}
</style>
