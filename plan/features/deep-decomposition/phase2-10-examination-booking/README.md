# 🚀 ĐẶT LỊCH KHÁM BỆNH TRỰC TUYẾN (ONLINE EXAMINATION BOOKING)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 2 - FEATURE 10)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Đặt lịch khám bệnh trực tuyến (Online Examination Booking)** của khách hàng trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu nghiệp vụ cốt lõi, chân dung khách hàng & bác sĩ thú y, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi MVP và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence luồng tương tác đặt lịch, cấu trúc thực thể database schema `Appointment`, tối ưu index chống trùng lịch và cấu trúc DTOs đầu vào. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Logic kiểm tra trùng lịch Bác sĩ trong khoảng $\pm30$ phút, thuật toán tự động phân bổ bác sĩ trực, phân loại trạng thái y tế và sinh mã QR Token Check-in. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế biểu mẫu đa bước (Multi-step Wizard), sơ đồ ASCII Mockup cho màn hình đặt lịch và grid chọn giờ rảnh, bảng màu CSS HSL Tokens và các hiệu ứng. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`bookingStore`) quản lý lưu trữ dữ liệu Wizard tạm thời, tải danh mục dịch vụ/bác sĩ và gửi request. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò Customer `[Authorize(Roles = "customer")]`, giải pháp chống tấn công IDOR chéo thú cưng, SQL Index tối ưu hóa và Rate Limiting. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho các cổng tạo lịch, xem danh sách và hủy lịch khám kèm mẫu dữ liệu JSON trả về trong các trường hợp thành công/lỗi. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối luồng biểu mẫu, kiểm tra tính hợp lệ chuyển bước và cơ chế khóa gửi form (Submit Lockout). | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm tương tác từ chọn thú cưng, gợi ý triệu chứng nhanh (Quick Tags) đến hiển thị QR code sau khi đặt. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn đặt lịch cho chủ nuôi, hướng dẫn debug bằng lệnh `curl` và cẩm nang sửa lỗi trùng lịch/lệch múi giờ hiển thị (UTC vs Local Time). | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 3 giai đoạn nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm tra biên trùng lịch bác sĩ và bảo mật IDOR. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng đặt lịch khám bệnh trực tuyến sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1.  **Chống trùng lịch bác sĩ tuyệt đối:** Chặn đứng 100% các request đặt lịch khám trùng ca làm việc của cùng một bác sĩ điều trị trong phạm vi $\pm30$ phút.
2.  **Chống IDOR chéo:** Bắt buộc đối chiếu quyền sở hữu thú cưng từ token claims trước khi cho phép liên kết lịch hẹn y tế.
3.  **Trải nghiệm biểu mẫu mượt mà:** Sử dụng Wizard Form đa bước, cơ chế submit locking chống gửi đúp dữ liệu và check-in nhanh bằng mã QR Token quét tại quầy lễ tân.
