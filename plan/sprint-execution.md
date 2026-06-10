# 📋 Kế Hoạch Thực Thi Sprint - Sprint Execution Plan

Tài liệu này đặc tả quy trình vận hành, phối hợp nhóm, quản trị rủi ro và các mốc thời gian thực thi 6 Sprints phát triển của dự án **MyPetClinic**.

---

## 1. Quy Trình Vận Hành Agile/Scrum Hàng Ngày

Để đảm bảo tiến độ 2 tuần/sprint được tuân thủ nghiêm ngặt, đội ngũ phát triển áp dụng khung làm việc Agile/Scrum rút gọn như sau:

```mermaid
flowchart TD
    A([Khởi đầu Sprint]) --> B[Sprint Planning - Lập kế hoạch Sprint]
    B --> C[Daily Standup - 15 phút đầu ngày]
    C --> D[Phát triển & Kiểm thử liên tục]
    D --> E{Hết 2 tuần?}
    E -- Chưa --> C
    E -- Rồi --> F[Sprint Review - Demo sản phẩm]
    F --> G[Sprint Retrospective - Họp rút kinh nghiệm]
    G --> H([Kết thúc Sprint])
```

### 1.1. Các sự kiện trong Sprint (Sprint Events)
*   **Sprint Planning (Đầu Sprint - 2 giờ):** 
    *   Thống nhất mục tiêu Sprint (Sprint Goal).
    *   Lựa chọn các User Story từ Product Backlog vào Sprint Backlog dựa trên vận tốc làm việc (Velocity) của nhóm.
    *   Phân rã User Story thành các task kỹ thuật cụ thể và gán người thực hiện.
*   **Daily Stand-up (Hàng ngày - 15 phút đầu giờ):**
    *   Trả lời 3 câu hỏi: Hôm qua đã làm gì? Hôm nay sẽ làm gì? Có gặp khó khăn/nút thắt (impediment) nào không?
*   **Sprint Review & Demo (Cuối Sprint - 1 giờ):**
    *   Demo các tính năng đã hoàn thành (đạt chuẩn Definition of Done) cho Product Owner/Giảng viên.
    *   Ghi nhận phản hồi để điều chỉnh backlog nếu cần thiết.
*   **Sprint Retrospective (Cuối Sprint - 1 giờ):**
    *   Nhìn nhận lại quá trình làm việc: Cái gì tốt (Start/Keep)? Cái gì chưa tốt (Stop)? Hành động cải tiến trong Sprint tới (Action items)?

---

## 2. Phân Vai & Phối Hợp Thành Viên (Team Roles & Collaboration)

### 2.1. Phân chia vai trò chính
*   **Nam (Backend Lead / DevOps):** Thiết kế kiến trúc, xây dựng database, quản lý API lõi (Authentication, Booking, Clinical, Payment) và pipeline CI/CD.
*   **Phương (Frontend Lead):** Xây dựng khung giao diện Vue 3 + TS, thiết kế UI/UX đồng bộ, tối ưu hóa responsive và tích hợp API.
*   **Lâm (Frontend Developer / QA):** Phát triển giao diện cổng thông tin khách hàng, cổng lễ tân, viết tài liệu hướng dẫn và thực hiện kiểm thử tích hợp (Integration Test).
*   **Hạnh (Backend Developer / AI Specialist):** Phát triển các API tiện ích (Services, Doctor schedules, queue management), tích hợp Gemini AI và xây dựng Background Jobs (nhắc lịch tiêm chủng).

### 2.2. Quy trình làm việc trên Git (Git Workflow)
*   **Nhánh chính:**
    *   `main`: Chứa code stable đã release qua các mốc quan trọng.
    *   `develop`: Nhánh tích hợp chính. Mọi tính năng mới đều được merge vào đây.
*   **Nhánh tính năng (Feature branches):** `feature/sprint-[id]-[feature-name]` (Ví dụ: `feature/sprint-1-login-backend`).
*   **Quy chuẩn Pull Request (PR):**
    *   Tối thiểu phải có **1 reviewer** chấp thuận trước khi merge vào `develop`.
    *   Code phải pass qua pipeline kiểm tra tự động (GitHub Actions - Build & Unit Test).

---

## 3. Mốc Thời Gian & Kế Hoạch Bàn Giao (Milestones & Deliverables)

| Sprint | Thời gian dự kiến | Mục tiêu bàn giao chính | Môi trường triển khai |
| :--- | :--- | :--- | :--- |
| **Sprint 1** | Tuần 1 - Tuần 2 | Bộ khung Clean Architecture, API & UI Đăng ký/Đăng nhập/Quên mật khẩu. | Local Staging (Dockerized PostgreSQL) |
| **Sprint 2** | Tuần 3 - Tuần 4 | Trang chủ hiển thị dịch vụ & bác sĩ, chức năng CRUD thông tin cá nhân và hồ sơ thú cưng. | Local Staging |
| **Sprint 3** | Tuần 5 - Tuần 6 | Cấu hình Slot khám, luồng Đặt lịch khám và tiêm chủng trực tuyến hoàn chỉnh. | Staging / Development |
| **Sprint 4** | Tuần 7 - Tuần 8 | Cổng lễ tân (check-in, quản lý hàng đợi khám), cổng bác sĩ (xem hàng đợi, tiếp nhận ca khám). | Staging / Development |
| **Sprint 5** | Tuần 9 - Tuần 10 | Bệnh án điện tử, kê đơn trừ kho thuốc tự động, thanh toán hóa đơn tổng hợp tại quầy lễ tân. | Demo Environment |
| **Sprint 6** | Tuần 11 - Tuần 12 | Dashboard quản trị (Doanh thu, dịch vụ), AI Chatbot tư vấn, Background job gửi email, E2E Testing. | Production Release (UAT) |

---

## 4. Quản Trị Rủi Ro & Giải Pháp Khắc Phục (Risk Management)

| Loại rủi ro | Khả năng | Ảnh hưởng | Giải pháp phòng ngừa / Khắc phục |
| :--- | :--- | :---: | :--- |
| **Trễ hạn task quan trọng (Đặc biệt là Booking ở Sprint 3)** | Trung bình | Rất cao | Chia nhỏ task, hỗ trợ chéo giữa Nam và Hạnh ở backend. Thiết lập mốc cảnh báo trước 3 ngày khi kết thúc Sprint. |
| **Xung đột API giữa FE và BE** | Cao | Trung bình | Định nghĩa trước tài liệu giao ước API ([api-spec.md](file:///e:/DATN/MyPetClinic/plan/api-spec.md)) trước khi code. Sử dụng công cụ mock API nếu BE chưa hoàn thành kịp. |
| **Lỗi tồn kho thuốc bất đồng bộ (Race Condition)** | Thấp | Cao | Sử dụng Database Transactions và Lock ở tầng Database khi thực hiện trừ số lượng thuốc tồn kho trong [Medical_records](file:///e:/DATN/MyPetClinic/plan/database.md#L160). |
| **Quá tải API Gemini AI khi chat** | Trung bình | Trung bình | Triển khai cơ chế lưu cache kết quả trả về cho các câu hỏi phổ biến, giới hạn tần suất yêu cầu (Rate Limiting) trên mỗi tài khoản. |

---

## 5. Tiêu Chí Nghiệm Thu & Kiểm Thử (Definition of Done & Testing)
*   **Unit Test:** 100% các Service xử lý nghiệp vụ đặt lịch khám, kiểm tra trùng lặp thời gian bác sĩ phải được viết Unit Test và pass thành công (ví dụ: [tests/MyPetClinic.Tests/](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/)).
*   **UI/UX:** Giao diện đáp ứng tốt trên các kích thước màn hình phổ biến (Responsive Design), không vỡ layout, tuân thủ bảng mã màu CSS chuẩn.
*   **Báo cáo tiến độ:** Mọi thay đổi về tiến độ thực thi phải được ghi nhận vào nhật ký [progress.md](file:///e:/DATN/MyPetClinic/plan/tracking/progress.md) vào ngày cuối của mỗi Sprint.
