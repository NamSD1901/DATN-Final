# 📖 API Reference Details - Pet Portfolio Management

Tài liệu này đặc tả chi tiết các cổng giao tiếp API (Endpoints Contracts) phục vụ cho các thao tác CRUD hồ sơ thú cưng cá nhân của khách hàng. Tất cả các endpoint đều yêu cầu xác thực JWT.

---

## 1. GET /api/mypets
Lấy toàn bộ danh sách thú cưng chưa bị xóa của khách hàng đang đăng nhập.

*   **HTTP Method:** `GET`
*   **Headers:**
    *   `Authorization: Bearer <JWT_TOKEN>` (Bắt buộc)
*   **Response (200 OK):**
    ```json
    [
      {
        "id": 12,
        "ownerId": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
        "name": "Bé Leo",
        "species": "dog",
        "breed": "Poodle",
        "gender": "Male",
        "birthDate": "2024-05-15T00:00:00Z",
        "weight": 4.2,
        "color": "Nâu đỏ",
        "bloodType": "DEA 1.1",
        "sterilized": true,
        "microchipCode": "MC-9876543210",
        "allergyNote": "Dị ứng với thuốc kháng sinh nhóm Penicillin",
        "createdAt": "2026-06-01T08:30:00Z"
      },
      {
        "id": 15,
        "ownerId": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
        "name": "Bé Mimi",
        "species": "cat",
        "breed": "Ba Tư",
        "gender": "Female",
        "birthDate": "2025-10-10T00:00:00Z",
        "weight": 3.1,
        "color": "Trắng",
        "bloodType": "A",
        "sterilized": false,
        "microchipCode": null,
        "allergyNote": null,
        "createdAt": "2026-06-05T14:20:00Z"
      }
    ]
    ```

---

## 2. POST /api/mypets
Đăng ký thêm một thú cưng mới cho tài khoản hiện tại.

*   **HTTP Method:** `POST`
*   **Headers:**
    *   `Authorization: Bearer <JWT_TOKEN>`
    *   `Content-Type: application/json`
*   **Request Body JSON:**
    ```json
    {
      "name": "Bé Lu",
      "species": "dog",
      "breed": "Corgi",
      "gender": "Male",
      "birthDate": "2025-01-20T00:00:00Z",
      "weight": 8.5,
      "color": "Vàng trắng",
      "bloodType": "DEA 1.2",
      "sterilized": false,
      "microchipCode": "MC-1234567890",
      "allergyNote": "Không có"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "success": true,
      "message": "Thêm thú cưng thành công!"
    }
    ```
*   **Response (400 Bad Request) - Validation Error:**
    ```json
    {
      "errors": {
        "name": [
          "Tên thú cưng không được để trống."
        ],
        "species": [
          "Loài thú cưng chỉ chấp nhận: dog (chó), cat (mèo) hoặc other (khác)."
        ]
      }
    }
    ```

---

## 3. GET /api/mypets/{id}
Lấy thông tin chi tiết của một thú cưng cụ thể.

*   **HTTP Method:** `GET`
*   **URL Parameter:** `id` (Kiểu số `long`, ví dụ: `/api/mypets/12`)
*   **Response (200 OK):**
    ```json
    {
      "id": 12,
      "ownerId": "a3b2c4d5-e6f7-8a9b-0c1d-2e3f4a5b6c7d",
      "name": "Bé Leo",
      "species": "dog",
      "breed": "Poodle",
      "gender": "Male",
      "birthDate": "2024-05-15T00:00:00Z",
      "weight": 4.2,
      "color": "Nâu đỏ",
      "bloodType": "DEA 1.1",
      "sterilized": true,
      "microchipCode": "MC-9876543210",
      "allergyNote": "Dị ứng với thuốc kháng sinh nhóm Penicillin"
    }
    ```
*   **Response (404 Not Found) - Không tồn tại hoặc IDOR Block:**
    Khi ID thú cưng không thuộc sở hữu của người dùng hiện tại, hệ thống trả về mã lỗi 404 để bảo mật thông tin.
    ```json
    {
      "message": "Không tìm thấy thú cưng."
    }
    ```

---

## 4. PUT /api/mypets/{id}
Cập nhật thông tin chi tiết hồ sơ thú cưng.

*   **HTTP Method:** `PUT`
*   **URL Parameter:** `id` (Ví dụ: `/api/mypets/12`)
*   **Request Body JSON:**
    ```json
    {
      "id": 12,
      "name": "Bé Leo Sửa Tên",
      "species": "dog",
      "breed": "Poodle Toy",
      "gender": "Male",
      "birthDate": "2024-05-15T00:00:00Z",
      "weight": 4.5,
      "color": "Nâu đỏ ánh cam",
      "bloodType": "DEA 1.1",
      "sterilized": true,
      "microchipCode": "MC-9876543210",
      "allergyNote": "Dị ứng nặng với Penicillin"
    }
    ```
*   **Response (200 OK):**
    ```json
    {
      "success": true,
      "message": "Cập nhật thông tin thú cưng thành công!"
    }
    ```

---

## 5. DELETE /api/mypets/{id}
Xóa mềm thú cưng khỏi danh sách hiển thị của khách hàng.

*   **HTTP Method:** `DELETE`
*   **URL Parameter:** `id` (Ví dụ: `/api/mypets/12`)
*   **Response (200 OK):**
    ```json
    {
      "success": true,
      "message": "Đã xóa thú cưng thành công."
    }
    ```
