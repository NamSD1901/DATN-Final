<template>
  <router-view />
  <AiChatbotWidget v-if="showChatbot" />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import AiChatbotWidget from './components/shared/AiChatbotWidget.vue';

const route = useRoute();
const showChatbot = computed(() => {
  return !route.path.startsWith('/dashboard') && 
         !route.path.startsWith('/login') && 
         !route.path.startsWith('/register') &&
         !route.path.startsWith('/qr-checkin');
});
</script>

<style>
/* App global styles are handled by style.css */
</style>
