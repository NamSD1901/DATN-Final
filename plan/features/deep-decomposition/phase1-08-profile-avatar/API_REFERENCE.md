# 📖 API Reference Details - Profile Avatar Upload

Tài liệu đặc tả chi tiết giao thức kết nối (API Contracts) dành cho tính năng tải lên ảnh đại diện cá nhân thuộc `ProfileController`.

---

## 1. POST /api/profile/avatar

Thực hiện tải lên một tệp hình ảnh định dạng nhị phân, lưu trữ vật lý trên server và cập nhật thuộc tính Avatar của tài khoản đang đăng nhập.

*   **URL:** `/api/profile/avatar`
*   **Method:** `POST`
*   **Headers:**
    *   `Authorization: Bearer <JWT_ACCESS_TOKEN>` (Bắt buộc)
    *   `Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...` (Bắt buộc)
*   **Request Body Form-Data:**
    Dữ liệu gửi lên phải được đóng gói ở định dạng Form-Data. Phím (Key) của trường tệp tin bắt buộc phải khớp chính xác với định nghĩa tham số trên API Controller.

| Tham số (Key) | Kiểu dữ liệu | Định dạng (Type) | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- | :--- |
| `avatarFile` | `File` | `Binary` | Bắt buộc | Tệp tin hình ảnh cần đặt làm ảnh đại diện. |

---

## 2. Đặc tả các mã phản hồi (Response Payloads)

### A. Response (200 OK) - Tải lên thành công
Hệ thống lưu trữ ảnh thành công và trả về đường dẫn tương đối để Client hiển thị.
```json
{
  "success": true,
  "avatarUrl": "/uploads/avatars/b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8_my-pet-avatar.png",
  "message": "Cập nhật ảnh đại diện thành công!"
}
```

### B. Response (400 Bad Request) - Định dạng tệp tin bị cấm
Khi người dùng tải lên tệp tin không phải định dạng ảnh được hỗ trợ (Ví dụ: `.pdf`, `.zip`, `.html`).
```json
{
  "message": "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif"
}
```

### C. Response (400 Bad Request) - Kích thước vượt quá giới hạn
Khi người dùng tải lên file ảnh chất lượng quá cao vượt quá giới hạn 2MB của hệ thống.
```json
{
  "message": "Kích thước ảnh không được vượt quá 2MB."
}
```

### D. Response (400 Bad Request) - Tệp trống rỗng
Khi người dùng gửi request không đính kèm tệp tin nào hoặc tệp tin bị hỏng có dung lượng 0 bytes.
```json
{
  "message": "Vui lòng chọn một file ảnh hợp lệ."
}
```

### E. Response (401 Unauthorized) - Chưa xác thực danh tính
Khi request không gửi kèm mã JWT hoặc chữ ký token không khớp với khóa bảo mật của hệ thống Web API.
```json
{
  "message": "User ID not found in token/cookie."
}
```

### F. Response (429 Too Many Requests) - Vượt quá tần suất
Khi người dùng gọi API tải lên ảnh đại diện liên tiếp quá 5 lần trong vòng 1 phút.
```json
{
  "message": "Yêu cầu quá thường xuyên. Vui lòng thử lại sau."
}
```

### G. Response (500 Internal Server Error) - Lỗi đĩa ghi máy chủ
Lỗi xảy ra khi máy chủ hết dung lượng đĩa cứng hoặc tiến trình Web API không có quyền ghi thư mục `wwwroot/uploads`.
```json
{
  "success": false,
  "message": "Có lỗi xảy ra khi lưu ảnh đại diện."
}
```
