# 🚀 Product Requirements Document (PRD) - Gemini AI Chatbot Advisor

## 1. Tổng quan & Tầm nhìn
Tính năng **Trợ lý ảo Tư vấn Sức khỏe (Gemini AI Advisor Chatbot)** tích hợp trí tuệ nhân tạo (Gemini Pro/Flash API) trực tiếp vào ứng dụng để cung cấp câu trả lời tức thì, 24/7 cho các câu hỏi thường gặp về chăm sóc thú cưng, dinh dưỡng, hành vi và các bước sơ cứu khẩn cấp. Đây là điểm nhấn công nghệ giúp gia tăng tương tác của khách hàng trên hệ thống MyPetClinic.

---

## 2. Đối tượng sử dụng (Target Persona)
- **Khách hàng (Customer):** Cần lời khuyên nhanh khi thú cưng có biểu hiện lạ trước khi quyết định đặt lịch khám chính thức tại phòng khám.

---

## 3. Yêu cầu Nghiệp vụ Chi tiết
- **Giao diện Trò chuyện Hội thoại (PB14):**
  - Khung Chat Widget nổi dưới góc màn hình hoặc trang Chat chuyên dụng.
  - Cho phép người dùng nhập câu hỏi dạng văn bản tự do, trả về kết quả tức thì.
- **Ràng buộc Hệ thống (System Prompt Constraints) - Vô cùng Quan trọng:**
  - AI phải đóng vai là "Bác sĩ thú y ảo tư vấn thân thiện của MyPetClinic".
  - **TUYỆT ĐỐI KHÔNG** được phép tự ý đưa ra phác đồ điều trị chuyên khoa, không được tự ý kê đơn thuốc kháng sinh hoặc thuốc đặc trị.
  - Phải luôn luôn đính kèm khuyến cáo: *"Thông tin tư vấn mang tính chất tham khảo. Nếu thú cưng có triệu chứng nặng (sốt cao, co giật, khó thở, ngộ độc độc chất...), vui lòng mang ngay tới phòng khám thú y gần nhất hoặc đặt lịch hẹn khám trực tiếp tại phòng khám của chúng tôi."*
- **Quản lý Lịch sử Hội thoại (Context Maintenance):**
  - Lưu giữ lịch sử 5-10 câu hội thoại gần nhất trong phiên làm việc để AI hiểu ngữ cảnh thảo luận.
