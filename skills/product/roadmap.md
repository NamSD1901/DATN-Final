# 📅 Lộ Trình Học Tập & Đánh Giá Product, BA, QA (MyPetClinic)

Tài liệu này cung cấp kế hoạch hành động theo tuần và các tiêu chí đánh giá năng lực rõ ràng cho vai trò Product Owner, Business Analyst và QA Tester của dự án MyPetClinic.

---

## 📅 Lộ Trình Phát Triển (Timeline 12 Tuần)

### 🟢 Tuần 1 - 2: Foundation (Nền Tảng)
* **Mục tiêu:** Thấu hiểu người dùng phòng khám và viết yêu cầu chuẩn xác.
* **Hoạt động:**
  - Nghiên cứu Personas và Customer Journeys tại [foundation.md](file:///e:/DATN/MyPetClinic/skills/product/foundation.md).
  - Viết 3 User Stories hoàn chỉnh cho luồng đặt lịch khám kèm theo Acceptance Criteria chuẩn Gherkin (`Given-When-Then`).
  - Thực hành Manual Test (Giao diện & Chức năng) và báo cáo lỗi (Bug Report).

### 🟡 Tuần 3 - 6: Core (Cốt Lõi)
* **Mục tiêu:** Làm chủ quy trình nghiệp vụ thú y, kỹ thuật thiết kế test case và quản lý backlog.
* **Hoạt động:**
  - Mô hình hóa quy trình khám chữa bệnh thực tế bằng sơ đồ phân làn BPMN tại [core.md](file:///e:/DATN/MyPetClinic/skills/product/core.md).
  - Thiết kế Test Cases áp dụng kỹ thuật Phân tích giá trị biên (cho cân nặng pet, số lượng thuốc) và Chuyển đổi trạng thái (cho vòng đời lịch hẹn).
  - Thực hành chấm điểm ưu tiên backlog bằng RICE và phân nhóm theo MoSCoW.

### 🟠 Tuần 7 - 10: Advanced (Nâng Cao)
* **Mục tiêu:** Kiểm thử tích hợp thông qua APIs và cơ sở dữ liệu PostgreSQL.
* **Hoạt động:**
  - Sử dụng Postman / Swagger UI để kiểm thử tính hợp lệ của APIs tại [advanced.md](file:///e:/DATN/MyPetClinic/skills/product/advanced.md).
  - Viết các câu lệnh SQL kiểm tra sự thay đổi dữ liệu trong database sau khi thực thi các ca test.
  - Lập kế hoạch nghiệm thu người dùng (UAT Plan) cho hai đối tượng: Lễ tân và Bác sĩ thú y.

### 🔴 Tuần 11 - 12: Expert (Chuyên Gia)
* **Mục tiêu:** Tự động hóa kiểm thử và xây dựng chiến lược phát hành sản phẩm.
* **Hoạt động:**
  - Viết kịch bản test tự động e2e bằng Playwright hoặc Cypress cho luồng đăng ký tài khoản -> đăng ký pet -> đặt lịch khám thành công tại [expert.md](file:///e:/DATN/MyPetClinic/skills/product/expert.md).
  - Biên soạn tài liệu Release Notes và chuẩn bị bài hướng dẫn sử dụng phần mềm (User Manual) bàn giao cho phòng khám thú y.

---

## 🏆 Tiêu Chí Đánh Giá Hoàn Thành

Dưới đây là các tiêu chí nghiệm thu năng lực của thành viên đảm nhiệm vai trò Product/BA/QA:

| Cấp độ | Tiêu chí kỹ thuật bắt buộc | Phương thức đánh giá |
|:---|:---|:---|
| **Foundation (Nền tảng)** | - Viết User Story tuân thủ đúng định dạng chuẩn Agile.<br>- Viết Acceptance Criteria rõ ràng, dễ test dưới dạng Given-When-Then.<br>- Viết Bug Report có đầy đủ các bước tái hiện lỗi (Steps to Reproduce) và kết quả mong đợi. | Review 1-1 & Sửa tài liệu trực tiếp |
| **Core (Cốt lõi)** | - Vẽ được sơ đồ BPMN chuẩn phân làn cho các chức năng trong phòng khám.<br>- Thiết kế Test Cases bao phủ được các giá trị biên của đầu vào và các trường hợp lỗi logic nghiệp vụ.<br>- Thực hiện phân bổ độ ưu tiên backlog thuyết phục dựa trên điểm số RICE. | Thuyết trình tại buổi làm việc nhóm |
| **Advanced (Nâng cao)** | - Kiểm thử độc lập API qua Swagger/Postman mà không cần Frontend.<br>- Sử dụng thành thạo các câu lệnh SELECT, JOIN, WHERE trong PostgreSQL để kiểm chứng dữ liệu.<br>- Xây dựng được bộ kịch bản UAT hoàn chỉnh cho nhân viên phòng khám sử dụng thử. | Chạy Demo kiểm thử trực tiếp trên DB & APIs |
| **Expert (Chuyên gia)** | - Viết và chạy thành công script Playwright kiểm thử tự động luồng Đăng nhập & Đặt lịch chính.<br>- Quản lý quy trình phát hành phiên bản mới, viết Release Notes ngắn gọn, dễ hiểu.<br>- Có kế hoạch cụ thể cho việc huấn luyện người dùng và phát triển tính năng AI. | Đánh giá qua mã nguồn Automation test & Sự ổn định của bản Build Release |
