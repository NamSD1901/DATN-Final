# 📝 Implementation Plan & Testing Strategy - Receptionist Portal & Queue Management

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) phục vụ bộ phận QA kiểm định chất lượng, ngăn chặn lỗi tranh chấp số thứ tự khám, phòng chống tấn công phân quyền sai lệch và kiểm tra sự ổn định của kết nối SignalR thời gian thực.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự qua 4 giai đoạn khép kín để bảo đảm độ tin cậy vận hành:

### Giai đoạn 1: Database Setup & Migration
* **Bước 1.1:** Thêm các trường `QueueNumber` (string, nullable), `CheckInTime` (DateTime, nullable), và `ClinicRoom` (string, nullable) vào thực thể `Appointment`.
* **Bước 1.2:** Khởi tạo PostgreSQL Sequence `queue_number_seq` và viết cron job tự động reset về 1 lúc 00:00 hàng ngày.
* **Bước 1.3:** Thiết lập composite index `IX_Appointments_Queue_Today` hỗ trợ tải nhanh Kanban và TV board.
* **Bước 1.4:** Tạo EF Core Migration và chạy update database.

### Giai đoạn 2: Lõi xử lý Nghiệp vụ & SignalR Gateway (Backend)
* **Bước 2.1:** Cài đặt SignalR `QueueHub` và cấu hình CORS Credentials cho phép các subdomain phòng khám kết nối.
* **Bước 2.2:** Xây dựng `QueueService` thực hiện check-in Thread-safe (sử dụng `SemaphoreSlim` và khóa bi quan) ngăn chặn trùng lặp số thứ tự.
* **Bước 2.3:** Cài đặt thuật toán tự động đề xuất phòng khám trống `SuggestOptimalRoomAsync`.
* **Bước 2.4:** Viết các Unit Test kiểm tra bộ sinh số thứ tự và kiểm tra tranh chấp đa luồng.

### Giai đoạn 3: Giao diện Kanban & WebSockets Client (Frontend)
* **Bước 3.1:** Xây dựng Pinia store `receptionistQueueStore` tích hợp SignalR Client tự động kết nối và lắng nghe sự kiện `QueueUpdated`.
* **Bước 3.2:** Dựng giao diện Kanban Board 3 cột sử dụng CSS Glassmorphism mờ kính: Chờ khám, Đang khám, Đã khám xong. Tích hợp thư viện kéo thả mượt mà.
* **Bước 3.3:** Viết Quick Form Modal đăng ký khách vãng lai và autofocus input check-in nhanh bằng đầu đọc barcode.
* **Bước 3.4:** Dựng giao diện Tivi sảnh chờ công cộng cỡ chữ lớn, tối giản.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Hàng đợi (Functional & Queue Testing)

#### TC-FUN-01: Check-in lịch hẹn hợp lệ & Cấp số thứ tự Q-XXX
* **Mục tiêu:** Xác minh hệ thống check-in và sinh số thứ tự tăng dần.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Lễ tân.
  2. Mở dashboard tiếp đón, nhập số điện thoại khách hàng có lịch hẹn đã xác nhận.
  3. Nhấp "Check-in", chọn "Phòng khám 101" và bấm xác nhận.
* **Kết quả mong đợi:**
  * API trả về `200 OK`.
  * Trạng thái lịch hẹn đổi sang `waiting`.
  * Sinh số thứ tự tiếp theo dạng `Q-012`.
  * SignalR Hub phát sóng sự kiện làm mới Kanban Board trên máy tính của Lễ tân và Tivi sảnh chờ.

#### TC-FUN-02: Đăng ký khách vãng lai (Quick Walk-in) xếp hàng trực tiếp
* **Mục tiêu:** Đảm bảo khách vãng lai được thêm trực tiếp vào hàng chờ.
* **Các bước thực hiện:**
  1. Click "Đăng ký Walk-in".
  2. Điền thông tin: Chủ Nam (0987654321), Bé Cún (Chó Poodle), chỉ định Khám sức khỏe tổng quát, chọn Phòng khám 102.
  3. Nhấn "Đăng ký & Xếp hàng".
* **Kết quả mong đợi:**
  * DB tạo mới bản ghi khách hàng, thú cưng và lịch hẹn ở trạng thái `waiting`.
  * Cấp số thứ tự tiếp theo (Ví dụ: `Q-013`). Thẻ xuất hiện trên Kanban cột Waiting.

---

### B. Kiểm thử Tranh chấp & Bảo mật (Concurrency & Security Testing)

#### TC-CON-01: Kiểm thử tranh chấp sinh số thứ tự khám (Concurrency Test)
* **Mục tiêu:** Đảm bảo không bao giờ cấp trùng một số thứ tự khám khi 2 quầy cùng check-in đồng thời tại một mili-giây.
* **Các bước thực hiện:** Giả lập 2 HTTP Request gửi đến API check-in cùng lúc thông qua công cụ kiểm thử tải (Apache JMeter hoặc viết script C# Parallel Task).
* **Kết quả mong đợi:**
  * Cả hai request đều thành công.
  * Hệ thống xử lý tuần tự: request A nhận số thứ tự `Q-005`, request B nhận số thứ tự `Q-006`. Tuyệt đối không có chuyện cả hai cùng nhận `Q-005`.

#### TC-SEC-01: Chặn đứng tài khoản Khách hàng truy cập API tiếp đón
* **Mục tiêu:** Đảm bảo khách hàng thường không thể tự check-in hoặc đổi phòng khám.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Khách hàng (Role = `customer`).
  2. Gửi request `POST /api/receptionist/check-in` qua Postman.
* **Kết quả mong đợi:** API chặn đứng yêu cầu và trả về lỗi `403 Forbidden` do vi phạm phân quyền RBAC.

---

### C. Mã nguồn Integration Test C# tham khảo (Concurrency Test)

```csharp
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class QueueConcurrencyTests
{
    [Fact]
    public async Task ParallelCheckIn_ShouldGenerateUniqueSequenceNumbers()
    {
        // Arrange
        var service = new QueueService(dbContext, mockHub.Object);
        var tasks = new List<Task<QueueStatusDto>>();
        
        // Act - Kích hoạt 5 request check-in đồng thời bằng đa luồng
        for (int i = 0; i < 5; i++)
        {
            var appointmentId = Guid.NewGuid(); // Giả lập các ID lịch hẹn khác nhau
            tasks.Add(Task.Run(() => service.CheckInAppointmentAsync(appointmentId, "Phòng khám 101")));
        }
        
        var results = await Task.WhenAll(tasks);
        
        // Assert - Kiểm tra không có số thứ tự nào bị trùng lặp
        var queueNumbers = new HashSet<string>();
        foreach (var result in results)
        {
            queueNumbers.Add(result.QueueNumber).Should().BeTrue(); // HashSet.Add trả về false nếu đã tồn tại
        }
        queueNumbers.Count.Should().Be(5);
    }
}
```
