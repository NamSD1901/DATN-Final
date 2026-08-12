<template>
  <div class="review-card bg-white p-4 p-md-4 d-flex flex-column gap-3 position-relative hover-lift mb-3 rounded-4 border shadow-sm">
    <!-- Header: User info & Rating -->
    <div class="d-flex align-items-start justify-content-between">
      <div class="d-flex align-items-center gap-3">
        <img :src="review.customerAvatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(review.customerName)}&background=f59e0b&color=fff&bold=true`" 
             alt="Avatar" 
             class="rounded-circle object-fit-cover shadow-sm border" 
             style="width: 50px; height: 50px; border-color: #f3f4f6 !important;" 
             @error="handleImageError" />
        <div>
          <div class="d-flex align-items-center gap-2 mb-1">
            <h6 class="mb-0 fw-bold" style="color: var(--text-dark);">{{ review.customerName }}</h6>
            <!-- Badge Verified Visit -->
            <span v-if="review.isVerified" class="badge rounded-pill fw-semibold border d-flex align-items-center gap-1" style="background-color: #f0fdf4; color: #166534; border-color: #bbf7d0;">
              <i class="bi bi-patch-check-fill" style="color: #16a34a;"></i> Đã trải nghiệm
            </span>
          </div>
          <small class="text-muted">Đánh giá vào {{ formatDate(review.createdAt) }}</small>
        </div>
      </div>
      <!-- Rating Stars -->
      <div class="d-flex gap-1 align-items-center px-2 py-1 rounded" style="background-color: #fffbeb;">
        <span class="fw-bold me-1" style="color: var(--primary-dark); font-size: 0.95rem;">{{ review.rating }}.0</span>
        <i v-for="i in 5" :key="i"
           class="bi"
           :class="i <= review.rating ? 'bi-star-fill text-warning' : 'bi-star-fill text-black-50 opacity-25'"
           style="font-size: 0.85rem;">
        </i>
      </div>
    </div>
    
    <!-- Meta Info: Removed as requested -->
    
    <!-- Review Text Content -->
    <div class="mt-1 position-relative">
      <p v-if="review.comment" :class="{ 'text-truncate-multi': !isExpanded }" class="mb-0 fs-6 text-dark" style="white-space: pre-line; line-height: 1.6;">
        "{{ review.comment }}"
      </p>
      <p v-else class="fst-italic mb-0 text-muted">Người dùng không để lại bình luận.</p>
      
      <button v-if="review.comment && review.comment.length > 200" 
              @click="isExpanded = !isExpanded" 
              class="btn btn-link p-0 text-decoration-none fw-bold mt-2"
              style="color: var(--primary-dark);">
        {{ isExpanded ? 'Thu gọn' : 'Đọc thêm' }} <i :class="isExpanded ? 'bi bi-chevron-up' : 'bi bi-chevron-down'" style="font-size: 0.75rem;"></i>
      </button>
    </div>

    <!-- Review Images -->
    <div v-if="parsedImages.length > 0" class="row g-2 mt-2">
      <div v-for="(img, idx) in parsedImages.slice(0, 4)" :key="idx" 
           class="col-6 col-md-3 position-relative cursor-pointer group"
           @click="openGallery(idx)">
        <div class="ratio ratio-1x1 rounded-3 overflow-hidden border">
          <img :src="img" alt="Review Image" class="w-100 h-100 object-fit-cover transition-transform img-hover-zoom" @error="handleImageError" />
        </div>
        <!-- More images overlay -->
        <div v-if="idx === 3 && parsedImages.length > 4" class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex align-items-center justify-content-center text-white fw-bold fs-5 rounded-3" style="margin: 0.25rem; width: calc(100% - 0.5rem); height: calc(100% - 0.5rem);">
          +{{ parsedImages.length - 4 }}
        </div>
      </div>
    </div>

    <!-- Clinic Reply -->
    <div v-if="review.clinicReply" class="mt-3 p-3 rounded-3 position-relative" style="background-color: #f8fafc; border-left: 4px solid var(--primary-gold);">
      <div class="d-flex align-items-center gap-2 mb-2">
        <i class="bi bi-reply-fill" style="color: var(--primary-gold); font-size: 1.2rem;"></i>
        <strong class="text-dark">Phản hồi từ phòng khám</strong>
      </div>
      <p class="mb-0 text-dark" style="line-height: 1.6; font-size: 0.95rem;">{{ review.clinicReply }}</p>
      <p v-if="review.repliedAt" class="small text-muted mb-0 mt-2">{{ formatDate(review.repliedAt) }}</p>
    </div>

    <!-- Helpful / Like Actions -->
    <div class="d-flex align-items-center gap-3 mt-3 pt-3" style="border-top: 1px solid #f1f5f9;">
      <button 
        @click="handleLike" 
        class="btn btn-sm rounded-pill px-3 py-1 d-flex align-items-center gap-2 fw-medium transition-all"
        :class="hasLiked ? 'btn-primary text-white border-primary' : 'btn-outline-secondary hover-text-primary'">
        <i class="bi d-inline-block" :class="[hasLiked ? 'bi-hand-thumbs-up-fill' : 'bi-hand-thumbs-up', { 'liked-animation': isAnimating }]"></i>
        Hữu ích ({{ localHelpfulCount }})
      </button>
    </div>

    <!-- Admin Actions or Customer Actions can be slotted here -->
    <div v-if="$slots.actions" class="position-absolute top-0 end-0 p-4">
      <slot name="actions"></slot>
    </div>

    <!-- Fullscreen Gallery Modal -->
    <div v-if="isGalleryOpen" class="position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-75 d-flex align-items-center justify-content-center" style="z-index: 9999; backdrop-filter: blur(5px);" @click.self="closeGallery">
      <button @click="closeGallery" class="btn btn-link text-white position-absolute top-0 end-0 p-4 fs-1">
        <i class="bi bi-x"></i>
      </button>
      
      <button v-if="parsedImages.length > 1" @click.stop="prevImage" class="btn btn-light rounded-circle position-absolute start-0 ms-4 p-3 shadow-lg hover-glow">
        <i class="bi bi-chevron-left fs-4"></i>
      </button>
      
      <img :src="parsedImages[currentImageIdx]" class="img-fluid rounded-4 shadow-lg" style="max-height: 90vh; max-width: 90vw; object-fit: contain;" />
      
      <button v-if="parsedImages.length > 1" @click.stop="nextImage" class="btn btn-light rounded-circle position-absolute end-0 me-4 p-3 shadow-lg hover-glow">
        <i class="bi bi-chevron-right fs-4"></i>
      </button>

      <div class="position-absolute bottom-0 mb-5 bg-white text-dark fw-bold px-4 py-2 rounded-pill shadow-lg">
        {{ currentImageIdx + 1 }} / {{ parsedImages.length }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import Swal from 'sweetalert2';
import type { ReviewDto } from '../../services/review.service';
import ReviewService from '../../services/review.service';

const props = defineProps<{
  review: ReviewDto
}>();

const router = useRouter();
const isExpanded = ref(false);
const hasLiked = ref(false);
const isAnimating = ref(false);
const localHelpfulCount = ref(props.review.helpfulCount || 0);

watch(() => props.review.helpfulCount, (newVal) => {
  localHelpfulCount.value = newVal || 0;
});

onMounted(() => {
  const likedReviews = JSON.parse(localStorage.getItem('likedReviews') || '[]');
  if (likedReviews.includes(props.review.id)) {
    hasLiked.value = true;
  }
});

const handleLike = async () => {
  try {
    isAnimating.value = true;
    
    if (hasLiked.value) {
      await ReviewService.unmarkHelpful(props.review.id);
      hasLiked.value = false;
      if (localHelpfulCount.value > 0) localHelpfulCount.value--;
      
      const likedReviews = JSON.parse(localStorage.getItem('likedReviews') || '[]');
      const updatedReviews = likedReviews.filter((id: number) => id !== props.review.id);
      localStorage.setItem('likedReviews', JSON.stringify(updatedReviews));
    } else {
      await ReviewService.markHelpful(props.review.id);
      hasLiked.value = true;
      localHelpfulCount.value++;
      
      const likedReviews = JSON.parse(localStorage.getItem('likedReviews') || '[]');
      if (!likedReviews.includes(props.review.id)) {
        likedReviews.push(props.review.id);
        localStorage.setItem('likedReviews', JSON.stringify(likedReviews));
      }
    }
    
    setTimeout(() => {
      isAnimating.value = false;
    }, 400);

  } catch (error: any) {
    if (error.response && (error.response.status === 401 || error.response.status === 403)) {
      Swal.fire({
        title: 'Chưa đăng nhập!',
        text: 'Vui lòng đăng nhập với tài khoản khách hàng để thả tim đánh giá này.',
        icon: 'info',
        iconColor: '#f59e0b',
        showCancelButton: true,
        confirmButtonText: 'Đăng nhập ngay',
        cancelButtonText: 'Để sau',
        confirmButtonColor: '#f59e0b',
        cancelButtonColor: '#f3f4f6',
        background: '#ffffff',
        customClass: {
          popup: 'rounded-4 shadow-lg border-0',
          title: 'fw-bold text-dark fs-4 mb-2',
          htmlContainer: 'text-muted mb-4',
          confirmButton: 'btn btn-warning rounded-pill px-4 fw-bold shadow-sm me-2',
          cancelButton: 'btn btn-light rounded-pill px-4 fw-bold shadow-sm border text-muted'
        },
        buttonsStyling: false
      }).then((result) => {
        if (result.isConfirmed) {
          router.push('/login');
        }
      });
    } else {
      console.error('Failed to mark helpful', error);
      Swal.fire({
        title: 'Lỗi',
        text: error.response?.data?.message || 'Có lỗi xảy ra, vui lòng thử lại sau.',
        icon: 'error'
      });
    }
  } finally {
    isAnimating.value = false;
  }
};

const parsedImages = computed<string[]>(() => {
  if (!props.review.imageUrls) return [];
  try {
    return JSON.parse(props.review.imageUrls) as string[];
  } catch (e) {
    return props.review.imageUrls.split(',').map((url: string) => url.trim()).filter((url: string) => url.length > 0);
  }
});

const isGalleryOpen = ref(false);
const currentImageIdx = ref(0);

const handleImageError = (e: Event) => {
  const target = e.target as HTMLImageElement;
  target.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(props.review.customerName)}&background=f59e0b&color=fff&bold=true`;
};

const openGallery = (index: number) => {
  currentImageIdx.value = index;
  isGalleryOpen.value = true;
  document.body.style.overflow = 'hidden';
};

const closeGallery = () => {
  isGalleryOpen.value = false;
  document.body.style.overflow = '';
};

const nextImage = () => {
  if (currentImageIdx.value < parsedImages.value.length - 1) {
    currentImageIdx.value++;
  } else {
    currentImageIdx.value = 0;
  }
};

const prevImage = () => {
  if (currentImageIdx.value > 0) {
    currentImageIdx.value--;
  } else {
    currentImageIdx.value = parsedImages.value.length - 1;
  }
};

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric'
  });
};
</script>

<style scoped>
@keyframes like-pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.5); }
  100% { transform: scale(1); }
}
.liked-animation {
  animation: like-pop 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.hover-lift {
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.text-truncate-multi {
  display: -webkit-box;
  -webkit-line-clamp: 4;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.cursor-pointer {
  cursor: pointer;
}
.img-hover-zoom {
  transition: transform 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}
.group:hover .img-hover-zoom {
  transform: scale(1.08);
}
.hover-text-primary:hover {
  color: var(--primary-dark) !important;
  border-color: var(--primary-gold) !important;
}
</style>
