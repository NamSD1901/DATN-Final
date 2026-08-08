<template>
  <div class="w-100">
    <input 
      type="file" 
      ref="fileInput" 
      class="d-none" 
      multiple 
      accept="image/jpeg, image/png, image/webp" 
      @change="handleFileInput"
      :disabled="images.length >= maxImages"
    />
    
    <!-- Big Dropzone when empty -->
    <div 
      v-if="images.length === 0"
      class="p-4 d-flex flex-column align-items-center justify-content-center position-relative"
      style="border: 2px dashed #dee2e6; border-radius: 1rem; transition: background-color 0.2s;"
      :class="[
        isDragging ? 'bg-primary bg-opacity-10 border-primary' : 'bg-light custom-hover-bg',
        'cursor-pointer'
      ]"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="handleDrop"
      @click="triggerFileInput"
    >
      <svg class="text-secondary mb-3" style="width: 48px; height: 48px;" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
      </svg>
      <p class="fw-medium text-dark text-center mb-1">
        Nhấn để chọn ảnh hoặc kéo thả vào đây
      </p>
      <p class="small text-muted text-center mb-0">
        Tối đa {{ maxImages }} ảnh (JPG, PNG, WEBP). Tối đa {{ maxSizeMB }}MB/ảnh.
      </p>
    </div>

    <!-- Previews and Mini Add Button -->
    <div v-else class="row g-2 mt-1">
      <div 
        v-for="(img, index) in previews" 
        :key="index" 
        class="col-4 col-md-3 col-lg-2"
      >
        <div class="position-relative rounded overflow-hidden border bg-white shadow-sm" style="aspect-ratio: 1; cursor: pointer;" @click="previewUrl = img">
          <img :src="img" alt="Preview" class="w-100 h-100 object-fit-cover" />
          
          <!-- Delete Button (Top Right) -->
          <button 
            @click.stop="removeImage(index)" 
            class="btn btn-danger position-absolute top-0 end-0 m-1 rounded-circle p-0 d-flex align-items-center justify-content-center shadow"
            style="width: 24px; height: 24px; z-index: 10;"
            title="Xóa ảnh"
          >
            <i class="bi bi-x" style="font-size: 1.2rem; line-height: 1;"></i>
          </button>
        </div>
      </div>
      
      <!-- Mini Add Button -->
      <div v-if="images.length < maxImages" class="col-4 col-md-3 col-lg-2">
        <div 
          class="position-relative rounded overflow-hidden d-flex flex-column align-items-center justify-content-center cursor-pointer custom-hover-bg"
          style="aspect-ratio: 1; border: 2px dashed #dee2e6; background-color: #f8f9fa;"
          @dragover.prevent="isDragging = true"
          @dragleave.prevent="isDragging = false"
          @drop.prevent="handleDrop"
          @click="triggerFileInput"
        >
          <i class="bi bi-plus-lg text-secondary mb-1" style="font-size: 1.5rem;"></i>
          <span class="text-secondary fw-bold" style="font-size: 0.75rem;">Thêm</span>
        </div>
      </div>
    </div>
    
    <!-- Error Message -->
    <p v-if="error" class="text-danger small mt-2 mb-0">{{ error }}</p>

    <!-- Fullscreen Image Preview -->
    <Teleport to="body">
      <div v-if="previewUrl" class="position-fixed top-0 start-0 w-100 h-100 bg-dark bg-opacity-75 d-flex align-items-center justify-content-center" style="z-index: 9999;" @click="previewUrl = null">
        <div class="position-relative" style="max-width: 90%; max-height: 90%;">
          <img :src="previewUrl" class="img-fluid rounded shadow-lg" style="max-height: 90vh;" @click.stop />
          <button class="btn-close btn-close-white position-absolute top-0 end-0 m-3 shadow" @click="previewUrl = null"></button>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.custom-hover-bg:hover {
  background-color: #e9ecef !important;
}
.custom-hover-opacity {
  opacity: 0;
  transition: opacity 0.2s ease-in-out;
}
.position-relative:hover .custom-hover-opacity {
  opacity: 1;
}
.object-fit-cover {
  object-fit: cover;
}
</style>

<script setup lang="ts">
import { ref, watch, onBeforeUnmount } from 'vue';

const props = withDefaults(defineProps<{
  modelValue: File[];
  maxImages?: number;
  maxSizeMB?: number;
}>(), {
  maxImages: 5,
  maxSizeMB: 5
});

const emit = defineEmits(['update:modelValue']);

const isDragging = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const error = ref('');

const images = ref<File[]>([...props.modelValue]);
const previews = ref<string[]>([]);
const previewUrl = ref<string | null>(null);

// Sync from parent
watch(() => props.modelValue, (newVal) => {
  if (newVal !== images.value) {
    images.value = [...newVal];
    generatePreviews();
  }
});

const triggerFileInput = () => {
  if (images.value.length < props.maxImages && fileInput.value) {
    fileInput.value.click();
  }
};

const handleFileInput = (e: Event) => {
  const target = e.target as HTMLInputElement;
  if (target.files) {
    addFiles(Array.from(target.files));
  }
  // Reset input value to allow selecting the same file again if removed
  if (target) {
    target.value = '';
  }
};

const handleDrop = (e: DragEvent) => {
  isDragging.value = false;
  if (e.dataTransfer?.files) {
    addFiles(Array.from(e.dataTransfer.files));
  }
};

const addFiles = (files: File[]) => {
  error.value = '';
  const currentCount = images.value.length;
  const availableSlots = props.maxImages - currentCount;
  
  if (availableSlots <= 0) {
    error.value = `Bạn chỉ được tải lên tối đa ${props.maxImages} ảnh.`;
    return;
  }

  const validFiles = files.filter(file => {
    // Check type
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      error.value = 'Chỉ chấp nhận định dạng JPG, PNG, WEBP.';
      return false;
    }
    // Check size
    if (file.size > props.maxSizeMB * 1024 * 1024) {
      error.value = `Kích thước ảnh không được vượt quá ${props.maxSizeMB}MB.`;
      return false;
    }
    return true;
  });

  const filesToAdd = validFiles.slice(0, availableSlots);
  if (filesToAdd.length > 0) {
    images.value = [...images.value, ...filesToAdd];
    generatePreviews();
    emit('update:modelValue', images.value);
  }
  
  if (validFiles.length > availableSlots) {
    error.value = `Đã bỏ qua ${validFiles.length - availableSlots} ảnh vì vượt quá giới hạn ${props.maxImages} ảnh.`;
  }
};

const removeImage = (index: number) => {
  images.value.splice(index, 1);
  generatePreviews();
  emit('update:modelValue', images.value);
};

const generatePreviews = () => {
  // Revoke old object URLs to avoid memory leaks
  previews.value.forEach(url => URL.revokeObjectURL(url));
  
  previews.value = images.value.map(file => URL.createObjectURL(file));
};

// Cleanup on unmount
onBeforeUnmount(() => {
  previews.value.forEach(url => URL.revokeObjectURL(url));
});
</script>
