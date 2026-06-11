# 🗓️ Sprint 15: Quản Lý Kho Thuốc & Lịch Trực Nhân Sự
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng và hoàn thiện phân hệ quản trị vận hành kho dược phẩm và điều phối ca trực phòng khám:
1. **Quản Lý Kho Thuốc & Cảnh Báo (T47):** Admin thực hiện CRUD thông tin thuốc, số lượng tồn kho ban đầu, cảnh báo tự động khi số lượng dưới ngưỡng tối thiểu (Low Stock Threshold) hoặc thuốc sắp hết hạn sử dụng (Expiry Warning).
2. **Cấu Hình Phân Lịch Trực Nhân Sự (T48):** Phân lịch trực làm việc theo ca (Sáng/Chiều) cho Bác sĩ và Lễ tân theo ngày trong tuần/tháng, đảm bảo tự động kiểm tra chặn trùng lịch trực của nhân viên.

---

## 📋 Danh Sách Tasks (Sprint Backlog)

| Task ID | Tên Task | Trách nhiệm | Mô tả chi tiết | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| **T47** | Drug Inventory CRUD & Alert | Backend & Frontend | - API CRUD thuốc và danh mục nhóm thuốc.<br>- Logic Backend lọc thuốc có tồn kho < `MinThreshold` hoặc ngày hết hạn < 30 ngày.<br>- Giao diện Danh mục kho, cảnh báo trực quan bằng màu sắc (Đỏ/Vàng). | `❌ SPEC ONLY` |
| **T48** | Staff Work Schedule Config | Backend & Frontend | - API CRUD phân lịch trực nhân viên theo ngày & ca làm việc (Ca 1: 08:00-12:00, Ca 2: 13:30-17:30).<br>- Logic chặn xếp lịch trùng ca cho cùng một nhân viên trong ngày.<br>- Bảng lịch trực tổng quan (Schedule Calendar View). | `❌ SPEC ONLY` |

---

## 🛡️ Tiêu Chí Nghiệm Thu (Definition of Done - DoD)

### 1. Phía Backend (.NET Core)
- [ ] Hoàn thành API quản lý thuốc trong `MedicineController` và API xếp lịch trong `ScheduleController`.
- [ ] Chặn xếp trùng lịch ở tầng nghiệp vụ (`ScheduleService`) và trả về mã lỗi cụ thể khi có xung đột ca trực.
- [ ] Lọc hiệu năng cao trực tiếp trên cơ sở dữ liệu khi kết xuất báo cáo hàng sắp hết hạn/hết tồn kho.

### 2. Phía Frontend (Vue 3 / TypeScript)
- [ ] Xây dựng Pinia store `useInventoryStore` quản lý kho thuốc kèm các action lọc thông minh.
- [ ] Xây dựng Pinia store `useScheduleStore` để điều phối, hiển thị lịch trực theo dạng lưới hoặc lịch biểu trực quan.
- [ ] Giao diện Glassmorphism cao cấp, thiết kế responsive tối ưu trải nghiệm Admin/Lễ tân điều phối.

### 3. Chất Lượng & Kiểm Thử (QA)
- [ ] Viết xUnit Unit Test kiểm tra logic cảnh báo tồn kho thấp.
- [ ] Viết xUnit Unit Test kiểm thử logic chặn trùng lịch trực của nhân viên (Conflict detection).
