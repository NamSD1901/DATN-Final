<template>
  <div class="d-flex align-items-center bg-light border rounded-pill px-3 py-2 shadow-sm time-picker-wrapper mx-1">
    <div class="d-flex align-items-center">
      <i class="bi bi-clock text-warning me-2"></i>
      <input
        type="time"
        v-model="startTime"
        @change="updateRange"
        class="form-control border-0 bg-transparent p-0 fw-bold text-dark time-input"
        required
      />
    </div>
    <span class="text-muted mx-2 fw-bold">-</span>
    <div class="d-flex align-items-center">
      <input
        type="time"
        v-model="endTime"
        @change="updateRange"
        class="form-control border-0 bg-transparent p-0 fw-bold text-dark time-input text-end"
        required
      />
    </div>
    <button @click="$emit('remove')" type="button" class="btn btn-link text-danger p-0 ms-3 text-decoration-none hover-scale" title="Xóa ca">
      <i class="bi bi-x-circle-fill fs-5"></i>
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps({
  modelValue: {
    type: Object,
    required: true,
  }
});

const emit = defineEmits(['update:modelValue', 'remove']);

const startTime = ref(props.modelValue.startTime.substring(0, 5));
const endTime = ref(props.modelValue.endTime.substring(0, 5));

watch(() => props.modelValue, (newVal) => {
  startTime.value = newVal.startTime.substring(0, 5);
  endTime.value = newVal.endTime.substring(0, 5);
}, { deep: true });

const updateRange = () => {
  emit('update:modelValue', {
    startTime: startTime.value + ':00',
    endTime: endTime.value + ':00'
  });
};
</script>

<style scoped>
.time-picker-wrapper {
  transition: all 0.2s ease;
  border-color: #e2e8f0 !important;
}
.time-picker-wrapper:hover {
  border-color: #f59e0b !important;
  box-shadow: 0 4px 6px rgba(245, 158, 11, 0.1) !important;
}
.time-input {
  width: 105px;
  cursor: pointer;
}
.time-input:focus {
  box-shadow: none;
}
.hover-scale {
  transition: transform 0.2s;
}
.hover-scale:hover {
  transform: scale(1.15);
}

/* Hide visual clock icon but keep the click area inside the input bounds */
input[type="time"]::-webkit-calendar-picker-indicator {
  background: transparent;
  color: transparent;
  cursor: pointer;
  opacity: 0;
}
</style>
