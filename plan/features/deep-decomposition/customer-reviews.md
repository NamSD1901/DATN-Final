# Đặc tả tính năng: Customer Reviews (Booking.com Style)

## 1. Giới thiệu (Overview)
Trang Customer Reviews cung cấp một không gian đáng tin cậy để khách hàng xem các đánh giá về chất lượng dịch vụ của phòng khám thú y. Giao diện được thiết kế theo phong cách hiện đại, cao cấp (Premium) lấy cảm hứng từ Booking.com, Airbnb, tập trung vào trải nghiệm người dùng với khoảng trắng lớn, typography rõ ràng và cơ chế lọc nâng cao.

## 2. Chân dung người dùng (User Personas)
- **Khách hàng tiềm năng:** Đang tìm kiếm phòng khám uy tín, muốn xem đánh giá thực tế trước khi đặt lịch.
- **Khách hàng cũ:** Muốn chia sẻ trải nghiệm (text, hình ảnh) sau khi khám xong.
- **Quản lý phòng khám:** Cần theo dõi mức độ hài lòng (CSAT) và phản hồi lại đánh giá của khách.

## 3. User Stories (Cốt truyện người dùng)
- `US-REV-01`: Là một khách hàng tiềm năng, tôi muốn xem điểm trung bình và biểu đồ phân bổ sao để nhanh chóng biết chất lượng tổng thể của phòng khám.
- `US-REV-02`: Là một khách hàng tiềm năng, tôi muốn lọc đánh giá theo Bác sĩ, Loài vật, Dịch vụ hoặc Số sao để xem trải nghiệm liên quan đến đúng nhu cầu của thú cưng của tôi.
- `US-REV-03`: Là một người dùng, tôi muốn thấy huy hiệu "Verified Visit" để đảm bảo rằng đây là đánh giá thật từ người đã sử dụng dịch vụ.
- `US-REV-04`: Là một khách hàng cũ, tôi muốn có thể tải lên ảnh đính kèm minh chứng cho đánh giá của tôi.
- `US-REV-05`: Là người dùng, tôi muốn xem được phòng khám đã phản hồi lại những đánh giá tiêu cực như thế nào để đánh giá tính chuyên nghiệp.

## 4. Acceptance Criteria (Tiêu chí nghiệm thu - AC)
### AC1: Review Summary (Tổng quan)
- Hiển thị Điểm trung bình (làm tròn 1 chữ số thập phân, ví dụ 4.9).
- Hiển thị Biểu đồ dạng progress bar cho từng mức sao từ 1 đến 5.
- Các chỉ số % (Quay lại, Tích cực, Đặt lịch) được tính toán chuẩn xác từ DB.

### AC2: Filters & Search (Lọc và Tìm kiếm)
- Hỗ trợ thanh Search (tìm theo từ khóa trong nội dung review).
- Sort mặc định là "Newest". Có thể sort theo "Highest/Lowest Rating".
- Các Checkbox Filters có thể kết hợp với nhau (Ví dụ: 5 sao + Chó + Tiêm Vaccine).
- Thanh filter phải dính lại (Sticky) khi cuộn trang.

### AC3: Danh sách Card Review
- Card bắt buộc hiển thị: Avatar, Tên, Badge Verified Visit, Ngày khám.
- Các tag bổ sung: Tên thú cưng, Giống, Tuổi, Dịch vụ đã khám.
- Hiển thị tối đa 4 ảnh đầu tiên ở dạng Grid, nhấn vào sẽ mở Lightbox Gallery đầy đủ.
- Nếu text quá 5-8 dòng, hiển thị nút "Read more" (ẩn bớt văn bản).
- Khối phản hồi từ phòng khám (nếu có) phải nằm trong Card bằng một background màu nhạt.

### AC4: Tương tác & UX
- Hiệu ứng Hover card nảy lên `translateY(-4px)` mượt mà.
- Dữ liệu tải theo cơ chế Infinite Scroll hoặc Pagination để tránh sập trình duyệt.
- Nếu không có đánh giá nào khớp với filter, hiển thị Empty State (Illustration + Nút "Xóa bộ lọc").

## 5. Edge Cases (Trường hợp biên)
- **Edge 1 (Filter quá sâu):** Khách chọn quá nhiều filter khiến kết quả = 0 -> Phải hiển thị UI rõ ràng thông báo không tìm thấy kết quả và gợi ý xóa bớt bộ lọc.
- **Edge 2 (Hình ảnh lỗi):** Nếu URL hình ảnh bị hỏng hoặc lỗi 404, cần có cơ chế hiển thị một ảnh fallback (Placeholder).
- **Edge 3 (Nội dung độc hại):** Review có chứa từ khóa bậy (nếu có bộ lọc, nhưng tạm thời người dùng có thể nhấn nút Report).
- **Edge 4 (Thiếu dữ liệu Pet):** Trường hợp appointment đó chưa liên kết với một Pet cụ thể (hiếm gặp) -> Ẩn dòng hiển thị thông tin Pet, không làm vỡ layout card.
- **Edge 5 (Đang load data):** Phải có Skeleton Loading nhấp nháy cho Card để tạo cảm giác mượt mà (perceived performance).

## 6. Lịch sử tài liệu (Revision History)
- **Phiên bản:** 1.0.0
- **Ngày:** [Current Date]
- **Tác giả:** Product Agent
- **Trạng thái:** ✅ CODE DONE.
