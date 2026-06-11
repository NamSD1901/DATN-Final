# 🎨 UI/UX Design Specification - Admin Revenue Reports

Tài liệu thiết kế giao diện người dùng, sơ đồ bố cục ASCII Dashboard, mã màu HSL đồ họa biểu đồ Chart.js và các hiệu ứng phản hồi tooltip trực quan.

---

## 1. Bản vẽ Bố cục giao diện (ASCII Art Mockups)

### Giao diện Dashboard Báo cáo Doanh thu & Hiệu suất (Admin Reports Workspace)
Giao diện được thiết kế theo phong cách Glassmorphism mờ kính sang trọng, hiển thị các thẻ KPIs nổi bật ở đầu và hệ thống biểu đồ so sánh ở dưới.

```text
+-------------------------------------------------------------------------------------------------------------------+
|  [Logo] MYPETCLINIC - BÁO CÁO DOANH THU & HIỆU SUẤT (REPORTS DASHBOARD)               [NV: Khánh] [Đăng xuất]     |
+-------------------------------------------------------------------------------------------------------------------+
|  Khoảng ngày lọc: [ 01/05/2026 ] đến [ 31/05/2026 ]   ( Nhanh: [Tháng này]  [Tháng trước]  [Năm nay] ) [XUẤT EXCEL] |
+-------------------------------------------------------------------------------------------------------------------+
|  THẺ CHỈ SỐ KPI CHÍNH (KEY PERFORMANCE INDICATORS)                                                                |
|  +---------------------------+  +---------------------------+  +---------------------------+                      |
|  | TỔNG DOANH THU            |  | TỔNG SỐ LỊCH HẸN          |  | GIÁ TRỊ TRUNG BÌNH ĐƠN    |                      |
|  | 275.000.000 đ             |  | 420 ca khám               |  | 654.000 đ                 |                      |
|  | [ +12.4% so với tháng tr. ] |  | [ +5.8% so với tháng tr. ]  |  | [ +6.2% so với tháng tr. ] |                      |
|  +---------------------------+  +---------------------------+  +---------------------------+                      |
+-------------------------------------------------------------------------------------------------------------------+
|  BIỂU ĐỒ XU HƯỚNG DOANH THU THEO NGÀY (Line Chart) |  CƠ CẤU NGUỒN THU PHÒNG KHÁM (Donut Chart)                   |
|  Doanh thu (Triệu đ)                                |                                                             |
|   30 |         *                                    |             +---------+                                     |
|   20 |       *   *       *                          |          *  *  Dịch   *  *                                  |
|   10 |     *       *   *   *                        |        *     vụ khám     *                                  |
|    0 +----------------------------                  |       *    (150.000k)     *                                 |
|      01  05  10  15  20  25  31 (Ngày)              |       *   Vắc-xin   Thuốc *                                 |
|                                                     |        * (40.000k)(85.000k) *                               |
|                                                     |          *  *  *  *  *  *  *                                 |
|                                                     |             +---------+                                     |
+-------------------------------------------------------------------------------------------------------------------+
|  TOP 5 BÁC SĨ ĐIỀU TRỊ HIỆU SUẤT CAO               |  TOP 5 DỊCH VỤ ĐEM LẠI DOANH THU CAO                         |
|  1. Bác sĩ Đỗ Quốc Huy.......... 120 ca (85.000k)   |  1. Phí khám lâm sàng........... 420 lượt (42.000k)          |
|  2. Bác sĩ Nguyễn Văn Minh...... 110 ca (72.000k)   |  2. Tiêm phòng dại.............. 150 lượt (30.000k)          |
|  3. Bác sĩ Lê Thị Mai............ 95 ca (60.000k)   |  3. Phẫu thuật triệt sản........  15 lượt (22.500k)          |
+-------------------------------------------------------------------------------------------------------------------+
```

---

## 2. Hệ thống CSS Design Tokens (HSL Color Theme)

Mã màu HSL cho các đường biểu diễn dữ liệu của biểu đồ:

```css
:root {
  /* Màu biểu diễn biểu đồ Chart.js */
  --chart-line-stroke: hsl(200, 90%, 55%);    /* Màu đường doanh thu chính (Xanh lam neon) */
  --chart-line-fill: hsla(200, 90%, 55%, 0.1);  /* Màu vùng đổ bóng bên dưới */
  
  --chart-bar-color: hsl(175, 70%, 45%);       /* Màu cột dịch vụ (Ngọc bích) */
  
  --chart-donut-service: hsl(200, 90%, 55%);   /* Doanh thu dịch vụ (Xanh lam) */
  --chart-donut-medicine: hsl(45, 95%, 50%);   /* Doanh thu thuốc (Vàng) */
  --chart-donut-vaccine: hsl(280, 75%, 60%);   /* Doanh thu vắc-xin (Tím) */

  /* Màu chỉ số tăng trưởng */
  --growth-positive: hsl(145, 65%, 40%);     /* Xanh lá - Tăng trưởng dương */
  --growth-positive-bg: hsla(145, 65%, 45%, 0.1);
  
  --growth-negative: hsl(355, 75%, 45%);     /* Đỏ - Suy giảm */
  --growth-negative-bg: hsla(355, 75%, 45%, 0.1);

  --glass-card-bg: rgba(22, 30, 49, 0.45);
  --glass-card-border: rgba(255, 255, 255, 0.06);
}
```

---

## 3. Thiết kế Tooltip Hiển thị Biểu đồ (Hover Card Interactions)

Khi người dùng di chuột qua các cột hoặc các nút trên biểu đồ, Chart.js sẽ kích hoạt hiển thị một Custom Tooltip được custom CSS mờ kính:

```css
.chartjs-custom-tooltip {
  opacity: 0;
  position: absolute;
  background: var(--glass-card-bg);
  border: 1px solid var(--glass-card-border);
  backdrop-filter: blur(8px);
  border-radius: 6px;
  color: hsl(210, 20%, 98%);
  padding: 8px 12px;
  font-family: 'Outfit', sans-serif;
  font-size: 11px;
  pointer-events: none;
  transition: all 0.15s ease;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
}

.chartjs-tooltip-title {
  font-weight: bold;
  margin-bottom: 4px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  padding-bottom: 2px;
}

.chartjs-tooltip-value {
  display: flex;
  align-items: center;
  gap: 6px;
}
```

---

## 4. Đặc tả Layout Bảng Xếp hạng (Responsive CSS Grid)

```css
.rankings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.ranking-card {
  background: var(--glass-card-bg);
  border: 1px solid var(--glass-card-border);
  backdrop-filter: var(--backdrop-blur);
  border-radius: 12px;
  padding: 20px;
  box-shadow: var(--card-shadow);
}

.ranking-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.ranking-item:last-child {
  border-bottom: none;
}
```
