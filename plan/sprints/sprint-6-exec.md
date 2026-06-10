# 🚀 Kế Hoạch Thực Thi Chi Tiết - Sprint 6

## 🎯 Mục Tiêu Sprint 6
Tích hợp chatbot tư vấn thông minh sử dụng Gemini AI, thiết lập hệ thống gửi email tự động nhắc lịch tiêm phòng, xây dựng trang quản trị (Admin Dashboard) với báo cáo thống kê doanh thu trực quan, hoàn thiện các tính năng phụ trợ (Tin tức, Đánh giá dịch vụ) và kiểm thử toàn diện toàn hệ thống (E2E Testing).

---

## 🛠️ Chi Tiết Các Bước Thực Thi (Step-by-Step)

### 🤖 1. Trợ Lý Tư Vấn Gemini AI Chatbot (T51, T52)
#### 🖥️ Backend API - *Hạnh thực hiện*
* **API Endpoint:** `POST /api/v1/ai-chatbot/ask`
* **Logic xử lý:**
  1. Cấu hình SDK Gemini AI kết nối bằng API Key bảo mật lưu trữ trong cấu hình hệ thống (`appsettings.json` / Environment Variable).
  2. Xây dựng System Prompt định hướng cho AI đóng vai trò là bác sĩ thú y ảo chuyên nghiệp của MyPetClinic:
     * Chỉ tư vấn kiến thức y tế cơ bản, hướng dẫn sơ cứu khẩn cấp.
     * Khuyên khách hàng mang thú cưng đến phòng khám nếu gặp triệu chứng nguy hiểm.
     * Không kê đơn thuốc chuyên sâu trực tuyến.
  3. Duy trì lịch sử chat ngắn hạn gửi kèm theo request để tạo hội thoại liền mạch.

#### 🌐 Frontend UI - *Phương thực hiện*
* **Thành phần giao diện:** Chat Widget (thường hiển thị ở góc dưới cùng bên phải màn hình).
* **Giao diện:** Cửa sổ chat nhỏ gọn, hiển thị tin nhắn của user và AI phản hồi sinh động (hiệu ứng typing).

---

### ⏰ 2. Tự Động Nhắc Lịch Tiêm Phòng (T53)
#### 🖥️ Backend Background Job - *Hạnh thực hiện*
* **Thư viện tích hợp:** Hangfire hoặc Quartz.NET.
* **Logic xử lý:**
  1. Thiết lập một Background Job chạy định kỳ hàng ngày vào lúc 08:00 sáng.
  2. Quét bảng `Vaccination_records` tìm kiếm các thú cưng có `Next_due_date` cách ngày hiện tại đúng 3 đến 5 ngày.
  3. Lấy thông tin email chủ nuôi của thú cưng đó.
  4. Sử dụng dịch vụ gửi thư SMTP (SendGrid/Gmail) để tự động gửi email nhắc lịch tiêm nhắc lại, kèm theo link dẫn nhanh đến trang đặt lịch hẹn trực tuyến của phòng khám.

---

### 📊 3. Trang Quản Trị & Báo Cáo Thống Kê (T45, T46, T47, T48, T49, T50)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoints:**
  * `GET /api/v1/admin/dashboard/revenue` (Lấy dữ liệu doanh thu theo khoảng thời gian: ngày, tuần, tháng, năm).
  * `GET /api/v1/admin/dashboard/stats` (Số lượng lịch hẹn mới, số ca hoàn thành, số thú cưng đăng ký mới).
  * CRUD các API quản lý: Người dùng, Dịch vụ, Kho thuốc, Lịch làm việc của bác sĩ.

#### 🌐 Frontend UI - *Lâm & Phương thực hiện*
* **Trang:** `views/Dashboard.vue` (giao diện Admin)
* **Giao diện:**
  * Tích hợp biểu đồ trực quan (sử dụng thư viện `chart.js` hoặc `apexcharts`) hiển thị đường xu hướng doanh thu và biểu đồ tròn thể hiện tỉ lệ doanh thu theo từng dịch vụ.
  * Các trang quản lý dạng bảng (Tables) có tính năng phân trang, tìm kiếm và form thêm/sửa nhanh bằng Modals.

---

### 📝 4. Đánh Giá & Cẩm Nang Sức Khỏe (T54, T55, T56)
#### 🖥️ Backend & Frontend - *Hạnh & Lâm thực hiện*
* **Đánh giá dịch vụ (PB13):** API & Form giao diện cho phép khách hàng gửi số sao (1-5) và nhận xét sau khi hoàn tất ca khám chữa bệnh.
* **Cẩm nang sức khỏe (PB06):** API & Giao diện đăng bài viết chia sẻ kinh nghiệm chăm sóc thú cưng dành cho bác sĩ/admin.

---

### 🧪 5. Kiểm Thử Hệ Thống E2E & Tối Ưu Hóa (T57)
#### 👥 Thực hiện - *Cả nhóm*
1. Thực thi kiểm thử luồng nghiệp vụ hoàn chỉnh (End-to-End Test):
   * Đăng ký tài khoản $\rightarrow$ Thêm thú cưng $\rightarrow$ Đặt lịch hẹn khám $\rightarrow$ Lễ tân duyệt & Check-in $\rightarrow$ Bác sĩ ghi nhận khám bệnh & kê đơn $\rightarrow$ Lễ tân thanh toán hóa đơn.
2. Kiểm tra tính bảo mật của API: Chặn các truy cập không có token JWT hợp lệ hoặc sai phân quyền Role.
3. Tối ưu hóa Database: Tạo Index trên các cột tìm kiếm thường xuyên như `Email` trong bảng `Users`, `Phone` trong bảng `Users`, và `Pet_id` trong bảng `Appointments`.

---

## 🔬 Kế Hoạch Kiểm Thử & Nghiệm Thu (DoD)
1. **Kiểm thử tự động:** Đảm bảo tất cả các test suite từ các Sprint trước pass 100%.
2. **Kiểm thử thủ công:** Demo hoàn tất một ca khám, xác minh doanh thu trên biểu đồ Admin cập nhật tăng đúng với số tiền hóa đơn vừa thanh toán.
