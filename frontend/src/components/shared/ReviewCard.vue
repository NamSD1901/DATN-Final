<template>
  <div class="review-card glassmorphism p-6 rounded-2xl flex flex-col gap-4 relative transition-all duration-300 hover:shadow-lg hover:-translate-y-1">
    <div class="flex items-center gap-4">
      <img :src="review.customerAvatarUrl || 'https://ui-avatars.com/api/?name=' + review.customerName + '&background=random'" 
           alt="Avatar" 
           class="w-12 h-12 rounded-full object-cover border-2 border-[var(--primary-color)]" />
      <div class="flex-1">
        <h4 class="font-bold text-gray-800 text-lg">{{ review.customerName }}</h4>
        <p class="text-xs text-gray-500">{{ formatDate(review.createdAt) }} <span v-if="review.serviceName">• {{ review.serviceName }}</span></p>
      </div>
      <div class="flex gap-1">
        <svg v-for="i in 5" :key="i"
             :class="i <= review.rating ? 'text-yellow-400' : 'text-gray-300'"
             class="w-5 h-5 fill-current" viewBox="0 0 20 20">
          <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
        </svg>
      </div>
    </div>
    
    <div class="text-gray-700 italic border-l-4 border-[var(--primary-color)] pl-4">
      <p v-if="review.comment">"{{ review.comment }}"</p>
      <p v-else class="text-gray-400">Người dùng không để lại bình luận.</p>
    </div>

    <!-- Admin Actions or Customer Actions can be slotted here -->
    <div v-if="$slots.actions" class="absolute top-4 right-4">
      <slot name="actions"></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ReviewDto } from '../../services/review.service';

const props = defineProps<{
  review: ReviewDto
}>();

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric'
  });
};
</script>

<style scoped>
.glassmorphism {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}
</style>
