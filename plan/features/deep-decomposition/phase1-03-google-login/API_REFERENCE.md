# 📄 API Reference - Google Login (Phase 1)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng (API Contract) cho endpoint đăng nhập bằng Google của hệ thống **MyPetClinic**.

---

## 1. API Đăng nhập / Tự động đăng ký qua Google

Tiếp nhận Google ID Token, tiến hành xác thực chữ ký số, tự động tạo tài khoản nếu là email mới, và trả về JWT Token của MyPetClinic.

*   **URL:** `/api/account/google-login`
*   **Method:** `POST`
*   **Headers:**
    *   `Content-Type: application/json`

### 1.1. Request Body (JSON)
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjFhMmIzYzRkNWU2Zi..."
}
```
*   **idToken:** Chuỗi JSON Web Token (JWT) được cấp từ Google Sign-In SDK ở client sau khi người dùng xác thực thành công.

### 1.2. Response Success (HTTP 200 OK)
Trả về khi xác thực Google Token thành công, tài khoản được tạo/liên kết và cấp token MyPetClinic.
```json
{
  "success": true,
  "message": "Đăng nhập Google thành công.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJhMTJiM2M0ZC01ZTZmLTdhOGItOWMxZC01ZTZmN2E4YjkxMmMiLCJlbWFpbCI6ImN1c3RvbWVyLnRlc3RAZ21haWwuY29tIiwicm9sZSI6IktoYWNoSGFuZyJ9...",
    "tokenType": "Bearer",
    "expiresInSeconds": 43200,
    "user": {
      "id": "a12b3c4d-5e6f-7a8b-9c1d-5e6f7a8b912c",
      "fullName": "Nguyen Van Customer",
      "email": "customer.test@gmail.com",
      "role": "KhachHang"
    }
  }
}
```

### 1.3. Response Fail - Missing Token (HTTP 400 Bad Request)
Trả về khi không gửi token lên.
```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "errors": {
    "IdToken": [
      "Google ID Token không được để trống."
    ]
  }
}
```

### 1.4. Response Fail - Verification Failed (HTTP 401 Unauthorized)
Trả về khi token bị sửa đổi, hết hạn hoặc sai chữ ký Google.
```json
{
  "success": false,
  "errorType": "GOOGLE_AUTH_FAILED",
  "message": "Xác thực Token Google không thành công hoặc mã xác thực đã hết hạn."
}
```

### 1.5. Response Fail - Locked Account (HTTP 422 Unprocessable Entity)
Trả về khi tài khoản liên kết Google đang bị khóa 15 phút.
```json
{
  "success": false,
  "errorType": "ACCOUNT_LOCKED",
  "message": "Tài khoản liên kết của bạn hiện đang bị khóa tạm thời. Vui lòng quay lại sau.",
  "details": {
    "lockoutExpiry": "2026-06-10T08:12:00Z"
  }
}
```

### 1.6. Response Fail - Rate Limit Exceeded (HTTP 429 Too Many Requests)
```json
{
  "success": false,
  "errorType": "RATE_LIMIT_EXCEEDED",
  "message": "Bạn đã gửi yêu cầu xác thực quá nhanh. Vui lòng thử lại sau."
}
```
