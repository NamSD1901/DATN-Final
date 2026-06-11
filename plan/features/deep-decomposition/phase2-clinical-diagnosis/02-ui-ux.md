# 🎨 UI/UX Design Specification - Clinical Diagnosis & Treatment

## 1. Bản vẽ Thiết kế Giao diện Bác sĩ (ASCII Mockups)

### A. Giao diện Không gian khám bệnh chính (Doctor's Workspace - 3-Pane Layout)

```text
+---------------------------------------------------------------------------------------------------+
|  [Logo Clinic]  Bảng Hàng Đợi    [PHÒNG KHÁM LÂM SÀNG]    Quản Lý Kho Dược     (BS. Trần Quốc Anh) |
+---------------------------------------------------------------------------------------------------+
|                                                                                                   |
|  +-----------------------+  +-------------------------------------+  +--------------------------+ |
|  | HÀNG CHỜ KHÁM     (3) |  |🩺 CA KHÁM HIỆN TẠI: 🐶 Bé Leo (Dog) |  | BỆNH SỬ Y KHOA           | |
|  +-----------------------+  +-------------------------------------+  +--------------------------+ |
|  | Q-010 | Bé Leo   [>]  |  | Triệu chứng lâm sàng:               |  | * 10/05/2026:            | |
|  | (🐶 Chó Poodle)       |  | [ Chó bỏ ăn, nôn dịch vàng lỏng   ] |  |   Chẩn đoán: Tiêu chảy   | |
|  |                       |  |                                     |  |   Đơn: Amoxicillin (5)   | |
|  | Q-011 | Bé MiuMiu     |  | Chẩn đoán bệnh lý:                  |  |                          | |
|  | (🐱 Mèo Ta)           |  | [ Viêm dạ dày cấp tính            ] |  | * 01/04/2026:            | |
|  |                       |  |                                     |  |   Tiêm phòng Dại mũi 1   | |
|  | Q-012 | Bé Miu        |  | 💊 ĐƠN THUỐC ĐIỀU TRỊ               |  |   BS. Nguyễn Đức         | |
|  | (🐱 Mèo Ba Tư)        |  | +---------------------------------+ |  |                          | |
|  |                       |  | | Thuốc: Amoxicillin    |SL: [ 5 ]| |  | * 15/02/2026:            | |
|  |                       |  | | HDSD: Uống 2 lần/ngày, sau ăn   | |  |   Dị ứng: Không          | |
|  |                       |  | +---------------------------------+ |  |                          | |
|  |                       |  | | Thuốc: [ Nhập tên thuốc... ]  | | |  |                          | |
|  |                       |  | +---------------------------------+ |  |                          | |
|  |                       |  |                                     |  |                          | |
|  |                       |  | [ IN ĐƠN THUỐC ] [ HOÀN THÀNH KHÁM ]|  |                          | |
|  +-----------------------+  +-------------------------------------+  +--------------------------+ |
+---------------------------------------------------------------------------------------------------+
```

### B. Khung Autocomplete gợi ý thuốc & Cảnh báo tồn kho (Medicine Autocomplete Dropdown)

```text
  Thuốc: [ Amox_                      ]
         +--------------------------------------------+
         | Amoxicillin 250mg    | Tồn: 140 liều       |  <-- Nhấn Enter chọn
         | Amoxicillin 500mg    | Tồn: 8 liều [Sắp hết]|
         | Amoxicillin (Siro)   | Tồn: 0 liều [Hết]    |  <-- Disabled, chữ xám mờ
         +--------------------------------------------+
```

---

## 2. Hệ thống CSS Variable & Token Y khoa (HSL Color Tokens)

Sử dụng bảng màu ngọc lam chủ đạo (`--medical-primary`) tạo cảm giác y khoa sạch sẽ, giảm mỏi mắt cho bác sĩ khi ngồi phòng khám liên tục:

```css
:root {
  /* Bảng màu Y khoa */
  --medical-primary: hsl(190, 90%, 40%);        /* Ngọc lam sẫm */
  --medical-primary-hover: hsl(190, 90%, 35%);
  --medical-primary-light: hsl(190, 90%, 95%);
  
  --medical-accent: hsl(160, 80%, 40%);         /* Xanh bạc hà */
  --medical-bg: hsl(190, 30%, 98%);
  
  /* Cảnh báo kho dược */
  --stock-instock-bg: hsl(145, 80%, 96%);
  --stock-instock-text: hsl(145, 80%, 30%);
  
  --stock-low-bg: hsl(45, 100%, 95%);
  --stock-low-text: hsl(45, 100%, 35%);
  
  --stock-empty-bg: hsl(0, 100%, 96%);
  --stock-empty-text: hsl(0, 100%, 40%);

  /* Glassmorphism Doctor Panels */
  --panel-bg: rgba(255, 255, 255, 0.7);
  --panel-border: rgba(190, 220, 230, 0.3);
  --panel-shadow: 0 10px 40px -10px rgba(15, 23, 42, 0.05);
}
```

---

## 3. Hoạt ảnh Tương tác Vi mô (Micro-animations)

*   **Hiệu ứng Thêm dòng Thuốc (Slide-in-down Prescription Line):**
    Khi bác sĩ chọn thuốc thành công từ dropdown, dòng thuốc mới được thêm vào đơn sẽ trượt xuống từ vị trí input tìm kiếm và mờ dần ra (Opacity từ 0 sang 1) với độ trễ chuyển động tự nhiên.
    ```css
    .prescription-item-enter-active {
      animation: slide-in-down 0.3s cubic-bezier(0.16, 1, 0.3, 1);
    }
    @keyframes slide-in-down {
      0% { opacity: 0; transform: translateY(-10px); }
      100% { opacity: 1; transform: translateY(0); }
    }
    ```
*   **Hiệu ứng Cảnh báo Vượt quá Tồn kho (Pulse Red Glow):**
    Nếu bác sĩ nhập số lượng thuốc kê đơn lớn hơn số lượng tồn kho khả dụng của loại thuốc đó:
    * Viền của ô số lượng (`input[type="number"]`) nhấp nháy ánh đỏ neon.
    * Hiển thị chữ cảnh báo nhỏ màu đỏ run rẩy nhẹ bên dưới ô nhập.
    ```css
    .stock-error-border {
      border-color: #EF4444 !important;
      box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.2);
      animation: border-glow-red 1.5s infinite alternate;
    }
    @keyframes border-glow-red {
      0% { box-shadow: 0 0 0 1px rgba(239, 68, 68, 0.2); }
      100% { box-shadow: 0 0 0 4px rgba(239, 68, 68, 0.4); }
    }
    ```

---

## 4. Thiết kế Thích ứng (Responsive Design Rules)

*   **Màn hình Desktop lớn (>= 1280px - PC phòng khám):** Hiển thị đầy đủ bố cục 3 cột (3-pane layout) song song trên 1 trang để bác sĩ nhìn tổng thể không phải click qua lại: Cột trái (Hàng chờ khám - 20%), Cột giữa (Nội dung khám & kê đơn - 50%), Cột phải (Timeline bệnh sử cũ - 30%).
*   **Màn hình Laptop & Laptops (< 1280px):** Ẩn cột bệnh sử bên phải. Thay thế bằng nút bấm "Bệnh sử của bé" nổi bật. Khi click sẽ hiển thị một Slide-out Panel (Drawer) trượt từ mép phải màn hình đè lên nội dung chính để bác sĩ đối chiếu nhanh.
*   **Màn hình Máy tính bảng (Tablet - Bác sĩ cầm tay đi buồng):** Ẩn hàng chờ bên trái và bệnh sử bên phải thành các menu trượt. Màn hình chính tập trung 100% chiều rộng cho Form ghi nhận chẩn đoán và Grid kê đơn thuốc với kích thước các nút bấm to dễ nhấn bằng ngón tay.
