# 🎨 UI/UX Design Spec - Automatic Notification Service

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Sử dụng các gam màu tương phản tốt cho badge thông báo:
  - `--notify-badge-bg`: `#EF4444` (Màu đỏ tươi thông báo chưa đọc).
  - `--notify-item-unread`: `#FDF2F8` (Nền hồng nhạt/xám mờ cho tin nhắn chưa đọc).
- **FE-F01 (HTML Semantic):** Sử dụng cấu trúc danh sách `<ul>` và `<li>` để hiển thị hộp thư thông báo dropdown.

---

## 1. Giao diện Dropdown thông báo ở thanh Topbar (Notification Center)

- **Vị trí hiển thị:** Cạnh avatar người dùng ở góc trên cùng bên phải thanh Topbar, biểu diễn bằng icon hình chiếc chuông.
- **Badge số lượng tin:** Nếu có thông báo chưa đọc, hiển thị hình tròn đỏ nhỏ bên góc trên của chuông kèm số lượng (VD: `3`).
- **Hiệu ứng Animation:**
  - Chuông rung nhẹ (Shake Animation) khi SignalR nhận được thông báo mới chuyển từ server xuống.
  - Hiệu ứng đổ bóng mờ kính (Glassmorphism backdrop-filter) cho khung dropdown thông báo khi nhấp mở chuông.

- **Mẫu HTML Item thông báo:**
  ```html
  <li class="notification-item" :class="{ unread: !item.isRead }">
    <div class="item-header">
      <span class="title">📅 Nhắc lịch tiêm vaccine</span>
      <span class="time">3 giờ trước</span>
    </div>
    <p class="body-text">Bé Bông sắp đến lịch tiêm mũi vaccine mới vào ngày 18/06/2026.</p>
  </li>
  ```
