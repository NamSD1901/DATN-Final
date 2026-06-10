# 📅 Lộ Trình Học Tập & Đánh Giá QA (MyPetClinic)

Tài liệu này cung cấp lộ trình phát triển kỹ năng kiểm thử theo tuần và các tiêu chí cụ thể để đánh giá sự tiến bộ của QA Tester (Lâm) trong dự án MyPetClinic.

---

## 📅 Lộ Trình Học Tập & Thực Hành (12 Tuần)

### 🟢 Tuần 1 - 2: QA Foundation (Nền Tảng)
* **Mục tiêu:** Nắm vững lý thuyết kiểm thử và thiết kế kịch bản test case thủ công.
* **Hoạt động:**
  - Đọc hiểu Kế hoạch kiểm thử tổng thể (Master Test Plan) tại [foundation.md](file:///e:/DATN/MyPetClinic/skills/quality/foundation.md).
  - Viết 20 Test Cases bao gồm cả Happy Path và Edge Cases cho tính năng Đặt lịch khám (`PB09`).
  - Thực hành Smoke test và tạo báo cáo lỗi (Bug Report) chi tiết khi chạy thử nghiệm trên các trình duyệt.

### 🟡 Tuần 3 - 6: QA Core (Cốt Lõi)
* **Mục tiêu:** Kiểm thử APIs qua Postman và truy vấn cơ sở dữ liệu PostgreSQL.
* **Hoạt động:**
  - Thiết lập các folder, viết script Javascript kiểm định tự động các API chính (Login, Create Booking) trong Postman tại [core.md](file:///e:/DATN/MyPetClinic/skills/quality/core.md).
  - Viết các câu lệnh SQL kiểm tra tính toàn vẹn dữ liệu (không có lịch trùng giờ, tồn kho thuốc không bị âm).
  - Tiến hành Responsive test và điền Checklist nhất quán giao diện trên các thiết bị Mobile/Tablet.

### 🟠 Tuần 7 - 10: QA Advanced (Nâng Cao)
* **Mục tiêu:** Kiểm thử hiệu năng và lập báo cáo chất lượng Sprint.
* **Hoạt động:**
  - Viết script kiểm thử hiệu năng APIs bằng k6 tại [advanced.md](file:///e:/DATN/MyPetClinic/skills/quality/advanced.md).
  - Quản lý vòng đời lỗi và thiết lập tài liệu Sprint Quality Report tổng kết các chỉ số (Defect Density, MTTR).
  - Chuẩn bị tài liệu kịch bản nghiệm thu người dùng (UAT Plan) cho Lễ tân và Bác sĩ thú y.

### 🔴 Tuần 11 - 12: QA Expert (Chuyên Gia)
* **Mục tiêu:** Tự động hóa kiểm thử bằng Playwright và tích hợp CI/CD.
* **Hoạt động:**
  - Viết và chạy thành công script tự động hóa E2E bằng Playwright cho luồng đặt lịch khám chính tại [expert.md](file:///e:/DATN/MyPetClinic/skills/quality/expert.md).
  - Thiết lập file YAML cấu hình chạy Playwright tự động trên GitHub Actions.
  - Phân tích rủi ro và áp dụng kiểm thử hỗn loạn (Chaos Engineering) cơ bản.

---

## 🏆 Tiêu Chí Đánh Giá Hoàn Thành

Dưới đây là các tiêu chí nghiệm thu năng lực kiểm thử của thành viên QA:

| Cấp độ | Tiêu chí kỹ thuật bắt buộc | Phương thức đánh giá |
|:---|:---|:---|
| **Foundation (Nền tảng)** | - Phân loại đúng mức độ nghiêm trọng (Severity) của lỗi phát sinh.<br>- Viết Test Cases bao phủ đầy đủ các giá trị biên của đầu vào.<br>- Viết Bug Report có đầy đủ log console, ảnh chụp màn hình và các bước tái dựng lỗi dễ dàng. | Đánh giá qua tài liệu Test cases & Bug reports |
| **Core (Cốt lõi)** | - Chạy test tự động được toàn bộ API core bằng Postman Runner.<br>- Viết câu lệnh SQL đúng cú pháp JOIN, WHERE để truy vấn đối chiếu DB PostgreSQL.<br>- Kiểm chứng được giao diện hiển thị mượt mà trên Mobile Safari và Chrome không bị vỡ layout. | Chạy thử nghiệm trực tiếp & Code review SQL |
| **Advanced (Nâng cao)** | - Viết và chạy thành công k6 Load test với biểu đồ phản hồi trực quan.<br>- Quản lý và xử lý dứt điểm các bug trong Sprint, hạn chế tối đa tỷ lệ lỗi lọt sang Staging.<br>- Viết báo cáo chất lượng Sprint Quality Report chính xác, đầy đủ chỉ số đo lường. | Đánh giá báo cáo chất lượng & Demo chạy k6 |
| **Expert (Chuyên gia)** | - Viết script Playwright test chạy thành công không có lỗi flaky test (lỗi chập chờn).<br>- Tích hợp thành công GitHub Actions chạy kiểm thử tự động mỗi khi tạo Pull Request.<br>- Đề xuất được các phương án tối ưu hóa quy trình kiểm thử sớm (Shift-left). | Review mã nguồn Playwright & Lịch sử chạy CI/CD |
