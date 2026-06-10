# 🎨 UI/UX Design Specification - Customer Appointments Dashboard

## 1. Bản vẽ Thiết kế Giao diện (ASCII Mockups)

### A. Giao diện Bảng điều khiển Lịch hẹn (Appointments Dashboard View)

```text
+---------------------------------------------------------------------------------------------------+
|  [Logo MyPetClinic]  Trang chủ    Đặt lịch khám    Đặt lịch tiêm    [QUẢN LÝ LỊCH HẸN]    (Avatar) |
+---------------------------------------------------------------------------------------------------+
|                                                                                                   |
|  DANH SÁCH LỊCH HẸN KHÁM & TIÊM PHÒNG                                                             |
|                                                                                                   |
|  +---------------------------------------------------------------------------------------------+  |
|  | [ Tất cả (5) ]  [ Chờ duyệt (1) ]  [ Đã duyệt (2) ]  [ Đang khám (0) ]  [ Đã hủy (2) ]          |  |
|  +---------------------------------------------------------------------------------------------+  |
|                                                                                                   |
|  +-----------------------------------------+  +-------------------------------------------------+ |
|  | Grid / List Thẻ Lịch hẹn                |  | Khung hiển thị chi tiết (Chọn bên trái)         | |
|  |                                         |  |                                                 | |
|  | +-------------------------------------+ |  | LỊCH HẸN: #LH-00472                             | |
|  | | 🐶 Bé LuLu          [ Chờ duyệt ]   | |  | Thú cưng: 🐶 LuLu (Chó Poodle - 12 tháng)       | |
|  | | Dịch vụ: Khám tổng quát             | |  | Dịch vụ: Khám tổng quát chó mèo                 | |
|  | | Bác sĩ: BS. Trần Quốc Anh           | |  | Bác sĩ phụ trách: BS. Trần Quốc Anh             | |
|  | | Thời gian: 10:00 - 15/06/2026       | |  | Thời gian hẹn: 10:00, Thứ Hai ngày 15/06/2026   | |
|  | +-------------------------------------+ |  | Trạng thái: [ Chờ duyệt ]                       | |
|  |                                         |  |                                                 | |
|  | +-------------------------------------+ |  | Tiến trình lịch hẹn (Timeline):                 | |
|  | | 🐱 Bé Miu           [ Đã duyệt ]    | |  | (x) Đã đặt ---> ( ) Đã xác nhận ---> ( ) Đang khám | |
|  | | Dịch vụ: Tiêm phòng 4 bệnh          | |  |                                                 | |
|  | | Bác sĩ: Bác sĩ bất kỳ               | |  | [ Quét mã tiếp nhận nhanh tại quầy ]            | |
|  | | Thời gian: 14:30 - 16/06/2026       | |  |      +-------------+                            | |
|  | +-------------------------------------+ |  |      |  [QR CODE]  |                            | |
|  |                                         |  |      |   PATIENT   |                            | |
|  | +-------------------------------------+ |  |      |   CHECKIN   |                            | |
|  | | 🐶 Bé LuLu          [ Đã hủy ]      | |  |      +-------------+                            | |
|  | | Dịch vụ: Tiêm Dại (Rabisin)         | |  |      Mã Token: QR-LH00472                       | |
|  | | Thời gian: 09:00 - 10/05/2026       | |  |                                                 | |
|  | +-------------------------------------+ |  | [ HỦY LỊCH HẸN ] (Chỉ khả dụng cho Pending/Confirm)| |
|  +-----------------------------------------+  +-------------------------------------------------+ |
+---------------------------------------------------------------------------------------------------+
```

### B. Hộp thoại Xác nhận Hủy lịch hẹn (Cancel Confirmation Dialog)

```text
+-----------------------------------------------------------------+
| HỦY LỊCH HẸN Y TẾ                                           [X] |
+-----------------------------------------------------------------+
|                                                                 |
| Bạn đang thực hiện hủy lịch hẹn khám cho bé LuLu vào lúc        |
| 10:00 ngày 15/06/2026. Hành động này không thể hoàn tác.        |
|                                                                 |
| Vui lòng cho phòng khám biết lý do hủy lịch của bạn:            |
| +-------------------------------------------------------------+ |
| | Bé bị mệt đột xuất không đi được / Tôi bận lịch công tác... | |
| |                                                             | |
| +-------------------------------------------------------------+ |
| (Yêu cầu nhập tối thiểu 10 ký tự để gửi. Hiện tại: 25/500)      |
|                                                                 |
| [ ] Tôi xác nhận muốn hủy lịch và giải phóng ca trực bác sĩ.    |
|                                                                 |
|    [ BỎ QUA ]                          [ XÁC NHẬN HỦY LỊCH ]    |
+-----------------------------------------------------------------+
```

---

## 2. Hệ thống CSS Variable & Token Trạng thái (HSL Color Tokens)

Đặc tả các màu sắc trực quan (HSL) phản ánh trạng thái của từng lịch hẹn trên giao diện:

```css
:root {
  /* Bảng màu Trạng thái Lịch hẹn */
  --status-pending-bg: hsl(45, 100%, 96%);     /* Vàng nhạt mờ */
  --status-pending-text: hsl(45, 100%, 35%);   /* Vàng đậm hổ phách */
  --status-pending-border: hsl(45, 100%, 75%);

  --status-confirmed-bg: hsl(210, 100%, 96%);   /* Xanh dương nhạt */
  --status-confirmed-text: hsl(210, 100%, 45%); /* Xanh dương */
  --status-confirmed-border: hsl(210, 100%, 80%);

  --status-waiting-bg: hsl(280, 100%, 97%);     /* Tím nhạt (Đã đến quầy, chờ gọi) */
  --status-waiting-text: hsl(280, 100%, 45%);   /* Tím */
  --status-waiting-border: hsl(280, 100%, 85%);

  --status-inprogress-bg: hsl(190, 100%, 95%);  /* Xanh ngọc (Đang trong phòng khám) */
  --status-inprogress-text: hsl(190, 100%, 35%);/* Xanh ngọc đậm */
  --status-inprogress-border: hsl(190, 100%, 75%);

  --status-completed-bg: hsl(145, 100%, 96%);   /* Xanh lá nhạt */
  --status-completed-text: hsl(145, 100%, 35%); /* Xanh lá */
  --status-completed-border: hsl(145, 100%, 75%);

  --status-cancelled-bg: hsl(0, 100%, 97%);     /* Đỏ nhạt */
  --status-cancelled-text: hsl(0, 100%, 40%);   /* Đỏ */
  --status-cancelled-border: hsl(0, 100%, 85%);

  /* Glassmorphism Tokens */
  --glass-bg: rgba(255, 255, 255, 0.45);
  --glass-bg-dark: rgba(15, 23, 42, 0.6);
  --glass-border: rgba(255, 255, 255, 0.25);
  --glass-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.08);
}
```

---

## 3. Hoạt ảnh Tương tác Vi mô (Micro-animations)

*   **Hover phồng nhẹ (Card Pop-up):** Khi hover chuột qua thẻ lịch hẹn, thẻ dịch chuyển nhẹ lên trên và đổ bóng đậm hơn để tạo cảm giác phản hồi xúc giác.
    ```css
    .appointment-card {
      transition: transform 0.25s cubic-bezier(0.4, 0, 0.2, 1), box-shadow 0.25s ease;
    }
    .appointment-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 24px rgba(0, 0, 0, 0.06);
    }
    ```
*   **Hiệu ứng rung lắc nút hủy (Shake Error Animation):** Nếu người dùng nhấn nút "Xác nhận hủy lịch" nhưng chưa nhập đủ 10 ký tự lý do hủy, ô nhập liệu và nút bấm sẽ lắc nhẹ theo chiều ngang để cảnh báo.
    ```css
    @keyframes shake {
      0%, 100% { transform: translateX(0); }
      20%, 60% { transform: translateX(-6px); }
      40%, 80% { transform: translateX(6px); }
    }
    .shake-error {
      animation: shake 0.4s ease-in-out;
    }
    ```

---

## 4. Thiết kế Thích ứng (Responsive Design Rules)

*   **Desktop Layout (>= 1024px):** Giao diện chia 2 cột song song (Cột trái chiếm 40% hiển thị danh sách cuộn, Cột phải chiếm 60% hiển thị chi tiết lịch hẹn cố định). Trải nghiệm tối ưu, giảm thao tác bấm mở/đóng.
*   **Tablet Layout (768px - 1023px):** Thẻ lịch hẹn thiết kế dạng Grid 2 cột. Nhấp vào thẻ sẽ hiển thị một Modal chi tiết trượt từ dưới lên (Slide-up Bottom Sheet).
*   **Mobile Layout (< 768px):** Danh sách xếp dọc 1 cột. Mỗi lịch hẹn hiển thị dạng thẻ rút gọn. Khi click vào thẻ sẽ mở Modal chi tiết toàn màn hình (Full-screen Overlay) có nút [X] đóng ở góc trên bên phải để tối ưu hóa không gian hiển thị trên điện thoại.
