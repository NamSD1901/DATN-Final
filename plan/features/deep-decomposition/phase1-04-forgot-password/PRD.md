# 🚀 Product Requirements Document (PRD) - Forgot Password Recovery (Phase 1)

## 1. Tổng quan Dự án (Overview)
Tính năng **Khôi phục mật khẩu** (Forgot Password Recovery) cung cấp cơ chế tự phục vụ an toàn cho người dùng khi quên thông tin đăng nhập trong hệ thống **MyPetClinic**. Thông qua xác thực mã OTP gửi về Email, khách hàng hoặc nhân viên phòng khám có thể thiết lập mật khẩu mới một cách an toàn mà không cần liên hệ trực tiếp với quản trị viên hệ sinh thái phòng khám.

---

## 2. Mục tiêu Sản phẩm (Goals)
*   **Tự phục vụ an toàn:** Hỗ trợ người dùng tự lấy lại mật khẩu nhanh chóng tại nhà thông qua xác thực Email đã sở hữu.
*   **Tránh rò rỉ thông tin:** Luồng xử lý không được tiết lộ thông tin nhạy cảm (ví dụ: không báo cụ thể email có tồn tại hay không ở giao diện công khai để tránh hacker dò tìm email người dùng).
*   **Tự động mở khóa tài khoản:** Khi người dùng khôi phục mật khẩu thành công, hệ thống tự động reset số lần nhập sai `AccessFailedCount` và xóa thuộc tính khóa `LockoutEnd`, cho phép họ đăng nhập lại lập tức.

---

## 3. Chân dung Người dùng (User Personas & Stories)

### 3.1. Chân dung Người dùng
*   **Khách hàng hay quên (Vy, 21 tuổi):**
    *   *Bối cảnh:* Vy đã tạo tài khoản MyPetClinic tháng trước nhưng hôm nay khi chú cún Poodle của Vy bị ho, Vy cần đăng nhập để đặt lịch khám bệnh khẩn cấp nhưng gõ sai mật khẩu 3 lần liên tiếp và không nhớ rõ mật khẩu là gì.
    *   *Nhu cầu:* Vy cần nút "Quên mật khẩu", điền email và nhận ngay mã OTP trong 5 giây để đặt lại mật khẩu mới, đặt xong đăng nhập vào đặt lịch được ngay.
*   **Bác sĩ thú y (Bác sĩ Minh, 35 tuổi):**
    *   *Bối cảnh:* Bác sĩ Minh thay đổi mật khẩu định kỳ của tài khoản nhân viên theo yêu cầu bảo mật của phòng khám nhưng sáng hôm sau đi làm không nhớ chính xác mật khẩu mới thay đổi.
    *   *Nhu cầu:* Muốn thực hiện nhanh quy trình đặt lại mật khẩu bảo mật ngay trên giao diện đăng nhập để kịp giờ tiếp đón các bé thú cưng đến khám buổi sáng.

### 3.2. User Stories
*   Là một người dùng quên mật khẩu, tôi muốn yêu cầu gửi mã OTP khôi phục về email của tôi để xác minh tôi là chủ sở hữu hợp pháp của email này.
*   Là một khách hàng đang khôi phục mật khẩu, tôi muốn nhập mật khẩu mới và được hệ thống kiểm duyệt độ mạnh mật khẩu trực quan ngay tại chỗ để đảm bảo mật khẩu mới đủ an toàn.
*   Là một quản trị viên phòng khám, tôi muốn hệ thống tự động mở khóa tài khoản cho người dùng ngay sau khi họ khôi phục mật khẩu thành công để tôi không phải thực hiện mở khóa thủ công dưới database.

---

## 4. Phạm vi Tính năng (Scope of Work)

### 4.1. Trong phạm vi (In-Scope - MVP)
*   Màn hình 1: Nhập Email yêu cầu khôi phục.
*   Màn hình 2: Nhập OTP xác thực gồm 6 chữ số (hiệu lực 5 phút) gửi về hòm thư.
*   Màn hình 3: Nhập Mật khẩu mới và Nhập lại mật khẩu mới (xác nhận khớp).
*   Gửi email OTP khôi phục an toàn sử dụng template MailKit SMTP.
*   Backend tự động reset bộ đếm lỗi `AccessFailedCount = 0` và mở khóa `LockoutEnd = null` khi khôi phục thành công.

### 4.2. Ngoài phạm vi (Out-of-Scope - Các phase tiếp theo)
*   Khôi phục mật khẩu bằng cách gửi link đặt lại mật khẩu trực tiếp (Reset Password Link) thay vì mã OTP.
*   Hỗ trợ khôi phục qua câu hỏi bảo mật cá nhân.

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements)
*   **Giao diện chuyển đổi mượt mà:** Chuyển đổi giữa 3 bước (Nhập email -> Nhập OTP -> Đặt mật khẩu mới) bằng các hiệu ứng trượt màn hình mượt mà dưới **200ms**.
*   **Bảo mật OTP:** Mã OTP chỉ được sử dụng một lần (One-Time). Ngay sau khi xác thực thành công, mã OTP khôi phục trong DB lập tức bị xóa bỏ (`null`).
*   **Tốc độ giao phát thư:** Thư điện tử chứa OTP khôi phục phải được gửi đi bất đồng bộ không gây nghẽn kết nối và đến tay người dùng trong vòng **5 giây**.
