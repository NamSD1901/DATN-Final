# 🚀 HỒ SƠ CÁ NHÂN & CẬP NHẬT THÔNG TIN (PROFILE DETAILS UPDATE)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - FEATURE 07)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Xem và Cập nhật hồ sơ thông tin cá nhân** của người dùng trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Đặc tả mục tiêu kinh doanh, chân dung người dùng (User Personas), câu chuyện người dùng (User Stories) kèm tiêu chí nghiệm thu (Acceptance Criteria), phạm vi MVP và các yêu cầu phi chức năng (NFRs). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Đặc tả kiến trúc kỹ thuật bao gồm sơ đồ tuần tự (Sequence Diagram) tương tác tổng quát giữa SPA Client, Web API, Middleware, Service và Database PostgreSQL; cấu trúc database schema và cấu trúc dữ liệu đầu vào. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Logic giải mã token claims, xử lý phòng chống IDOR, thuật toán cập nhật dữ liệu và chuẩn hóa định dạng thời gian UTC cho PostgreSQL (Npgsql Exception Handling). | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế form Glassmorphic, sơ đồ ASCII Mockup hoàn chỉnh cho cả hai phiên bản Desktop và Mobile, bảng màu CSS HSL Tokens và các hiệu ứng micro-animations. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store bằng TypeScript quản lý trạng thái tải dữ liệu, cập nhật đệm, cơ chế Dirty Checking (`isDirty`) và khôi phục form (`resetForm`). | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cấu hình hạ tầng phân quyền Middleware `[Authorize]`, cơ chế phòng chống tấn công IDOR, chính sách Rate Limiting (chống DoS/DDoS API) và bảo mật dữ liệu PII theo GDPR. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Tài liệu đặc tả hợp đồng API (API Contracts) chi tiết cho các phương thức GET/PUT profile và PUT đổi mật khẩu kèm mẫu dữ liệu JSON thành công/thất bại chi tiết. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) mô tả vòng đời của form chỉnh sửa bằng Mermaid và cơ chế chặn chuyển trang Vue Router Guard. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm đi qua các màn hình từ lúc bắt đầu tải trang (Skeleton Loader) cho đến khi cập nhật thành công (Toast notify). | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành nhanh dành cho người dùng cuối và các công cụ phát triển nhanh (lệnh `curl` debug API) kèm xử lý lỗi thường gặp. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm thử biên, bảo mật IDOR và Rate Limiting. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng cập nhật hồ sơ cá nhân sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1.  **Bảo mật IDOR tuyệt đối:** Giải mã ID người dùng duy nhất từ token đã được ký điện tử bảo mật, loại bỏ hoàn toàn khả năng người dùng thay đổi thông tin chéo của nhau.
2.  **Trải nghiệm mượt mà:** Phản hồi giao diện tức thì (<300ms), áp dụng Skeleton Loading để tối ưu hiệu ứng hiển thị, cảnh báo thông minh bằng Popup khi người dùng cố tình chuyển trang mà chưa lưu thay đổi (Dirty form state).
3.  **Toàn vẹn dữ liệu:** Toàn bộ đầu vào số điện thoại phải được validate theo chuẩn định dạng nhà mạng Việt Nam di động chính quy trước khi ghi vào cơ sở dữ liệu. Xử lý chuẩn hóa UTC tránh lỗi múi giờ PostgreSQL.
