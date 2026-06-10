# 🚀 Product Requirements Document (PRD) - Online Vaccination Booking

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Trong y tế thú y, tiêm phòng vắc-xin định kỳ là biện pháp hiệu quả nhất để phòng tránh các dịch bệnh nguy hiểm ở chó mèo (như bệnh Care, Parvo, Dại, Giảm bạch cầu ở mèo...). Phân hệ **Đặt lịch tiêm phòng trực tuyến (Online Vaccination Booking)** mở rộng từ nền tảng đặt lịch khám thường, bổ sung luồng liên kết chuyên sâu y tế:
*   **Quản lý phác đồ tiêm chủng:** Cho phép chủ nuôi lựa chọn loại vắc-xin phù hợp với độ tuổi và loài của thú cưng.
*   **Cảnh báo phác đồ tiêm sớm:** Hệ thống tự động tính toán lịch sử tiêm để ngăn ngừa việc tiêm nhắc lại quá sớm, có thể gây hại cho sức khỏe thú cưng hoặc làm mất tác dụng miễn dịch.
*   **Bảo vệ chuỗi cung ứng y tế:** Lịch hẹn chỉ cho phép đăng ký khi loại vắc-xin đó còn tồn kho thực tế tại phòng khám.

Mục tiêu cốt lõi là cung cấp một luồng đặt lịch an toàn y khoa, trực quan và tối ưu hóa thời gian tái chủng cho vật nuôi.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)

### 👩‍💼 Persona 1: Nguyễn Thu Hà (Chủ nuôi chú mèo con 2 tháng tuổi)
*   **Mục tiêu:** Cần đặt lịch tiêm mũi vắc-xin đầu tiên (vắc-xin 4 bệnh của mèo) cho bé mèo ta mới nhặt được. Hà muốn hệ thống tự động gợi ý lịch tiêm phù hợp vì cô không có kiến thức thú y.
*   **Nỗi đau:** Các loại vắc-xin có tên khoa học quá phức tạp, không hiểu khoảng cách thời gian giữa mũi 1 và mũi 2 là bao lâu, lo lắng tiêm sai phác đồ sẽ gây nguy hiểm cho mèo con.

### 🥼 Persona 2: Bác sĩ Trần Quốc Anh (Bác sĩ thú y trực ca)
*   **Mục tiêu:** Kiểm tra nhanh lịch sử tiêm phòng của thú cưng khi chủ nuôi đưa bé đến tiêm. Yêu cầu dữ liệu đồng bộ chính xác để tránh việc tiêm thừa hoặc tiêm thiếu mũi.
*   **Nỗi đau:** Khách hàng không nhớ rõ bé đã tiêm những mũi gì ở phòng khám khác, hệ thống không có cảnh báo nếu chủ nuôi đặt lịch tiêm vắc-xin trùng lặp trong thời gian ngắn.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Đặt lịch tiêm phòng tích hợp phác đồ y tế
*   **Là một** khách hàng đã đăng nhập,
*   **Tôi muốn** đặt lịch tiêm vắc-xin cho thú cưng và chọn loại vắc-xin từ danh sách gợi ý phù hợp,
*   **Để** bé được tiêm phòng định kỳ đúng hạn.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Ở bước chọn vắc-xin, hệ thống chỉ hiển thị các loại vắc-xin khớp với Loài của thú cưng đã chọn ở bước 1 (Ví dụ: Mèo chỉ chọn vắc-xin cho mèo, Chó chọn vắc-xin cho chó).
    *   **AC2:** Kiểm tra tồn kho y tế: Chỉ hiển thị vắc-xin còn số lượng tồn kho khả dụng lớn hơn 0 tại kho dược của phòng khám.
    *   **AC3:** Chống trùng lịch Bác sĩ thú y trực ca ($\pm30$ phút) tương tự luồng đặt lịch khám thường.

### User Story 2: Cảnh báo khoảng cách tiêm chủng (Vaccination Interval Warning)
*   **Là một** chủ nuôi thú cưng,
*   **Tôi muốn** hệ thống cảnh báo nếu tôi vô tình đặt lịch tiêm quá sớm so với phác đồ quy định,
*   **Để tôi** tránh gây ảnh hưởng xấu đến sức khỏe của vật nuôi.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Khi khách hàng chọn vắc-xin X, hệ thống truy quét bảng `VaccinationRecords` tìm mũi tiêm gần nhất của vắc-xin này trên bé cún/mèo đó.
    *   **AC2:** Nếu khoảng cách từ mũi tiêm gần nhất đến ngày hẹn khám mới nhỏ hơn khoảng cách tối thiểu quy định (Ví dụ: mũi vắc-xin dại nhắc lại tối thiểu 11 tháng, mũi vắc-xin 4 bệnh mèo nhắc lại tối thiểu 21 ngày):
        *   Hiển thị cảnh báo trực quan màu vàng trên giao diện: *"Cảnh báo: Bé đã tiêm mũi gần nhất vào ngày DD/MM/YYYY. Thời gian tiêm nhắc lại khuyến nghị là sau ngày DD/MM/YYYY."*
        *   Cho phép người dùng bỏ qua cảnh báo nếu có chỉ định đặc biệt của Bác sĩ (vẫn cho gửi đặt lịch nhưng đánh dấu cờ warning cho lễ tân lưu ý).

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Form đặt lịch tiêm phòng đa bước có lọc thông minh vắc-xin theo loài thú cưng.
*   Logic nghiệp vụ truy vấn lịch sử tiêm chủng và kiểm tra khoảng cách thời gian tiêm an toàn (`VaccinationScheduleChecker`).
*   Tự động cập nhật trạng thái lịch hẹn (`pending` hoặc `waiting`) và phát sinh QR Token check-in.
*   API liên kết vaccine trong lịch hẹn (`Appointment.ServiceId = 2` (Tiêm chủng) và liên kết `VaccineId`).

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Theo dõi nhiệt độ bảo quản tủ lạnh vắc-xin chuyên sâu IoT (Sẽ nghiên cứu ở Phase sau).
*   Thực hiện tiêm chủng (Nghiệp vụ ghi nhận mã lô thuốc, phản ứng sau tiêm sẽ do Bác sĩ thực hiện tại phòng khám sau khi check-in lịch hẹn).
