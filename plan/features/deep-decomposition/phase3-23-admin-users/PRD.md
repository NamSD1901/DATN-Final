# 🚀 Product Requirements Document (PRD) - Admin Staff Management

## 1. Tổng quan & Tầm nhìn
Phân hệ **Quản trị Nhân sự (Admin Staff Management)** cho phép Quản trị viên (Admin) quản lý và phân quyền toàn bộ đội ngũ nhân sự trong hệ thống phòng khám MyPetClinic (bao gồm: Bác sĩ thú y, Lễ tân, Thu ngân, Admin). Tính năng này bảo vệ an ninh thông tin và phân chia rõ ràng trách nhiệm công việc trong phòng khám.

---

## 2. Yêu cầu Nghiệp vụ Chi tiết
- **Quản lý danh sách nhân sự (PB28):**
  - Hiển thị danh sách toàn bộ nhân viên trong phòng khám kèm vai trò (Role), phòng ban làm việc, trạng thái tài khoản.
  - Cho phép Admin thêm mới nhân viên, cập nhật thông tin cá nhân và thay đổi mật khẩu mặc định.
- **Phân quyền và Vai trò (RBAC):**
  - Admin có thể cấp quyền và thay đổi vai trò (Role) cho nhân viên sang: `doctor` (Bác sĩ), `receptionist` (Lễ tân), `cashier` (Thu ngân), `admin` (Quản trị viên).
- **Khóa & Kích hoạt tài khoản:**
  - Cho phép Admin tạm thời khóa tài khoản nhân viên (ngăn cản đăng nhập tức thời) hoặc mở khóa tài khoản khi cần thiết.
- **Bảo mật:**
  - Chặn đứng mọi hành vi tự động nâng quyền (Privilege Escalation) từ người dùng không phải Admin.
