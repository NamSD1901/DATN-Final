# 🚀 Product Requirements Document (PRD) - Gemini AI Chatbot Advisor

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Tính năng **Trợ lý ảo Tư vấn Sức khỏe AI (Gemini AI Advisor Chatbot)** tích hợp trí tuệ nhân tạo (Gemini Pro/Flash API) trực tiếp vào ứng dụng MyPetClinic để cung cấp câu trả lời tức thì, 24/7 cho các câu hỏi thường gặp về chăm sóc thú cưng, dinh dưỡng, hành vi và các bước sơ cứu khẩn cấp. 

Mục tiêu chính là gia tăng tương tác của khách hàng trên hệ thống, cung cấp kiến thức sơ cứu ban đầu chính xác và hướng khách hàng đặt lịch khám thực tế một cách tự nhiên khi thú cưng có dấu hiệu bệnh lý nghiêm trọng.

---

## 2. Đối tượng sử dụng (Target Personas)

### 🧑‍🌾 Khách hàng nuôi thú cưng - Anh Nam (28 tuổi)
- **Mô tả:** Nam đem chú mèo Anh lông ngắn về nuôi được 2 tuần. Đêm muộn lúc 23h, mèo bỗng dưng bỏ bữa, nằm im một chỗ và nôn trớ nhẹ. Nam cực kỳ lo lắng nhưng phòng khám thú y đã đóng cửa.
- **Nỗi đau (Pain points):**
  - Không biết mèo của mình bị ngộ độc, khó tiêu thông thường hay bị các bệnh truyền nhiễm nguy hiểm.
  - Sợ tự ý cho mèo uống thuốc người sẽ gây nguy hại tính mạng mèo.
  - Muốn tìm kiếm thông tin nhanh nhưng các bài viết trên mạng quá chung chung và không tương tác được.
- **Mong muốn:** Một khung chat thông minh phản hồi ngay lập tức, hướng dẫn anh cách sơ cứu tạm thời và đưa ra lời khuyên có nên mang mèo đi cấp cứu khẩn cấp hay không.

---

## 3. User Stories & Tiêu chí Nghiệm thu (Acceptance Criteria)

### Story 1: Trò chuyện và nhận tư vấn chăm sóc sơ cứu tức thì
> **Là một** Khách hàng nuôi thú cưng,  
> **Tôi muốn** trò chuyện bằng ngôn ngữ tự nhiên với trợ lý AI của phòng khám,  
> **Để** tôi nhận được các lời khuyên sơ cứu và kiến thức chăm sóc thú cưng nhanh chóng bất kể thời gian.

#### Tiêu chí Nghiệm thu (AC):
- **AC 1.1:** Giao diện hiển thị Chat Widget nổi ở góc dưới bên phải màn hình. Người dùng có thể click để mở rộng hoặc thu nhỏ khung chat.
- **AC 1.2:** Khách hàng nhập câu hỏi văn bản tự do và bấm gửi. Hệ thống hiển thị trạng thái gõ chữ (Typing Indicator) và phản hồi câu trả lời dạng text mượt mà dưới 1.5 giây.
- **AC 1.3:** AI phải trả lời bằng tiếng Việt thân thiện, dễ hiểu, xưng hô phù hợp với chân dung "Trợ lý bác sĩ thú y ảo của MyPetClinic".

### Story 2: Ràng buộc an toàn y khoa nghiêm ngặt (Medical Guardrails)
> **Là một** Bác sĩ thú y của phòng khám,  
> **Tôi muốn** trợ lý AI không được phép tự ý kê đơn thuốc chuyên khoa hoặc chẩn đoán xác định bệnh nặng thay bác sĩ,  
> **Để** bảo vệ an toàn tính mạng cho thú cưng và tránh các rủi ro pháp lý cho phòng khám.

#### Tiêu chí Nghiệm thu (AC):
- **AC 2.1:** Khi người dùng hỏi về phác đồ điều trị các bệnh truyền nhiễm nặng (Care, Parvo, ngộ độc...) hoặc yêu cầu kê tên thuốc kháng sinh/thuốc đặc trị, AI **tuyệt đối không** được đưa ra đơn thuốc cụ thể.
- **AC 2.2:** AI bắt buộc phải trả về câu từ từ chối khéo léo và hiển thị thông điệp cảnh báo màu vàng: *"Thông tin tư vấn mang tính chất tham khảo. Bạn không nên tự ý cho thú cưng uống thuốc mà chưa có chỉ định của bác sĩ chuyên khoa."*
- **AC 2.3:** AI phải tự động chèn nút **[Đặt lịch khám ngay]** liên kết trực tiếp đến trang đặt lịch hẹn khám của MyPetClinic mỗi khi phát hiện thú cưng của khách hàng có triệu chứng nguy hiểm.

### Story 3: Quản lý lịch sử hội thoại ngắn hạn (Context Memory)
> **Là một** Khách hàng nuôi thú cưng,  
> **Tôi muốn** AI hiểu và nhớ nội dung các câu hỏi trước đó trong cùng phiên chat,  
> **Để** tôi không phải lặp lại thông tin mô tả nhiều lần.

#### Tiêu chí Nghiệm thu (AC):
- **AC 3.1:** Hệ thống gửi kèm tối đa **10 tin nhắn** gần nhất trong phiên chat hiện tại lên API để làm dữ liệu ngữ cảnh (Context History).
- **AC 3.2:** Phiên chat được lưu trữ tạm thời trong bộ nhớ State của Client. Khi người dùng reload trang hoặc tắt trình duyệt, lịch sử chat ngắn hạn này sẽ tự động xóa sạch để bảo vệ tài nguyên hệ thống.

---

## 4. Phạm vi dự án (In-Scope & Out-of-Scope)

### ✅ In-Scope (Phase 3 MVP)
- Khung Chat Widget nổi mờ kính Glassmorphism góc dưới màn hình.
- Tích hợp Gemini 1.5 Flash API thông qua Backend Gateway làm proxy bảo mật.
- Thiết lập System Instruction nghiêm ngặt chống kê đơn và hướng đặt lịch khám thực tế.
- Lưu trữ 10 tin nhắn gần nhất làm ngữ cảnh hội thoại.
- Nút bấm liên kết nhanh đến Form đặt lịch hẹn khám từ khung chat.

### ❌ Out-of-Scope (Bàn giao Phase sau)
- Tải lên hình ảnh thú cưng để AI chẩn đoán vết thương lâm sàng qua camera (Multimodal).
- Tích hợp AI Chatbot vào kênh Zalo OA hoặc Facebook Messenger của phòng khám.

---

## 5. Yêu cầu phi chức năng (Non-Functional Requirements - NFRs)
- **Hiệu năng:** Thời gian phản hồi của API AI Chatbot từ khi gửi đến khi bắt đầu hiển thị kết quả dưới 1.5 giây.
- **Bảo mật:** API Key của Google Gemini được lưu ẩn hoàn toàn ở Backend. API chat áp dụng rate limit chống spam làm tăng chi phí token.
- **Tương thích:** Chat Widget hiển thị tốt và không che khuất các phần tử tương tác chính trên giao diện Mobile và Desktop.
