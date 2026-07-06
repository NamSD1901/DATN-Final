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
        <div class="quick-questions border-top d-flex gap-2 text-nowrap">
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
    const startTime = Date.now();
    const res = await api.post('/AiChatbot/chat', {
      message: userPrompt
    });
    
    // Groq phản hồi quá nhanh (thường dưới 200ms), ta sẽ thêm delay ảo để hiển thị hiệu ứng typing "tự nhiên" hơn
    const elapsed = Date.now() - startTime;
    if (elapsed < 1200) {
      await new Promise(resolve => setTimeout(resolve, 1200 - elapsed));
    }
    
    messages.value.push({
      sender: 'ai',
      text: res.data.reply || 'Rất tiếc, tôi gặp sự cố khi phản hồi.',
      timestamp: new Date()
    });
  } catch (err: any) {
    console.error('AI Chat Error:', err);
    messages.value.push({
      sender: 'ai',
      text: err.response?.data?.reply || 'Kết nối với máy chủ bị gián đoạn. Vui lòng thử lại sau ít phút!',
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
  font-family: 'Inter', system-ui, -apple-system, sans-serif;
}

.chat-fab {
  width: 65px;
  height: 65px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f59e0b 0%, #ea580c 100%);
  color: white;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  position: relative;
  box-shadow: 0 10px 25px rgba(234, 88, 12, 0.4);
}

.chat-fab:hover {
  transform: scale(1.08) translateY(-5px);
  box-shadow: 0 15px 35px rgba(234, 88, 12, 0.5);
}

.chat-fab.active {
  transform: scale(1.0) rotate(90deg);
  background: #1f2937;
  box-shadow: 0 10px 25px rgba(31, 41, 55, 0.4);
}

.chat-window {
  position: absolute;
  bottom: 85px;
  right: 0;
  width: 380px;
  height: 560px;
  border-radius: 24px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.8);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.12), 0 0 0 1px rgba(0,0,0,0.02);
  transform-origin: bottom right;
}

@media (max-width: 480px) {
  .chat-window {
    width: calc(100vw - 40px);
    height: 500px;
    bottom: 80px;
    right: 0;
  }
}

.chat-header {
  border-bottom: 1px solid rgba(255,255,255,0.2);
  padding: 16px 20px;
}

.bg-gold-gradient {
  background: linear-gradient(135deg, #f59e0b 0%, #ea580c 100%);
}

.online-indicator {
  width: 8px;
  height: 8px;
  background-color: #10b981;
  border-radius: 50%;
  display: inline-block;
  box-shadow: 0 0 8px rgba(16, 185, 129, 0.6);
}

.chat-messages {
  flex-grow: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  padding: 20px;
  background: #f8fafc;
}

.message-bubble {
  max-width: 85%;
  padding: 12px 16px;
  border-radius: 18px;
  font-size: 0.95rem;
  line-height: 1.5;
  display: flex;
  flex-direction: column;
  word-wrap: break-word;
  position: relative;
}

.message-bubble.system {
  background: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(229, 231, 235, 0.8);
  color: #4b5563;
  align-self: center;
  max-width: 90%;
  text-align: center;
  font-size: 0.85rem;
  box-shadow: 0 2px 10px rgba(0,0,0,0.02);
  border-radius: 12px;
}

.message-bubble-wrapper.user {
  justify-content: flex-end;
}

.message-bubble-wrapper.user .message-bubble {
  background: linear-gradient(135deg, #f59e0b 0%, #ea580c 100%);
  color: white;
  border-bottom-right-radius: 4px;
  align-self: flex-end;
  box-shadow: 0 4px 15px rgba(234, 88, 12, 0.2);
}

.message-bubble-wrapper.user .message-time {
  color: rgba(255,255,255,0.7);
}

.message-bubble-wrapper.ai {
  justify-content: flex-start;
}

.message-bubble-wrapper.ai .message-bubble {
  background: white;
  border: 1px solid rgba(0, 0, 0, 0.04);
  color: #1f2937;
  border-bottom-left-radius: 4px;
  align-self: flex-start;
  box-shadow: 0 4px 15px rgba(0,0,0,0.04);
}

.message-time {
  font-size: 0.7rem;
  opacity: 0.7;
  align-self: flex-end;
  margin-top: 6px;
}

.message-text {
  white-space: pre-wrap;
}

/* Quick questions auto-scroll section */
.quick-questions {
  background: #ffffff;
  padding: 12px 16px 14px 16px;
  overflow-x: auto;
  overflow-y: hidden;
}

.quick-questions::-webkit-scrollbar {
  height: 0px; /* Hide scrollbar for a cleaner look */
}

.quick-questions button {
  font-size: 0.85rem;
  transition: all 0.25s ease;
  box-shadow: 0 2px 6px rgba(0,0,0,0.04);
  border: 1px solid #fef3c7 !important;
  color: #d97706 !important;
  background-color: #fffbeb !important;
}

.quick-questions button:hover {
  background-color: #f59e0b !important;
  color: #ffffff !important;
  border-color: #f59e0b !important;
  transform: translateY(-1px);
}

/* Input area */
.chat-input-area {
  background: #ffffff;
  padding: 16px;
}

.chat-input-area input {
  background: #f1f5f9;
  border: 1px solid transparent;
  transition: all 0.3s;
  padding: 12px 20px;
  font-size: 0.95rem;
}

.chat-input-area input:focus {
  background: #ffffff;
  border-color: #fcd34d;
  box-shadow: 0 0 0 4px rgba(245, 158, 11, 0.1);
  outline: none;
}

.chat-input-area button {
  background: linear-gradient(135deg, #f59e0b 0%, #ea580c 100%);
  color: white;
  border: none;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.chat-input-area button:hover:not(:disabled) {
  transform: scale(1.1) rotate(-10deg);
  box-shadow: 0 4px 12px rgba(234, 88, 12, 0.3) !important;
}

/* Loading bubble dots */
.loading-bubble {
  display: flex;
  gap: 6px;
  align-items: center;
  padding: 14px 20px;
}
.dot {
  width: 8px;
  height: 8px;
  background-color: #cbd5e1;
  border-radius: 50%;
  animation: wave 1.2s infinite ease-in-out;
}
.dot:nth-child(2) { animation-delay: 0.2s; }
.dot:nth-child(3) { animation-delay: 0.4s; }

@keyframes wave {
  0%, 60%, 100% { transform: translateY(0); }
  30% { transform: translateY(-6px); background-color: #f59e0b; }
}

/* Animations */
.slide-fade-enter-active, .slide-fade-leave-active {
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}
.slide-fade-enter-from, .slide-fade-leave-to {
  transform: scale(0.9) translateY(20px);
  opacity: 0;
}
</style>
