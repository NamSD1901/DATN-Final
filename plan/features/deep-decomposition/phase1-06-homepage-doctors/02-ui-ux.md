# 🎨 UI & UX Specifications - Vets Team Cards (Vue 3)

Tài liệu này đặc tả chi tiết thiết kế giao diện danh sách bác sĩ thú y trực ca, sơ đồ cấu trúc trực quan (ASCII Mockup) và các hiệu ứng visual feedback tương tác trên thẻ bác sĩ.

---

## 1. Thiết kế Giao diện Đội ngũ Bác sĩ (Vets Grid Layout)

Khu vực đội ngũ bác sĩ sử dụng bộ lọc chuyên khoa dạng nút bấm hiện đại và hiển thị lưới thẻ chân dung bo góc:

```
+-----------------------------------------------------------------------------------+
|                                🏥 ĐỘI NGŨ BÁC SĨ THÚ Y                            |
|             Các chuyên gia y tế giàu kinh nghiệm, yêu thương động vật             |
|                                                                                   |
|  [ Tất cả ]    [ Nội khoa ]    [ Da liễu 🩺 ]    [ Phẫu thuật ]    [ Tiêm phòng ]  | (Tabs lọc)
|                                                                                   |
|  +--------------------+   +--------------------+   +--------------------+         |
|  | +----------------+ |   | +----------------+ |   | +----------------+ |         |
|  | | [ Chân dung ]  | |   | | [ Chân dung ]  | |   | | [ Chân dung ]  | |         |
|  | +----------------+ |   | +----------------+ |   | +----------------+ |         |
|  | ThS. BS. Minh      |   | ThS. BS. Vy        |   | BS. Tuấn           |         | (Dạng lưới Cards)
|  | Chuyên khoa: DaLiễu|   | Chuyên khoa:NộiKhoa|   | Chuyên khoa: Ngoại |         |
|  | Kinh nghiệm: 8 năm |   | Kinh nghiệm: 6 năm |   | Kinh nghiệm: 10 năm|         |
|  | [🟢 Đang trực ca]  |   | [🟢 Đang trực ca]  |   | [🔴 Nghỉ phép]     |         | (Status Badge)
|  |   [⚡ Đặt Lịch ]   |   |   [⚡ Đặt Lịch ]   |   | [🔒 Không khả dụng]|         |
|  +--------------------+   +--------------------+   +--------------------+         |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

### 1.1. Cấu trúc Card Bác sĩ (Vet Card Detail)
Thẻ bác sĩ được thiết kế theo phong cách tối giản nhưng cao cấp:
*   **Ảnh chân dung:** Đặt trong khung hình tròn hoặc bo góc lớn (`border-radius: 50%` hoặc `16px`) nằm phía trên cùng.
*   **Nhãn trạng thái trực ca (Duty Badge):**
    *   *Đang trực ca:* Hiển thị một chấm tròn nhấp nháy xanh lục (`#10B981`) kèm chữ *"Đang trực ca"*.
    *   *Nghỉ phép:* Hiển thị chấm tròn đỏ hồng (`#EF4444`) kèm chữ *"Nghỉ phép"*. Nút **Đặt lịch** với bác sĩ này sẽ tự động bị khóa (disabled).

---

## 2. Trạng thái Tương tác & Hiệu ứng Thị giác (Visual Cues)

*   **Hiệu ứng Hover ảnh chân dung (Portrait Zoom & Scale):**
    *   Khi rê chuột vào thẻ Card bác sĩ, ảnh chân dung tự động phóng to nhẹ 3%, đồng thời bộ lọc màu xám (grayscale) nếu có sẽ tự động chuyển sang màu sắc rực rỡ trong vòng **300ms** (`transition: filter 0.3s ease, transform 0.3s ease`).
    *   Nút "Đặt lịch" trượt nhẹ từ dưới lên và đổi màu nền sang Teal Neon sáng.
*   **Trạng thái loading (Skeleton Vets Card):**
    *   Trong quá trình gọi API, hiển thị 3 thẻ xương với hình tròn giả lập ảnh đại diện và các đường kẻ ngang giả lập chữ với hiệu ứng Shimmer chạy qua liên tục.

---

## 3. Mã nguồn CSS Badge Nhấp nháy (Pulse Animation)

Tạo sự chú ý cho trạng thái đang trực ca của bác sĩ:

```css
@keyframes pulse {
  0% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.7);
  }
  70% {
    transform: scale(1);
    box-shadow: 0 0 0 6px rgba(16, 185, 129, 0);
  }
  100% {
    transform: scale(0.95);
    box-shadow: 0 0 0 0 rgba(16, 185, 129, 0);
  }
}

.pulse-badge {
  display: inline-block;
  width: 8px;
  height: 8px;
  background-color: #10B981;
  border-radius: 50%;
  animation: pulse 2s infinite;
}
```
