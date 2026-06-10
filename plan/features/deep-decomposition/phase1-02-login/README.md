# 🚀 HỆ THỐNG ĐĂNG NHẬP PHÂN QUYỀN (LOGIN SYSTEM)
## 📝 TÀI LIỆU KHẢO SÁT & THIẾT KẾ CHI TIẾT (PHASE 1 - 02)

Chào mừng bạn đến với tài liệu kỹ thuật chi tiết nhất về phân hệ **Đăng nhập hệ thống** và **Phân quyền người dùng (RBAC)** của ứng dụng phòng khám thú y **MyPetClinic**. Phân hệ này chịu trách nhiệm xác thực danh tính người dùng (khách hàng, bác sĩ, lễ tân, thu ngân, admin), cấp chứng chỉ số JWT Token bảo mật cao và điều phối định tuyến giao diện làm việc tương ứng.

---

## 📌 BẢN ĐỒ MỤC LỤC & TÀI LIỆU CHI TIẾT

Tài liệu thiết kế chi tiết được phân rã thành các tệp chuyên biệt dưới đây để đảm bảo tính độc lập và dễ bảo trì:

1.  **[Product Requirements Document (PRD)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/PRD.md)**
    *   Mục tiêu nghiệp vụ, chân dung người dùng (Vy - Khách hàng, Bác sĩ Minh), User Stories.
    *   Phân tích phạm vi phát triển (In-Scope/Out-of-Scope) và yêu cầu phi chức năng (NFRs) về độ trễ API.
2.  **[Technical Specification](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/TECHNICAL_SPEC.md)**
    *   Sơ đồ luồng xác thực tuần tự (Sequence Diagram) từ Client qua Controller đến AuthService.
    *   Cấu trúc payload của mã JWT Token, sơ đồ các Claims và DTOs validation.
3.  **[Core Business Logic Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/01-core-logic.md)**
    *   Logic đối chiếu mật khẩu BCrypt và sinh JWT Bearer Token tại tầng Infrastructure.
    *   Cơ chế khóa tài khoản tạm thời 15 phút (Lockout policy) khi nhập sai mật khẩu quá 5 lần.
4.  **[UI/UX Design Spec](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/02-ui-ux.md)**
    *   Sơ đồ giao diện Đăng nhập dạng **ASCII Art Mockup** có tích hợp nút ẩn/hiện mật khẩu.
    *   Bảng mã màu CSS HSL Glassmorphism, hiệu ứng visual feedback rung lắc (shake animation) khi đăng nhập sai.
5.  **[State Management (Pinia)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/03-state-management.md)**
    *   Mã nguồn TypeScript Pinia Store (`useAuthStore.ts`) quản lý phiên đăng nhập và token.
    *   Quy tắc chặn định tuyến tự động (Router Guards) dựa trên vai trò Role của người dùng.
6.  **[Infrastructure & Security](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/04-infrastructure.md)**
    *   Bảo mật kênh truyền mã hóa HTTPS/SSL, chính sách lưu trữ JWT Token trong HTTP-Only Cookie.
    *   Lá chắn bảo mật Rate Limiting chống Brute-force/DDoS và cấu hình CORS.
7.  **[API Reference Details](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/API_REFERENCE.md)**
    *   Đặc tả API Contract: chi tiết URL, HTTP Method, Request/Response payload mẫu cho các trường hợp thành công và lỗi (400, 401, 422, 429).
8.  **[Behavioral Specification (FSM)](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/BEHAVIOR_SPEC.md)**
    *   Mô tả máy trạng thái hữu hạn bằng sơ đồ Mermaid và diễn giải luồng đi của giao diện.
    *   Xử lý các lỗi biên (nhập sai mật khẩu liên tục, xử lý mất mạng đột ngột).
9.  **[UX Flow Walks](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/UX_FLOW.md)**
    *   Hành trình trải nghiệm người dùng chi tiết từng bước (User Journey Map) với các hiệu ứng chuyển đổi mượt mà.
10. **[User & Developer Documentation](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/DOCUMENTATION.md)**
    *   Hướng dẫn đăng nhập hệ thống dành cho người dùng và nhân viên phòng khám.
    *   Hướng dẫn debug mở khóa nhanh tài khoản bằng SQL và sử dụng token xác thực trong Postman/Swagger.
11. **[Implementation Plan & Testing Strategy](file:///e:/DATN/MyPetClinic/plan/features/deep-decomposition/phase1-02-login/plan.md)**
    *   Lộ trình phát triển 6 giai đoạn và kịch bản kiểm thử (Test Cases Suite) bao gồm Integration Test Web API mẫu.

---

## 🏛️ TỔNG QUAN HÀNG RÀO PHÂN QUYỀN ĐIỀU PHỐI (ROLE REDIRECTION GATE)

Hệ thống sau khi xác minh đúng thông tin đăng nhập sẽ tự động trích xuất thuộc tính `Role` để thực hiện điều phối luồng làm việc cho nhân viên phòng khám:

```
[Đăng nhập thành công] ---> [Lưu JWT Token] ---> [Router Guard quét Claim Role]
                                                             |
                 +-------------------+-----------------+-----+-----------------+
                 |                   |                 |                       |
            [Role: BacSi]      [Role: LeTan]     [Role: ThuNgan]       [Role: KhachHang]
                 |                   |                 |                       |
                 v                   v                 v                       v
          [/portal/doctor]    [/portal/reception]  [/portal/cashier]      [/dashboard]
```
Mã JWT Token được lưu ở Client sẽ tự động đính kèm vào Authorization Header của các HTTP Requests tiếp theo để vượt qua bộ lọc phân quyền tại Backend Web API.
