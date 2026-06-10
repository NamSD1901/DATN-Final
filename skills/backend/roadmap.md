# 📅 Lộ Trình Học Tập & Phát Triển Backend (MyPetClinic)

Lộ trình học tập chi tiết giúp các thành viên dự án nâng cấp kỹ năng backend từ cơ bản lên nâng cao trong bối cảnh thực tế của dự án phòng khám thú y MyPetClinic.

---

## 📅 Timeline Cho Backend Developer (Ví dụ: Nam - Backend Lead)

### Tuần -2 đến 0 (Trước Sprint 1)
- **Ngày 1-3:** Ôn tập C# Advanced (LINQ, Async/Await, Generics, Pattern Matching).
- **Ngày 4-6:** Học Clean Architecture (Domain-Driven Design concepts, Layers Separation).
- **Ngày 7-8:** Thực hành Entity Framework Core nâng cao & tích hợp PostgreSQL.
- **Ngày 9-10:** Setup Solution cấu trúc Clean Architecture, cấu hình CI/CD và Docker containerization.

### Sprint 1-2
- Thực hành Cookie Authentication & Google OAuth.
- Xây dựng Base Repository + Unit of Work mẫu cho dự án.
- Thiết lập khung Unit Test (xUnit + Moq).

---

## 📝 Bài Tập Thực Hành Đề Xuất

### Bài tập 1: Xử lý Logic Nghiệp Vụ và Linq
- **Mục tiêu:** Nâng cao kỹ năng C# Fundamentals & LINQ.
- **Yêu cầu:** Viết helper tính tuổi chính xác cho thú cưng từ DateOfBirth và truy vấn danh sách thú cưng sắp tới lịch tái chủng tiêm phòng bằng LINQ.

### Bài tập 2: Tách biệt Layer theo Clean Architecture
- **Mục tiêu:** Áp dụng SOLID & Clean Architecture.
- **Yêu cầu:** Refactor một endpoint API từ Controller "fat" (nhiều logic trực tiếp gọi DbContext) sang kiến trúc 4 lớp sử dụng Domain Entity, Application Service/Command và Infrastructure Repository.

### Bài tập 3: Giao dịch an toàn với Unit of Work
- **Mục tiêu:** Mastery Unit of Work & EF Core Transactions.
- **Yêu cầu:** Xây dựng luồng tạo Đơn đặt lịch (Booking) đồng thời xuất hóa đơn tạm tính (Billing) và trừ số lượng thuốc/vật tư tiêu hao trong kho. Nếu một trong các bước thất bại, toàn bộ giao dịch phải được rollback.

---

## 🏆 Tiêu Chí Đánh Giá Hoàn Thành

| Cấp độ | Tiêu chí kỹ thuật bắt buộc | Phương thức đánh giá |
|:---|:---|:---|
| **Foundation (Nền tảng)** | - Sử dụng tốt các cú pháp C# hiện đại.<br>- Hiểu và áp dụng đúng Dependency Injection.<br>- Viết code async không gây deadlock. | Code Review 1-1 |
| **Core (Cốt lõi)** | - Tạo API RESTful đúng chuẩn HTTP Methods/Status codes.<br>- Viết câu lệnh EF Core tối ưu, hạn chế N+1 query.<br>- Thực hiện migrations thành công. | Pull Request review & Run demo |
| **Advanced (Nâng cao)** | - Triển khai đúng các lớp theo Clean Architecture.<br>- Sử dụng FluentValidation để kiểm tra dữ liệu đầu vào.<br>- Xử lý xác thực/phân quyền đúng vai trò. | Hệ thống chạy thử nghiệm |
| **Expert (Chuyên gia)** | - Viết Unit test đạt độ phủ (Coverage) > 70%.<br>- Tích hợp thành công dịch vụ chạy nền Hangfire.<br>- Tối ưu hóa Database Index và xử lý tranh chấp dữ liệu (Concurrency). | Test report & Performance profiling |
