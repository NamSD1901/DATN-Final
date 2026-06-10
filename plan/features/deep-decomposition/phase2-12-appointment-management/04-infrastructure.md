# 🌐 Infrastructure & Security - Customer Appointment Management

## 1. Phân quyền API và Bảo mật vai trò (RBAC & Endpoint Protection)

Tính năng quản lý lịch hẹn (Dashboard) là khu vực lưu trữ thông tin bệnh lịch nhạy cảm của thú cưng và thông tin cá nhân của chủ nuôi:
*   **Ràng buộc Endpoint:** Endpoint `/api/my-appointments` được cấu hình phân quyền nghiêm ngặt thông qua annotation `[Authorize(Roles = "customer")]`.
*   **Xác thực JWT Token:** Trích xuất định danh khách hàng hoàn toàn từ token claims của phiên đăng nhập (ClaimsPrincipal). Không sử dụng các ID truyền thủ công trên Query string hoặc Request Body để tránh tấn công chiếm đoạt hoặc giả danh phiên.

---

## 2. Phòng chống Tấn công IDOR chéo lịch hẹn (Appointment Owner Checking)

IDOR (Insecure Direct Object Reference) trong nghiệp vụ quản lý lịch hẹn rất dễ xảy ra nếu nhà phát triển chỉ kiểm tra sự tồn tại của ID lịch hẹn mà bỏ qua quyền sở hữu. 

*   **Nguy cơ:** Một khách hàng A có thể gửi yêu cầu hủy lịch hẹn với ID `appointmentId = 150` (Thực chất ID này thuộc về khách hàng B) bằng cách dò quét số thứ tự ID tăng dần hoặc đoán GUID.
*   **Cơ chế phòng ngự cứng ở Service Layer:**
    Trước khi thay đổi trạng thái hoặc truy vấn chi tiết lịch hẹn, Backend bắt buộc thực hiện kiểm tra liên kết chéo qua bảng thú cưng `Pets`:
    ```csharp
    var appointment = await _context.Appointments
        .Include(a => a.Pet)
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    // Chặn IDOR: Đảm bảo thú cưng liên kết với lịch hẹn thuộc quyền sở hữu của khách hàng yêu cầu
    if (appointment != null && appointment.Pet.OwnerId != currentUserId)
    {
        throw new UnauthorizedAccessException("Bạn không có quyền truy cập hoặc thao tác trên lịch hẹn này.");
    }
    ```
    Nếu vi phạm, hệ thống ghi nhận log cảnh báo bảo mật dạng `Security Warning: Unauthorized access attempt to Appointment ID {appointmentId} by User {userId}` và trả về mã lỗi `403 Forbidden` (hoặc `404 Not Found` để ẩn sự tồn tại của tài nguyên).

---

## 3. Thiết kế Index Cơ sở dữ liệu và Tối ưu hóa Truy vấn Paging

Khi phòng khám vận hành lâu năm, số lượng lịch hẹn tích lũy của khách hàng tăng cao. Để bảo đảm tải trang Dashboard mượt mà, ta áp dụng phân trang (Pagination) kết hợp cấu hình chỉ mục (SQL Indexes):

*   **Composite Index cho truy vấn danh sách lịch hẹn:**
    Hỗ trợ truy vấn tải nhanh lịch sử khám lọc theo trạng thái và sắp xếp thời gian mới nhất.
    ```sql
    CREATE INDEX IX_Appointments_Customer_Status_Date 
    ON "Appointments" ("PetId", "Status", "AppointmentDate" DESC);
    ```
    *Giải thích:* Vì chúng ta lọc lịch hẹn theo khách hàng (thông qua `Pet.OwnerId`), composite index trên `PetId` kết hợp `Status` và sắp xếp giảm dần theo `AppointmentDate` giúp CSDL PostgreSQL tìm kiếm nhanh các bản ghi phân trang bằng phương pháp Index Scan với chi phí CPU gần như bằng không.

---

## 4. Tự động hóa Gửi thông báo Nền (Background Worker & Mail Queue)

Hoạt động gửi email thông báo hủy lịch hẹn có thể gặp trễ mạng (Network Latency) nếu tích hợp đồng bộ trong HTTP Request Thread (khiến thời gian phản hồi của API hủy lịch bị kéo dài lên 2 - 3 giây):

*   **Giải pháp:** Áp dụng mô hình Publish-Subscribe bất đồng bộ bằng cách đưa công việc gửi email vào hàng đợi xử lý nền (Background Job) thông qua thư viện Hangfire hoặc .NET HostedService.
*   **Workflow:**
    1. Người dùng gửi yêu cầu hủy lịch.
    2. Backend cập nhật DB thành công và gọi `BackgroundJob.Enqueue<IEmailJob>(job => job.SendCancellationMail(appointmentId))`.
    3. Trả về phản hồi `200 OK` cho Client ngay lập tức (Thời gian xử lý API < 100ms).
    4. Hangfire Worker ở tiến trình nền tiếp nhận Job và kết nối SMTP Server để gửi email xác nhận cho khách hàng trong luồng chạy riêng biệt.

---

## 5. Giới hạn tần suất gọi API Hủy lịch (Rate Limiting)

Hành động hủy lịch hẹn kích hoạt tiến trình giải phóng kho dược và bác sĩ, có khả năng bị lợi dụng để spam.
*   **Cấu hình Rate Limit:**
    *   Endpoint xem danh sách `/my-appointments`: Tối đa **30 requests / 1 phút**.
    *   Endpoint hủy lịch `/my-appointments/{id}/cancel`: Tối đa **3 requests / 1 phút / 1 tài khoản**.
*   **Mã cấu hình .NET 8 API Limiter:**
    ```csharp
    options.AddFixedWindowLimiter("CancelAppointmentPolicy", limitOptions =>
    {
        limitOptions.PermitLimit = 3;
        limitOptions.Window = TimeSpan.FromMinutes(1);
        limitOptions.QueueLimit = 0;
    });
    ```
