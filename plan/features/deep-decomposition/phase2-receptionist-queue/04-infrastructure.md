# 🌐 Infrastructure & Security - Receptionist Portal & Queue Management

## 1. Phân quyền Endpoint bảo mật (RBAC Settings & Cross-Role Security)

Nghiệp vụ tiếp tiếp đón và điều phối hàng đợi yêu cầu bảo mật cao để tránh việc can thiệp trái phép vào thứ tự ưu tiên khám bệnh hoặc xem thông tin bệnh án y tế:
*   **Ràng buộc Endpoint:** API Controller `ReceptionistQueueController` được cấu hình cứng phân quyền `[Authorize(Roles = "receptionist,admin")]`.
*   **Phòng chống giả danh Role:** Hệ thống giải mã Token Claims trực tiếp từ JWT. Bác sĩ (`doctor`) hoặc Khách hàng (`customer`) cố tình gửi request POST check-in hoặc thay đổi số thứ tự khám sẽ bị chặn và trả về `403 Forbidden` ngay tại cổng Middleware.
*   **SignalR Connection Authorization:** Chỉ chấp nhận các kết nối SignalR Client đã xác thực JWT Token thành công. Khách hàng thông thường truy cập Public Queue Board trên sảnh Tivi sẽ được cấu hình kết nối ở chế độ **Read-only** (chỉ lắng nghe sự kiện, không được gửi thông điệp thay đổi hàng đợi).

---

## 2. Thiết lập SignalR CORS Policy & WebSockets Gateway

Do màn hình Tivi sảnh chờ và máy tính lễ tân có thể chạy trên các cổng (Ports) hoặc subdomain khác nhau, ta cần cấu hình CORS chính xác để bảo vệ kết nối WebSockets:

*   **Cấu hình ASP.NET Core Startup (.NET 8):**
    ```csharp
    app.UseCors(policy => policy
        .WithOrigins("https://clinic.mypetclinic.vn", "https://tivi.mypetclinic.vn")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()); // Bắt buộc cho SignalR WebSockets
    ```
*   **Cấu hình SignalR Hub Endpoint Mapping:**
    ```csharp
    app.MapHub<QueueHub>("/hubs/queue", options =>
    {
        options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
    });
    ```

---

## 3. Thiết kế Index Cơ sở dữ liệu và Tối ưu hóa Truy vấn Kanban

Truy vấn hàng đợi khám trong ngày được gọi liên tục mỗi khi SignalR phát sự kiện thay đổi. Để tránh tải nặng PostgreSQL Database:

*   **Composite Index cho Hàng đợi Ngày:**
    CSDL cần index nhanh các ca khám diễn ra trong ngày hôm nay ở 3 trạng thái hoạt động (`waiting`, `in_progress`, `completed`):
    ```sql
    CREATE INDEX IX_Appointments_Queue_Today 
    ON "Appointments" ("CheckInTime" DESC, "Status") 
    WHERE "Status" IN ('waiting', 'in_progress', 'completed') AND "QueueNumber" IS NOT NULL;
    ```
    *Ý nghĩa:* Index này gom cụm toàn bộ các ca check-in trong ngày sắp xếp theo thời gian mới nhất. Khi API `/receptionist/queue` được gọi, PostgreSQL thực hiện Index Scan cực nhanh trong vòng dưới 1ms để lấy dữ liệu đổ vào Kanban Board.

---

## 4. Quản lý Concurrency và Tránh trùng lặp STT (Concurrency Control)

Khi phòng khám đông khách, hai lễ tân tại quầy số 1 và quầy số 2 có thể click "Check-in" cho 2 thú cưng khác nhau ở cùng một giây. Nếu không kiểm soát tranh chấp (Race Condition), hệ thống có thể cấp trùng một Số thứ tự (Ví dụ: hai bé đều nhận số `Q-015`):

*   **Giải pháp Khóa Bi quan (Pessimistic Lock):**
    Sử dụng câu lệnh `FOR UPDATE` trong PostgreSQL Transaction để khóa bản ghi lịch hẹn và bảng đếm Sequence cho đến khi hoàn tất ghi nhận:
    ```sql
    -- Khóa bản ghi sinh số thứ tự trong transaction
    SELECT nextval('queue_number_seq') FOR UPDATE;
    ```
*   **SemaphoreSlim tại Application Layer:**
    Như đã cấu hình trong `QueueService`, việc bọc đoạn sinh mã `GenerateNextQueueNumberAsync` trong một `SemaphoreSlim(1, 1)` đảm bảo tại một thời điểm chỉ có duy nhất một luồng (Thread) được phép tính toán số thứ tự tiếp theo, loại bỏ hoàn toàn khả năng trùng lặp STT trên môi trường đa luồng.

---

## 5. Nhật ký hoạt động & Giám sát (Audit Logging)

Để phục vụ quản trị và kiểm tra nếu xảy ra tranh chấp hoặc khiếu nại của khách hàng về thứ tự gọi khám:
*   Mọi thao tác thay đổi trạng thái hàng đợi (Lễ tân check-in, Bác sĩ gọi khám, Lễ tân chuyển ca khám) đều được ghi nhận vào bảng `AuditLogs` trong Database.
*   **Thông tin ghi nhận:** `[Thời gian] | [Tài khoản thực hiện] | [Hành động] | [ID Lịch hẹn] | [Trạng thái cũ] -> [Trạng thái mới] | [Phòng khám chỉ định]`.
    *   Ví dụ: `2026-06-10 17:30:15 | Lễ tân Mai | CHECKIN | LH-00472 | confirmed -> waiting | Phòng khám 101`.
