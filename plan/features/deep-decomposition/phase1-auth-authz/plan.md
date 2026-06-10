# 📝 Implementation Plan & Testing Strategy - Authentication & Authorization

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) phục vụ QA kiểm tra bảo mật API đăng ký/đăng nhập, chống tấn công dò mật khẩu và phân quyền RBAC.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự theo 4 giai đoạn khép kín nhằm bảo đảm chất lượng hệ thống:

### Giai đoạn 1: Database Setup & Security Infrastructure
*   **Bước 1.1:** Khởi tạo bảng `Roles` trong PostgreSQL CSDL và nạp dữ liệu tĩnh 4 vai trò chính (`admin`, `doctor`, `receptionist`, `customer`).
*   **Bước 1.2:** Thiết lập cơ chế băm mật khẩu `BCrypt` với Work Factor 11 trong thư viện bảo mật.
*   **Bước 1.3:** Cấu hình thư viện JWT Bearer Auth trong `Program.cs` và thiết lập các thông số ký Token hợp lệ.

### Giai đoạn 2: API Endpoints & Verification Logic
*   **Bước 2.1:** Xây dựng `AuthController` chứa các endpoints Đăng ký, Đăng nhập, Gửi lại OTP, Xác thực OTP.
*   **Bước 2.2:** Cài đặt logic sinh mã OTP 6 chữ số an toàn mật mã bằng `RandomNumberGenerator` và cơ chế lưu vào DB/Cache có thời hạn hết hạn 5 phút.
*   **Bước 2.3:** Tích hợp bộ thư viện Google OAuth Authentication để nhận diện IdToken và tự động đồng bộ tài khoản khách hàng mới.
*   **Bước 2.4:** Thiết lập chính sách tạm khóa tài khoản 15 phút (Lockout Policy) sau 5 lần nhập mật khẩu sai liên tiếp.

### Giai đoạn 3: Frontend Views & Pinia Integration
*   **Bước 3.1:** Viết Pinia store (`stores/auth.ts`) quản lý state phiên, cờ xác thực và Axios interceptor tự động gán Token vào header.
*   **Bước 3.2:** Dựng giao diện Đăng ký, Đăng nhập mờ kính Glassmorphism sang trọng, hỗ trợ xem trước mật khẩu và tự động chuyển ô nhập OTP.
*   **Bước 3.3:** Cài đặt Route Guards trong Vue Router chặn điều hướng và kiểm tra vai trò Role động.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Xác thực (Functional Testing Cases)

#### TC-FUN-01: Đăng nhập thành công và điều hướng đúng vai trò
*   **Mục tiêu:** Kiểm tra đăng nhập và tự động phân quyền điều hướng ở Client.
*   **Các bước thực hiện:**
    1. Đăng nhập bằng tài khoản Bác sĩ: `doctor@mypetclinic.com` / `Password123@`.
*   **Kết quả mong đợi:**
    *   API trả về mã `200 OK` chứa Token JWT có claim Role là `doctor`.
    *   Client SPA tự động chuyển hướng màn hình sang trang khám lâm sàng `/doctor/appointments`.
    *   Thử truy cập trang `/admin` bị chặn và hiển thị màn hình 403 Forbidden.

#### TC-FUN-02: Chặn tài khoản chưa kích hoạt đăng nhập
*   **Các bước thực hiện:**
    1. Đăng ký tài khoản mới nhưng không nhập mã OTP kích hoạt.
    2. Thử đăng nhập bằng tài khoản vừa tạo.
*   **Kết quả mong đợi:**
    *   API trả về mã `200 OK` nhưng chứa thuộc tính `requiresOtp: true`.
    *   Client SPA tự động chặn không cho vào Dashboard, giữ nguyên ở màn hình Xác thực OTP.

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Kiểm thử chính sách Lockout (Nhập sai mật khẩu liên tiếp)
*   **Mục tiêu:** Bảo vệ tài khoản khỏi các bot dò quét mật khẩu hàng loạt.
*   **Các bước thực hiện:**
    1. Nhập sai mật khẩu liên tục 5 lần cho tài khoản `customer@gmail.com`.
    2. Thực hiện lần đăng nhập thứ 6 với mật khẩu đúng.
*   **Kết quả mong đợi:**
    *   Tại lần thứ 6, API vẫn từ chối xử lý và trả về mã lỗi `400 Bad Request` kèm thông báo tài khoản đã bị khóa.
    *   Đồng hồ đếm ngược 15 phút hiển thị trên UI. Tài khoản chỉ được phép đăng nhập lại sau khi kết thúc thời gian khóa.

#### TC-SEC-02: Kiểm thử chống tấn công XSS trộm Token (Cookie Session Flag)
*   **Mục tiêu:** Bảo đảm Token không thể bị đánh cắp qua lỗ hổng chèn mã script.
*   **Các bước thực hiện:**
    1. Thiết lập Token lưu trong Session Cookie.
    2. Chạy đoạn script thử nghiệm trong Console trình duyệt: `console.log(document.cookie)`.
*   **Kết quả mong đợi:**
    *   Chuỗi cookie chứa JWT không được hiển thị (do đã kích hoạt cờ `HttpOnly` ở server), ngăn chặn việc tin tặc gửi token về server độc hại của chúng qua script.

#### TC-SEC-03: Kiểm thử Rate Limiting (Spam gọi API đăng ký)
*   **Các bước thực hiện:** Gửi liên tiếp 8 request `POST /api/account/register` từ một IP trong vòng 10 giây.
*   **Kết quả mong đợi:** 5 request đầu tiên được xử lý, request thứ 6 trở đi bị chặn lập tức và nhận HTTP Status `429 Too Many Requests`.
