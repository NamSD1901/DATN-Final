# 🚀 Product Requirements Document (PRD) - Homepage Vets Team (Phase 1)

## 1. Tổng quan Dự án (Overview)
Tính năng **Đội ngũ Bác sĩ Trang chủ** (Homepage Vets Team) hiển thị thông tin giới thiệu về đội ngũ bác sĩ thú y đang công tác tại phòng khám **MyPetClinic**. Nó cho phép khách hàng xem chi tiết chuyên môn, số năm kinh nghiệm, học hàm/học vị và lịch trực hiện tại của các bác sĩ, củng cố lòng tin của chủ nuôi và hỗ trợ họ chọn đúng bác sĩ phù hợp để đặt lịch hẹn khám cho thú cưng.

---

## 2. Mục tiêu Sản phẩm (Goals)
*   **Xây dựng lòng tin khách hàng:** Cung cấp hồ sơ cá nhân minh bạch của từng bác sĩ (Chuyên khoa, Học vị, Chứng chỉ quốc tế).
*   **Hỗ trợ lọc theo chuyên khoa:** Người dùng dễ dàng tìm kiếm bác sĩ chuyên về Nội khoa, Ngoại khoa/Phẫu thuật, Da liễu, hoặc Tiêm chủng.
*   **Liên kết đặt lịch trực ca:** Tích hợp nút đặt lịch trực tiếp với bác sĩ được chọn, tự động điền thông tin bác sĩ vào biểu mẫu đặt lịch.

---

## 3. Chân dung Người dùng (User Personas & Stories)

### 3.1. Chân dung Người dùng
*   **Chủ nuôi thú cưng lo lắng (Vy, 21 tuổi):**
    *   *Bối cảnh:* Chú mèo Ba Tư của Vy xuất hiện nhiều nốt đỏ dị ứng trên da và rụng lông từng mảng. Vy muốn tìm kiếm một bác sĩ thú y có chuyên môn sâu về **Da liễu thú cưng** tại MyPetClinic để đặt lịch khám chính xác, tránh việc khám chung chung không hiệu quả.
    *   *Nhu cầu:* Giao diện hiển thị rõ ràng chuyên khoa của bác sĩ da liễu, có nút đặt lịch khám nhanh với bác sĩ đó.
*   **Khách hàng vãng lai (Chú Tuấn, 48 tuổi):**
    *   *Bối cảnh:* Chú Tuấn nuôi một chú chó Becgie bị gãy chân do tai nạn. Chú muốn xem hồ sơ năng lực của các bác sĩ ngoại khoa tại phòng khám xem ai có kinh nghiệm phẫu thuật xương khớp tốt trước khi mang bé đến.
    *   *Nhu cầu:* Hiển thị rõ ràng số năm kinh nghiệm của bác sĩ phẫu thuật ngay trên card đại diện.

### 3.2. User Stories
*   Là một chủ nuôi thú cưng, tôi muốn xem thông tin chuyên môn và số năm kinh nghiệm của các bác sĩ để tôi có thể chọn được người điều trị tốt nhất cho thú cưng của mình.
*   Là một người đang duyệt web, tôi muốn lọc danh sách bác sĩ theo chuyên khoa (ví dụ: Phẫu thuật) để tôi nhanh chóng tìm ra các chuyên gia phẫu thuật ngoại khoa của phòng khám.
*   Là một khách hàng, tôi muốn biết bác sĩ nào đang có trạng thái trực ca hôm nay để tôi có thể đặt lịch hẹn khám được ngay.

---

## 4. Phạm vi Tính năng (Scope of Work)

### 4.1. Trong phạm vi (In-Scope - MVP)
*   Hiển thị danh sách bác sĩ thú y dưới dạng lưới thẻ (Grid cards) gồm: Ảnh chân dung, Họ tên, Học vị (Thạc sĩ, Bác sĩ), Chuyên khoa chính, Số năm kinh nghiệm, Trạng thái hoạt động (Đang trực ca/Nghỉ phép).
*   Thanh Tab lọc nhanh theo Chuyên khoa (Specialization filter tabs).
*   Nút bấm "Đặt lịch khám" liên kết trực tiếp, truyền `doctorId` vào Modal đặt lịch.
*   Lưu đệm Cache danh sách bác sĩ tĩnh ở Backend để tối ưu hiệu năng tải trang.

### 4.2. Ngoài phạm vi (Out-of-Scope - Các phase tiếp theo)
*   Xem lịch trực tuần chi tiết (Duty Calendar Grid) của từng bác sĩ ngay tại trang chủ.
*   Đánh giá xếp hạng sao (Rating) hoặc viết review trực tiếp của khách hàng cho từng bác sĩ.

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements)
*   **Thời gian phản hồi:** Tốc độ tải danh sách bác sĩ từ cache phải dưới **100ms** (ở điều kiện thông thường).
*   **Độ phân giải hình ảnh:** Ảnh chân dung bác sĩ phải sắc nét, có kích thước đồng nhất (ví dụ tỷ lệ 1:1 hoặc 3:4), hỗ trợ hiệu ứng làm mờ nền nhẹ khi hover chuột.
*   **Độ tương thích di động:** Tự động thu gọn lưới hiển thị từ 3-4 cột trên PC thành 1 cột đứng trên Mobile.
