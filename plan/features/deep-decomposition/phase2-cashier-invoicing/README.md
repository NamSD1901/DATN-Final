# 💵 Cashier & Invoicing (Thu ngân & Hóa đơn)

## 📝 Mô tả Tính năng
Phân hệ hỗ trợ bộ phận thu ngân (lễ tân) tạo hóa đơn tổng hợp phí dịch vụ và tiền thuốc sau khi bác sĩ hoàn thành khám, xác nhận thanh toán trực tiếp hoặc chuyển khoản và in hóa đơn.

## 📋 User Stories (Acceptance Criteria)
*   **PB19 (Thanh toán hóa đơn):** Tự động tổng hợp dữ liệu từ bệnh án: Phí khám + Giá bán lẻ từng loại thuốc kê đơn $\times$ số lượng thuốc $\rightarrow$ xuất hóa đơn.
*   **PB19 (Xác nhận thanh toán):** Lễ tân bấm xác nhận trạng thái hóa đơn đã thanh toán (`Payment_status = 'Paid'`), chọn phương thức thanh toán (Tiền mặt, Chuyển khoản).

## 🛠️ Đặc tả Kỹ thuật (Technical Specs)
*   **API Endpoints:**
    *   `POST /api/receptionist/invoices` (Tạo hóa đơn từ mã ca khám)
    *   `PUT /api/receptionist/invoices/{id}/pay` (Xác nhận hóa đơn đã thanh toán)
    *   `GET /api/receptionist/invoices/{id}` (Xem chi tiết hóa đơn)
*   **Database Tables:** `Invoices`, `Invoice_items`, `Appointments` (Chuyển trạng thái sang `completed` sau thanh toán).

## 🎨 Giao diện UI/UX
*   **Views/Components:** `InvoicesTab.vue` (Cổng quản lý hóa đơn của thu ngân), popup giao diện in hóa đơn tinh tế (Print view).
