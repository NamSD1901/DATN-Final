# 📄 User & Dev Documentation - Customer Appointment Management

Tài liệu này cung cấp hướng dẫn sử dụng giao diện dành cho khách hàng và tài liệu tích hợp kỹ thuật dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng & Quy trình vận hành (Operator & End-User Guide)

### Quy trình dành cho Khách hàng:
1. Đăng nhập vào hệ thống MyPetClinic, nhấp chọn mục **"Lịch hẹn của tôi"** trên thanh điều hướng.
2. Hệ thống tải danh sách toàn bộ lịch hẹn khám/tiêm của bạn.
3. Sử dụng các thẻ tab bộ lọc ở phía trên (`Tất cả`, `Chờ duyệt`, `Đã duyệt`, `Đã hủy`) để phân loại nhanh danh sách lịch hẹn.
4. Click chọn một thẻ lịch hẹn bất kỳ ở danh sách bên trái để mở bảng thông tin chi tiết ở bên phải:
   * **Medical Timeline:** Theo dõi tiến trình của thú cưng tại phòng khám.
   * **QR Code Check-in:** Khi đưa bé đến phòng khám, hãy mở trang này và quét mã QR Code tại quầy lễ tân để check-in tức thì.
5. **Hủy lịch hẹn trực tuyến:** Nếu có công việc đột xuất, bạn nhấp vào nút **"Hủy lịch hẹn"** màu đỏ (Chỉ khả dụng khi trạng thái là Chờ duyệt hoặc Đã xác nhận). Nhập lý do cụ thể (tối thiểu 10 ký tự) và bấm nút xác nhận.

### Quy trình tiếp nhận dành cho Lễ tân:
1. Khi khách hàng xuất trình mã QR Token check-in trên điện thoại.
2. Lễ tân sử dụng máy quét mã vạch quét mã QR. Hệ thống tự động phân giải token `QR-XXXXXX` thành ID lịch hẹn và tải hồ sơ tiếp đón.
3. Lễ tân nhấn nút "Check-in" để chuyển trạng thái lịch hẹn từ `confirmed` sang `waiting` (Xếp hàng chờ khám), đưa thú cưng vào hàng đợi của Bác sĩ trực ca.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
* **Backend .NET Core:**
  * [CustomerAppointmentController.cs](file:///e:/DATN/MyPetClinic/backend/src/WebApi/Controllers/CustomerAppointmentController.cs) — API controller xử lý request lấy danh sách, chi tiết và hủy lịch hẹn của khách hàng.
  * [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs) — Logic nghiệp vụ chi tiết (Eager Loading, kiểm tra ràng buộc hủy, bảo mật IDOR).
  * `MyPetClinic.Domain/Entities/Appointment.cs` — Thực thể CSDL chứa các trường bổ sung `CancelledReason` và `CancelledAt`.
* **Frontend Vue 3 SPA:**
  * `frontend/src/stores/customerAppointments.ts` — Pinia Store quản lý state phân trang, bộ lọc và action gửi lệnh hủy.
  * [MyAppointmentsTab.vue](file:///e:/DATN/MyPetClinic/frontend/src/components/dashboard/MyAppointmentsTab.vue) — Component Vue hiển thị giao diện Dashboard lịch hẹn, timeline và xử lý Dialog hủy.

### B. Kiểm thử API bằng Curl
Các nhà phát triển có thể sử dụng các lệnh curl sau để kiểm thử nhanh API tại Terminal (yêu cầu gửi kèm Token JWT Customer hợp lệ):

#### 1. Lấy danh sách lịch hẹn có phân trang và lọc trạng thái
```bash
curl -X GET "https://localhost:5001/api/my-appointments?status=confirmed&page=1&pageSize=10" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_CUSTOMER_TẠI_ĐÂY>"
```

#### 2. Xem chi tiết lịch hẹn cụ thể
```bash
curl -X GET "https://localhost:5001/api/my-appointments/e2c38d4f-3721-4f18-a664-d3a373ff2010" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_CUSTOMER_TẠI_ĐÂY>"
```

#### 3. Yêu cầu hủy lịch hẹn (Gửi request PUT)
```bash
curl -X PUT "https://localhost:5001/api/my-appointments/e2c38d4f-3721-4f18-a664-d3a373ff2010/cancel" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_CUSTOMER_TẠI_ĐÂY>" \
     -d "{\"reason\":\"Bé cún bị ốm mệt đột xuất không thể đi xe chuyển vùng được.\"}"
```

---

## 3. Khắc phục sự cố thường gặp (Troubleshooting)

### Sự cố 1: Lỗi lệch giờ hiển thị lịch hẹn trên Dashboard (UTC vs Local Time)
* **Nguyên nhân:** Database PostgreSQL lưu trữ thời gian ở chuẩn UTC (`DateTimeKind.Utc`), còn người dùng chọn giờ khám theo giờ Việt Nam (GMT+7). Khi hiển thị trên UI, nếu không parse đúng múi giờ địa phương, giờ khám sẽ bị lệch giảm đi 7 tiếng (Ví dụ: đặt lịch 10:00 AM nhưng hiển thị thành 03:00 AM).
* **Khắc phục:** Tại Client Vue, sử dụng thư viện `dayjs` hoặc `date-fns` để tự động định dạng hiển thị ngày giờ dựa trên múi giờ cục bộ của trình duyệt khách hàng:
  ```javascript
  import dayjs from 'dayjs';
  const formatDateTime = (utcString) => {
    return dayjs(utcString).format('HH:mm - DD/MM/YYYY');
  };
  ```

### Sự cố 2: Khách hàng báo không nhận được email xác nhận hủy lịch hẹn
* **Nguyên nhân:** SMTP Server bị quá tải, thông tin email khách hàng bị sai, hoặc Hangfire Worker chạy tiến trình nền chưa được khởi chạy đúng cách trên server IIS/Kestrel.
* **Khắc phục:**
  1. Kiểm tra dashboard giám sát của Hangfire tại `/hangfire` để xác định Job gửi mail có bị báo lỗi (Failed state) hay không.
  2. Kiểm tra log lỗi của ứng dụng tại backend để xác định mã phản hồi từ SMTP Server (ví dụ: lỗi xác thực cổng SMTP 587).
  3. Đảm bảo cấu hình SMTP đầy đủ trong tệp `appsettings.json`.
