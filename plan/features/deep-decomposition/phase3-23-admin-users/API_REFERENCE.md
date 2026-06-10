# 📖 API Reference Details - Admin Staff Management

## 1. GET /api/admin/users
Lấy danh sách tất cả người dùng hệ thống thuộc nhóm nhân viên.

*   **Auth:** `[Authorize(Roles = "admin")]`
*   **Response (200 OK):**
    ```json
    [
      {
        "id": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
        "email": "doctorA@mypetclinic.vn",
        "fullName": "Nguyễn Văn A",
        "role": "doctor",
        "isLockedOut": false
      }
    ]
    ```

---

## 2. POST /api/admin/users/{id}/lock
Thực hiện khóa tài khoản nhân viên.

*   **Auth:** `[Authorize(Roles = "admin")]`
*   **Request URL Param:** `id` (Guid) - ID người dùng cần khóa.
*   **Response (200 OK):**
    ```json
    {
      "message": "Đã khóa tài khoản thành công."
    }
    ```
*   **Response (400 Bad Request):**
    ```json
    {
      "message": "Bạn không thể tự khóa tài khoản của chính mình."
    }
    ```
