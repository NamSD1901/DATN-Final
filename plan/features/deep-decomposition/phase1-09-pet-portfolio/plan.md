# 📝 Implementation Plan & Testing Strategy - Pet Portfolio Management

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) toàn diện bao phủ các kịch bản kiểm thử tích hợp liên thông, kiểm thử biên ngày sinh/cân nặng và kiểm thử bảo mật IDOR.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự theo 4 giai đoạn khép kín nhằm bảo đảm chất lượng và tính bảo mật của tính năng:

### Giai đoạn 1: Cơ sở dữ liệu & C# Core Layer
*   **Bước 1.1:** Tạo bảng `Pets` trong PostgreSQL với các ràng buộc khóa ngoại `OwnerId` tham chiếu đến `Users(Id)`.
*   **Bước 1.2:** Thiết lập non-clustered index trên cặp trường `(OwnerId, IsDeleted)` để tối ưu hóa truy vấn tìm kiếm.
*   **Bước 1.3:** Định nghĩa Entity `Pet.cs` và ánh xạ DBContext trong tầng Infrastructure.
*   **Bước 1.4:** Viết các lớp DTO (`CreatePetDto`, `UpdatePetDto`, `PetDto`) và cài đặt FluentValidation cho các quy tắc kiểm tra nghiệp vụ.

### Giai đoạn 2: Lớp Dịch vụ & Xử lý Bảo mật IDOR
*   **Bước 2.1:** Cài đặt giao diện `IPetRepository` và lớp thực thi `PetRepository` có hỗ trợ hàm xóa mềm `SoftDeletePetAsync`.
*   **Bước 2.2:** Viết logic trong `PetService` thực hiện đối chiếu quyền sở hữu `pet.OwnerId == ownerId` để chặn đứng IDOR trước khi lưu.
*   **Bước 2.3:** Khai báo API Controller `MyPetsController` với thuộc tính phân quyền vai trò `[Authorize(Roles = "customer")]`.
*   **Bước 2.4:** Viết xUnit Unit Tests trong [PetServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/PetServiceTests.cs) bảo đảm logic IDOR và Soft Delete hoạt động đúng 100%.

### Giai đoạn 3: Giao diện Vue 3 & State Management
*   **Bước 3.1:** Viết Pinia store (`stores/pets.ts`) điều phối trạng thái tải danh sách reactive và lưu cache.
*   **Bước 3.2:** Dựng giao diện thẻ thú cưng (Pet Cards Grid) phong cách Glassmorphism và màn hình trống (Empty state) thân thiện.
*   **Bước 3.3:** Viết form nhập liệu Modal có xử lý client-side validation và tự động gán avatar mặc định theo Loài.
*   **Bước 3.4:** Tích hợp modal xác nhận trước khi thực hiện hành động Xóa mềm.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Hợp lệ (Functional Testing Cases)

#### TC-FUN-01: Đăng ký thành công thú cưng mới hợp lệ
*   **Mục tiêu:** Kiểm tra luồng thêm mới thú cưng thành công.
*   **Dữ liệu đầu vào:**
    *   `name`: "Bé Leo", `species`: "dog", `breed`: "Poodle", `gender`: "Male"
    *   `birthDate`: "2024-05-15", `weight`: 4.2
*   **Các bước thực hiện:**
    1. Đăng nhập tài khoản Customer A, lấy token JWT.
    2. Gọi API `POST /api/mypets` truyền kèm dữ liệu trên và JWT trong header.
*   **Kết quả mong đợi:**
    *   API trả về mã `200 OK` (success: true).
    *   Kiểm tra database bảng `Pets` xuất hiện bản ghi mới có `OwnerId` khớp chính xác với ID của Customer A.

#### TC-FUN-02: Chặn thêm thú cưng với ngày sinh ở tương lai
*   **Dữ liệu đầu vào:** `birthDate: "2030-10-10"` (Ngày sinh trong tương lai).
*   **Các bước thực hiện:** Gửi request `POST /api/mypets` với dữ liệu trên.
*   **Kết quả mong đợi:** API trả về mã lỗi `400 Bad Request` chỉ rõ lỗi tại trường `BirthDate`.

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Kiểm thử tấn công IDOR xem chi tiết thú cưng của người khác
*   **Mục tiêu:** Đảm bảo dữ liệu riêng tư của khách hàng được bảo vệ tuyệt đối.
*   **Kịch bản giả lập:**
    *   Customer A sở hữu thú cưng ID `12`.
    *   Customer B sở hữu thú cưng ID `15`.
*   **Các bước thực hiện:**
    1. Đăng nhập hệ thống bằng tài khoản Customer B để lấy token JWT của Customer B.
    2. Sử dụng Postman gửi request `GET /api/mypets/12` (Yêu cầu xem chi tiết thú cưng của Customer A), gửi kèm token của Customer B.
*   **Kết quả mong đợi:**
    *   Hệ thống chặn đứng yêu cầu.
    *   API trả về mã lỗi `404 Not Found` (hoặc `403 Forbidden`) để tránh rò rỉ dữ liệu.

#### TC-SEC-02: Kiểm thử tấn công IDOR sửa thông tin hoặc xóa thú cưng của người khác
*   **Các bước thực hiện:**
    1. Sử dụng token của Customer B.
    2. Gửi request `PUT /api/mypets/12` hoặc `DELETE /api/mypets/12`.
*   **Kết quả mong đợi:**
    *   API từ chối xử lý, trả về mã lỗi `403 Forbidden` hoặc `400 Bad Request`.
    *   Bản ghi thú cưng ID `12` của Customer A trong DB hoàn toàn không bị thay đổi thông tin hay bị đánh dấu xóa mềm (`IsDeleted` vẫn là `false`).

#### TC-SEC-03: Kiểm thử Rate Limiting (Spam tạo thú cưng)
*   **Các bước thực hiện:** Viết script gửi liên tiếp 10 request `POST /api/mypets` tạo thú cưng trong vòng 5 giây dưới tài khoản Customer A.
*   **Kết quả mong đợi:**
    *   Các request đầu tiên tạo thành công.
    *   Request từ lượt thứ 6 trở đi trong phút đó bị chặn ngay tại Middleware với mã lỗi `429 Too Many Requests`.
