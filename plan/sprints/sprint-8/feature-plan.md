# 🗓️ Sprint 8: Theo Dõi Cuộc Hẹn & Lịch Sử
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Giúp khách hàng (Chủ nuôi) theo dõi được trạng thái thời gian thực của các cuộc hẹn đã đặt (PB11) và tra cứu hồ sơ bệnh án, lịch sử sử dụng dịch vụ của thú cưng dưới dạng trục thời gian (Timeline) trực quan (PB12), đảm bảo phân quyền chặt chẽ chống IDOR.

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T23] Quản lý lịch hẹn - Phân trang & Lọc trạng thái (Backend)
*   **Người thực hiện:** Hạnh (6 giờ)
*   **Nội dung công việc:**
    *   Xây dựng API Get danh sách lịch hẹn của chủ tài khoản đang đăng nhập (`currentUserId`).
    *   Hỗ trợ các tham số truy vấn: `page`, `pageSize` để phân trang dữ liệu, và `status` (Pending/Confirmed/Cancelled...) để lọc.
    *   Tối ưu hóa LINQ Query bằng `AsNoTracking` để tăng tốc độ tải.

### 2. [T24] Theo dõi lịch hẹn - Timeline View (Frontend)
*   **Người thực hiện:** Lâm (8 giờ)
*   **Nội dung công việc:**
    *   Xây dựng UI hiển thị danh sách lịch hẹn dưới dạng danh sách cuộn mượt và timeline.
    *   Phân biệt màu sắc trực quan theo trạng thái: Đỏ (Cancelled), Xanh lá (Completed/Confirmed), Vàng (Pending).

### 3. [T44] Xem lịch sử dịch vụ & Bệnh án - Trục thời gian (Frontend)
*   **Người thực hiện:** Lâm (8 giờ)
*   **Nội dung công việc:**
    *   Tích hợp giao diện Trục thời gian y khoa (Medical History Timeline) hiển thị chi tiết các lần khám cũ, bác sĩ điều trị, chẩn đoán bệnh và đơn thuốc đã kê của từng thú cưng.

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Phân trang hiệu quả:** API trả về siêu dữ liệu phân trang (PageNumber, PageSize, TotalRecords, TotalPages).
2.  **Bảo mật Tuyệt đối chống IDOR:** Người dùng A tuyệt đối không thể xem lịch hẹn hoặc bệnh án thú cưng của Người dùng B thông qua việc đoán/đổi ID trên URL hoặc API Payload.
3.  **UI Premium:** Giao diện Timeline thiết kế mờ kính (Glassmorphism), có hiệu ứng hover mượt mà và responsive trên thiết bị di động.
4.  **Tối ưu hiệu suất:** Thời gian phản hồi của API truy vấn lịch sử dưới 200ms.
