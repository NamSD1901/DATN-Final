<template>
  <aside class="col-lg-3 sticky-top" style="top: 100px; z-index: 10;">
    <div class="bg-white rounded-4 p-4 shadow-sm border">
      <div class="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom">
        <h6 class="fw-bold mb-0 text-dark">Bộ lọc</h6>
        <button @click="clearAll" class="btn btn-link text-decoration-none p-0 fw-bold" style="font-size: 0.85rem; color: var(--primary-gold);">
          Xóa lọc
        </button>
      </div>

      <!-- Lọc theo số sao -->
      <div class="mb-4">
        <h6 class="fw-bold mb-3 text-dark fs-6" style="font-size: 0.85rem !important; text-transform: uppercase; letter-spacing: 0.5px;">Số Sao</h6>
        <div class="d-flex flex-column gap-2">
          <div v-for="stars in [5,4,3,2,1]" :key="stars" class="form-check custom-radio d-flex align-items-center gap-2">
            <input class="form-check-input mt-0" type="radio" :id="'star'+stars" :value="stars" v-model="localFilters.rating" @change="emitChange" style="cursor: pointer;">
            <label class="form-check-label d-flex align-items-center gap-1 w-100" :for="'star'+stars" style="cursor: pointer;">
              <span class="fw-medium text-dark">{{ stars }}</span>
              <i class="bi bi-star-fill text-warning" style="font-size: 0.9rem;"></i>
            </label>
          </div>
        </div>
      </div>

      <!-- Lọc theo loại vật nuôi -->
      <div class="mb-4">
        <h6 class="fw-bold mb-3 text-dark" style="font-size: 0.85rem !important; text-transform: uppercase; letter-spacing: 0.5px;">Loài Vật</h6>
        <select class="form-select form-select-sm shadow-none py-2 px-3 fw-medium" v-model="localFilters.petType" @change="emitChange" style="border-radius: 8px; border-color: #e2e8f0; color: #475569;">
          <option value="">Tất cả</option>
          <option value="Chó">Chó</option>
          <option value="Mèo">Mèo</option>
          <option value="Khác">Khác</option>
        </select>
      </div>

      <!-- Lọc có hình ảnh -->
      <div class="mb-4">
        <div class="form-check custom-checkbox d-flex align-items-center gap-2">
          <input class="form-check-input mt-0" type="checkbox" id="hasImages" v-model="localFilters.hasImages" @change="emitChange" style="cursor: pointer;">
          <label class="form-check-label fw-bold text-dark" for="hasImages" style="font-size: 0.9rem; cursor: pointer;">
            Chỉ xem có hình ảnh
          </label>
        </div>
      </div>

      <!-- Sắp xếp -->
      <div class="mb-2">
        <h6 class="fw-bold mb-3 text-dark" style="font-size: 0.85rem !important; text-transform: uppercase; letter-spacing: 0.5px;">Sắp xếp theo</h6>
        <select class="form-select form-select-sm shadow-none py-2 px-3 fw-medium" v-model="localFilters.sortBy" @change="emitChange" style="border-radius: 8px; border-color: #e2e8f0; color: #475569;">
          <option value="date_desc">Mới nhất</option>
          <option value="date_asc">Cũ nhất</option>
          <option value="rating_desc">Điểm cao nhất</option>
          <option value="rating_asc">Điểm thấp nhất</option>
        </select>
      </div>

    </div>
  </aside>
</template>

<script setup lang="ts">
import { reactive, watch } from 'vue';

const props = defineProps<{
  modelValue: {
    rating: number | null,
    petType: string,
    sortBy: string,
    hasImages: boolean
  }
}>();

const emit = defineEmits(['update:modelValue', 'change']);

const localFilters = reactive({ ...props.modelValue });

watch(() => props.modelValue, (newVal) => {
  Object.assign(localFilters, newVal);
}, { deep: true });

const emitChange = () => {
  emit('update:modelValue', { ...localFilters });
  emit('change');
};

const clearFilters = () => {
  localFilters.value = {
    rating: null,
    petType: '',
    sortBy: 'date_desc',
    hasImages: false
  };
  emitUpdate();
};
</script>

<style scoped>
.cursor-pointer {
  cursor: pointer;
}
.custom-radio-label {
  transition: all var(--transition-speed) ease;
}
.custom-radio-label:hover {
  color: var(--primary-dark) !important;
  transform: translateX(4px);
}
.custom-radio:checked {
  background-color: var(--primary-gold) !important;
  border-color: var(--primary-dark) !important;
}
.custom-checkbox:checked {
  background-color: var(--primary-gold) !important;
  border-color: var(--primary-dark) !important;
}
.form-check-input:focus {
  border-color: var(--primary-gold) !important;
  box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.15) !important;
}
</style>
