# 🚀 Product Requirements Document (PRD) - Pet Portfolio Management

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Trong hệ thống phòng khám thú y **MyPetClinic**, thú cưng chính là đối tượng phục vụ trung tâm. Tính năng **Quản lý Hồ sơ Thú Cưng (Pet Portfolio Management)** đóng vai trò là "chìa khóa" mở ra toàn bộ hoạt động nghiệp vụ y tế:
*   **Điểm xuất phát dịch vụ:** Một hồ sơ thú cưng được đăng ký đầy đủ thông tin (loài, giống, ngày sinh, cân nặng, tiền sử dị ứng) là điều kiện bắt buộc để thực hiện đặt lịch khám y tế hoặc lịch tiêm phòng vắc-xin.
*   **Bệnh án điện tử liên thông:** Hồ sơ này kết nối trực tiếp với lịch sử khám lâm sàng của Bác sĩ, danh mục thuốc điều trị và lịch sử tiêm chủng giúp tối ưu hóa chẩn đoán.
*   **Trải nghiệm khách hàng:** Giúp khách hàng (chủ nuôi) dễ dàng theo dõi chỉ số cân nặng, ghi chú dị ứng của nhiều thú cưng trong cùng một tài khoản.

Mục tiêu của phân hệ này là cung cấp một giao diện CRUD (Thêm, Đọc, Sửa, Xóa mềm) hồ sơ thú cưng trực quan dạng thẻ, phản hồi tức thời, và bảo mật IDOR tuyệt đối để chủ nuôi khác không thể truy cập trái phép thông tin thú cưng của nhau.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)
### 👩‍💼 Persona 1: Chị Vũ Hoàng Lan (Chủ nuôi 3 chú mèo)
*   **Đặc điểm:** Yêu thương động vật, sở hữu nhiều bé mèo (Anh lông ngắn, mèo Ta) với các chế độ dinh dưỡng và sức khỏe khác nhau.
*   **Mục tiêu:** Muốn quản lý danh sách cả 3 bé mèo trong một Dashboard cá nhân, dễ dàng cập nhật cân nặng sau mỗi lần khám, và lưu chú ý dị ứng thuốc của từng bé.
*   **Nỗi đau (Pain Points):** Giao diện quản lý rườm rà, nút thêm thú cưng khó tìm thấy trên điện thoại hoặc không có ảnh/avatar đại diện cho mỗi bé để phân biệt trực quan.

### 🥼 Persona 2: Bác sĩ Nguyễn Minh Đức (Bác sĩ thú y)
*   **Đặc điểm:** Chẩn đoán bệnh nhanh, cần tra cứu chính xác thông tin cơ bản của thú cưng (như giống, tuổi, cân nặng, nhóm máu, trạng thái triệt sản) trước khi tiến hành phẫu thuật.
*   **Mục tiêu:** Xem được hồ sơ thú cưng đồng bộ với thông tin chủ nuôi khai báo để hạn chế hỏi lại khách hàng.
*   **Nỗi đau (Pain Points):** Dữ liệu cân nặng hoặc dị ứng bị thiếu hoặc sai lệch do khách hàng không thể tự cập nhật hoặc giao diện cập nhật quá khó dùng.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Thêm mới thú cưng vào tài khoản
*   **Là một** khách hàng đã đăng nhập,
*   **Tôi muốn** nhập thông tin chi tiết của thú cưng mới (Tên, Loài, Giống, Giới tính, Ngày sinh, Cân nặng, Màu lông, Nhóm máu, Trạng thái triệt sản, Mã Microchip, Ghi chú dị ứng),
*   **Để tôi** đăng ký hồ sơ y tế cho thú cưng tại phòng khám.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Các trường bắt buộc nhập: *Tên thú cưng*, *Loài* (Chó/Mèo/Khác), *Giới tính* (Đực/Cái).
    *   **AC2:** Trường ngày sinh (`BirthDate`) phải là ngày trong quá khứ và không được vượt quá 30 năm trước.
    *   **AC3:** Trường cân nặng (`Weight`) phải lớn hơn 0 và hỗ trợ định dạng số thập phân (ví dụ: `4.5` kg).
    *   **AC4:** Mã Microchip nếu có nhập phải là duy nhất trên toàn hệ thống (nếu trùng mã sẽ hiển thị cảnh báo lỗi).

### User Story 2: Xem danh sách và Chi tiết hồ sơ thú cưng
*   **Là một** khách hàng,
*   **Tôi muốn** xem toàn bộ danh sách thú cưng của mình hiển thị dưới dạng thẻ Grid trực quan,
*   **Để tôi** quản lý tổng quan hoặc chọn nhanh một bé để xem lịch sử bệnh án.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Chỉ hiển thị những thú cưng thuộc quyền sở hữu của chính chủ nuôi đang đăng nhập (`OwnerId == CurrentUserId`) và chưa bị xóa mềm (`IsDeleted == false`).
    *   **AC2:** Thẻ thú cưng hiển thị tóm tắt: Tên, Loài, Ảnh đại diện mặc định theo loài, Tuổi tính tự động từ ngày sinh (ví dụ: *"2 tuổi 3 tháng"*).
    *   **AC3:** Nhấp vào thẻ sẽ mở trang chi tiết hiển thị đầy đủ các thông tin chuyên môn (Nhóm máu, Triệt sản, Dị ứng).

### User Story 3: Cập nhật và Xóa mềm hồ sơ thú cưng
*   **Là một** khách hàng,
*   **Tôi muốn** chỉnh sửa thông tin hoặc xóa bớt hồ sơ thú cưng đã không còn nuôi,
*   **Để** giữ danh sách hồ sơ luôn sạch sẽ.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Hỗ trợ form cập nhật đầy đủ các trường thông tin ngoại trừ `OwnerId`.
    *   **AC2:** Khi click Xóa, hệ thống phải xuất hiện pop-up xác nhận: *"Bạn có chắc chắn muốn xóa hồ sơ của bé [Tên] không? Lịch sử y tế sẽ được lưu trữ nội bộ."*
    *   **AC3:** Thao tác xóa thực hiện **Xóa mềm (Soft Delete)**. Đổi thuộc tính `IsDeleted = true` trong database. Bản ghi vẫn nằm trong DB để đối chiếu hóa đơn tài chính và lịch sử bệnh án lâm sàng của bác sĩ, nhưng biến mất khỏi giao diện khách hàng.

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Bộ API RESTful quản lý hồ sơ thú cưng (`GET /api/mypets`, `POST /api/mypets`, `PUT /api/mypets/{id}`, `DELETE /api/mypets/{id}`).
*   Cơ chế phân quyền chặt chẽ: Chỉ chủ sở hữu thực sự của thú cưng mới được gọi API cập nhật hoặc xóa (IDOR protection).
*   Giao diện thẻ thú cưng (Pet Card Grid) phong cách Glassmorphism.
*   Form thêm/sửa thú cưng có validation đầu vào và tính tuổi tự động.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Tính năng upload ảnh đại diện riêng cho từng thú cưng (Sẽ sử dụng avatar mặc định theo Loài ở MVP này).
*   Quản lý phác đồ điều trị chi tiết (Nghiệp vụ dành riêng cho phân hệ điều trị lâm sàng của Bác sĩ).
