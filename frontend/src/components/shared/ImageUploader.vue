<template>
  <div class="w-full">
    <div 
      class="border-2 border-dashed rounded-xl p-6 flex flex-col items-center justify-center transition-colors relative"
      :class="[
        isDragging ? 'border-[var(--primary-color)] bg-[var(--primary-color)]/5' : 'border-gray-300 bg-gray-50 hover:bg-gray-100',
        images.length >= maxImages ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
      ]"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="handleDrop"
      @click="triggerFileInput"
    >
      <input 
        type="file" 
        ref="fileInput" 
        class="hidden" 
        multiple 
        accept="image/jpeg, image/png, image/webp" 
        @change="handleFileInput"
        :disabled="images.length >= maxImages"
      />
      
      <svg class="w-10 h-10 text-gray-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
      </svg>
      <p class="text-sm font-medium text-gray-700 text-center">
        Nhấn để chọn ảnh hoặc kéo thả vào đây
      </p>
      <p class="text-xs text-gray-500 mt-1 text-center">
        Tối đa {{ maxImages }} ảnh (JPG, PNG, WEBP). Tối đa {{ maxSizeMB }}MB/ảnh.
      </p>
    </div>

    <!-- Error Message -->
    <p v-if="error" class="text-red-500 text-xs mt-2">{{ error }}</p>

    <!-- Previews -->
    <div v-if="images.length > 0" class="mt-4 grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-3">
      <div 
        v-for="(img, index) in previews" 
        :key="index" 
        class="relative aspect-square rounded-lg overflow-hidden border border-gray-200 group shadow-sm"
      >
        <img :src="img" alt="Preview" class="w-full h-full object-cover" />
        <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
          <button 
            @click.stop="removeImage(index)" 
            class="bg-red-500 text-white p-1.5 rounded-full hover:bg-red-600 focus:outline-none transform hover:scale-110 transition-transform"
            title="Xóa ảnh"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

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
