# 🚀 Product Requirements Document (PRD) - Admin Revenue Reports

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Phân hệ **Báo cáo & Thống kê Doanh thu (Admin Revenue Reports)** là công cụ đầu não hỗ trợ Quản trị viên (Admin) nắm bắt tức thời bức tranh tài chính và hiệu suất làm việc của toàn phòng khám. Bằng việc tổng hợp dữ liệu hóa đơn thực tế và lịch hẹn, phân hệ cung cấp các biểu đồ động trực quan (đường xu hướng doanh thu, cơ cấu dịch vụ, hiệu suất bác sĩ) giúp Admin đưa ra các quyết định nhân sự và kế hoạch đầu tư thiết bị y tế hợp lý.

Mục tiêu chính là số hóa hoàn toàn khâu đối soát tài chính, tự động hóa tính toán chỉ số tăng trưởng kinh doanh và mang lại trải nghiệm phân tích số liệu nhanh chóng.

---

## 2. Đối tượng sử dụng (Target Personas)

### 🧑‍💼 Quản trị viên phòng khám - Anh Khánh (38 tuổi)
- **Mô tả:** Khánh trực tiếp quản lý doanh số kinh doanh của phòng khám. Hàng tháng, anh cần báo cáo số liệu cho các cổ đông và đánh giá hiệu suất làm việc của các bác sĩ thú y để thưởng doanh số.
- **Nỗi đau (Pain points):**
  - Mất quá nhiều thời gian cộng sổ tay hóa đơn và tính doanh thu thuốc thủ công cuối tháng.
  - Không biết bác sĩ nào đang điều trị nhiều ca nhất hoặc dịch vụ spa/khám bệnh nào đang mang lại nguồn thu lớn nhất để đẩy mạnh quảng cáo.
  - Khó tính toán chính xác tỷ lệ tăng trưởng doanh thu so với tháng trước do công thức tính toán phức tạp.
- **Mong muốn:** Một Dashboard báo cáo tài chính hiển thị tức thời các chỉ số KPI, biểu đồ trực quan có thể lọc theo khoảng thời gian tùy chọn và hỗ trợ xuất báo cáo định dạng PDF/Excel.

---

## 3. User Stories & Tiêu chí Nghiệm thu (Acceptance Criteria)

### Story 1: Thẻ chỉ số tài chính tổng hợp nhanh (KPI Cards)
> **Là một** Quản trị viên phòng khám,  
> **Tôi muốn** xem nhanh các chỉ số tổng hợp gồm: Tổng doanh thu, tổng số ca khám thành công và tỷ lệ tăng trưởng so với kỳ trước ở đầu trang,  
> **Để** tôi có thể đánh giá nhanh sức khỏe tài chính phòng khám.

#### Tiêu chí Nghiệm thu (AC):
- **AC 1.1:** Dashboard hiển thị 3 thẻ KPI chính:
  - **Tổng doanh thu thực tế:** Tổng tiền từ các hóa đơn đã thanh toán (`Status = 'Paid'`) trong khoảng thời gian lọc.
  - **Số ca khám thành công:** Tổng số lịch hẹn đã hoàn thành khám y khoa hoặc tiêm phòng.
  - **Doanh thu trung bình mỗi ca khám (Average Order Value):** Tính bằng `Tổng doanh thu / Tổng số hóa đơn đã thanh toán`.
- **AC 1.2:** Mỗi thẻ KPI phải hiển thị tỷ lệ phần trăm tăng trưởng (+/- %) so với khoảng thời gian tương đương của kỳ trước (ví dụ: So sánh doanh thu tháng này với tháng trước đó).
- **AC 1.3:** Tỷ lệ tăng trưởng phải có màu sắc tương ứng: Màu xanh lá cho tăng trưởng dương (+), màu đỏ cho tăng trưởng âm (-).

### Story 2: Biểu đồ xu hướng và cơ cấu doanh thu động (Dynamic Charts)
> **Là một** Quản trị viên phòng khám,  
> **Tôi muốn** xem biểu đồ xu hướng doanh thu theo ngày và biểu đồ tròn thể hiện cơ cấu nguồn thu (Khám dịch vụ vs. Bán thuốc),  
> **Để** tôi hiểu rõ hành vi tiêu dùng của khách hàng và lập kế hoạch nhập kho thuốc.

#### Tiêu chí Nghiệm thu (AC):
- **AC 2.1:** Hiển thị biểu đồ đường (Line Chart) mô tả sự biến động doanh thu theo từng ngày/tháng trong khoảng thời gian lọc. Khi rê chuột vào từng điểm nút trên biểu đồ, hệ thống phải hiển thị tooltip thông tin số tiền chính xác.
- **AC 2.2:** Hiển thị biểu đồ tròn/donut (Donut Chart) phân tích tỷ trọng doanh thu của 3 nhóm: Phí dịch vụ khám, Tiền bán thuốc kê đơn, và Tiền tiêm chủng vắc-xin.
- **AC 2.3:** Dữ liệu biểu đồ tự động tải lại và dựng hình vẽ mới mượt mà (dưới 300ms) mỗi khi Admin thay đổi khoảng thời gian lọc (Date Range Picker).

### Story 3: Bảng xếp hạng Top Dịch vụ & Hiệu suất Bác sĩ
> **Là một** Quản trị viên phòng khám,  
> **Tôi muốn** xem danh sách Top 5 dịch vụ mang lại doanh thu cao nhất và Top 5 bác sĩ điều trị nhiều ca khám nhất,  
> **Để** tôi đưa ra các chiến lược tối ưu hóa nguồn lực nhân sự.

#### Tiêu chí Nghiệm thu (AC):
- **AC 3.1:** Hiển thị bảng Top 5 dịch vụ được đặt lịch nhiều nhất, sắp xếp theo doanh thu giảm dần hoặc số lượt đặt giảm dần.
- **AC 3.2:** Hiển thị bảng Top 5 bác sĩ thú y thực hiện nhiều ca khám thành công nhất trong kỳ lọc.
- **AC 3.3:** Bảng dữ liệu phải hiển thị đầy đủ thông tin: Tên bác sĩ/Tên dịch vụ, số lượng ca thực hiện, tổng doanh số đóng góp và phần trăm tỷ lệ đóng góp vào tổng doanh thu phòng khám.

---

## 4. Phạm vi dự án (In-Scope & Out-of-Scope)

### ✅ In-Scope (Phase 3 MVP)
- Dashboard báo cáo tài chính mờ kính tích hợp Chart.js.
- Thẻ chỉ số KPIs doanh thu, số ca khám và tính phần trăm tăng trưởng kỳ trước.
- Biểu đồ xu hướng Line Chart và biểu đồ cơ cấu Donut Chart.
- Bảng xếp hạng Top 5 bác sĩ điều trị và Top 5 dịch vụ doanh số cao.
- Bộ lọc ngày bắt đầu / ngày kết thúc linh hoạt.
- Xuất dữ liệu báo cáo thô ra định dạng file Excel.

### ❌ Out-of-Scope (Bàn giao Phase sau)
- Tự động dự báo doanh số các tháng tiếp theo sử dụng thuật toán học máy (Machine Learning / AI Forecasting).
- Tính toán chi phí lương nhân sự để ra chỉ số Lợi nhuận ròng (Net Profit) tự động (Sẽ làm ở Phase 3 nâng cấp).

---

## 5. Yêu cầu phi chức năng (Non-Functional Requirements - NFRs)
- **Hiệu năng:** Tải trang và kết xuất biểu đồ dưới 300ms đối với khoảng thời gian lọc dưới 90 ngày.
- **Bảo mật:** Chặn truy cập tuyệt đối đối với tất cả vai trò không phải Admin. API trả về `403 Forbidden` để ngăn chặn lộ dữ liệu tài chính nhạy cảm.
- **Tương thích:** Responsive tốt trên màn hình máy tính bàn và Laptop (1366x768 trở lên).
- **Độ chính xác:** Làm tròn số tiền VND chuẩn chỉnh, không có sai số dấu phẩy động trên giao diện và báo cáo Excel.
