<template>
  <div class="page-wrapper">
    <Header @open-booking="showBookingModal = true" />

    <div v-if="loading" class="text-center py-5" style="min-height: 50vh;">
      <div class="spinner-border text-warning mt-5" role="status"></div>
      <p class="text-muted mt-2 small">Đang tải bài viết...</p>
    </div>

    <div v-else-if="error" class="text-center py-5 text-danger" style="min-height: 50vh;">
      <i class="bi bi-exclamation-circle fs-1"></i>
      <p class="mt-2">{{ error }}</p>
      <router-link to="/news" class="btn btn-outline-secondary mt-3">Quay lại Tin Tức</router-link>
    </div>

    <template v-else-if="post">
      <!-- SEO Metadata Head Update conceptually (Vue Meta / useHead if Nuxt) -->
      
      <!-- Article Header -->
      <section class="py-5 bg-light position-relative">
        <div class="container py-4 text-center">
          <div class="mb-3">
            <span class="badge bg-warning text-dark px-3 py-2 rounded-pill fw-bold text-uppercase me-2 shadow-sm">
              <i class="bi bi-folder2-open me-1"></i> {{ post.categoryName || 'Cẩm Nang' }}
            </span>
            <span v-for="tag in post.tags" :key="tag" class="badge bg-white text-secondary border px-3 py-2 rounded-pill text-uppercase me-2 shadow-sm">
              #{{ tag }}
            </span>
          </div>
          <h1 class="display-5 fw-bold mb-4 text-dark max-w-3xl mx-auto" style="line-height: 1.3;">{{ post.title }}</h1>
          
          <div class="d-flex justify-content-center align-items-center gap-4 text-muted small">
            <span class="d-flex align-items-center gap-1"><i class="bi bi-person-circle fs-5"></i> {{ post.authorName }}</span>
            <span class="d-flex align-items-center gap-1"><i class="bi bi-calendar3 fs-5"></i> {{ formatDate(post.publishedAt || post.createdAt) }}</span>
            <span class="d-flex align-items-center gap-1"><i class="bi bi-eye fs-5"></i> {{ post.viewCount || 0 }} lượt xem</span>
          </div>
        </div>
      </section>

      <!-- Featured Image -->
      <section v-if="post.thumbnail" class="container mt-n5 position-relative" style="z-index: 10;">
        <img :src="post.thumbnail" :alt="post.title" class="img-fluid rounded-4 shadow w-100 object-fit-cover" style="max-height: 500px;" />
      </section>

      <!-- Article Content -->
      <section class="py-5 bg-white">
        <div class="container max-w-3xl mx-auto article-content-wrapper">
          <p v-if="post.summary" class="lead fw-bold text-muted mb-5 border-start border-4 border-warning ps-4 py-2 bg-light rounded-end">
            {{ post.summary }}
          </p>

          <div class="article-content text-dark" v-html="formattedContent"></div>

          <!-- Share actions -->
          <div class="d-flex justify-content-between align-items-center mt-5 pt-4 border-top">
            <div class="fw-bold text-dark">Chia sẻ bài viết này:</div>
            <div class="d-flex gap-2">
              <button class="btn btn-light rounded-circle shadow-sm text-primary"><i class="bi bi-facebook"></i></button>
              <button class="btn btn-light rounded-circle shadow-sm text-info"><i class="bi bi-twitter"></i></button>
              <button class="btn btn-light rounded-circle shadow-sm text-success" @click="copyLink"><i class="bi bi-link-45deg"></i></button>
            </div>
          </div>
        </div>
      </section>

      <!-- Related Posts -->
      <section v-if="relatedPosts.length > 0" class="py-5 bg-light">
        <div class="container">
          <h3 class="fw-bold mb-4 text-dark text-center">Bài Viết Liên Quan</h3>
          <div class="row g-4 justify-content-center">
            <div v-for="related in relatedPosts" :key="related.id" class="col-md-4">
              <router-link :to="`/news/${related.slug}`" class="text-decoration-none">
                <div class="card border-0 shadow-sm h-100 rounded-4 overflow-hidden article-card bg-white">
                  <div class="img-wrapper">
                    <img :src="related.thumbnail || 'https://images.unsplash.com/photo-1548199973-03cce0bbc87b?q=80&w=400&auto=format&fit=crop'" class="card-img-top article-img" :alt="related.title" />
                  </div>
                  <div class="card-body p-4">
                    <div class="text-warning small fw-bold mb-2">{{ related.categoryName || 'Cẩm nang' }}</div>
                    <h5 class="card-title fw-bold text-dark mb-0 line-clamp-2">{{ related.title }}</h5>
                  </div>
                </div>
              </router-link>
            </div>
          </div>
        </div>
      </section>
    </template>

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
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { usePostStore } from '../stores/post.store';
import Header from '../components/layout/Header.vue';
import Footer from '../components/layout/Footer.vue';
import BookingModal from '../components/shared/BookingModal.vue';
// Note: Consider importing marked or markdown-it if content is markdown
// For now, we will assume it might be raw text or HTML and render it.
// If it's pure markdown without a parser, we just use white-space: pre-line or simple replace.

const route = useRoute();
const postStore = usePostStore();

const showBookingModal = ref(false);
const post = ref<any>(null);
const relatedPosts = ref<any[]>([]);
const loading = ref(true);
const error = ref('');

const fetchPostDetail = async () => {
  loading.value = true;
  error.value = '';
  try {
    const slug = route.params.slug as string;
    post.value = await postStore.fetchPostBySlug(slug);
    
    // Set title for SEO
    if (post.value) {
      document.title = (post.value.metaTitle || post.value.title) + ' - MyPetClinic';
      
      // Increment view asynchronously
      postStore.incrementView(post.value.id);
      
      // Fetch related
      try {
        await postStore.fetchPublicPosts(1, 4, undefined, post.value.categorySlug);
        relatedPosts.value = postStore.posts.filter((p: any) => p.id !== post.value.id).slice(0, 3);
      } catch (err) {
        // Ignore related error
      }
    }
  } catch (err: any) {
    error.value = 'Không tìm thấy bài viết hoặc bài viết đã bị gỡ.';
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateStr: string) => {
  if (!dateStr) return '';
  return new Date(dateStr).toLocaleDateString('vi-VN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};

const formattedContent = computed(() => {
  if (!post.value || !post.value.content) return '';
  // Convert newlines to <br> if it's plain text. If HTML, this might be redundant but safe if carefully used.
  // Ideally, use a markdown parser here if the content is markdown.
  // For simplicity, assuming text with newlines.
  let content = post.value.content;
  // Simple check: if it doesn't look like HTML, add <br>
  if (!content.includes('<p>') && !content.includes('<div>')) {
    content = content.replace(/\n/g, '<br>');
  }
  return content;
});

const copyLink = () => {
  navigator.clipboard.writeText(window.location.href);
  alert('Đã copy đường dẫn bài viết!');
};

const handleBookingSuccess = (msg: string) => alert(msg);
const handleBookingError = (msg: string) => alert(msg);

// Re-fetch when route changes (clicking related post)
watch(() => route.params.slug, () => {
  if (route.name === 'PostDetail') {
    window.scrollTo(0, 0);
    fetchPostDetail();
  }
});

onMounted(() => {
  window.scrollTo(0, 0);
  fetchPostDetail();
});
</script>

<style scoped>
.page-wrapper {
  background-color: var(--bg-light);
  color: var(--text-dark);
}

.max-w-3xl {
  max-width: 800px;
}

.article-content {
  font-size: 1.1rem;
  line-height: 1.8;
  color: #333;
}

.article-content :deep(h2) {
  font-weight: 700;
  margin-top: 2rem;
  margin-bottom: 1rem;
}

.article-content :deep(p) {
  margin-bottom: 1.5rem;
}

.article-content :deep(img) {
  max-width: 100%;
  height: auto;
  border-radius: 8px;
  margin: 1.5rem 0;
}

.article-card {
  transition: transform 0.3s;
}

.article-card:hover {
  transform: translateY(-5px);
}

.img-wrapper {
  height: 200px;
  overflow: hidden;
}

.article-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
}

.article-card:hover .article-img {
  transform: scale(1.05);
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>
