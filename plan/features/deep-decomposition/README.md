# 🏛️ BẢN ĐỒ PHÂN RÃ CHI TIẾT DỰ ÁN MYPETCLINIC (DEEP DECOMPOSITION MASTER INDEX)

## 📝 TRUNG TÂM QUẢN TRỊ KIẾN TRÚC & CHỈ MỤC CÁC PHÂN HỆ DỰ ÁN

Tài liệu này đóng vai trò hạt nhân điều phối toàn bộ các phân hệ chức năng độc lập (Epics / User Stories) của dự án **MyPetClinic**. Nhằm chi tiết hóa tối đa thiết kế của từng cấu trúc, mỗi thư mục đặc tả chỉ đại diện cho **đúng 1 tính năng độc nhất**, liên kết PRD, Technical Spec, Code Logic và trạng thái thực tế.

---

## 📌 BẢN ĐỒ CHỈ MỤC LIÊN KẾT NHANH (MASTER PATHS INDEX)

> Chú thích trạng thái:
> *   `✅ CODE DONE`: Tính năng đã được code và kiểm thử thành công.
> *   `🟡 IN PROGRESS`: Tính năng đang được triển khai trong sprint hiện tại.
> *   `❌ SPEC ONLY`: Chỉ có tài liệu đặc tả, chưa viết code.

---

### 🟢 PHASE 1: NỀN TẢNG, XÁC THỰC & HỒ SƠ CÁ NHÂN (SPRINT 1 - 2)

- **[00. Authentication & Authorization Architecture](./phase1-auth-authz)** `✅ CODE DONE` — Tài liệu tổng quan kiến trúc, sơ đồ bảo mật và cơ chế xác thực toàn bộ hệ thống.
- **[01. Register Account](./phase1-01-register)** `✅ CODE DONE` — Đăng ký tài khoản khách hàng mới, sinh mã OTP 6 số ngẫu nhiên.
- **[02. Login System](./phase1-02-login)** `✅ CODE DONE` — Đăng nhập bằng email/mật khẩu, cấp quyền Cookie/JWT, phân quyền Role.
- **[03. Google OAuth Login](./phase1-03-google-login)** `✅ CODE DONE` — Tích hợp đăng nhập nhanh một chạm sử dụng Google Identity Services.
- **[04. Forgot Password Recovery](./phase1-04-forgot-password)** `✅ CODE DONE` — Khôi phục tài khoản, đặt lại mật khẩu qua mã OTP gửi về Email.
- **[05. Homepage Services](./phase1-05-homepage-services)** `✅ CODE DONE` — Xem danh sách dịch vụ và bảng giá công khai, hỗ trợ lọc theo danh mục.
- **[06. Homepage Vets Team](./phase1-06-homepage-doctors)** `✅ CODE DONE` — Xem danh sách bác sĩ thú y, chuyên môn và kinh nghiệm làm việc công khai.
- **[07. Profile Details](./phase1-07-profile-update)** `✅ CODE DONE` — Khách hàng tự xem và cập nhật thông tin cá nhân (Họ tên, SĐT, Địa chỉ, Ngày sinh).
- **[08. Profile Avatar Upload](./phase1-08-profile-avatar)** `✅ CODE DONE` — Tải lên tệp ảnh làm đại diện, giới hạn dung lượng < 2MB và định dạng tệp an toàn.
- **[09. Pet Portfolio Management](./phase1-09-pet-portfolio)** `🟡 IN PROGRESS` — CRUD danh sách thú cưng của khách hàng, kiểm duyệt quyền sở hữu tránh IDOR qua ActionFilter, UI Glassmorphism.

---

### 🔵 PHASE 2: NGHIỆP VỤ CỐT LÕI MVP & ĐIỀU PHỐI PHÒNG KHÁM (SPRINT 3 - 5)

- **[10. Online Examination Booking](./phase2-10-examination-booking)** `✅ CODE DONE` — Khách hàng đặt lịch hẹn khám bệnh trực tuyến, chọn dịch vụ, bác sĩ và ngày/giờ.
- **[11. Online Vaccination Booking](./phase2-11-vaccination-booking)** `✅ CODE DONE` — Khách hàng đặt lịch tiêm phòng vắc-xin trực tuyến, chọn loại vắc-xin cụ thể từ danh mục.
- **[12. Customer Appointments Dashboard](./phase2-12-appointment-management)** `✅ CODE DONE` — Khách hàng quản lý, theo dõi lịch sử và trạng thái timeline các lịch hẹn đã đặt.
- **[13. Receptionist Portal & Queue Management](./phase2-receptionist-queue)** `✅ CODE DONE` — Lễ tân check-in khách có lịch hẹn, duyệt/hủy lịch hẹn và quản lý hàng đợi.
- **[14. Clinical Diagnosis & Treatment](./phase2-clinical-diagnosis)** `✅ CODE DONE` — Bác sĩ xem hàng khám, xem bệnh sử thú cưng, ghi nhận chẩn đoán và kê đơn thuốc.
- **[15. Cashier & Invoicing](./phase2-cashier-invoicing)** `✅ CODE DONE` — Lễ tân/Thu ngân lập hóa đơn dịch vụ & thuốc, xác nhận thanh toán và in hóa đơn.

---

### 🟡 PHASE 3: TRÍ TUỆ NHÂN TẠO, TỰ ĐỘNG HÓA & QUẢN TRỊ NÂNG CAO (SPRINT 6)

- **[21. Gemini AI Advisor Chatbot](./phase3-ai-chatbot)** `✅ CODE DONE` — Trợ lý AI chatbot tư vấn nhanh kiến thức y tế thú y và sơ cứu khẩn cấp cơ bản.
- **[22. Automatic Vaccine Reminders](./phase3-notifications)** `✅ CODE DONE` — Background job quét database và tự động gửi email nhắc lịch tiêm phòng trước 3-5 ngày.
- **[23. Admin Personnel & Role Authorization](./phase3-23-admin-users)** `✅ CODE DONE` — Admin quản lý tài khoản nhân sự phòng khám, phân quyền vai trò nhân viên.
- **[24. Admin Drug & Medical Supplies Inventory](./phase3-24-admin-inventory)** `✅ CODE DONE` — Admin quản lý tồn kho thuốc vật tư, cảnh báo hàng sắp hết hoặc hết hạn sử dụng.
- **[25. Admin Revenue & Performance Reports](./phase3-25-admin-reports)** `✅ CODE DONE` — Admin báo cáo thống kê doanh số trực quan bằng biểu đồ và KPI chi tiết.

---

## 🎨 HƯỚNG DẪN THIẾT KẾ VÀ TIÊU CHUẨN LẬP TRÌNH (CORE RULES)

1.  **Một thư mục - Một tính năng:** Mỗi thư mục con đại diện cho đúng một chức năng độc lập (User Story). Phải có đầy đủ cấu trúc 12 tệp đặc tả chi tiết bên trong.
2.  **Rich Aesthetics & Dark Mode:** Giao diện Dashboard sang trọng sử dụng bảng màu HSL, hiệu ứng Glassmorphism mờ kính và bo tròn góc mượt mà.
3.  **Tối ưu hóa Hiệu năng & Bảo mật:** Chặn truy cập trái phép ở backend bằng Middleware RBAC, kiểm tra quyền sở hữu đối với các API cá nhân.
4.  **Kiểm thử liên tục:** Chạy `dotnet test` trước khi kết thúc bất cứ Sprint nào để bảo đảm tỷ lệ pass 100%.
