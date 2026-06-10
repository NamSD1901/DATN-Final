# 🎨 UI/UX Design Spec - Admin Drug Inventory

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Màu sắc phân cấp cảnh báo:
  - `--alert-critical`: `#EF4444` (Đỏ - Sắp hết hạn hoặc hết kho).
  - `--alert-warning`: `#F59E0B` (Vàng - Tồn kho dưới mức tối thiểu).
  - `--status-ok`: `#10B981` (Xanh lá - Bình thường).
- **FE-F01 (HTML Semantic):** Bảng danh sách thuốc dùng `<table>` với các `<thead>`, `<tbody>` chuẩn.

---

## 1. Giao diện Bảng Quản lý Kho Thuốc (Inventory Dashboard)

- **Tổng quan nhanh (Summary Cards):** Thẻ tóm tắt ở đầu trang hiển thị:
  - Số loại thuốc **sắp hết hàng** (tồn kho < mức tối thiểu).
  - Số loại thuốc **sắp hết hạn** (trong 30 ngày tới).
- **Bảng danh mục thuốc:**
  - Cột: Tên thuốc | Đơn vị | Tồn kho | Mức tối thiểu | Hạn sử dụng | Đơn giá bán | Hành động.
  - Dòng thuốc cảnh báo hiển thị viền đỏ nhấp nháy nhẹ hoặc nền hồng mờ kèm icon cảnh báo `⚠️`.
- **Modal Nhập kho (Restock):** Popup nhập số lượng hàng nhập thêm và ghi chú nguồn hàng, sau đó cộng dồn vào `StockQuantity`.
