# 🎨 UI/UX Design Specification - Gemini AI Chatbot

Tài liệu thiết kế giao diện người dùng, sơ đồ bố cục ASCII Chat Widget, bảng màu bong bóng chat HSL và các hiệu ứng gõ chữ (Typing Indicator) sinh động.

---

## 1. Bản vẽ Bố cục giao diện (ASCII Art Mockups)

### Khung Chat Widget nổi dưới góc màn hình (Floating Chat Widget)
Khung chat xuất hiện dạng cửa sổ mini mờ kính sang trọng ở góc dưới bên phải màn hình người dùng.

```text
+---------------------------------------------------------+
| [o] BÁC SĨ THÚ Y ẢO (AI ADVISOR)                    [x] |
+---------------------------------------------------------+
| Trợ lý: Chào bạn! Cún/mèo của bạn có biểu hiện gì?     |
|         Tôi có thể giúp bạn tư vấn sơ cứu nhanh.        |
|                                                         |
| Khách:  Mèo con bị sặc sữa nằm im thì làm thế nào ạ?    |
|                                                         |
| Trợ lý: [!] CẢNH BÁO Y KHOA: Đây là tình huống nguy cấp!|
|         1. Hãy đặt đầu mèo dốc xuống nhẹ, vuốt lưng.    |
|         2. Dùng xi-lanh hút dịch mũi miệng ngay.        |
|         3. Mang mèo tới phòng khám thú y lập tức!       |
|                                                         |
|         [ ĐẶT LỊCH HẸN KHÁM THỰC TẾ NGAY TẠI PHÒNG KHÁM ]|
|                                                         |
| Trợ lý đang nhập câu trả lời... (o o o)                 |
+---------------------------------------------------------+
| [ Nhập câu hỏi của bạn tại đây...             ]  [GỬI]  |
+---------------------------------------------------------+
```

---

## 2. Hệ thống CSS Design Tokens (HSL Color Theme)

Mã màu HSL thiết lập giao diện Glassmorphism mờ kính sâu và các bong bóng chat tương phản:

```css
:root {
  /* Giao diện khung Chat Widget */
  --chat-bg: hsla(220, 25%, 12%, 0.75);     /* Nền tối mờ kính */
  --chat-border: hsla(220, 15%, 100%, 0.08);
  --chat-backdrop-blur: blur(16px);
  
  /* Bong bóng chat */
  --bubble-user: hsl(200, 85%, 45%);         /* Xanh neon - Người dùng */
  --bubble-user-text: hsl(210, 20%, 98%);
  
  --bubble-ai: hsla(220, 15%, 20%, 0.6);      /* Xám mờ kính - Trợ lý AI */
  --bubble-ai-text: hsl(210, 20%, 95%);
  
  /* Cảnh báo y khoa */
  --warning-badge: hsl(40, 95%, 50%);        /* Vàng hổ phách cảnh báo */
  --warning-badge-bg: hsla(40, 95%, 50%, 0.12);

  --btn-action-green: hsl(145, 65%, 45%);    /* Xanh ngọc - Nút Đặt lịch khám */
  --chat-shadow: 0 12px 40px rgba(0, 0, 0, 0.5);
}
```

---

## 3. Hoạt ảnh typing indicator ba chấm nhấp nháy (Typing Indicator)

Khi hệ thống đang chờ phản hồi từ Gemini API, bong bóng chat của trợ lý hiển thị ba dấu chấm chuyển động nhấp nháy tuần tự để tăng cảm giác tương tác thực tế:

```css
/* Container dấu ba chấm */
.typing-indicator {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 8px 12px;
  background: var(--bubble-ai);
  border-radius: 12px;
  width: fit-content;
}

.typing-dot {
  width: 6px;
  height: 6px;
  background-color: var(--bubble-ai-text);
  border-radius: 50%;
  opacity: 0.3;
  animation: wave 1.2s infinite ease-in-out;
}

/* Hiệu ứng nhấp nháy lệch pha giữa 3 chấm */
.typing-dot:nth-child(1) {
  animation-delay: 0s;
}
.typing-dot:nth-child(2) {
  animation-delay: 0.2s;
}
.typing-dot:nth-child(3) {
  animation-delay: 0.4s;
}

@keyframes wave {
  0%, 100% {
    transform: translateY(0);
    opacity: 0.3;
  }
  50% {
    transform: translateY(-4px);
    opacity: 1;
  }
}
```

---

## 4. Đặc tả Bong bóng chat Markdown và Tự động cuộn xuống (Auto-scroll)

- **Markdown Rendering:** Các đoạn code, định dạng in đậm, danh sách gạch đầu dòng (Markdown) từ Gemini trả về phải được giải mã tự động bằng thư viện `marked` hoặc `vue-markdown` phía client để hiển thị đẹp mắt thay vì hiển thị text thô chứa dấu sao `*`.
- **Auto-scroll Interaction:** Khi có tin nhắn mới hoặc nội dung đang được stream ra, vùng hiển thị chat phải tự động cuộn xuống dưới cùng (`scrollTop = scrollHeight`) bằng một chuyển động cuộn trượt mượt (Smooth Scroll):

```typescript
// AutoScroll.ts
export function scrollToBottom(container: HTMLElement | null) {
  if (container) {
    container.scrollTo({
      top: container.scrollHeight,
      behavior: 'smooth'
    });
  }
}
```
