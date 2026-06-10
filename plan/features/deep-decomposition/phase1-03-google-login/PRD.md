# 🚀 Product Requirements Document (PRD) - Google OAuth Login (Phase 1)

## 1. Tổng quan Dự án (Overview)
Tính năng **Đăng nhập qua Google** (Google OAuth Login) cung cấp cơ chế xác thực một chạm (one-click) bảo mật và tiện lợi cho khách hàng tại hệ thống **MyPetClinic**. Nó giúp tối giản hóa trải nghiệm người dùng bằng cách bỏ qua biểu mẫu đăng nhập email/mật khẩu truyền thống, đồng thời hỗ trợ đăng ký tài khoản tự động (on-the-fly registration) khi khách hàng lần đầu truy cập hệ thống qua tài khoản Google.

---

## 2. Mục tiêu Sản phẩm (Goals)
*   **Trải nghiệm đăng nhập không mật khẩu (Passwordless):** Đăng nhập nhanh chóng và an toàn trong vòng 1-click thông qua hệ sinh thái Google.
*   **Tự động tạo tài khoản thông minh (Auto-Provisioning):** Nếu địa chỉ email Google chưa có trong hệ thống, tự động khởi tạo hồ sơ `User` mới, kích hoạt sẵn (`IsActive = true`) và liên kết thông tin đại diện (FullName, Avatar).
*   **Liên kết tài khoản trơn tru:** Nếu email Google đã tồn tại dưới dạng tài khoản đăng ký thường, tự động cho phép đăng nhập và liên kết tài khoản an toàn mà không làm mất mát dữ liệu thú cưng hiện có.

---

## 3. Chân dung Người dùng (User Personas & Stories)

### 3.1. Chân dung Người dùng
*   **Khách hàng nuôi thú cưng bận rộn (Minh, 30 tuổi):**
    *   *Bối cảnh:* Anh Minh là lập trình viên, có nuôi một chú mèo Golden đang cần hẹn giờ tắm spa gấp tại MyPetClinic. Anh Minh không muốn nhớ thêm một mật khẩu mới hoặc mất thời gian thực hiện các bước xác thực OTP Email.
    *   *Nhu cầu:* Anh Minh muốn click chọn "Đăng nhập với Google", chọn tài khoản gmail đang đăng nhập sẵn trên trình duyệt Chrome và ngay lập tức vào màn hình đặt lịch.
*   **Khách hàng lớn tuổi (Cô Hoa, 52 tuổi):**
    *   *Bối cảnh:* Cô Hoa không rành công nghệ, nuôi một bé cún Poodle. Cô Hoa thường quên mật khẩu tài khoản và gặp khó khăn khi thao tác bàn phím điện thoại để gõ mật khẩu phức tạp.
    *   *Nhu cầu:* Một nút bấm to, rõ ràng, hoạt động mượt mà trên Safari điện thoại di động giúp cô đăng nhập ngay lập tức qua tài khoản Google.

### 3.2. User Stories
*   Là một người dùng mới, tôi muốn đăng nhập bằng Google để tôi không phải nhập form đăng ký dài dòng và xác thực OTP Email mất thời gian.
*   Là một khách hàng cũ, tôi muốn đăng nhập bằng Google (cùng địa chỉ email đã đăng ký trước đó) để hệ thống nhận diện đúng tài khoản của tôi và tôi không bị tạo trùng tài khoản mới.
*   Là một lập trình viên, tôi muốn quá trình xác thực token Google ở phía Backend diễn ra an toàn, đảm bảo kẻ xấu không thể giả mạo token Google để đăng nhập trái phép vào tài khoản của người khác.

---

## 4. Phạm vi Tính năng (Scope of Work)

### 4.1. Trong phạm vi (In-Scope - MVP)
*   Nút bấm "Đăng nhập với Google" hiển thị chuẩn theo Identity Guidelines của Google.
*   Tích hợp thư viện Google Identity Services (GIS) SDK ở Frontend để hiển thị Google Sign-In Popup.
*   Frontend lấy mã xác thực `CredentialToken` (JWT) của Google và gửi lên Web API.
*   Backend sử dụng thư viện chính thức `Google.Apis.Auth` để xác thực chữ ký số của Token từ Google Server.
*   Tự động chèn bản ghi User mới vào cơ sở dữ liệu nếu Email chưa tồn tại, mặc định đặt `IsActive = true` (vì đã được Google xác minh email trước đó).

### 4.2. Ngoài phạm vi (Out-of-Scope - Các phase tiếp theo)
*   Liên kết tài khoản mạng xã hội khác như Facebook Login, Apple ID.
*   Quản lý việc ngắt kết nối tài khoản Google trong trang cập nhật thông tin cá nhân.

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements)
*   **Tốc độ xác thực trao đổi:** API đổi Token Google lấy JWT Token của MyPetClinic phải hoàn tất xử lý dưới **300ms** (ở điều kiện kết nối mạng thông thường giữa server MyPetClinic và Google).
*   **Bảo mật Token:** Mã xác thực nhận được từ Google chỉ được sử dụng một lần và kiểm tra tính toàn vẹn (Audience, ClientID, Expiry) nghiêm ngặt trước khi Backend cấp Token MyPetClinic.
*   **Độ tin cậy:** Giao diện đăng nhập Google tự động thích ứng hoàn hảo trên mọi kích thước màn hình thiết bị và trình duyệt di động.
