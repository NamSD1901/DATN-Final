# 🐾 MyPetClinic - Hệ Thống Quản Lý Phòng Khám Thú Y Toàn Diện

Dự án **MyPetClinic** là một giải pháp phần mềm toàn diện (ERP) hỗ trợ chuyển đổi số quá trình quản lý và vận hành phòng khám thú y. Dự án được nghiên cứu và phát triển bởi nhóm sinh viên: **Nam, Phương, Hạnh, Lâm**.

Được xây dựng trên nền tảng kiến trúc **Clean Architecture** mạnh mẽ kết hợp với phong cách thiết kế UI/UX hiện đại (Premium Glassmorphism), hệ thống đem lại một quy trình làm việc khép kín, tự động hóa cao và trải nghiệm người dùng xuất sắc cho cả Khách hàng lẫn Đội ngũ nhân viên y tế.

---

## 🎯 Mục Tiêu & Chủ Đề Dự Án

Chủ đề cốt lõi của dự án là **"Chuyển đổi số trong quản lý dịch vụ chăm sóc sức khỏe thú cưng"**. Mục tiêu của MyPetClinic là thay thế hoàn toàn sổ khám bệnh giấy truyền thống, số hóa luồng đi của bệnh án từ khâu **Tiếp đón (Lễ tân) -> Khám chữa bệnh (Bác sĩ) -> Thanh toán (Thu ngân) -> Chăm sóc khách hàng sau dịch vụ**, giảm thiểu sai sót y khoa và tối ưu hóa thời gian chờ đợi.

---

## 🌟 Các Tính Năng Nổi Bật (Core Features)

### 🏥 Quản Lý Bệnh Án Chuẩn Y Khoa (SOAP)
- Tích hợp mô hình ghi chép bệnh án **SOAP (Subjective - Objective - Assessment - Plan)** chuyên nghiệp.
- Cho phép bác sĩ nhập liệu chi tiết: Lý do khám (S), Khám lâm sàng & Sinh hiệu (O), Chẩn đoán y khoa sơ bộ & xác định (A), và Phác đồ điều trị kèm kê đơn thuốc (P).
- Số hóa hồ sơ tiêm phòng (Vaccination Records) và tự động nhắc lịch tiêm nhắc lại.

### 📅 Lập Lịch Khám & Xử Lý Hàng Đợi (Smart Queue)
- **Double-booking Prevention:** Thuật toán tính toán khe thời gian trống (Slot Calculation) chặt chẽ, loại bỏ hoàn toàn tình trạng đặt trùng lịch (back-to-back scheduling).
- **Real-time Queue Management:** Lễ tân check-in khách hàng, hệ thống tự động đẩy bệnh nhi vào hàng chờ điện tử của bác sĩ tương ứng theo thời gian thực.
- Khách hàng có thể theo dõi trạng thái ca khám (Đang chờ, Đang khám, Đã khám xong, Chờ thanh toán) ngay trên điện thoại.

### 💊 Quản Lý Kho Dược Cực Kỳ Chặt Chẽ
- Danh mục Thuốc và Vắc-xin được quản lý theo từng Lô (Batches) và Hạn sử dụng (Expiration Dates).
- Hệ thống **tự động trừ tồn kho** ngay khi bác sĩ kê đơn hoặc thực hiện tiêm phòng. Cảnh báo tự động khi xuất hiện lô thuốc sắp hết hạn hoặc hết hàng.

### 📊 Thống Kê & Báo Cáo Thông Minh (Admin Dashboard)
- Tích hợp biểu đồ trực quan (Chart.js) theo dõi Doanh thu, Số ca khám và Tỷ lệ tăng trưởng theo thời gian thực.
- Báo cáo chi tiết hiệu suất làm việc của từng bác sĩ, tỷ lệ dịch vụ được sử dụng (Khám bệnh vs Spa vs Tiêm phòng).

### 🤖 Trợ Lý Ảo Tích Hợp AI (AI Chatbot)
- Tích hợp Gemini AI LLM trực tiếp vào hệ thống để tư vấn khách hàng 24/7 về dinh dưỡng, lịch tiêm phòng, sơ cứu cơ bản trước khi đưa thú cưng tới phòng khám.

---

## 🛠️ Kiến Trúc & Công Nghệ Cốt Lõi

MyPetClinic được phát triển dựa trên bộ công nghệ tiên tiến, đáp ứng quy mô (scalability) và độ bảo mật (security) cao.

### ⚙️ Backend (Core API)
- **Framework:** C# .NET 8 WebAPI.
- **Architecture:** Clean Architecture 4 lớp (Domain, Application, Infrastructure, WebApi) đảm bảo tính độc lập của Business Logic (SOLID, DRY).
- **Database:** PostgreSQL kết hợp Entity Framework Core. Tối ưu truy vấn bằng LINQ AsNoTracking.
- **Authentication & Security:** 
  - JWT (JSON Web Tokens) lưu trữ qua HttpOnly Cookies (chống tấn công XSS). 
  - Phân quyền cứng RBAC (Role-Based Access Control) chống IDOR.
  - BCrypt Hash passwords.
- **Background Jobs:** Quartz.NET tích hợp IHostedService để tự động chạy các tác vụ định kỳ (gửi email nhắc lịch, dọn dẹp hàng đợi).

### 🎨 Frontend (Client App)
- **Framework:** Vue 3 (Composition API) + Vite (siêu tốc).
- **State Management:** Pinia Store để quản lý trạng thái luồng bệnh án và giỏ hàng.
- **Styling:** CSS HSL Variables kết hợp phong cách thiết kế **Premium Glassmorphism** (Mờ kính, viền nổi 3D, Gradient dịu mắt).
- **Routing:** Vue Router tích hợp Navigation Guards chặn truy cập trái phép vào các Dashboard quản trị.
- **Data Fetching:** Axios với Interceptors tự động đính kèm thông tin Credentials.

---

## 👥 Thành Viên Phát Triển

*   **Nam** (Software Architecture, AI Integration, Backend Core)
*   **Phương** (Frontend Development, Vue UI/UX Design)
*   **Hạnh** (Business Analysis, Product Requirement Document)
*   **Lâm** (Quality Assurance, System Testing)

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

### Yêu cầu hệ thống:
- .NET 8 SDK
- Node.js (v18+)
- PostgreSQL (v15+)

### 1. Thiết lập & Khởi chạy Backend
```bash
# Di chuyển vào thư mục WebApi
cd backend/src/WebApi

# Cài đặt các Entity Framework CLI (nếu chưa có)
dotnet tool install --global dotnet-ef

# Áp dụng Database Migrations để tạo bảng
dotnet ef database update

# Khởi chạy server API (Mặc định chạy ở cổng 5285)
dotnet run
```

### 2. Thiết lập & Khởi chạy Frontend
```bash
# Di chuyển vào thư mục Frontend
cd frontend

# Cài đặt toàn bộ thư viện npm
npm install

# Khởi chạy Vite Dev Server (Mặc định chạy ở cổng 5173)
npm run dev
```

### 3. Thông tin Đăng nhập Mặc định (Tùy chọn)
- **Admin:** `admin@mypetclinic.com` / `123456`
- **Bác sĩ:** `bacsiha@gmail.com` / `123456`
- **Lễ tân:** `letan@mypetclinic.com` / `123456`
- **Khách hàng:** Tự đăng ký tài khoản mới trên giao diện.

---
*© 2026 MyPetClinic. Nền tảng số hóa quản lý phòng khám thú y.*
