# 🛎️ CỔNG LỄ TÂN & ĐIỀU PHỐI HÀNG ĐỢI KHÁM (RECEPTIONIST PORTAL & QUEUE MANAGEMENT)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 2 - FEATURE 13)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Cổng Lễ Tân & Điều phối Hàng đợi khám (Receptionist Portal & Queue Management)** trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu nghiệp vụ tiếp đón sảnh, chân dung lễ tân & khách vãng lai (Walk-in), User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ WebSockets SignalR Hub, Sequence Diagrams cho quét QR check-in & walk-in, database schema mở rộng `Appointments`, SQL Sequence y tế, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Thuật toán cân bằng tải bác sĩ khám `SuggestOptimalRoomAsync`, logic check-in Thread-safe (SemaphoreSlim), bộ tạo số thứ tự tự động `Q-XXX`. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế Dashboard Lễ tân, sảnh Tivi công cộng, Quick Form Walk-in, CSS HSL variables, các hoạt ảnh micro-animations (pulse badge, drag card style). | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`receptionistQueueStore`) quản lý Kanban 3 cột, khởi tạo SignalR WebSockets lắng nghe sự kiện Server realtime. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò Lễ tân/Admin `[Authorize(Roles = "receptionist,admin")]`, SignalR CORS Policy, SQL Index tối ưu hóa I/O, và ghi nhật ký hoạt động (Audit Logging). | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho các cổng lấy hàng đợi hôm nay, check-in, walk-in, và cập nhật Kanban status kèm payloads JSON mẫu và HTTP Status. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối luồng barcode check-in, popover chọn phòng, kéo thả Kanban, và rollback UI khi lỗi. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm từ quét QR không chạm, thẻ phân loại loài pet (Chó/Mèo) trực quan, đến hiệu ứng âm thanh gọi khám Tivi. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn cấu hình đầu đọc Barcode Scanner USB HID, hướng dẫn cấu hình Nginx/Cloudflare WebSockets, command curl test API, và khắc phục lỗi lệch múi giờ in phiếu. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 4 giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm tra tranh chấp sinh số thứ tự (Concurrency), và code kiểm thử mẫu xUnit. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng tiếp đón lễ tân và điều phối hàng đợi khám sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Tiếp nhận Siêu tốc & Không chạm:** Hỗ trợ quét mã QR bằng máy barcode USB HID trong vòng dưới 1 giây để hoàn tất check-in, tự động sinh số thứ tự và in phiếu khám.
2. **Cập nhật Real-time đồng bộ:** Trạng thái hàng đợi Kanban và Tivi sảnh chờ đồng bộ ngay lập tức dưới 500ms khi bác sĩ/lễ tân thực hiện thay đổi qua SignalR WebSockets.
3. **Thread-safe & Chống trùng Số thứ tự:** Áp dụng Semaphore và database locking bi quan bảo đảm không bao giờ cấp trùng số thứ tự khám trong ngày kể cả khi có nhiều quầy check-in đồng thời.
4. **Cân bằng tải y tế:** Tự động thống kê số ca khám để đề xuất phòng khám trống nhất, tối ưu thời gian chờ đợi trung bình của chủ nuôi dưới 15 phút/ca.
