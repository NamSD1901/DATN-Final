# 👥 QUẢN TRỊ NHÂN SỰ & PHÂN QUYỀN (ADMIN STAFF & ROLE AUTHORIZATION)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 3 - FEATURE 23)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Quản trị Nhân sự & Phân quyền (Admin Staff & Role Authorization)** của Quản trị viên (Admin) trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu an ninh nhân sự, chân dung quản trị viên trực ban, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence tạo nhân viên & đổi quyền, sơ đồ CSDL thực thể người dùng `Users` mở rộng các trường trạng thái, SQL scripts, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Thuật toán mã hóa băm mật khẩu, logic sinh mật khẩu tạm thời an toàn bằng RNG, ngăn chặn tự xóa tài khoản Admin chính mình. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế workspace quản lý nhân sự mờ kính CSS HSL, ASCII Mockups danh sách nhân viên và popup thêm mới. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`useAdminStaffStore`) quản lý danh sách nhân sự, bộ lọc tìm kiếm và các action đổi quyền/khóa. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền nghiêm ngặt vai trò Admin `[Authorize(Roles = "admin")]`, ngăn chặn privilege escalation, Audit Logging lưu vết thao tác. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho thêm, sửa, đổi vai trò, khóa tài khoản nhân viên kèm payloads JSON mẫu và HTTP Status. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối trạng thái tài khoản (`PendingActivation` -> `Active` -> `Suspended` / `Deactivated`). | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm từ lúc tuyển nhân sự mới, phân quyền phòng ban, đến khi kích hoạt hoặc khóa tài khoản. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành giao diện quản trị, hướng dẫn lập trình phân quyền frontend, command curl test API, và debug phân quyền. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai Giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm soát quyền hạn nâng cấp, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Hệ thống quản lý nhân sự sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Bảo Mật Phân Quyền Tuyệt Đối (No Privilege Escalation):** Chặn đứng hoàn toàn mọi nỗ lực nâng quyền trái phép. Chỉ duy nhất tài khoản có vai trò `admin` đã được ký mã hóa trong token JWT mới được phép gọi các API cấu hình nhân sự.
2. **Audit Logging Minh Bạch:** Ghi nhận lại toàn bộ nhật ký hệ thống (Ai đã làm gì, thời gian nào, tác động lên tài khoản nhân viên nào) khi có các thao tác nhạy cảm như đổi quyền (`Change Role`), khóa tài khoản (`Suspend Account`) hoặc cấp lại mật khẩu.
3. **Mật Khẩu Mặc Định An Toàn:** Quy trình khởi tạo tài khoản nhân viên mới sẽ sinh mật khẩu ngẫu nhiên có độ dài tối thiểu 12 ký tự gồm chữ hoa, chữ thường, số và ký tự đặc biệt, đồng thời kích hoạt trạng thái bắt buộc đổi mật khẩu ở lần đăng nhập tiếp theo.
4. **Phục Hồi Trạng Thái Nhanh Chóng:** Chức năng khóa và mở khóa tài khoản nhân sự phải có hiệu lực tức thời (Invalidate cache session / JWT blacklist) trong dưới 1 giây để bảo vệ thông tin phòng khám khi nhân viên thôi việc.
