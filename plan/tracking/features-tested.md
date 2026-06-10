# 🧪 Nhật Ký Kiểm Thử Tính Năng - Features Tested Log

Dưới đây là nhật ký ghi nhận các tính năng đã được kiểm thử tự động (Unit Test / Integration Test) trong dự án MyPetClinic.

---

## 🔑 Phase 1: Authentication & Profile

### 1. AuthService (Authentication Service)
- **Tệp kiểm thử:** [AuthServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/AuthServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExistsAndUserIsActive`: Chặn đăng ký khi trùng Email và tài khoản đang hoạt động.
  - `RegisterAsync_ShouldReturnSuccess_WhenNewEmail`: Đăng ký thành công tài khoản mới ở trạng thái chờ kích hoạt, tạo mã OTP và gửi email xác thực.
  - `VerifyOtpAsync_ShouldReturnSuccess_WhenOtpIsValid`: Kích hoạt tài khoản thành công khi nhập đúng mã OTP.
- **Trạng thái:** `✅ PASSED`

---

## 📅 Phase 2: Booking & Appointments

### 1. AppointmentService (Booking Service)
- **Tệp kiểm thử:** [AppointmentServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/AppointmentServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `CreateAppointment_ShouldThrowError_WhenDoubleBooking`: Chặn đặt lịch trùng lặp cùng giờ đối với bác sĩ.
  - `RescheduleAppointment_ShouldThrowError_WhenDateInPast`: Chặn dời lịch khám về thời gian quá khứ.
  - `UpdateStatus_ShouldFail_WhenTransitionIsInvalid`: Chặn chuyển đổi trạng thái timeline cuộc hẹn không hợp lệ.
- **Trạng thái:** `✅ PASSED`

### 2. PetService (Pet Portfolio Service)
- **Tệp kiểm thử:** [PetServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/PetServiceTests.cs)
- **Kịch bản kiểm thử:**
  - Lấy danh sách thú cưng và CRUD theo đúng phân quyền sở hữu.
- **Trạng thái:** `✅ PASSED`
