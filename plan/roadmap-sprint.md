# 🗓️ Lộ Trình Phân Chia Sprint & Sprint Backlog - MyPetClinic

Tài liệu này đặc tả chi tiết **Sprint Backlog** gồm 17 Sprints phát triển và hướng dẫn vận hành cho dự án MyPetClinic.

---

## 🎯 Quy Ước Chung & Định Nghĩa Hoàn Thành (DoD)
*   **Độ dài mỗi Sprint:** 2 tuần (10 ngày làm việc thực tế).
*   **Tổng số Sprint:** 17 Sprints.
*   **Định nghĩa Hoàn thành (Definition of Done - DoD):**
    *   Code đã được viết Clean Code, review chéo và merge thành công vào nhánh `develop`.
    *   Đã viết và chạy pass Unit Tests cho các API endpoints và dịch vụ nghiệp vụ cốt lõi.
    *   Đã deploy bản dựng lên môi trường Staging/Development cục bộ và kiểm thử liên kết (Integration Test) thành công.
    *   Không còn lỗi nghiêm trọng thuộc nhóm `Critical` hay `High` tồn đọng.

---

## 🎯 Danh Sách Chi Tiết 17 Sprints (Đang thực thi rà soát & Tối ưu hóa chất lượng từ Sprint 5)


### 🎯 Sprint 1: Khởi Tạo Dự Án & CI/CD
**Mục tiêu:** Thiết lập nền tảng kỹ thuật và CI/CD.
*   **T1:** Thiết lập cấu trúc Solution Backend (.NET 8 Clean Architecture) [Nam - 8 giờ]
*   **T2:** Thiết lập dự án Frontend (Vue 3 + TypeScript + CSS/Tailwind) [Phương - 8 giờ]
*   **T3:** Thiết lập CI/CD Pipeline (GitHub Actions) [Lâm - 6 giờ]

### 🎯 Sprint 2: Xác Thực Tài Khoản (Authentication)
**Mục tiêu:** Hoàn thiện luồng Đăng ký và Đăng nhập JWT.
*   **T4:** PB01 - Đăng ký tài khoản (Backend API & mã hóa BCrypt) [Nam - 8 giờ]
*   **T5:** PB01 - Đăng ký tài khoản (Frontend Form UI & Validation) [Phương - 6 giờ]
*   **T6:** PB02 - Đăng nhập hệ thống (Backend JWT Generator) [Nam - 6 giờ]
*   **T7:** PB02 - Đăng nhập hệ thống (Frontend store, Token storage & Router guard) [Phương - 6 giờ]

### 🎯 Sprint 3: Quên Mật Khẩu & Hồ Sơ Cá Nhân
**Mục tiêu:** Xử lý xác thực OTP và thông tin tài khoản người dùng.
*   **T8:** PB03 - Quên mật khẩu (Backend sinh OTP & gửi SMTP Email) [Hạnh - 8 giờ]
*   **T9:** PB03 - Quên mật khẩu (Frontend wizard nhập OTP & reset password) [Phương - 4 giờ]
*   **T14:** PB07 - Quản lý thông tin cá nhân (Backend Get/Update Profile) [Nam - 6 giờ]
*   **T15:** PB07 - Quản lý thông tin cá nhân (Frontend Form Profile) [Lâm - 6 giờ]

### 🎯 Sprint 4: Cổng Dịch Vụ & Bác Sĩ Công Khai
**Mục tiêu:** Khách vãng lai có thể tra cứu thông tin dịch vụ và bác sĩ tại phòng khám.
*   **T10:** PB04 - Xem danh sách dịch vụ & bảng giá (Backend API & Seed Data) [Hạnh - 6 giờ]
*   **T11:** PB04 - Xem danh sách dịch vụ & bảng giá (Frontend Cards view & Filter) [Phương - 8 giờ]
*   **T12:** PB05 - Xem đội ngũ bác sĩ (Backend API lọc Vet) [Hạnh - 4 giờ]
*   **T13:** PB05 - Xem đội ngũ bác sĩ (Frontend Doctors Grid view) [Phương - 6 giờ]

### 🎯 Sprint 5: Hồ Sơ Thú Cưng (Tái Cấu Trúc Chất Lượng Cao & Chống IDOR)
**Mục tiêu:** Nâng cấp chất lượng code, tối ưu truy vấn, bảo mật IDOR qua ActionFilter và xây dựng giao diện Premium Glassmorphic.
*   **T16:** PB08 - Quản lý hồ sơ thú cưng (Backend CRUD & Chống IDOR bằng cách check OwnerId) [Nam - 10 giờ]
*   **T17:** PB08 - Quản lý hồ sơ thú cưng (Frontend Grid & Modal Form thêm/sửa có ảnh) [Lâm - 10 giờ]

### 🎯 Sprint 6: Ca Làm Việc & Khung Giờ Bác Sĩ
**Mục tiêu:** Xây dựng cấu trúc ca trực bác sĩ và tính toán slot trống.
*   **T18:** Thiết kế logic Slot & Ca làm việc (Backend Helper kiểm tra Slot Availability) [Nam - 12 giờ]

### 🎯 Sprint 7: Đặt Lịch Khám & Tiêm Chủng Trực Tuyến
**Mục tiêu:** Chủ nuôi đặt lịch hẹn trực tuyến ngăn ngừa double-booking.
*   **T19:** PB09 - Đặt lịch khám bệnh (Backend Validate trùng lịch & Serializable Transaction) [Nam - 12 giờ]
*   **T20:** PB09 - Đặt lịch khám bệnh (Frontend Wizard Form đặt lịch động) [Phương - 14 giờ]
*   **T21:** PB10 - Đặt lịch tiêm chủng (Backend gắn Vaccine & kiểm tra kho) [Nam - 8 giờ]
*   **T22:** PB10 - Đặt lịch tiêm chủng (Frontend tích hợp chọn vaccine) [Phương - 10 giờ]

### 🎯 Sprint 8: Theo Dõi Cuộc Hẹn & Lịch Sử
**Mục tiêu:** Theo dõi và xem lại lịch sử y tế.
*   **T23:** PB11 - Quản lý lịch hẹn (Backend API lọc trạng thái & phân trang) [Hạnh - 6 giờ]
*   **T24:** PB11 - Quản lý lịch hẹn (Frontend Timeline view) [Lâm - 8 giờ]
*   **T44:** PB12 - Xem lịch sử dịch vụ & bệnh án (Frontend Customer view) [Lâm - 8 giờ]

### 🎯 Sprint 9: Tiếp Nhận & Duyệt Lịch Hẹn
**Mục tiêu:** Cổng lễ tân duyệt lịch hẹn và thực hiện check-in.
*   **T25:** PB15 - Tiếp nhận khách hàng (Backend Check-in hoặc tạo Walk-in) [Nam - 8 giờ]
*   **T26:** PB15 - Tiếp nhận khách hàng (Frontend Receptionist dashboard & Search) [Lâm - 10 giờ]
*   **T27:** PB17 - Xác nhận/Hủy lịch hẹn (Backend Update trạng thái & gửi mail lý do hủy) [Nam - 8 giờ]
*   **T28:** PB17 - Xác nhận/Hủy lịch hẹn (Frontend Dashboard chờ duyệt & cancel modal) [Lâm - 10 giờ]

### 🎯 Sprint 10: Điều Phối Hàng Đợi
**Mục tiêu:** Cấp số thứ tự tự động và màn hình sảnh chờ công cộng.
*   **T29:** PB18 - Quản lý hàng đợi (Backend logic Queue tự động cấp số thứ tự khám) [Hạnh - 10 giờ]
*   **T30:** PB18 - Quản lý hàng đợi (Frontend nút lễ tân điều phối) [Lâm - 10 giờ]

### 🎯 Sprint 11: Cổng Bác Sĩ & Tiếp Nhận Khám
**Mục tiêu:** Bác sĩ theo dõi và kích hoạt ca khám bệnh.
*   **T31:** PB20 - Xem lịch khám của bác sĩ (Backend API gán Queue của bác sĩ) [Nam - 6 giờ]
*   **T32:** PB20 - Xem lịch khám của bác sĩ (Frontend Doctor Queue dashboard) [Phương - 8 giờ]
*   **T33:** PB22 - Tiếp nhận ca khám (Backend đổi trạng thái sang In_Progress) [Nam - 4 giờ]
*   **T34:** PB22 - Tiếp nhận ca khám (Frontend kích hoạt chuyển sang trang khám) [Phương - 6 giờ]

### 🎯 Sprint 12: Bệnh Án & Kê Đơn Thuốc (Trừ Kho Tự Động)
**Mục tiêu:** Khám lâm sàng và kê đơn thuốc trừ kho có giao dịch an toàn.
*   **T35:** PB21 - Xem hồ sơ & lịch sử thú cưng (Backend API tổng hợp bệnh sử) [Hạnh - 6 giờ]
*   **T36:** PB21 - Xem hồ sơ & lịch sử thú cưng (Frontend Doctor Timeline view) [Phương - 8 giờ]
*   **T37:** PB23 - Quản lý bệnh án & Kê đơn (Backend MedicalRecord Transaction & trừ kho rollback) [Nam - 14 giờ]
*   **T38:** PB23 - Quản lý bệnh án & Kê đơn (Frontend Dynamic prescription form & stock warning) [Phương - 14 giờ]
*   **T39:** PB25 - Cập nhật trạng thái lịch hẹn (Backend hoàn thành ca khám sang Completed) [Hạnh - 4 giờ]

### 🎯 Sprint 13: Tiêm Chủng Vaccine & Thanh Toán Hóa Đơn
**Mục tiêu:** Ghi nhận mũi tiêm và thanh toán xuất hóa đơn tại quầy.
*   **T40:** PB19 - Thanh toán hóa đơn (Backend tự động kết xuất hóa đơn & phí dịch vụ/thuốc) [Nam - 10 giờ]
*   **T41:** PB19 - Thanh toán hóa đơn (Frontend In hóa đơn & nút Xác nhận thanh toán) [Lâm - 10 giờ]
*   **T42:** PB24 - Thực hiện tiêm chủng (Backend cập nhật lịch sử tiêm & tính NextDueDate) [Hạnh - 8 giờ]
*   **T43:** PB24 - Thực hiện tiêm chủng (Frontend nhập vaccine & số lô) [Phương - 6 giờ]

### 🎯 Sprint 14: Quản Trị Hệ Thống & Cấu Hình Dịch Vụ
**Mục tiêu:** Quản lý tài khoản và danh mục dịch vụ dành cho Quản trị viên.
*   **T45:** PB28 - Quản lý người dùng (Backend & Frontend CRUD user) [Nam & Lâm - 12 giờ]
*   **T46:** PB26, 27 - Quản lý dịch vụ & danh mục (Backend & Frontend CRUD Services) [Hạnh & Phương - 10 giờ]
*   **T49:** PB31 - Cấu hình khung giờ đặt lịch (Backend & Frontend slot config) [Hạnh & Phương - 8 giờ]

### 🎯 Sprint 15: Quản Trị Kho Thuốc & Ca Trực Bác Sĩ
**Mục tiêu:** Quản trị dược phẩm và phân ca trực của bác sĩ.
*   **T47:** PB29 - Quản lý thuốc & Vật tư (Backend & Frontend CRUD thuốc & nhập kho) [Nam & Lâm - 14 giờ]
*   **T48:** PB30 - Quản lý lịch làm việc bác sĩ (Backend & Frontend CRUD Vet Schedules) [Hạnh & Phương - 12 giờ]

### 🎯 Sprint 16: Báo Cáo Doanh Thu & Cổng Tin Tức
**Mục tiêu:** Thống kê biểu đồ tài chính và quản lý blog cẩm nang.
*   **T50:** PB33 - Báo cáo doanh thu (Backend aggregate SQL & Frontend Chart.js) [Nam & Lâm - 16 giờ]
*   **T54:** PB06 - Xem bài viết/Blog (Backend & Frontend cẩm nang sức khỏe) [Hạnh & Lâm - 10 giờ]
*   **T55:** PB32 - Quản lý bài viết & đánh giá (Admin duyệt bài) [Hạnh & Phương - 8 giờ]
*   **T56:** PB13 - Đánh giá dịch vụ (Khách hàng gửi số sao & nhận xét) [Nam & Lâm - 8 giờ]

### 🎯 Sprint 17: Trợ Lý Gemini AI, Nhắc Lịch Tự Động & E2E Test
**Mục tiêu:** Các tính năng nâng cao thông minh và tối ưu hóa trước phát hành.
*   **T51:** PB14 - Tích hợp Gemini AI Chatbot (Backend System Prompt & SDK) [Hạnh - 10 giờ]
*   **T52:** PB14 - Tích hợp Gemini AI Chatbot (Frontend Floating Widget Chat UI) [Phương - 8 giờ]
*   **T53:** PB34 - Tự động nhắc lịch tái chủng (Quartz Background Job) [Hạnh - 8 giờ]
*   **T57:** Technical - Kiểm thử tích hợp E2E & Sửa lỗi tổng thể [Cả nhóm - 16 giờ]
