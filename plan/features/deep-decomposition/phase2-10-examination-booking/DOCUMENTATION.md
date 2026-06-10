# 📄 User & Dev Documentation - Online Examination Booking

Tài liệu cung cấp hướng dẫn đặt lịch khám dành cho khách hàng và tài liệu tích hợp kỹ thuật dành cho nhà phát triển hệ thống **MyPetClinic**.

---

## 1. Hướng dẫn sử dụng dành cho Khách hàng (End-User Guide)

### Các bước đặt lịch hẹn khám trực tuyến:
1. Đăng nhập vào tài khoản của bạn, truy cập vào Dashboard chính và click chọn nút **"Đặt lịch khám mới"**.
2. **Bước 1 (Chọn bé cưng):** Click chọn thẻ thú cưng bạn muốn mang đi khám (Ví dụ: bé cún Leo). Nhấn **"Tiếp tục"**.
3. **Bước 2 (Chọn dịch vụ):** Chọn loại hình khám (Khám tổng quát, Phẫu thuật, Chăm sóc răng miệng...) và mô tả ngắn triệu chứng hiện tại của bé (Ví dụ: bé bỏ ăn và nôn 2 lần). Bạn có thể chọn nhanh các nhãn triệu chứng gợi ý ở dưới. Nhấn **"Tiếp tục"**.
4. **Bước 3 (Chọn Bác sĩ):** Chọn bác sĩ điều trị yêu thích của bạn hoặc chọn "Bác sĩ bất kỳ" để hệ thống tự động sắp xếp. Nhấn **"Tiếp tục"**.
5. **Bước 4 (Chọn giờ khám):** Chọn ngày khám trên lịch, hệ thống sẽ hiển thị các khung giờ trống của bác sĩ đó. Nhấp chọn khung giờ bạn mong muốn (Ví dụ: `10:00 sáng`). Nhấn **"Tiếp tục"**.
6. **Bước 5 (Xác nhận):** Đọc lại toàn bộ thông tin hiển thị trên bảng tóm tắt. Nếu đã chính xác, nhấp nút **"Xác nhận đặt lịch"**.
7. **Nhận mã QR Check-in:** Khi màn hình hiển thị thông báo thành công kèm mã QR, bạn có thể lưu mã này về điện thoại để lễ tân quét mã tiếp nhận nhanh khi đến phòng khám.

---

## 2. Hướng dẫn dành cho Nhà phát triển (Developer Guide)

### A. Cấu trúc thư mục liên quan trong dự án
*   **Backend C# Service:**
    *   [CustomerAppointmentController.cs](file:///e:/DATN/MyPetClinic/backend/src/WebApi/Controllers/CustomerAppointmentController.cs) — API controller tiếp nhận các request đặt lịch.
    *   [AppointmentService.cs](file:///e:/DATN/MyPetClinic/backend/src/MyPetClinic.Application/Services/AppointmentService.cs) — Lõi xử lý chống trùng lịch bác sĩ ($\pm30$ phút) và phân chia trạng thái.
    *   `MyPetClinic.Domain/Entities/Appointment.cs` — Định nghĩa thực thể CSDL.
*   **Frontend SPA Component:**
    *   `frontend/src/stores/booking.ts` — Pinia Store quản lý state Wizard và lưu dữ liệu tạm thời.
    *   `frontend/src/components/booking/BookingWizardModal.vue` — Component UI chứa form 5 bước đặt lịch.

### B. Kiểm thử nhanh API bằng công cụ Curl
Bạn có thể dùng công cụ dòng lệnh `curl` để gửi request kiểm tra API (yêu cầu gửi kèm Token JWT của Customer):

#### 1. Đặt lịch hẹn khám mới (Gửi request POST)
```bash
curl -X POST "https://localhost:5001/api/my-appointments" \
     -H "accept: application/json" \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_CUSTOMER_TẠI_ĐÂY>" \
     -d "{\"petId\":12,\"doctorId\":\"b6c7d2e3-4a5b-8a9b-0c1d-e6f7a5b6c7d8\",\"serviceId\":3,\"appointmentDate\":\"2026-06-15T10:00:00Z\",\"symptom\":\"Chú cún bị nôn mửa\",\"note\":\"Không\"}"
```

#### 2. Hủy lịch hẹn đã đặt (Gửi request PUT)
```bash
curl -X PUT "https://localhost:5001/api/my-appointments/147/cancel" \
     -H "accept: application/json" \
     -H "Authorization: Bearer <NHẬP_TOKEN_JWT_CUSTOMER_TẠI_ĐÂY>"
```

---

## 3. Các sự cố thường gặp & Giải pháp khắc phục (Troubleshooting)

### Sự cố 1: Lỗi `400 Bad Request` - "Bác sĩ đã có lịch hẹn trong khoảng thời gian này"
*   **Mô tả:** Lỗi xảy ra do cơ chế chặn trùng lịch. Khung giờ bạn vừa chọn đã bị một khách hàng khác đặt trước đó vài giây (Race Condition).
*   **Cách khắc phục:** Quay lại Bước 4, chọn ngày khác hoặc đổi khung giờ khám khác, hoặc chọn "Bác sĩ bất kỳ" để hệ thống phân phối sang một bác sĩ khác đang rảnh trong khung giờ đó.

### Sự cố 2: Lỗi múi giờ hiển thị trên lịch bị lệch 7 tiếng (UTC vs Local Time)
*   **Mô tả:** Ngày giờ chọn trên UI là `10:00 AM` nhưng khi gửi lên database PostgreSQL lưu thành `03:00 AM` (hoặc ngược lại).
*   **Cách khắc phục:** PostgreSQL và ASP.NET Core API mặc định giao tiếp múi giờ UTC (`DateTimeKind.Utc`). Ở Frontend, trước khi gửi API, bắt buộc phải dùng lệnh `date.toISOString()` để chuyển múi giờ cục bộ sang chuẩn UTC ISO 8601.
*   Tại `AppointmentService.cs`, sử dụng: `DateTime.SpecifyKind(date.Value, DateTimeKind.Utc)` trước khi ghi vào database.
