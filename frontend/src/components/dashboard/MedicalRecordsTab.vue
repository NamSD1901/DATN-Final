<template>
  <component :is="activeComponent" @switch-tab="$emit('switch-tab', $event)" />
</template>

<script setup lang="ts">
import { ref, onMounted, defineAsyncComponent, markRaw } from 'vue';

const emit = defineEmits<{
  (e: 'switch-tab', tab: string): void;
}>();

// Async load the components to avoid circular dependencies and load faster
const ConsultationRecordTab = defineAsyncComponent(() => import('./ConsultationRecordTab.vue'));
const VaccinationRecordTab = defineAsyncComponent(() => import('./VaccinationRecordTab.vue'));

const activeComponent = ref<any>(null);

onMounted(() => {
  // Determine the active service type
  // Defaulting to Consultation if not set
  const serviceType = localStorage.getItem('active_treatment_service_type') || 'Consultation';
  
  if (serviceType === 'Vaccination') {
    activeComponent.value = markRaw(VaccinationRecordTab);
  } else {
    activeComponent.value = markRaw(ConsultationRecordTab);
  }
});
</script>
