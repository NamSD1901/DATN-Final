# 📝 Implementation Plan & Testing Strategy - Cashier & Invoicing

## 1. Kế hoạch Triển khai (Sprint 5)

| Giai đoạn | Task | Skills áp dụng | Est. |
|---|---|---|---|
| 1 | Tạo Entity `Invoice` và `InvoiceItem`, thiết lập các mối quan hệ DB và chạy Migration | BE-C02 (EF Core) | 1h |
| 2 | Hiện thực `InvoiceService` với logic tính tiền tự động từ dữ liệu Bệnh án và Đơn thuốc | BE-F01, BE-F03 | 2h |
| 3 | Xây dựng API `/api/receptionist/invoices` và logic thanh toán đồng bộ cập nhật Lịch hẹn | BE-A02 (UoW) | 2h |
| 4 | Xây dựng màn hình danh sách hóa đơn chờ thanh toán và popup in hóa đơn K80 | FE-F01, FE-F02 | 4h |
| 5 | Tích hợp thư viện sinh ảnh QR thanh toán VietQR động trên frontend | FE-F01 (HTML/JS) | 2h |

---

## 2. QA Test Suite (Kiểm thử chức năng & Tính đúng đắn của Hóa đơn)

### Case 1: Tính toán hóa đơn nháp chính xác
- **Các bước:** Bác sĩ kê đơn thuốc cho thú cưng có Phí khám (150.000 VNĐ), 2 viên thuốc A (giá 10.000 VNĐ/viên) ➡️ Thu ngân nhấn "Tạo hóa đơn".
- **Kết quả mong muốn:** Hóa đơn nháp được tạo ra với `ServiceAmount = 150000`, `MedicineAmount = 20000` và `TotalAmount = 170000`. HTTP 200.

### Case 2: Xác nhận thanh toán & đồng bộ lịch hẹn
- **Các bước:** Thu ngân chọn một hóa đơn `Unpaid` ➡️ Nhấn "Xác nhận đã thu tiền" ➡️ Chọn `Cash`.
- **Kết quả mong muốn:** Hóa đơn chuyển trạng thái sang `Paid`. Lịch hẹn tương ứng tự động chuyển `PaymentStatus = Paid`. Toàn bộ quá trình lưu vết trong DB đúng đắn.

### Case 3: Chặn thanh toán hóa đơn đã được thanh toán rồi
- **Các bước:** Gửi request PUT `/api/receptionist/invoices/{id-hóa-đơn-đã-Paid}/pay` lần thứ hai.
- **Kết quả mong muốn:** API trả về HTTP 400 Bad Request kèm thông báo lỗi "Hóa đơn này đã được thanh toán trước đó".
