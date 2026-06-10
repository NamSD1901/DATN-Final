# 🎨 UI/UX Design Specification - Receptionist Portal & Queue Management

## 1. Bản vẽ Thiết kế Giao diện (ASCII Mockups)

### A. Giao diện Dashboard Quản trị Hàng đợi Lễ tân (Receptionist Dashboard)

```text
+---------------------------------------------------------------------------------------------------+
|  [Logo Clinic]  [DUYỆT LỊCH HẸN (3)]    [BẢNG HÀNG ĐỢI (KANBAN)]    [ĐĂNG KÝ WALK-IN]     (Lễ tân) |
+---------------------------------------------------------------------------------------------------+
|  QUẦY TIẾP ĐÓN                                                                                    |
|  [ Nhập SĐT / Tên hoặc quét QR Code để check-in...                     ]  [ QUÉT MÃ QR ] [ TIẾP NHẬN] |
|                                                                                                   |
|  KANBAN ĐIỀU PHỐI HÀNG ĐỢI KHÁM                                                                   |
|  +---------------------------+   +---------------------------+   +---------------------------+    |
|  | CHỜ KHÁM (WAITING)   (3)  |   | ĐANG KHÁM (IN PROG)  (2)  |   | HOÀN TẤT / CHỜ THANH TOÁN |    |
|  +---------------------------+   +---------------------------+   +---------------------------+    |
|  | [Q-012] Bé Miu (Cat)      |   | [Q-010] Bé Leo (Dog)      |   | [Q-009] Bé Milo (Dog)     |    |
|  | Dịch vụ: Tiêm phòng 4 bệnh|   | Phòng: Phòng khám 101     |   | Chẩn đoán: Viêm da dị ứng |    |
|  | Chờ: 12 phút (Bác sĩ Anh) |   | Bác sĩ: BS. Trần Quốc Anh |   | Bác sĩ: BS. Nguyễn Đức    |    |
|  | [ Chỉ định ]  [ Bắt đầu ] |   | [ Chuyển khám ] [ Xong ]  |   | [ Lập Hóa Đơn ]           |    |
|  |                           |   |                           |   |                           |    |
|  | [Q-013] Bé Bông (Dog) [!] |   | [Q-011] Bé MiuMiu (Cat)   |   | [Q-008] Bé Vàng (Dog)     |    |
|  | Dịch vụ: Khám tổng quát   |   | Phòng: Phòng khám 102     |   | Dịch vụ: Tiêm phòng dại   |    |
|  | Chờ: 35 phút (Bác sĩ Đức) |   | Bác sĩ: BS. Lê Thị Mai    |   | Bác sĩ: BS. Trần Quốc Anh |    |
|  |                           |   |                           |   |                           |    |
|  +---------------------------+   +---------------------------+   +---------------------------+    |
+---------------------------------------------------------------------------------------------------+
```

### B. Màn hình Tivi Sảnh chờ Công cộng (Public TV Queue Board)

```text
=====================================================================================================
                                  HÀNG ĐỢI KHÁM BỆNH - MYPETCLINIC
=====================================================================================================
  [ SỐ THỨ TỰ ]      [ TÊN BÉ CƯNG ]      [ LOÀI ]          [ PHÒNG KHÁM ]          [ TRẠNG THÁI ]  
-----------------------------------------------------------------------------------------------------
      Q-010              Bé Leo             Chó             Phòng khám 101           ĐANG KHÁM      
      Q-011              Bé MiuMiu          Mèo             Phòng khám 102           ĐANG KHÁM      
-----------------------------------------------------------------------------------------------------
      Q-012              Bé Miu             Mèo             Phòng khám 101           ĐANG CHỜ       
      Q-013              Bé Bông            Chó             Phòng khám 102           ĐANG CHỜ       
      Q-014              Bé LuLu            Chó             Phòng khám 103           ĐANG CHỜ       
=====================================================================================================
```

---

## 2. Hệ thống CSS Variable & Token Hàng đợi (HSL Color Tokens)

Bảng màu tương phản cao, đặc trưng cho từng khu vực Kanban và hỗ trợ hiển thị trên Tivi sảnh chờ ở khoảng cách xa:

```css
:root {
  /* Kanban Column Colors */
  --kanban-waiting-bg: hsl(210, 100%, 98%);
  --kanban-waiting-border: hsl(210, 100%, 85%);
  --kanban-waiting-text: hsl(210, 100%, 35%);

  --kanban-inprogress-bg: hsl(280, 100%, 98%);
  --kanban-inprogress-border: hsl(280, 100%, 88%);
  --kanban-inprogress-text: hsl(280, 100%, 40%);

  --kanban-completed-bg: hsl(145, 100%, 97%);
  --kanban-completed-border: hsl(145, 100%, 80%);
  --kanban-completed-text: hsl(145, 100%, 30%);

  /* Cảnh báo đợi quá lâu (> 30 phút) */
  --alert-critical-bg: hsl(0, 100%, 96%);
  --alert-critical-text: hsl(0, 100%, 35%);
  --alert-critical-border: hsl(0, 100%, 80%);

  /* Glassmorphism Kanban Card */
  --card-bg: rgba(255, 255, 255, 0.65);
  --card-border: rgba(255, 255, 255, 0.4);
  --card-shadow: 0 4px 16px 0 rgba(31, 38, 135, 0.05);
}
```

---

## 3. Hoạt ảnh tương tác Vi mô (Micro-animations)

*   **Hiệu ứng Kéo thả (Drag & Drop Shimmer):**
    Khi lễ tân kéo một thẻ thú cưng di chuyển trên bảng Kanban, các khu vực thả khả dụng (Drop zones) sẽ phát sáng nhẹ viền nét đứt. Thẻ đang kéo sẽ chuyển sang trạng thái mờ kính 50% (`opacity: 0.5`) và nghiêng nhẹ 2 độ để tạo cảm giác vật lý chân thực.
    ```css
    .kanban-card-dragging {
      opacity: 0.5;
      transform: scale(1.02) rotate(2deg);
      cursor: grabbing;
    }
    ```
*   **Đèn nhấp nháy Cảnh báo sảnh chờ (Critical Pulse Badge):**
    Đối với các ca xếp hàng chờ quá 30 phút mà chưa được gọi khám:
    *   Thẻ card chuyển sang viền màu đỏ `--alert-critical-border`.
    *   Có chấm tròn màu đỏ nhấp nháy ở cạnh số thứ tự để thu hút sự chú ý của lễ tân ưu tiên gọi ca này trước.
    ```css
    @keyframes pulse-red {
      0% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7); }
      70% { box-shadow: 0 0 0 8px rgba(239, 68, 68, 0); }
      100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
    }
    .critical-pulse {
      width: 10px;
      height: 10px;
      background-color: #EF4444;
      border-radius: 50%;
      animation: pulse-red 1.5s infinite;
    }
    ```

---

## 4. Thiết kế Thích ứng (Responsive Design Rules)

*   **Desktop & Large Monitors (>= 1200px):** Kanban 3 cột dàn hàng ngang đầy đủ, hiển thị toàn bộ thông tin chi tiết trên thẻ card bao gồm: Tên pet, SĐT chủ, Dịch vụ chỉ định, Bác sĩ khám, và Thời gian đã chờ.
*   **Tablet & Laptops (< 1200px):** Kanban tự động chuyển sang dạng cuộn ngang (Horizontal scroll), hoặc cho phép chuyển đổi chế độ xem qua bộ chọn Tabs ở trên: `[ Chờ khám ] | [ Đang khám ] | [ Đã khám ]` để tối ưu diện tích hiển thị trên máy tính bảng của lễ tân.
*   **Màn hình Public TV sảnh chờ (Public TV Layout):**
    *   Khóa cuộn trang (No scrolling).
    *   Tăng kích thước font chữ lên gấp đôi (Ví dụ: `font-size: 2.5rem` cho số thứ tự).
    *   Sử dụng màu nền tương phản cực cao (Dark mode mờ kính trên nền ảnh hoạt họa nhẹ nhàng) để chủ nuôi có thể đọc rõ số từ khoảng cách 10 - 15 mét tại sảnh.
