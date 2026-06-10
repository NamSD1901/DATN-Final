# 🐾 CS-06: API Testing with Postman & Swagger (Kiểm thử và Tích hợp API)

Tài liệu này hướng dẫn các thành viên trong nhóm (đặc biệt là QA: Lâm, Hạnh và Frontend: Phương) cách làm việc với API của dự án thông qua hai công cụ chính là **Swagger UI** và **Postman**.

---

## 1. Swagger UI - Tài liệu API Tự động

Khi chạy Backend dưới local (`dotnet run`), hệ thống tự động sinh ra tài liệu Swagger tại địa chỉ:
👉 **`http://localhost:5000/swagger/index.html`** (hoặc cổng local tương ứng).

### Cách sử dụng Swagger:
1. **Xem danh sách endpoint:** Các endpoint được nhóm lại theo Controller (ví dụ: `Pets`, `Bookings`, `Users`).
2. **Xem cấu trúc DTO:** Click vào từng endpoint để xem cấu trúc Request Body cần gửi lên và Response Object nhận về.
3. **Thử nghiệm trực tiếp (Try it out):**
   * Nhấn nút **Try it out** ở góc phải của endpoint.
   * Nhập giá trị tham số hoặc sửa đổi request body.
   * Nhấn **Execute** để gửi request thật đến Backend và nhận kết quả phản hồi trực tiếp (Status Code, Response Headers, Response Body).

---

## 2. Postman Workflow - Kiểm thử tự động và Seed dữ liệu nhanh

Nhóm duy trì bộ sưu tập API chung (Postman Collection) đặt tại thư mục [scratch/postman](file:///e:/DATN/MyPetClinic/scratch/postman) hoặc chia sẻ qua tài khoản nhóm.

### Quy trình Bàn giao & Kiểm thử API:
1. **Đối với Backend (Nam):** Mỗi khi viết xong 1 API mới:
   * Thêm request đó vào Postman Collection.
   * Thiết lập đầy đủ các biến môi trường (ví dụ: `{{baseUrl}}`).
   * Xuất (Export) bộ sưu tập mới nhất đè lên thư mục `scratch/postman`.
2. **Đối với QA/Frontend (Lâm, Hạnh, Phương):**
   * Tải (Import) file collection từ thư mục dự án vào ứng dụng Postman của mình.
   * Chọn môi trường phù hợp (Local: `http://localhost:5000` hoặc Staging: `https://mypetclinic-staging.azurewebsites.net`).
   * Thực hiện gọi thử để kiểm chứng tính năng.

### Xử lý Token xác thực (Bearer Token) trên Postman:
Hầu hết các API nghiệp vụ phòng khám (như đặt lịch, kê đơn) yêu cầu đăng nhập.
1. Gửi request đăng nhập `POST /api/v1/auth/login`.
2. Lấy chuỗi mã Token nhận được ở response body.
3. Trong Postman, click vào thư mục cha của Collection -> Chọn tab **Authorization** -> Chọn Type là **Bearer Token** -> Dán mã token vào ô **Token**.
4. Các request con bên trong chỉ cần chọn Authorization là **Inherit auth from parent** để tự động đính kèm token vào header khi gọi API.

---

## 3. Bài tập Thực Hành Đạt Yêu Cầu CS-06

- [ ] Mở thành công trang Swagger UI dưới local và chạy thử 1 request lấy danh sách thú cưng.
- [ ] Import thành công Postman Collection của nhóm vào Postman cá nhân.
- [ ] Thực hiện thành công chuỗi thao tác: Đăng nhập -> Lấy token -> Đính kèm Token vào Collection -> Gọi thành công API tạo mới một lịch đặt khám.
- [ ] Sử dụng Postman để kiểm tra hành vi của API khi gửi dữ liệu sai định dạng (ví dụ: thiếu thông tin bắt buộc) và chụp lại mã lỗi nhận được.
