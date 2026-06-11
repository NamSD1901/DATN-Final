# 🗓️ Sprint 7: Đặt Lịch Khám & Tiêm Chủng Trực Tuyến
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng chức năng đặt lịch khám bệnh trực tuyến (PB09) và đặt lịch tiêm phòng kèm lựa chọn vaccine (PB10). Điểm cốt lõi là giải quyết triệt để nguy cơ đặt trùng lịch (double-booking) tại tầng Backend và thiết kế UI Wizard đa bước mượt mà ở Frontend Vue 3.

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T19] Đặt lịch khám bệnh - Chống double-booking (Backend)
*   **Người thực hiện:** Nam (12 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `Appointment` (Chứa các trường: Thú cưng, Bác sĩ, Ngày, Khung giờ, Trạng thái, Lý do khám).
    *   Sử dụng giao dịch cơ sở dữ liệu ở mức cô lập cao (`IsolationLevel.Serializable`) hoặc cơ chế khóa lạc quan (Optimistic Concurrency) để chặn đứng tình trạng 2 khách hàng đặt chung một bác sĩ tại cùng một khung giờ cùng lúc.
    *   Xây dựng API `/api/appointments` nhận thông tin đặt lịch.

### 2. [T20] Wizard Form đặt lịch động (Frontend)
*   **Người thực hiện:** Phương (14 giờ)
*   **Nội dung công việc:**
    *   Xây dựng form đặt lịch chia làm nhiều bước (Wizard): Chọn thú cưng -> Chọn dịch vụ/Bác sĩ -> Chọn Ngày trực & Khung giờ khả dụng -> Nhập lý do khám & Xác nhận.
    *   Gắn kết Pinia store để lưu trạng thái tạm thời giữa các bước khám.

### 3. [T21] Đặt lịch tiêm chủng - Liên kết Vaccine & Kiểm kho (Backend)
*   **Người thực hiện:** Nam (8 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể liên kết `AppointmentVaccine` đính kèm thông tin vaccine được chọn khi đặt lịch.
    *   Kiểm tra số lượng vaccine tồn kho trước khi cho phép đặt lịch tiêm chủng thành công.

### 4. [T22] Tích hợp chọn vaccine vào Wizard (Frontend)
*   **Người thực hiện:** Phương (10 giờ)
*   **Nội dung công việc:**
    *   Khi người dùng chọn dịch vụ là "Tiêm phòng", hiển thị thêm bước chọn loại Vaccine (được lấy động từ API danh mục vaccine còn hàng).

---

## 🔍 Tiêu Chỉ Nghiệm Thu (Definition of Done - DoD)
1.  **Chống trùng tuyệt đối:** Test đồng thời (concurrency test) chứng minh khi gửi 2 request đặt lịch cùng 1 slot bác sĩ tại cùng 1 giây, chỉ có tối đa 1 request thành công.
2.  **Kiểm tra kho Vaccine:** Không cho phép đặt lịch tiêm chủng nếu loại vaccine được chọn có số lượng tồn kho = 0.
3.  **Trải nghiệm người dùng:** Giao diện Wizard chuyển bước mượt mà, tự động cập nhật slot rảnh ngay khi bác sĩ hoặc ngày thay đổi.
4.  **Bảo mật IDOR:** Đảm bảo `PetId` gửi lên thuộc quyền sở hữu của chính User đang đăng nhập (`currentUserId`).
