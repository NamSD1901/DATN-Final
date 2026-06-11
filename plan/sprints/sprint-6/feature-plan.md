# 🗓️ Sprint 6: Ca Làm Việc & Khung Giờ Bác Sĩ
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Thiết kế cấu trúc dữ liệu ca trực của bác sĩ và xây dựng động cơ (Helper Engine) tính toán các khung giờ khám trống (Slot Availability) của bác sĩ thú y. Đây là nền tảng cốt lõi trước khi xây dựng tính năng đặt lịch khám ở Sprint 7 nhằm ngăn chặn tình trạng trùng lịch (double-booking).

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T18] Thiết kế logic Slot & Ca làm việc (Backend)
*   **Người thực hiện:** Nam (12 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `DoctorSchedule` mô tả lịch đăng ký trực của bác sĩ theo ngày (ví dụ: Thứ Hai, Thứ Ba...) hoặc ngày cụ thể.
    *   Thiết kế thực thể `WorkShift` hoặc enum `ShiftType` định nghĩa các ca làm việc cố định (Sáng: 08:00 - 12:00, Chiều: 13:30 - 17:30).
    *   Xây dựng thuật toán trong `SlotCalculationHelper` tự động sinh các khung giờ nhỏ (ví dụ: mỗi slot 30 phút) và đối chiếu với danh sách các lịch hẹn (`Appointment`) hiện tại của bác sĩ để tìm ra các slot còn khả dụng (`AvailableSlots`).
    *   Viết Unit Tests kiểm tra tính chính xác của thuật toán phân rã và loại trừ slot bận.

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Định nghĩa thực thể:** Thực thể `DoctorSchedule` được định nghĩa rõ ràng cấu trúc và Fluent API mapping.
2.  **Độ chính xác thuật toán:** Thuật toán tính toán phải loại trừ chính xác các slot trùng với lịch hẹn đã được xác nhận hoặc đang chờ duyệt (trừ trạng thái đã hủy).
3.  **Cấu hình linh hoạt:** Khoảng cách giữa các slot (slot duration) có thể cấu hình được (mặc định 30 phút).
4.  **Bao phủ kiểm thử:** Unit Test bao phủ ít nhất 4 trường hợp biên của thuật toán sinh slot và kiểm tra khả dụng (không có lịch hẹn, lịch hẹn đè lên một phần ca trực, ca trực trống hoàn toàn, ca trực đã kín chỗ).
