# 🚀 Product Requirements Document (PRD) - Profile Avatar Upload

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Trong hệ thống quản lý phòng khám thú y **MyPetClinic**, ảnh đại diện (User Avatar) không chỉ là một yếu tố trang trí giao diện đơn thuần, mà đóng vai trò quan trọng trong việc tăng tính nhân văn và cá nhân hóa trải nghiệm người dùng:
*   **Với Khách hàng:** Ảnh đại diện tạo sự gắn kết, giúp họ cá nhân hóa hồ sơ chủ nuôi trong các cuộc thảo luận hoặc khi chat trực tuyến với Bác sĩ thú y và Chatbot AI.
*   **Với Nhân viên & Bác sĩ:** Giúp khách hàng và đồng nghiệp dễ dàng nhận diện khuôn mặt bác sĩ điều trị hoặc lễ tân đang trực ca, tăng độ tin cậy và tính chuyên nghiệp trong giao dịch.

Tính năng **Tải lên ảnh đại diện** cung cấp cơ chế tải lên trực quan (bao gồm kéo thả và chọn tệp truyền thống), tự động tối ưu hóa hiển thị tức thì trên các vùng giao diện mà không cần làm mới (reload) trang, bảo đảm an toàn dữ liệu và tối ưu dung lượng máy chủ lưu trữ.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)
### 👩‍⚕️ Persona 1: Bác sĩ Lê Thị Mai (Bác sĩ thú y)
*   **Đặc điểm:** Thường xuyên sử dụng máy tính làm việc tại phòng khám. Muốn ảnh đại diện của mình hiển thị rõ ràng trên trang chủ để khách hàng tin tưởng khi đặt lịch khám.
*   **Mục tiêu:** Cần tải lên bức ảnh mặc áo blouse trắng lịch sự từ máy tính cá nhân.
*   **Nỗi đau (Pain Points):** Định dạng ảnh chụp từ máy chuyên nghiệp dung lượng quá lớn bị hệ thống từ chối mà không giải thích rõ nguyên nhân hoặc không có công cụ xem trước ảnh trước khi bấm lưu.

### 🧑‍💼 Persona 2: Nguyễn Tuấn Hải (Khách hàng nuôi thú cưng)
*   **Đặc điểm:** Thường sử dụng điện thoại thông minh để đặt lịch khám cho cún cưng.
*   **Mục tiêu:** Tải ảnh đại diện chụp chung với cún cưng lên tài khoản từ thư viện ảnh di động.
*   **Nỗi đau (Pain Points):** Thao tác kéo thả khó thực hiện trên điện thoại, nút bấm tải lên quá nhỏ và thời gian upload chậm qua kết nối 4G yếu.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Tải lên hình ảnh mới làm ảnh đại diện
*   **Là một** người dùng đã đăng nhập vào hệ thống MyPetClinic,
*   **Tôi muốn** kéo thả hoặc chọn một tệp hình ảnh từ thiết bị của mình để đặt làm ảnh đại diện,
*   **Để tôi** làm nổi bật danh tính và cá nhân hóa tài khoản cá nhân.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Hỗ trợ kéo thả tệp tin trực tiếp vào vùng nét đứt chỉ định (Drop Zone) hoặc click vào vùng đó để mở hộp thoại chọn tệp của hệ điều hành.
    *   **AC2:** Hiển thị màn hình xem trước (Image Preview) hình ảnh dạng hình tròn ngay khi tệp được chọn trước khi gửi lên máy chủ.
    *   **AC3:** Khi tệp đang được tải lên, hiển thị hiệu ứng Loading mờ kính và thanh tiến trình phần trăm (%) upload thực tế.
    *   **AC4:** Cập nhật ngay lập tức ảnh đại diện trên thanh Navbar điều hướng và Sidebar mà không cần F5 tải lại trang.

### User Story 2: Ràng buộc kích thước và định dạng tệp tin an toàn
*   **Là một** quản trị viên hệ thống,
*   **Tôi muốn** hệ thống chặn đứng mọi tệp tin có dung lượng quá lớn hoặc định dạng không an toàn,
*   **Để** tiết kiệm dung lượng lưu trữ trên máy chủ và ngăn chặn các mã độc (webshell) ẩn dưới dạng ảnh.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Chỉ chấp nhận các định dạng tệp ảnh thông dụng: `.jpg`, `.jpeg`, `.png`, `.gif`.
    *   **AC2:** Chặn đứng và báo lỗi rõ ràng nếu tệp tin vượt quá dung lượng **2MB** (2.097.152 bytes).
    *   **AC3:** Backend phải kiểm tra kỹ MIME Type của tệp tin nhận được để bảo đảm đó thực sự là một file ảnh (ngăn chặn file thực thi `.exe` giả mạo đuôi `.jpg`).

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Giao diện vùng kéo thả ảnh (Drag-and-Drop Dropzone) và xem trước ảnh dạng tròn (Preview circle).
*   API `POST /api/profile/avatar` nhận tệp nhị phân thông qua `multipart/form-data`.
*   Logic kiểm tra dung lượng (<2MB) và định dạng mở rộng tại cả Frontend và Backend.
*   Lưu trữ tệp tin trên đĩa cứng máy chủ dưới thư mục `wwwroot/uploads/avatars/`.
*   Tự động gán tên tệp tin độc nhất bằng định dạng UUID để tránh ghi đè dữ liệu.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Tính năng cắt ảnh trực tuyến (Image Cropper) trước khi tải lên (Sẽ được nghiên cứu phát triển thêm ở Phase tiếp theo).
*   Lưu trữ hình ảnh trên các dịch vụ đám mây bên ngoài như AWS S3 hoặc Cloudinary (MVP lưu tại máy chủ cục bộ wwwroot).
*   Xóa tệp tin ảnh cũ ngay lập tức (Để tránh lỗi mất ảnh khi rollback dữ liệu, ảnh cũ sẽ được xử lý quét dọn định kỳ bởi một background worker riêng).
