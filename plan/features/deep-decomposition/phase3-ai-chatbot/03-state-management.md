# 🗃️ State Management - Gemini AI Chatbot Advisor

## 🔗 Skills Liên Quan
- **FE-C03 (Pinia):** Quản lý trạng thái đóng/mở chat widget và danh sách tin nhắn lịch sử của phiên hiện tại.

---

## 1. Pinia Store: `useAiChatStore`

```typescript
import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export interface Message {
  role: 'user' | 'model';
  content: string;
}

export const useAiChatStore = defineStore('aiChat', () => {
  const isOpen = ref(false);
  const messages = ref<Message[]>([]);
  const loading = ref(false);

  fn toggleChat() {
    isOpen.value = !isOpen.value;
  }

  async fn sendMessage(text: string) {
    if (!text.trim()) return;

    // 1. Thêm tin nhắn của User vào UI trước
    messages.value.push({ role: 'user', content: text });
    
    loading.value = true;
    try {
      // Gửi lịch sử chat cùng với tin nhắn mới
      const history = messages.value.slice(0, -1); // Loại bỏ tin nhắn cuối vừa thêm
      
      const response = await axios.post('/api/ai-chatbot/ask', {
        history,
        message: text
      });

      // 2. Thêm phản hồi của AI vào UI
      messages.value.push({ role: 'model', content: response.data.response });
    } catch (error) {
      console.error('Lỗi khi gửi tin nhắn cho AI', error);
      messages.value.push({
        role: 'model',
        content: 'Xin lỗi, tôi đang gặp lỗi kết nối với máy chủ AI. Bạn vui lòng thử lại sau nhé!'
      });
    } finally {
      loading.value = false;
    }
  }

  fn clearHistory() {
    messages.value = [];
  }

  return {
    isOpen,
    messages,
    loading,
    toggleChat,
    sendMessage,
    clearHistory
  };
});
```
