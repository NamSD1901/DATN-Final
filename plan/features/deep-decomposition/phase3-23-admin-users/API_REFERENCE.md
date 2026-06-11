# 📄 API Reference - Admin Staff Management

Tài liệu đặc tả chi tiết các cổng kết nối (RESTful API Contracts) dành cho phân hệ Quản trị Nhân sự & Phân quyền Admin.

---

## 1. Danh sách các API Endpoints hỗ trợ

| Phương thức | Đường dẫn API | Phân quyền | Mô tả |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/admin/staff` | `admin` | Lấy danh sách toàn bộ tài khoản nhân sự phòng khám. |
| **POST** | `/api/admin/staff` | `admin` | Thêm mới một tài khoản nhân viên (tự sinh pass mặc định & gửi mail). |
| **PUT** | `/api/admin/staff/{id}/role` | `admin` | Thay đổi vai trò chuyên môn của nhân viên. |
| **PUT** | `/api/admin/staff/{id}/toggle-status` | `admin` | Khóa hoặc mở khóa hoạt động tài khoản nhân viên. |
| **GET** | `/api/admin/audit-logs` | `admin` | Xem nhật ký lịch sử thay đổi quyền của hệ thống. |

---

## 2. Đặc tả Chi tiết từng Endpoint

### 1. Thêm mới tài khoản nhân viên
- **Endpoint:** `POST /api/admin/staff`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Request Body Schema (Application/JSON)
```json
{
  "fullName": "Nguyễn Văn Đức",
  "email": "duc.nv@mypet.vn",
  "phoneNumber": "0912345678",
  "role": "doctor"
}
```

#### Response (201 Created)
```json
{
  "id": "a993e54b-d72b-42fa-97ab-713217b1897d",
  "fullName": "Nguyễn Văn Đức",
  "email": "duc.nv@mypet.vn",
  "phoneNumber": "0912345678",
  "role": "doctor",
  "status": "PendingActivation",
  "requirePasswordChange": true,
  "createdAt": "2026-06-11T09:30:00.000Z"
}
```

#### Response (400 Bad Request) - Sai định dạng Email / Số điện thoại
```json
{
  "status": 400,
  "title": "Bad Request",
  "errors": {
    "Email": ["Địa chỉ Email không đúng định dạng."],
    "PhoneNumber": ["Số điện thoại phải gồm 10 chữ số."]
  }
}
```

#### Response (422 Unprocessable Entity) - Email đã tồn tại
```json
{
  "status": 422,
  "title": "Conflict Business Rule",
  "detail": "Địa chỉ Email duc.nv@mypet.vn đã được đăng ký trong hệ thống."
}
```

---

### 2. Thay đổi vai trò chuyên môn của nhân viên
- **Endpoint:** `PUT /api/admin/staff/{id}/role`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Request Body Schema (Application/JSON)
```json
{
  "newRole": "admin"
}
```

#### Response (200 OK)
```json
{
  "id": "a993e54b-d72b-42fa-97ab-713217b1897d",
  "fullName": "Nguyễn Văn Đức",
  "email": "duc.nv@mypet.vn",
  "phoneNumber": "0912345678",
  "role": "admin",
  "status": "Active",
  "requirePasswordChange": false,
  "createdAt": "2026-06-11T09:30:00.000Z"
}
```

#### Response (403 Forbidden) - Tự hạ quyền chính mình hoặc tài khoản đăng nhập không phải Admin
```json
{
  "status": 403,
  "title": "Forbidden Action",
  "detail": "Admin không được phép tự thay đổi vai trò của chính mình."
}
```

---

### 3. Khóa hoặc mở khóa hoạt động tài khoản nhân viên
- **Endpoint:** `PUT /api/admin/staff/{id}/toggle-status`
- **Headers:** `Authorization: Bearer <JWT_TOKEN>`

#### Response (200 OK) - Trạng thái mới: Khóa
```json
{
  "id": "a993e54b-d72b-42fa-97ab-713217b1897d",
  "fullName": "Nguyễn Văn Đức",
  "email": "duc.nv@mypet.vn",
  "phoneNumber": "0912345678",
  "role": "doctor",
  "status": "Suspended",
  "requirePasswordChange": false,
  "createdAt": "2026-06-11T09:30:00.000Z"
}
```

#### Response (404 Not Found)
```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Không tìm thấy tài khoản nhân viên với ID được yêu cầu."
}
```
