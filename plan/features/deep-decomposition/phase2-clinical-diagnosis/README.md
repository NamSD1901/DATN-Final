# 🩺 CHẨN ĐOÁN LÂM SÀNG & KÊ ĐƠN ĐIỀU TRỊ (CLINICAL DIAGNOSIS & TREATMENT)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 2 - FEATURE 14)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Chẩn đoán lâm sàng & Kê đơn điều trị (Clinical Diagnosis & Treatment)** của Bác sĩ thú y trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Mục tiêu y học lâm sàng, chân dung bác sĩ thú y trực ca, User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence chẩn đoán & kiểm kho, sơ đồ CSDL thực thể y bạ `MedicalRecords`, SQL scripts, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Thuật toán cảnh báo trùng hoạt chất `VerifyPrescriptionSafetyAsync`, C# logic giao dịch ACID trừ kho bi quan PostgreSQL (`FOR UPDATE`), sinh hóa đơn nháp. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế workspace 3 vùng mờ kính CSS HSL, ASCII Mockups màn hình khám chính, Autocomplete dropdown, các hoạt ảnh slide-in và warning đỏ. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`doctorSessionStore`) quản lý mảng thuốc kê tạm thời, auto-save bản nháp local, và tải lịch sử y tế. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Cơ chế phân quyền vai trò Doctor/Admin `[Authorize(Roles = "doctor,admin")]`, bảo mật IDOR chặn đứng xem lén bệnh sử, SQL Index tối ưu và Rate Limiting. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho tiếp nhận khám, tra cứu bệnh sử, autocomplete thuốc, và lưu bệnh án kèm payloads JSON mẫu và HTTP Status. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối luồng gõ thuốc, check stock, rollback transaction, và phục hồi bản nháp local. | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm người dùng đi qua các điểm chạm từ autocomplete siêu tốc, timeline bệnh sử dọc mờ kính, đến popup đơn thuốc PDF tự động. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành in đơn thuốc PDF, hướng dẫn lập trình frontend debounce autocomplete, command curl test API, và khắc phục lỗi deadlock CSDL. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai 4 giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm tra tính atomic rollback khi hết thuốc, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Màn hình làm việc của bác sĩ và hệ thống quản trị bệnh án sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Tính Toàn Vẹn Kho Dược 100% (ACID Transaction):** Quy trình trừ kho dược và tạo hóa đơn nháp phải diễn ra trong một giao dịch nguyên tử duy nhất. Nếu bất kỳ loại thuốc nào không đủ số lượng tồn kho khả dụng, hệ thống lập tức rollback toàn bộ thay đổi để tránh thất thoát.
2. **An toàn Y khoa Cảnh báo trùng hoạt chất:** Phát hiện và cảnh báo tức thời khi đơn thuốc chứa các biệt dược trùng lặp hoạt chất để bảo vệ sức khỏe thú cưng khỏi rủi ro quá liều.
3. **Bảo mật Bệnh án nghiêm ngặt:** Chặn đứng IDOR bằng cách chỉ cho phép bác sĩ chỉ định được phép truy xuất bệnh lịch chi tiết và ghi chép bệnh án của thú cưng có lịch hẹn hoạt động trong ngày.
4. **Autocomplete Gợi ý Thuốc Siêu tốc:** Phản hồi kết quả tìm kiếm biệt dược dưới 150ms bằng kỹ thuật Debounce và tối ưu hóa PostgreSQL Index, nâng cao hiệu suất khám chữa bệnh.
