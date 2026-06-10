# 📄 API Reference - Register & Activation (Phase 1)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng (API Contract) cho toàn bộ luồng đăng ký tài khoản, xác thực OTP kích hoạt và gửi lại OTP của hệ thống **MyPetClinic**.

---

## 1. API Đăng ký Tài khoản mới (Register)

Tạo một tài khoản khách hàng mới ở trạng thái chờ kích hoạt (`IsActive = false`) và kích hoạt gửi OTP.

*   **URL:** `/api/account/register`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 1.1. Request Body (JSON)
```json
{
  "fullName": "Nguyễn Văn A",
  "email": "nguyenvana@example.com",
  "phoneNumber": "0912345678",
  "password": "Password123!"
}
```
*   **fullName:** Chuỗi kí tự từ 3 đến 100 kí tự.
*   **email:** Chuỗi địa chỉ email hợp lệ, duy nhất chưa được đăng ký.
*   **phoneNumber:** Số điện thoại Việt Nam gồm 10 chữ số (đầu số 03, 05, 07, 08, 09).
*   **password:** Mật khẩu độ dài tối thiểu 8 ký tự, chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 chữ số và 1 ký tự đặc biệt.

### 1.2. Response Success (HTTP 200 OK)
```json
{
  "success": true,
  "message": "Đăng ký tài khoản thành công. Vui lòng kiểm tra hộp thư email nguyenvana@example.com để lấy mã kích hoạt OTP.",
  "data": {
    "email": "nguyenvana@example.com"
  }
}
```

### 1.3. Response Fail - Validation Error (HTTP 400 Bad Request)
```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "errors": {
    "Email": [
      "Địa chỉ email không đúng định dạng."
    ],
    "Password": [
      "Mật khẩu phải dài tối thiểu 8 ký tự và chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 chữ số."
    ]
  }
}
```

### 1.4. Response Fail - Business Rule Error (HTTP 422 Unprocessable Entity)
Trả về khi email đã tồn tại trên hệ thống.
```json
{
  "success": false,
  "errorType": "EMAIL_ALREADY_EXISTS",
  "message": "Địa chỉ email này đã được sử dụng để đăng ký tài khoản."
}
```

---

## 2. API Xác thực OTP kích hoạt tài khoản (Verify OTP)

Kích hoạt tài khoản người dùng sau khi nhập đúng mã OTP gửi qua email.

*   **URL:** `/api/account/verify-otp`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 2.1. Request Body (JSON)
```json
{
  "email": "nguyenvana@example.com",
  "otpCode": "123456"
}
```
*   **email:** Email cần xác thực kích hoạt.
*   **otpCode:** Chuỗi 6 chữ số OTP được gửi về hòm thư.

### 2.2. Response Success (HTTP 200 OK)
```json
{
  "success": true,
  "message": "Tài khoản của bạn đã được kích hoạt thành công. Bạn hiện có thể đăng nhập vào hệ thống."
}
```

### 2.3. Response Fail - Invalid OTP (HTTP 400 Bad Request)
```json
{
  "success": false,
  "errorType": "INVALID_OTP_CODE",
  "message": "Mã OTP bạn nhập không chính xác."
}
```

### 2.4. Response Fail - Expired OTP (HTTP 400 Bad Request)
```json
{
  "success": false,
  "errorType": "OTP_EXPIRED",
  "message": "Mã OTP đã hết hiệu lực (quá thời hạn 5 phút). Vui lòng yêu cầu gửi lại mã mới."
}
```

---

## 3. API Gửi lại Mã OTP mới (Resend OTP)

Yêu cầu sinh và gửi lại mã OTP mới trong trường hợp OTP cũ hết hạn hoặc khách hàng không nhận được thư.

*   **URL:** `/api/account/resend-otp`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 3.1. Request Body (JSON)
```json
{
  "email": "nguyenvana@example.com"
}
```

### 3.2. Response Success (HTTP 200 OK)
```json
{
  "success": true,
  "message": "Mã kích hoạt OTP mới đã được gửi thành công đến email của bạn."
}
```

### 3.3. Response Fail - Rate Limit Exceeded (HTTP 429 Too Many Requests)
```json
{
  "success": false,
  "errorType": "RATE_LIMIT_EXCEEDED",
  "message": "Bạn đã gửi yêu cầu quá nhanh. Vui lòng chờ 60 giây trước khi yêu cầu mã OTP tiếp theo."
}
```
