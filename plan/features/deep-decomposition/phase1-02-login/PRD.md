# 🚀 Product Requirements Document (PRD) - Login System (Phase 1)

## 1. Tổng quan Dự án (Overview)
Tính năng **Đăng nhập hệ thống** (Login System) là chốt chặn bảo mật cốt lõi và điều phối vai trò người dùng trong hệ thống quản lý phòng khám thú y **MyPetClinic**. Nó cho phép khách hàng (chủ nuôi thú cưng) và nhân sự phòng khám (Lễ tân, Bác sĩ thú y, Thu ngân, Admin) đăng nhập bằng Email và Mật khẩu, cung cấp cơ chế phân quyền dựa trên vai trò (Role-Based Access Control - RBAC) và định hướng điều phối giao diện hiển thị phù hợp.

---

## 2. Mục tiêu Sản phẩm (Goals)
*   **Xác thực định danh đa vai trò:** Xác minh thông tin đăng nhập của cả khách hàng lẫn toàn bộ nhân viên phòng khám.
*   **Hỗ trợ điều phối giao diện tự động (Role-Based Redirection):** Đăng nhập xong, khách hàng được đưa về trang Portal cá nhân, bác sĩ được đưa về Portal chẩn đoán lâm sàng, lễ tân về Portal hàng đợi, thu ngân về Portal hóa đơn, và admin về Dashboard quản trị hệ thống.
*   **Bảo mật thông tin tối đa:** Sử dụng Cookie/JWT Token an toàn để lưu trữ trạng thái đăng nhập, chống đánh cắp phiên làm việc (session hijacking).
*   **Ngăn chặn dò mật khẩu (Brute-force):** Khóa tạm thời tài khoản hoặc IP khi phát hiện đăng nhập sai nhiều lần liên tiếp.

---

## 3. Chân dung Người dùng (User Personas & Stories)

### 3.1. Chân dung Người dùng
*   **Khách hàng nuôi thú cưng (Vy, 21 tuổi):**
    *   *Bối cảnh:* Vy có một chú chó Husky đang điều trị bệnh tiêu hóa tại MyPetClinic. Vy muốn đăng nhập vào hệ thống để kiểm tra lịch hẹn khám tái chủng của chú chó chiều nay và xem lại đơn thuốc bác sĩ đã kê ngày hôm qua.
    *   *Nhu cầu:* Vy cần form đăng nhập đơn giản trên điện thoại di động, có ghi nhớ tài khoản để lần sau không phải nhập lại, và nút hiển thị mật khẩu để kiểm tra tránh gõ nhầm ký tự.
*   **Bác sĩ Thú y (Bác sĩ Minh, 35 tuổi):**
    *   *Bối cảnh:* Buổi sáng đến phòng khám, Bác sĩ Minh cần đăng nhập nhanh vào tài khoản nhân viên của mình trên máy tính bàn tại phòng khám để xem danh sách thú cưng đang xếp hàng chờ chẩn đoán.
    *   *Nhu cầu:* Tốc độ đăng nhập tức thì (<150ms), điều phối thẳng vào màn hình làm việc (Clinical Portal), không cần thực hiện nhiều bước trung gian.

### 3.2. User Stories
*   Là một người dùng đã có tài khoản, tôi muốn đăng nhập bằng Email và Mật khẩu để truy cập vào các tính năng cá nhân hóa của tôi.
*   Là một Bác sĩ thú y, tôi muốn sau khi đăng nhập thành công, hệ thống tự động đưa tôi đến giao diện chẩn đoán bệnh án để tôi có thể làm việc ngay mà không phải bấm chuyển trang thủ công.
*   Là một khách hàng, tôi muốn xem được mật khẩu dưới dạng text khi nhấp vào icon con mắt bên cạnh ô mật khẩu để tôi chắc chắn mình đã gõ đúng ký tự.
*   Là một quản trị viên hệ thống, tôi muốn tài khoản bị khóa tạm thời 15 phút nếu nhập sai mật khẩu quá 5 lần liên tiếp để ngăn ngừa tin tặc dùng tool tự động dò mật khẩu.

---

## 4. Phạm vi Tính năng (Scope of Work)

### 4.1. Trong phạm vi (In-Scope - MVP)
*   Form đăng nhập nhập Email và Mật khẩu, có icon hiển thị/ẩn mật khẩu.
*   Xác thực email và đối chiếu hash mật khẩu bằng BCrypt ở Backend.
*   Trả về mã JWT Token chứa các Claims thông tin người dùng (Id, Email, FullName, Role).
*   Lưu trữ JWT Token an toàn phía Client (localStorage hoặc HTTP-Only Cookie tùy cấu hình bảo mật).
*   Phân phối luồng định tuyến (Router Guard) chuyển hướng người dùng dựa vào thuộc tính `Role` nhận được.
*   Cơ chế khóa tài khoản tạm thời khi đăng nhập sai quá số lần quy định (5 lần).

### 4.2. Ngoài phạm vi (Out-of-Scope - Các phase tiếp theo)
*   Tính năng xác thực 2 yếu tố (2FA - Two Factor Authentication) qua Google Authenticator hoặc SMS OTP mỗi lần đăng nhập.
*   Khôi phục mật khẩu trực tiếp tại form đăng nhập (làm ở module Quên mật khẩu riêng).

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements)
*   **Thời gian phản hồi xác thực:** API xử lý đối chiếu thông tin đăng nhập và sinh JWT phải hoàn tất dưới **150ms** (ở điều kiện thông thường).
*   **Độ an toàn của Token:** Mã JWT Token phải được ký bằng thuật toán bảo mật mạnh mẽ (HMAC SHA256) với khóa bí mật (Secret Key) tối thiểu 256-bit được lưu trong biến môi trường.
*   **Tính riêng tư:** Không lưu trữ mật khẩu dạng thô trong bộ nhớ tạm thời của Client hay in ra log hệ thống dưới bất kỳ hình thức nào.
*   **Hạn sử dụng phiên:** Phiên đăng nhập JWT có thời gian hết hạn (Expiration Time) là **12 giờ** để đảm bảo tính an toàn.
