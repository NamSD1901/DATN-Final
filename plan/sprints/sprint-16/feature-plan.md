# 🗓️ Sprint 16: Báo Cáo Doanh Thu & Cổng Thông Tin (Blog)
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng phân hệ báo cáo quản trị và cổng thông tin truyền thông của phòng khám:
1. **Báo Cáo Doanh Thu & Hiệu Suất (T50):** Cung cấp các biểu đồ cột/đường thống kê tổng doanh thu theo ngày/tuần/tháng/năm, so sánh doanh thu giữa các dịch vụ và thuốc, thống kê số ca khám hoàn thành của từng bác sĩ.
2. **Cổng Thông Tin Kiến Thức Thú Y (Blog) (T51):** Xây dựng trang Blog công khai cho phép người dùng đọc tin tức, cẩm nang chăm sóc thú cưng. Admin có quyền viết bài, chỉnh sửa, đăng bài hoặc xóa bài viết.

---

## 📋 Danh Sách Tasks (Sprint Backlog)

| Task ID | Tên Task | Trách nhiệm | Mô tả chi tiết | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| **T50** | Revenue Reports & Charts | Backend & Frontend | - API tính tổng doanh thu từ hóa đơn (Invoices) đã thanh toán bằng aggregate LINQ trực tiếp ở DB.<br>- Giao diện Admin Dashboard tích hợp Chart.js vẽ biểu đồ doanh thu theo thời gian. | `❌ SPEC ONLY` |
| **T51** | Blog Management Portal | Backend & Frontend | - API CRUD bài viết (`BlogPost`) lưu trong Database.<br>- Giao diện đọc bài viết ngoài trang chủ (Public Client Portal) và trang quản trị bài viết của Admin (Rich-text editor mockup). | `❌ SPEC ONLY` |

---

## 🛡️ Tiêu Chỉ Nghiệm Thu (Definition of Done - DoD)

### 1. Phía Backend (.NET Core)
- [ ] Xây dựng `ReportService` thực hiện các truy vấn gộp (`Sum`, `Count`, `GroupBy`) trên SQL Server thông qua EF Core, đảm bảo không tải dữ liệu thô (raw list) lên bộ nhớ server.
- [ ] Bảo mật: Chặn đứng truy cập trái phép API báo cáo thống kê đối với các vai trò không phải `admin`.
- [ ] Hoàn thành API CRUD bài viết tin tức y tế thú cưng.

### 2. Phía Frontend (Vue 3 / TypeScript)
- [ ] Tạo Pinia store `useReportStore` để lưu trữ dữ liệu thống kê doanh thu nhận từ Backend.
- [ ] Tích hợp thư viện Chart.js hiển thị trực quan các biểu đồ doanh thu trên giao diện Admin Dashboard Glassmorphism.
- [ ] Thiết kế trang danh sách bài viết Blog và chi tiết bài viết thân thiện với công cụ tìm kiếm (SEO meta tags).

### 3. Chất Lượng & Kiểm Thử (QA)
- [ ] Viết xUnit Unit Test kiểm thử logic tính toán doanh thu tổng hợp (đảm bảo gộp chính xác giá trị từ các hóa đơn đã thanh toán, bỏ qua hóa đơn chưa thanh toán hoặc đã hủy).
