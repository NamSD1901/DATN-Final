# ⏰ Automatic Notification Service (Hệ thống Thông báo & Nhắc lịch)

## 📝 Mô tả Tính năng
Xây dựng dịch vụ chạy ngầm trên máy chủ để phát hiện lịch hẹn tiêm phòng sắp đến hạn và gửi thông báo tự động (thông báo hệ thống & email) cho chủ nuôi.

## 📋 User Stories (Acceptance Criteria)
*   **PB34 (Nhắc lịch tiêm chủng):** Background job tự động quét các sổ tiêm phòng của thú cưng, gửi email nhắc nhở trước ngày tái chủng 3 - 5 ngày để khách hàng kịp đăng ký đặt lịch hẹn mới.

## 🛠️ Đặc tả Kỹ thuật (Technical Specs)
*   **Background Worker:** Hangfire hoặc Quartz.NET. Quét DB định kỳ vào lúc 08:00 sáng hàng ngày.
*   **Email Service:** Triển khai SMTP client kết nối qua Gmail/SendGrid.
*   **API Endpoints:**
    *   `GET /api/notifications` (Khách hàng xem danh sách thông báo hệ thống của mình).
*   **Database Tables:** `Notifications`, `Vaccination_records`.

## 🎨 Giao diện UI/UX
*   **Views/Components:** Popup chuông thông báo (Notification Dropdown) trên thanh Topbar của Dashboard, hiển thị số thông báo chưa đọc màu đỏ phát sáng.
