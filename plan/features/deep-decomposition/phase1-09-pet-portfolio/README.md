# 🚀 QUẢN LÝ HỒ SƠ THÚ CƯNG (PET PORTFOLIO MANAGEMENT)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - FEATURE 09)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Quản lý Hồ sơ Thú cưng (Pet Portfolio)** của khách hàng trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Đặc tả mục tiêu nghiệp vụ y tế, chân dung khách hàng & bác sĩ, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi MVP và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence luồng xử lý CRUD, đặc tả cấu trúc bảng cơ sở dữ liệu `Pets` trong PostgreSQL, tối ưu index ngoại khóa và cấu trúc DTOs đầu vào. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Logic chặn đứng lỗi bảo mật IDOR tại tầng dịch vụ, thuật toán xóa mềm (Soft Delete) bảo toàn liên kết bệnh án và mã nguồn helper tính tuổi thú cưng. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế thẻ Grid Glassmorphism, sơ đồ ASCII Mockup cho màn hình Portfolio và Modal Form thêm/sửa, bảng màu CSS HSL Tokens và hoạt ảnh Enter/Exit. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`usePetsStore`) xử lý lưu đệm danh sách thú cưng, cập nhật reactive cục bộ và quản lý trạng thái active. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò `[Authorize(Roles = "customer")]`, giải pháp chống tấn công IDOR dò quét dữ liệu, câu lệnh SQL tạo index và Rate Limiting. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho các cổng giao tiếp GET/POST/PUT/DELETE thú cưng kèm mẫu dữ liệu JSON trả về trong các trường hợp thành công/lỗi. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Sơ đồ FSM Mermaid điều phối vòng đời của lưới grid, modal và cơ chế khóa tương tác nút bấm (submit lock) trong lúc gọi API đồng bộ. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm tương tác từ màn hình trống (Empty state) đến các hiệu ứng trượt thẻ trực quan. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn sử dụng chi tiết dành cho chủ nuôi, hướng dẫn debug API bằng lệnh `curl` và xử lý sự cố CORS/sai số thập phân trọng lượng. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 3 giai đoạn nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm tra biên nghiệp vụ và kiểm thử bảo mật IDOR. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng quản lý hồ sơ thú cưng sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1.  **Bảo vệ dữ liệu y tế chủ nuôi (Chống IDOR):** Mọi request sửa hoặc xóa đều bắt buộc đối chiếu ID chủ sở hữu thực sự trong token claims của JWT, ngăn chặn hoàn toàn việc can thiệp trái phép.
2.  **Bảo toàn tính toàn vẹn (Xóa mềm):** Áp dụng cờ xóa mềm `IsDeleted = true` để lưu giữ lịch sử bệnh án lâm sàng và dữ liệu hóa đơn tài chính phòng khám.
3.  **Trải nghiệm người dùng thông minh:** Tự động tính toán tuổi thú cưng chính xác theo định dạng "năm + tháng" hoặc "ngày" hỗ trợ bác sĩ kê toa thuốc; tự động hiển thị avatar thông minh theo Loài khi chưa có ảnh tải lên.
