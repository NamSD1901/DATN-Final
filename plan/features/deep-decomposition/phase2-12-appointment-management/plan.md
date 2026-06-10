# 📝 Implementation Plan & Testing Strategy - Customer Appointment Management

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) phục vụ bộ phận QA kiểm định chất lượng, ngăn chặn lỗi trạng thái lịch hẹn, phòng chống tấn công IDOR chéo và kiểm tra sự ổn định của hệ thống gửi mail thông báo.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự qua 3 giai đoạn nhỏ nhằm bảo đảm chất lượng cao nhất của mã nguồn:

### Giai đoạn 1: Database Setup & Migration
* **Bước 1.1:** Thêm các trường `CancelledReason` (nullable string) và `CancelledAt` (nullable DateTime) vào thực thể `Appointment` trong dự án Domain.
* **Bước 1.2:** Cấu hình EF Core Fluent API và cập nhật Check Constraint cho trường `Status` để chấp nhận giá trị `'cancelled'`.
* **Bước 1.3:** Thiết lập composite index `IX_Appointments_Customer_Status_Date` chống quét chậm cơ sở dữ liệu khi phân trang.
* **Bước 1.4:** Tạo EF Core Migration và cập nhật cơ sở dữ liệu PostgreSQL.

### Giai đoạn 2: Lõi xử lý Nghiệp vụ & Bảo mật (Backend)
* **Bước 2.1:** Cài đặt logic Eager Loading và phân trang tại `AppointmentService.GetCustomerAppointmentsAsync` chống N+1 query.
* **Bước 2.2:** Cài đặt logic kiểm soát quyền IDOR đối chiếu thú cưng và khách hàng tại `AppointmentService.CancelAppointmentAsync`.
* **Bước 2.3:** Thiết lập kiểm soát ràng buộc trạng thái: Chỉ cho phép hủy khi trạng thái là `pending` hoặc `confirmed`. Đồng thời thực thi ràng buộc thời gian (Hủy trước giờ hẹn tối thiểu 2 tiếng đối với lịch đã confirmed).
* **Bước 2.4:** Viết các Unit Test kiểm tra logic hủy lịch, kiểm tra validation lý do hủy, và bảo mật IDOR.

### Giai đoạn 3: Giao diện & Wizard Integration (Frontend)
* **Bước 3.1:** Thiết lập Pinia store `customerAppointments` quản lý state phân trang, tabs lọc, active item, loading, và caching.
* **Bước 3.2:** Dựng giao diện Dashboard 2 cột: Danh sách Grid thẻ Glassmorphic bên trái và Panel chi tiết bên phải (hỗ trợ Bottom Sheet trượt trên Mobile).
* **Bước 3.3:** Tích hợp bộ tạo mã QR Code động hiển thị token check-in y tế của thú cưng.
* **Bước 3.4:** Xây dựng Dialog hủy lịch hẹn bắt buộc nhập lý do hủy, kiểm tra validate client-side và cờ submit locking.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ (Functional Testing Cases)

#### TC-FUN-01: Tải danh sách lịch hẹn cá nhân có phân trang và lọc trạng thái
* **Mục tiêu:** Xác minh hệ thống lọc chính xác trạng thái và phân trang lịch hẹn.
* **Dữ liệu đầu vào:**
  * Tài khoản Khách hàng A.
  * status = `confirmed`, page = 1, pageSize = 10.
* **Các bước thực hiện:**
  1. Đăng nhập Customer A, truy cập tab "Đã duyệt".
* **Kết quả mong đợi:**
  * API trả về `200 OK`.
  * Trả về danh sách chỉ chứa các lịch hẹn có trạng thái `confirmed` của Customer A.
  * Giao diện hiển thị đúng định dạng ngày giờ địa phương.

#### TC-FUN-02: Hủy lịch hẹn ở trạng thái Pending hoặc Confirmed hợp lệ
* **Mục tiêu:** Xác minh khách hàng hủy thành công lịch hẹn khi đủ điều kiện thời gian.
* **Kịch bản giả lập:** Lịch hẹn A ở trạng thái `confirmed` lúc `15:00` ngày mai. Thời điểm hiện tại là `08:00` sáng nay (cách 31 tiếng > 2 tiếng).
* **Các bước thực hiện:**
  1. Nhấp nút "Hủy lịch hẹn".
  2. Nhập lý do: *"Bận lịch đột xuất không đi được."* (28 ký tự > 10).
  3. Bấm xác nhận.
* **Kết quả mong đợi:**
  * API trả về `200 OK` (success: true).
  * Trạng thái lịch trên UI đổi sang `cancelled` lập tức mà không cần F5 tải lại trang.
  * DB cập nhật cột `Status = 'cancelled'`, `CancelledReason` ghi nhận lý do, `CancelledAt` cập nhật thời gian UTC.

#### TC-FUN-03: Chặn hủy lịch hẹn Confirmed sát giờ khám (Dưới 2 tiếng)
* **Kịch bản giả lập:** Lịch hẹn A được xác nhận khám vào ngày mai lúc `09:00`. Người dùng gửi yêu cầu hủy lúc `08:15` ngày mai (cách 45 phút).
* **Các bước thực hiện:** Gửi lệnh hủy lịch hẹn.
* **Kết quả mong đợi:**
  * API trả về lỗi `400 Bad Request`.
  * Nội dung báo lỗi: *"Lịch hẹn đã được xác nhận chỉ có thể hủy trực tuyến trước giờ hẹn tối thiểu 2 tiếng."*.
  * Lịch hẹn dưới DB vẫn giữ nguyên trạng thái `confirmed`.

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Chống IDOR hủy lịch hẹn của khách hàng khác
* **Mục tiêu:** Đảm bảo khách hàng không thể hủy lịch hẹn của người khác qua ID.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Customer A.
  2. Gửi request `PUT /api/my-appointments/e2c38d4f-3721-4f18-a664-d3a373ff2010/cancel` (Trong đó lịch hẹn ID này thuộc về Customer B).
* **Kết quả mong đợi:**
  * API trả về lỗi `403 Forbidden` hoặc `404 Not Found`.
  * Lịch hẹn của Customer B không bị thay đổi dưới DB.

---

### C. Mã nguồn Unit Test C# tham khảo (xUnit & FluentAssertions)

```csharp
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;

public class AppointmentCancellationTests
{
    [Fact]
    public async Task CancelConfirmedAppointment_TooCloseToTime_ShouldThrowException()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Status = "confirmed",
            AppointmentDate = DateTime.UtcNow.AddMinutes(90), // Cách hiện tại 90 phút (< 120 phút)
            Pet = new Pet { OwnerId = Guid.NewGuid() }
        };
        var service = new AppointmentService(mockContext.Object, mockEmail.Object);

        // Act
        Func<Task> act = async () => await service.CancelAppointmentAsync(
            appointment.Id, 
            appointment.Pet.OwnerId, 
            "Bận đột xuất lý do cá nhân"
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tối thiểu 2 tiếng*");
    }
}
```
