# 💵 THU NGÂN & LẬP HÓA ĐƠN (CASHIER & INVOICING)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 2 - FEATURE 15)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Thu ngân & Lập hóa đơn (Cashier & Invoicing)** của Lễ tân/Thu ngân trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu tài chính phòng khám, chân dung thu ngân trực ca, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence tạo hóa đơn & thanh toán, sơ đồ CSDL thực thể hóa đơn `Invoices` & `InvoiceItems`, SQL scripts, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Thuật toán tính toán tổng tiền có thuế VAT, C# logic giao dịch ACID cập nhật trạng thái hóa đơn & lịch hẹn, sinh hóa đơn nháp. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế workspace mờ kính CSS HSL, ASCII Mockups màn hình hóa đơn & popup QR Code, cấu hình `@media print` cho máy in nhiệt K80. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`useInvoiceStore`) quản lý danh sách hóa đơn, bộ lọc tìm kiếm và trạng thái thanh toán/in. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò Receptionist/Cashier/Admin, bảo mật IDOR chặn đứng xem lén hóa đơn khách hàng khác, tích hợp VietQR API và Rate Limiting. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho tạo hóa đơn từ ca khám, xem chi tiết hóa đơn, xác nhận thanh toán kèm payloads JSON mẫu và HTTP Status. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối trạng thái hóa đơn (`Draft` -> `Pending` -> `Paid` / `Cancelled`) và rollback. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm từ lúc ca khám hoàn thành, hiển thị cảnh báo chưa thanh toán đến popup in hóa đơn. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành máy in nhiệt K80 phòng khám, cấu hình API VietQR, command curl test API, và khắc phục lỗi lệch số tiền. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 4 giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm tra tính atomic transaction, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Hệ thống quản lý hóa đơn và thanh toán sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Tính Toàn Vẹn Tài Chính 100% (ACID Transaction):** Quy trình cập nhật trạng thái hóa đơn sang `Paid` và đồng bộ trạng thái thanh toán của lịch hẹn khám sang `Paid` phải diễn ra trong một giao dịch cơ sở dữ liệu duy nhất.
2. **QR Code Động Siêu Tốc:** Sinh mã QR VietQR động (chứa chính xác số tài khoản ngân hàng của phòng khám, số tiền và nội dung chuyển khoản tự động) trong dưới 150ms để khách hàng thanh toán chuyển khoản không cần nhập thủ công.
3. **Bảo mật Hóa đơn & Chặn IDOR:** Chặn đứng nguy cơ IDOR bằng cách đối chiếu thông tin hóa đơn với `currentUserId` khi khách hàng xem hóa đơn của họ, đồng thời chỉ cho phép các tài khoản có vai trò `receptionist`, `cashier`, hoặc `admin` thực hiện thao tác quản lý và thanh toán hóa đơn.
4. **In Hóa đơn Chuẩn Biên Lai K80:** Layout in hóa đơn phải hiển thị trực quan, hỗ trợ tự động căn lề thụt lề chuẩn kích thước giấy nhiệt K80 (80mm) khi sử dụng tính năng in trực tiếp từ trình duyệt.
