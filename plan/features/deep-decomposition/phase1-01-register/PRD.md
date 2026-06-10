# 🚀 Product Requirements Document (PRD) - Register Account (Phase 1)

## 1. Tổng quan Dự án (Overview)
Tính năng **Đăng ký tài khoản** (Register Account) cung cấp điểm khởi đầu quan trọng cho khách hàng (chủ nuôi thú cưng) để tham gia vào hệ sinh thái của phòng khám thú y **MyPetClinic**. Nó cho phép họ thiết lập hồ sơ định danh duy nhất để thực hiện các chức năng cốt lõi sau này như: quản lý thú cưng, đặt lịch hẹn khám/tiêm phòng trực tuyến, theo dõi lịch sử bệnh án và tương tác với chatbot AI tư vấn.

---

## 2. Mục tiêu Sản phẩm (Goals)
*   **Trải nghiệm đăng ký nhanh chóng & an toàn:** Biểu mẫu đăng ký trực quan với các trường thông tin tối giản nhưng đầy đủ để định danh khách hàng.
*   **Xác thực kích hoạt tài khoản bằng OTP:** Ngăn ngừa tài khoản rác (spam) và đảm bảo tính hợp lệ của địa chỉ Email bằng cách gửi mã OTP 6 số qua email.
*   **Hàng rào bảo mật ngay từ đầu:** Mã hóa mật khẩu bằng thuật toán băm an toàn (BCrypt) và ngăn chặn đăng ký trùng lặp thông tin nhạy cảm (như Email).

---

## 3. Chân dung Người dùng (User Personas & Stories)

### 3.1. Chân dung Người dùng
*   **Chủ nuôi thú cưng (Chị Lan, 28 tuổi):**
    *   *Bối cảnh:* Chị Lan mới nuôi một chú mèo Anh lông ngắn tên Bông. Chú mèo cần được đưa đi tiêm chủng vắc-xin định kỳ. Chị muốn đăng ký tài khoản trên ứng dụng MyPetClinic để đặt lịch tiêm phòng và tiện theo dõi sổ tiêm chủng trực tuyến.
    *   *Nhu cầu:* Form đăng ký dễ điền trên điện thoại di động, gửi OTP về email nhanh chóng và không bị lỗi tải trang khi kết nối mạng 4G yếu.
*   **Lễ tân phòng khám (Anh Nam, 25 tuổi):**
    *   *Bối cảnh:* Anh Nam thường xuyên hỗ trợ khách hàng đăng ký tài khoản trực tiếp tại quầy lễ tân nếu khách chưa có tài khoản khi đến khám lần đầu.
    *   *Nhu cầu:* Khách hàng có thể tự đăng ký qua mã QR dán tại quầy nhanh chóng hoặc anh Nam có thể hỗ trợ kiểm tra xem email của khách đã tồn tại trên hệ thống hay chưa thông qua giao diện đăng ký để tránh trùng lặp dữ liệu.

### 3.2. User Stories
*   Là một chủ nuôi thú cưng, tôi muốn đăng ký tài khoản bằng email cá nhân để tôi có thể quản lý lịch sử khám bệnh của thú cưng ở một nơi bảo mật duy nhất.
*   Là một người dùng mới, tôi muốn hệ thống gửi mã OTP xác thực về email ngay sau khi đăng ký để kích hoạt tài khoản của tôi, đảm bảo không có ai giả mạo email của tôi để tạo tài khoản.
*   Là một người dùng đang đăng ký, tôi muốn được thông báo ngay lập tức nếu email tôi nhập đã có người khác sử dụng, giúp tôi tiết kiệm thời gian và sử dụng đúng tài khoản của mình.

---

## 4. Phạm vi Tính năng (Scope of Work)

### 4.1. Trong phạm vi (In-Scope - MVP)
*   Form đăng ký gồm: Họ và tên, Email, Số điện thoại và Mật khẩu (đáp ứng độ mạnh mật khẩu).
*   Cơ chế xác thực hai bước (Two-step verification): Đăng ký -> Gửi email chứa OTP 6 chữ số -> Nhập OTP kích hoạt tài khoản (`IsActive = true`).
*   Mã OTP có thời hạn hiệu lực là 5 phút. Sau 5 phút, mã sẽ hết hiệu lực và người dùng phải yêu cầu gửi lại mã mới (Resend OTP).
*   Kiểm tra tính duy nhất của Email ngay tại tầng Backend.
*   Regex validation định dạng Email và Số điện thoại tại Client và Server.

### 4.2. Ngoài phạm vi (Out-of-Scope - Các phase tiếp theo)
*   Đăng ký tài khoản nhanh qua các mạng xã hội như Facebook, Apple ID (Phase 1 chỉ làm Google Login ở module riêng).
*   Đăng ký bằng số điện thoại nhận mã OTP qua tin nhắn SMS (do chi phí thuê cổng SMS Gateway cao).

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements)
*   **Tốc độ phản hồi cục bộ:** Giao diện phản hồi lỗi validation (nhập sai định dạng email, mật khẩu yếu) dưới **50ms** ngay sau khi người dùng dừng gõ.
*   **Thời gian gửi OTP:** Mã OTP phải được gửi đến hòm thư người dùng trong vòng **5 giây** kể từ khi hoàn tất đăng ký (ở điều kiện mạng thông thường).
*   **Bảo mật thông tin:** Mật khẩu người dùng phải được băm bằng thuật toán BCrypt với độ muối (work factor) tối thiểu là 11 trước khi lưu trữ vào database. Không lưu mật khẩu dạng text thô.
*   **Tính tương thích thiết bị:** Giao diện responsive hoạt động hoàn hảo trên các thiết bị di động phổ biến (iOS, Android) và các trình duyệt lớn (Chrome, Safari, Firefox, Edge).
