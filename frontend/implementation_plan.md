# KẾ HOẠCH TRIỂN KHAI PHÂN HỆ BỆNH ÁN SOAP
*Dự án: MyPet Clinic*

> [!NOTE]
> Đây là bản kế hoạch thực thi (Implementation Plan) chi tiết từng bước. Chúng ta sẽ áp dụng phương pháp **Incremental Development**, thực hiện dứt điểm từng bước, kiểm thử kỹ lưỡng trước khi chuyển sang bước tiếp theo nhằm triệt tiêu tối đa lỗi phát sinh.

## User Review Required
> [!IMPORTANT]
> Vui lòng xem xét các bước (Phases) dưới đây. Nếu bạn đồng ý với lộ trình này, tôi sẽ tiến hành tạo danh sách Task (task.md) và bắt đầu code **Phase 1** ngay lập tức.

## MỤC TIÊU PHÁT TRIỂN
Chuyển đổi bản thiết kế SRS (soap_medical_record_design.md) thành hệ thống thực tế hoạt động hoàn chỉnh, tuân thủ Clean Architecture (.NET) và Vue 3 Composition API.

---

## LỘ TRÌNH THỰC THI (PHASES)

### CHẶNG 1: MỞ RỘNG CẤU TRÚC DỮ LIỆU HIỆN TẠI (DATABASE EXTENSION)
*Bạn nói rất đúng, chúng ta đã có sẵn entity `MedicalRecord` và các cột cơ bản (`MedicalHistory`, `ClinicalSigns`, `Diagnosis`, `TreatmentPlan`). Tuy nhiên, để đáp ứng được form SOAP khổng lồ vừa phân tích, chúng ta cần quyết định cách lưu trữ mở rộng:*

1. **Tối ưu hóa Entity `MedicalRecord` hiện có:**
   - **Phương án A (Lưu chuỗi/JSON):** Giữ nguyên số lượng cột hiện tại. Ở Frontend, khi bác sĩ tick chọn hàng chục checkbox, ta sẽ gom (concatenate) tất cả lại thành một đoạn Text hoặc cục JSON và lưu vào cột `MedicalHistory` / `ClinicalSigns` có sẵn. (Ưu điểm: Không cần đụng vào Database).
   - **Phương án B (Tách bảng chuẩn hóa EAV):** Tạo thêm các bảng con `ClinicalAssessment` (Lưu từng triệu chứng riêng biệt) để phục vụ cho tính năng Thống kê (Report) sau này. (Ưu điểm: Truy vấn thống kê tốt).
   
   *(Trong kế hoạch này, tôi đề xuất dùng **Phương án A (Lưu JSON/Text gom nhóm)** ở giai đoạn này để giúp hệ thống nhẹ gọn và triển khai cực nhanh, tận dụng 100% Database hiện có).*

2. **Cập nhật/Kiểm tra khóa ngoại:**
   - Đảm bảo `Prescription` (Đơn thuốc) liên kết đúng với `MedicalRecordId`.

### CHẶNG 2: XÂY DỰNG LỚP NGHIỆP VỤ & API (BACKEND)
*Xây dựng bộ não xử lý của hệ thống, cài đặt các Business Rules đã phân tích để đảm bảo tính an toàn y khoa.*

1. **Data Transfer Objects (DTOs):**
   - Tạo `MedicalRecordSoapRequestDto` (Để nhận dữ liệu từ Frontend).
   - Tạo `MedicalRecordSoapResponseDto` (Để trả dữ liệu hiển thị).
2. **Triển khai AutoMapper:**
   - Map dữ liệu phức tạp từ nhiều Entities sang Response DTO.
3. **Cài đặt Validator (FluentValidation):**
   - Viết các quy tắc bắt lỗi chặt chẽ (Ví dụ: Cân nặng > 0, Nhiệt độ hợp lý, Không kê thuốc kỵ nhau).
4. **Triển khai Service (`IMedicalRecordService` & `MedicalRecordService`):**
   - Viết logic lưu bệnh án SOAP (Transaction: Lưu MedicalRecord -> Lưu Vitals -> Lưu Prescriptions cùng một lúc).
   - Xử lý tự động chuyển trạng thái `Appointment` thành `Completed` khi bệnh án đóng.
   - Tự động tạo Invoice (Hóa đơn) từ danh sách thuốc và dịch vụ.
5. **Tạo Endpoints API (`MedicalRecordsController`):**
   - API `POST /api/medical-records` (Tạo mới).
   - API `GET /api/medical-records/{id}` (Lấy chi tiết).
   - API `PUT /api/medical-records/{id}` (Cập nhật khi đang khám).

### CHẶNG 3: TÍCH HỢP GIAO DIỆN PHÒNG KHÁM (FRONTEND UI)
*Xây dựng giao diện trạm làm việc của Bác sĩ (Doctor Workspace) thật thông minh để nhập liệu nhanh chóng.*

1. **Chuẩn bị State & API Client (Pinia/Axios):**
   - Viết các hàm API fetch, create, update bệnh án SOAP.
2. **Xây dựng Form Nhập liệu Bệnh án (ConsultationRecordTab.vue):**
   - **Giao diện Subjective:** Ô nhập lý do khám, hệ thống tag (chip) để chọn nhanh triệu chứng.
   - **Giao diện Objective:** Grid nhập sinh hiệu (Cân nặng, nhiệt độ), các Nút "Mark All as Normal" cho khám lâm sàng.
   - **Giao diện Assessment:** Ô chẩn đoán có Autocomplete.
   - **Giao diện Plan:** Bảng kê đơn thuốc (Tự động tính liều/kg) và Textarea dặn dò chủ nuôi.
3. **Kết nối Form với Backend:**
   - Bind dữ liệu (v-model) phức tạp, xử lý sự kiện Gửi (Submit).
   - Hiển thị Toast Notification (Thành công/Lỗi).
4. **Hoàn thiện UI Tái khám:**
   - Giao diện chọn ngày tái khám. Cấu hình gửi nhắc nhở tự động qua Gmail (Sử dụng hệ thống Background Worker nếu cần thiết).

### CHẶNG 4: KIỂM THỬ VÀ NGHIỆM THU (QA & VERIFICATION)
*Chốt chặn cuối cùng đảm bảo không có lỗi ngớ ngẩn (bugs) trước khi merge nhánh.*

1. **Kiểm thử Unit Test (Backend):**
   - Đảm bảo logic tính tiền thuốc, logic bắt lỗi cân nặng/tuổi hoạt động đúng.
2. **Kiểm thử Liên thông (Integration Test):**
   - Test flow: Đặt lịch -> Nhận khám -> Điền SOAP -> Kê thuốc -> Thanh toán hóa đơn.
3. **Rà soát Giao diện (Responsive & Edge Cases):**
   - Đảm bảo giao diện Form khám bệnh không bị vỡ trên màn hình nhỏ.
   - Xử lý trường hợp rớt mạng (Network Error) khi đang lưu bệnh án.

---

## TỔ CHỨC CÔNG VIỆC TỨC THÌ (IMMEDIATE ACTION)
Nếu bạn **Phê duyệt (Approve)** kế hoạch này, tôi sẽ khởi tạo tệp tin theo dõi tiến độ `task.md` và chúng ta sẽ xắn tay áo vào **CHẶNG 1: DATABASE & ENTITIES**.

Tôi sẽ tuyệt đối tuân thủ nguyên tắc:
1. Làm xong chặng nào, xác nhận chặng đó.
2. Không viết code tràn lan, chỉ tập trung đúng file và scope của task hiện tại.
3. Báo cáo chi tiết kết quả cho bạn trước khi qua task mới.

## Open Questions
* Bạn có muốn tôi bổ sung hoặc cắt giảm bước nào trong 4 chặng trên trước khi bắt đầu không?
