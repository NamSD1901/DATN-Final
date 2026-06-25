<template>
  <router-view />
  <AiChatbotWidget v-if="!isDashboard" />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import AiChatbotWidget from './components/shared/AiChatbotWidget.vue';

const route = useRoute();
const isDashboard = computed(() => {
  return route.path.startsWith('/dashboard');
});
</script>

<style>
/* App global styles are handled by style.css */
</style>
