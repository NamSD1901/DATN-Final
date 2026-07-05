<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="isOpen" class="modal d-block" tabindex="-1" style="background: rgba(0, 0, 0, 0.5); z-index: 1055;">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content rounded-4 shadow-lg border-0">
            
            <div class="modal-header border-0 pb-0">
              <h5 class="modal-title fw-bold text-dark fs-4 mx-auto">
                {{ isEdit ? 'Chỉnh sửa đánh giá' : 'Đánh giá dịch vụ' }}
              </h5>
              <button type="button" class="btn-close position-absolute end-0 me-3" @click="closeModal"></button>
            </div>

            <div class="modal-body text-center px-4 pt-4 pb-2">
              <p class="text-muted mb-3 fw-medium">Bạn cảm thấy dịch vụ như thế nào?</p>
              
              <div class="d-flex justify-content-center gap-3 mb-4">
                <button v-for="i in 5" :key="i" @click="form.rating = i" class="btn btn-link p-0 text-decoration-none shadow-none border-0 transition-transform hover-scale">
                  <i class="bi bi-star-fill" :class="i <= form.rating ? 'text-warning' : 'text-secondary opacity-25'" style="font-size: 2.5rem;"></i>
                </button>
              </div>

              <div class="text-start mb-3">
                <label class="form-label fw-bold text-secondary small">Chia sẻ trải nghiệm của bạn (tùy chọn)</label>
                <textarea v-model="form.comment" class="form-control rounded-4 bg-light p-3" rows="3" 
                          placeholder="Bác sĩ rất tận tình, không gian sạch sẽ..." style="resize: none;"></textarea>
              </div>
            </div>

            <div class="modal-footer border-0 px-4 pb-4 pt-0 d-flex gap-3">
              <button @click="closeModal" class="btn btn-light rounded-pill flex-grow-1 fw-bold py-2 border">
                Để sau
              </button>
              <button @click="submit" :disabled="loading || form.rating === 0" 
                      class="btn rounded-pill flex-grow-1 fw-bold py-2 text-white" style="background-color: #0f766e;">
                <span v-if="loading">Đang xử lý...</span>
                <span v-else>Gửi đánh giá</span>
              </button>
            </div>
            
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
.hover-scale {
  transition: transform 0.2s;
}
.hover-scale:hover {
  transform: scale(1.15);
}
</style>

<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps<{
  isOpen: boolean;
  isEdit?: boolean;
  initialData?: { appointmentId?: number, reviewId?: number, rating?: number, comment?: string };
}>();

const emit = defineEmits(['close', 'submit']);

const form = ref({
  rating: 0,
  comment: ''
});

const loading = ref(false);

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    if (props.isEdit && props.initialData) {
      form.value = {
        rating: props.initialData.rating || 0,
        comment: props.initialData.comment || ''
      };
    } else {
      form.value = { rating: 5, comment: '' }; // Default to 5 stars
    }
  }
});

const closeModal = () => {
  emit('close');
};

const submit = () => {
  if (form.value.rating === 0) return;
  emit('submit', { ...form.value, appointmentId: props.initialData?.appointmentId, reviewId: props.initialData?.reviewId });
};
</script>
