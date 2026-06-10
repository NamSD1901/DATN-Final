# 🚀 Product Requirements Document (PRD) - Online Examination Booking

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Trong hệ sinh thái **MyPetClinic**, tính năng **Đặt lịch khám bệnh trực tuyến (Online Examination Booking)** là nghiệp vụ trung tâm kết nối Khách hàng với Đội ngũ Y tế. Tính năng này cho phép chủ thú cưng chủ động lên lịch thăm khám cho thú cưng 24/7 mà không cần liên hệ qua hotline, giúp giảm tải công việc cho bộ phận Lễ tân và tối ưu lịch làm việc của Bác sĩ thú y.

Mục tiêu cốt lõi của tính năng này là:
*   **Tối ưu hóa quy trình tiếp đón:** Tự động xếp lịch vào hàng chờ y tế phù hợp theo thời gian đặt lịch.
*   **Tránh xung đột tài nguyên:** Chặn đứng lỗi trùng lịch khám của Bác sĩ điều trị.
*   **Bảo vệ dữ liệu y tế:** Ngăn chặn IDOR chéo hồ sơ thú cưng giữa các khách hàng khác nhau.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)

### 👩‍💼 Persona 1: Nguyễn Thị Hà (Chủ nuôi mèo Anh lông ngắn)
*   **Đặc điểm:** Nhân viên văn phòng, thường xuyên bận rộn, chỉ rảnh vào cuối tuần hoặc sau giờ hành chính.
*   **Mục tiêu:** Muốn đặt lịch tiêm phòng hoặc khám bệnh cho chú mèo cưng nhanh gọn qua điện thoại di động mà không cần gọi điện xác nhận.
*   **Nỗi đau:** Việc đặt lịch mất nhiều bước hoặc không biết lịch làm việc trống của bác sĩ để chọn lựa, dẫn đến việc đặt trùng ca và phải hẹn lại.

### 🥼 Persona 2: Bác sĩ Phạm Hoàng Nam (Bác sĩ thú y)
*   **Đặc điểm:** Tập trung điều trị chuyên môn, lịch khám dày đặc.
*   **Mục tiêu:** Cần danh sách ca khám được sắp xếp khoa học, giãn cách tối thiểu 30 phút giữa các ca để có đủ thời gian chuẩn bị dụng cụ y tế và vệ sinh phòng khám.
*   **Nỗi đau:** Ca khám bị xếp chồng chéo (Double-booking) khiến bác sĩ quá tải và khách hàng phải chờ đợi lâu gây bực dọc.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Đặt lịch khám đa bước (Multi-step Booking Form)
*   **Là một** khách hàng đã đăng nhập,
*   **Tôi muốn** đặt lịch khám qua biểu mẫu đa bước trực quan,
*   **Để tôi** đăng ký lịch khám cho bé thú cưng của mình một cách chính xác.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Hệ thống cung cấp form 5 bước:
        1.  *Bước 1 (Chọn Thú cưng):* Chọn từ danh sách thú cưng của tôi.
        2.  *Bước 2 (Chọn Dịch vụ & Triệu chứng):* Chọn dịch vụ y tế và nhập mô tả triệu chứng của bé.
        3.  *Bước 3 (Chọn Bác sĩ):* Chọn bác sĩ mong muốn hoặc chọn "Bác sĩ bất kỳ" (Hệ thống tự động phân phối bác sĩ đang trực).
        4.  *Bước 4 (Chọn Ngày giờ):* Chọn ngày khám (chỉ cho phép ngày tương lai) và chọn khung giờ trống.
        5.  *Bước 5 (Xác nhận):* Xem lại tóm tắt thông tin và xác nhận gửi.
    *   **AC2:** Chống IDOR: Chỉ cho phép đặt lịch cho thú cưng thuộc sở hữu của chính chủ tài khoản đang đăng nhập.

### User Story 2: Chống trùng lịch (Double-booking Prevention)
*   **Là một** quản trị viên phòng khám,
*   **Tôi muốn** hệ thống tự động chặn các lượt đặt lịch trùng ca của cùng một Bác sĩ,
*   **Để** đảm bảo thời gian khám bệnh của mỗi bé thú cưng được chuẩn bị chu đáo nhất.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Một bác sĩ không thể có 2 lịch hẹn cách nhau dưới **30 phút** (khung giờ vàng để khám và vệ sinh).
    *   **AC2:** Nếu phát hiện trùng lịch, hệ thống báo lỗi rõ ràng trên UI và gợi ý chọn khung giờ khác.

### User Story 3: Phân loại trạng thái lịch hẹn tự động
*   **Là một** lễ tân phòng khám,
*   **Tôi muốn** hệ thống tự động phân loại lịch hẹn mới dựa trên thời gian bắt đầu,
*   **Để tôi** dễ dàng phê duyệt hoặc tiếp nhận khám ngay.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Nếu lịch hẹn được đặt trước thời gian khám **> 1 giờ**: Trạng thái lịch là `pending` (Chờ lễ tân duyệt duyệt).
    *   **AC2:** Nếu lịch hẹn được đặt trong vòng **1 giờ** so với thời gian hiện tại (khám ngay): Trạng thái lịch là `waiting` (Chuyển thẳng vào hàng đợi phòng chờ khám).

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Bộ API RESTful đặt lịch khám y tế (`GET /api/my-appointments`, `POST /api/my-appointments`, `PUT /api/my-appointments/{id}/cancel`).
*   Form đặt lịch đa bước (Multi-step Wizard) phong cách Glassmorphism mờ kính.
*   Thuật toán chống trùng lịch bác sĩ trong khoảng thời gian $\pm30$ phút.
*   Cơ chế phân loại trạng thái tự động (`pending` / `waiting`) và phát sinh mã QR token (`QR-XXXXXXXX`) phục vụ check-in nhanh.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Thanh toán tiền khám trực tuyến (Sẽ được xử lý bởi Thu ngân tại quầy ở Phase 2 - Cashier).
*   Gửi SMS nhắc lịch hẹn (Phase 3 - Notification).
