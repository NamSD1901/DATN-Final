# 🔌 Đặc Tả Giao Diện Lập Trình Ứng Dụng - Web API Specifications (MyPetClinic)

Tài liệu này đặc tả chi tiết giao diện lập trình ứng dụng RESTful API kết nối giữa Frontend Vue 3 máy khách và Backend C# ASP.NET Core máy chủ trong dự án **MyPetClinic** (Hệ thống phòng khám thú y).

---

## 1. Kiến Trúc Giao Tiếp (Communication Architecture)
*   **Giao thức:** HTTPS RESTful API.
*   **Định dạng dữ liệu:** JSON (UTF-8).
*   **Ngưỡng phản hồi cam kết (SLA):** < 50ms cho các tác vụ lấy dữ liệu tĩnh, < 300ms cho các tác vụ ghi/cập nhật dữ liệu.
*   **Bảo mật:** Tiêu chuẩn **JWT Bearer Token** truyền qua HTTP Header `Authorization: Bearer <Token>` hoặc Session Cookie tùy thuộc vào cấu hình môi trường.
*   **Soft Delete:** Áp dụng xóa mềm cho các thực thể quan trọng (Pet, User, Appointment, v.v.) để bảo đảm tính toàn vẹn của lịch sử điều trị và tài chính phòng khám.

---

## 2. Danh Sách Các Cổng API (API Endpoints Directory)

### 2.1. Quản Lý Hồ Sơ Cá Nhân (User Profile Management)

#### 📥 API 1: Lấy hồ sơ người dùng hiện tại
*   **Đường dẫn:** `GET /api/profile`
*   **Yêu cầu:** Đã đăng nhập (Authorize).
*   **Mô tả:** Trả về thông tin chi tiết của người dùng đang đăng nhập dựa trên định danh Token/Cookie.
*   **Đầu ra (Response - 200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fullName": "Nguyễn Văn A",
  "email": "customer@mypetclinic.com",
  "phone": "0987654321",
  "address": "123 Đường ABC, Quận 1, TP. Hồ Chí Minh",
  "gender": 1,
  "dateOfBirth": "1995-10-15T00:00:00Z",
  "avatar": "/uploads/avatars/default.png",
  "roleName": "Customer"
}
```

#### 📥 API 2: Cập nhật hồ sơ cá nhân
*   **Đường dẫn:** `PUT /api/profile`
*   **Yêu cầu:** Đã đăng nhập (Authorize).
*   **Mô tả:** Cập nhật thông tin cá nhân cơ bản của tài khoản hiện tại.
*   **Đầu vào (Request Body):**
```json
{
  "fullName": "Nguyễn Văn A",
  "phone": "0987654321",
  "address": "123 Đường ABC, Quận 1, TP. Hồ Chí Minh",
  "gender": 1,
  "dateOfBirth": "1995-10-15"
}
```
*   **Đầu ra (Response - 200 OK):**
```json
{
  "success": true,
  "message": "Cập nhật thông tin cá nhân thành công!"
}
```

#### 📥 API 3: Thay đổi mật khẩu tài khoản
*   **Đường dẫn:** `PUT /api/profile/password`
*   **Yêu cầu:** Đã đăng nhập (Authorize).
*   **Mô tả:** Kiểm duyệt mật khẩu cũ và đổi mật khẩu mới cho tài khoản.
*   **Đầu vào (Request Body):**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword123!",
  "confirmNewPassword": "NewSecurePassword123!"
}
```
*   **Đầu ra (Response - 200 OK):**
```json
{
  "success": true,
  "message": "Đổi mật khẩu thành công!"
}
```

#### 📥 API 4: Cập nhật ảnh đại diện
*   **Đường dẫn:** `POST /api/profile/avatar`
*   **Yêu cầu:** Đã đăng nhập (Authorize).
*   **Mô tả:** Upload file ảnh đại diện mới. Giới hạn dung lượng < 2MB và các định dạng: `.jpg, .jpeg, .png, .gif`.
*   **Đầu vào (Multipart Form Data):**
    *   `avatarFile`: File ảnh upload.
*   **Đầu ra (Response - 200 OK):**
```json
{
  "success": true,
  "avatarUrl": "/uploads/avatars/d83f15-unique-filename.png",
  "message": "Cập nhật ảnh đại diện thành công!"
}
```

---

### 2.2. Số hóa Hồ sơ Thú cưng (My Pets Management)

#### 📥 API 5: Lấy danh sách thú cưng của tôi
*   **Đường dẫn:** `GET /api/mypets`
*   **Yêu cầu:** Đăng nhập dưới quyền `Customer` (Authorize).
*   **Mô tả:** Lấy danh sách toàn bộ thú cưng thuộc sở hữu của chủ tài khoản đang đăng nhập.
*   **Đầu ra (Response - 200 OK):**
```json
[
  {
    "id": 1,
    "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Mochi",
    "species": "Chó",
    "breed": "Corgi",
    "gender": 1,
    "birthDate": "2023-05-10T00:00:00Z",
    "weight": 8.5,
    "color": "Vàng trắng",
    "bloodType": "DEA 1.1",
    "sterilized": true,
    "microchipCode": "900006000123456",
    "allergyNote": "Dị ứng thuốc kháng sinh Penicillin",
    "medicalProfile": {
      "allergies": ["Penicillin", "Thịt gà"],
      "chronicDiseases": ["Viêm da cơ địa"],
      "vaccinationStatus": "Đầy đủ"
    },
    "createdAt": "2026-06-01T10:00:00Z"
  }
]
```

#### 📥 API 6: Thêm mới hồ sơ thú cưng
*   **Đường dẫn:** `POST /api/mypets`
*   **Yêu cầu:** Đăng nhập dưới quyền `Customer` (Authorize).
*   **Mô tả:** Khai báo thông tin hành chính và tiền sử y tế ban đầu của thú cưng mới.
*   **Đầu vào (Request Body):**
```json
{
  "name": "Mochi",
  "species": "Chó",
  "breed": "Corgi",
  "gender": 1,
  "birthDate": "2023-05-10",
  "weight": 8.5,
  "color": "Vàng trắng",
  "bloodType": "DEA 1.1",
  "sterilized": true,
  "microchipCode": "900006000123456",
  "allergyNote": "Dị ứng thuốc kháng sinh Penicillin",
  "medicalProfile": {
    "allergies": ["Penicillin", "Thịt gà"],
    "chronicDiseases": ["Viêm da cơ địa"],
    "vaccinationStatus": "Đầy đủ"
  }
}
```
*   **Đầu ra (Response - 200 OK):**
```json
{
  "success": true,
  "message": "Thêm thú cưng thành công!"
}
```

#### 📥 API 7: Cập nhật thông tin thú cưng
*   **Đường dẫn:** `PUT /api/mypets/{id}`
*   **Yêu cầu:** Đăng nhập dưới quyền `Customer` (Authorize).
*   **Mô tả:** Chỉnh sửa thông tin hành chính hoặc thay đổi tiền sử bệnh án của thú cưng.
*   **Đầu vào (Request Body):**
```json
{
  "id": 1,
  "name": "Mochi",
  "species": "Chó",
  "breed": "Corgi",
  "gender": 1,
  "birthDate": "2023-05-10",
  "weight": 9.2,
  "color": "Vàng trắng",
  "bloodType": "DEA 1.1",
  "sterilized": true,
  "microchipCode": "900006000123456",
  "allergyNote": "Dị ứng thuốc kháng sinh Penicillin và Thức ăn hạt hạt ngũ cốc",
  "medicalProfile": {
    "allergies": ["Penicillin", "Thịt gà", "Ngũ cốc"],
    "chronicDiseases": ["Viêm da cơ địa"],
    "vaccinationStatus": "Đầy đủ"
  }
}
```
*   **Đầu ra (Response - 200 OK):**
```json
{
  "success": true,
  "message": "Cập nhật thông tin thú cưng thành công!"
}
```

#### 📥 API 8: Xem chi tiết hồ sơ thú cưng
*   **Đường dẫn:** `GET /api/mypets/{id}`
*   **Yêu cầu:** Đăng nhập dưới quyền `Customer` hoặc nhân viên phòng khám.
*   **Mô tả:** Trả về thông tin đầy đủ, chi tiết của một thú cưng cụ thể.
*   **Đầu ra (Response - 200 OK):**
```json
{
  "id": 1,
  "ownerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Mochi",
  "species": "Chó",
  "breed": "Corgi",
  "gender": 1,
  "birthDate": "2023-05-10T00:00:00Z",
  "weight": 9.2,
  "color": "Vàng trắng",
  "bloodType": "DEA 1.1",
  "sterilized": true,
  "microchipCode": "900006000123456",
  "allergyNote": "Dị ứng thuốc kháng sinh Penicillin và Thức ăn hạt hạt ngũ cốc",
  "medicalProfile": {
    "allergies": ["Penicillin", "Thịt gà", "Ngũ cốc"],
    "chronicDiseases": ["Viêm da cơ địa"],
    "vaccinationStatus": "Đầy đủ"
  },
  "createdAt": "2026-06-01T10:00:00Z"
}
```

#### 📥 API 9: Xóa mềm thú cưng
*   **Đường dẫn:** `DELETE /api/mypets/{id}`
*   **Yêu cầu:** Đăng nhập dưới quyền `Customer` (Authorize).
*   **Mô tả:** Đánh dấu xóa mềm thú cưng (Set giá trị `DeletedAt` thay vì xóa khỏi CSDL) để bảo lưu dữ liệu.
*   **Đầu ra (Response - 200 OK):**
```json
{
  "success": true,
  "message": "Đã xóa thú cưng thành công."
}
```

---

## 3. Quản Lý Lỗi Thống Nhất (Unified Error Handling)

Hệ thống trả về các phản hồi lỗi tiêu chuẩn tương tự chuẩn RFC 7807 giúp Client Vue xử lý giao diện linh hoạt:

### 3.1. Phản hồi Lỗi xác thực hoặc Đầu vào không hợp lệ (400 Bad Request)
```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Vui lòng nhập tên thú cưng"
    ],
    "Species": [
      "Vui lòng chọn giống loài"
    ]
  }
}
```

### 3.2. Lỗi token hết hạn hoặc chưa đăng nhập (401 Unauthorized)
```json
{
  "status": 401,
  "message": "Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại."
}
```
