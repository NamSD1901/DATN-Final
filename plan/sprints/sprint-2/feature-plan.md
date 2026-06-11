# 🗺️ LỘ TRÌNH VÀ CHI TIẾT SPRINT 2 - XÁC THỰC TÀI KHOẢN
## 📝 TÀI LIỆU KẾ HOẠCH TRIỂN KHAI VÀ PHÂN CHIA TÍNH NĂNG (SPRINT 2 MASTER PLAN)

Tài liệu này đặc tả chi tiết kế hoạch triển khai cho **Sprint 2: Xác Thực Tài Khoản (Account Authentication)**. Phân hệ chịu trách nhiệm xây dựng toàn bộ quy trình đăng ký tài khoản khách hàng mới bảo mật, mã hóa mật khẩu băm BCrypt, và cung cấp cổng đăng nhập xác thực cấp mã JSON Web Token (JWT) điều phối phân quyền truy cập hệ thống.

---

## 🛠 SPRINT 2: ACCOUNT AUTHENTICATION

### 2.1. Mục tiêu Sprint (Sprint Goal)
Triển khai hệ thống phân quyền và xác thực người dùng cốt lõi:
*   **Đăng ký tài khoản (PB01):** Cho phép người dùng mới tạo tài khoản khách hàng (`Customer`) trực tuyến, mã hóa mật khẩu bảo mật trước khi lưu vào CSDL.
*   **Đăng nhập hệ thống (PB02):** Xác thực tài khoản người dùng, sinh mã JSON Web Token (JWT) có đính kèm thông tin phân quyền vai trò (Role Claims) để kiểm soát truy cập hệ thống.

### 2.2. Danh sách công việc (Task Backlog)
1.  `[ ]` **T4:** PB01 - Hiện thực Backend API đăng ký tài khoản và mã hóa mật khẩu BCrypt.
2.  `[ ]` **T5:** PB01 - Xây dựng giao diện Form đăng ký tài khoản trên Vue 3, validate dữ liệu đầu vào.
3.  `[ ]` **T6:** PB02 - Hiện thực Backend API đăng nhập hệ thống và sinh mã Access Token JWT.
4.  `[ ]` **T7:** PB02 - Xây dựng giao diện Form đăng nhập, thiết lập Axios Interceptors tự động đính kèm Token và phân quyền Router Guard.

### 2.3. Tiêu chí nghiệm thu (DoD)
*   Mật khẩu người dùng được băm bảo mật bằng BCrypt (Work Factor = 11).
*   Không cho phép đăng ký trùng email (trả về lỗi `400 Bad Request`).
*   Xác thực thành công sinh mã JWT chứa Claims vai trò tương ứng và lưu trữ an toàn trong `localStorage` của trình duyệt.
*   API trả về mã `401 Unauthorized` khi thông tin đăng nhập sai.
