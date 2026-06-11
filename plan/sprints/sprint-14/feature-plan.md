# 🗓️ Sprint 14: Quản Trị Hệ Thống & Cấu Hình Dịch Vụ
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng và hoàn thiện hệ thống quản trị hành chính của Phòng khám bao gồm:
1. **Quản lý Nhân sự & Phân quyền (RBAC) (T45):** Cho phép Admin thực hiện CRUD tài khoản nhân viên (Bác sĩ, Lễ tân, Thu ngân, Admin), gán vai trò (Roles) và khóa/mở khóa tài khoản nhân sự tức thời.
2. **Quản lý Danh mục Dịch vụ & Bảng giá (T46):** Admin quản lý danh sách các gói khám, dịch vụ thú y (phí khám lâm sàng, siêu âm, xét nghiệm...) và cấu hình đơn giá tương ứng.
3. **Cấu hình Khung giờ & Slots Đặt lịch (T49):** Admin thiết lập cấu hình mặc định cho các khung giờ làm việc (Slots), thời lượng mỗi ca khám, và số lượng thú cưng tối đa được tiếp nhận trong mỗi slot để tối ưu công suất phòng khám.

---

## 📋 Danh Sách Tasks (Sprint Backlog)

| Task ID | Tên Task | Trách nhiệm | Mô tả chi tiết | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| **T45** | CRUD Staff & Roles | Backend & Frontend | - API CRUD tài khoản nhân viên.<br>- Tích hợp RBAC filter `[Authorize(Roles = "admin")]`. (Ngăn chặn Privilege Escalation).<br>- Giao diện Staff List & Role Assignment mờ kính. | `❌ SPEC ONLY` |
| **T46** | CRUD Medical Services | Backend & Frontend | - API CRUD dịch vụ thú y và nhóm danh mục dịch vụ.<br>- Bảng quản trị danh mục và cập nhật bảng giá trực tiếp (Inline Editing). | `❌ SPEC ONLY` |
| **T49** | Flexible Slots Setup | Backend & Frontend | - Cấu hình hệ thống: Số lượng lịch hẹn tối đa trên mỗi Slot (Max Appointments), thời lượng mỗi ca (Duration).<br>- API & Giao diện cấu hình SlotConfig toàn cục. | `❌ SPEC ONLY` |

---

## 🛡️ Tiêu Chí Nghiệm Thu (Definition of Done - DoD)

### 1. Phía Backend (.NET Core)
- [ ] Hoàn thành API CRUD nhân sự, dịch vụ và cấu hình slot trong `AdminController` hoặc các Controller nghiệp vụ tương ứng.
- [ ] Bảo mật: Chặn đứng tấn công IDOR và Privilege Escalation bằng JWT Role Check ở tầng API.
- [ ] Tích hợp ghi nhận nhật ký hệ thống (Audit Logs) khi Admin thay đổi quyền hạn hoặc khóa tài khoản nhân viên.

### 2. Phía Frontend (Vue 3 / TypeScript)
- [ ] Dựng màn hình Dashboard Admin với sidebar điều hướng chuyên biệt.
- [ ] Sử dụng Pinia Store `useAdminStore` quản lý tập trung danh sách nhân viên, dịch vụ và thông số cấu hình slots.
- [ ] Giao diện Glassmorphism cao cấp, thiết kế responsive trên mọi màn hình máy tính và máy tính bảng.

### 3. Chất Lượng & Kiểm Thử (QA)
- [ ] Viết xUnit Unit Test kiểm thử cơ chế bảo mật (Chỉ Admin mới có quyền truy cập, các Role khác bị trả về HTTP 403 Forbidden).
- [ ] Kiểm thử biên: Đảm bảo không cho phép Admin tự xóa hoặc tự hạ quyền (demote) tài khoản của chính mình.
