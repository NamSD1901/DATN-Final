# 📄 API Reference - Forgot Password Recovery (Phase 1)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng (API Contract) cho toàn bộ luồng yêu cầu OTP khôi phục mật khẩu và đặt lại mật khẩu của hệ thống **MyPetClinic**.

---

## 1. API Yêu cầu Gửi mã OTP Khôi phục (Forgot Password)

Sinh mã OTP khôi phục mật khẩu và gửi về Email (nếu email tồn tại trên hệ thống).

*   **URL:** `/api/account/forgot-password`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 1.1. Request Body (JSON)
```json
{
  "email": "customer.test@gmail.com"
}
```

### 1.2. Response Success (HTTP 200 OK)
*Lưu ý: Luôn trả về 200 OK cho cả trường hợp Email không tồn tại để ngăn chặn dò tìm email.*
```json
{
  "success": true,
  "message": "Nếu địa chỉ email này tồn tại trong hệ thống, một mã OTP xác thực khôi phục mật khẩu đã được gửi đi."
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
    ]
  }
}
```

### 1.4. Response Fail - Rate Limit Exceeded (HTTP 429 Too Many Requests)
```json
{
  "success": false,
  "errorType": "RATE_LIMIT_EXCEEDED",
  "message": "Bạn đã gửi yêu cầu khôi phục quá nhanh. Vui lòng thử lại sau."
}
```

---

## 2. API Đặt lại Mật khẩu mới (Reset Password)

Xác thực mã OTP và tiến hành cập nhật mật khẩu mới băm bằng BCrypt, mở khóa tài khoản.

*   **URL:** `/api/account/reset-password`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 2.1. Request Body (JSON)
```json
{
  "email": "customer.test@gmail.com",
  "otpCode": "921504",
  "newPassword": "NewStrongPassword123!"
}
```
*   **email:** Email cần khôi phục mật khẩu.
*   **otpCode:** Chuỗi 6 chữ số OTP được gửi về hòm thư.
*   **newPassword:** Mật khẩu mới cần cài đặt (độ dài tối thiểu 8 ký tự, chữ hoa, thường, số, ký tự đặc biệt).

### 2.2. Response Success (HTTP 200 OK)
```json
{
  "success": true,
  "message": "Mật khẩu của bạn đã được đặt lại thành công. Bạn hiện có thể đăng nhập vào hệ thống bằng mật khẩu mới."
}
```

### 2.3. Response Fail - Validation Error (HTTP 400 Bad Request)
```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "errors": {
    "NewPassword": [
      "Mật khẩu mới phải dài tối thiểu 8 ký tự."
    ]
  }
}
```

### 2.4. Response Fail - Invalid OTP or Attempts Exceeded (HTTP 400 Bad Request)
```json
{
  "success": false,
  "errorType": "INVALID_OTP",
  "message": "Mã OTP khôi phục không chính xác. Bạn còn 2 lần nhập lại."
}
```
Hoặc khi đã nhập sai quá 3 lần:
```json
{
  "success": false,
  "errorType": "OTP_ATTEMPTS_EXCEEDED",
  "message": "Mã OTP này đã bị hủy bỏ do nhập sai quá 3 lần. Vui lòng yêu cầu gửi lại OTP mới."
}
```
### 2.5. Response Fail - Expired OTP (HTTP 400 Bad Request)
```json
{
  "success": false,
  "errorType": "OTP_EXPIRED",
  "message": "Mã OTP khôi phục đã hết hiệu lực."
}
```
