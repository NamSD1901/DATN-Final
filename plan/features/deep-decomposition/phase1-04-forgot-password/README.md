# 🔑 KHÔI PHỤC MẬT KHẨU (FORGOT PASSWORD RECOVERY)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - 04)

Chào mừng bạn đến với tài liệu kỹ thuật chi tiết nhất về phân hệ **Khôi phục mật khẩu (Forgot Password)** của ứng dụng phòng khám thú y **MyPetClinic**. Phân hệ này chịu trách nhiệm cung cấp luồng khôi phục tự phục vụ 3 bước an toàn thông qua mã OTP Email, tự động băm mật khẩu mới bằng BCrypt và giải phóng trạng thái khóa tài khoản (lockout) cho người dùng.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU CHI TIẾT

Tài liệu thiết kế chi tiết được phân rã thành các tệp chuyên biệt dưới đây để đảm bảo cấu trúc kiến trúc Clean và dễ bảo trì:

1.  **[Product Requirements Document (PRD)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/PRD.md)**
    *   Mục tiêu nghiệp vụ, chân dung khách hàng nuôi thú cưng (Vy - Chủ Poodle), User Stories.
    *   Phạm vi tính năng (In-Scope/Out-of-Scope) và các yêu cầu phi chức năng (NFRs) về thời gian và độ bảo mật OTP.
2.  **[Technical Specification](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/TECHNICAL_SPEC.md)**
    *   Sơ đồ luồng xử lý tuần tự (Sequence Diagram) 3 bước khôi phục mật khẩu.
    *   Cấu trúc thực thể `User`, Database Schema Migration bổ sung và DTOs validation.
3.  **[Core Business Logic Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/01-core-logic.md)**
    *   Logic gửi mã OTP khôi phục, đặt lại mật khẩu mới, băm BCrypt mật khẩu.
    *   Cơ chế phòng chống rò rỉ email (User Enumeration Defense) và giới hạn 3 lần nhập sai OTP khôi phục.
4.  **[UI/UX Design Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/02-ui-ux.md)**
    *   Sơ đồ giao diện 3 bước dạng **ASCII Art Mockup** và hiệu ứng trượt chuyển tiếp (Slide transition).
    *   Visual feedback báo mật khẩu không trùng khớp và logic tự động chuyển ô nhập OTP.
5.  **[State Management (Pinia)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/03-state-management.md)**
    *   Mã nguồn TypeScript Pinia Store (`useForgotPasswordStore.ts`) quản lý wizard và gọi các API khôi phục.
    *   Quản lý bộ đếm ngược 60 giây (countdown timer) cho nút gửi lại OTP.
6.  **[Infrastructure & Security](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/04-infrastructure.md)**
    *   Cấu hình template email khôi phục mật khẩu HTML gửi qua SMTP MailKit.
    *   Thiết lập Rate Limiting chặn gửi email spam liên tục và cơ chế một lần sử dụng của OTP (One-Time Use).
7.  **[API Reference Details](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/API_REFERENCE.md)**
    *   Đặc tả API Contract: URL, HTTP Method, Request/Response payload mẫu cho các trường hợp thành công và lỗi (400, 422, 429).
8.  **[Behavioral Specification (FSM)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/BEHAVIOR_SPEC.md)**
    *   Mô tả máy trạng thái hữu hạn bằng sơ đồ Mermaid và diễn giải luồng đi của giao diện.
    *   Xử lý các lỗi biên (nhập sai mật khẩu liên tục, xử lý mất mạng đột ngột).
9.  **[UX Flow Walks](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/UX_FLOW.md)**
    *   Hành trình trải nghiệm người dùng chi tiết từng bước (User Journey Map) qua các màn hình wizard.
10. **[User & Developer Documentation](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/DOCUMENTATION.md)**
    *   Hướng dẫn khôi phục mật khẩu dành cho người dùng và lập trình viên debug lấy OTP nhanh qua Console Log.
11. **[Implementation Plan & Testing Strategy](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-04-forgot-password/plan.md)**
    *   Lộ trình 5 giai đoạn phát triển và bộ kiểm thử đơn vị (Unit Tests) sử dụng FluentAssertions kiểm tra giới hạn nhập sai OTP.

---

## 🏛️ LUỒNG ĐỒNG BỘ ĐỒNG THỜI MỞ KHÓA TÀI KHOẢN (AUTO-UNLOCK FLOW)

Khi một tài khoản bị tạm khóa do đăng nhập sai mật khẩu quá 5 lần (AccessFailedCount >= 5, LockoutEnd có giá trị), luồng Khôi phục mật khẩu cung cấp một cơ chế tự động mở khóa (Auto-Unlock) thông minh và an toàn:

```
[Nhập đúng OTP Reset] ---> [Gửi mật khẩu mới] ---> [Mã hóa BCrypt mật khẩu mới]
                                                               |
                                                  (Cập nhật vào Database)
                                                               |
                                     +-------------------------+-------------------------+
                                     |                                                   |
                     [Đặt PasswordHash mới]                                    [Reset trạng thái khoá]
                                                                               - AccessFailedCount = 0
                                                                               - LockoutEnd = null
                                                                               - ResetOtpFailedAttempts = 0
```
Cơ chế đồng bộ này giúp tài khoản người dùng ngay lập tức trở lại trạng thái hoạt động bình thường (`IsActive = true`) sau khi reset mật khẩu thành công, mang lại trải nghiệm tối ưu nhất cho khách hàng của phòng khám.
