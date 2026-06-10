# 🎨 UI/UX Design Spec - Admin Revenue Reports

## 🔗 Skills Liên Quan
- **FE-F02 (CSS Variables):** Định nghĩa bảng màu doanh thu và biểu đồ trực quan:
  - `--revenue-line`: `#0EA5E9` (Sky Blue - Doanh thu theo ngày)
  - `--service-donut`: `#10B981` (Xanh lá - Dịch vụ)
  - `--medicine-donut`: `#F59E0B` (Vàng - Thuốc)
  - `--kpi-up-green`: `#10B981` (Tăng trưởng dương)
  - `--kpi-down-red`: `#EF4444` (Tăng trưởng âm)
- **FE-F01 (HTML Semantic):** Sử dụng các thẻ `<section>`, `<article>`, `<header>` kết hợp với thư viện canvas biểu đồ để phân hoạch không gian dashboard.

---

## 1. Giao diện Tổng quan Dashboard Doanh thu

### 1.1. Bộ lọc Khoảng ngày (Date Range Filter)
- Nằm góc trên cùng bên phải của dashboard.
- Hai ô input kiểu `date` (Từ ngày - Đến ngày) kèm nút "Áp dụng" (Apply) và các nút chọn nhanh: "7 ngày qua", "30 ngày qua", "Tháng này".

### 1.2. Thẻ chỉ số KPI (KPI Cards Grid)
Chia làm 3 thẻ xếp hàng ngang (hoặc 1 cột trên mobile):
1. **Tổng Doanh Thu (Total Revenue):**
   - Số lớn: Định dạng VNĐ (Ví dụ: `150,230,000 đ`).
   - Tỷ lệ tăng trưởng so với kỳ trước: Hiển thị kèm icon mũi tên lên/xuống màu tương ứng (`↑ +12.5%` dùng `--kpi-up-green` hoặc `↓ -3.2%` dùng `--kpi-down-red`).
2. **Tổng Số Lịch Hẹn (Total Appointments):**
   - Số lớn hiển thị tổng số cuộc hẹn hoàn thành trong khoảng thời gian đã chọn.
3. **Phân bổ Doanh thu (Revenue Breakdown):**
   - Hiển thị tỷ lệ doanh thu từ Dịch vụ (Service) vs. Bán Thuốc (Medicine) dạng thanh tiến trình (progress bar) ngang phân chia theo tỉ lệ.

### 1.3. Khu vực Biểu đồ (Charts Layout)
Bố cục chia làm 2 cột:
- **Cột Trái (70%): Line Chart - Doanh thu theo ngày:**
   - Trục hoành (X): Các ngày trong khoảng lọc.
   - Trục tung (Y): Doanh thu (VND).
   - Đường biểu đồ mượt (smooth curve) có vùng phủ mờ (gradient fill) màu `--revenue-line` với độ trong suốt (opacity) giảm dần.
- **Cột Phải (30%): Donut Chart - Cơ cấu doanh thu:**
   - Tỷ lệ phần trăm doanh thu giữa Dịch vụ và Thuốc.

### 1.4. Bảng Xếp Hạng (Top Performers Table)
Hiển thị danh sách Top 5 dịch vụ mang lại doanh thu cao nhất:
- Cột: Hạng | Tên dịch vụ | Số lượng bán | Tổng doanh thu (VND).
- Dòng xen kẽ (zebra striping) để dễ đọc thông tin.
