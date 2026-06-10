# 🚀 ĐĂNG NHẬP MỘT CHẠM QUA GOOGLE (GOOGLE OAUTH LOGIN)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - 03)

Chào mừng bạn đến với tài liệu kỹ thuật chi tiết nhất về phân hệ **Đăng nhập một chạm qua Google (Google Sign-In)** của hệ thống quản lý phòng khám thú y **MyPetClinic**. Phân hệ này cung cấp giải pháp đăng nhập và đăng ký tài khoản tự động (Auto-Provisioning) an toàn, nhanh chóng chỉ với 1 click, giải phóng khách hàng khỏi việc phải ghi nhớ nhiều mật khẩu phức tạp.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU CHI TIẾT

Tài liệu thiết kế chi tiết được phân rã thành các tệp chuyên biệt dưới đây để đảm bảo tính độc lập và quản lý kiến trúc chặt chẽ:

1.  **[Product Requirements Document (PRD)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/PRD.md)**
    *   Mục tiêu nghiệp vụ, chân dung người dùng (Anh Minh bận rộn, Cô Hoa lớn tuổi), User Stories.
    *   Phạm vi tính năng (In-Scope/Out-of-Scope) và các yêu cầu phi chức năng (NFRs) về thời gian xử lý.
2.  **[Technical Specification](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/TECHNICAL_SPEC.md)**
    *   Sơ đồ luồng xác thực tuần tự (Sequence Diagram) từ Vue SDK đến Google Server và Web API Backend.
    *   Đặc tả DTOs validation, cấu hình Google Client ID và API nhận token.
3.  **[Core Business Logic Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/01-core-logic.md)**
    *   Logic giải mã và xác thực token sử dụng thư viện chính thức `Google.Apis.Auth`.
    *   Cơ chế tự động tạo tài khoản mới (Auto-Provisioning) ở trạng thái đã kích hoạt.
4.  **[UI/UX Design Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/02-ui-ux.md)**
    *   Sơ đồ nút Đăng nhập Google dạng **ASCII Art Mockup** tuân thủ nguyên tắc thương hiệu Google.
    *   Mã nguồn Vue 3 nạp động Google SDK và render nút bấm an toàn.
5.  **[State Management (Pinia)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/03-state-management.md)**
    *   Mã nguồn TypeScript Pinia Store (`useAuthStore.ts`) xử lý action `loginWithGoogle`.
    *   Đồng bộ luồng dữ liệu phiên làm việc và chuyển đổi routing sau khi đăng nhập thành công.
6.  **[Infrastructure & Security](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/04-infrastructure.md)**
    *   Đặc tả cấu hình dự án trên Google Cloud Console (JavaScript Origins, Authorized Redirect URIs).
    *   Lá chắn an ninh chống Replay Attack (xác thực `aud` Audience và `exp` Expiry) và Rate Limiting.
7.  **[API Reference Details](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/API_REFERENCE.md)**
    *   Đặc tả API Contract: chi tiết URL, HTTP Method, Request/Response payload mẫu cho các trường hợp thành công và lỗi (400, 401, 422, 429).
8.  **[Behavioral Specification (FSM)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/BEHAVIOR_SPEC.md)**
    *   Mô tả máy trạng thái hữu hạn bằng sơ đồ Mermaid và diễn giải luồng đi của giao diện.
    *   Xử lý lỗi biên (popup bị chặn adblocker, lỗi timeout mạng kết nối server Google).
9.  **[UX Flow Walks](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/UX_FLOW.md)**
    *   Hành trình trải nghiệm người dùng chi tiết từng bước (User Journey Map) từ lúc click nút Google đến màn hình làm việc.
10. **[User & Developer Documentation](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/DOCUMENTATION.md)**
    *   Hướng dẫn đăng nhập nhanh cho khách hàng và lập trình viên debug cài đặt môi trường cục bộ.
11. **[Implementation Plan & Testing Strategy](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-03-google-login/plan.md)**
    *   Lộ trình 5 giai đoạn phát triển và bộ kiểm thử (Unit Tests) sử dụng Moq giả lập chữ ký token Google.

---

## 🏛️ TỔNG QUAN KIẾN TRÚC MÁY TRẠNG THÁI XÁC THỰC CHÉO (CROSS-VALIDATION)

Để đảm bảo tính an toàn tối đa cho hệ thống, MyPetClinic không bao giờ trực tiếp tạo phiên dựa vào ID Token thô gửi từ Frontend. Toàn bộ Token bắt buộc phải gửi lên Backend để thực hiện cuộc xác thực chữ ký chéo (Cross-Validation) với khóa công khai từ Google:

```
[Vue 3 Client] --(1. Click đăng nhập)--> [Google SDK Popup]
      |                                           |
      |                                    (2. Xác nhận gmail)
      v                                           v
[Nhận ID Token Google] <------------------ [Google Auth Server]
      |
(3. Gửi ID Token qua HTTP POST /api/account/google-login)
      v
[WebApi Controller] ---> [GoogleJsonWebSignature.ValidateAsync] (4. Fetch Google Keys & Check Signature)
      |                                           |
      |                                    (5. Chữ ký hợp lệ)
      v                                           v
[Kiểm tra Email trong DB] ----------------> [Cấp JWT Token MyPetClinic] ---> [200 OK trả về Client]
```
Kiến trúc này giúp triệt tiêu hoàn toàn khả năng tin tặc dùng token giả hoặc token đã bị sửa đổi payload để đánh cắp phiên làm việc.
