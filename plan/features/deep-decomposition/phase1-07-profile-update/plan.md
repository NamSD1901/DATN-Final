# 📝 Implementation Plan & Testing Strategy - Profile Details Update

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) toàn diện từ kiểm thử đơn vị (Unit Test), kiểm thử tích hợp (Integration Test) cho đến kiểm thử bảo mật IDOR và Rate Limiting.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự theo 4 giai đoạn khép kín nhằm bảo đảm tính bền vững của mã nguồn:

### 🛠️ Giai đoạn 1: Backend DTO, Validation & Services
*   **Bước 1.1:** Xây dựng cấu trúc `UpdateProfileDto` chứa các thuộc tính cho phép chỉnh sửa.
*   **Bước 1.2:** Viết bộ Validator bằng `FluentValidation` kiểm tra độ dài tên, Regex số điện thoại di động Việt Nam, giới tính hợp lệ và ngày sinh trong quá khứ.
*   **Bước 1.3:** Nâng cấp lớp `UserService` để cập nhật thực thể `User` và ép kiểu `DateTimeKind.Utc` cho ngày sinh để tránh xung đột múi giờ của PostgreSQL.
*   **Bước 1.4:** Viết các Unit Test đầu tiên bao phủ lớp Validator và Service.

### 🌐 Giai đoạn 2: API Endpoints & Security Integration
*   **Bước 2.1:** Khai báo Endpoint PUT `/api/profile` trong `ProfileController`.
*   **Bước 2.2:** Thiết lập logic giải mã định danh bảo mật từ JWT Token claims sử dụng `User.FindFirstValue(ClaimTypes.NameIdentifier)` thay vì nhận ID từ client.
*   **Bước 2.3:** Áp dụng chính sách Rate Limiting giới hạn 10 lần cập nhật / phút cho mỗi tài khoản để chống spam dữ liệu.

### 🎨 Giai đoạn 3: Vue 3 UI & Pinia State Integration
*   **Bước 3.1:** Viết Pinia store (`stores/profile.ts`) quản lý state, cờ `isDirty` và lưu trữ đệm dữ liệu nguyên bản từ API.
*   **Bước 3.2:** Dựng giao diện Profile cá nhân sử dụng Glassmorphism CSS, bố cục 2 cột cho Desktop và tự động chuyển về 1 cột trên Mobile.
*   **Bước 3.3:** Tích hợp logic Client-side validation thời gian thực để cảnh báo lỗi và vô hiệu hóa nút "Lưu thay đổi" ngay lập tức nếu dữ liệu sai.
*   **Bước 3.4:** Đăng ký Vue Router Guard (`onBeforeRouteLeave`) để cảnh báo nếu người dùng chuyển trang khi biểu mẫu đang ở trạng thái Dirty.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Hợp lệ (Functional Testing Cases)

#### TC-FUN-01: Cập nhật hồ sơ cá nhân với dữ liệu hợp lệ
*   **Mục tiêu:** Xác minh người dùng có thể cập nhật thông tin cá nhân của mình thành công.
*   **Dữ liệu đầu vào:**
    *   Họ tên: `"Nguyễn Văn Anh"`
    *   Số điện thoại: `"0987654321"`
    *   Địa chỉ: `"321 Lý Tự Trọng, Quận 1, TP HCM"`
    *   Giới tính: `1` (Nam)
    *   Ngày sinh: `"1990-12-25"`
*   **Các bước thực hiện:**
    1. Đăng nhập tài khoản khách hàng, lấy JWT Token.
    2. Gửi request `PUT /api/profile` với dữ liệu đầu vào trên kèm JWT trong header.
*   **Kết quả mong đợi:**
    *   Mã phản hồi trả về là `200 OK`.
    *   Cơ sở dữ liệu được cập nhật chính xác các thông tin trên.
    *   Gọi `GET /api/profile` ngay sau đó nhận lại chính xác dữ liệu mới.

#### TC-FUN-02: Cập nhật thất bại do Số điện thoại sai định dạng Việt Nam
*   **Mục tiêu:** Đảm bảo hệ thống chặn dữ liệu rác hoặc số điện thoại nước ngoài không thuộc diện liên lạc khẩn cấp của phòng khám.
*   **Dữ liệu đầu vào:** `phone: "012345678"` (Thiếu số), `phone: "08887776655"` (Quá dài), hoặc `phone: "19001008"` (Đầu số hotline, không phải di động).
*   **Các bước thực hiện:** Gửi request `PUT /api/profile` với số điện thoại lỗi.
*   **Kết quả mong đợi:**
    *   Mã phản hồi trả về là `400 Bad Request`.
    *   Nội dung trả về chỉ rõ trường `phone` vi phạm lỗi validation và có thông báo hướng dẫn.

#### TC-FUN-03: Cập nhật thất bại do Ngày sinh ở tương lai
*   **Mục tiêu:** Chặn các lỗi nhập liệu phi lý.
*   **Dữ liệu đầu vào:** `dateOfBirth: "2030-05-15"`
*   **Các bước thực hiện:** Gửi request `PUT /api/profile` với ngày sinh lớn hơn ngày hiện tại.
*   **Kết quả mong đợi:** Mã phản hồi trả về `400 Bad Request` kèm thông báo lỗi trường ngày sinh.

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Kiểm thử tấn công IDOR sửa chéo dữ liệu người dùng
*   **Mục tiêu:** Bảo đảm không ai có thể sửa đổi thông tin của tài khoản khác bằng cách cố ý can thiệp HTTP payload.
*   **Các bước thực hiện:**
    1. Tạo 2 tài khoản: User A (ID: `999...`) và User B (ID: `888...`).
    2. Đăng nhập bằng tài khoản User A để lấy JWT Token của User A.
    3. Sử dụng công cụ Postman để giả lập request `PUT /api/profile`. Cố gắng chèn thêm tham số `"id": "888..."` hoặc `"userId": "888..."` vào Body JSON hoặc Query Parameter của API, đi kèm Token của User A.
*   **Kết quả mong đợi:**
    *   Hệ thống xử lý cập nhật thành công nhưng **chỉ cập nhật hồ sơ của User A** (do ID được giải mã bắt buộc từ Token Claims của User A).
    *   Hồ sơ của User B hoàn toàn không bị ảnh hưởng.

#### TC-SEC-02: Gọi API khi chưa xác thực (Missing Token)
*   **Mục tiêu:** Chặn truy cập công cộng.
*   **Các bước thực hiện:** Gửi request `GET /api/profile` hoặc `PUT /api/profile` mà không truyền kèm Header `Authorization`.
*   **Kết quả mong đợi:** Mã phản hồi trả về ngay lập tức là `401 Unauthorized`.

#### TC-SEC-03: Kiểm thử Rate Limiting (Spam cập nhật)
*   **Mục tiêu:** Chống bot spam hoặc script phá hoại DB.
*   **Các bước thực hiện:** Gửi liên tục 15 request `PUT /api/profile` trong vòng 10 giây bằng tài khoản đã đăng nhập.
*   **Kết quả mong đợi:**
    *   10 request đầu tiên xử lý thành công (hoặc báo lỗi validate bình thường).
    *   Từ request thứ 11 trở đi, API trả về ngay lập tức mã trạng thái `429 Too Many Requests` mà không truy cập cơ sở dữ liệu.
