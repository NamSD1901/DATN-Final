# 🗓️ Sprint 10: Điều Phối Hàng Đợi & TV Board
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Xây dựng động cơ cấp số thứ tự tự động (Queue Entry) cho phòng khám khi có khách hàng Check-in hoặc Walk-in (PB18), giúp lễ tân điều phối luân chuyển bệnh nhân qua các trạng thái khám bệnh, đồng thời thiết lập màn hình TV Board trực quan ngoài sảnh chờ hiển thị số thứ tự đang khám thời gian thực.

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T29] Quản lý hàng đợi - Tự động cấp số thứ tự khám (Backend)
*   **Người thực hiện:** Hạnh (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế thực thể `QueueEntry` liên kết lịch hẹn hoặc thông tin walk-in với số thứ tự (`QueueNumber` dạng `Q-001`, `Q-002`...).
    *   Xây dựng thuật toán tăng dần số thứ tự trong ngày: Tìm số lớn nhất của ngày hôm nay, cộng 1 và định dạng chuỗi. Reset số thứ tự về `1` vào đầu ngày mới.
    *   Hỗ trợ chuyển đổi trạng thái của phần tử trong hàng đợi: `Waiting` -> `Calling` -> `Serving` -> `Completed`/`Skipped`.

### 2. [T30] Giao diện TV Board ngoài sảnh & Điều phối (Frontend)
*   **Người thực hiện:** Lâm (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế giao diện TV Board trình chiếu ngoài sảnh chờ bằng kính mờ (Glassmorphism), chia làm 2 cột: "Đang khám" (Serving) và "Đang chờ" (Waiting) kèm giọng nói nhân tạo gọi số (giả lập hoặc Web Speech API).
    *   Xây dựng bảng điều khiển Lễ tân: Nút Gọi số tiếp theo, Bỏ qua (Skip), Hoàn thành (Complete).

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Cấp số thứ tự chính xác:** Định dạng số thứ tự bắt buộc là `Q-XXX` (ví dụ: Q-001) và tự động bắt đầu lại từ `Q-001` vào lúc 00:00:00 mỗi ngày.
2.  **Đồng bộ tức thời:** Khi lễ tân bấm "Gọi số tiếp theo" (Next), màn hình TV Board sảnh chờ lập tức cập nhật trạng thái mới mà không cần tải lại trình duyệt.
3.  **Hỗ trợ Web Speech API:** Phát âm thanh đọc số thứ tự bằng Tiếng Việt (ví dụ: "Xin mời thú cưng của khách hàng số Q không không một vào phòng khám").
