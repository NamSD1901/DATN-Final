# 📦 QUẢN LÝ KHO THUỐC & VẬT TƯ (ADMIN DRUG & MEDICAL SUPPLIES INVENTORY)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 3 - FEATURE 24)

Thư mục này chứa toàn bộ hệ thống tài liệu khảo sát nghiệp vụ và đặc tả thiết kế kỹ thuật chi tiết dành cho tính năng **Quản lý Kho thuốc & Vật tư (Admin Drug & Medical Supplies Inventory)** của Quản trị viên (Admin) trong hệ thống quản lý phòng khám **MyPetClinic**.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU LIÊN QUAN (MASTER INDEX)

Dưới đây là bảng chỉ mục liên kết nhanh đến từng cấu phần tài liệu đặc tả chi tiết. Vui lòng click vào các liên kết dưới đây để xem thông tin chi tiết:

| STT | Tài liệu đặc tả | Mô tả nội dung chính | Liên kết tài liệu |
| :--- | :--- | :--- | :--- |
| 1 | **Product Requirements Document (PRD)** | Tầm nhìn kiểm kho, chân dung người dùng (kiểm kho/bác sĩ), User Stories chi tiết kèm tiêu chí nghiệm thu (AC), phạm vi In/Out-Scope và yêu cầu phi chức năng (NFR). | **[Đọc PRD.md](./PRD.md)** |
| 2 | **Technical Specification** | Sơ đồ Sequence nhập kho & trừ kho đồng thời, sơ đồ CSDL thực thể thuốc `Medicines` và lô hàng `InventoryBatches`, SQL scripts, DTOs validation. | **[Đọc TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md)** |
| 3 | **Core Business Logic Reference** | Thuật toán trừ kho an toàn kiểm tra tồn khả dụng, thuật toán phát hiện lô thuốc sắp hết hạn (Expiry Detection), và code C# xử lý giao dịch ACID. | **[Đọc 01-core-logic.md](./01-core-logic.md)** |
| 4 | **UI/UX Design Specification** | Layout thiết kế workspace quản trị kho mờ kính CSS HSL, ASCII Mockups danh sách thuốc và popup nhập kho kèm pulse animation cảnh báo. | **[Đọc 02-ui-ux.md](./02-ui-ux.md)** |
| 5 | **State Management (Pinia Store)** | Đặc tả mã nguồn Vue 3 Pinia Store TypeScript (`useInventoryStore`) quản lý danh sách thuốc, bộ lọc theo cảnh báo hết hàng/hết hạn. | **[Đọc 03-state-management.md](./03-state-management.md)** |
| 6 | **Infrastructure & Security** | Phân quyền vai trò Admin/Doctor, an toàn đồng thời (concurrency control) bằng Row Versioning chặn race-condition, và Rate Limiting. | **[Đọc 04-infrastructure.md](./04-infrastructure.md)** |
| 7 | **API Reference Details** | Đặc tả API Contracts chi tiết cho CRUD thuốc, nhập lô hàng mới, lấy danh sách cảnh báo tồn kho/hạn sử dụng kèm payloads JSON mẫu. | **[Đọc API_REFERENCE.md](./API_REFERENCE.md)** |
| 8 | **Behavioral Specification** | Biểu đồ máy trạng thái hữu hạn (FSM) bằng Mermaid điều phối trạng thái của một lô thuốc (`InTransit` -> `InStock` -> `LowStock` / `Expired`). | **[Đọc BEHAVIOR_SPEC.md](./BEHAVIOR_SPEC.md)** |
| 9 | **UX Flow & Interactions** | Luồng trải nghiệm của Admin từ khi phát hiện cảnh báo Low Stock/Expired, lập phiếu nhập kho, đến khi thuốc được cập nhật số lượng. | **[Đọc UX_FLOW.md](./UX_FLOW.md)** |
| 10 | **User & Developer Documentation** | Hướng dẫn vận hành kiểm kê kho cuối tháng, cách thiết lập định mức cảnh báo tồn kho tối thiểu, cURL commands test API. | **[Đọc DOCUMENTATION.md](./DOCUMENTATION.md)** |
| 11 | **Implementation Plan & Test Strategy** | Lộ trình triển khai Giai đoạn chi tiết (Micro-roadmap) và các kịch bản kiểm thử (Test Cases) kiểm tra concurrency trừ kho, xUnit code mẫu. | **[Đọc plan.md](./plan.md)** |

---

## 🎯 TÓM TẮT MỤC TIÊU & CHỈ TIÊU CHẤT LƯỢNG (QUALITY CRITERIA)

Hệ thống quản lý kho thuốc và vật tư sau khi nâng cấp phải bảo đảm đạt các chỉ tiêu chất lượng nghiêm ngặt của MyPetClinic:
1. **Kiểm Soát Tồn Kho Chính Xác 100% (Pessimistic Concurrency):** Đảm bảo không xảy ra hiện tượng bán âm kho hoặc bán trùng số lượng khả dụng khi nhiều bác sĩ cùng lúc kê một loại thuốc hiếm thông qua cơ chế khóa dòng Postgres (`FOR UPDATE`).
2. **Hệ Thống Cảnh Báo Chủ Động (Low Stock Alert):** Tự động phát hiện và đánh dấu trực quan bằng nhãn đỏ phát sáng các loại biệt dược có số lượng tồn kho giảm xuống dưới định mức tối thiểu đã cấu hình.
3. **Quản Lý Hạn Dùng Nghiêm Ngặt (First Expired First Out - FEFO):** Ưu tiên xuất các lô thuốc có hạn sử dụng gần hơn trước, đồng thời phát cảnh báo tự động trước 30 ngày đối với các lô thuốc sắp hết hạn sử dụng.
4. **Đồng Bộ Dữ Liệu Thời Gian Thực:** Số lượng thuốc trong kho khả dụng hiển thị trên giao diện của Bác sĩ thú y phải được đồng bộ hóa tức thời dưới 150ms sau khi Admin hoàn tất giao dịch nhập kho mới.
