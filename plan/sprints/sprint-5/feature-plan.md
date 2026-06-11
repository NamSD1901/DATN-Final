# 🗺️ LỘ TRÌNH VÀ CHI TIẾT SPRINT 5 - HỒ SƠ THÚ CƯNG (IDOR PROTECTED)
## 📝 TÀI LIỆU KẾ HOẠCH TRIỂN KHAI VÀ PHÂN CHIA TÍNH NĂNG (SPRINT 5 MASTER PLAN)

Tài liệu này đặc tả chi tiết kế hoạch triển khai cho **Sprint 5: Hồ Sơ Thú Cưng (Pet Portfolio Management)**. Phân hệ chịu trách nhiệm xây dựng toàn bộ quy trình quản lý hồ sơ thú cưng (CRUD) dành cho chủ nuôi, đồng thời áp dụng cơ chế xác thực chặt chẽ chống tấn công IDOR trực tiếp tại lớp Application Service nhằm chặn đứng nguy cơ truy xuất hoặc phá hoại hồ sơ của tài khoản khác.

---

## 🛠 SPRINT 5: PET PORTFOLIO MANAGEMENT (IDOR PROTECTED)

### 5.1. Mục tiêu Sprint (Sprint Goal)
Triển khai hệ thống quản trị hồ sơ thú cưng an toàn và trực quan:
*   **CRUD Hồ sơ Thú cưng (PB08):** Cho phép chủ nuôi thêm mới, chỉnh sửa thông tin sinh học (Cân nặng, giống loài, tiền sử dị ứng) và xóa mềm (Soft Delete) hồ sơ thú cưng.
*   **Chống tấn công IDOR:** Bắt buộc đối chiếu `OwnerId` của thú cưng với `currentUserId` giải mã từ mã JWT Token trực tiếp trong lớp logic nghiệp vụ của Backend Service.

### 5.2. Danh sách công việc (Task Backlog)
1.  `[ ]` **T16:** PB08 - Hiện thực Backend API CRUD thú cưng và logic phân quyền an toàn, chặn đứng 100% tấn công IDOR.
2.  `[ ]` **T17:** PB08 - Xây dựng giao diện Frontend Grid hiển thị danh sách thú cưng và form modal thêm/sửa tích hợp tải ảnh.

### 5.3. Tiêu chí nghiệm thu (DoD)
*   Thực hiện xóa mềm (Soft Delete) bằng cách gán `DeletedAt` trong DB thay vì xóa vật lý.
*   Khi có bất kỳ yêu cầu sửa đổi hoặc truy xuất chi tiết, hệ thống trả về lỗi `403 Forbidden` hoặc `404 Not Found` nếu người dùng hiện tại không phải là chủ sở hữu thực sự.
*   Độ phủ kiểm thử bảo mật (IDOR test case) đạt 100% các API tác động đến dữ liệu.
