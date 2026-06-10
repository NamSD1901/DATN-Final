# 📝 Implementation Plan & Testing Strategy - Profile Avatar Upload

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) toàn diện từ kiểm thử biên kích thước, định dạng tệp tin cho đến kiểm thử bảo mật nâng cao chống Path Traversal và Web Shell.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự theo 4 giai đoạn khép kín nhằm bảo đảm tính bền vững của mã nguồn:

### Giai đoạn 1: Thiết lập cấu trúc hạ tầng & Backend Controllers
*   **Bước 1.1:** Khai báo thư mục lưu trữ tĩnh `wwwroot/uploads/avatars/` và cấp quyền Đọc/Ghi (Read/Write) cho tài khoản chạy dịch vụ Web API.
*   **Bước 1.2:** Viết endpoint `POST /api/profile/avatar` trong `ProfileController` sử dụng tham số `IFormFile avatarFile` nhận tệp nhị phân.
*   **Bước 1.3:** Viết hàm `UpdateAvatarAsync` trong `UserService` để cập nhật đường dẫn ảnh đại diện vào DB.

### Giai đoạn 2: Tích hợp Lớp Bảo mật & Validation
*   **Bước 2.1:** Cấu hình Kestrel `MaxRequestBodySize` và IIS `maxAllowedContentLength` giới hạn cứng kích thước payload request tối đa 2MB.
*   **Bước 2.2:** Viết bộ lọc đuôi file mở rộng (White-list) chỉ cho phép `.jpg, .jpeg, .png, .gif`.
*   **Bước 2.3:** Áp dụng thuật toán sinh UUID cho tên file mới để chặn đứng Path Traversal.
*   **Bước 2.4:** Viết các Unit Test kiểm thử logic lưu file và cập nhật đường dẫn database.

### Giai đoạn 3: Xây dựng Giao diện & Tích hợp State Store
*   **Bước 3.1:** Viết Pinia store (`stores/avatar.ts`) quản lý cờ `uploading`, `progress` (%) sử dụng axios `onUploadProgress`.
*   **Bước 3.2:** Dựng giao diện Modal Drag & Drop Dropzone mờ kính Glassmorphism, bo góc mượt mà, hỗ trợ hiệu ứng hover phát sáng neon khi kéo file qua.
*   **Bước 3.3:** Viết logic Client-side validation kiểm tra nhanh đuôi file và kích thước tệp tin trước khi tải lên.
*   **Bước 3.4:** Tích hợp cơ chế tự động giải phóng Object URL (`URL.revokeObjectURL`) sau khi kết thúc vòng đời upload.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Kích thước (Functional Testing Cases)

#### TC-FUN-01: Tải lên ảnh đại diện hợp lệ (.png, < 1MB)
*   **Mục tiêu:** Xác minh tính năng upload hoạt động bình thường với ảnh hợp chuẩn.
*   **Dữ liệu đầu vào:** File `cat-avatar.png`, kích thước `450KB`.
*   **Các bước thực hiện:**
    1. Đăng nhập hệ thống, mở Modal Avatar.
    2. Kéo thả file `cat-avatar.png` vào vùng chỉ định.
    3. Nhấn nút "Tải Lên Máy Chủ".
*   **Kết quả mong đợi:**
    *   Thanh tiến trình chạy mượt mà từ 0% đến 100%.
    *   API trả về `200 OK` kèm `avatarUrl` dạng `/uploads/avatars/uuid_cat-avatar.png`.
    *   Ảnh đại diện trên Navbar thay đổi sang ảnh mới ngay lập tức.
    *   Tệp tin thực tế được ghi vào thư mục `wwwroot/uploads/avatars` trên máy chủ.

#### TC-FUN-02: Chặn tệp tin vượt quá dung lượng cho phép (> 2MB)
*   **Mục tiêu:** Đảm bảo hệ thống từ chối tải tệp tin quá lớn gây tốn băng thông.
*   **Dữ liệu đầu vào:** File ảnh chất lượng cao `my-dog.jpg` dung lượng `4.5MB`.
*   **Các bước thực hiện:** Kéo thả tệp tin `my-dog.jpg` vào vùng Dropzone.
*   **Kết quả mong đợi:**
    *   Hệ thống không gửi file lên server (Client-side block).
    *   Vùng Dropzone rung lắc nhẹ (Shake animation) kèm thông báo viền đỏ: *"Kích thước ảnh không được vượt quá 2MB."*.

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Kiểm thử tấn công Upload Web Shell (Bypass Extension)
*   **Mục tiêu:** Bảo đảm hacker không thể tải lên script độc hại trá hình.
*   **Dữ liệu đầu vào:** Tệp tin chứa mã độc php/asp: `webshell.php` hoặc `backdoor.jpg.exe`.
*   **Các bước thực hiện:** Gửi request `POST /api/profile/avatar` bằng công cụ Postman chứa tệp tin trên.
*   **Kết quả mong đợi:**
    *   API trả về mã lỗi `400 Bad Request`.
    *   Đường dẫn máy chủ không ghi nhận bất cứ tệp tin thực thi nào.

#### TC-SEC-02: Kiểm thử tấn công Path Traversal ghi đè tệp tin hệ thống
*   **Mục tiêu:** Đảm bảo tên file được làm sạch hoàn toàn trên đĩa cứng.
*   **Dữ liệu đầu vào:** Tệp ảnh có tên hiểm độc: `../../../../appsettings.json` hoặc `..\..\..\..\Program.cs`.
*   **Các bước thực hiện:** Tải lên tệp ảnh trên qua API.
*   **Kết quả mong đợi:**
    *   API lưu tệp tin thành công với mã `200 OK`.
    *   Tuy nhiên, tên tệp tin thực tế được lưu trên đĩa cứng là dạng UUID ngẫu nhiên (Ví dụ: `e4d5c6b7-a8b9..._appsettings.json` hoặc chỉ là chuỗi UUID ngẫu nhiên tùy cấu hình).
    *   Tệp cấu hình hệ thống `appsettings.json` ở thư mục gốc hoàn toàn không bị ảnh hưởng hay ghi đè.

#### TC-SEC-03: Gọi API upload khi chưa xác thực (Missing Token)
*   **Các bước thực hiện:** Gửi request `POST /api/profile/avatar` đính kèm file ảnh nhưng không gửi header `Authorization`.
*   **Kết quả mong đợi:** API trả về mã lỗi `401 Unauthorized` ngay lập tức.
