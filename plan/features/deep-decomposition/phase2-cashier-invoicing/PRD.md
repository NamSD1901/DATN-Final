# 🚀 Product Requirements Document (PRD) - Cashier & Invoicing

## 1. Tổng quan & Tầm nhìn
Phân hệ **Thu ngân & Hóa đơn (Cashier & Invoicing)** là điểm chạm cuối cùng trong hành trình khám chữa bệnh tại phòng khám. Tính năng này giúp lễ tân/thu ngân tự động kết xuất hóa đơn chi tiết ngay khi bác sĩ hoàn tất ca khám, bao gồm: Phí khám dịch vụ, tiền thuốc kê đơn, và vắc-xin (nếu có). Thu ngân có thể ghi nhận thanh toán nhanh chóng (Tiền mặt hoặc Chuyển khoản ngân hàng qua mã QR động) và in biên lai hóa đơn chuyên nghiệp cho khách hàng.

---

## 2. Đối tượng sử dụng (Target Persona)
- **Lễ tân / Thu ngân (Receptionist / Cashier):** Cần thao tác xuất hóa đơn cực nhanh, tự động hóa toàn bộ khâu tính toán tiền để tránh sai sót thủ công.

---

## 3. Yêu cầu Nghiệp vụ Chi tiết
- **Tự động Tổng hợp Hóa đơn (PB19):**
  - Khi bác sĩ hoàn thành ca khám (trạng thái Appointment là `Completed`), thu ngân có thể nhấn "Tạo hóa đơn" (hoặc hệ thống tự động tạo nháp).
  - Hóa đơn tổng hợp tự động bao gồm:
    - **Phí dịch vụ:** Phí khám lâm sàng, phí xét nghiệm hoặc phí tiêm chủng.
    - **Tiền thuốc/vắc-xin:** Số lượng kê đơn $\times$ Đơn giá bán lẻ tại thời điểm xuất hóa đơn.
- **Xác nhận Thanh toán:**
  - Thu ngân lựa chọn phương thức thanh toán: `Tiền mặt (Cash)` hoặc `Chuyển khoản (Bank Transfer)`.
  - Cập nhật trạng thái hóa đơn thành `Paid`. Cập nhật trạng thái thanh toán của Lịch hẹn tương ứng thành `Paid`.
- **Hỗ trợ in ấn Hóa đơn chuyên nghiệp:**
  - Thiết kế trang in hóa đơn (Print View) tối giản, căn lề chuẩn hóa đơn nhiệt (kích thước K80 hoặc A5) để in trực tiếp qua máy in hóa đơn của phòng khám.
