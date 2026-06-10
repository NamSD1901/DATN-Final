# 🚀 Kế Hoạch Thực Thi Chi Tiết - Sprint 4

## 🎯 Mục Tiêu Sprint 4
Xây dựng phân hệ chức năng cho Lễ tân để thực hiện tiếp nhận khách hàng (Check-in), duyệt lịch hẹn trực tuyến và điều phối hàng đợi. Đồng thời xây dựng màn hình làm việc của Bác sĩ để theo dõi danh sách bệnh nhân và tiếp nhận khám.

---

## 🛠️ Chi Tiết Các Bước Thực Thi (Step-by-Step)

### 🛎️ 1. Cổng Lễ Tân - Check-in & Duyệt Lịch Hẹn (T25, T26, T27, T28)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoints:**
  * `PUT /api/v1/receptionist/appointments/{id}/confirm` (Lễ tân duyệt lịch hẹn ở trạng thái Pending).
  * `PUT /api/v1/receptionist/appointments/{id}/cancel` (Hủy lịch hẹn, nhận lý do hủy, chuyển trạng thái về Cancelled và tự động gửi email thông báo cho khách hàng).
  * `PUT /api/v1/receptionist/appointments/{id}/check-in` (Xác nhận khách hàng đã đến phòng khám, cập nhật trạng thái lịch hẹn thành Confirmed).

#### 🌐 Frontend UI - *Lâm thực hiện*
* **Trang:** `views/Dashboard.vue` (giao diện Lễ tân)
* **Giao diện:**
  * Bảng danh sách các lịch hẹn chờ duyệt trong ngày. Hỗ trợ nút "Xác nhận" nhanh và nút "Từ chối" (mở Modal điền lý do hủy).
  * Thanh tìm kiếm nhanh khách hàng bằng số điện thoại hoặc email.
  * Danh sách khách đến trực tiếp phòng khám (Walk-in check-in): Form tạo nhanh lịch hẹn mà không cần qua quy trình đặt trước trực tuyến.

---

### 🚶‍♂️ 2. Quản Lý Hàng Đợi (Queue Management) (T29, T30)
#### 🖥️ Backend API - *Hạnh thực hiện*
* **Logic xử lý:** Khi lễ tân bấm "Check-in" thành công cho một lịch hẹn $\rightarrow$ Hệ thống tự sinh số thứ tự khám (Queue Number) tăng dần theo ngày và phân phối ca khám vào phòng chờ của bác sĩ đã đăng ký trong lịch hẹn đó.

#### 🌐 Frontend UI - *Lâm thực hiện*
* **Màn hình hiển thị công cộng (Public Queue Board):** 
  * Thiết kế giao diện tivi lớn (hiển thị ở sảnh chờ phòng khám) hiển thị danh sách số thứ tự đang khám, số chuẩn bị khám và mã phòng/tên bác sĩ phụ trách.
  * Sử dụng CSS sinh động để số thứ tự mới nổi bật khi có thay đổi.

---

### 🩺 3. Cổng Bác Sĩ - Xem Lịch & Tiếp Nhận Ca Khám (T31, T32, T33, T34)
#### 🖥️ Backend API - *Nam thực hiện*
* **API Endpoints:**
  * `GET /api/v1/doctor/my-queue` (Lấy danh sách thú cưng đang chờ khám được phân phối cho bác sĩ hiện tại).
  * `PUT /api/v1/doctor/appointments/{id}/start-exam` (Chuyển trạng thái lịch hẹn từ Confirmed sang "In_Progress" - Đang khám).

#### 🌐 Frontend UI - *Phương thực hiện*
* **Trang:** `views/Dashboard.vue` (giao diện Bác sĩ)
* **Giao diện:**
  * Bảng hàng chờ bệnh nhân được phân bổ cho mình theo số thứ tự tăng dần.
  * Thẻ hiển thị thông tin ca đang khám hiện tại.
  * Nút "Bắt đầu khám" nổi bật. Khi nhấp vào, giao diện tự động chuyển sang trang bệnh án điện tử (Medical Record) để bác sĩ thao tác ghi triệu chứng.

---

## 🔬 Kế Hoạch Kiểm Thử & Nghiệm Thu (DoD)
1. **Kiểm thử tự động:** Viết Unit Test cho API duyệt và hủy lịch hẹn, xác thực email hủy lịch gửi thành công (Mock Email Service).
2. **Kiểm thử thủ công:**
   * Tạo 3 lịch hẹn giả lập cùng một bác sĩ $\rightarrow$ Bấm check-in cả 3 ca ở tài khoản Lễ tân $\rightarrow$ Đăng nhập tài khoản Bác sĩ kiểm tra xem danh sách hàng chờ có hiển thị đúng 3 ca theo thứ tự thời gian hay không.
