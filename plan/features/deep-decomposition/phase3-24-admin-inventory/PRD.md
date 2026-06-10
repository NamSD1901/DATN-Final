# 🚀 Product Requirements Document (PRD) - Admin Drug Inventory

## 1. Tổng quan & Tầm nhìn
Phân hệ **Quản lý Kho thuốc & Vật tư (Admin Drug Inventory)** cung cấp giao diện quản trị cho Admin kiểm soát danh mục thuốc điều trị, cập nhật số lượng tồn kho thực tế, điều chỉnh đơn giá nhập/bán, và đưa ra các cảnh báo trực quan khi thuốc trong kho sắp cạn kiệt hoặc sắp hết hạn sử dụng. Điều này giúp tránh tình trạng gián đoạn ca điều trị tại phòng khám và kiểm soát chặt chẽ hàng hóa.

---

## 2. Yêu cầu Nghiệp vụ Chi tiết
- **Quản lý danh mục thuốc (PB29):**
  - Thêm, sửa, xóa các loại thuốc trong danh mục (Tên thuốc, đơn vị tính, đơn giá nhập, đơn giá bán lẻ, nhà sản xuất).
- **Kiểm soát Tồn kho & Cảnh báo:**
  - Thiết lập định mức tồn kho tối thiểu cho mỗi loại thuốc (ví dụ: tối thiểu là 10 hộp).
  - Tự động hiển thị nhãn cảnh báo màu đỏ (`Low Stock`) nếu số lượng tồn kho thực tế giảm xuống dưới định mức tối thiểu.
  - Hiển thị nhãn cảnh báo đỏ đối với các lô thuốc sắp hết hạn sử dụng (trong vòng 30 ngày) để nhân viên phòng khám xử lý trả hàng hoặc tiêu hủy.
- **Lịch sử nhập/xuất kho:**
  - Ghi nhận lịch sử mỗi lần Admin nhập hàng mới vào kho (tự động cộng dồn số lượng tồn).
