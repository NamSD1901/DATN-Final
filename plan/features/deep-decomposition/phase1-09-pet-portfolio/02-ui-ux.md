# 🎨 UI/UX Design Spec - Pet Portfolio Management

## 1. Bố cục Giao diện & Trực quan hóa (Responsive Layout)

Giao diện danh sách thú cưng được hiển thị dưới dạng lưới thẻ (Grid Cards) mờ kính Glassmorphism sang trọng trên nền tối chủ đạo của ứng dụng MyPetClinic.

### A. Sơ đồ ASCII Mockup - Danh sách Thú Cưng (Pet Grid Layout)
```text
+-----------------------------------------------------------------------------------------------+
|  [Sidebar]  |  MY PETS PORTFOLIO (HỒ SƠ THÚ CƯNG CỦA TÔI)                 [+ Thêm Thú Cưng]   |
|             |                                                                                 |
|  * Profile  |  +----------------------------+   +----------------------------+                |
|  > My Pets  |  | [PET CARD - CARD 1]        |   | [PET CARD - CARD 2]        |                |
|  * Bookings |  |   /---------\  Tên: BÉ LEO |   |   /---------\  Tên: BÉ MIMI|                |
|             |  |  /  (o.o)  \  Loài: Chó    |   |  /  (=^.^=) \  Loài: Mèo   |                |
|             |  |  |   Dog   |  Giống: Poodle|   |  |   Cat   |  Giống: Ba Tư|                |
|             |  |  \         /  Tuổi: 2 tuổi |   |  \         /  Tuổi: 8 thg |                |
|             |  |   \-------/   Nặng: 4.2 kg |   |   \-------/   Nặng: 3.1 kg |                |
|             |  |  ------------------------  |   |  ------------------------  |                |
|             |  |  [Chi tiết] [Sửa] [Xóa ❌] |   |  [Chi tiết] [Sửa] [Xóa ❌] |                |
|             |  +----------------------------+   +----------------------------+                |
|             |                                                                                 |
|             |  * Số lượng thú cưng hiện tại: 2 bé                                             |
+-----------------------------------------------------------------------------------------------+
```

### B. Sơ đồ ASCII Mockup - Form Thêm mới / Cập nhật Thú Cưng (Modal Form)
```text
+-----------------------------------------------------------------------------+
|   THÊM THÚ CƯNG MỚI                                                     [X] |
+-----------------------------------------------------------------------------+
|                                                                             |
|   Tên thú cưng (*):                Ngày sinh:                               |
|   [ Bé Leo                     ]   [ 15 / 05 / 2024               [Calendar]|
|                                                                             |
|   Loài (*):                        Giống loài (*):                          |
|   [ Mèo ] ( Đực / (*) Cái )        [ Ba Tư                         ]        |
|                                                                             |
|   Cân nặng (Kg):                   Nhóm máu (nếu có):                       |
|   [ 3.5                        ]   [ Nhóm máu A                    ]        |
|                                                                             |
|   Mã số Chip (Microchip):          Trạng thái triệt sản:                    |
|   [ MC-9876543210              ]   [ ] Đã triệt sản / thiến                 |
|                                                                             |
|   Tiền sử dị ứng & Ghi chú sức khỏe:                                        |
|   [ Dị ứng với hoạt chất Penicillin và thức ăn hạt nhiều đạm.             ] |
|                                                                             |
|   [ Hủy bỏ ]                                               [ Lưu Hồ Sơ ]    |
+-----------------------------------------------------------------------------+
```

---

## 2. Thiết kế Hệ thống Màu sắc & Trạng thái CSS (HSL Variables)

Thẻ thú cưng (Pet Card) được thiết kế có hiệu ứng tương tác cao để thu hút người dùng:

```css
:root {
  /* Khung thẻ Glassmorphism */
  --card-bg: HSL(217, 33%, 17%, 0.5);             /* Trong suốt 50% */
  --card-border: HSL(217, 30%, 25%);              /* Viền xám mờ */
  --card-hover-border: HSL(235, 100%, 75%, 0.4);  /* Viền xanh phát sáng neon khi hover */
  --card-shadow-hover: 0 10px 25px HSL(239, 84%, 67%, 0.15);
  
  /* Màu thẻ giới tính */
  --badge-male-bg: HSL(200, 80%, 20%, 0.3);
  --badge-male-text: HSL(200, 90%, 75%);
  --badge-female-bg: HSL(330, 80%, 20%, 0.3);
  --badge-female-text: HSL(330, 90%, 75%);
}

/* Kiểu dáng Thẻ Thú Cưng */
.pet-card {
  background: var(--card-bg);
  border: 1px solid var(--card-border);
  backdrop-filter: blur(12px);
  border-radius: 16px;
  padding: 20px;
  transition: all 0.35s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}

/* Hiệu ứng di chuột (Hover Effect) */
.pet-card:hover {
  transform: translateY(-6px) scale(1.02); /* Bay lên nhẹ + phóng to 2% */
  border-color: var(--card-hover-border);
  box-shadow: var(--card-shadow-hover);
}

/* Thẻ chỉ thị đực/cái */
.gender-badge.male {
  background-color: var(--badge-male-bg);
  color: var(--badge-male-text);
  border: 1px solid HSL(200, 80%, 40%);
}

.gender-badge.female {
  background-color: var(--badge-female-bg);
  color: var(--badge-female-text);
  border: 1px solid HSL(330, 80%, 40%);
}
```

---

## 3. Hoạt ảnh & Trải nghiệm Tương tác (UX/UI Animations)

### A. Hiệu ứng thêm mới thẻ (Grid Insert Animation)
Khi khách hàng thêm thành công một thú cưng mới, thẻ thú cưng mới sẽ xuất hiện ở đầu lưới Grid với hiệu ứng trượt nhẹ từ dưới lên (Slide-up) và mờ dần vào (Fade-in) để tạo sự mượt mà thay vì xuất hiện đột ngột:
```css
.pet-card-enter-active {
  animation: pet-card-in 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes pet-card-in {
  0% {
    opacity: 0;
    transform: translateY(20px) scale(0.95);
  }
  100% {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
```

### B. Client-side Form Validation Feedback
*   Trường **Tên thú cưng** và **Giống loài** nếu để trống khi di chuột ra ngoài sẽ lập tức chuyển viền đỏ và hiển thị text lỗi màu `--color-error` (Đỏ neon).
*   Nút **Lưu Hồ Sơ** chỉ sáng lên khi tất cả các trường bắt buộc đã được điền đầy đủ và hợp lệ.
