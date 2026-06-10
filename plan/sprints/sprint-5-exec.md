# 🚀 Kế Hoạch Thực Thi Chi Tiết - Sprint 5

## 🎯 Mục Tiêu Sprint 5
Hoàn thiện các nghiệp vụ lâm sàng chuyên sâu của bác sĩ (Ghi nhận bệnh án, Kê đơn thuốc, Tiêm chủng vaccine) và quy trình thanh toán hóa đơn của lễ tân để kết thúc hoàn toàn một ca khám bệnh.

---

## 🛠️ Chi Tiết Các Bước Thực Thi (Step-by-Step)

### 📋 1. Hồ Sơ Bệnh Án & Kê Đơn Thuốc (T35, T36, T37, T38, T39)
#### 🖥️ Backend API - *Nam & Hạnh thực hiện*
* **Bảng dữ liệu liên quan:** `Medical_records`, `Prescriptions`, `Prescription_items`, `Medicines`
* **API Endpoints:**
  * `GET /api/v1/pets/{id}/medical-history` (Truy xuất toàn bộ lịch sử khám bệnh và các mũi tiêm cũ của thú cưng).
  * `POST /api/v1/doctor/medical-records` (Tạo bệnh án mới).
* **Quy trình nghiệp vụ lưu bệnh án và kê đơn (Medical Record Transaction):**
  1. Kiểm tra trạng thái lịch hẹn phải là `'In_Progress'`.
  2. Bắt đầu một Database Transaction để lưu thông tin khám lâm sàng (Cân nặng, Nhiệt độ, Triệu chứng, Chẩn đoán) vào bảng `Medical_records`.
  3. Duyệt qua danh sách thuốc kê đơn (`Prescription_items`):
     * Với mỗi loại thuốc: Truy vấn số lượng tồn trong bảng `Medicines`.
     * Nếu số lượng yêu cầu lớn hơn số lượng tồn kho $\rightarrow$ Rollback transaction và trả về lỗi thông báo thuốc A hết hàng.
     * Nếu đủ: Trừ trực tiếp số lượng tồn kho (`Stock_quantity = Stock_quantity - Quantity`).
  4. Lưu đơn thuốc `Prescriptions` và danh sách chi tiết thuốc.
  5. Cập nhật trạng thái lịch hẹn thành `'Completed'`.
  6. Commit Transaction.

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** Màn hình khám bệnh của Bác sĩ.
* **Giao diện:** 
  * Tab 1: Xem dòng thời gian (Timeline) bệnh sử của thú cưng để phục vụ chẩn đoán.
  * Tab 2: Nhập các thông số sinh hiệu (nhiệt độ, nhịp tim) và triệu chứng/chẩn đoán/lời dặn.
  * Tab 3: Form kê đơn thuốc động (Dynamic Form): Cho phép gõ tìm kiếm thuốc bằng Autocomplete (gọi API lấy từ kho), hiển thị số lượng tồn kho real-time, nút thêm dòng thuốc mới, nhập liều lượng và tần suất.

---

### 💉 2. Tiêm Chủng Vắc-xin (T42, T43)
#### 🖥️ Backend API - *Hạnh thực hiện*
* **Bảng dữ liệu liên quan:** `Vaccination_records`, `Vaccines`
* **API Endpoint:** `POST /api/v1/doctor/vaccinations`
* **Logic xử lý:** Ghi nhận thông tin tiêm chủng của thú cưng bao gồm: Loại vắc-xin, số lô tiêm, ngày tiêm và tự động tính ngày nhắc tiêm tiếp theo (`Next_due_date`) tùy thuộc vào loại vắc-xin (ví dụ: vắc-xin dại nhắc lại sau 1 năm).

#### 🌐 Frontend UI - *Phương thực hiện*
* Giao diện nhập thông tin tiêm phòng tích hợp ngay trong màn hình làm việc của bác sĩ hoặc hiển thị khi dịch vụ đi kèm lịch hẹn thuộc danh mục tiêm chủng.

---

### 💵 3. Thanh Toán Hóa Đơn (T40, T41, T44)
#### 🖥️ Backend API - *Nam thực hiện*
* **Bảng dữ liệu liên quan:** `Invoices`, `Invoice_items`
* **API Endpoints:**
  * `POST /api/v1/receptionist/invoices` (Tạo hóa đơn từ mã lịch hẹn `AppointmentId`).
  * `PUT /api/v1/receptionist/invoices/{id}/pay` (Xác nhận thanh toán thành công).
* **Logic xử lý tính tiền hóa đơn:**
  * Phí dịch vụ: Lấy giá của các dịch vụ đã làm từ bảng `Appointment_services`.
  * Phí thuốc: Lấy đơn giá bán lẻ từ bảng `Medicines` $\times$ Số lượng thuốc trong đơn.
  * Tổng tiền = Phí dịch vụ + Phí thuốc - Giảm giá.
  * Khi xác nhận thanh toán thành công (`Payment_status = 'Paid'`), ghi nhận phương thức thanh toán (Tiền mặt, Chuyển khoản ngân hàng) và thời gian thanh toán.

#### 🌐 Frontend UI - *Lâm thực hiện*
* Giao diện Lễ tân hiển thị chi tiết hóa đơn cần thanh toán, liệt kê rõ ràng từng khoản phí dịch vụ và thuốc, có nút in hóa đơn (Print View) và nút "Xác nhận thanh toán".

---

## 🔬 Kế Hoạch Kiểm Thử & Nghiệm Thu (DoD)
1. **Kiểm thử tự động:** Viết Unit Test cho nghiệp vụ kê đơn thuốc, kiểm tra xem giao dịch có tự động rollback và giữ nguyên số lượng tồn kho nếu có một loại thuốc bất kỳ bị thiếu hàng hay không.
2. **Kiểm thử thủ công:** Thực hiện trọn vẹn luồng từ kê đơn thuốc, xem hóa đơn được tạo tự động tương ứng với giá trị thuốc, và bấm thanh toán hóa đơn để kiểm tra trạng thái lịch hẹn chuyển sang `Completed`.
