# 🗓️ Sprint 11: Cổng Bác Sĩ & Tiếp Nhận Khám
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Thiết lập cổng làm việc (Doctor Workspace) cho bác sĩ thú y. Hệ thống phân phối và hiển thị danh sách hàng đợi các bệnh nhân thú cưng được chỉ định khám cho bác sĩ đang đăng nhập (PB20). Bác sĩ có khả năng kích hoạt ca khám lâm sàng (PB22), chuyển trạng thái cuộc hẹn y tế sang `In_Progress` và tự động mở giao diện khám bệnh.

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T31] Xem lịch khám của bác sĩ - API Hàng đợi (Backend)
*   **Người thực hiện:** Nam (6 giờ)
*   **Nội dung công việc:**
    *   Xây dựng API truy vấn danh sách hàng đợi bệnh nhân (`QueueEntry`) được gán cho bác sĩ hiện tại (giải mã `DoctorId` từ JWT Token) có trạng thái là `Waiting` hoặc `Calling`.
    *   Lọc dữ liệu chính xác theo ngày hiện tại.

### 2. [T32] Dashboard hàng đợi của Bác sĩ (Frontend)
*   **Người thực hiện:** Phương (8 giờ)
*   **Nội dung công việc:**
    *   Thiết kế giao diện Dashboard bác sĩ hiển thị danh sách thú cưng đang xếp hàng chờ.
    *   Hiển thị thông tin cơ bản: Tên thú cưng, Tên chủ nuôi, Giờ hẹn, Trạng thái trong hàng đợi.

### 3. [T33] Tiếp nhận ca khám lâm sàng - Đổi trạng thái (Backend)
*   **Người thực hiện:** Nam (4 giờ)
*   **Nội dung công việc:**
    *   Xây dựng API `POST /api/doctor/appointments/{id}/start` đổi trạng thái cuộc hẹn y tế thành `In_Progress` và đổi trạng thái QueueEntry tương ứng sang `Serving`.

### 4. [T34] Kích hoạt chuyển trang khám (Frontend)
*   **Người thực hiện:** Phương (6 giờ)
*   **Nội dung công việc:**
    *   Khi bác sĩ nhấn nút "Bắt đầu khám" trên Dashboard hàng đợi, gửi request lên Backend và tự động chuyển hướng (router-push) sang trang ghi nhận bệnh án lâm sàng.

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Chỉ hiển thị dữ liệu được gán:** Bác sĩ A tuyệt đối không thể nhìn thấy hàng đợi khám bệnh của Bác sĩ B.
2.  **Chuyển trạng thái chính xác:** Khi bắt đầu khám, cả `Appointment.Status` chuyển sang `In_Progress` và `QueueEntry.Status` chuyển sang `Serving`.
3.  **Điều hướng mượt mà:** Trải nghiệm chuyển hướng router phía Client hoạt động nhanh chóng, lưu trữ thông tin cuộc hẹn hiện tại vào Pinia store để chuẩn bị khám.
4.  **Security Auth:** Endpoints yêu cầu Role `Doctor` / `Vet`.
