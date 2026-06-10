# 🚀 ĐẶT LỊCH TIÊM PHÒNG TRỰC TUYẾN (ONLINE VACCINATION BOOKING)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 2 - FEATURE 11)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Đặt lịch tiêm phòng vắc-xin trực tuyến (Online Vaccination Booking)** của khách hàng trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu nghiệp vụ tiêm chủng định kỳ, chân dung khách hàng & bác sĩ thú y, các User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence tương tác đặt lịch tiêm chủng, cấu trúc thực thể DB mở rộng `Medicine` và bảng mới `VaccinationRecords`, tối ưu index, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Logic thuật toán kiểm tra phác đồ tiêm y tế `VaccinationScheduleChecker`, chống trùng lịch bác sĩ trực ca, tự động cập nhật trạng thái y khoa và cơ chế check-in bằng mã QR. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế biểu mẫu đa bước (Multi-step Wizard) CSS Glassmorphism mờ kính sang trọng, bảng mã màu HSL CSS Tokens, ASCII Mockups cho bước chọn vắc-xin và timeline lịch sử. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`vaccinationBookingStore`) quản lý lưu trữ dữ liệu Wizard tạm thời, tải vắc-xin còn hàng, lịch sử tiêm và gọi API validate khoảng cách tiêm. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò Customer `[Authorize(Roles = "customer")]`, giải pháp chống IDOR chéo thú cưng/lịch sử tiêm phòng, SQL Index tối ưu và Rate Limiting bảo vệ API. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho các cổng tra cứu vắc-xin, xem lịch sử tiêm, validate phác đồ và tạo lịch đặt tiêm phòng kèm mẫu dữ liệu JSON trả về (200 OK / 400 Bad Request). | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối luồng biểu mẫu, kiểm tra tính hợp lệ chuyển bước và luồng bỏ qua cảnh báo y khoa khi khách đồng ý (bypassWarning). | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm từ chọn thú cưng, lọc vắc-xin tự động theo loài, hiển thị timeline lịch sử nhanh đến phản hồi trực quan của Alert Card cảnh báo y tế. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành đặt lịch cho chủ nuôi và tiếp nhận cho lễ tân/bác sĩ, hướng dẫn tích hợp database, seed dữ liệu mẫu và câu lệnh `curl` test API, hướng dẫn xử lý múi giờ. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 4 giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm tra biên phác đồ tiêm y tế, bảo mật IDOR và race condition hết thuốc. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng đặt lịch tiêm phòng vắc-xin trực tuyến sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Bảo đảm Phác đồ Y tế An toàn:** Chặn đứng 100% các yêu cầu tiêm vắc-xin không đúng chủng loài (ví dụ: vắc-xin mèo tiêm cho chó) và đưa ra cảnh báo bắt buộc khi khoảng cách giữa các mũi tiêm nhỏ hơn thời gian an toàn quy định.
2. **Kiểm soát Tồn kho Thời gian thực:** Chỉ cho phép khách hàng đặt lịch tiêm khi loại vắc-xin đó thực tế còn hàng khả dụng tại kho dược của clinic (`StockQuantity > 0`).
3. **Chống tấn công IDOR chéo:** Bắt buộc đối chiếu quyền sở hữu thú cưng của khách hàng đang đăng nhập trước khi cho phép xem lịch sử tiêm phòng hoặc tạo lịch tiêm mới.
4. **Trải nghiệm Glassmorphic Premium:** Giao diện SPA mượt mà, sử dụng form wizard 4 bước kết hợp timeline lịch sử tiêm và thông báo cảnh báo phác đồ y tế mờ kính sang trọng, hỗ trợ responsive hoàn hảo.
