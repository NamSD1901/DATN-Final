# 📈 BÁO CÁO DOANH THU & HIỆU SUẤT (ADMIN REVENUE & PERFORMANCE REPORTS)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 3 - FEATURE 25)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Báo cáo doanh thu & Hiệu suất (Admin Revenue & Performance Reports)** của Quản trị viên (Admin) trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu kinh doanh, chân dung quản lý phòng khám, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence truy xuất báo cáo tài chính, thiết kế tối ưu SQL cho các truy vấn tổng hợp, cấu trúc dữ liệu JSON trả về cho Chart.js. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Thuật toán tính toán phần trăm tăng trưởng so với kỳ trước, logic truy vấn LINQ GroupBy hiệu năng cao trong C# thực thi tại `ReportService.cs`. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế workspace Dashboard mờ kính CSS HSL hiển thị các thẻ KPIs, biểu đồ đường Line Chart và biểu đồ cơ cấu Donut. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`useReportStore`) quản lý bộ lọc thời gian và dữ liệu đồng bộ các biểu đồ. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Phân quyền vai trò Admin nghiêm ngặt, cơ chế cache báo cáo tài chính giảm tải database, Rate Limiting chống spam API. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho KPIs tài chính, doanh thu theo dòng thời gian và cơ cấu dịch vụ kèm payloads JSON mẫu. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối luồng chuyển trạng thái và hiển thị biểu đồ từ lúc tải đến lúc dựng. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng hành trình của Admin từ khi thiết lập khoảng thời gian lọc, cập nhật biểu đồ tương tác, đến khi rê chuột xem tooltip chi tiết. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn xuất dữ liệu báo cáo ra Excel/PDF, hướng dẫn cài đặt Chart.js trên Vue 3, và debug múi giờ UTC trong báo cáo. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình phát triển nhỏ (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm tra aggregate queries, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Hệ thống quản lý báo cáo và hiệu suất sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Truy Vấn Hiệu Năng Cao (PostgreSQL Index Optimization):** Thời gian phản hồi API báo cáo tài chính phải dưới 250ms cho các khoảng thời gian lọc lên tới 1 năm, sử dụng index tổng hợp trên ngày tạo hóa đơn (`Invoices.CreatedAt`) và trạng thái (`Invoices.Status`).
2. **Chính Xác Số Liệu Tài Chính 100%:** Dữ liệu doanh thu hiển thị trên biểu đồ phải trùng khớp hoàn toàn với tổng số tiền thu thực tế trên các hóa đơn đã thanh toán (`Status = 'Paid'`). Chặn không tính các hóa đơn nháp hoặc đã bị hủy.
3. **Bảo Mật Báo Cáo Tài Chính Tuyệt Đối:** Sử dụng bộ lọc Middleware chặn đứng mọi nỗ lực truy cập API báo cáo của các nhân viên không có vai trò `admin`, ngăn chặn nguy cơ rò rỉ thông tin kinh doanh.
4. **Hiển Thị Trực Quan & Responsive:** Hệ thống biểu đồ Chart.js tự động co dãn mượt mà trên các màn hình khác nhau (Responsive Layout), hỗ trợ hiển thị tooltip thông tin chi tiết khi người dùng rê chuột qua các điểm dữ liệu.
