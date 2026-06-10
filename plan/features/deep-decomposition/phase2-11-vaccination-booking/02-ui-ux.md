# 🎨 UI/UX Design Spec - Online Vaccination Booking

## 1. Bố cục Giao diện & Trực quan hóa (Responsive Layout)

Giao diện đặt lịch tiêm chủng trực tuyến sử dụng cấu trúc Wizard Form đa bước, được tối ưu hóa hiển thị bảng lựa chọn vắc-xin và cảnh báo khoảng cách y tế.

### A. Sơ đồ ASCII Mockup - Bước chọn Vắc-xin & Cảnh báo (Step 2.5 Layout)
```text
+-----------------------------------------------------------------------------------------------+
|  ĐẶT LỊCH TIÊM PHÒNG VẮC-XIN                                                              [X] |
+-----------------------------------------------------------------------------------------------+
|  [Tiến trình: (1) Thú Cưng ==> (*) Chọn Vắc-xin ==> (3) Giờ khám ==> (4) Xác Nhận]            |
|                                                                                               |
|  +-----------------------------------------------------------------------------------------+  |
|  | CHỌN LOẠI VẮC-XIN PHÙ HỢP CHO: BÉ MIMI (MÈO)                                            |  |
|  |                                                                                         |  |
|  |  +----------------------------+   +----------------------------+   +------------------+ |  |
|  |  | [VACCINE CARD - SELECTION] |   | [VACCINE CARD - WARNING]   |   | [VACCINE CARD]   | |  |
|  |  |                            |   |                            |   |                  | |  |
|  |  | Name: Feline 4-in-1        |   | Name: Rabies (Phòng dại)   |   | Name: Leucogen   | |  |
|  |  | Mũi tiêm: Nhắc lại hàng năm|   | Mũi tiêm: Mũi dại thường   |   | Mũi tiêm: Bạch cầu| |  |
|  |  | Xuất xứ: Pháp (Merial)     |   | Xuất xứ: Mỹ (Zoetis)       |   | Xuất xứ: Pháp    | |  |
|  |  | Trạng thái: [ Còn Hàng ]   |   | Trạng thái: [ Tiêm Sớm ⚠️] |   | Trạng thái: [Hết]| |  |
|  |  |                            |   |                            |   |                  | |  |
|  |  | [ CHỌN LOẠI NÀY ]          |   | [ CHỌN LOẠI NÀY ]          |   | [ HẾT HÀNG ](dis)| |  |
|  |  +----------------------------+   +----------------------------+   +------------------+ |  |
|  |                                                                                         |  |
|  |  [⚠️ CẢNH BÁO Y TẾ]: Bé Mimi đã tiêm vaccine Dại gần nhất vào ngày 10/01/2026.           |  |
|  |  Thời gian tiêm nhắc khuyến nghị tiếp theo là sau ngày 10/12/2026 (còn 6 tháng nữa).    |  |
|  |  [ ] Tôi đã hiểu và muốn tiếp tục đặt lịch (Cần chỉ định đặc biệt từ Bác sĩ).           |  |
|  +-----------------------------------------------------------------------------------------+  |
|                                                                                               |
|  [ Quay lại ]                                                                  [ Tiếp tục ]   |
+-----------------------------------------------------------------------------------------------+
```

---

## 2. Thiết kế Hệ thống Màu sắc & Trạng thái CSS (HSL Variables)

Bảng màu HSL phục vụ thiết kế Glassmorphism và các cảnh báo phác đồ tiêm chủng:

```css
:root {
  /* Khối mờ kính */
  --vax-glass-bg: HSL(217, 33%, 17%, 0.7);
  --vax-border: HSL(217, 30%, 25%);
  
  /* Trạng thái Thẻ Vắc-xin */
  --vax-card-bg: HSL(223, 47%, 10%, 0.5);
  --vax-card-border: HSL(217, 20%, 30%);
  
  /* Vắc-xin còn hàng */
  --vax-in-stock: HSL(142, 71%, 45%);
  --vax-in-stock-bg: HSL(142, 71%, 10%, 0.3);
  
  /* Vắc-xin hết hàng */
  --vax-out-stock: HSL(0, 0%, 50%);
  --vax-out-stock-bg: HSL(0, 0%, 15%, 0.4);
  
  /* Cảnh báo tiêm chủng quá sớm */
  --vax-warning-bg: HSL(38, 92%, 50%, 0.15);      /* Màu vàng cam nhạt */
  --vax-warning-border: HSL(38, 92%, 55%);
  --vax-warning-text: HSL(38, 92%, 75%);
}

/* Thẻ cảnh báo phác đồ tiêm */
.vax-warning-box {
  background-color: var(--vax-warning-bg);
  border: 1px solid var(--vax-warning-border);
  color: var(--vax-warning-text);
  border-radius: 10px;
  padding: 15px;
  margin-top: 15px;
  backdrop-filter: blur(8px);
  animation: slide-down-fade 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slide-down-fade {
  0% { transform: translateY(-10px); opacity: 0; }
  100% { transform: translateY(0); opacity: 1; }
}
```

---

## 3. Hoạt ảnh & Trải nghiệm Tương tác (UX/UI Animations)

### A. Lọc động theo loài (Species Filter Transition)
*   Khi người dùng chọn thú cưng ở Bước 1 (Ví dụ: bé mèo):
    *   Hệ thống chuyển sang Bước 2, danh sách vắc-xin cho Chó sẽ co lại và biến mất bằng hiệu ứng thu nhỏ (`transform: scale(0.8); opacity: 0;`).
    *   Các thẻ vắc-xin cho Mèo lập tức giãn ra và hiển thị mượt mà.
*   Điều này giúp giao diện không bị giật cục và định hướng người dùng tập trung chọn đúng loại thuốc.

### B. Hộp kiểm xác nhận tiêm sớm (Forced Override Checkbox)
*   Nếu vắc-xin bị cảnh báo tiêm sớm:
    *   Nút **Tiếp tục** lập tức bị disable.
    *   Hộp kiểm `[ ] Tôi đã hiểu và muốn tiếp tục đặt lịch` hiển thị nhấp nháy chậm viền vàng.
    *   Chỉ khi người dùng tích chọn hộp kiểm này để xác nhận chịu trách nhiệm thông tin, nút **Tiếp tục** mới mở khóa trở lại.
*   Hiệu ứng đập mạch (pulse) màu vàng cảnh báo sẽ dừng khi hộp kiểm được tích chọn.
