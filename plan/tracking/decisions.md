# 🏛️ Nhật Ký Quyết Định Kiến Trúc - Architectural Decision Records (ADR)

*(Người dùng sẽ điền các quyết định thiết kế ở đây)*

## ADR 01: Chuẩn hóa cấu trúc tài liệu Sprint theo dạng Đặc tả Kỹ thuật (Technical Specification)

- **Bối cảnh:** Các tài liệu Sprint ban đầu của MyPetClinic mang tính mô tả công việc chung chung, thiếu các định nghĩa class, interface và cấu trúc database/test case cụ thể, dẫn đến nguy cơ lệch pha thiết kế giữa các thành viên.
- **Quyết định:** Chuẩn hóa toàn bộ cấu trúc tài liệu của từng Sprint theo mô hình của dự án VisualizationDSA. Mỗi Sprint sẽ được chuyển đổi thành một thư mục riêng (ví dụ: `plan/sprints/sprint-1/`), chứa `feature-plan.md` (roadmap, backlog, DoD) và `technical-spec.md` (chứa chi tiết database configurations, C# interfaces, API controllers, Pinia state stores, và khung Unit Tests).
- **Hệ quả:** Tăng tính đồng bộ, giúp lập trình viên nắm rõ cấu trúc code cần viết, giảm thời gian giải quyết xung đột khi tích hợp và bảo đảm độ phủ của Unit Test ngay từ đầu.

## ADR 02: Áp dụng cơ chế chặn tấn công IDOR trực tiếp tại lớp Application Service

- **Bối cảnh:** Lỗ hổng IDOR (Insecure Direct Object References) cho phép người dùng trái phép xem/sửa/xóa thú cưng hoặc tài nguyên của người khác bằng cách thay đổi ID trên URL/Payload.
- **Quyết định:** Không chỉ phụ thuộc vào filter ở API Controller, chúng ta chuyển logic so khớp quyền sở hữu (`OwnerId == currentUserId`) xuống lớp `Application.Services` (ví dụ: `PetService`). Lập trình viên phải truyền `currentUserId` giải mã từ token vào tham số phương thức service để đối chiếu trước khi thực hiện cập nhật hoặc xóa trong DB.
- **Hệ quả:** Tách biệt nghiệp vụ tốt hơn, đồng thời cho phép viết Unit Test độc lập cho tầng Application Service mà không cần mock HTTP Context.

## ADR 03: Sử dụng Database Transaction cô lập Serializable để xử lý tranh chấp đặt lịch hẹn đồng thời

- **Bối cảnh:** Khi có nhiều yêu cầu đặt lịch khám (Appointments) cho cùng một bác sĩ vào cùng một thời điểm gửi lên song song, có nguy cơ hai khách hàng cùng đặt thành công một slot (Double-booking) do hiện tượng tranh chấp điều kiện ghi (Race Condition).
- **Quyết định:** Sử dụng Database Transaction với mức độ cô lập là `IsolationLevel.Serializable` trong phương thức đặt lịch ở `AppointmentService`. Đồng thời sử dụng khóa độc quyền trên dòng lịch trực của bác sĩ (`GetScheduleForLockAsync`).
- **Hệ quả:** Đảm bảo tính nhất quán tuyệt đối về mặt dữ liệu, loại bỏ 100% khả năng trùng ca khám. Nhược điểm nhỏ là tăng nhẹ tài nguyên xử lý khóa của DB, tuy nhiên do tần suất đặt lịch khám tại một thời điểm là không quá cao nên đây là giải pháp tối ưu và an toàn nhất.

## ADR 04: Xử lý bất đồng bộ các tác vụ gửi thông báo (Email) và tự động tính số thứ tự hàng đợi

- **Bối cảnh:** Việc kết nối với máy chủ SMTP để gửi email phản hồi (như thông báo hủy lịch) tốn nhiều thời gian (thường từ 1-3 giây), nếu chạy đồng bộ sẽ làm nghẽn luồng xử lý chính của HTTP Request làm giảm trải nghiệm người dùng. Đồng thời, hàng đợi cần tự động sinh số thứ tự liên tục theo từng ngày.
- **Quyết định:** 
  1. Triển khai phương thức gửi email bất tuần tự theo dạng "Fire-and-forget" (`_ = _emailService.SendEmailAsync(...)`) để giải phóng HTTP thread lập tức, nâng tốc độ phản hồi API.
  2. Số thứ tự hàng đợi (`QueueNumber`) được tính toán bằng cách truy vấn số lớn nhất hiện tại của bác sĩ trong ngày rồi cộng thêm 1, bọc trong Database Transaction để tránh trùng số thứ tự khám khi check-in đồng thời.
- **Hệ quả:** Cải thiện đáng kể hiệu năng phản hồi của hệ thống API tiếp nhận Lễ tân, đảm bảo hàng đợi vận hành chính xác.

## ADR 05: Sử dụng Database Transaction bảo đảm tính nguyên tử khi kê đơn thuốc và trừ kho

- **Bối cảnh:** Quy trình hoàn tất một ca khám bệnh của bác sĩ bao gồm việc tạo bệnh án (`MedicalRecord`), lưu đơn thuốc (`Prescriptions`), chi tiết đơn thuốc (`PrescriptionItems`) và thực hiện trừ số lượng tồn kho dược phẩm (`Medicines`). Nếu bất kỳ một loại thuốc nào hết hàng hoặc xảy ra lỗi ở giữa chu kỳ lưu dữ liệu, kho thuốc sẽ bị sai lệch nếu không được khôi phục.
- **Quyết định:** Sử dụng Database Transaction (giao dịch nguyên tử - Unit of Work) bao trùm toàn bộ chuỗi tác vụ của phương thức `CreateRecordWithPrescriptionAsync`. Đồng thời áp dụng khóa hàng độc quyền (`GetByIdForUpdateAsync`) trên bản ghi kho thuốc để tránh hiện tượng tranh chấp giảm kho đồng thời. Nếu phát hiện thiếu hàng của bất kỳ dòng nào, hệ thống thực hiện Rollback lập tức.
- **Hệ quả:** Ngăn chặn tuyệt đối việc thất thoát hoặc sai lệch số lượng tồn kho giữa đơn thuốc thực tế và dữ liệu vật lý trong DB, nâng cao tính chính xác của hệ thống quản lý kho dược phẩm.

## ADR 06: Thiết lập Prompt an toàn cho AI Chatbot và triển khai Background Service nhắc lịch tiêm chủng

- **Bối cảnh:** 
  1. Tích hợp AI chatbot nếu không được cấu hình chặt chẽ về mặt Prompt có nguy cơ đưa ra các chỉ dẫn y tế sai lệch, tự ý kê đơn thuốc gây nguy hiểm cho thú cưng và ảnh hưởng pháp lý tới phòng khám.
  2. Tác vụ nhắc lịch tiêm phòng cần hoạt động tự động hàng ngày mà không được gây ảnh hưởng đến hiệu năng hoặc làm gián đoạn luồng xử lý Web chính.
- **Quyết định:**
  1. Đóng gói logic tư vấn AI trong `GeminiChatService` và tích hợp System Prompt nghiêm ngặt (chỉ tư vấn kỹ năng chăm sóc và sơ cứu cơ bản, từ chối kê đơn và chẩn đoán lâm sàng chuyên sâu).
  2. Triển khai công việc định kỳ bằng `.NET BackgroundService` kế thừa `BackgroundService` của framework để thực hiện quét bảng CSDL và gửi email tự động hàng ngày lúc 08:00 sáng.
- **Hệ quả:** Đảm bảo tính pháp lý và an toàn y tế trong tư vấn AI, đồng thời tự động hóa hoàn toàn quy trình CSKH nhắc lịch tiêm chủng một cách hiệu quả và đáng tin cậy.

## ADR 07: Chuyển đổi luồng đặt lịch sang tự động phân bổ bác sĩ và gộp slot giờ khám chung của phòng khám

- **Bối cảnh:** Việc bắt buộc khách hàng phải chọn đích danh bác sĩ khi đặt lịch online gây bất tiện nếu bác sĩ đó chưa có lịch trực hoặc bị quá tải, trong khi các bác sĩ khác vẫn rảnh.
- **Quyết định:** 
  1. Gộp tất cả các slot thời gian trống của tất cả các bác sĩ trực trong ngày thành một danh sách khung giờ chung duy nhất trên Frontend.
  2. Cho phép khách hàng chọn giờ trực tiếp mà không cần chọn bác sĩ (Tự động phân công).
  3. Ở Backend, cải tiến logic `CreateAppointmentAsync` để tự động lọc các bác sĩ rảnh vào khung giờ đó và chọn ra bác sĩ có ít ca khám nhất trong ngày (Cân bằng tải).
- **Hệ quả:** Tối giản hóa quy trình đặt lịch của khách hàng, tối ưu hóa công suất làm việc của đội ngũ bác sĩ thú y, và tránh lỗi trùng lịch/quá tải cục bộ.
