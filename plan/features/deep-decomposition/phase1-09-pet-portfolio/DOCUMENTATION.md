# 📄 User & Dev Documentation - Pet Portfolio Management

Tài liệu cung cấp hướng dẫn vận hành chi tiết dành cho người dùng cuối và tài liệu hướng dẫn kỹ thuật dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng dành cho Khách hàng (End-User Guide)

### Bước 1: Truy cập trang quản lý Thú Cưng
1. Đăng nhập vào tài khoản Khách hàng của bạn trên hệ thống phòng khám.
2. Tại thanh điều hướng bên trái (Sidebar), nhấp chọn mục **"Thú cưng của tôi"** (hoặc **"My Pets"**).
3. Giao diện hiển thị danh sách các thẻ thú cưng hiện tại của bạn.

### Bước 2: Đăng ký thêm thú cưng mới
1. Nhấp vào nút **"+ Thêm Thú Cưng"** ở góc trên cùng bên phải màn hình.
2. Điền đầy đủ các thông tin trong biểu mẫu:
   *   **Tên thú cưng:** Nhập tên gọi ở nhà của bé (Ví dụ: Leo, Mimi, Lu).
   *   **Loài:** Chọn Chó, Mèo hoặc Khác từ danh sách.
   *   **Giống loài:** Nhập giống của bé (Ví dụ: Poodle, Ba Tư, Corgi...). Nếu là mèo ta hãy nhập "Mèo Mướp" hoặc "Mèo Ta".
   *   **Giới tính & Ngày sinh:** Cung cấp thông tin chính xác giúp bác sĩ chẩn đoán và tính tuổi.
   *   **Cân nặng:** Nhập cân nặng hiện tại của bé bằng số (ví dụ: `4.5`).
   *   **Mã số Chip (Microchip):** Nếu bé có gắn chip định danh dưới da, hãy nhập mã số vào đây để đồng bộ dữ liệu y tế quốc tế.
   *   **Ghi chú sức khỏe:** Điền các thông tin đặc biệt lưu ý như *"Dị ứng thuốc Penicillin"*, *"Sợ người lạ"*, *"Đang mang thai"*.
3. Nhấn **"Lưu hồ sơ"** để hoàn tất đăng ký.

### Bước 3: Cập nhật hoặc Xóa hồ sơ
*   **Cập nhật:** Click nút **"Sửa"** trên thẻ thú cưng tương ứng, thay đổi thông tin cần cập nhật (ví dụ sau mỗi tháng bé tăng cân) và chọn **"Lưu hồ sơ"**.
*   **Xóa:** Click nút **"Xóa"** (biểu tượng thùng rác), đọc kỹ thông báo xác nhận và chọn **"Xóa ngay"** nếu bé không còn được điều trị tại phòng khám.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
*   **Backend C# Core:**
    *   [MyPetsController.cs](file:///e:/DATN/MyPetClinic/backend/src/WebApi/Controllers/MyPetsController.cs) — API controller tiếp nhận các request CRUD từ client.
    *   [PetService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/PetService.cs) — Thực thi logic nghiệp vụ và chặn IDOR chéo tài khoản.
    *   [Pet.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Domain/Entities/User.cs) — Định nghĩa thực thể Entity cho bảng dữ liệu.
*   **Frontend Vue 3 SPA:**
    *   `frontend/src/stores/pets.ts` — Pinia Store quản lý danh sách reactive và state đồng bộ.
    *   `frontend/src/views/dashboard/MyPets.vue` — Trang hiển thị chính thức của Portfolio.

### B. Kiểm thử nhanh các Endpoint API bằng Curl

#### 1. Lấy danh sách thú cưng của tôi
```bash
curl -X GET "https://localhost:5001/api/mypets" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_TẠI_ĐÂY>"
```

#### 2. Thêm mới thú cưng
```bash
curl -X POST "https://localhost:5001/api/mypets" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_TẠI_ĐÂY>" \
     -d "{\"name\":\"Bé Leo\",\"species\":\"dog\",\"breed\":\"Poodle\",\"gender\":\"Male\",\"birthDate\":\"2024-05-15T00:00:00Z\",\"weight\":4.2,\"color\":\"Nâu\",\"bloodType\":\"DEA 1.1\",\"sterilized\":true,\"microchipCode\":\"MC-12345\",\"allergyNote\":\"Không\"}"
```

#### 3. Xóa mềm thú cưng
```bash
curl -X DELETE "https://localhost:5001/api/mypets/12" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_TẠI_ĐÂY>"
```

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Lỗi 1: Không thể sửa đổi thông tin - API trả về lỗi `403 Forbidden` hoặc `400 BadRequest`
*   **Nguyên nhân:** Lỗi xảy ra do cơ chế chặn IDOR. Bạn đang cố gắng gửi request cập nhật ID thú cưng không thuộc sở hữu của tài khoản đang đăng nhập (hoặc Token JWT bị hết hạn/lệch định danh).
*   **Cách kiểm tra:**
    1. Kiểm tra JWT Token trong Header đã đúng định dạng `Bearer <Token>` hay chưa.
    2. Kiểm tra cột `OwnerId` của dòng dữ liệu thú cưng đó trong bảng `Pets` xem có khớp với `Id` của tài khoản đang đăng nhập hay không.

### Lỗi 2: Trọng lượng thú cưng hiển thị sai số thập phân (Ví dụ `4.5` thành `4`)
*   **Nguyên nhân:** Do database hoặc DTO khai báo kiểu dữ liệu Số nguyên (`int` hoặc `long`) thay vì Số thực (`float` hoặc `real`).
*   **Cách khắc phục:** Đảm bảo trường `Weight` ở cả DTO (`CreatePetDto`, `UpdatePetDto`), Entity `Pet.cs` và Column Database đều được cấu hình kiểu `float?` hoặc `real` (PostgreSQL).
