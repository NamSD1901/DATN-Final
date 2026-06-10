# 🚀 Product Requirements Document (PRD) - Authentication & Authorization Architecture

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Phân hệ **Xác thực và Phân quyền (Authentication & Authorization)** là nền tảng bảo mật cốt lõi, đóng vai trò như lớp bảo vệ vòng ngoài bảo vệ toàn bộ tài nguyên số của **MyPetClinic**. Nó định danh người dùng và kiểm soát chặt chẽ quyền hạn truy cập của 4 nhóm vai trò chính trong hệ thống phòng khám thú y:
1.  **Khách hàng (Customer - Chủ thú cưng):** Đăng ký, quản lý thú cưng của mình, đặt lịch khám/tiêm chủng trực tuyến.
2.  **Lễ tân (Receptionist):** Quản lý hàng đợi, check-in khách đến, duyệt lịch hẹn, quản lý thanh toán hóa đơn.
3.  **Bác sĩ thú y (Doctor):** Ghi chẩn đoán lâm sàng, kê đơn thuốc, xem lịch sử bệnh án thú cưng.
4.  **Quản trị viên (Admin):** Quản lý nhân sự, kiểm soát kho thuốc/vắc-xin, theo dõi báo cáo doanh thu tài chính.

Mục tiêu tối thượng là đảm bảo thông tin bệnh án và thông tin cá nhân của khách hàng được bảo vệ tuyệt đối theo các quy chuẩn bảo mật (GDPR/OWASP), ngăn chặn IDOR chéo, cung cấp trải nghiệm đăng nhập nhanh chóng bằng Google OAuth2 và quy trình kích hoạt/khôi phục tài khoản an toàn qua OTP Email.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas & Scenarios)

### 👩‍💼 Persona 1: Nguyễn Tiến Hùng (Khách hàng - Chủ thú cưng)
*   **Mục tiêu:** Muốn tạo tài khoản nhanh chóng để đặt ca khám khẩn cấp cho chú chó Golden bị tiêu chảy. Anh Hùng chọn phương thức **Google OAuth** để không phải ghi nhớ thêm một mật khẩu mới.
*   **Nỗi đau:** Việc đăng ký quá phức tạp hoặc thời gian nhận OTP kích hoạt tài khoản quá lâu (>2 phút) sẽ khiến anh mất kiên nhẫn và chuyển sang phòng khám đối thủ.

### 🥼 Persona 2: Bác sĩ Nguyễn Minh Đức (Bác sĩ thú y)
*   **Mục tiêu:** Đăng nhập an toàn để xem danh sách ca khám trực ban trong ngày. Yêu cầu tính năng giữ trạng thái đăng nhập (Remember Me) suốt ca trực dài 12 tiếng.
*   **Nỗi đau:** Tài khoản thường xuyên bị thoát phiên đột ngột (Session timeout) trong lúc đang gõ thông tin chẩn đoán cho thú cưng, làm mất dữ liệu bệnh án chưa kịp lưu.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Đăng ký tài khoản khách hàng mới có xác thực OTP
*   **Là một** khách hàng mới truy cập MyPetClinic,
*   **Tôi muốn** đăng ký tài khoản bằng Họ tên, Email, Số điện thoại và Mật khẩu,
*   **Để tôi** có thể bắt đầu sử dụng dịch vụ đặt lịch khám trực tuyến.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Tài khoản sau khi đăng ký thành công mặc định ở trạng thái `IsActive = false` (Chưa kích hoạt) và chưa thể đăng nhập.
    *   **AC2:** Hệ thống sinh mã OTP 6 chữ số ngẫu nhiên có hiệu lực trong 5 phút và gửi email kích hoạt qua dịch vụ SMTP.
    *   **AC3:** Khi người dùng nhập đúng mã OTP, tài khoản chuyển sang trạng thái `IsActive = true`, đồng thời tự động đăng nhập đưa người dùng vào Dashboard.

### User Story 2: Đăng nhập hệ thống phân quyền vai trò (RBAC)
*   **Là một** nhân viên hoặc khách hàng,
*   **Tôi muốn** đăng nhập bằng Email/Mật khẩu hoặc thông qua Google Login,
*   **Để** hệ thống đưa tôi vào đúng giao diện làm việc được phân quyền.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Mật khẩu nhập vào phải được kiểm tra trùng khớp với hash BCrypt lưu trong CSDL.
    *   **AC2:** Khi đăng nhập thành công, máy chủ cấp phát mã JWT an toàn. Ở phân quyền Client, Router Guard sẽ kiểm tra vai trò (Role) trong Token để điều hướng:
        *   `customer` -> chuyển hướng đến trang Dashboard chủ nuôi.
        *   `receptionist` -> chuyển hướng đến trang Tiếp nhận & Xếp hàng đợi khám.
        *   `doctor` -> chuyển hướng đến trang Danh sách ca khám bệnh.
        *   `admin` -> chuyển hướng đến trang Thống kê & Quản trị hệ thống.
    *   **AC3:** Cơ chế Lockout Policy: Đăng nhập sai quá 5 lần liên tiếp sẽ khóa tài khoản tạm thời trong 15 phút để chống tấn công brute-force.

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Kiến trúc middleware xác thực dựa trên JWT Token được lưu trong Secure HttpOnly Cookies hoặc Headers.
*   Trình xử lý đăng ký tài khoản khách hàng mới kèm OTP kích hoạt gửi qua Email.
*   Tích hợp Google Identity Services (OAuth2) một chạm cho Khách hàng.
*   Middleware phân quyền Route (RBAC) chặt chẽ ở cả Backend (.NET Authorize Attribute) và Frontend (Vue Router Guards).
*   Khóa tài khoản tự động (Lockout Policy) và Rate Limiting cho API Đăng nhập/Đăng ký.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Đăng ký tài khoản cho nhân viên (Tài khoản Doctor, Receptionist, Admin chỉ được cấp phát bởi quản trị viên thông qua Dashboard Admin ở Phase 3).
*   Xác thực 2 lớp qua SMS OTP (giới hạn ở OTP Email do chi phí đầu tư tổng đài SMS Brandname cao).
