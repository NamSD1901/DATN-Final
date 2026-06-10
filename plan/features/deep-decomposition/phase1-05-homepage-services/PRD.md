# 🚀 Product Requirements Document (PRD) - Homepage Services (Phase 1)

## 1. Tổng quan Dự án (Overview)
Tính năng **Danh mục Dịch vụ Trang chủ** (Homepage Services) là giao diện công khai giới thiệu năng lực chuyên môn của phòng khám **MyPetClinic**. Nó cho phép khách hàng vãng lai và thành viên xem toàn bộ danh sách dịch vụ y tế, tiêm phòng, phẫu thuật, spa thú cưng kèm bảng giá minh bạch, hỗ trợ tìm kiếm và lọc theo danh mục nhanh chóng trước khi quyết định đặt lịch khám.

---

## 2. Mục tiêu Sản phẩm (Goals)
*   **Minh bạch giá cả và dịch vụ:** Cung cấp thông tin mô tả chi tiết, giá tiền cụ thể cho từng dịch vụ để khách hàng dễ dàng tham khảo.
*   **Trải nghiệm lọc tìm kiếm nhanh:** Hỗ trợ tìm kiếm theo từ khóa tên dịch vụ và lọc nhanh theo nhóm dịch vụ (Khám lâm sàng, Tiêm phòng, Phẫu thuật, Chăm sóc/Spa).
*   **Thúc đẩy tỷ lệ chuyển đổi đặt lịch (Conversion Rate):** Tích hợp nút đặt lịch nhanh (Book Now) trên từng thẻ dịch vụ, hướng người dùng thẳng đến Form đặt lịch tương ứng.

---

## 3. Chân dung Người dùng (User Personas & Stories)

### 3.1. Chân dung Người dùng
*   **Khách hàng tham khảo dịch vụ (Chị Vy, 25 tuổi):**
    *   *Bối cảnh:* Chị Vy có một bé mèo Ba Tư cần được tỉa lông và tắm sấy. Chị chưa từng tới MyPetClinic nên muốn truy cập trang chủ để xem phòng khám có dịch vụ "Spa/Grooming mèo" không và giá cả thế nào để chuẩn bị tài chính.
    *   *Nhu cầu:* Giao diện dạng thẻ trực quan, hình ảnh minh họa rõ ràng, bộ lọc danh mục nhạy bén trên trình duyệt di động.
*   **Chủ nuôi thú cưng khẩn cấp (Anh Hùng, 34 tuổi):**
    *   *Bối cảnh:* Bé chó của anh Hùng nuốt phải dị vật và đang có dấu hiệu khó thở. Anh Hùng truy cập nhanh vào website MyPetClinic để xem phòng khám có dịch vụ "Phẫu thuật khẩn cấp / Cấp cứu" không và số hotline liên hệ.
    *   *Nhu cầu:* Tốc độ tải trang cực nhanh (<100ms), thông tin dịch vụ cấp cứu hiển thị rõ ràng, dễ nhìn thấy hotline liên hệ.

### 3.2. User Stories
*   Là một khách hàng mới, tôi muốn xem bảng giá chi tiết các dịch vụ tại trang chủ để tôi không phải gọi điện hỏi giá trực tiếp.
*   Là một chủ thú cưng, tôi muốn lọc dịch vụ theo danh mục "Tiêm chủng" hoặc "Spa" để tôi dễ dàng tìm thấy dịch vụ cần thiết mà không phải cuộn trang tìm kiếm thủ công.
*   Là một người dùng di động, tôi muốn các thẻ dịch vụ tự động co giãn hiển thị tốt trên màn hình nhỏ để tôi đọc thông tin mô tả dịch vụ dễ dàng.

---

## 4. Phạm vi Tính năng (Scope of Work)

### 4.1. Trong phạm vi (In-Scope - MVP)
*   Hiển thị danh sách dịch vụ dạng lưới (Grid layout) với các trường: Tên dịch vụ, Mô tả ngắn, Giá tiền, Danh mục, Hình ảnh đại diện.
*   Thanh tìm kiếm theo từ khóa (Search bar) thời gian thực.
*   Thanh Tab lọc danh mục (Category filter tabs): *Tất cả*, *Khám bệnh*, *Tiêm phòng*, *Phẫu thuật*, *Spa/Làm đẹp*.
*   Nút bấm "Đặt lịch ngay" trên mỗi thẻ dịch vụ (Yêu cầu đăng nhập, nếu chưa đăng nhập chuyển hướng sang trang Login).
*   Áp dụng bộ đệm (Caching) ở Backend để tăng tốc độ phản hồi danh sách dịch vụ công khai.

### 4.2. Ngoài phạm vi (Out-of-Scope - Các phase tiếp theo)
*   Thanh toán tiền dịch vụ trực tuyến ngay tại trang chủ (Sẽ làm ở phân hệ Thanh toán thu ngân sau).
*   Đánh giá và bình luận (Review/Comment) chất lượng dịch vụ của khách hàng dưới mỗi thẻ dịch vụ.

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements)
*   **Tốc độ tải danh sách:** Thời gian API phản hồi danh sách dịch vụ khi có Caching phải dưới **100ms** (ở điều kiện thông thường).
*   **Thiết kế hình ảnh (Visuals):** Sử dụng các ảnh minh họa độ phân giải cao, bo tròn góc, có hiệu ứng zoom nhẹ khi rê chuột (hover).
*   **Khả năng tương thích responsive:** Chuyển đổi hiển thị linh hoạt từ 4 cột (màn hình PC rộng) sang 2 cột (Tablet) và 1 cột (Mobile).
