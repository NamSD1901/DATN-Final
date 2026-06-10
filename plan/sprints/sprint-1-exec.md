# 🚀 Kế Hoạch Thực Thi Chi Tiết - Sprint 1

## 🎯 Mục Tiêu Sprint 1
Thiết lập toàn bộ khung kiến trúc phần mềm cho Backend (.NET 8 Clean Architecture) và Frontend (Vue 3 Single Page Application), cấu hình cơ sở dữ liệu PostgreSQL ban đầu, thiết lập CI/CD và hoàn thành luồng xác thực (Authentication) cốt lõi (Đăng ký, Đăng nhập, Quên mật khẩu).

---

## 🛠️ Chi Tiết Các Bước Thực Thi (Step-by-Step)

### 📂 1. Thiết lập Cấu trúc dự án (T1, T2, T3)
#### 🖥️ Backend (.NET 8 Clean Architecture) - *Nam thực hiện*
1. Khởi tạo Solution `MyPetClinic.slnx` và 4 dự án thành viên:
   * **Domain:** Chứa Entities (`User.cs`, `Role.cs`), Value Objects, Enums.
   * **Application:** Định nghĩa DTOs, Interfaces (`IUserRepository`, `ITokenService`), Mapper, Validators.
   * **Infrastructure:** Hiện thực `ApplicationDbContext`, repositories, cấu hình JWT Token và BCrypt.
   * **WebApi:** Chứa các Controller, `Program.cs` cấu hình Dependency Injection (DI) và Middleware.
2. Cấu hình EF Core kết nối PostgreSQL. Tạo Migration đầu tiên để khởi tạo bảng `Roles` và `Users`.
3. Viết tập lệnh Seed dữ liệu ban đầu cho các Role (Admin, Doctor, Receptionist, Customer).

#### 🌐 Frontend (Vue 3 + TS + Vite) - *Phương thực hiện*
1. Tạo dự án Vue 3 sử dụng Vite và TypeScript: `npx -y create-vite-app@latest ./` (nếu setup mới) hoặc cấu hình lại khung mục lục hiện tại.
2. Cài đặt các thư viện thiết yếu: `vue-router`, `pinia`, `axios`, `jwt-decode`.
3. Định cấu hình Layout dùng chung: `DefaultLayout.vue`, `AuthLayout.vue`.
4. Thiết lập Axios Interceptors để tự động đính kèm JWT Token vào Header của mọi request.

#### 🔄 CI/CD Pipeline - *Lâm thực hiện*
1. Tạo tệp cấu hình `.github/workflows/dotnet-build-test.yml` để tự động build và chạy test backend khi có Pull Request vào nhánh `develop`.
2. Tạo tệp `.github/workflows/vue-build.yml` để build thử dự án frontend, cảnh báo lỗi TypeScript hoặc lint.

---

### 🔑 2. Hiện thực hóa chức năng Đăng ký tài khoản (T4, T5)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoint:** `POST /api/v1/accounts/register`
* **Request DTO:**
  ```json
  {
    "fullName": "Nguyen Van A",
    "email": "customer@example.com",
    "password": "SecurePassword123",
    "phone": "0987654321"
  }
  ```
* **Logic xử lý:**
  1. Kiểm tra định dạng Email và Số điện thoại (sử dụng FluentValidation).
  2. Kiểm tra email đã tồn tại trong database chưa. Nếu rồi, trả về lỗi `400 Bad Request`.
  3. Băm mật khẩu bằng BCrypt (Work Factor = 11).
  4. Gán vai trò mặc định là `Customer`.
  5. Lưu thực thể `User` vào PostgreSQL.

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `Register.vue`
* **Giao diện:** Form đăng ký trực quan với các trường: Họ tên, Email, Số điện thoại, Mật khẩu, Xác nhận mật khẩu. Có hiển thị/ẩn mật khẩu bằng icon mắt.
* **Validation:** Kiểm tra client-side (độ dài mật khẩu > 6 ký tự, định dạng email, mật khẩu khớp nhau).
* **Luồng đi:** Gửi request $\rightarrow$ Nếu thành công $\rightarrow$ Hiển thị thông báo thành công và chuyển hướng sang trang Đăng nhập.

---

### 🔓 3. Hiện thực hóa chức năng Đăng nhập hệ thống (T6, T7)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoint:** `POST /api/v1/accounts/login`
* **Request DTO:**
  ```json
  {
    "email": "customer@example.com",
    "password": "SecurePassword123"
  }
  ```
* **Logic xử lý:**
  1. Tìm kiếm User theo Email trong database. Nếu không thấy, trả về `401 Unauthorized` (Thông báo chung để bảo mật).
  2. So khớp hash mật khẩu bằng `BCrypt.Verify()`. Nếu không khớp, trả về `401`.
  3. Lấy thông tin vai trò (Role Name) của User.
  4. Sinh mã JWT Access Token chứa các Claims: `sub` (UserId), `email`, `role`, `exp` (Hạn 1 ngày).
  5. Trả về JWT Token và thông tin User cơ bản.

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `Login.vue`
* **Giao diện:** Form đăng nhập gồm Email và Mật khẩu. Thiết kế nút đăng nhập có trạng thái Loading khi gửi API.
* **Quản lý trạng thái (Pinia):** Tạo `useAuthStore` để lưu trữ JWT Token và thông tin user hiện tại vào `localStorage`.
* **Luồng đi:** Đăng nhập thành công $\rightarrow$ Lưu token $\rightarrow$ Giải mã token lấy role $\rightarrow$ Điều hướng về trang Dashboard tương ứng với quyền hạn.

---

### 📧 4. Hiện thực hóa chức năng Quên mật khẩu (T8, T9)
#### 🖥️ Backend API - *Hạnh thực hiện*
1. **API Endpoint 1:** `POST /api/v1/accounts/forgot-password` (Nhận email, sinh mã OTP gồm 6 số ngẫu nhiên lưu vào Cache/Database với thời hạn 5 phút, gửi mail SMTP đến người dùng).
2. **API Endpoint 2:** `POST /api/v1/accounts/verify-otp` (Xác thực OTP, trả về mã Token dùng một lần để đổi mật khẩu).
3. **API Endpoint 3:** `POST /api/v1/accounts/reset-password` (Nhận Token một lần và mật khẩu mới, băm mật khẩu và cập nhật vào CSDL).

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `ForgotPassword.vue`
* **Giao diện:** Luồng 3 bước: 
  * Bước 1: Nhập Email $\rightarrow$ Gửi mã OTP.
  * Bước 2: Nhập OTP nhận từ email.
  * Bước 3: Form thiết lập mật khẩu mới.

---

## 🔬 Kế Hoạch Kiểm Thử & Nghiệm Thu (DoD)
1. **Kiểm thử tự động:** Viết Unit Test cho `AuthService` kiểm tra logic đăng ký trùng email, mật khẩu không hợp lệ và logic sinh mã OTP hợp lệ.
2. **Kiểm thử thủ công:**
   * Sử dụng Swagger UI để test các endpoint API `/api/v1/accounts/*`.
   * Kiểm tra giao diện trên cả Desktop và Mobile để đảm bảo không bị tràn khung.
