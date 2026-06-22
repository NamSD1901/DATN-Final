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
  - `CreateAppointment_ShouldCheckStockAndDecrement_WhenVaccineRequested`: Kiểm kho tự động và trừ 1 liều khi đặt lịch tiêm thành công.
  - `GetPetMedicalHistory_ShouldThrowUnauthorizedAccessException_WhenUserIsNotPetOwner`: Chặn truy xuất bệnh sử thú cưng bất hợp pháp (chống tấn công IDOR).
  - `GetCustomerAppointmentsPaginated_ShouldReturnPaginatedResults`: Lấy danh sách lịch hẹn của khách hàng có phân trang và lọc trạng thái thành công.
  - `GetAvailableSlots_ShouldReturnAvailableSlots_WhenDoctorHasSchedule`: Tính toán chính xác các khung giờ trống của từng bác sĩ dựa trên lịch trực và lịch khám hiện có.
- **Trạng thái:** `✅ PASSED`


### 2. PetService (Pet Portfolio Service)
- **Tệp kiểm thử:** [PetServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/PetServiceTests.cs)
- **Kịch bản kiểm thử:**
  - Lấy danh sách thú cưng và CRUD theo đúng phân quyền sở hữu.
- **Trạng thái:** `✅ PASSED`

### 6. DoctorController (Cổng Bác sĩ & Tiếp nhận khám)
- **Tệp kiểm thử:** Tích hợp kiểm thử thủ công & Rà soát IDOR bảo mật.
- **Kịch bản kiểm thử:**
  - `StartTreatment_ShouldReturnForbidden_WhenDoctorIdMismatch`: Chặn đứng lỗ hổng IDOR khi bác sĩ can thiệp bắt đầu ca khám không thuộc thẩm quyền của mình.
  - `FinishTreatment_ShouldReturnForbidden_WhenDoctorIdMismatch`: Chặn đứng lỗ hổng IDOR khi bác sĩ can thiệp hoàn thành ca khám của bác sĩ khác.
- **Trạng thái:** `✅ PASSED`


### 3. SlotCalculationHelper (Doctor Availability Logic)
- **Tệp kiểm thử:** [SlotCalculationHelperTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/SlotCalculationHelperTests.cs)
- **Kịch bản kiểm thử:**
  - `GenerateSlots_ShouldReturnCorrectSlots_ForStandardShift`: Sinh chính xác các slot trong ca trực của bác sĩ.
  - `GetAvailableSlots_ShouldReturnAllSlots_WhenNoAppointmentsExist`: Giữ nguyên tất cả các slot khi không có lịch hẹn nào.
  - `GetAvailableSlots_ShouldRemoveBusySlots_WhenMatchingAppointmentsExist`: Loại bỏ chính xác các slot bị trùng lịch hẹn.
  - `GetAvailableSlots_ShouldExcludeCancelledAppointments`: Không loại bỏ các slot trùng với lịch hẹn đã bị hủy.
  - `GetAvailableSlots_ShouldBlockSlots_WithConflictUnderThreshold`: Chặn chính xác các slot có xung đột thời gian dưới 30 phút.
- **Trạng thái:** `✅ PASSED`
 
### 4. VaccinationScheduleChecker (Phác đồ tiêm chủng)
- **Tệp kiểm thử:** [VaccinationScheduleCheckerTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/VaccinationScheduleCheckerTests.cs)
- **Kịch bản kiểm thử:**
  - `ValidateInterval_ShouldReturnValid_WhenNoConstraintsViolated`: Hợp lệ khi các ràng buộc không bị vi phạm.
  - `ValidateInterval_ShouldFail_WhenSpeciesMismatch`: Từ chối tiêm nếu loài thú cưng không khớp loài đích của vắc-xin.
  - `ValidateInterval_ShouldFail_WhenPetTooYoung`: Từ chối tiêm nếu thú cưng chưa đủ tuổi tối thiểu theo phác đồ.
  - `ValidateInterval_ShouldWarn_WhenIntervalTooShort`: Cảnh báo và yêu cầu bác sĩ ghi đè nếu thời gian giãn cách mũi tiêm quá ngắn.
- **Trạng thái:** `✅ PASSED`
 
### 5. ReceptionistService (Tiếp nhận & Xử lý hàng đợi)
- **Tệp kiểm thử:** [ReceptionistServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/ReceptionistServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `OmniSearch_ShouldReturnMatches_WhenPetOrCustomerNameMatches`: Tìm kiếm nhanh chủ nuôi, số điện thoại hoặc mã thú cưng thành công.
  - `CheckIn_ShouldSucceed_WhenAppointmentIsToday`: Khách hàng check-in đúng ngày hẹn thành công, tự động cập nhật cân nặng mới của thú cưng và xếp số thứ tự hàng đợi.
  - `CheckIn_ShouldThrow_WhenAppointmentDateIsNotToday`: Từ chối check-in nếu sai ngày đặt lịch (chống gian lận xếp hàng).
  - `CreateWalkIn_ShouldCreateNewCustomerAndPet_WhenNotExists`: Tiếp nhận ca vãng lai thành công, tự động tạo mới hồ sơ khách hàng & thú cưng khi chưa có trong hệ thống và tự động phân phối bác sĩ trực rảnh nhất.
  - `GetTodayQueue_ShouldPrioritizeEmergency_AndThenQueueNumber`: Hiển thị hàng đợi ngày hôm nay chính xác theo giờ Việt Nam, tự động ưu tiên các ca nguy kịch/cấp cứu lên đầu bảng.
  - `UpdateEmergencyCustomer_ShouldMapDummyToRealProfile_AndCleanupDummy`: Ghép nối hồ sơ ca cấp cứu ẩn danh với thông tin thật của chủ nuôi và thú cưng để chuẩn hóa dữ liệu bệnh án sau điều trị.
- **Trạng thái:** `✅ PASSED`

---

## 🩺 Phase 2 Extension: Medical Records & Prescriptions (Sprint 12)

### 1. MedicalRecordService & Prescription
- **Tệp kiểm thử:** [MedicalRecordServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/MedicalRecordServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `CreateMedicalRecord_ShouldCompleteAppointment_WhenSuccessful`: Đóng ca khám, đổi trạng thái cuộc hẹn thành completed và trừ kho tự động khi kê đơn thuốc thành công.
  - `CreateMedicalRecord_ShouldRollbackAllDeductions_WhenAnyMedicineOutOfStock`: Rollback toàn bộ giao dịch nếu bất kỳ loại thuốc nào hết hàng hoặc không đủ tồn kho (đảm bảo tính nhất quán dữ liệu).
- **Trạng thái:** `✅ PASSED`

---

## 💉 Phase 2 Extension: Vaccination & Invoices (Sprint 13)

### 1. VaccinationService
- **Tệp kiểm thử:** [VaccinationServiceTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/VaccinationServiceTests.cs)
- **Kịch bản kiểm thử:**
  - `RecordVaccination_ShouldAutoCalculateNextDueDate_BasedOnIntervalDays`: Tự động tính ngày tiêm nhắc lại chính xác dựa trên cấu hình chu kỳ vắc-xin và trừ 1 vắc-xin tồn kho.
  - `RecordVaccination_ShouldBeNullNextDueDate_WhenIntervalDaysIsZero`: Trả về NextDueDate bằng null đối với vắc-xin không cần tiêm nhắc lại.
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
  - `UpdateUserRole_SelfChange_ReturnsBadRequest`: Ngăn chặn Admin tự hạ quyền hoặc thay đổi vai trò của bản thân.
  - `ToggleUserStatus_SelfLock_ReturnsBadRequest`: Ngăn chặn Admin tự khóa/đóng tài khoản của chính mình.
  - `UpdateUserRole_ValidUserAndRole_UpdatesSuccessfully`: Admin cập nhật vai trò nhân sự khác thành công và ghi Audit Log.
  - `ToggleUserStatus_ValidUser_UpdatesSuccessfully`: Admin kích hoạt/tạm khóa trạng thái nhân sự khác thành công và ghi Audit Log.
  - `GetSlotConfig_ReturnsDefaultOrExistingConfig`: Đọc cấu hình Slot từ JSON thành công.
- **Trạng thái:** `✅ PASSED`

---

## 📦 Phase 3: Medicines Inventory & Staff Work Schedules (Sprint 15)

### 1. MedicineAdminTests (Quản trị Kho thuốc)
- **Tệp kiểm thử:** [MedicineAdminTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/MedicineAdminTests.cs)
- **Kịch bản kiểm thử:**
  - `GetMedicines_ReturnsAllMedicines`: Lấy danh sách toàn bộ dược phẩm trong kho thuốc.
  - `GetMedicineWarnings_CategorizesLowStockAndExpiring`: Lọc và phân loại cảnh báo các loại thuốc sắp hết hàng (tồn <= 10) hoặc sắp hết hạn (<= 30 ngày).
  - `CreateMedicine_AddsAndSaves`: Thêm thuốc mới thành công.
  - `UpdateMedicine_ExistingMedicine_UpdatesSuccessfully`: Cập nhật thông tin chi tiết của thuốc đang có.
  - `DeleteMedicine_ExistingMedicine_RemovesSuccessfully`: Xóa thuốc ra khỏi kho dữ liệu.
- **Trạng thái:** `✅ PASSED`

### 1.5. InventoryIntegrationTests (Kiểm thử luồng Tồn kho tổng thể)
- **Tệp kiểm thử:** [InventoryIntegrationTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/InventoryIntegrationTests.cs)
- **Kịch bản kiểm thử:**
  - `InventoryFlow_Import_Prescribe_Export_Audit_ShouldWorkCorrectly`: Chạy luồng E2E từ lúc Admin nhập kho một lô thuốc mới, Bác sĩ kê đơn thuốc (tự động xuất kho theo FEFO), hệ thống cảnh báo thuốc sắp hết hạn, và kiểm kê xử lý chênh lệch thất thoát.
- **Trạng thái:** `✅ PASSED`

### 2. ScheduleAdminTests (Ca trực Bác sĩ)
- **Tệp kiểm thử:** [ScheduleAdminTests.cs](file:///e:/DATN/MyPetClinic/backend/tests/MyPetClinic.Tests/ScheduleAdminTests.cs)
- **Kịch bản kiểm thử:**
  - `GetSchedules_ReturnsFilteredSchedules`: Lấy danh sách lịch trực của các bác sĩ.
  - `CreateSchedule_Overlapping_ReturnsBadRequest`: Chặn xếp trùng hoặc chồng lấn thời gian trực trong ngày của cùng một bác sĩ.
  - `CreateSchedule_ValidTime_CreatesSuccessfully`: Thêm ca trực mới hợp lệ thành công.
  - `UpdateSchedule_OverlappingWithAnother_ReturnsBadRequest`: Chặn cập nhật ca trực nếu khung giờ mới chồng lấn lịch trực khác của bác sĩ đó.
  - `DeleteSchedule_ExistingSchedule_RemovesSuccessfully`: Xóa lịch trực thành công.
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
  - `SendVaccineReminders_ShouldSendEmail_OnlyWhenNextDueDateIsExactlyThreeDaysAhead`: Kiểm thử logic quét và tự động gửi email nhắc lịch tiêm chủng vắc-xin trước đúng 3 ngày, bỏ qua các mũi tiêm chưa đến hạn hoặc đã quá hạn.
- **Trạng thái:** `✅ PASSED`
