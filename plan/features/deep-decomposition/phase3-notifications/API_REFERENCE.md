# 📖 API Reference Details - Automatic Notification Service

## 1. GET /api/notifications
Lấy danh sách tất cả các thông báo hệ thống của người dùng hiện tại, sắp xếp theo thời gian mới nhất.

*   **Auth:** `[Authorize(Roles = "customer")]`
*   **Response (200 OK):**
    ```json
    [
      {
        "id": "e2c38d4f-3721-4f18-a664-d3a373ff2010",
        "title": "Nhắc lịch tiêm chủng vắc-xin",
        "message": "Thú cưng Bông của bạn có lịch hẹn tiêm phòng vắc-xin tiếp theo sau 3 ngày nữa.",
        "isRead": false,
        "createdAt": "2026-06-10T08:00:00Z"
      }
    ]
    ```

---

## 2. PUT /api/notifications/{id}/read
Đánh dấu một thông báo cụ thể là đã đọc.

*   **Auth:** `[Authorize(Roles = "customer")]`
*   **Request URL Param:** `id` (Guid) - ID của thông báo cần sửa đổi.
*   **Response (200 OK):**
    ```json
    {
      "message": "Cập nhật thành công"
    }
    ```
*   **Response (403 Forbidden - IDOR Detection):**
    ```json
    {
      "message": "Bạn không có quyền chỉnh sửa thông báo này."
    }
    ```

---

## 3. PUT /api/notifications/read-all
Đánh dấu tất cả thông báo của người dùng hiện tại là đã đọc.

*   **Auth:** `[Authorize(Roles = "customer")]`
*   **Response (200 OK):**
    ```json
    {
      "message": "Đã đánh dấu đọc tất cả thông báo"
    }
    ```
