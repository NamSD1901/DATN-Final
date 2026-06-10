# 📄 API Reference - Login System (Phase 1)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng (API Contract) cho endpoint đăng nhập hệ thống phòng khám **MyPetClinic**.

---

## 1. API Đăng nhập Hệ thống (Login)

Xác thực thông tin đăng nhập của người dùng và trả về JWT Token cùng thông tin vai trò (Role).

*   **URL:** `/api/account/login`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 1.1. Request Body (JSON)
```json
{
  "email": "doctor.minh@mypetclinic.com",
  "password": "DoctorPassword123!"
}
```
*   **email:** Địa chỉ email hợp lệ đã được kích hoạt.
*   **password:** Mật khẩu của tài khoản (độ dài tối thiểu 8 ký tự).

### 1.2. Response Success (HTTP 200 OK)
Trả về khi xác thực thông tin đăng nhập thành công.
```json
{
  "success": true,
  "message": "Đăng nhập thành công.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJkM2IwNzM4NC1kMTEzLTRlYzYtYTVkNi1jOGMzZThhNmExMjMiLCJlbWFpbCI6ImRvY3Rvci5taW5oQG15cGV0Y2xpbmljLmNvbSIsInJvbGUiOiJCYWNTaSJ9...",
    "tokenType": "Bearer",
    "expiresInSeconds": 43200,
    "user": {
      "id": "d3b07384-d113-4ec6-a5d6-c8c3e8a6a123",
      "fullName": "Bác sĩ Minh",
      "email": "doctor.minh@mypetclinic.com",
      "role": "BacSi"
    }
  }
}
```

### 1.3. Response Fail - Validation Error (HTTP 400 Bad Request)
Trả về khi email sai định dạng hoặc mật khẩu trống.
```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "errors": {
    "Email": [
      "Địa chỉ email không đúng định dạng."
    ],
    "Password": [
      "Mật khẩu đăng nhập không được để trống."
    ]
  }
}
```

### 1.4. Response Fail - Authentication Error (HTTP 401 Unauthorized)
Trả về khi email hoặc mật khẩu bị sai.
```json
{
  "success": false,
  "errorType": "INVALID_CREDENTIALS",
  "message": "Tài khoản hoặc mật khẩu không chính xác."
}
```

### 1.5. Response Fail - Account Locked (HTTP 422 Unprocessable Entity)
Trả về khi tài khoản bị khóa tạm thời do nhập sai quá 5 lần.
```json
{
  "success": false,
  "errorType": "ACCOUNT_LOCKED",
  "message": "Tài khoản này đã bị tạm khóa 15 phút do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau.",
  "details": {
    "lockoutExpiry": "2026-06-10T08:05:00Z"
  }
}
```

### 1.6. Response Fail - Account Inactive (HTTP 422 Unprocessable Entity)
Trả về khi tài khoản chưa được kích hoạt qua OTP Email.
```json
{
  "success": false,
  "errorType": "ACCOUNT_INACTIVE",
  "message": "Tài khoản chưa được kích hoạt qua OTP Email. Vui lòng hoàn tất kích hoạt."
}
```

### 1.7. Response Fail - Rate Limit Exceeded (HTTP 429 Too Many Requests)
Trả về khi gửi quá nhiều request đăng nhập trong thời gian ngắn từ 1 IP.
```json
{
  "success": false,
  "errorType": "RATE_LIMIT_EXCEEDED",
  "message": "Bạn đã gửi yêu cầu đăng nhập quá nhanh. Vui lòng chờ 60 giây."
}
```
