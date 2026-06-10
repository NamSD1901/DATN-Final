# 🚀 Product Requirements Document (PRD) - Customer Appointments Dashboard

## 1. Tổng quan & Tầm nhìn (Overview & Vision)

Trong mô hình hoạt động của phòng khám thú y **MyPetClinic**, tính năng đặt lịch khám/tiêm phòng (Phase 2 - 10 & 11) chỉ là bước khởi đầu. Để hoàn thiện hành trình trải nghiệm người dùng, phân hệ **Bảng điều khiển quản lý lịch hẹn của Khách hàng (Customer Appointments Dashboard)** đóng vai trò là trung tâm tương tác sau đặt lịch:
*   **Quản trị thông tin minh bạch:** Cho phép chủ nuôi theo dõi toàn bộ trạng thái y tế của lịch hẹn từ lúc chờ duyệt, được tiếp nhận, đang khám cho đến khi hoàn thành thanh toán.
*   **Hủy lịch linh hoạt:** Cung cấp khả năng hủy lịch hẹn trực tuyến an toàn kèm lý do cụ thể, tự động giải phóng khung giờ rảnh của bác sĩ để đón tiếp khách hàng khác.
*   **Check-in thông minh:** Tích hợp hiển thị mã QR check-in độc bản, biến quá trình tiếp đón tại quầy lễ tân thành trải nghiệm một chạm.

Mục tiêu là xây dựng một dashboard quản lý thân thiện, giảm tỷ lệ vắng mặt không báo trước (No-show rate) và nâng cao hiệu suất vận hành của clinic.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)

### 👩‍💼 Persona 1: Trần Thu Trang (Chủ nuôi mèo bận rộn)
*   **Mục tiêu:** Đặt lịch tiêm vắc-xin cho bé mèo nhưng đột xuất lịch làm việc thay đổi. Trang cần truy cập nhanh vào Dashboard để xem lại ngày hẹn và thực hiện hủy lịch cũ để đặt lịch mới mà không cần phải gọi điện trực tiếp cho phòng khám.
*   **Nỗi đau:** Việc hủy lịch qua điện thoại rất mất thời gian và phiền toái. Trang muốn tự thao tác trên điện thoại và nhận được xác nhận ngay lập tức.

### 🥼 Persona 2: Bác sĩ Nguyễn Minh Đức (Bác sĩ thú y trực ca)
*   **Mục tiêu:** Muốn chủ nuôi đến đúng giờ và nắm rõ số lượng ca khám bị hủy trong ngày để sắp xếp ca phẫu thuật hoặc nghỉ ngơi.
*   **Nỗi đau:** Nhiều chủ nuôi không đến cũng không hủy lịch trực tuyến, dẫn đến việc bác sĩ ngồi chờ lãng phí thời gian trống trong ca trực.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Dashboard quản trị & Bộ lọc trạng thái thông minh
*   **Là một** khách hàng đã đăng nhập,
*   **Tôi muốn** xem danh sách toàn bộ các lịch hẹn khám và tiêm chủng của mình theo dạng timeline trực quan,
*   **Để tôi** quản lý thời gian chăm sóc sức khỏe cho các bé cún/mèo của mình một cách thuận tiện.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Giao diện hiển thị danh sách lịch hẹn sắp xếp theo thứ tự ngày hẹn giảm dần (Mới nhất hiển thị ở trên).
    *   **AC2:** Hỗ trợ các Tabs lọc trạng thái nhanh:
        *   `Tất cả` (All)
        *   `Chờ duyệt` (Pending)
        *   `Đã xác nhận` (Confirmed)
        *   `Đang khám` (In Progress)
        *   `Hoàn thành` (Completed)
        *   `Đã hủy` (Cancelled)
    *   **AC3:** Mỗi thẻ lịch hẹn phải trình bày rõ ràng: Ảnh đại diện & tên thú cưng, tên dịch vụ y tế/loại vắc-xin, tên bác sĩ phụ trách, ngày giờ khám, và badge trạng thái có màu sắc phân biệt đặc trưng.

### User Story 2: Xem chi tiết lịch hẹn & QR Code Check-in
*   **Là một** khách hàng đến phòng khám,
*   **Tôi muốn** mở nhanh chi tiết lịch hẹn để hiển thị mã QR Token,
*   **Để tôi** check-in nhanh tại quầy lễ tân mà không cần khai báo lại thông tin cá nhân.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Khi click vào một thẻ lịch hẹn, hệ thống hiển thị Modal chi tiết chứa: Mã lịch hẹn, triệu chứng/ghi chú, thông tin chi phí tạm tính (nếu là dịch vụ cụ thể), thông tin bác sĩ, và mã QR Token.
    *   **AC2:** QR Code phải được sinh tự động dựa trên mã token bảo mật dạng `QR-XXXXXX` được mã hóa từ ID lịch hẹn.

### User Story 3: Hủy lịch hẹn tự phục vụ (Self-service Cancellation)
*   **Là một** khách hàng có việc đột xuất,
*   **Tôi muốn** tự hủy lịch hẹn trực tuyến của mình trên Dashboard,
*   **Để** bác sĩ có thể tiếp đón thú cưng khác và tôi không bị đánh dấu vi phạm lịch hẹn.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Khách hàng chỉ được phép hủy lịch hẹn khi trạng thái hiện tại là `pending` (Chờ duyệt) hoặc `confirmed` (Đã xác nhận).
    *   **AC2:** Chặn hủy lịch hẹn khi trạng thái là `in_progress` (Đang khám), `completed` (Đã hoàn thành) hoặc lịch hẹn đã qua thời gian chỉ định (quá khứ).
    *   **AC3:** Yêu cầu người dùng điền lý do hủy lịch (Tối thiểu 10 ký tự) tại Popup xác nhận để lưu vết phân tích.
    *   **AC4:** Khi hủy thành công, hệ thống chuyển trạng thái lịch hẹn sang `cancelled`, giải phóng lịch rảnh của Bác sĩ trực ca đó trên DB, và gửi email thông báo tự động xác nhận hủy lịch cho khách hàng.

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Giao diện Dashboard danh sách lịch hẹn dạng Tabs lọc trạng thái thiết kế Glassmorphism.
*   Modal xem chi tiết lịch hẹn kèm bộ sinh ảnh QR Code phía client.
*   Popup xác nhận hủy lịch hẹn bắt buộc nhập lý do hủy.
*   API hủy lịch hẹn (`PUT /api/my-appointments/{id}/cancel`) có kiểm tra quyền sở hữu IDOR và ràng buộc trạng thái.
*   Gửi email tự động thông qua background job/event handler sau khi lịch hẹn bị hủy.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Khách hàng tự dời lịch (Reschedule) sang ngày/giờ khác trực tiếp trên lịch cũ (Sẽ phát triển ở phase sau, tạm thời khách hàng phải hủy lịch cũ và đặt lịch mới).
*   Thanh toán trực tuyến hóa đơn đặt cọc trước khi đặt lịch (Hiện tại thanh toán sẽ thực hiện trực tiếp tại quầy thu ngân sau khi khám xong).

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements - NFRs)
*   **Hiệu năng:** Thời gian tải danh sách lịch hẹn phải dưới 1.2s trong điều kiện mạng 3G/4G thông thường. Sử dụng phân trang hoặc Lazy Loading nếu danh sách lịch sử vượt quá 20 bản ghi.
*   **Bảo mật:** Chặn đứng 100% các request truy cập trái phép xem chi tiết hoặc hủy lịch hẹn của khách hàng khác (chống IDOR).
*   **Tính tương thích:** Giao diện Responsive hoạt động hoàn hảo trên các thiết bị di động (iOS/Android Safari/Chrome) để khách hàng dễ dàng xuất trình mã QR check-in tại quầy.
