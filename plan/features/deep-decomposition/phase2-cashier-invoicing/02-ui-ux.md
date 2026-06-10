# 🎨 UI/UX Design Spec - Cashier & Invoicing

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Sử dụng biến màu sắc sáng/tối cho hóa đơn, thiết kế phong cách hóa đơn tối giản (minimalist).
- **FE-F01 (HTML Semantic):** Sử dụng các thẻ table chuẩn (`<table>`, `<thead>`, `<tbody>`, `<tfoot>`) để hiển thị danh sách dòng hàng hóa đơn.

---

## 1. Thiết kế Giao diện In ấn Hóa đơn (Print Template CSS)

Khi in hóa đơn, hệ thống cần ẩn đi toàn bộ các thành phần định dạng Web như Header, Sidebar, các Nút bấm và chỉ hiển thị khung hóa đơn khổ K80 tiêu chuẩn:

```css
@media print {
  body * {
    visibility: hidden;
  }
  #invoice-print-area, #invoice-print-area * {
    visibility: visible;
  }
  #invoice-print-area {
    position: absolute;
    left: 0;
    top: 0;
    width: 80mm; /* Kích thước chuẩn giấy in nhiệt */
    font-family: 'Courier New', Courier, monospace;
    font-size: 12px;
  }
  .no-print {
    display: none !important;
  }
}
```

## 2. Giao diện Cổng Thu ngân (Invoicing Board)
- Thiết kế danh sách các hóa đơn cần thanh toán dạng bảng phân chia trạng thái: `Chờ thanh toán` (Unpaid) và `Đã thanh toán` (Paid).
- Sử dụng hiệu ứng chuyển đổi trạng thái hóa đơn mượt mà, đổi màu badge trạng thái từ đỏ sang xanh lá cây khi thu ngân bấm xác nhận thanh toán thành công.
- Tích hợp nút tạo mã QR Code động chuyển khoản ngân hàng (VietQR standard) chứa thông tin: Số tài khoản phòng khám, Tên chủ tài khoản, Số tiền phải trả (`TotalAmount`) và nội dung chuyển khoản (`Mã hóa đơn`).
