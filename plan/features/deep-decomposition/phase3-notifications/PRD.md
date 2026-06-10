# 🚀 Product Requirements Document (PRD) - Automatic Notification Service

## 1. Tổng quan & Tầm nhìn
Phân hệ **Thông báo & Nhắc lịch tự động (Automatic Notification Service)** giúp MyPetClinic chủ động chăm sóc khách hàng bằng cách nhắc lịch tiêm chủng vắc-xin tự động cho thú cưng và thông báo trạng thái lịch hẹn thời gian thực. Bằng cách gửi email nhắc nhở trước ngày tái chủng 3-5 ngày, hệ thống giúp duy trì tỉ lệ tiêm phòng định kỳ đầy đủ, bảo vệ sức khỏe vật nuôi và nâng cao chất lượng dịch vụ của phòng khám.

---

## 2. Đối tượng sử dụng (Target Persona)
- **Khách hàng (Customer):** Không cần nhớ lịch tiêm phức tạp của thú cưng, nhận được thông báo nhắc nhở kịp thời.
- **Hệ thống (System/Background Job):** Tự động vận hành ngầm hàng ngày mà không cần sự can thiệp thủ công từ nhân viên phòng khám.

---

## 3. Yêu cầu Nghiệp vụ Chi tiết
- **Gửi Email Nhắc lịch Tiêm chủng tự động (PB34):**
  - Hệ thống chạy ngầm quét bảng nhật ký tiêm phòng (`VaccinationRecords`) định kỳ vào lúc 08:00 sáng mỗi ngày.
  - Tìm kiếm các mũi tiêm có ngày tái chủng dự kiến (`NextDoseDate`) cách ngày hiện tại chính xác 3 ngày hoặc 5 ngày.
  - Tự động biên soạn nội dung email cá nhân hóa (Tên chủ nuôi, tên thú cưng, loại vắc-xin cần tái chủng, liên kết đặt lịch nhanh) và gửi tới email của khách hàng.
- **Hộp thư Thông báo Hệ thống (In-app Notification):**
  - Khách hàng xem danh sách thông báo hệ thống trực tiếp trên ứng dụng.
  - Đánh dấu đã đọc tất cả hoặc từng thông báo.
  - Thông báo hiển thị real-time khi lễ tân phê duyệt hoặc hủy lịch hẹn.
- **Bảo mật:**
  - Khách hàng chỉ được phép xem thông báo của chính tài khoản của mình.
