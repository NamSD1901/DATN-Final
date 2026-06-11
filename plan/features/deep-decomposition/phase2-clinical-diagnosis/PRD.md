# 🚀 Product Requirements Document (PRD) - Clinical Diagnosis & Treatment

## 1. Tổng quan & Tầm nhìn (Overview & Vision)

Phân hệ **Chẩn đoán lâm sàng & Kê đơn điều trị (Clinical Diagnosis & Treatment)** là hạt nhân chuyên môn của phòng khám **MyPetClinic**. Đây là không gian làm việc chuyên nghiệp (Doctor's Workbench) hỗ trợ các Bác sĩ thú y đưa ra các quyết định y tế chuẩn xác nhất:
*   **Quản trị quy trình khám liền mạch:** Theo dõi hàng chờ bệnh nhân được phân bổ, ghi nhận triệu chứng lâm sàng, và tiến hành chẩn đoán.
*   **Tra cứu y bạ tức thời:** Cung cấp dòng thời gian (Timeline) bệnh sử toàn diện của thú cưng chỉ với 1 click, bao gồm cả lịch sử tiêm phòng cũ.
*   **Kê đơn an toàn y khoa & Tồn kho dược:** Tích hợp bộ tìm kiếm thuốc thông minh, kiểm tra số lượng khả dụng trong kho thời gian thực, ngăn chặn tình trạng kê đơn khống hoặc kê thuốc đã hết hàng.
*   **Liên kết hóa đơn tự động:** Chuyển đổi bệnh án đã hoàn thành sang quầy thu ngân dưới dạng hóa đơn nháp, tối ưu hóa quy trình thanh toán khép kín.

---

## 2. Đối tượng Người dùng & Hành vi (User Personas)

### 🥼 Persona 1: BS. Trần Quốc Anh (Bác sĩ thú y chính của phòng khám)
*   **Mục tiêu:** Khám chữa bệnh cho từ 15-20 bé cún/mèo mỗi ca trực. Cần tra cứu nhanh bệnh sử y tế cũ của bé (các triệu chứng trước đây, các loại thuốc đã từng dị ứng). Khi kê đơn, anh muốn hệ thống tự động cảnh báo nếu thuốc được chọn hiện tại đã hết hoặc sắp hết hàng tại kho dược để anh đổi sang hoạt chất tương đương.
*   **Nỗi đau:** Các ca bệnh dồn dập vào giờ cao điểm. Việc gõ nhập tên thuốc dài phức tạp rất mất thời gian. Bệnh án giấy cũ không lưu vết đầy đủ dẫn đến chẩn đoán sai sót.

### 👩‍💼 Persona 2: Nguyễn Thu Hà (Chủ nuôi đưa thú cưng đến khám)
*   **Mục tiêu:** Muốn bác sĩ nắm rõ tiền sử bệnh của bé mèo cưng mà không cần cô phải giải thích lại từ đầu (vì cô không nhớ rõ tên các loại thuốc bé từng uống).
*   **Nỗi đau:** Việc chờ đợi bác sĩ viết tay đơn thuốc lâu, và thỉnh thoảng quầy thuốc báo hết loại thuốc bác sĩ vừa kê dẫn đến việc phải quay lại phòng khám xin đổi đơn.

---

## 3. Quy trình Nghiệp vụ & Kịch bản Sử dụng (User Stories & Acceptance Criteria)

### User Story 1: Hàng chờ khám lâm sàng của Bác sĩ
*   **Là một** Bác sĩ thú y,
*   **Tôi muốn** xem danh sách các bé cưng được chỉ định khám cho tôi theo số thứ tự (QueueNumber) từ lễ tân truyền sang,
*   **Để tôi** tiếp nhận ca khám đúng trình tự thời gian.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Giao diện hiển thị danh sách hàng chờ phân theo phòng khám của bác sĩ hiện tại đang đăng nhập.
    *   **AC2:** Hỗ trợ nút "Tiếp nhận" (Start Exam) ➡️ Chuyển trạng thái lịch hẹn từ `waiting` sang `in_progress`, cập nhật thời điểm bắt đầu khám `StartExamTime`.
    *   **AC3:** Khi bác sĩ đang trong ca khám `in_progress`, hệ thống khóa ca khám này trên bảng điều khiển của các bác sĩ khác để tránh tiếp nhận chồng chéo.

### User Story 2: Tra cứu Bệnh sử & Lịch sử Tiêm phòng nhanh
*   **Là một** Bác sĩ thú y đang trong ca khám,
*   **Tôi muốn** xem dòng thời gian (Timeline) bệnh sử chi tiết của thú cưng,
*   **Để tôi** đưa ra chẩn đoán chính xác dựa trên lịch sử điều trị trước đây.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Hiển thị nút "Xem Bệnh Sử" trên màn hình khám hiện tại. Click mở Panel bệnh sử dạng Timeline ngược thời gian (Mới nhất ở trên).
    *   **AC2:** Timeline trình bày rõ ràng: Ngày khám, Triệu chứng, Chẩn đoán của bác sĩ trước, và Đơn thuốc chi tiết đã kê (tên thuốc, liều dùng).
    *   **AC3:** Hiển thị danh sách các mũi vắc-xin bé đã tiêm và ngày dự kiến tiêm nhắc lại.

### User Story 3: Kê đơn thuốc thông minh & Tích hợp kiểm kho dược
*   **Là một** Bác sĩ thú y,
*   **Tôi muốn** kê đơn thuốc cho thú cưng bằng bộ tìm kiếm tự động điền (Autocomplete) và kiểm tra tồn kho thời gian thực,
*   **Để** đảm bảo đơn thuốc hợp lệ và thuốc được kê thực tế có sẵn trong kho dược phòng khám.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Bảng kê đơn cho phép thêm nhiều dòng thuốc. Trường nhập tên thuốc hỗ trợ gõ gợi ý (Autocomplete) theo tên thương mại hoặc tên hoạt chất dược lý.
    *   **AC2:** Khi chọn thuốc, hệ thống hiển thị số lượng tồn kho khả dụng hiện tại (`StockQuantity`) ngay bên cạnh.
    *   **AC3:** Chặn lưu đơn thuốc nếu số lượng kê đơn lớn hơn `StockQuantity` trong kho. Viền dòng thuốc báo đỏ kèm cảnh báo: *"Không đủ số lượng trong kho dược."*.

### User Story 4: Hoàn thành Chẩn đoán & Tự động tạo hóa đơn nháp
*   **Là một** Bác sĩ thú y,
*   **Tôi muốn** lưu ghi chép chẩn đoán và hoàn thành ca khám,
*   **Để** hệ thống tự động đẩy dữ liệu sang quầy thu ngân xử lý thanh toán cho khách hàng.
*   **Tiêu chí nghiệm thu (Acceptance Criteria):**
    *   **AC1:** Bác sĩ điền trường bắt buộc: Chẩn đoán lâm sàng (`Diagnosis`) và Hướng điều trị (`TreatmentPlan`).
    *   **AC2:** Nhấn nút "Hoàn thành ca khám" ➡️ Chuyển trạng thái lịch hẹn sang `completed`, ghi nhận `EndExamTime`.
    *   **AC3:** Hệ thống tự động phát sinh một Hóa đơn nháp (`Invoice` trạng thái `draft`) chứa tiền dịch vụ khám và toàn bộ chi phí thuốc trong đơn thuốc vừa kê, gửi tín hiệu realtime sang Cổng thu ngân.

---

## 4. Phạm vi Tính năng (Scope of Work)

### ✅ Trong phạm vi (In-Scope)
*   Dashboard làm việc của bác sĩ thú y tích hợp danh sách hàng chờ, bệnh án, và đơn thuốc trên cùng một màn hình.
*   Bộ gõ tự động gợi ý danh mục thuốc hoạt chất từ bảng dược phẩm `Medicines`.
*   Thuật toán đối chiếu số lượng kê đơn và tồn kho dược thời gian thực.
*   Cơ chế transaction nguyên tử (Atomic transaction) lưu bệnh án, cập nhật trạng thái lịch hẹn, trừ kho thuốc và sinh hóa đơn nháp.

### ❌ Ngoài phạm vi (Out-of-Scope)
*   Thực hiện pha chế thuốc tự động bằng máy móc (Nghiệp vụ dược sĩ thực tế tại quầy thuốc sau khi hóa đơn được thanh toán).
*   Chụp X-quang, siêu âm từ xa (Bác sĩ chỉ ghi nhận kết quả và đính kèm đường link file ảnh chụp X-quang được lưu trữ ở cloud riêng).

---

## 5. Yêu cầu Phi chức năng (Non-Functional Requirements - NFRs)
*   **Hiệu năng:** Tốc độ tìm kiếm gợi ý thuốc (Autocomplete) phải phản hồi dưới **150ms** khi gõ để đảm bảo không tạo độ trễ cho bác sĩ khi kê đơn.
*   **Bảo mật thông tin:** Chỉ có các tài khoản thuộc vai trò `doctor`, `receptionist` và `admin` mới được quyền truy cập xem hồ sơ bệnh sử chi tiết của thú cưng (chặn hoàn toàn truy cập nặc danh hoặc chéo tài khoản khách hàng khác).
*   **Tính toàn vẹn (ACID):** Quá trình trừ kho và sinh hóa đơn nháp phải nằm trong một transaction duy nhất. Bất kỳ lỗi hệ thống hoặc xung đột dữ liệu nào xảy ra ở bước cuối cũng bắt buộc phải khôi phục (Rollback) 100% dữ liệu kho dược về trạng thái cũ để tránh thất thoát thuốc.
