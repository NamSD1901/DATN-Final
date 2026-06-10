# 📄 User & Dev Documentation - Profile Details Update

Tài liệu cung cấp hướng dẫn vận hành chi tiết dành cho người dùng và tài liệu tích hợp kỹ thuật dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng dành cho Người dùng (End-User Guide)

### Bước 1: Truy cập màn hình quản lý tài khoản
1. Đăng nhập vào tài khoản của bạn thông qua cổng đăng nhập MyPetClinic.
2. Click vào tên hiển thị hoặc Avatar của bạn ở thanh điều hướng trên cùng bên phải, sau đó chọn **"Hồ sơ cá nhân"** (hoặc **"Cấu hình tài khoản"**).
3. Màn hình mặc định hiển thị thông tin ở trạng thái **Chỉ xem (Read-only)**.

### Bước 2: Thực hiện chỉnh sửa hồ sơ
1. Click vào nút **"Chỉnh sửa"** nằm ở góc dưới cùng bên phải của khối biểu mẫu. Giao diện biểu mẫu sẽ kích hoạt các ô nhập liệu.
2. Bạn có thể cập nhật các thông tin sau:
   *   **Họ và tên:** Vui lòng nhập đúng họ tên thật tiếng Việt, không chứa ký hiệu lạ hoặc số.
   *   **Số điện thoại:** Bắt buộc nhập 10 chữ số (Ví dụ: `0987654321`) phục vụ nhận SMS thông báo nhắc lịch khám/tiêm phòng cho thú cưng.
   *   **Địa chỉ:** Cung cấp địa chỉ hiện tại để phục vụ dịch vụ cấp cứu thú cưng tận nhà hoặc giao thuốc.
   *   **Giới tính & Ngày sinh:** Cập nhật để nhận ưu đãi hoặc hỗ trợ tốt nhất từ phòng khám.
3. **Lưu ý quan trọng về Email:** Địa chỉ email không thể tự chỉnh sửa. Đây là khóa định danh đăng nhập cố định của bạn. Nếu cần đổi Email, vui lòng liên hệ trực tiếp với quầy Lễ tân của phòng khám để được hỗ trợ xác minh giấy tờ tùy thân.

### Bước 3: Lưu hoặc Hủy bỏ
*   Sau khi nhập đầy đủ thông tin hợp lệ, hãy click nút **"Lưu thay đổi"**. Hệ thống sẽ tải dữ liệu và hiển thị thông báo: *"Cập nhật thông tin cá nhân thành công!"*.
*   Nếu không muốn thay đổi nữa, click **"Hủy bỏ"** để quay về trạng thái cũ.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
*   **Backend Code:**
    *   [ProfileController.cs](file:///e:/DATN/MyPetClinic/backend/src/WebApi/Controllers/ProfileController.cs) — Định nghĩa API Endpoint `GET /api/profile` và `PUT /api/profile` được bảo vệ bằng middleware xác thực `[Authorize]`.
    *   [UpdateProfileDto.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/DTOs/UpdateProfileDto.cs) — Cấu trúc DTO nhận payload cập nhật từ client.
    *   [UserService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/UserService.cs) — Logic xử lý cập nhật cơ sở dữ liệu PostgreSQL.
*   **Frontend Code:**
    *   [MyAppointmentsTab.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/dashboard/MyAppointmentsTab.vue) (Trang dashboard tham chiếu)
    *   `frontend/src/stores/profile.ts` — Quản lý trạng thái Pinia, cờ `isDirty` và lưu trữ đệm dữ liệu người dùng.

### B. Kiểm thử nhanh API bằng công cụ Curl
Bạn có thể sử dụng công cụ dòng lệnh `curl` để gửi request kiểm tra nhanh API (yêu cầu gửi kèm Token JWT):

#### 1. Lấy thông tin hồ sơ
```bash
curl -X GET "https://localhost:5001/api/profile" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_TẠI_ĐÂY>"
```

#### 2. Cập nhật thông tin hồ sơ
```bash
curl -X PUT "https://localhost:5001/api/profile" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_TẠI_ĐÂY>" \
     -d "{\"fullName\":\"Nguyễn Văn Khách\",\"phone\":\"0988776655\",\"address\":\"456 Tôn Đức Thắng, Quận 1, TP HCM\",\"gender\":1,\"dateOfBirth\":\"1992-08-20T00:00:00Z\"}"
```

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Lỗi 1: `DateTime` Timezone Exception với PostgreSQL (`Npgsql`)
*   **Mô tả lỗi:** Khi lưu trường `DateOfBirth`, backend quăng ngoại lệ: *"Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'... Specify Kind=Utc."*
*   **Nguyên nhân:** PostgreSQL cấu hình kiểu dữ liệu có múi giờ (`timestamptz`), trong khi C# gửi xuống kiểu dữ liệu `DateTime` không xác định múi giờ (`DateTimeKind.Unspecified`).
*   **Cách khắc phục:** Tại `UserService.cs`, trước khi cập nhật trường, bắt buộc phải dùng lệnh `DateTime.SpecifyKind` để ép múi giờ UTC:
    ```csharp
    user.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc);
    ```

### Lỗi 2: Trình duyệt báo lỗi CORS khi gửi request PUT
*   **Mô tả lỗi:** Console của Chrome báo lỗi: *"Access to XMLHttpRequest at '...' from origin 'http://localhost:5173' has been blocked by CORS policy..."*
*   **Cách khắc phục:** Hãy kiểm tra cấu hình CORS trong `Program.cs` ở Backend. Endpoint PUT yêu cầu kiểm tra tiền trình `OPTIONS` của browser (Preflight request). Hãy đảm bảo Policy CORS cấu hình đầy đủ `WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")` và sử dụng `WithOrigins` đúng cổng localhost của Vue (`5173`).
