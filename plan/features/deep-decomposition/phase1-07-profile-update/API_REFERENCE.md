# 📖 API Reference Details - Profile Details Update

Tài liệu đặc tả chi tiết toàn bộ các cổng kết nối (Endpoints) liên quan đến việc xem, cập nhật thông tin cá nhân và đổi mật khẩu bảo mật thuộc `ProfileController`.

---

## 1. GET /api/profile
Lấy thông tin hồ sơ của người dùng hiện tại đang đăng nhập từ JWT Token.

*   **URL:** `/api/profile`
*   **Method:** `GET`
*   **Headers:**
    *   `Authorization: Bearer <JWT_ACCESS_TOKEN>` (Bắt buộc)
    *   `Accept: application/json`
*   **Response (200 OK):**
    Trả về dữ liệu chi tiết của người dùng đang đăng nhập.
    ```json
    {
      "id": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
      "roleId": 3,
      "roleName": "Customer",
      "fullName": "Nguyễn Khách Hàng",
      "email": "khachhang@gmail.com",
      "phone": "0987654321",
      "address": "123 Đường ABC, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
      "gender": 1,
      "dateOfBirth": "1995-05-15T00:00:00Z",
      "avatar": "/uploads/avatars/default.webp"
    }
    ```
*   **Response (401 Unauthorized):**
    Khi không gửi kèm Token hoặc Token không hợp lệ/hết hạn.
    ```json
    {
      "message": "User ID not found in token/cookie."
    }
    ```
*   **Response (404 Not Found):**
    Khi giải mã được ID nhưng tài khoản đó không còn tồn tại trong DB (đã bị xóa cứng).
    ```json
    {
      "message": "Không tìm thấy hồ sơ."
    }
    ```

---

## 2. PUT /api/profile
Cập nhật thông tin chi tiết hồ sơ cá nhân (Họ tên, SĐT, Địa chỉ, Giới tính, Ngày sinh).

*   **URL:** `/api/profile`
*   **Method:** `PUT`
*   **Headers:**
    *   `Authorization: Bearer <JWT_ACCESS_TOKEN>` (Bắt buộc)
    *   `Content-Type: application/json`
*   **Request Body JSON:**
    ```json
    {
      "fullName": "Nguyễn Khách Hàng Mới",
      "phone": "0912345678",
      "address": "456 Đường XYZ, Phường Võ Thị Sáu, Quận 3, TP. Hồ Chí Minh",
      "gender": 0,
      "dateOfBirth": "1995-10-20"
    }
    ```
*   **Response (200 OK):**
    Cập nhật thành công.
    ```json
    {
      "success": true,
      "message": "Cập nhật thông tin cá nhân thành công!"
    }
    ```
*   **Response (400 Bad Request):**
    Khi dữ liệu gửi lên vi phạm quy tắc validation (ví dụ: số điện thoại sai định dạng, họ tên chứa ký tự đặc biệt).
    ```json
    {
      "errors": {
        "fullName": [
          "Họ tên không được chứa chữ số hoặc ký tự đặc biệt."
        ],
        "phone": [
          "Số điện thoại không đúng định dạng Việt Nam (10 chữ số, bắt đầu bằng 03, 05, 07, 08 hoặc 09)."
        ],
        "dateOfBirth": [
          "Ngày sinh phải là một ngày trong quá khứ."
        ]
      }
    }
    ```
*   **Response (401 Unauthorized):**
    Không có quyền thực hiện hành động.
    ```json
    {
      "message": "User ID not found in token/cookie."
    }
    ```
*   **Response (429 Too Many Requests):**
    Khi người dùng gửi cập nhật vượt quá 10 lần trong 1 phút.
    ```json
    {
      "message": "Yêu cầu quá thường xuyên. Vui lòng thử lại sau."
    }
    ```

---

## 3. PUT /api/profile/password
Đổi mật khẩu bảo mật của người dùng hiện tại (yêu cầu mật khẩu cũ để xác thực).

*   **URL:** `/api/profile/password`
*   **Method:** `PUT`
*   **Headers:**
    *   `Authorization: Bearer <JWT_ACCESS_TOKEN>` (Bắt buộc)
    *   `Content-Type: application/json`
*   **Request Body JSON:**
    ```json
    {
      "currentPassword": "OldPassword123@",
      "newPassword": "NewSecurePassword456!",
      "confirmPassword": "NewSecurePassword456!"
    }
    ```
*   **Response (200 OK):**
    Đổi mật khẩu thành công.
    ```json
    {
      "success": true,
      "message": "Đổi mật khẩu thành công!"
    }
    ```
*   **Response (400 Bad Request):**
    Mật khẩu hiện tại không chính xác hoặc mật khẩu mới vi phạm quy tắc độ phức tạp (ví dụ: thiếu chữ viết hoa, ký tự đặc biệt) hoặc mật khẩu xác nhận không khớp.
    ```json
    {
      "success": false,
      "message": "Mật khẩu hiện tại không chính xác hoặc có lỗi xảy ra."
    }
    ```
    Hoặc validation lỗi ModelState:
    ```json
    {
      "errors": {
        "newPassword": [
          "Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt."
        ],
        "confirmPassword": [
          "Mật khẩu xác nhận không khớp với mật khẩu mới."
        ]
      }
    }
    ```
