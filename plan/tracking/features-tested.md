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

---

## 🩺 Phase 2 Extension: Medical Records & Prescriptions (Sprint 12)

### 1. MedicalRecordService & Prescription
- **Tệp kiểm thử:** [MedicalRecordServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/MedicalRecordServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `CreateRecord_ShouldDeductStock_WhenPrescriptionIsCreated`: Trừ kho tự động khi kê đơn thuốc.
  - `CreateRecord_ShouldRollback_WhenStockIsInsufficient`: Rollback toàn bộ giao dịch nếu bất kỳ loại thuốc nào hết hàng hoặc không đủ tồn kho.
- **Trạng thái:** `✅ PASSED`

---

## 💉 Phase 2 Extension: Vaccination & Invoices (Sprint 13)

### 1. VaccinationService
- **Tệp kiểm thử:** [VaccinationServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/VaccinationServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `CreateVaccinationRecord_ShouldAutoCalculateNextDueDate`: Tự động tính ngày tiêm nhắc lại chính xác dựa trên cấu hình chu kỳ vắc-xin.
- **Trạng thái:** `✅ PASSED`

### 2. InvoiceService
- **Tệp kiểm thử:** [InvoiceServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/InvoiceServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `GenerateInvoice_ShouldCalculateTotalAmount_SummingServicesAndMedicines`: Tính tổng hóa đơn chính xác bằng tổng chi phí dịch vụ khám lâm sàng cộng với tiền thuốc đã kê đơn.
- **Trạng thái:** `✅ PASSED`

---

## 👥 Phase 3: System Administration & Security (Sprint 14)

### 1. AdminController
- **Tệp kiểm thử:** [AdminControllerTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/AdminControllerTests.cs)
- **Kịch bản kiểm thử:**
  - `UpdateUserRole_SelfDemote_ShouldReturnBadRequest`: Ngăn chặn Admin tự hạ quyền của bản thân.
  - `ToggleUserStatus_SelfSuspend_ShouldReturnBadRequest`: Ngăn chặn Admin tự khóa tài khoản của chính mình.
- **Trạng thái:** `✅ PASSED`

---

## 📦 Phase 3: Medicines Inventory & Staff Work Schedules (Sprint 15)

### 1. ScheduleService
- **Tệp kiểm thử:** [ScheduleServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/ScheduleServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `AssignSchedule_ShouldFail_WhenConflictExists`: Ngăn chặn xếp trùng lịch trực cho nhân viên trong cùng một ngày và ca trực.
- **Trạng thái:** `✅ PASSED`

### 2. MedicineService
- **Tệp kiểm thử:** [MedicineServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/MedicineServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `GetLowStockMedicinesAsync`: Cảnh báo và lọc các thuốc có tồn kho dưới ngưỡng an toàn.
  - `GetExpiringMedicinesAsync`: Cảnh báo và lọc các thuốc có hạn sử dụng dưới 30 ngày.
- **Trạng thái:** `✅ PASSED`

---

## 📊 Phase 3: Revenue Reports & Public Blog (Sprint 16)

### 1. ReportService
- **Tệp kiểm thử:** [ReportServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/ReportServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `GetRevenueReport_ShouldSumOnlyPaidInvoices`: Tính toán doanh thu gộp (Sum) chính xác bằng cách chỉ cộng dồn giá trị từ các hóa đơn đã thanh toán.
- **Trạng thái:** `✅ PASSED`

---

## 🤖 Phase 3: Gemini AI & Automatic Reminders (Sprint 17)

### 1. VaccineReminderWorker
- **Tệp kiểm thử:** [VaccineReminderWorkerTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/VaccineReminderWorkerTests.cs)
- **Kịch bản kiểm thử:**
  - `SendVaccineReminders_ShouldSendEmail_OnlyWhenNextDueDateIsExactlyThreeDaysAhead`: Xác thực background service quét chính xác và chỉ gửi email nhắc nhở cho các lịch hẹn tiêm phòng trước đúng 3 ngày.
- **Trạng thái:** `✅ PASSED`




