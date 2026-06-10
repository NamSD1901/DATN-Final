# 🎨 UI/UX Design Spec - Admin Staff Management

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Sử dụng gam màu cho trạng thái tài khoản:
  - `--status-active`: `#10B981` (Xanh lá - Hoạt động)
  - `--status-locked`: `#EF4444` (Đỏ - Khóa)
- **FE-F01 (HTML Semantic):** Sử dụng các thẻ semantic bảng biểu `<table>` và cấu hình phân trang rõ ràng.

---

## 1. Giao diện Danh sách Nhân sự (Staff Directory Dashboard)

- **Layout:** Thiết kế dạng danh sách bảng lớn (Datatable) hiển thị rõ ràng thông tin: Ảnh đại diện, Tên đầy đủ, Email, Số điện thoại, Vai trò (Role badge), Ngày gia nhập và Trạng thái.
- **Role Badges:**
  - `Admin`: Nền đỏ, chữ trắng.
  - `Doctor`: Nền xanh ngọc, chữ trắng.
  - `Receptionist`: Nền tím nhạt, chữ trắng.
  - `Cashier`: Nền vàng đậm, chữ trắng.
- **Hành động nhanh (Quick Action Context):**
  - Mỗi dòng nhân viên có nút bấm thao tác nhanh: Sửa thông tin, Đổi Role, Khóa/Mở khóa.
  - Popup xác nhận khóa tài khoản có thông báo rõ ràng về quyền truy cập bị vô hiệu hóa.
