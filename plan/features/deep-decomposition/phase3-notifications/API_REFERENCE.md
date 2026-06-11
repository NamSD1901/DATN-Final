# 📄 API Reference - Automatic Notification Service

Tài liệu đặc tả chi tiết các cổng kết nối (RESTful API Contracts) dành cho phân hệ Nhắc lịch tiêm phòng & Thông báo tự động.

---

## 1. Danh sách các API Endpoints hỗ trợ

| Phương thức | Đường dẫn API | Phân quyền | Mô tả |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/customer/notifications` | Người dùng đăng nhập | Lấy danh sách thông báo in-app của người dùng hiện tại. |
| **PUT** | `/api/customer/notifications/{id}/read` | Người dùng đăng nhập | Đánh dấu đã đọc một thông báo cụ thể. |
| **PUT** | `/api/customer/notifications/read-all` | Người dùng đăng nhập | Đánh dấu đã đọc toàn bộ thông báo của người dùng. |
| **DELETE**| `/api/customer/notifications/{id}` | Người dùng đăng nhập | Xóa thông báo khỏi danh sách hiển thị. |

---

## 2. Đặc tả Chi tiết từng Endpoint

### 1. Lấy danh sách thông báo in-app của người dùng hiện tại
- **Endpoint:** `GET /api/customer/notifications`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Response (200 OK)
```json
[
  {
    "id": "c883e54b-d72b-42fa-97ab-713217b1897d",
    "title": "Lịch hẹn phê duyệt thành công",
    "content": "Lịch hẹn khám cho bé mèo LuLu của bạn vào 14:00 ngày 12/06/2026 đã được phê duyệt.",
    "isRead": false,
    "type": "AppointmentUpdate",
    "createdAt": "2026-06-11T10:50:00.000Z"
  },
  {
    "id": "d994f65c-e83c-53fb-08bc-824328c2908e",
    "title": "Nhắc lịch tiêm vắc-xin cho bé LuLu",
    "content": "Bé cưng LuLu có lịch tiêm nhắc vắc-xin Dại vào ngày 14/06/2026. Vui lòng đặt lịch hẹn.",
    "isRead": false,
    "type": "VaccineReminder",
    "createdAt": "2026-06-11T08:00:00.000Z"
  }
]
```

#### Response (401 Unauthorized) - Chưa đăng nhập
```json
{
  "status": 401,
  "title": "Unauthorized",
  "detail": "Vui lòng đăng nhập để truy cập hộp thư thông báo."
}
```

---

### 2. Đánh dấu đã đọc một thông báo cụ thể (Chống IDOR)
- **Endpoint:** `PUT /api/customer/notifications/{id}/read`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Response (200 OK)
```json
{
  "success": true,
  "message": "Đã đánh dấu đọc thông báo thành công."
}
```

#### Response (403 Forbidden) - Gọi API đổi trạng thái thông báo của tài khoản khác (Chặn IDOR)
```json
{
  "status": 403,
  "title": "Forbidden Action",
  "detail": "Tài khoản của bạn không được phân quyền thay đổi trạng thái của thông báo này."
}
```

#### Response (404 Not Found)
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Không tìm thấy thông báo được yêu cầu."
}
```

---

### 3. Đánh dấu đã đọc toàn bộ thông báo
- **Endpoint:** `PUT /api/customer/notifications/read-all`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Response (200 OK)
```json
{
  "success": true,
  "updatedCount": 2,
  "message": "Đã đánh dấu đọc thành công tất cả thông báo của bạn."
}
```
