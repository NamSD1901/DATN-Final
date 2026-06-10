# 🎭 Behavioral Specification - Authentication & Authorization

## 1. Máy trạng thái phiên làm việc (Finite State Machine - FSM)

Toàn bộ quá trình định danh và phiên làm việc (Session) của người dùng chạy trên ứng dụng SPA Client được kiểm soát chặt chẽ thông qua mô hình trạng thái sau:

```mermaid
stateDiagram-v2
    [*] --> Unauthenticated : Khởi chạy ứng dụng (Không có Token)
    
    Unauthenticated --> Registering : Click "Đăng ký tài khoản"
    Unauthenticated --> Authenticating : Click "Đăng nhập" (Hoặc Google Login)
    
    %% Tiến trình Đăng Ký
    state Registering {
        [*] --> InputDetails
        InputDetails --> RegisterFailed : Dữ liệu không hợp lệ / Email trùng
        InputDetails --> RegisterSuccess : Dữ liệu đạt chuẩn
    }
    RegisterFailed --> InputDetails : Sửa dữ liệu
    RegisterSuccess --> WaitingOTP : Chuyển hướng sang Form nhập OTP
    
    %% Xác thực OTP
    state WaitingOTP {
        [*] --> InputOTP
        InputOTP --> OtpExpired : Hết hạn 5 phút
        InputOTP --> OtpInvalid : Nhập sai mã (Tối đa 3 lần)
        InputOTP --> OtpValid : Nhập đúng mã
    }
    OtpExpired --> InputOTP : Click "Gửi lại OTP"
    OtpInvalid --> Unauthenticated : Sai 3 lần liên tiếp (Chặn spam)
    OtpValid --> Authenticated : Kích hoạt & Tự động lưu Token
    
    %% Tiến trình Đăng Nhập
    state Authenticating {
        [*] --> CheckCredentials
        CheckCredentials --> WrongCredentials : Sai email/mật khẩu
        CheckCredentials --> SuccessLogin : Khớp mật khẩu
        WrongCredentials --> LockoutTrigger : Nhập sai liên tiếp 5 lần
    }
    LockoutTrigger --> LockoutState : Tạm khóa 15 phút
    LockoutState --> Unauthenticated : Hết 15 phút khóa
    
    SuccessLogin --> Authenticated : Lưu JWT Token & Trích xuất Roles
    
    %% Trạng thái Đăng Nhập Thành Công
    state Authenticated {
        [*] --> CheckRoleAndRedirect
        CheckRoleAndRedirect --> CustomerDashboard : Role: customer
        CheckRoleAndRedirect --> StaffDashboard : Role: receptionist / doctor
        CheckRoleAndRedirect --> AdminDashboard : Role: admin
    }
    
    Authenticated --> Unauthenticated : Click "Đăng xuất" (Clear Token & Reset Pinia)
    Authenticated --> SessionExpired : Token hết hạn (Hết 24 giờ)
    SessionExpired --> Unauthenticated : Redirect về trang Login + Toast cảnh báo
```

---

## 2. Diễn giải các chuyển dịch trạng thái chính (Transitions)

### A. Đăng ký & Chờ kích hoạt OTP
*   Khi người dùng đăng ký hợp lệ ở trạng thái `RegisterSuccess`, Client SPA tự động chuyển hướng màn hình sang Form OTP, lưu địa chỉ Email gửi OTP vào state `otpEmailTarget`.
*   Cờ `requiresOtpVerification` được đặt thành `true`.
*   Bộ đếm ngược (Timer) 5 phút được kích hoạt trên giao diện. Khi đếm ngược về `00:00`, nút "Gửi lại mã" sẽ sáng lên để cho phép yêu cầu một OTP mới.

### B. Cơ chế Khóa tài khoản tạm thời (Lockout Transition)
*   Khi trạng thái `WrongCredentials` lặp lại lần thứ 5 trên cùng một Email:
    *   Hệ thống khóa tài khoản trong CSDL và phản hồi HTTP `400 Bad Request` kèm cờ khóa.
    *   Client SPA chuyển giao diện sang `LockoutState` hiển thị đồng hồ đếm ngược 15 phút.
    *   Vô hiệu hóa toàn bộ nút Đăng nhập và ô nhập liệu của tài khoản bị khóa trong suốt 15 phút này để bảo vệ chống Brute-force.

### C. Hết hạn phiên làm việc (Session Expiry Behavior)
*   Token JWT của hệ thống có thời hạn cứng 24 giờ.
*   **Token Expiry Check:** Hệ thống sử dụng Axios Interceptor để kiểm tra mọi request gửi đi.
*   Nếu API trả về mã lỗi HTTP `401 Unauthorized` (do Token hết hạn), Interceptor lập tức bắt được lỗi, tự động gọi action `authStore.logout()`, xóa token cũ, chuyển hướng người dùng về trang `/login` và hiển thị Toast thông báo: *"Phiên làm việc của bạn đã hết hạn. Vui lòng đăng nhập lại."*.
*   Điều này giúp bảo đảm an toàn dữ liệu, ngăn việc trình duyệt lưu lại token cũ quá hạn.
