# 📝 Implementation Plan & Testing Strategy - Online Examination Booking

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) phục vụ bộ phận QA kiểm định chất lượng, ngăn chặn lỗi trùng lịch khám của Bác sĩ và kiểm tra bảo mật chống tấn công IDOR chéo.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự theo 4 giai đoạn khép kín nhằm bảo đảm độ tin cậy cao nhất của tính năng:

### Giai đoạn 1: Database Setup & Repository Setup
*   **Bước 1.1:** Khởi tạo bảng `Appointments` trong PostgreSQL cơ sở dữ liệu với các ràng buộc khóa ngoại liên kết `Users` (Customer, Doctor), `Pets` và `Services`.
*   **Bước 1.2:** Thiết lập chỉ mục composite filtered `IX_Appointments_DoubleBookingCheck` chống trùng lịch trên DB.
*   **Bước 1.3:** Viết Repository `AppointmentRepository` chứa các hàm kiểm tra trùng lịch `AnyAsync` và lấy danh sách theo Owner.

### Giai đoạn 2: Backend Logic & Security Controls
*   **Bước 2.1:** Cài đặt logic nghiệp vụ chống trùng lịch $\pm30$ phút trong `AppointmentService.CreateAppointmentAsync`.
*   **Bước 2.2:** Viết logic phân loại trạng thái lịch hẹn tự động: Nếu thời gian lớn hơn hiện tại 1 giờ -> `pending` (Chờ duyệt), ngược lại -> `waiting` (Xếp hàng chờ khám ngay).
*   **Bước 2.3:** Khai báo API Controller `CustomerAppointmentController` phân quyền `[Authorize(Roles = "customer")]` và tích hợp bộ giải mã token claims.
*   **Bước 2.4:** Viết các Unit Tests trong [AppointmentServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/AppointmentServiceTests.cs) bảo đảm bao phủ 100% logic chặn trùng lịch bác sĩ.

### Giai đoạn 3: Giao diện Front-End & Wizards
*   **Bước 3.1:** Viết Pinia store (`stores/booking.ts`) quản lý cờ trạng thái Wizard, lưu cache dịch vụ/bác sĩ và cờ submit locking.
*   **Bước 3.2:** Dựng giao diện Modal đặt lịch 5 bước sử dụng phong cách Glassmorphism, bo góc mượt mà, hỗ trợ grid chọn giờ động.
*   **Bước 3.3:** Tích hợp bộ kiểm tra dữ liệu đầu vào Client-side validation.
*   **Bước 3.4:** Thiết lập hiển thị mã QR Token Check-in sau khi đặt lịch thành công.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Chống trùng lịch (Functional Testing Cases)

#### TC-FUN-01: Đăng ký đặt lịch thành công với bác sĩ bất kỳ (Auto-assignment)
*   **Mục tiêu:** Xác minh hệ thống tự động phân phối bác sĩ khi để trống DoctorId.
*   **Dữ liệu đầu vào:**
    *   `petId: 12`, `serviceId: 3` (Khám tổng quát), `doctorId: null` (hoặc Guid.Empty).
    *   `appointmentDate`: Ngày mai lúc `10:00`.
*   **Các bước thực hiện:**
    1. Đăng nhập tài khoản Customer, gửi request `POST /api/my-appointments`.
*   **Kết quả mong đợi:**
    *   API phản hồi mã `200 OK` (success: true).
    *   Kiểm tra database bản ghi mới tạo đã được tự động gán ID của một bác sĩ đang hoạt động trong hệ thống.

#### TC-FUN-02: Chặn đặt lịch trùng giờ của Bác sĩ (Double-booking)
*   **Mục tiêu:** Đảm bảo quy tắc giãn cách cứng 30 phút giữa các ca khám.
*   **Kịch bản giả lập:** Bác sĩ A đã có lịch khám vào ngày mai lúc `09:00`.
*   **Các bước thực hiện:**
    1. Gửi request `POST /api/my-appointments` đặt lịch cho Bác sĩ A vào ngày mai lúc `09:15` (Sai lệch 15 phút, vi phạm quy tắc $\pm30$ phút).
*   **Kết quả mong đợi:**
    *   API trả về mã lỗi `400 Bad Request`.
    *   Nội dung phản hồi báo lỗi rõ ràng: *"Bác sĩ đã có lịch hẹn trong khoảng thời gian này."*.
    *   Lịch thứ hai không được tạo dưới DB.

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Kiểm thử tấn công IDOR chéo thú cưng (Pet Ownership Bypass)
*   **Mục tiêu:** Đảm bảo người dùng không thể đặt lịch bằng ID thú cưng của người khác.
*   **Các bước thực hiện:**
    1. Đăng nhập bằng tài khoản Customer A.
    2. Gửi request `POST /api/my-appointments` nhưng điền tham số `petId: 99` (Trong đó pet ID 99 thuộc sở hữu của Customer B).
*   **Kết quả mong đợi:**
    *   API trả về mã lỗi `400 Bad Request`.
    *   Nội dung phản hồi: *"Thú cưng không hợp lệ hoặc không thuộc về bạn."*.
    *   Lịch hẹn hoàn toàn không được khởi tạo.

#### TC-SEC-02: Kiểm thử tấn công IDOR hủy lịch của người khác
*   **Các bước thực hiện:**
    1. Đăng nhập bằng tài khoản Customer A.
    2. Gửi request hủy lịch `PUT /api/my-appointments/150/cancel` (Trong đó lịch hẹn ID 150 là của Customer B).
*   **Kết quả mong đợi:**
    *   API trả về mã lỗi `403 Forbidden` (hoặc `404 Not Found`).
    *   Trạng thái lịch hẹn ID 150 dưới DB vẫn giữ nguyên (không bị chuyển sang `cancelled`).
