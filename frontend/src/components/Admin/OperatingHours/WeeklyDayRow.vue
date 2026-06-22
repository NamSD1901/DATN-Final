<template>
  <div class="d-flex flex-column flex-md-row align-items-md-center p-3 border-bottom row-hover-effect">
    <div class="d-flex align-items-center mb-3 mb-md-0" style="width: 180px;">
      <div class="form-check form-switch fs-5 mb-0">
        <input class="form-check-input cursor-pointer shadow-sm" type="checkbox" role="switch" :id="'switch-' + localDay.dayOfWeek" v-model="localDay.isOpen" @change="updateDay">
        <label class="form-check-label ms-2 fw-bold cursor-pointer" :class="localDay.isOpen ? 'text-primary' : 'text-muted'" :for="'switch-' + localDay.dayOfWeek" style="min-width: 85px;">
          {{ getDayName(localDay.dayOfWeek) }}
        </label>
      </div>
    </div>

    <div class="flex-grow-1 d-flex flex-wrap gap-2 align-items-center">
      <template v-if="localDay.isOpen">
        <div v-for="(_, index) in localDay.shifts" :key="index" class="d-flex align-items-center">
          <TimeRangePicker 
            v-model="localDay.shifts[index]" 
            @remove="removeShift(Number(index))" 
            @update:modelValue="updateDay" 
          />
        </div>
        
        <button v-if="localDay.shifts.length < 3" @click="addShift" class="btn btn-outline-primary btn-sm rounded-pill fw-bold d-flex align-items-center dashed-border-btn">
          <i class="bi bi-plus-lg me-1"></i> Thêm ca
        </button>
      </template>
      <div v-else class="text-muted fst-italic small py-1 px-3 bg-light rounded-pill border">
        <i class="bi bi-door-closed me-1"></i> Đóng cửa (Nghỉ)
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import TimeRangePicker from './TimeRangePicker.vue';

const props = defineProps({
  modelValue: {
    type: Object,
    required: true
  }
});

const emit = defineEmits(['update:modelValue']);

// Deep clone to avoid mutating props directly
const localDay = ref(JSON.parse(JSON.stringify(props.modelValue)));

watch(() => props.modelValue, (newVal) => {
  localDay.value = JSON.parse(JSON.stringify(newVal));
}, { deep: true });

const getDayName = (dayOfWeek: number) => {
  const days = ['Chủ nhật', 'Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7'];
  return days[dayOfWeek];
};

const updateDay = () => {
  if (localDay.value.isOpen && localDay.value.shifts.length === 0) {
    // Add default shift if opened and empty
    localDay.value.shifts.push({ startTime: '08:00:00', endTime: '17:00:00' });
  }
  emit('update:modelValue', localDay.value);
};

const removeShift = (index: number) => {
  localDay.value.shifts.splice(index, 1);
  updateDay();
};

const addShift = () => {
  if (localDay.value.shifts.length < 3) {
    localDay.value.shifts.push({ startTime: '08:00:00', endTime: '12:00:00' });
    updateDay();
  }
};
</script>

<style scoped>
.row-hover-effect {
  transition: background-color 0.2s ease;
}
.row-hover-effect:hover {
  background-color: #f8f9fa;
}
.cursor-pointer {
  cursor: pointer;
}
.dashed-border-btn {
  border-style: dashed;
  border-width: 1.5px;
}
.dashed-border-btn:hover {
  border-style: solid;
}
.form-check-input:checked {
  background-color: #0d6efd;
  border-color: #0d6efd;
}
</style>
