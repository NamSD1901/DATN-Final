# 🟢 Level 1: Foundation (Nền Tảng - Tuần 1-2)

Mục tiêu của cấp độ này là giúp học viên làm quen với tư duy sản phẩm, phương pháp phát triển phần mềm Agile/Scrum, cách viết tài liệu yêu cầu (User Stories & Acceptance Criteria) và thực hiện các bước kiểm thử thủ công cơ bản.

---

## 🎯 PRD-01: Product Mindset & User Empathy

### 1. Tầm Nhìn Sản Phẩm (Product Vision & Value Proposition)
* **Tầm nhìn (Vision):** Số hóa và tối ưu hóa toàn bộ hành trình chăm sóc thú cưng tại phòng khám, kết nối chặt chẽ giữa Chủ nuôi (Customer) - Lễ tân (Receptionist) - Bác sĩ (Veterinarian) - Quản trị viên (Admin).
* **Định vị sản phẩm:** Thay thế việc quản lý bằng sổ ghi chép giấy và đặt lịch qua điện thoại truyền thống bằng một ứng dụng Single Page App (Vue 3) mượt mà, hỗ trợ bởi AI Chatbot 24/7 và hệ thống quản lý hàng chờ thời gian thực.

### 2. Hồ Sơ Người Dùng (User Personas)
* **Persona 1 (Chủ nuôi - Chị Minh Anh, 28 tuổi):** Nhân viên văn phòng bận rộn. Cần đặt lịch khám ngoài giờ hành chính trong 2 phút, nhận nhắc nhở tiêm chủng tự động, lưu trữ bệnh án trực tuyến tránh mất sổ tiêm giấy.
* **Persona 2 (Bác sĩ thú y - BS. Hùng, 35 tuổi):** Cần tra cứu nhanh tiền sử bệnh án của thú cưng chỉ với 1 click, kê đơn điện tử tự động kiểm tra tồn kho thuốc để tránh kê toa thuốc đã hết.
* **Persona 3 (Chủ phòng khám - Chị Mai, 40 tuổi):** Cần dashboard cập nhật doanh thu hàng ngày theo thời gian thực, quản lý ca trực của bác sĩ và nhận cảnh báo khi dược phẩm sắp hết hạn/hết hàng.

### 3. Bản Đồ Hành Trình Khách Hàng (User Journey Map - Đặt lịch khám)
```
[As-Is: Thủ công] Gọi điện -> Chờ máy (5-10p) -> Ghi nhận nhầm lẫn -> Đến phòng khám đợi lâu -> Nhận sổ giấy dễ mất.
[To-Be: Số hóa]   Vào Web -> Chọn thú cưng & Bác sĩ -> Chọn Slot trống -> Nhận Email xác nhận -> Đến khám đúng giờ.
```
* **Wow Moment:** Khách hàng đặt lịch thành công và thấy ngay slot của mình trên danh sách lịch hẹn của hệ thống; Bác sĩ mở bệnh án hiển thị toàn bộ lịch sử tiêm chủng và điều trị cũ của Pet.

---

## 🎯 PRD-02: Agile & Scrum Mastery

### 1. Các Buổi Lễ Scrum (Scrum Events) & Vai Trò
* **Sprint Planning (Lập kế hoạch):** PO chuẩn bị backlog đã được làm mịn, Dev & QA thảo luận kỹ thuật, estimate Story Points và chốt Sprint Goal.
* **Daily Scrum (Họp hàng ngày):** 15 phút cập nhật tiến độ: Hôm qua làm gì? Hôm nay làm gì? Có gặp khó khăn (blocker) gì không?
* **Sprint Review (Demo & Nghiệm thu):** PO xem xét demo tính năng thực tế trên môi trường staging/dev để chấp nhận hoặc từ chối dựa trên Acceptance Criteria.
* **Sprint Retrospective (Cải tiến):** Rút kinh nghiệm những điểm tốt và chưa tốt của Sprint vừa qua để đưa ra hành động cải tiến cho Sprint tiếp theo.

### 2. Định Nghĩa Sẵn Sàng (DoR) & Hoàn Thành (DoD) áp dụng cho MyPetClinic
* **Definition of Ready (DoR) - Tiêu chuẩn để Dev bắt đầu code:**
  - User Story có mô tả rõ ràng mục đích nghiệp vụ.
  - Acceptance Criteria viết theo chuẩn BDD (Given-When-Then).
  - Đã có thiết kế UI/UX (.figma hoặc mockup) được thống nhất (đối với tính năng có giao diện).
  - Đã được estimate độ phức tạp (Story Points).
* **Definition of Done (DoD) - Tiêu chuẩn để tính năng được coi là hoàn thành:**
  - Code đã được review bởi ít nhất 1 thành viên khác (Pull Request Approved).
  - Không còn lỗi nghiêm trọng (Blocker/Critical) trên môi trường kiểm thử.
  - Test Cases UAT chạy thành công 100%.
  - Code đã được deploy lên môi trường Staging/Dev.

---

## 🎯 PRD-03: User Story Writing

Một User Story tốt phải tuân thủ cấu trúc chuẩn: **As a... I want to... So that...** và đạt tiêu chuẩn **INVEST** (Independent, Negotiable, Valuable, Estimable, Small, Testable).

### Ví dụ Thực Tế từ Backlog MyPetClinic:
* **Mã số:** `PB-09`
* **Tiêu đề:** Khách hàng đặt lịch khám trực tuyến
* **Nội dung:** 
  > **As a** Khách hàng đã đăng nhập hệ thống,
  > **I want to** chọn thú cưng, chọn bác sĩ thú y, chọn dịch vụ khám và khung giờ còn trống trong lịch biểu,
  > **So that** tôi có thể chủ động sắp xếp thời gian đưa thú cưng đến khám mà không phải xếp hàng chờ đợi lâu tại phòng khám.

---

## 🎯 PRD-04: Acceptance Criteria (BDD / Gherkin Syntax)

Để tránh hiểu lầm giữa PO, BA, Dev và QA, tất cả tiêu chí nghiệm thu phải được viết dưới dạng **BDD (Behavior-Driven Development)** sử dụng cú pháp Gherkin: **Given - When - Then**.

### Ví dụ 1: Nghiệm thu tính năng Đặt lịch khám trực tuyến (`PB-09`)
```gherkin
Scenario: Đặt lịch khám thành công với thông tin hợp lệ
  Given Khách hàng "Chị Minh Anh" đã đăng nhập thành công
    And Khách hàng có thú cưng tên "Mun" trong hồ sơ
    And Ngày "15/12/2026" khung giờ "09:00 - 09:30" của "Bác sĩ Hùng" vẫn còn slot trống
  When Khách hàng vào trang "/booking"
    And Chọn thú cưng "Mun", chọn dịch vụ "Khám sức khỏe định kỳ", chọn ngày "15/12/2026"
    And Chọn khung giờ "09:00 - 09:30" và chọn "Bác sĩ Hùng"
    And Nhấn nút "Xác nhận đặt lịch"
  Then Hệ thống tạo một lịch hẹn mới với trạng thái "Pending" (Chờ xác nhận)
    And Khấu trừ 1 slot trống của khung giờ tương ứng
    And Hiển thị thông báo Toast xanh: "Đặt lịch khám thành công!"
```

### Ví dụ 2: Nghiệm thu tính năng Kê đơn thuốc điện tử của Bác sĩ (`PB-23`)
```gherkin
Scenario: Kê đơn thuốc thành công và tự động trừ kho dược phẩm
  Given Bác sĩ "BS. Hùng" đang thực hiện ca khám cho pet "Mun"
    And Thuốc "Amoxicillin 250mg" trong kho hiện còn "50" viên
  When Bác sĩ kê đơn thuốc gồm "10" viên "Amoxicillin 250mg"
    And Bác sĩ nhấn "Hoàn thành ca khám và Kê đơn"
  Then Hệ thống lưu thông tin đơn thuốc vào bệnh án điện tử của pet "Mun"
    And Hệ thống tự động trừ kho thuốc "Amoxicillin 250mg" đi "10" viên (Số lượng tồn kho mới: "40")
    And Chuyển trạng thái ca khám sang "Completed"
```

---

## 🎯 PRD-05: Basic Manual Testing

QA cần nắm vững quy trình chạy test thủ công và thiết lập bảng kiểm tra để phát hiện sớm các lỗi giao diện và chức năng cơ bản.

### 1. Quy Trình Khảo Sát Tính Năng (Smoke Testing & Sanity Testing)
* **Smoke Test:** Chạy một lượt qua các luồng chính ngay sau khi deploy bản build mới (Đăng nhập -> Xem danh sách -> Tạo lịch hẹn -> Đăng xuất) để đảm bảo app không bị crash.
* **Sanity Test:** Tập trung kiểm thử chi tiết chức năng vừa mới được sửa đổi hoặc thêm mới để chắc chắn lỗi cũ đã được sửa và không làm hỏng các hàm liên quan.

### 2. Giao Diện UI & Khả Năng Khả Dụng (Usability Checklist)
* **Responsive Design:** Giao diện có bị tràn viền, vỡ khung card trên các thiết bị Mobile (iPhone, Samsung) hay Tablet không?
* **Form & Validation:** 
  - Các trường bắt buộc (*) có hiển thị cảnh báo đỏ khi để trống không?
  - Nút "Submit" có bị disable khi form chưa điền hợp lệ không?
* **Trạng thái Loading & Empty:**
  - Có hiển thị Spinner/Loading xương cá (Skeleton) khi đang fetch API không?
  - Có hiển thị hình ảnh minh họa "Không có dữ liệu" khi danh sách trống không?
* **Aesthetics (Tính thẩm mỹ):** Các nút bấm, màu sắc có tuân thủ đúng bảng màu Premium Gold của dự án (màu vàng kim chủ đạo phối trên nền tối/sáng sang trọng) không?
