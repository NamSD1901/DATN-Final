# 🚀 Kế Hoạch Thực Thi Chi Tiết - Sprint 3

## 🎯 Mục Tiêu Sprint 3
Triển khai phân hệ cốt lõi: Đặt lịch khám và tiêm chủng trực tuyến từ phía khách hàng. Thiết lập cấu hình khung giờ (Slots) và ca trực của bác sĩ giúp hệ thống tự động ngăn chặn việc đặt trùng lặp.

---

## 🛠️ Chi Tiết Các Bước Thực Thi (Step-by-Step)

### 🗓️ 1. Logic Khung Giờ (Slots) & Ca Trực (T18)
#### 🖥️ Backend API - *Nam thực hiện*
* **Bảng dữ liệu liên quan:** `Doctor_schedules`, `Appointments`
* **Logic xử lý:**
  1. Cho phép cấu hình lịch trực của bác sĩ theo ngày (ví dụ: Thứ Hai, Bác sĩ A trực từ 08:00 đến 12:00, tối đa 8 ca khám).
  2. Xây dựng hàm Helper kiểm tra sự sẵn sàng của khung giờ (Slot Availability Helper):
     * Nhận vào: `DoctorId`, `AppointmentDate`, `StartTime`.
     * Logic: Tính tổng số lịch hẹn đã đặt ở trạng thái khác `'Cancelled'` trong khung giờ đó. Nếu vượt quá `Max_appointments` hoặc bác sĩ không đăng ký trực vào ngày đó $\rightarrow$ Trả về `False`.

---

### 🏥 2. Đặt Lịch Khám & Tiêm Chủng Trực Tuyến (T19, T20, T21, T22)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoints:** 
  * `POST /api/v1/customer-appointments/book-examination` (Đặt lịch khám bệnh).
  * `POST /api/v1/customer-appointments/book-vaccination` (Đặt lịch tiêm phòng).
* **Luồng xử lý nghiệp vụ:**
  1. Kiểm tra thú cưng (`Pet_id`) thuộc sở hữu của người dùng đang đăng nhập.
  2. Kiểm tra khung giờ bác sĩ được chọn còn trống hay không (sử dụng Helper ở T18).
  3. Đối với lịch tiêm phòng: Validate loại vaccine (`Vaccine_id`) có tồn tại và còn trong kho hay không.
  4. Lưu lịch hẹn với trạng thái mặc định là `'Pending'` (Chờ xác nhận).

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `views/services/KhamDieuTri.vue` và `views/services/TiemPhong.vue`
* **Giao diện đặt lịch dạng Multi-step (Wizard Form):**
  * **Bước 1:** Chọn thú cưng của tôi (tải động danh sách Pet của user).
  * **Bước 2:** Chọn gói dịch vụ (hoặc loại vắc-xin đối với tiêm chủng).
  * **Bước 3:** Chọn ngày khám $\rightarrow$ Hiển thị danh sách bác sĩ trực và các khung giờ (Slots) còn trống tương ứng.
  * **Bước 4:** Ghi chú triệu chứng ban đầu $\rightarrow$ Nhấp đặt lịch.
  * **Hiệu ứng:** Spinner loading khi gửi request, Modal thông báo đặt thành công kèm chi tiết lịch hẹn.

---

### 📊 3. Quản Lý & Theo Dõi Lịch Hẹn (T23, T24)
#### 🖥️ Backend API - *Hạnh thực hiện*
* **API Endpoint:** `GET /api/v1/customer-appointments`
* **Logic xử lý:** Lấy danh sách lịch hẹn của khách hàng hiện tại, hỗ trợ phân trang (Pagination) và lọc (Filter) theo trạng thái (`Pending`, `Confirmed`, `Cancelled`, `Completed`).

#### 🌐 Frontend UI - *Lâm thực hiện*
* **Trang:** `views/History.vue` (hoặc tab Lịch hẹn của tôi).
* **Giao diện:** Hiển thị danh sách lịch hẹn dưới dạng danh sách hoặc timeline. Sử dụng màu sắc trực quan tương ứng với trạng thái (Đỏ: Đã hủy, Vàng: Chờ duyệt, Xanh lá: Đã xác nhận/Hoàn thành).

---

## 🔬 Kế Hoạch Kiểm Thử & Nghiệm Thu (DoD)
1. **Kiểm thử tự động:** Viết Unit Test giả lập 2 khách hàng cùng click đặt lịch cho 1 bác sĩ trong cùng 1 slot giờ cận kề (Concurrency Test) để đảm bảo hệ thống chặn thành công ca thứ 2.
2. **Kiểm thử thủ công:** Đặt thử lịch khám thành công, sau đó truy cập trang quản lý của khách hàng để kiểm tra tính chính xác của thông tin trạng thái hiển thị.
