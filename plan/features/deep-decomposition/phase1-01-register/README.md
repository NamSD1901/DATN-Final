# 🚀 ĐĂNG KÝ TÀI KHOẢN KHÁCH HÀNG (REGISTER ACCOUNT)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - 01)

Chào mừng bạn đến với tài liệu kỹ thuật chi tiết nhất về phân hệ **Đăng ký tài khoản khách hàng** và **Xác thực mã OTP kích hoạt** của ứng dụng **MyPetClinic**. Phân hệ này là cánh cổng đầu tiên tiếp nhận chủ nuôi thú cưng đăng nhập vào hệ thống, thiết lập môi trường bảo mật, ngăn chặn tài khoản giả mạo/rác bằng mã OTP gửi qua Email.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU CHI TIẾT

Tài liệu thiết kế chi tiết được phân rã thành các tệp chuyên biệt dưới đây để đảm bảo tính độc lập và dễ theo dõi:

1.  **[Product Requirements Document (PRD)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/PRD.md)**
    *   Mục tiêu nghiệp vụ, chân dung khách hàng nuôi thú cưng (Personas), User Stories.
    *   Phân tách rõ ranh giới phát triển (In-Scope/Out-of-Scope) và yêu cầu phi chức năng (NFRs) về thời gian phản hồi.
2.  **[Technical Specification](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/TECHNICAL_SPEC.md)**
    *   Sơ đồ luồng xử lý tuần tự (Sequence Diagram) từ Client Vue 3 qua Web API .NET Core đến PostgreSQL.
    *   Cấu trúc thực thể `User`, Database Schema Migration, các DTOs xác thực Model Validation ở API.
3.  **[Core Business Logic Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/01-core-logic.md)**
    *   Logic băm mật khẩu bảo mật (BCrypt workFactor:11) và bộ sinh OTP bảo mật mã hóa (`RandomNumberGenerator`).
    *   Cơ chế phòng ngừa lỗi ghi dữ liệu đồng thời (Race Condition) ở mức Database.
4.  **[UI/UX Design Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/02-ui-ux.md)**
    *   Sơ đồ giao diện biểu mẫu Đăng ký và Nhập OTP dạng **ASCII Art Mockup**.
    *   Bảng mã màu CSS HSL Glassmorphism, hiệu ứng visual feedback khi nhập sai thông tin.
5.  **[State Management (Pinia)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/03-state-management.md)**
    *   Mã nguồn Pinia Store (`useRegisterStore.ts`) đầy đủ State, Getters, Actions bằng TypeScript.
    *   Quản lý bộ đếm ngược thời gian (countdown timer) cho phép gửi lại mã OTP.
6.  **[Infrastructure & Security](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/04-infrastructure.md)**
    *   Cấu hình gửi email SMTP HTML bất đồng bộ bằng thư viện MailKit.
    *   Các lá chắn bảo mật: Rate Limiting chống spam và cấu hình CORS chặt chẽ.
7.  **[API Reference Details](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/API_REFERENCE.md)**
    *   Tài liệu tích hợp API: chi tiết URL, HTTP Method, Request Body, Response JSON thành công và lỗi (400, 422, 429).
8.  **[Behavioral Specification (FSM)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/BEHAVIOR_SPEC.md)**
    *   Mô tả máy trạng thái hữu hạn bằng sơ đồ Mermaid và diễn giải luồng đi của giao diện.
    *   Các trường hợp lỗi biên (nhập sai OTP quá 5 lần, xử lý mất mạng đột ngột).
9.  **[UX Flow Walks](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/UX_FLOW.md)**
    *   Hành trình trải nghiệm người dùng chi tiết từng bước (User Journey Map) với các hiệu ứng chuyển đổi mượt mà.
10. **[User & Developer Documentation](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/DOCUMENTATION.md)**
    *   Hướng dẫn đăng ký tài khoản cho khách hàng.
    *   Hướng dẫn debug lấy mã OTP nhanh qua Console Log hoặc Mailtrap dành cho lập trình viên.
11. **[Implementation Plan & Testing Strategy](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-01-register/plan.md)**
    *   Lộ trình 6 bước triển khai tính năng và bộ kịch bản kiểm thử (Test Cases Suite) bao gồm Unit Test C# mẫu.

---

## 🏛️ TỔNG QUAN KIẾN TRÚC LUỒNG XỬ LÝ (SYSTEM OVERVIEW)

Tính năng này được xây dựng tuân thủ nghiêm ngặt mô hình kiến trúc **Clean Architecture** và nguyên tắc lập trình **SOLID**:

```
[Vue 3 Client] --(HTTP POST: fullName, email, phone, pass)--> [AccountController]
                                                                     |
                                                           [AuthService (Application)]
                                                                     |
       +---------------------------------------------+---------------+---------------------+
       |                                             |                                     |
[BCrypt Hashing]                              [OTP Generator]                       [PostgreSQL DB]
(Mã hóa password)                            (Sinh mã OTP 6 số)                   (Lưu user ở dạng IsActive=false)
                                                     |
                                            [EmailService (SMTP)]
                                          (Gửi email OTP bất đồng bộ)
```
- Khi tài khoản được tạo thành công, người dùng nhận được thư điện tử chứa mã OTP 6 số.
- Khi người dùng nhập OTP kích hoạt thành công trên màn hình, thuộc tính `IsActive` chuyển thành `true`, trường `ActivationOtp` và `OtpExpiry` được dọn dẹp sạch sẽ để đảm bảo an toàn.
