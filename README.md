# 🐾 MyPetClinic - Hệ Thống Quản Lý Phòng Khám Thú Y Toàn Diện (ERP)

Dự án **MyPetClinic** là một giải pháp phần mềm toàn diện (ERP) hỗ trợ chuyển đổi số quá trình quản lý và vận hành phòng khám thú y. Dự án được nghiên cứu và phát triển bởi nhóm sinh viên: **Nam, Phương, Hạnh, Lâm**.

Được xây dựng trên nền tảng kiến trúc **Clean Architecture** mạnh mẽ kết hợp với phong cách thiết kế giao diện UI/UX hiện đại (**Premium Glassmorphism**), hệ thống đem lại một quy trình làm việc khép kín, tự động hóa cao và trải nghiệm người dùng xuất sắc cho cả Khách hàng lẫn Đội ngũ nhân viên y tế (Bác sĩ, Lễ tân, Quản trị viên).

---

## 🎯 1. Mục Tiêu & Chủ Đề Dự Án

Chủ đề cốt lõi của dự án là **"Chuyển đổi số toàn diện trong quản lý dịch vụ chăm sóc sức khỏe thú cưng"**. Mục tiêu của MyPetClinic là:
- **Paperless (Không giấy tờ):** Thay thế hoàn toàn sổ khám bệnh giấy truyền thống, số hóa luồng đi của bệnh án từ khâu **Tiếp đón (Lễ tân) -> Khám chữa bệnh (Bác sĩ) -> Thanh toán (Thu ngân) -> Chăm sóc khách hàng sau dịch vụ**.
- **Tự động hoá luồng nghiệp vụ:** Hệ thống tự động đẩy dữ liệu giữa các phòng ban theo thời gian thực (Real-time).
- **Giảm thiểu sai sót y khoa:** Quản lý chặt chẽ đơn thuốc, lượng tồn kho, cảnh báo thuốc hết hạn và lịch sử khám chữa bệnh.

---

## 🌟 2. Các Tính Năng Nổi Bật (Core Features)

### 🏥 2.1. Quản Lý Bệnh Án Chuẩn Y Khoa (SOAP)
- **Hồ sơ chuyên sâu:** Tích hợp mô hình ghi chép bệnh án **SOAP (Subjective - Objective - Assessment - Plan)** chuyên nghiệp giúp chuẩn hoá quy trình khám của bác sĩ.
- **Chi tiết từng khâu:** Cho phép bác sĩ nhập liệu chi tiết: Lý do khám (S), Khám lâm sàng & Sinh hiệu (O), Chẩn đoán y khoa sơ bộ & xác định (A), và Phác đồ điều trị kèm kê đơn thuốc (P).
- **Quản lý Tiêm phòng:** Số hóa hồ sơ tiêm phòng (Vaccination Records) tách biệt với bệnh sử chung, và tự động nhắc lịch tiêm nhắc lại.
- **Bệnh sử Accordion:** Giao diện xem lịch sử khám được thiết kế theo dạng Timeline đa tầng (Accordion), giúp bác sĩ dễ dàng tra cứu lại phác đồ cũ chỉ với 1 cú click.

### 📅 2.2. Lập Lịch Khám & Xử Lý Hàng Đợi (Smart Queue)
- **Thuật toán thông minh (Slot Calculation):** Tính toán khe thời gian trống chặt chẽ, loại bỏ hoàn toàn tình trạng đặt trùng lịch (Double-booking).
- **Điều phối Real-time:** Lễ tân check-in khách hàng, hệ thống tự động đẩy bệnh nhi vào hàng chờ điện tử của bác sĩ tương ứng theo thời gian thực (Real-time Queue Management).
- **Trải nghiệm liền mạch:** Khách hàng có thể theo dõi trạng thái ca khám (Đang chờ, Đang khám, Đã khám xong, Chờ thanh toán) ngay trên điện thoại di động.

### 💊 2.3. Quản Lý Kho Dược Cực Kỳ Chặt Chẽ
- Danh mục Thuốc và Vắc-xin được quản lý theo từng Lô (Batches) và Hạn sử dụng (Expiration Dates).
- Cơ chế **tự động trừ tồn kho (Auto-deduct)** ngay khi bác sĩ hoàn tất kê đơn hoặc thực hiện tiêm phòng. Cảnh báo tự động khi xuất hiện lô thuốc sắp hết hạn hoặc hết hàng.

### 💳 2.4. Thanh Toán & Đánh Giá Tự Động
- **Quản lý Hoá đơn tập trung:** Sinh hóa đơn dịch vụ tự động dựa trên đơn thuốc và gói dịch vụ khám đã sử dụng. Ghi nhận thời gian xuất hoá đơn cực kỳ chuẩn xác theo múi giờ địa phương.
- **Đánh giá dịch vụ (Auto Review):** Hệ thống tự động nhận diện các ca khám đã hoàn thành để kích hoạt Form đánh giá độ hài lòng. Tích hợp hiệu ứng pháo hoa (Confetti) hoành tráng để tăng tương tác người dùng.

### 🤖 2.5. Trợ Lý Ảo Tích Hợp AI (AI Chatbot)
- Tích hợp **Google Gemini AI LLM** trực tiếp vào hệ thống.
- Cung cấp tính năng tư vấn khách hàng 24/7 về chế độ dinh dưỡng, lịch tiêm phòng chuẩn, sơ cứu cơ bản trước khi đưa thú cưng tới phòng khám.

### 🔄 2.6. Background Workers (Cron Jobs)
- Hệ thống chạy ngầm tự động quét và gửi email/thông báo nhắc lịch khám, lịch tiêm phòng trước 24h và 1h.

---

## 🛠️ 3. Kiến Trúc & Công Nghệ Cốt Lõi

MyPetClinic được phát triển dựa trên bộ công nghệ tiên tiến, đáp ứng quy mô (scalability), hiệu năng cao và độ bảo mật (security) nghiêm ngặt.

### ⚙️ Backend (Core API)
- **Framework:** C# .NET 8 WebAPI.
- **Kiến trúc Clean Architecture 4 Lớp:** 
  - `Domain`: Chứa các Business Entities (Hồ sơ, Lịch khám, Hoá đơn).
  - `Application`: Chứa Business Logic (CQRS, Interfaces).
  - `Infrastructure`: Xử lý Database, External APIs (Email, Gemini).
  - `WebApi`: Endpoint Controllers, Middleware, Filter.
- **Database:** PostgreSQL kết hợp Entity Framework Core. Tối ưu truy vấn bằng cơ chế `AsNoTracking` và tính toán đẩy trực tiếp xuống Database.
- **Bảo mật (Security):** 
  - JWT (JSON Web Tokens) lưu trữ qua HttpOnly Cookies (chống tấn công XSS/CSRF). 
  - Phân quyền cứng RBAC (Role-Based Access Control). Cơ chế đối chiếu chủ sở hữu (Owner Check) chống lại các cuộc tấn công IDOR.
  - BCrypt Hash passwords.
- **Background Jobs:** Tích hợp `IHostedService` & `Quartz.NET` để tự động chạy các tác vụ định kỳ ngầm.

### 🎨 Frontend (Client SPA)
- **Framework:** Vue 3 (Composition API) + Vite (build tool siêu tốc).
- **Trạng thái (State Management):** Pinia Store để quản lý trạng thái luồng bệnh án, giỏ hàng, thông báo toàn cục.
- **Giao diện & UI/UX:** 
  - CSS Variables thuần kết hợp phong cách thiết kế **Premium Glassmorphism** (Giao diện mờ kính, viền nổi 3D, Gradient dịu mắt, Animation micro-interactions).
  - Component hóa mọi chi tiết (Modal, Timeline, Accordion, Toast).
- **Routing & Bảo mật Client:** Vue Router tích hợp Navigation Guards chặn truy cập trái phép vào các Dashboard quản trị tuỳ theo Roles.
- **Networking:** Axios với Interceptors tự động đính kèm thông tin Credentials và xử lý lỗi đồng bộ.

---

## 🤖 4. Kỷ Luật Phát Triển Bằng AI (AI-First & AI-Disciplined)

Dự án này là minh chứng cho việc áp dụng **Quy trình hợp tác Đa tác nhân AI (AI Multi-Agent Collaboration Playbook)**. Quá trình code được tự động hoá và tuân thủ các quy tắc sắt:
1. **Tổ Kiến Trúc & Nghiệp Vụ (Backend Agent):** Chuyên trách EF Core, Clean Architecture, Bảo mật IDOR.
2. **Tổ Giao Diện & Trải Nghiệm (Frontend Agent):** Chuyên trách Vue 3, Pinia, Premium UI/UX.
3. **Tổ Sản Phẩm (Product Agent):** Viết PRD, Phân rã Roadmap.
4. **Tổ Kiểm Soát Chất Lượng (QA Agent):** Chạy Unit Test (xUnit, FluentAssertions).

**Quy tắc phối hợp:** `Product` viết PRD -> `QA` viết Test Cases -> `Backend` xây dựng API -> `Frontend` dựng UI -> `QA` kiểm thử hồi quy. Mọi task đều phải cập nhật file tracking `progress.md` để đảm bảo không đứt gãy.

---

## 👥 5. Đội Ngũ Phát Triển (The Team)

Dự án được xây dựng với sự tâm huyết của 4 thành viên:
*   **Nam** - Software Architecture, AI Integration, Backend Core
*   **Phương** - Frontend Development, Vue UI/UX Design, CSS Animations
*   **Hạnh** - Business Analysis, Product Requirement Document
*   **Lâm** - Quality Assurance, System Testing & Deployment

---

## 🚀 6. Hướng Dẫn Cài Đặt & Khởi Chạy (Local Setup)

### Yêu cầu hệ thống:
- .NET 8 SDK
- Node.js (v18 trở lên)
- PostgreSQL (v15 trở lên)

### Bước 1: Thiết lập & Khởi chạy Backend
```bash
# Di chuyển vào thư mục WebApi
cd backend/src/WebApi

# Cài đặt công cụ Entity Framework CLI (nếu chưa có)
dotnet tool install --global dotnet-ef

# Áp dụng Database Migrations để tạo bảng và cấu trúc DB
dotnet ef database update

# Khởi chạy server API (Mặc định chạy ở cổng 5285)
dotnet run
```

### Bước 2: Thiết lập & Khởi chạy Frontend
```bash
# Di chuyển vào thư mục Frontend
cd frontend

# Cài đặt toàn bộ thư viện npm phụ thuộc
npm install

# Khởi chạy Vite Dev Server (Mặc định chạy ở cổng 5173)
npm run dev
```

### Bước 3: Tài khoản Đăng nhập Khởi tạo (Seeded Accounts)
- **Quản trị viên (Admin):** `admin@mypetclinic.com` / `123456`
- **Bác sĩ Thú y:** `bacsiha@gmail.com` / `123456`
- **Lễ tân (Receptionist):** `letan@mypetclinic.com` / `123456`
- **Khách hàng (Customer):** Bạn có thể tự đăng ký tài khoản mới trên giao diện ứng dụng.

---
*© 2026 MyPetClinic. Nền tảng số hóa quản lý phòng khám thú y dẫn đầu.*
