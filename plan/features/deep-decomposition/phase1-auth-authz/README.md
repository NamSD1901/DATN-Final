# 🚀 KIẾN TRÚC XÁC THỰC & PHÂN QUYỀN (AUTHENTICATION & AUTHORIZATION)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - FEATURE 00)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho phân hệ **Xác thực danh tính, OTP Email, Đăng nhập Google và Phân quyền vai trò (RBAC)** trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu bảo mật hạt nhân, chân dung người dùng khách hàng & nhân viên trực ban, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi MVP và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence luồng xác thực login/Google OAuth, cấu trúc Token Claims, quan hệ cơ sở dữ liệu `Users` & `Roles`, danh sách endpoints. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Logic băm mật khẩu BCrypt với factor 11, thuật toán sinh khóa token JWT an toàn, sinh mã OTP 6 số bằng RNG mật mã và middleware phân quyền route. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Thiết kế biểu mẫu mờ kính Glassmorphism, sơ đồ ASCII Mockup cho màn hình Login và màn hình nhập OTP kích hoạt, bảng màu CSS HSL Tokens và hoạt động chuyển form. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`authStore`) quản lý lưu trữ JWT Token, đồng bộ header Axios và đăng nhập một chạm Google. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cấu hình mã hóa kết nối HTTPS/HSTS, chính sách CORS nghiêm ngặt, Cookie Security Flags (HttpOnly, SameSite, Secure) và bộ giới hạn tần suất API. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho các cổng xác thực, đăng ký, quên/đặt lại mật khẩu kèm mẫu dữ liệu JSON trả về trong các trường hợp thành công/lỗi. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Sơ đồ FSM Mermaid điều phối toàn bộ phiên làm việc của người dùng, cơ chế tự động chuyển ô nhập OTP và Axios Interceptor bắt token hết hạn. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm tương tác từ Landing Page, cảnh báo lỗi và phân quyền chuyển hướng Dashboard động. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn sử dụng cho khách hàng/nhân viên, hướng dẫn debug bằng lệnh `curl` và cẩm nang sửa lỗi CORS/401 thường gặp. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 3 giai đoạn nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm tra khóa tài khoản lockout và kiểm thử XSS/CSRF. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Hệ thống xác thực và phân quyền sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1.  **Bảo mật thông tin tối thượng:** Mật khẩu bắt buộc được băm bằng thuật toán chống brute-force BCrypt, token JWT được bảo vệ chống XSS bằng cờ cookie HttpOnly và truyền HTTPS mã hóa hoàn toàn.
2.  **Khóa brute-force tự động (Lockout Policy):** Chặn đứng nguy cơ dò quét mật khẩu bằng cách khóa tài khoản 15 phút ngay khi nhập sai mật khẩu quá 5 lần liên tiếp.
3.  **Hàng rào phân quyền Route (RBAC):** Chặn đứng các nguy cơ truy cập chéo dữ liệu bằng Middleware Backend kết hợp Route Guards Frontend, điều hướng người dùng chính xác vào Dashboard riêng theo phân vai.
