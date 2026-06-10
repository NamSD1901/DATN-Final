# 🚀 Product Requirements Document (PRD) - Receptionist Portal & Queue Management

## 1. Tổng quan & Tầm nhìn (Overview & Vision)

Phân hệ **Cổng Lễ Tân & Điều phối Hàng đợi khám (Receptionist Portal & Queue Management)** đóng vai trò là "bộ não điều hành" trực tiếp mọi hoạt động tiếp đón vật lý tại phòng khám **MyPetClinic**. 

Tại sảnh tiếp đón, tốc độ xử lý và sự chính xác là yếu tố sống còn quyết định sự hài lòng của khách hàng:
*   **Tiếp nhận không chạm:** Tích hợp quét mã QR Token từ thiết bị di động của khách hàng để check-in tức thời dưới 1 giây.
*   **Cân bằng tải phòng khám:** Thuật toán phân bổ tự động đưa thú cưng vào hàng đợi khám của bác sĩ trực ca phù hợp nhất, giảm thiểu thời gian chờ đợi trung bình.
*   **Đồng bộ hóa nghiệp vụ:** Kết nối đồng thời luồng đặt lịch trực tuyến và luồng khách vãng lai (Walk-in) vào một hàng đợi duy nhất trên Kanban Board.
*   **Minh bạch sảnh chờ:** Xuất bản màn hình tivi công cộng hiển thị số thứ tự đang khám, giúp chủ nuôi chủ động theo dõi lượt khám của bé.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)

### 👩‍💼 Persona 1: Nguyễn Thị Mai (Lễ tân trưởng phòng khám)
*   **Mục tiêu:** Tiếp nhận trung bình 100-150 ca khám/ngày nhanh chóng, không bị nhầm lẫn hồ sơ bệnh án, duyệt lịch hẹn của khách hàng đặt trước đúng giờ và điều phối các bé cún/mèo vào các phòng khám một cách khoa học để bác sĩ không bị quá tải hay ngồi chơi.
*   **Nỗi đau:** Giờ cao điểm (17:00 - 19:30) khách hàng xếp hàng dài, thú cưng sủa náo loạn tại sảnh, phần mềm cũ chạy chậm, thao tác qua lại nhiều tab tìm tên chủ nuôi gây ức chế và dễ làm thất lạc số thứ tự.

### 👨‍💼 Persona 2: Hoàng Văn Nam (Khách hàng vãng lai - Walk-in)
*   **Mục tiêu:** Mang bé mèo bị sốt đột xuất đến khám trực tiếp mà không đặt lịch từ trước. Muốn lễ tân lấy thông tin nhanh, cấp số thứ tự khám rõ ràng và biết bé sẽ được khám ở phòng nào, khoảng bao lâu nữa tới lượt.
*   **Nỗi đau:** Phải điền tờ khai giấy rườm rà, xếp hàng chờ đợi lâu mà không biết khi nào đến lượt khám của bé.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Check-in thông minh & Tìm kiếm nhanh
*   **Là một** Lễ tân phòng khám,
*   **Tôi muốn** check-in nhanh lịch hẹn của khách hàng bằng quét mã QR hoặc tìm kiếm số điện thoại/tên,
*   **Để** hoàn tất thủ tục tiếp nhận và cấp số thứ tự khám trong vòng 5 giây.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Giao diện hỗ trợ cổng nhập liệu autofocus để quét mã QR bằng máy quét USB HID. Quét thành công tự động phân giải `QrToken` ➡️ Tải thông tin lịch hẹn ➡️ Đổi trạng thái từ `confirmed` sang `waiting` (Xếp hàng).
    *   **AC2:** Trường tìm kiếm thông minh hỗ trợ tự động gợi ý (Autocomplete) khi nhập Tên chủ nuôi, SĐT, hoặc Tên thú cưng.
    *   **AC3:** Khi tiếp nhận thành công, hệ thống cấp một Số thứ tự khám (`QueueNumber`) dạng `Q-XXX` tăng dần trong ngày và in phiếu khám nhiệt tự động.

### User Story 2: Đăng ký nhanh Khách vãng lai (Walk-in Registration)
*   **Là một** Lễ tân phòng khám,
*   **Tôi muốn** đăng ký nhanh thông tin chủ nuôi mới và thú cưng trực tiếp ngay tại màn hình tiếp đón,
*   **Để** cấp số thứ tự khám cho khách vãng lai không đặt trước mà không làm gián đoạn hàng đợi.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Cung cấp Form rút gọn (Quick Form) tạo mới Khách hàng (Họ tên, SĐT) và Thú cưng (Tên, Loài chó/mèo, Giống) trên cùng một cửa sổ modal.
    *   **AC2:** Sau khi lưu thành công, hệ thống tự động sinh một Lịch hẹn khám ngay ở trạng thái `waiting` và xếp vào hàng đợi tương tự khách đặt trước.

### User Story 3: Quản lý Duyệt lịch hẹn trực tuyến (Pending Approvals Portal)
*   **Là một** Lễ tân,
*   **Tôi muốn** xem danh sách các lịch hẹn chờ duyệt của ngày hiện tại và tương lai,
*   **Để** duyệt xác nhận giờ khám cho khách hoặc từ chối hủy lịch nếu trùng ca phẫu thuật đột xuất.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Hiển thị danh sách lịch hẹn trạng thái `pending`.
    *   **AC2:** Nút "Duyệt" (Confirm): Chuyển trạng thái sang `confirmed`, phân bổ bác sĩ trực ca và kích hoạt email thông báo thành công cho khách.
    *   **AC3:** Nút "Từ chối" (Reject): Yêu cầu nhập lý do từ chối (tối thiểu 10 ký tự), chuyển trạng thái sang `cancelled` và gửi email thông báo hủy kèm lý do.

### User Story 4: Bảng điều phối Hàng đợi khám (Kanban Queue Board)
*   **Là một** Lễ tân hoặc Bác sĩ điều trị,
*   **Tôi muốn** xem bảng phân phối hàng đợi Kanban thời gian thực,
*   **Để** biết chính xác bé nào đang chờ khám, bé nào đang khám trong phòng khám nào, và ca nào đã hoàn tất.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Giao diện hiển thị Kanban 3 cột:
        *   `Đang chờ khám (Waiting)`
        *   `Đang khám (In Progress)`
        *   `Hoàn tất khám (Completed)`
    *   **AC2:** Hỗ trợ tính năng kéo thả (Drag & Drop) hoặc click chuyển trạng thái nhanh để di chuyển thú cưng giữa các cột hàng chờ.
    *   **AC3:** Khi di chuyển sang `In Progress`, hệ thống yêu cầu gán Phòng khám chỉ định (Clinic Room ID / Bác sĩ khám).

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Dashboard điều hợp Lễ tân 3 trong 1: Bộ lọc tìm kiếm & Check-in QR, Cổng duyệt lịch, Bảng Kanban hàng đợi khám.
*   Quick Form đăng ký khách vãng lai (Chủ nuôi + Thú cưng) nhanh.
*   Thuật toán phân bổ số thứ tự tự động (`QueueNumberGenerator`) và đề xuất phòng khám.
*   Màn hình Public Queue Board công cộng (Tivi sảnh chờ) cập nhật realtime qua SignalR / WebSocket.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Gọi loa phát thanh tự động bằng AI đọc tên số thứ tự khám (Sẽ nghiên cứu tích hợp Text-To-Speech ở Phase sau).
*   Quản lý gọi xe đưa đón thú cưng cấp cứu tận nơi.

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements - NFRs)
*   **Tốc độ & Realtime:** Bảng Kanban hàng đợi khám và màn hình tivi công cộng phải cập nhật trạng thái đồng bộ tức thời dưới **500ms** khi có thay đổi trạng thái từ Lễ tân hoặc Bác sĩ (sử dụng SignalR).
*   **Độ bền bỉ dữ liệu:** Số thứ tự khám (`QueueNumber`) phải đảm bảo không bị trùng lặp hoặc nhảy cóc số ngay cả khi hệ thống mất điện đột ngột và khởi động lại giữa ngày (sử dụng Sequence Postgres lưu trữ).
*   **Giao diện Ergonomic:** Thiết kế font chữ lớn, khoảng cách click rộng để lễ tân thao tác nhanh bằng màn hình cảm ứng (Touchscreen) hoặc Tablet cầm tay khi di chuyển trong sảnh.
