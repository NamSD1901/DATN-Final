# 🗓️ Sprint 9: Tiếp Nhận & Duyệt Lịch Hẹn
## Lộ trình phát triển & Kế hoạch Sprint

---

## 🎯 Mục Tiêu Sprint
Thiết lập bàn làm việc (Dashboard) cho Lễ tân phòng khám y tế. Cho phép lễ tân duyệt/từ chối lịch hẹn trực tuyến của chủ nuôi gửi lên (kèm tự động gửi thư điện tử giải thích lý do khi hủy - PB17), thực hiện check-in cho các khách hàng đã đến đúng giờ, và tạo nhanh lịch khám vãng lai (walk-in) cho các ca khẩn cấp không đặt trước (PB15).

---

## 📋 Danh Sách Tasks (Task Backlog)

### 1. [T25] Tiếp nhận khách hàng - Check-in & Walk-in (Backend)
*   **Người thực hiện:** Nam (8 giờ)
*   **Nội dung công việc:**
    *   Xây dựng API `POST /reception/check-in` chuyển trạng thái lịch hẹn sang `Confirmed` hoặc chuẩn bị đưa vào hàng đợi khám lâm sàng.
    *   Xây dựng API `POST /reception/walk-in` cho phép tạo nhanh một cuộc hẹn ngay lập tức không cần đăng ký qua luồng đặt lịch phức tạp.

### 2. [T26] Giao diện Lễ tân Dashboard & Tìm kiếm nhanh (Frontend)
*   **Người thực hiện:** Lâm (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế bảng điều khiển Lễ tân: Bộ lọc lịch hẹn theo ngày hôm nay, thanh tìm kiếm nhanh số điện thoại khách hàng hoặc tên thú cưng.
    *   Tích hợp nút Check-in nhanh.

### 3. [T27] Xác nhận/Hủy lịch hẹn - Mail trigger SMTP (Backend)
*   **Người thực hiện:** Nam (8 giờ)
*   **Nội dung công việc:**
    *   Xây dựng API cập nhật trạng thái lịch hẹn `PUT /reception/appointments/{id}/status`.
    *   Tích hợp background event gửi Mail thông báo tới khách hàng khi lịch hẹn bị hủy bởi nhân viên (nêu rõ lý do hủy nhập từ form).

### 4. [T28] Dashboard chờ duyệt & Modal lý do hủy (Frontend)
*   **Người thực hiện:** Lâm (10 giờ)
*   **Nội dung công việc:**
    *   Thiết kế danh sách các cuộc hẹn ở trạng thái `Pending` chờ duyệt.
    *   Xây dựng Modal xác nhận hủy: Bắt buộc Lễ tân nhập lý do hủy trước khi gửi yêu cầu lên Backend.

---

## 🔍 Tiêu Chí Nghiệm Thu (Definition of Done - DoD)
1.  **Gửi thư tự động:** Đảm bảo thư điện tử (email) được gửi thành công đến hòm thư của khách hàng trong vòng 10 giây sau khi bấm nút hủy lịch.
2.  **Walk-in nhanh chóng:** Lễ tân có thể đăng ký walk-in chỉ với 3 trường thông tin tối thiểu: SĐT chủ nuôi, Tên thú cưng, lý do khám lâm sàng.
3.  **Realtime UI:** Giao diện dashboard tự động cập nhật danh sách lịch hẹn khi có thay đổi trạng thái mà không cần tải lại toàn bộ trang.
