# 🗺️ LỘ TRÌNH VÀ CHI TIẾT SPRINT 3 - QUÊN MẬT KHẨU & HỒ SƠ CÁ NHÂN
## 📝 TÀI LIỆU KẾ HOẠCH TRIỂN KHAI VÀ PHÂN CHIA TÍNH NĂNG (SPRINT 3 MASTER PLAN)

Tài liệu này đặc tả chi tiết kế hoạch triển khai cho **Sprint 3: Quên Mật Khẩu & Hồ Sơ Cá Nhân (Reset Password & Personal Profile)**. Phân hệ chịu trách nhiệm xây dựng các dịch vụ lấy lại mật khẩu thông qua gửi mã OTP 6 số bảo mật về Email (SMTP), xác thực mã OTP, thiết lập mật khẩu mới, đồng thời cung cấp cổng quản lý cập nhật thông tin cá nhân và tải lên ảnh đại diện giới hạn kích thước an toàn.

---

## 🛠 SPRINT 3: RESET PASSWORD & PERSONAL PROFILE

### 3.1. Mục tiêu Sprint (Sprint Goal)
Hoàn thành phân hệ quản lý thông tin tài khoản người dùng và bảo mật phục hồi mật khẩu:
*   **Quên mật khẩu & OTP (PB03):** Người dùng quên mật khẩu có thể nhận một mã OTP ngẫu nhiên gồm 6 chữ số gửi qua email (SMTP), xác thực và đổi sang mật khẩu mới.
*   **Hồ sơ cá nhân (PB07):** Cho phép người dùng đã xác thực cập nhật thông tin cá nhân (Họ tên, SĐT, ngày sinh, địa chỉ) và thực hiện tải lên avatar của mình lên máy chủ.

### 3.2. Danh sách công việc (Task Backlog)
1.  `[ ]` **T8:** PB03 - Hiện thực Backend API Quên mật khẩu: Sinh mã OTP ngẫu nhiên có thời hạn 5 phút, lưu vào CSDL và gửi email qua MailKit/SMTP.
2.  `[ ]` **T9:** PB03 - Xây dựng giao diện Frontend Quên mật khẩu dạng Wizard Form 3 bước (nhập email -> nhập OTP -> nhập mật khẩu mới).
3.  `[ ]` **T14:** PB07 - Hiện thực Backend API Get/Update Profile cá nhân và API tải lên tệp tin ảnh đại diện.
4.  `[ ]` **T15:** PB07 - Xây dựng giao diện Frontend Profile cá nhân, cho phép điền biểu mẫu thông tin và upload/crop ảnh đại diện.

### 3.3. Tiêu chí nghiệm thu (DoD)
*   Mã OTP gồm đúng 6 chữ số ngẫu nhiên, tự động hết hiệu lực sau 5 phút.
*   Khi cập nhật thông tin cá nhân, số điện thoại phải được validate đúng định dạng số điện thoại Việt Nam.
*   Tải lên ảnh đại diện phải giới hạn dung lượng tối đa 2MB và chỉ chấp nhận định dạng ảnh hợp lệ (jpg, jpeg, png).
*   Chặn đứng các truy cập trái phép vào API cập nhật hồ sơ khi thiếu Access Token JWT hợp lệ.
