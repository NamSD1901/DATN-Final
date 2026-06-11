# 03. State Management (Pinia Store) - Gemini AI Chatbot

Tài liệu thiết kế Vue 3 Pinia Store sử dụng TypeScript cho phân hệ Trợ lý ảo Tư vấn Sức khỏe AI.

---

## 1. Vai trò của Pinia Store trong Phân hệ

Pinia Store `useChatStore` chịu trách nhiệm quản lý toàn bộ mảng tin nhắn của phiên trò chuyện hiện tại, lưu vết cờ đang gửi tin nhắn (`isSending`) để kích hoạt Typing Indicator, và điều phối thuật toán giới hạn ngữ cảnh (chỉ giữ 10 tin nhắn gần nhất) trước khi gửi yêu cầu lên Backend Proxy.

---

## 2. Mã nguồn TypeScript hoàn chỉnh cho Pinia Store

Dưới đây là cài đặt chi tiết của file `useChatStore.ts` triển khai tại thư mục `frontend/src/stores/useChatStore.ts`:

```typescript
import { defineStore } from 'pinia';
import axios from 'axios';

// Định nghĩa cấu trúc tin nhắn hiển thị trên UI client
export interface Message {
  id: string;
  sender: 'user' | 'ai';
  text: string;
  timestamp: Date;
  requiresAppointment?: boolean; // Cờ đề xuất đặt lịch khám
  systemWarning?: string | null;  // Thông điệp cảnh báo y khoa nếu có
}

// Định nghĩa cấu trúc lịch sử chat rút gọn gửi lên API
export interface ChatHistoryItem {
  role: 'user' | 'model';
  text: string;
}

interface ChatState {
  messages: Message[];
  isSending: boolean;
  isOpen: boolean; // Cờ bật/tắt hiển thị cửa sổ widget
  error: string | null;
}

export const useChatStore = defineStore('chat', {
  state: (): ChatState => ({
    messages: [
      // Tin nhắn chào mừng mặc định từ hệ thống
      {
        id: 'welcome-msg',
        sender: 'ai',
        text: 'Xin chào! Tôi là Trợ lý Bác sĩ thú y ảo của MyPetClinic. Tôi có thể giúp bạn tư vấn sơ cứu khẩn cấp, dinh dưỡng hoặc hành vi thú cưng. Bạn cần trợ giúp gì?',
        timestamp: new Date()
      }
    ],
    isSending: false,
    isOpen: false,
    error: null
  }),

  getters: {
    // Trích xuất lịch sử chat rút gọn (tối đa 10 tin nhắn gần nhất) theo cấu trúc API yêu cầu
    apiHistory(state): ChatHistoryItem[] {
      // Bỏ qua tin nhắn chào mừng mặc định đầu tiên
      const chatMessages = state.messages.filter(m => m.id !== 'welcome-msg');
      
      // Chỉ lấy tối đa 10 tin nhắn gần nhất để giữ context
      const sliced = chatMessages.slice(-10);

      return sliced.map(m => ({
        role: m.sender === 'user' ? 'user' : 'model',
        text: m.text
      }));
    }
  },

  actions: {
    // Bật/tắt cửa sổ widget
    toggleChatWindow() {
      this.isOpen = !this.isOpen;
    },

    // Gửi tin nhắn mới sang Backend Proxy
    async sendMessage(userInput: string) {
      if (!userInput || userInput.trim().length === 0) return;

      const userMessage: Message = {
        id: `user-${Date.now()}`,
        sender: 'user',
        text: userInput,
        timestamp: new Date()
      };

      // 1. Thêm tin nhắn của User vào mảng hiển thị lập tức (Optimistic UI)
      this.messages.push(userMessage);
      this.isSending = true;
      this.error = null;

      // Chuẩn bị payload chứa message hiện tại và mảng history rút gọn
      const payload = {
        message: userInput,
        history: this.apiHistory
      };

      try {
        const response = await axios.post<{ textResponse: string; requiresAppointment: boolean; systemWarning: string | null }>(
          '/api/ai/chat',
          payload
        );

        // 2. Thêm tin nhắn phản hồi của AI vào mảng hiển thị
        const aiMessage: Message = {
          id: `ai-${Date.now()}`,
          sender: 'ai',
          text: response.data.textResponse,
          timestamp: new Date(),
          requiresAppointment: response.data.requiresAppointment,
          systemWarning: response.data.systemWarning
        };

        this.messages.push(aiMessage);
      } catch (err: any) {
        this.error = err.response?.data?.message || 'Không thể kết nối đến Trợ lý AI lúc này.';
        
        // Thêm tin nhắn báo lỗi của hệ thống để người dùng biết
        this.messages.push({
          id: `error-${Date.now()}`,
          sender: 'ai',
          text: `Hệ thống gặp lỗi: ${this.error}. Vui lòng thử lại sau.`,
          timestamp: new Date()
        });
      } finally {
        this.isSending = false;
      }
    },

    // Xóa sạch lịch sử trò chuyện (reset chat session)
    clearChatHistory() {
      this.messages = [
        {
          id: 'welcome-msg',
          sender: 'ai',
          text: 'Đã làm mới cuộc hội thoại. Tôi có thể giúp gì thêm cho sức khỏe bé cưng của bạn?',
          timestamp: new Date()
        }
      ];
      this.error = null;
    }
  }
});
```
