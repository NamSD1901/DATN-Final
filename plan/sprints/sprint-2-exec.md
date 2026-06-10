# 🚀 Kế Hoạch Thực Thi Chi Tiết - Sprint 2

## 🎯 Mục Tiêu Sprint 2
Khách vãng lai và chủ nuôi có thể xem thông tin phòng khám (danh sách dịch vụ, bảng giá, đội ngũ bác sĩ), đồng thời hoàn thành chức năng quản lý thông tin cá nhân và hồ sơ chi tiết thú cưng.

---

## 🛠️ Chi Tiết Các Bước Thực Thi (Step-by-Step)

### 📋 1. Xem Dịch Vụ & Bảng Giá (T10, T11)
#### 🖥️ Backend API - *Hạnh thực hiện*
* **Bảng dữ liệu liên quan:** `Service_categories`, `Services`
* **API Endpoint:** `GET /api/v1/services`
* **Logic xử lý:** 
  1. Truy vấn toàn bộ dịch vụ đang hoạt động (`Is_active = true` và `Deleted_at IS NULL`) kèm thông tin danh mục tương ứng.
  2. Hỗ trợ tìm kiếm theo tên và lọc theo `Category_id`.
  3. Trả về cấu trúc JSON phân tầng danh mục hoặc danh sách phẳng.

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `views/services/` (Gồm `KhamDieuTri.vue`, `SpaGrooming.vue`, `SucKhoe.vue`, `TiemPhong.vue`).
* **Giao diện:** Hiển thị thông tin dịch vụ dưới dạng thẻ (Cards) hoặc bảng biểu (Tables) sinh động, có phân loại danh mục, hỗ trợ tìm kiếm và sắp xếp theo giá tiền.

---

### 👨‍⚕️ 2. Xem Đội Ngũ Bác Sĩ (T12, T13)
#### 🖥️ Backend API - *Hạnh thực hiện*
* **Bảng dữ liệu liên quan:** `Users`, `Roles`
* **API Endpoint:** `GET /api/v1/doctors`
* **Logic xử lý:**
  1. Truy vấn các tài khoản `Users` có `Role_id` khớp với vai trò `'Doctor'`.
  2. Chỉ lấy các trường công khai: Họ tên, Email, Ảnh đại diện (Avatar), Giới tính, cùng thông tin chuyên môn.

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `views/Team.vue`
* **Giao diện:** Hiển thị danh sách bác sĩ thú y của phòng khám với ảnh đại diện trực quan, phần giới thiệu chuyên môn và kinh nghiệm làm việc để khách hàng dễ chọn khi đặt lịch.

---

### 👤 3. Quản Lý Thông Tin Cá Nhân (T14, T15)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoints:**
  * `GET /api/v1/profiles/me` (Lấy thông tin tài khoản hiện tại thông qua Claims trong JWT Token).
  * `PUT /api/v1/profiles/me` (Cập nhật thông tin: Họ tên, Số điện thoại, Địa chỉ, Ngày sinh, Giới tính).
  * `POST /api/v1/profiles/me/avatar` (Nhận file ảnh, upload lên Cloudinary hoặc lưu trữ cục bộ, cập nhật đường dẫn avatar vào CSDL).
* **Logic xử lý:** Đảm bảo chỉ người dùng sở hữu tài khoản mới có quyền xem/sửa thông tin chính mình.

#### 🌐 Frontend UI - *Lâm thực hiện*
* **Trang:** `views/Profile.vue`
* **Giao diện:** Form thông tin cá nhân hiển thị dữ liệu hiện tại, hỗ trợ upload/crop ảnh đại diện, kiểm tra tính hợp lệ số điện thoại trước khi lưu.

---

### 🐶 4. Quản Lý Hồ Sơ Thú Cưng (T16, T17)
#### 🖥️ Backend API - *Nam thực hiện*
* **Bảng dữ liệu liên quan:** `Pets`
* **API Endpoints (CRUD):**
  * `GET /api/v1/my-pets` (Lấy danh sách thú cưng của khách hàng đang đăng nhập).
  * `POST /api/v1/my-pets` (Thêm thú cưng mới).
  * `PUT /api/v1/my-pets/{id}` (Cập nhật thông tin thú cưng).
  * `DELETE /api/v1/my-pets/{id}` (Xóa mềm - đặt trường `Deleted_at`).
* **Logic xử lý:** Validate các trường bắt buộc (Tên, Loài). Đảm bảo kiểm tra `Owner_id == CurrentUserId` ở tất cả các request để tránh tấn công chiếm quyền (IDOR).

#### 🌐 Frontend UI - *Lâm thực hiện*
* **Trang:** Lồng trong mục quản lý cá nhân hoặc một view riêng biệt.
* **Giao diện:** 
  * Trang danh sách thú cưng (dạng Grid) kèm theo hình ảnh, loài, giống và tuổi.
  * Form modal thêm/sửa thú cưng có ảnh tải lên, chọn giống, ngày sinh và nhập ghi chú dị ứng/tiền sử bệnh lý.

---

## 🔬 Kế Hoạch Kiểm Thử & Nghiệm Thu (DoD)
1. **Kiểm thử tự động:** Viết Unit Test kiểm thử logic phân quyền ở API cập nhật thông tin cá nhân và CRUD thú cưng (đảm bảo không thể sửa/xóa pet của người khác).
2. **Kiểm thử thủ công:**
   * Thêm mới thú cưng với ảnh đại diện kích thước lớn để kiểm tra tối ưu hóa ảnh và hiển thị.
   * Xác nhận dữ liệu cập nhật lưu thành công vào PostgreSQL.
