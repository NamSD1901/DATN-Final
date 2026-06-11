# 🗓️ Sprint 17: Tích Hợp Gemini AI & Tự Động Nhắc Lịch
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Hoàn thành phân hệ tính năng nâng cao thông minh và tự động hóa cuối cùng của Phòng khám:
1. **Gemini AI Advisor Chatbot (T52):** Tích hợp Gemini SDK của Google tạo trợ lý ảo tư vấn trực tuyến kiến thức chăm sóc thú cưng, giải đáp nhanh các thắc mắc về triệu chứng bệnh y tế cơ bản và sơ cứu ban đầu.
2. **Tự Động Nhắc Lịch Tiêm Phòng (T53):** Xây dựng Background Worker (Quartz.NET hoặc HostedService) tự động quét cơ sở dữ liệu định kỳ mỗi ngày để tìm các thú cưng có lịch tiêm vắc-xin tiếp theo (`NextDueDate`) cách hiện tại 3 ngày và tự động gửi Email nhắc lịch cho khách hàng.

---

## 📋 Danh Sách Tasks (Sprint Backlog)

| Task ID | Tên Task | Trách nhiệm | Mô tả chi tiết | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| **T52** | Gemini AI Advisor Chatbot | Backend & Frontend | - API kết nối Gemini SDK Google AI Studio.<br>- Tạo system prompt y tế thú y chuyên nghiệp.<br>- Giao diện Chatbot mờ kính Glassmorphism, hiển thị timeline hội thoại. | `❌ SPEC ONLY` |
| **T53** | Automatic Email Reminders | Backend & Frontend | - Thiết lập Background Worker chạy ngầm mỗi ngày lúc 08:00 AM.<br>- Logic quét các mũi tiêm sắp đến hạn và gọi `EmailService` gửi thông báo tự động. | `❌ SPEC ONLY` |

---

## 🛡️ Tiêu Chí Nghiệm Thu (Definition of Done - DoD)

### 1. Phía Backend (.NET Core)
- [ ] Tích hợp thư viện `Google.GenerativeAI` chính hãng, quản lý API key bảo mật trong `appsettings.json` / Environment Variables.
- [ ] Triển khai `IHostedService` hoặc Quartz.NET Job chạy ngầm không làm gián đoạn luồng xử lý chính của Web API.
- [ ] Gửi Email HTML nhắc lịch thành công với đầy đủ thông tin: Tên chủ nuôi, tên thú cưng, tên vắc-xin và ngày hẹn dự kiến.

### 2. Phía Frontend (Vue 3 / TypeScript)
- [ ] Thiết kế Component bong bóng Chatbot nổi ở góc màn hình (Floating Widget) phong cách Glassmorphism.
- [ ] Tạo Pinia store `useChatStore` quản lý lịch sử trò chuyện trong phiên làm việc của khách hàng.

### 3. Chất Lượng & Kiểm Thử (QA)
- [ ] Viết xUnit Unit Test kiểm tra logic lọc các bản ghi tiêm chủng sắp đến hạn của Background Worker (đảm bảo lọc chính xác khoảng thời gian 3 ngày, bỏ qua các bản ghi đã quá hạn hoặc còn quá xa).
