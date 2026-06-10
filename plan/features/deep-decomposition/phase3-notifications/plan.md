# 📝 Implementation Plan & Testing Strategy - Automatic Notification Service

## 1. Kế hoạch Triển khai (Sprint 6)

| Giai đoạn | Task | Skills áp dụng | Est. |
|---|---|---|---|
| 1 | Tạo Entity `Notification`, chạy Migration cập nhật DB PostgreSQL | BE-C02 (EF Core) | 1h |
| 2 | Code logic SMTP Mailer và xây dựng `VaccinationReminderJob` | BE-F01, BE-F03 | 3h |
| 3 | Đăng ký và cấu hình Hangfire Dashboard, lên lịch Scheduler định kỳ 08:00 mỗi ngày | BE-F02, BE-F03 | 2h |
| 4 | Xây dựng API GET/PUT phục vụ đọc và quản lý thông báo, kiểm tra IDOR bảo mật | BE-A03, BE-F03 | 2h |
| 5 | Code giao diện Dropdown thông báo thời gian thực tích hợp SignalR trên Vue | FE-F01, FE-C03 | 4h |

---

## 2. QA Test Suite (Kiểm thử chức năng & Tính đúng đắn của Email)

### Case 1: Quét và gửi thông báo nhắc lịch chính xác
- **Các bước:** Tạo dữ liệu tiêm chủng cho cún Bông của Khách hàng A có ngày hẹn tái chủng `NextDoseDate` đúng vào ngày hiện tại cộng thêm 3 ngày ➡️ Thực thi cưỡng bức (Trigger manually) Hangfire Job `vaccination-daily-reminder`.
- **Kết quả mong muốn:** 
  - Khách hàng A nhận được 1 email thông báo nhắc lịch.
  - Bảng `Notifications` ghi nhận thêm 1 dòng thông báo trạng thái `IsRead = false`.
  - Icon chuông hiển thị badge số lượng tăng lên.

### Case 2: Kiểm thử chống tấn công IDOR xem thông báo
- **Các bước:** Đăng nhập tài khoản Khách hàng A ➡️ Gửi request chỉnh sửa trạng thái đọc: `PUT /api/notifications/{id-thông-báo-của-Khách-hàng-B}/read`.
- **Kết quả mong muốn:** API trả về HTTP 403 Forbidden. Trạng thái thông báo của Khách hàng B vẫn giữ nguyên là chưa đọc (`IsRead = false`).

### Case 3: Đánh dấu đã đọc tất cả thông báo thành công
- **Các bước:** Nhấp nút "Đọc tất cả thông báo" trên giao diện Dropdown.
- **Kết quả mong muốn:** Gọi thành công PUT `/api/notifications/read-all`. Toàn bộ thông báo chuyển màu từ hồng mờ sang trắng. Số lượng tin nhắn chưa đọc trên badge chuông trở về 0.
