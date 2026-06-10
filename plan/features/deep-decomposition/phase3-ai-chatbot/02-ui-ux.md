# 🎨 UI/UX Design Spec - Gemini AI Chatbot Advisor

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Thiết lập màu nền hội thoại, bo tròn bóng kính (glassmorphism) cho hộp thoại nổi.
- **FE-F01 (HTML Semantic):** Sử dụng các thẻ `<section>` và `<header>` cho Widget Chat.

---

## 1. Giao diện Chat Widget nổi (Floating Chat Widget)

- **Vị trí hiển thị:** Cố định ở góc dưới cùng bên phải màn hình (`position: fixed; right: 24px; bottom: 24px; z-index: 1000`).
- **Nút mở Chat (Floating Button):** Nút tròn nhỏ có biểu tượng robot/trợ lý ảo, sử dụng hiệu ứng hover phóng to nhẹ (`transform: scale(1.05)`) và màu gradient chuyển đổi.
- **Hộp Chat (Chat Container):**
  - **Header:** Màu xanh đậm gradient thương hiệu phòng khám, chứa ảnh đại diện Bác sĩ thú y ảo và nút đóng.
  - **Body (Danh sách tin nhắn):** Chiều cao cố định (khoảng `350px - 400px`), hỗ trợ tự động cuộn xuống dưới cùng (`scroll-behavior: smooth`) khi có tin nhắn mới.
  - **Footer (Nhập tin nhắn):** Ô nhập văn bản kèm nút Gửi.

---

## 2. Typing Indicator & Bubble CSS
- Hiệu ứng nhấp nháy 3 dấu chấm tròn nhỏ hiển thị khi đang chờ AI phản hồi:
```css
.typing-indicator span {
  height: 8px;
  width: 8px;
  background-color: var(--primary-gold);
  border-radius: 50%;
  display: inline-block;
  animation: bounce 1.3s infinite ease-in-out;
}
```
- Khung hội thoại tin nhắn người dùng (`.user-msg`) có nền `--primary-gold` hoặc màu xanh ngọc, tin nhắn của AI (`.ai-msg`) có nền xám sáng hoặc trắng mờ kính.
