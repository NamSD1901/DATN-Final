# 🗓️ Sprint 10: Điều Phối Hàng Đợi
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng động cơ cấp số thứ tự tự động (Queue Entry) cho phòng khám khi có khách hàng Check-in hoặc Walk-in (PB18), giúp lễ tân điều phối luân chuyển bệnh nhân qua các trạng thái khám bệnh.

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T29] Quản lý hàng đợi - Tự động cấp số thứ tự khám (Backend)
*   **Người thực hiện:** Hạnh (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `QueueEntry` liên kết lịch hẹn hoặc thông tin walk-in với số thứ tự (`QueueNumber` dạng `Q-001`, `Q-002`...).
    *   Xây dựng thuật toán tăng dần số thứ tự trong ngày: Tìm số lớn nhất của ngày hôm nay, cộng 1 và định dạng chuỗi. Reset số thứ tự về `1` vào đầu ngày mới.
    *   Hỗ trợ chuyển đổi trạng thái của phần tử trong hàng đợi: `Waiting` -> `Calling` -> `Serving` -> `Completed`/`Skipped`.

### 2. [T30] Giao diện Điều phối Lễ tân (Frontend)
*   **Người thực hiện:** Lâm (10 giờ)
*   **Nội dung công việc:**
    *   Xây dựng bảng điều khiển Lễ tân: Nút Gọi số tiếp theo, Bỏ qua (Skip), Hoàn thành (Complete).

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Cấp số thứ tự chính xác:** Định dạng số thứ tự bắt buộc là `Q-XXX` (ví dụ: Q-001) và tự động bắt đầu lại từ `Q-001` vào lúc 00:00:00 mỗi ngày.
2.  **Đồng bộ tức thời:** Khi lễ tân bấm "Gọi số tiếp theo" (Next), trạng thái cập nhật mới mà không cần tải lại trình duyệt.
