# 🐾 MyPetClinic - Hệ Thống Quản Lý Phòng Khám Thú Y

Dự án **MyPetClinic** là một hệ thống toàn diện hỗ trợ quản lý và vận hành phòng khám thú y, được phát triển bởi nhóm sinh viên: **Nam, Phương, Hạnh, Lâm**.

Hệ thống được thiết kế theo mô hình **Clean Architecture** sử dụng **.NET 8** làm Backend, **PostgreSQL** lưu trữ cơ sở dữ liệu và **Vue 3 (Vite + TypeScript)** làm Frontend, đem lại trải nghiệm mượt mà, bảo mật cao và khả năng mở rộng tốt.

---

## 🎯 Chủ Đề Dự Án

Chủ đề chính của dự án là **"Chuyển đổi số trong quản lý dịch vụ chăm sóc sức khỏe thú cưng"**. Dự án số hóa toàn bộ quy trình vận hành truyền thống của một phòng khám thú y, từ khâu tiếp đón khách hàng, xếp hàng đợi, khám chữa bệnh, tiêm phòng đến quản lý kho thuốc, hóa đơn và chăm sóc khách hàng sau dịch vụ.

---

## 💡 Ứng Dụng Thực Tiễn của Dự Án

Hệ thống cung cấp các giải pháp chuyên biệt cho từng nhóm đối tượng sử dụng:

### 1. Dành cho Khách hàng (Chủ nuôi thú cưng)
*   **Đăng ký & Quản lý Thú cưng:** Đăng ký tài khoản cá nhân, lưu trữ hồ sơ chi tiết (ảnh, giống, tuổi, cân nặng, tiền sử bệnh án) của từng thú cưng.
*   **Đặt lịch khám trực tuyến:** Chọn ngày, giờ khám, chọn dịch vụ chuyên biệt (Khám điều trị, Spa & Grooming, Tắm cắt lông, Tiêm phòng vaccine) và bác sĩ mong muốn một cách chủ động.
*   **Theo dõi sổ sức khỏe & tiêm phòng:** Theo dõi lịch sử khám bệnh, đơn thuốc đã kê và lịch nhắc hẹn tiêm chủng định kỳ.
*   **Trợ lý ảo AI Chatbot:** Tư vấn nhanh về cách chăm sóc sức khỏe thú cưng, dinh dưỡng và cách xử lý sơ cứu ban đầu trước khi mang tới phòng khám.

### 2. Dành cho Lễ tân (Receptionist)
*   **Quản lý lịch hẹn & Hàng chờ:** Check-in khi khách hàng đến, tự động cấp số thứ tự vào phòng khám (Queue) dựa trên lịch hẹn.
*   **Tạo hóa đơn & Thanh toán:** Tự động tổng hợp chi phí dịch vụ và tiền thuốc từ bệnh án của Bác sĩ để tạo hóa đơn, hỗ trợ xác nhận thanh toán viện phí nhanh chóng.

### 3. Dành cho Bác sĩ Thú y (Doctor)
*   **Xem lịch trực & Danh sách ca khám:** Theo dõi danh sách thú cưng đang đợi khám theo số thứ tự của mình.
*   **Khám và kê đơn điện tử:** Tạo hồ sơ bệnh án chi tiết (Medical Record), chẩn đoán bệnh, kê đơn thuốc điện tử (Prescription) trực tiếp trên hệ thống. Hệ thống tự động khấu trừ số lượng thuốc tương ứng trong kho.

### 4. Dành cho Quản trị viên (Admin)
*   **Báo cáo & Thống kê (Dashboard):** Xem biểu đồ doanh thu theo ngày/tháng/năm, số lượng thú cưng khám, các dịch vụ được sử dụng nhiều nhất để đưa ra chiến lược kinh doanh phù hợp.
*   **Quản lý danh mục:** Quản lý thông tin nhân viên (bác sĩ, lễ tân), các loại thuốc, vaccine, bảng giá dịch vụ và các bài viết cẩm nang y tế thú y trên website.

---

## 🛠️ Công Nghệ Sử Dụng

*   **Backend:** C# .NET 8 (Clean Architecture, WebAPI, Entity Framework Core)
*   **Database:** PostgreSQL (Mã hóa mật khẩu bằng BCrypt, Xác thực bảo mật JWT Token)
*   **Frontend:** Vue 3 (Composition API, Vite, TypeScript, Axios, Vue Router, TailwindCSS/Vanilla CSS)
*   **AI:** Tích hợp Gemini API phục vụ Chatbot hỗ trợ y tế thú y thông minh.

---

## 👥 Thành Viên Thực Hiện
*   **Nam**
*   **Phương**
*   **Hạnh**
*   **Lâm**

---

## 🚀 Hướng Dẫn Khởi Chạy Dự Án

### 1. Khởi chạy Backend (.NET Core)
```bash
cd backend/src/WebApi
dotnet run
```

### 2. Khởi chạy Frontend (Vue 3)
```bash
cd frontend
npm install
npm run dev
```
