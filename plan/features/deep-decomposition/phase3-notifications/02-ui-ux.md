# 🎨 UI/UX Design Specification - Automatic Notification Service

Tài liệu thiết kế giao diện người dùng, sơ đồ bố cục ASCII Chuông thông báo, thiết kế Email nhắc tái chủng, bảng màu HSL và các hiệu ứng động rung lắc chuông (Bell Shake).

---

## 1. Bản vẽ Bố cục giao diện (ASCII Art Mockups)

### Menu Thả Chuông Thông báo In-app (In-app Notification Dropdown)
Bảng thông báo xuất hiện khi người dùng click vào Icon Chuông mờ kính trên thanh điều hướng đầu trang.

```text
+-------------------------------------------------------------------------+
|  [Logo] MYPETCLINIC                              [ Khám bệnh ]  [🔔(3)] |
+-------------------------------------------------------------------------+
                                                | HỘP THƯ THÔNG BÁO (3)  |
                                                |------------------------|
                                                | (*) Nhắc lịch tiêm     |
                                                | Bé LuLu có lịch tiêm   |
                                                | vắc-xin dại vào 14/06. |
                                                | [ Đặt lịch ]  10 phút  |
                                                |------------------------|
                                                | (*) Lịch hẹn được duyệt|
                                                | Lịch hẹn khám bé Kiki  |
                                                | đã được xác nhận.      |
                                                |               1 giờ    |
                                                |------------------------|
                                                |   [ ĐÁNH DẤU ĐỌC HẾT ] |
                                                +------------------------+
```

### Mẫu Thư Nhắc chủng Vắc-xin HTML Email Layout
```text
+-------------------------------------------------------------------------+
|                                                                         |
|                          MYPETCLINIC VET HOSPITAL                       |
|                 Địa chỉ: 123 Đường Nguyễn Trãi, Quận 1, TP.HCM          |
|                                                                         |
|  Chào Nguyễn Văn Nam,                                                   |
|                                                                         |
|  Bé cưng LULU của bạn có lịch tiêm nhắc lại vắc-xin ngừa bệnh DẠI         |
|  vào ngày 14/06/2026 (còn 3 ngày nữa).                                  |
|                                                                         |
|  Việc tái chủng đúng hẹn giúp bảo vệ cún cưng tối đa trước các virus    |
|  nguy hiểm đe dọa tính mạng.                                            |
|                                                                         |
|              +-------------------------------------------+              |
|              |     ĐẶT LỊCH HẸN TÁI CHỦNG NHANH TẠI ĐÂY  |              |
|              +-------------------------------------------+              |
|                                                                         |
|  Thân mến,                                                              |
|  Đội ngũ phòng khám MyPetClinic                                         |
|                                                                         |
+-------------------------------------------------------------------------+
```

---

## 2. Hệ thống CSS Design Tokens (HSL Color Theme)

Bảng màu HSL phân biệt thông báo Đã đọc và Chưa đọc:

```css
:root {
  /* Bong bóng đếm số thông báo */
  --notification-badge: hsl(355, 75%, 50%);    /* Đỏ neon cảnh báo số chưa đọc */
  
  /* Trạng thái các hàng tin nhắn */
  --notif-unread-bg: hsla(200, 90%, 55%, 0.08); /* Xanh lam nhạt - Chưa đọc */
  --notif-unread-border: hsla(200, 90%, 55%, 0.15);
  
  --notif-read-bg: transparent;                 /* Trong suốt - Đã đọc */
  --notif-read-border: hsla(220, 10%, 100%, 0.05);

  --notif-text-primary: hsl(210, 20%, 98%);
  --notif-text-secondary: hsl(215, 15%, 75%);

  --backdrop-blur: blur(12px);
  --dropdown-shadow: 0 10px 30px rgba(0, 0, 0, 0.45);
}
```

---

## 3. Hoạt ảnh Rung Chuông Nhẹ khi nhận thông báo mới (Bell Shake)

Khi SignalR đẩy một thông báo mới về trình duyệt, icon hình chuông trên Header sẽ tự động thực hiện hoạt ảnh rung lắc xoay nhẹ (Bell Shake) trong 800ms để thu hút sự chú ý trực quan:

```css
/* Hiệu ứng rung lắc chuông */
.bell-shake-animation {
  animation: bellShake 0.8s ease-in-out;
}

@keyframes bellShake {
  0% { transform: rotate(0); }
  15% { transform: rotate(15deg); }
  30% { transform: rotate(-15deg); }
  45% { transform: rotate(10deg); }
  60% { transform: rotate(-10deg); }
  75% { transform: rotate(5deg); }
  85% { transform: rotate(-5deg); }
  100% { transform: rotate(0); }
}

/* Hiệu ứng nhấp nháy phát sáng Badge số đếm */
.badge-glow {
  box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7);
  animation: badgePulse 1.8s infinite;
}

@keyframes badgePulse {
  0% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.5);
  }
  70% {
    transform: scale(1);
    box-shadow: 0 0 0 6px rgba(239, 68, 68, 0);
  }
  100% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(239, 68, 68, 0);
  }
}
```

---

## 4. Tương tác Cuộn trang và Đọc thông báo (Scroll & Read interactions)

- **Đọc nhanh từng mục:** Khi người dùng mở dropdown và di chuột qua một dòng thông báo chưa đọc trong 1.5 giây, hệ thống sẽ tự động gọi API ngầm đánh dấu đã đọc mục đó và đổi background từ `--notif-unread-bg` sang `--notif-read-bg` một cách mềm mại.
- **Cuộn vô hạn (Infinite Scroll):** Hộp thư dropdown chỉ hiển thị 5 thông báo gần nhất. Nếu người dùng bấm "Xem tất cả", trình duyệt chuyển hướng sang trang `/profile/notifications` hỗ trợ cuộn vô hạn để tải thêm lịch sử thông báo cũ hơn.
