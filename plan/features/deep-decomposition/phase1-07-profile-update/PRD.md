# 🚀 Product Requirements Document (PRD) - Profile Details Update

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Trong hệ thống quản lý phòng khám thú y **MyPetClinic**, hồ sơ cá nhân (User Profile) đóng vai trò là danh tính số của mỗi người dùng (Khách hàng, Lễ tân, Bác sĩ thú y, Thu ngân, Admin). Việc duy trì thông tin cá nhân chính xác và cập nhật là vô cùng quan trọng nhằm đảm bảo:
*   **Liên lạc thông suốt:** Số điện thoại và địa chỉ chính xác giúp hệ thống gửi nhắc lịch hẹn tiêm phòng, khám bệnh, hoặc liên lạc khẩn cấp khi thú cưng có diễn biến xấu trong lúc nội trú.
*   **Bảo mật danh tính:** Đối chiếu thông tin chính xác phục vụ cho các giao dịch tài chính (xuất hóa đơn y tế) và quyền sở hữu thú cưng (Pet Portfolio).
*   **Cá nhân hóa trải nghiệm:** Hiển thị tên, giới tính và ngày sinh giúp tối ưu hóa giao diện và các chương trình chăm sóc khách hàng thân thiết.

Mục tiêu của tính năng này là cung cấp một giao diện quản lý hồ sơ cá nhân trực quan, bảo mật tuyệt đối chống tấn công chiếm đoạt dữ liệu chéo (IDOR), và mang lại trải nghiệm người dùng mượt mà với hiệu ứng Glassmorphism hiện đại.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)
### 👩‍💼 Persona 1: Nguyễn Thu Trang (Khách hàng - Chủ thú cưng)
*   **Đặc điểm:** Bận rộn, thường xuyên đặt lịch tiêm phòng cho chú mèo Anh lông ngắn qua di động.
*   **Mục tiêu:** Cần cập nhật số điện thoại mới để không bỏ lỡ các cuộc gọi xác nhận từ lễ tân phòng khám, và điền đúng địa chỉ để bác sĩ có thể đến khám tại nhà khi có yêu cầu.
*   **Nỗi đau (Pain Points):** Lo sợ lộ thông tin cá nhân hoặc giao diện trên điện thoại quá phức tạp, khó thao tác bằng một tay.

### 🥼 Persona 2: Bác sĩ Trần Quốc Anh (Bác sĩ thú y)
*   **Đặc điểm:** Sử dụng máy tính bảng tại phòng khám, thao tác nhanh khi di chuyển giữa các phòng điều trị.
*   **Mục tiêu:** Cập nhật thông tin cá nhân và ngày sinh để đồng bộ với hồ sơ nhân sự, hiển thị danh xưng chính xác trên bệnh án điện tử.
*   **Nỗi đau (Pain Points):** Hệ thống phản hồi chậm hoặc không lưu được dữ liệu khi chuyển tab đột ngột.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Xem thông tin hồ sơ hiện tại
*   **Là một** người dùng đã đăng nhập vào hệ thống MyPetClinic,
*   **Tôi muốn** xem đầy đủ các thông tin cá nhân hiện tại của mình bao gồm Họ tên, Email, Số điện thoại, Địa chỉ, Giới tính, Ngày sinh và Ảnh đại diện,
*   **Để tôi** kiểm tra xem thông tin liên hệ của mình đã chính xác hay chưa.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Giao diện tải thông tin nhanh chóng (< 300ms) ngay khi truy cập trang Profile.
    *   **AC2:** Trường **Email** phải hiển thị ở trạng thái chỉ đọc (Read-only) và có icon ổ khóa kèm tooltip giải thích lý do không thể chỉnh sửa trực tiếp (để bảo vệ định danh tài khoản).
    *   **AC3:** Định dạng hiển thị ngày sinh thân thiện với người Việt (ví dụ: `15/05/1995`) thay vì chuỗi ISO thô.

### User Story 2: Chỉnh sửa và cập nhật hồ sơ
*   **Là một** người dùng,
*   **Tôi muốn** có thể chuyển đổi sang chế độ chỉnh sửa để cập nhật Họ tên, Số điện thoại, Địa chỉ, Giới tính, Ngày sinh,
*   **Để tôi** cập nhật thông tin mới nhất của mình lên hệ thống.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Hệ thống cung cấp nút "Chỉnh sửa" để chuyển đổi form từ trạng thái hiển thị phẳng sang trạng thái có thể nhập liệu.
    *   **AC2:** Backend và Frontend phải đồng bộ bộ kiểm tra dữ liệu đầu vào (Validation Rules):
        *   `FullName`: Không được trống, từ 2 đến 100 ký tự, không chứa ký tự đặc biệt vô nghĩa.
        *   `Phone`: Bắt buộc phải là định dạng số điện thoại Việt Nam (10 chữ số, bắt đầu bằng 03, 05, 07, 08, 09).
        *   `DateOfBirth`: Phải là một ngày trong quá khứ (không được lớn hơn ngày hiện tại và tối thiểu là năm 1920).
    *   **AC3:** Khi đang sửa đổi dữ liệu mà người dùng nhấn "Hủy bỏ", hệ thống phải khôi phục lại dữ liệu ban đầu từ API và đóng chế độ chỉnh sửa mà không lưu thay đổi.
    *   **AC4:** Khi cập nhật thành công, hiển thị Toast thông báo đẹp mắt và chuyển form về chế độ xem chỉ đọc.

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   API lấy thông tin profile của người dùng hiện tại thông qua JWT claims.
*   API cập nhật thông tin profile (Họ tên, SĐT, Địa chỉ, Giới tính, Ngày sinh).
*   Giao diện Profile quản lý các trường thông tin trên với phong cách Glassmorphism.
*   Xử lý validate dữ liệu chặt chẽ ở cả Frontend (Vue 3 Form) và Backend (FluentValidation).
*   Chống tấn công chéo IDOR bằng cách bắt buộc giải mã UserId từ token/cookie thay vì nhận parameter từ client.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Thay đổi địa chỉ Email trực tiếp (Tính năng này yêu cầu quy trình bảo mật riêng với OTP kích hoạt sang Email mới để tránh chiếm đoạt tài khoản).
*   Thay đổi Mật khẩu (Thuộc tính năng Đổi mật khẩu độc lập trong `phase1-02-login`).
*   Tải lên Avatar (Thuộc tính năng `phase1-08-profile-avatar` xử lý luồng upload file hình ảnh riêng).

---

## 5. Yêu cầu Phi chức năng & Bảo mật (Non-Functional Requirements)

*   **Thời gian phản hồi (Response Time):** API GET và PUT profile phải phản hồi trong vòng `< 200ms` dưới điều kiện mạng bình thường.
*   **Giao diện di động (Mobile responsiveness):** Thiết kế Responsive hoàn chỉnh trên Mobile (breakpoint `< 640px`) và Tablet (breakpoint `< 1024px`) với layout 1 cột thay vì 2 cột như Desktop.
*   **Bảo mật dữ liệu cá nhân (GDPR & Cybersecurity):**
    *   Mã hóa đường truyền TLS/HTTPS cho mọi API giao tiếp.
    *   Tránh rò rỉ dữ liệu nhạy cảm (Không trả về Hash mật khẩu hoặc các Salt key trong API GetProfile).
    *   Giới hạn tần suất gọi API (Rate Limiting) để tránh brute-force cập nhật thông tin liên tục (tối đa 10 lần cập nhật/phút).
