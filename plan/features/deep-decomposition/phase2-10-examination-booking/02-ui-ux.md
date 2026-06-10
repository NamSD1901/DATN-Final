# 🎨 UI/UX Design Spec - Online Examination Booking

## 1. Bố cục Giao diện & Trực quan hóa (Responsive Layout)

Biểu mẫu đặt lịch hẹn khám bệnh trực tuyến được thiết kế theo dạng **Biểu mẫu đa bước (Multi-step Wizard Form)** mờ kính Glassmorphism sang trọng, bo góc mượt mà và hiển thị trực quan tiến trình hoàn thành.

### A. Sơ đồ ASCII Mockup - Bước chọn Thời gian & Bác sĩ (Steps 3 & 4 Layout)
```text
+-----------------------------------------------------------------------------------------------+
|  ĐẶT LỊCH HẸN KHÁM BỆNH                                                                   [X] |
+-----------------------------------------------------------------------------------------------+
|  [Tiến trình: (1) Thú Cưng ==> (2) Dịch Vụ ==> (*) Bác Sĩ & Giờ ==> (4) Xác Nhận]              |
|                                                                                               |
|  +-----------------------------------+   +-------------------------------------------------+  |
|  | [CHỌN BÁC SĨ ĐIỀU TRỊ]            |   | [CHỌN NGÀY VÀ KHUNG GIỜ RẢNH]                   |  |
|  |                                   |   |                                                 |  |
|  |  ( ) Bác sĩ bất kỳ (Hệ thống xếp) |   |  Ngày khám:                                     |  |
|  |  (*) Bác sĩ Trần Quốc Anh         |   |  [ 2026-06-15                              ][C]  |
|  |  ( ) Bác sĩ Lê Thị Mai            |   |                                                 |  |
|  |                                   |   |  Khung giờ trống y tế:                          |  |
|  |  [Ảnh Bác sĩ Anh]                 |   |  +------------+  +------------+  +------------+  |
|  |  Bác sĩ thú y chính khoa          |   |  | [ 08:00 ]  |  | [ 08:30 ]  |  | [ 09:00 ]  |  |
|  |  Kinh nghiệm: 8 năm lâm sàng      |   |  +------------+  +------------+  +------------+  |
|  |  Chuyên khoa: Ngoại khoa mèo      |   |  | [ 09:30 ]  |  |*[10:00]    |  | [10:30 ]   |  |
|  |                                   |   |  +------------+  +------------+  +------------+  |
|  |                                   |   |  | [14:00](B) |  | [14:30](B) |  | [15:00 ]   |  |
|  |                                   |   |  +------------+  +------------+  +------------+  |
|  |                                   |   |  *(B): Khung giờ đã có khách đặt (Blocked)      |  |
|  +-----------------------------------+   +-------------------------------------------------+  |
|                                                                                               |
|  [ Quay lại ]                                                                  [ Tiếp tục ]   |
+-----------------------------------------------------------------------------------------------+
```

---

## 2. Thiết kế Hệ thống Màu sắc & Trạng thái CSS (HSL Variables)

Bảng màu HSL phục vụ thiết kế Glassmorphism và trực quan hóa các khung giờ trống/bận:

```css
:root {
  /* Khối mờ kính */
  --booking-glass-bg: HSL(217, 33%, 17%, 0.7);
  --booking-border: HSL(217, 30%, 25%);
  
  /* Trạng thái Khung giờ (Time Slots) */
  --slot-bg-free: HSL(223, 47%, 10%);
  --slot-border-free: HSL(217, 20%, 30%);
  --slot-text-free: HSL(210, 40%, 98%);
  
  --slot-bg-active: HSL(239, 84%, 67%, 0.2);     /* Khung giờ đang chọn */
  --slot-border-active: HSL(235, 100%, 75%);
  --slot-glow-active: 0 0 10px HSL(235, 100%, 75%, 0.4);
  
  --slot-bg-blocked: HSL(223, 10%, 15%);          /* Khung giờ đã bị trùng lịch */
  --slot-border-blocked: HSL(217, 10%, 20%);
  --slot-text-blocked: HSL(215, 10%, 40%);        /* Màu xám tối */
}

/* Áp dụng kiểu dáng cho các ô chọn giờ (Time Slots) */
.time-slot-btn {
  background-color: var(--slot-bg-free);
  border: 1px solid var(--slot-border-free);
  color: var(--slot-text-free);
  padding: 10px 15px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.25s ease-in-out;
}

.time-slot-btn.is-active {
  background-color: var(--slot-bg-active);
  border-color: var(--slot-border-active);
  box-shadow: var(--slot-glow-active);
}

.time-slot-btn.is-blocked {
  background-color: var(--slot-bg-blocked);
  border-color: var(--slot-border-blocked);
  color: var(--slot-text-blocked);
  cursor: not-allowed;
  pointer-events: none; /* Khóa click */
}
```

---

## 3. Hoạt ảnh & Trải nghiệm Tương tác (UX/UI Animations)

### A. Hiệu ứng trượt chuyển bước (Step Slide Transition)
Khi người dùng nhấp "Tiếp tục" hoặc "Quay lại" giữa các bước của Wizard:
*   Bước hiện tại trượt mượt mà biến mất sang một bên và mờ dần trong `200ms`.
*   Bước tiếp theo xuất hiện từ hướng ngược lại, trượt nhẹ vào vị trí trung tâm, giúp người dùng cảm nhận được hành trình biểu mẫu (Progressive flow).

### B. Inline Validation & Visual Error Focus
*   Nếu người dùng nhấn "Tiếp tục" ở bước Triệu chứng mà chưa điền thông tin, ô nhập liệu triệu chứng sẽ rung nhẹ (Shake animation) và viền đổi màu đỏ để thu hút sự chú ý.
*   Nút "Tiếp tục" chỉ được hiển thị ở trạng thái sẵn sàng click khi bước hiện tại đã điền đủ thông tin hợp lệ.
