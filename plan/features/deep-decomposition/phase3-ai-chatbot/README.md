# 🤖 Gemini AI Chatbot Advisor (Trợ lý ảo Tư vấn Sức khỏe)

## 📝 Mô tả Tính năng
Tích hợp mô hình AI ngôn ngữ lớn (Gemini AI SDK) để làm bác sĩ thú y ảo, tư vấn nhanh kiến thức y học thú cưng, các triệu chứng phổ biến và cách sơ cứu tại chỗ cho chủ nuôi.

## 📋 User Stories (Acceptance Criteria)
*   **PB14 (AI Chatbot):** Khách hàng có thể nhập câu hỏi về tình trạng sức khỏe của thú cưng và nhận phản hồi tự động từ AI.
*   **System Prompt Constraint:** Chatbot phải có định hướng chỉ tư vấn thông tin tham khảo, không tự ý kê đơn thuốc chuyên sâu và khuyên mang thú cưng đến bệnh viện khám nếu gặp triệu chứng khẩn cấp.

## 🛠️ Đặc tả Kỹ thuật (Technical Specs)
*   **API Endpoints:**
    *   `POST /api/ai-chatbot/ask` (Gửi câu hỏi và lịch sử trò chuyện ngắn hạn lên server để nhận câu trả lời từ Gemini API)
*   **SDK Tích hợp:** Google Gen AI SDK for .NET.

## 🎨 Giao diện UI/UX
*   **Views/Components:** Chat Widget nổi ở góc phải bên dưới trang Dashboard.
*   **Trải nghiệm người dùng:** Giao diện hội thoại (bubble chat) mượt mà, hỗ trợ hiệu ứng hiển thị chữ đang gõ (typing indicator) tăng tính tương tác sinh động.
