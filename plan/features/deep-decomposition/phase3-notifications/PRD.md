# 🚀 Product Requirements Document (PRD) - Automatic Notification Service

## 1. Tổng quan & Tầm nhìn (Overview & Vision)
Phân hệ **Thông báo & Nhắc lịch tự động (Automatic Notification Service)** đóng vai trò quan trọng trong việc chăm sóc khách hàng chủ động và duy trì tính liên tục của phác đồ y tế tại phòng khám MyPetClinic. Bằng việc tự động hóa khâu quét lịch tiêm vắc-xin và gửi thư nhắc nhở tái chủng, hệ thống giúp chủ nuôi dễ dàng tuân thủ lịch tiêm phòng y khoa của thú cưng mà không cần ghi nhớ thủ công. Đồng thời, hệ thống thông báo đẩy thời gian thực (in-app realtime notifications) giúp cải thiện giao tiếp giữa nhân viên phòng khám và chủ nuôi, đảm bảo thông tin lịch hẹn duyệt/hủy được cập nhật tức thời.

---

## 2. Đối tượng sử dụng (Target Personas)

### 🧑‍💼 Khách hàng nuôi thú cưng - Chị Vy (26 tuổi)
- **Mô tả:** Vy nuôi một chú cún Poodle con. Cuộc sống văn phòng bận rộn khiến cô hay quên các dấu mốc tiêm chủng tiếp theo của cún, dẫn đến việc cún bị tiêm trễ lịch, giảm hiệu quả phòng bệnh của vắc-xin.
- **Nỗi đau (Pain points):**
  - Không nhớ nổi thời gian tiêm mũi 2, mũi 3 của vắc-xin 5 bệnh hay 7 bệnh.
  - Sợ bị lỡ lịch tiêm phòng dại định kỳ hàng năm.
  - Không biết lịch hẹn đặt trên web đã được lễ tân phòng khám phê duyệt hay chưa nếu không check email liên tục.
- **Mong muốn:** Nhận được email nhắc lịch tiêm chủng tự động trước 3-5 ngày kèm link đặt lịch nhanh, và nhận thông báo đẩy tức thời trên web khi trạng thái lịch hẹn thay đổi.

### 🧑‍💼 Lễ tân phòng khám - Anh Anh (29 tuổi)
- **Mô tả:** Anh duyệt hàng chục ca lịch hẹn khám bệnh mỗi ngày.
- **Nỗi đau (Pain points):**
  - Sau khi bấm duyệt hoặc hủy lịch hẹn của khách, anh phải gọi điện hoặc nhắn tin thủ công để thông báo cho khách, tốn nhiều thời gian hành chính.
- **Mong muốn:** Hệ thống tự động đẩy thông báo đẩy thời gian thực (in-app) báo trạng thái cho khách hàng ngay khi anh bấm duyệt trên trang quản trị.

---

## 3. User Stories & Tiêu chí Nghiệm thu (Acceptance Criteria)

### Story 1: Gửi email nhắc lịch tiêm chủng tự động (Automatic Email Reminders)
> **Là một** Hệ thống chạy ngầm tự động,  
> **Tôi muốn** quét danh sách lịch sử tiêm chủng mỗi ngày và tự động gửi email nhắc tái chủng cho khách hàng trước 3 ngày hoặc 5 ngày,  
> **Để** đảm bảo thú cưng của khách hàng được tiêm chủng đúng hẹn và đầy đủ.

#### Tiêu chí Nghiệm thu (AC):
- **AC 1.1:** Hệ thống kích hoạt một tiến trình nền chạy định kỳ (Background Job) vào lúc 08:00 sáng hàng ngày.
- **AC 1.2:** Tiến trình quét bảng bệnh án tiêm chủng (`VaccinationRecords`) để tìm các bản ghi có ngày tiêm tiếp theo dự kiến (`NextDoseDate`) cách ngày hiện tại chính xác 3 ngày hoặc 5 ngày.
- **AC 1.3:** Tự động biên soạn nội dung email định dạng HTML cá nhân hóa:
  - Tên chủ nuôi, Tên thú cưng.
  - Tên loại vắc-xin cần tiêm nhắc lại.
  - Ngày hẹn tiêm khuyến nghị.
  - Đường dẫn (link) đặt lịch nhanh đã chèn sẵn thông tin loại vắc-xin cần đặt.
- **AC 1.4:** Sử dụng giao thức gửi mail an toàn STARTTLS (cổng 587) thông qua thư viện MailKit để gửi tới hòm thư cá nhân của chủ nuôi.

### Story 2: Hộp thư thông báo in-app thời gian thực (In-app Realtime Notifications)
> **Là một** Khách hàng nuôi thú cưng,  
> **Tôi muốn** nhận được thông báo in-app thời gian thực và xem danh sách các thông báo hệ thống (như khi được duyệt lịch hẹn, có nhắc nhở tiêm chủng),  
> **Để** tôi nắm bắt thông tin kịp thời mà không cần reload trang.

#### Tiêu chí Nghiệm thu (AC):
- **AC 2.1:** Giao diện hiển thị một icon hình chiếc chuông (Bell Icon) ở thanh điều hướng trên cùng. Chiếc chuông hiển thị một huy hiệu màu đỏ (Badge) đếm số lượng thông báo chưa đọc.
- **AC 2.2:** Khi Lễ tân hoặc Admin duyệt/hủy lịch hẹn khám, hệ thống sử dụng kết nối WebSockets để đẩy thông báo hiển thị tức thời trên màn hình khách hàng mà không cần khách phải reload lại trang.
- **AC 2.3:** Khách hàng click vào icon chuông sẽ hiển thị menu dropdown danh sách thông báo. Cho phép khách hàng click vào từng dòng thông báo để đánh dấu đã đọc (`Read`), hoặc bấm nút "Đọc tất cả" để xóa badge màu đỏ.

---

## 4. Phạm vi dự án (In-Scope & Out-of-Scope)

### ✅ In-Scope (Phase 3 MVP)
- Cấu hình Quartz.NET chạy nền hàng ngày lúc 08:00 quét lịch tiêm chủng.
- Tự động gửi email nhắc lịch tiêm bằng MailKit/SMTP.
- Hệ thống thông báo in-app lưu trữ database bảng `Notifications`.
- Tích hợp SignalR Hub để đẩy thông báo thời gian thực.
- Cổng API đọc, đánh dấu đã đọc thông báo an toàn chống IDOR.

### ❌ Out-of-Scope (Bàn giao Phase sau)
- Gửi tin nhắn SMS nhắc lịch tiêm trực tiếp đến số điện thoại qua tổng đài SMS Gateway (Brandname SMS).
- Đẩy thông báo Notification di động thông qua Firebase Cloud Messaging (FCM) lên thiết bị iOS/Android.

---

## 5. Yêu cầu phi chức năng (Non-Functional Requirements - NFRs)
- **Hiệu năng:** SignalR đẩy tin nhắn thời gian thực đến client dưới 100ms từ khi backend cập nhật trạng thái database.
- **Độ tin cậy:** Quartz.NET chạy ngầm phải xử lý lỗi phục hồi (Failover) nếu máy chủ sập nguồn giữa chừng để không bỏ lỡ ngày quét.
- **Bảo mật:** Chặn đứng IDOR, khách hàng chỉ được xem thông báo của chính tài khoản của mình.
- **Dung lượng:** Hộp thư thông báo in-app chỉ lưu giữ tối đa 50 thông báo gần nhất của mỗi người dùng, tự động dọn dẹp các thông báo cũ hơn để tối ưu hiệu năng database.
