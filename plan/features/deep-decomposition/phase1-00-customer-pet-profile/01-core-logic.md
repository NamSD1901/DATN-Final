# 01. CORE LOGIC & MIGRATION STRATEGY

## 1. MỤC TIÊU LÕI
Tách biệt thực thể **User** (dùng chung cho hệ thống) thành **Customer** (Chủ nuôi) và **Account/User** (Tài khoản) mà không làm mất liên kết dữ liệu cũ của bảng `Pets` và `Appointments`.

## 2. CHIẾN LƯỢC MIGRATION (ID SYNC)
- Dữ liệu hiện tại: Bảng `Pets` đang trỏ `OwnerId` vào `Users.Id`.
- Giải pháp:
  1. Thêm Migration để tạo bảng `Customers`.
  2. Dùng Raw SQL để copy dữ liệu: `INSERT INTO Customers (Id, FullName, Phone) SELECT Id, FullName, Phone FROM Users WHERE RoleId = [Customer_Role]`.
  3. Cập nhật EF Core: Sửa cấu hình `Pet.OwnerId` trỏ sang `Customers.Id`.
- Kết quả: Không cần update bất kỳ giá trị UUID nào trong DB, mọi liên kết tự động khớp nhau.

## 3. LOGIC LIÊN KẾT TÀI KHOẢN (AUTO-LINK)
- **Kịch bản:** Khách hàng đến quầy Lễ tân, được tạo `Customer` (Không có `User`). Sau đó khách lên Web đăng ký tài khoản bằng SĐT đó.
- **Xử lý Backend:**
  - Nhận API Request Đăng ký (SĐT, Password).
  - Tìm trong bảng `Customers` xem có ai dùng SĐT này không.
  - Nếu CÓ: Tạo bảng `Users` mới (Lưu Password), gán `Users.CustomerId = Customers.Id`.
  - Trả về Token. Khách hàng lập tức thấy hồ sơ của mình.
