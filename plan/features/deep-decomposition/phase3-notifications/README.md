# 🔔 NHẮC LỊCH TIÊM PHÒNG & THÔNG BÁO TỰ ĐỘNG (AUTOMATIC VACCINE REMINDERS & NOTIFICATIONS)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 3 - FEATURE 22)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Nhắc lịch tiêm phòng & Thông báo tự động (Automatic Vaccine Reminders & Notifications)** của Khách hàng và Lễ tân trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Tầm nhìn chăm sóc y tế chủ động, chân dung khách hàng và lễ tân trực ban, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence Quartz.NET chạy nền quét lịch tiêm & SignalR đẩy tin thời gian thực, thiết kế database bảng thông báo. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | C# Code mẫu triển khai Quartz.NET Job `VaccinationReminderJob`, logic tìm kiếm thú cưng cận ngày tái chủng, và SignalR Hub. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế Chuông thông báo in-app nổi HSL, Email mẫu tái chủng K80/HTML mờ kính, và hoạt ảnh rung nhẹ. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`useNotificationStore`) kết nối WebSockets SignalR và quản lý tin tức in-app. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Phân quyền an toàn thông báo, bảo mật chống IDOR đối chiếu JWT, cấu hình SMTP MailKit và CORS SignalR. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho GET danh sách thông báo, PUT đánh dấu đã đọc kèm JSON payload mẫu và Http status. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối trạng thái của thông báo in-app (`Unread` -> `Read`). | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng hành trình người dùng nhận Email nhắc tái chủng, click đặt lịch khám, đến khi lễ tân duyệt lịch nhận đẩy realtime. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn cấu hình Quartz.NET cron trigger, hướng dẫn vận hành Email Template HTML, cURL commands test API. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình phát triển nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm tra Quartz/SignalR, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Hệ thống thông báo và nhắc lịch sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Chạy Ngầm Tự Động Chính Xác (Quartz.NET Reliability):** Tiến trình quét database vào lúc 08:00 sáng hàng ngày phải diễn ra chính xác, tìm đúng các bản ghi thú cưng có ngày tái chủng tiếp theo cách hiện tại đúng 3 hoặc 5 ngày để gửi email.
2. **Thông Báo Đẩy Thời Gian Thực (SignalR Realtime under 100ms):** Khi Lễ tân phê duyệt hoặc hủy lịch hẹn, thông báo in-app phải được đẩy thẳng lên màn hình khách hàng tương ứng trong thời gian dưới 100ms thông qua kết nối WebSocket SignalR Hub.
3. **Bảo Mật Quyền Sở Hữu Tuyệt Đối (Anti-IDOR):** Khách hàng tuyệt đối không thể gọi các API để đọc thông báo của người khác hoặc tự đánh dấu đã đọc các thông báo không thuộc quyền sở hữu của mình.
4. **Email Cá Nhân Hóa Đẹp Mắt:** Email gửi đi phải định dạng HTML đẹp mắt, tự động chèn thông tin tên chủ nuôi, tên thú cưng, loại vắc-xin và chèn link đặt lịch nhanh để tăng tỷ lệ quay lại của khách hàng.
