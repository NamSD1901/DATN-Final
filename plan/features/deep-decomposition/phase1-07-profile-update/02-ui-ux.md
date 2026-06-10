# 🎨 UI/UX Design Spec - Profile Details Update

## 1. Bố cục Giao diện (Responsive Layout)

Giao diện Hồ sơ cá nhân (Profile Details) được thiết kế theo phong cách tối giản, sang trọng (Glassmorphism), sử dụng các khối mờ kính trên nền tối sâu thẳm nhằm tạo cảm giác cao cấp và hiện đại cho ứng dụng phòng khám thú y **MyPetClinic**.

### A. Sơ đồ ASCII Mockup - Phiên bản Desktop (Màn hình lớn >= 1024px)
```text
+-------------------------------------------------------------------------------------------------------+
|  [Sidebar Navigation]  |  MY PROFILE (HỒ SƠ CỦA TÔI)                                                 |
|                        |                                                                              |
|  * Dashboard           |  +--------------------------------+  +------------------------------------+  |
|  * My Pets             |  | [LEFT PANEL - AVATAR & INFO]   |  | [RIGHT PANEL - DETAILED FORM]      |  |
|  * Book Appointment    |  |                                |  |                                    |  |
|  > Account Settings    |  |     +--------------------+     |  |  Họ và tên:                        |  |
|  * Logout              |  |     |                    |     |  |  [ Nguyễn Khách Hàng           ]   |  |
|                        |  |     |     Avatar Img     |     |  |                                    |  |
|                        |  |     |                    |     |  |  Số điện thoại:                    |  |
|                        |  |     +--------------------+     |  |  [ 0987654321                  ]   |  |
|                        |  |       Thay đổi ảnh đại diện    |  |                                    |  |
|                        |  |                                |  |  Địa chỉ liên hệ:                  |  |
|                        |  |  Họ tên: Nguyễn Khách Hàng     |  |  [ 123 Đường ABC, Quận 1, TP. HCM  ]   |  |
|                        |  |  Email: khachhang@gmail.com 🔒  |  |                                    |  |
|                        |  |  Vai trò: Khách hàng           |  |  Giới tính:                        |  |
|                        |  |  Ngày tham gia: 15/05/2026     |  |  ( ) Nam   (*) Nữ   ( ) Khác       |  |
|                        |  |                                |  |                                    |  |
|                        |  |                                |  |  Ngày sinh:                        |  |
|                        |  |                                |  |  [ 15 / 05 / 1995            [Calendar]|  |
|                        |  +--------------------------------+  |                                    |  |
|                        |                                      |  [ Hủy bỏ ]    [ Lưu Thay Đổi ]    |  |
|                        |                                      +------------------------------------+  |
+-------------------------------------------------------------------------------------------------------+
```

### B. Sơ đồ ASCII Mockup - Phiên bản Mobile (Màn hình di động < 640px)
```text
+------------------------------------+
| [=] MYPETCLINIC                ( ) |
+------------------------------------+
| HỒ SƠ CỦA TÔI                      |
|                                    |
| +--------------------------------+ |
| | [AVATAR & INFO - CENTERED]     | |
| |       +------------+           | |
| |       |   Avatar   |           | |
| |       +------------+           | |
| |     Nguyễn Khách Hàng          | |
| |     khachhang@gmail.com 🔒     | |
| +--------------------------------+ |
|                                    |
| +--------------------------------+ |
| | [DETAILED FORM]                | |
| | Họ và tên:                     | |
| | [ Nguyễn Khách Hàng          ] | |
| |                                | |
| | Số điện thoại:                 | |
| | [ 0987654321                 ] | |
| |                                | |
| | Địa chỉ liên hệ:               | |
| | [ 123 Đường ABC, Quận 1...   ] | |
| |                                | |
| | Giới tính:                     | |
| | (*) Nam  ( ) Nữ  ( ) Khác      | |
| |                                | |
| | Ngày sinh:                     | |
| | [ 1995-05-15              [C] ] | |
| |                                | |
| | [ Hủy bỏ ]                     | |
| | [ Lưu Thay Đổi ]               | |
| +--------------------------------+ |
+------------------------------------+
```

---

## 2. Thiết kế Hệ thống Màu sắc (HSL Design System Tokens)

Bảng màu CSS HSL được tối ưu hóa cho chế độ tối (Dark Mode) giúp giao diện trông sang trọng và giảm mỏi mắt cho người dùng:

```css
:root {
  /* Nền chủ đạo và các lớp Glassmorphism */
  --bg-main: HSL(222, 47%, 11%);      /* Xanh đen vũ trụ làm nền */
  --bg-glass: HSL(217, 33%, 17%, 0.7);/* Mờ kính với độ đục 70% */
  --bg-input: HSL(223, 47%, 7%);      /* Nền input tối sâu */
  
  /* Màu chữ */
  --text-primary: HSL(210, 40%, 98%);  /* Trắng ngà dễ đọc */
  --text-secondary: HSL(215, 20%, 65%);/* Xám xanh nhạt cho tiêu đề phụ */
  --text-muted: HSL(215, 15%, 45%);     /* Xám đậm cho trạng thái vô hiệu hóa */

  /* Màu sắc trạng thái và nút bấm */
  --color-primary: HSL(239, 84%, 67%);   /* Indigo neon */
  --color-primary-hover: HSL(245, 80%, 60%);
  --color-border: HSL(217, 30%, 25%);   /* Viền tinh tế */
  --color-focus: HSL(235, 100%, 75%);    /* Viền sáng khi active */
  
  /* Lỗi và Thành công */
  --color-error: HSL(0, 84%, 60%);      /* Đỏ neon cảnh báo */
  --color-success: HSL(142, 71%, 45%);  /* Xanh lá tươi thành công */
  
  /* Hiệu ứng */
  --backdrop-blur: blur(12px);
  --box-shadow-neon: 0 0 15px HSL(239, 84%, 67%, 0.3);
}
```

---

## 3. Hiệu ứng & Trải nghiệm Tương tác (Animations & Micro-interactions)

### A. Trạng thái ô nhập liệu (Input Fields Focus State)
Khi người dùng click chuột vào ô nhập liệu (`input:focus`):
*   Đường viền thay đổi từ `--color-border` sang `--color-focus`.
*   Tạo bóng viền phát sáng nhẹ: `box-shadow: 0 0 8px var(--color-focus);`.
*   Thời gian chuyển tiếp mượt mà: `transition: all 0.25s ease-in-out;`.

### B. Nút Lưu Thay Đổi (Save Button States)
*   **Trạng thái bình thường:** Hiển thị màu Gradient Indigo cá tính, chữ trắng.
*   **Trạng thái Hover:** Màu sắc chuyển hướng Gradient sáng dần và phóng to nhẹ (`transform: scale(1.02);`).
*   **Trạng thái Đang lưu (Loading/Updating):**
    *   Nút bị vô hiệu hóa (`pointer-events: none`).
    *   Hiển thị Spinner xoay tròn mượt mà thay thế cho text "Lưu Thay Đổi".
    *   Hiệu ứng mờ nhẹ nút bấm để báo hiệu hệ thống đang xử lý.
*   **Trạng thái Unsaved Changes (Dirty):** Nếu dữ liệu form chưa thay đổi so với dữ liệu gốc nhận về từ API, nút "Lưu" sẽ bị vô hiệu hóa (`opacity: 0.5; cursor: not-allowed`). Nút chỉ mở khi phát hiện form "bẩn" (Dirty).

### C. Validation Error Message display (Hiển thị Lỗi)
*   Thông báo lỗi xuất hiện ngay bên dưới ô nhập liệu với hiệu ứng trượt nhẹ từ trên xuống (slide-down) và mờ dần (fade-in) trong `150ms`.
*   Đồng thời viền của ô nhập liệu có lỗi chuyển sang màu đỏ `--color-error` để thu hút sự chú ý.
