# 🤖 TRỢ LÝ ẢO TƯ VẤN SỨC KHỎE AI (GEMINI AI ADVISOR CHATBOT)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 3 - FEATURE 21)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Trợ lý ảo Tư vấn Sức khỏe AI (Gemini AI Advisor Chatbot)** của Khách hàng trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Tầm nhìn chăm sóc y học của AI, chân dung khách hàng chat tư vấn, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence gửi câu hỏi & trả kết quả từ Gemini API, cấu trúc lưu trữ ngữ cảnh hội thoại ngắn hạn. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | C# Code mẫu tích hợp Gemini SDK, cấu hình System Instruction chặn kê đơn thuốc, và thuật toán định dạng mảng lịch sử chat. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế khung Chat Widget nổi mờ kính CSS HSL, bong bóng chat thân thiện, và hoạt ảnh ba chấm gõ chữ (Typing Indicator). | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`useChatStore`) quản lý danh sách tin nhắn hiện tại, cờ gửi tin nhắn. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Phân quyền an toàn gọi AI, bảo mật API Key qua cấu hình biến môi trường Backend, và Rate Limiting kiểm soát chi phí token Google. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho cổng `POST /api/ai/chat`, payloads JSON gửi lên và kết quả nhận về kèm mã lỗi. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối các trạng thái hoạt động của cửa sổ chat. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng hành trình người dùng từ lúc thú cưng có biểu hiện lạ, mở cửa sổ AI, nhận tư vấn sơ cứu đến khi bấm đặt lịch khám trực tiếp. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành chat cho khách hàng, cách thiết lập API Key của Google Gemini, cURL commands test API. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình phát triển nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) an toàn Prompt Injection, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Trợ lý AI Chatbot sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Tuyệt Đối An Toàn Y Khoa (Guardrails):** Mô hình AI bắt buộc phải sử dụng System Prompt chặn đứng 100% các câu hỏi yêu cầu kê đơn thuốc đặc trị, kháng sinh. Luôn đưa ra khuyến cáo y tế hướng người nuôi đặt lịch khám bác sĩ thực tế khi có biểu hiện nghiêm trọng.
2. **Bảo Mật API Key Tuyệt Đối:** API Key Google Gemini chỉ được lưu trữ và sử dụng trực tiếp tại Backend Layer. Tuyệt đối không để lộ key trên client hoặc trong mã nguồn frontend.
3. **Quản Lý Lịch Sử Chat Tối Ưu (Contextual Memory):** Lưu trữ tối đa 10 tin nhắn gần nhất của cuộc hội thoại làm ngữ cảnh (Session-based History) để AI có thể hiểu và trả lời tiếp nối câu hỏi trước mà không làm đội chi phí API token.
4. **Trải Nghiệm Giao Diện Mượt Mà (Typing Effect):** Khung chat widget mờ kính Glassmorphism sang trọng, bong bóng chat hiển thị nhanh chóng kết hợp hoạt ảnh gõ chữ (Typing Indicator) tạo cảm giác trò chuyện tự nhiên như người thật.
