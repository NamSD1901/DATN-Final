# 🩺 Clinical Diagnosis & Treatment (Khám bệnh & Tiêm chủng)

## 📝 Mô tả Tính năng
Màn hình nghiệp vụ chuyên sâu của Bác sĩ thú y để truy cập nhanh hồ sơ bệnh án, ghi nhận quá trình khám bệnh, chẩn đoán, thực hiện tiêm phòng vaccine và kê đơn thuốc điều trị.

## 📋 User Stories (Acceptance Criteria)
*   **PB20 (Hàng khám bác sĩ):** Bác sĩ xem danh sách hàng chờ bệnh nhân của mình theo số thứ tự để bấm bắt đầu khám.
*   **PB21 (Xem bệnh sử):** Truy xuất dòng thời gian (timeline) lịch sử bệnh án và các mũi tiêm phòng trước đây của thú cưng.
*   **PB23 (Ghi nhận bệnh án & Kê đơn):** Ghi chẩn đoán, triệu chứng lâm sàng. Kê đơn thuốc từ danh mục thuốc (kho tự động trừ số lượng tồn). Hệ thống rollback transaction và cảnh báo nếu có thuốc bị thiếu số lượng trong kho.
*   **PB24 (Tiêm chủng):** Ghi nhận mũi tiêm vaccine, số lô tiêm, ngày tiêm và thiết lập ngày tái chủng tiếp theo.

## 🛠️ Đặc tả Kỹ thuật (Technical Specs)
*   **API Endpoints:**
    *   `GET /api/doctor/queue` (Xem danh sách ca chờ khám của bác sĩ)
    *   `POST /api/doctor/medical-records` (Tạo bệnh án & đơn thuốc mới)
    *   `POST /api/doctor/vaccinations` (Ghi nhận thông tin tiêm chủng)
    *   `GET /api/pets/{id}/medical-history` (Lấy bệnh sử của thú cưng)
*   **Database Tables:** `Medical_records`, `Prescriptions`, `Prescription_items`, `Vaccination_records`, `Medicines` (Tồn kho).

## 🎨 Giao diện UI/UX
*   **Views/Components:** Dashboard dành riêng cho bác sĩ, form khám bệnh động có autocomplete khi gõ tên thuốc và hiển thị số lượng tồn kho real-time.
