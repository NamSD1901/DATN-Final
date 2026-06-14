<template>
  <div class="chatbot-wrapper">
    <!-- Floating Action Button -->
    <button @click="toggleChat" class="chat-fab shadow-lg" :class="{ 'active': isOpen }">
      <i v-if="!isOpen" class="bi bi-robot fs-3"></i>
      <i v-else class="bi bi-x-lg fs-4"></i>
      <span v-if="!isOpen && unread" class="position-absolute top-0 start-100 translate-middle p-2 bg-danger border border-light rounded-circle"></span>
    </button>

    <!-- Chat window -->
    <Transition name="slide-fade">
      <div v-if="isOpen" class="chat-window shadow-2xl glass-card">
        <!-- Header -->
        <div class="chat-header bg-gold-gradient text-white d-flex align-items-center justify-content-between p-3">
          <div class="d-flex align-items-center gap-2">
            <div class="avatar-wrapper bg-white text-warning rounded-circle d-flex align-items-center justify-content-center" style="width: 38px; height: 38px;">
              <i class="bi bi-robot fs-4"></i>
            </div>
            <div>
              <h6 class="fw-bold mb-0">Trợ Lý AI MyPetClinic</h6>
              <span class="small opacity-75 d-flex align-items-center gap-1">
                <span class="online-indicator"></span> Trực tuyến
              </span>
            </div>
          </div>
          <button @click="isOpen = false" class="btn-close btn-close-white border-0 bg-transparent" aria-label="Close"></button>
        </div>

        <!-- Messages list -->
        <div class="chat-messages p-3" ref="messagesContainer">
          <div class="message-bubble system mb-3">
            Chào mừng bạn đến với <b>MyPetClinic</b>! Tôi là trợ lý ảo y tế, sẵn sàng giải đáp thắc mắc về cách chăm sóc, dinh dưỡng và sơ cứu cơ bản cho thú cưng của bạn.
          </div>

          <div v-for="(msg, idx) in messages" :key="idx" class="message-bubble-wrapper d-flex mb-3" :class="msg.sender">
            <div class="message-bubble">
              <span class="message-text">{{ msg.text }}</span>
              <span class="message-time">{{ formatTime(msg.timestamp) }}</span>
            </div>
          </div>

          <!-- Loading indicator -->
          <div v-if="loading" class="message-bubble-wrapper d-flex mb-3 ai">
            <div class="message-bubble loading-bubble">
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
            </div>
          </div>
        </div>

        <!-- Quick Questions Suggestions -->
        <div class="quick-questions px-3 py-2 border-top d-flex gap-2 overflow-x-auto text-nowrap">
          <button v-for="q in quickQuestions" :key="q" @click="askQuickQuestion(q)" class="btn btn-sm btn-outline-warning rounded-pill px-3 py-1 font-size-xs text-dark border-light bg-light">
            {{ q }}
          </button>
        </div>

        <!-- Input area -->
        <form @submit.prevent="sendMessage" class="chat-input-area p-3 border-top d-flex gap-2 align-items-center bg-white">
          <input type="text" v-model="inputText" placeholder="Hỏi tôi về sức khỏe thú cưng..." class="form-control rounded-pill border-light-subtle shadow-sm ps-3" :disabled="loading" />
          <button type="submit" class="btn btn-warning rounded-circle p-2 shadow-sm text-dark d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;" :disabled="loading || !inputText.trim()">
            <i class="bi bi-send-fill fs-5"></i>
          </button>
        </form>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, watch } from 'vue';
import api from '../../services/api';

interface Message {
  sender: 'user' | 'ai';
  text: string;
  timestamp: Date;
}

const isOpen = ref(false);
const loading = ref(false);
const unread = ref(true);
const inputText = ref('');
const messages = ref<Message[]>([]);
const messagesContainer = ref<HTMLDivElement | null>(null);

const quickQuestions = [
  'Lịch tiêm phòng dại cho chó?',
  'Mèo bị nôn mửa phải làm sao?',
  'Dấu hiệu chó bị sốt?',
  'Chế độ ăn cho mèo con?'
];

const toggleChat = () => {
  isOpen.value = !isOpen.value;
  if (isOpen.value) {
    unread.value = false;
    scrollToBottom();
  }
};

const formatTime = (date: Date) => {
  return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
};

const scrollToBottom = async () => {
  await nextTick();
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
};

const askQuickQuestion = (q: string) => {
  inputText.value = q;
  sendMessage();
};

const sendMessage = async () => {
  if (!inputText.value.trim() || loading.value) return;

  const userPrompt = inputText.value;
  inputText.value = '';

  messages.value.push({
    sender: 'user',
    text: userPrompt,
    timestamp: new Date()
  });
  
  scrollToBottom();
  loading.value = true;

  try {
    const res = await api.post('/AiChatbot/chat', {
      message: userPrompt
    });
    
    messages.value.push({
      sender: 'ai',
      text: res.data.reply || 'Rất tiếc, tôi gặp sự cố khi phản hồi.',
      timestamp: new Date()
    });
  } catch (err) {
    console.error('AI Chat Error:', err);
    messages.value.push({
      sender: 'ai',
      text: 'Kết nối với máy chủ bị gián đoạn. Vui lòng thử lại sau ít phút!',
      timestamp: new Date()
    });
  } finally {
    loading.value = false;
    scrollToBottom();
  }
};

watch(isOpen, (newVal) => {
  if (newVal) {
    scrollToBottom();
  }
});
</script>

<style scoped>
.chatbot-wrapper {
  position: fixed;
  bottom: 25px;
  right: 25px;
  z-index: 1050;
  font-family: var(--font-family-sans-serif, system-ui, -apple-system, sans-serif);
}

.chat-fab {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: white;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  position: relative;
}

.chat-fab:hover {
  transform: scale(1.1) rotate(5deg);
  box-shadow: 0 8px 25px rgba(217, 119, 6, 0.4);
}

.chat-fab.active {
  transform: scale(1.0) rotate(90deg);
  background: #374151;
}

.chat-window {
  position: absolute;
  bottom: 80px;
  right: 0;
  width: 380px;
  height: 520px;
  border-radius: 20px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.35);
  transform-origin: bottom right;
}

@media (max-width: 480px) {
  .chat-window {
    width: 320px;
    height: 480px;
    bottom: 70px;
    right: -10px;
  }
}

.chat-header {
  border-bottom: 1px solid rgba(255,255,255,0.1);
}

.bg-gold-gradient {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
}

.online-indicator {
  width: 8px;
  height: 8px;
  background-color: #10b981;
  border-radius: 50%;
  display: inline-block;
  box-shadow: 0 0 8px #10b981;
}

.chat-messages {
  flex-grow: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.message-bubble {
  max-width: 80%;
  padding: 10px 14px;
  border-radius: 14px;
  font-size: 0.9rem;
  line-height: 1.45;
  display: flex;
  flex-direction: column;
  word-wrap: break-word;
}

.message-bubble.system {
  background: rgba(243, 244, 246, 0.8);
  border: 1px solid rgba(229, 231, 235, 0.5);
  color: #4b5563;
  align-self: center;
  max-width: 90%;
  text-align: center;
}

.message-bubble-wrapper.user {
  justify-content: flex-end;
}

.message-bubble-wrapper.user .message-bubble {
  background: #fdfaf0;
  border: 1px solid #fce8c3;
  color: #78350f;
  border-bottom-right-radius: 2px;
  align-self: flex-end;
}

.message-bubble-wrapper.ai {
  justify-content: flex-start;
}

.message-bubble-wrapper.ai .message-bubble {
  background: white;
  border: 1px solid rgba(0, 0, 0, 0.05);
  color: #1f2937;
  border-bottom-left-radius: 2px;
  align-self: flex-start;
  box-shadow: 0 2px 5px rgba(0,0,0,0.02);
}

.message-time {
  font-size: 0.7rem;
  opacity: 0.6;
  align-self: flex-end;
  margin-top: 4px;
}

.message-text {
  white-space: pre-wrap;
}

/* Quick questions auto-scroll section */
.quick-questions::-webkit-scrollbar {
  height: 4px;
}
.quick-questions::-webkit-scrollbar-thumb {
  background: rgba(0,0,0,0.1);
  border-radius: 4px;
}

.quick-questions button {
  font-size: 0.8rem;
  transition: all 0.2s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.02);
}

.quick-questions button:hover {
  background-color: #fdfaf0;
  color: #d97706 !important;
  border-color: #fce8c3;
}

/* Loading bubble dots */
.loading-bubble {
  display: flex;
  gap: 4px;
  align-items: center;
  padding: 12px 18px;
}
.dot {
  width: 7px;
  height: 7px;
  background-color: #9ca3af;
  border-radius: 50%;
  animation: wave 1.2s infinite ease-in-out;
}
.dot:nth-child(2) { animation-delay: 0.2s; }
.dot:nth-child(3) { animation-delay: 0.4s; }

@keyframes wave {
  0%, 60%, 100% { transform: translateY(0); }
  30% { transform: translateY(-6px); }
}

/* Animations */
.slide-fade-enter-active, .slide-fade-leave-active {
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}
.slide-fade-enter-from, .slide-fade-leave-to {
  transform: scale(0.8) translateY(20px);
  opacity: 0;
}
</style>
