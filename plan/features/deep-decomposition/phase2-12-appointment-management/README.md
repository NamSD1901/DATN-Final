# 🚀 QUẢN LÝ LỊCH HẸN CỦA KHÁCH HÀNG (CUSTOMER APPOINTMENT Dashboard)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 2 - FEATURE 12)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Bảng điều khiển quản lý lịch hẹn của Khách hàng (Customer Appointments Dashboard)** trong hệ thống quản lý phòng khám thú y **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu quản lý lịch hẹn y tế, chân dung khách hàng & bác sĩ thú y, các User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence tương tác xem danh sách và hủy lịch, cấu trúc thực thể DB mở rộng của `Appointments`, tối ưu SQL index, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Logic máy trạng thái (State Engine) của lịch hẹn, logic nghiệp vụ ràng buộc thời gian hủy, logic truy vấn lọc phân trang tối ưu Eager Loading. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế Dashboard 2 cột CSS Glassmorphism mờ kính sang trọng, bảng mã màu HSL CSS Tokens, ASCII Mockups cho trang Dashboard và Dialog hủy. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`customerAppointmentsStore`) quản lý lưu trữ danh sách, bộ lọc, trang hiện tại, và action hủy đồng bộ local state. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò Customer `[Authorize(Roles = "customer")]`, giải pháp chống IDOR kiểm tra chéo qua Pets, SQL Index tối ưu và Rate Limiting bảo vệ API. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho các cổng lấy danh sách, xem chi tiết và hủy lịch hẹn kèm mẫu dữ liệu JSON trả về (200 OK / 400 Bad Request / 403 Forbidden). | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối các sự kiện lọc, chuyển trang, Dialog hủy và cơ chế khóa gửi form (Submit Lockout). | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm từ chuyển Tab bộ lọc, timeline vòng đời ca khám, phóng to mã QR check-in, đến thông báo Toast thông minh. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn sử dụng cho khách hàng và tiếp nhận cho lễ tân, hướng dẫn cấu trúc thư mục, command curl test API, hướng dẫn xử lý lệch múi giờ và lỗi Hangfire. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 3 giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) phục vụ QA kiểm tra biên trạng thái, bảo mật IDOR và xUnit test mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Tính năng quản lý lịch hẹn của khách hàng sau khi nâng cấp tài liệu và mã nguồn phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Bảo mật thông tin & Chống IDOR:** Bắt buộc 100% các request GET/PUT liên quan đến lịch hẹn phải đối chiếu trực tiếp quyền sở hữu thú cưng từ token claims.
2. **Quản lý trạng thái chặt chẽ:** Chuyển trạng thái an toàn, thực thi ràng buộc thời gian hủy nghiêm ngặt (chỉ cho hủy trực tuyến trước giờ hẹn tối thiểu 2 tiếng đối với ca khám đã xác nhận) để bảo vệ hiệu quả ca trực bác sĩ.
3. **Hiệu suất tải nhanh:** Sử dụng Eager Loading kết hợp composite index, giảm thiểu N+1 Query. Phân trang thông minh giúp thời gian phản hồi API dưới 100ms.
4. **Đồng bộ Client-side mượt mà:** Sử dụng Pinia Store để cập nhật in-place dữ liệu sau khi hủy thành công mà không cần tải lại toàn bộ trang, mang lại trải nghiệm ứng dụng cao cấp.
