# 📖 API Reference Details - Authentication & Authorization

Tài liệu đặc tả các giao thức API kết nối (API Contracts) phục vụ toàn bộ phân hệ quản lý tài khoản, xác thực và phân quyền của hệ thống **MyPetClinic**.

---

## 1. POST /api/account/register
Đăng ký tài khoản khách hàng mới.

*   **URL:** `/api/account/register`
*   **Method:** `POST`
*   **Content-Type:** `application/json`
*   **Request Body JSON:**
    ```json
    {
      "email": "customer@example.com",
      "password": "SecurePassword123!",
      "fullName": "Nguyễn Văn Khách",
      "phone": "0987654321"
    }
    ```
*   **Response (200 OK):**
    Tài khoản được tạo thành công ở trạng thái chờ kích hoạt OTP.
    ```json
    {
      "success": true,
      "email": "customer@example.com",
      "message": "Đăng ký thành công. Vui lòng kiểm tra email để nhận mã OTP kích hoạt tài khoản."
    }
    ```
*   **Response (400 Bad Request):**
    Email đã bị trùng đăng ký hoặc mật khẩu yếu.
    ```json
    {
      "success": false,
      "message": "Email đã được sử dụng bởi một tài khoản khác."
    }
    ```

---

## 2. POST /api/account/login
Đăng nhập tài khoản bằng Email và Mật khẩu.

*   **URL:** `/api/account/login`
*   **Method:** `POST`
*   **Request Body JSON:**
    ```json
    {
      "email": "customer@example.com",
      "password": "SecurePassword123!",
      "rememberMe": true
    }
    ```
*   **Response (200 OK) - Thành công:**
    Trả về token và vai trò người dùng để lưu trữ và điều hướng route.
    ```json
    {
      "success": true,
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "user": {
        "id": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
        "fullName": "Nguyễn Văn Khách",
        "email": "customer@example.com",
        "phone": "0987654321",
        "role": "customer",
        "avatar": "/uploads/avatars/default.webp"
      },
      "message": "Đăng nhập thành công."
    }
    ```
*   **Response (200 OK) - Cần xác thực OTP (Chưa kích hoạt):**
    Nếu tài khoản đăng ký trước đó nhưng chưa nhập OTP kích hoạt.
    ```json
    {
      "requiresOtp": true,
      "email": "customer@example.com",
      "message": "Tài khoản chưa kích hoạt. Vui lòng xác thực mã OTP vừa được gửi lại qua email."
    }
    ```
*   **Response (401 Unauthorized) - Sai mật khẩu:**
    ```json
    {
      "success": false,
      "message": "Tài khoản hoặc mật khẩu không chính xác."
    }
    ```

---

## 3. POST /api/account/google-login
Đăng nhập một chạm bằng Google Identity Services Token.

*   **URL:** `/api/account/google-login`
*   **Method:** `POST`
*   **Request Body JSON:**
    ```json
    {
      "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjFkNW..."
    }
    ```
*   **Response (200 OK):**
    Trả về token tương tự đăng nhập thông thường (nếu user chưa tồn tại trong hệ thống, tự động đăng ký với role mặc định là `customer`).
    ```json
    {
      "success": true,
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "user": {
        "id": "f8e7d6c5-b4a3-2a1b-0c9d-8e7f6a5b4c3d",
        "fullName": "Nguyễn Google User",
        "email": "googleuser@gmail.com",
        "phone": null,
        "role": "customer",
        "avatar": "https://lh3.googleusercontent.com/a/AGNmyx..."
      }
    }
    ```

---

## 4. POST /api/account/verify-otp
Xác thực OTP kích hoạt tài khoản hoặc đặt lại mật khẩu.

*   **URL:** `/api/account/verify-otp`
*   **Method:** `POST`
*   **Request Body JSON:**
    ```json
    {
      "email": "customer@example.com",
      "otpCode": "852019",
      "purpose": "RegisterVerification" // Hoặc: "PasswordRecovery"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "success": true,
      "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "user": {
        "id": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
        "fullName": "Nguyễn Văn Khách",
        "email": "customer@example.com",
        "phone": "0987654321",
        "role": "customer",
        "avatar": "/uploads/avatars/default.webp"
      },
      "message": "Xác thực OTP thành công. Tài khoản đã được kích hoạt."
    }
    ```
*   **Response (400 Bad Request):**
    OTP sai hoặc hết hiệu lực quá 5 phút.
    ```json
    {
      "success": false,
      "message": "Mã OTP không chính xác hoặc đã hết hạn sử dụng."
    }
    ```
