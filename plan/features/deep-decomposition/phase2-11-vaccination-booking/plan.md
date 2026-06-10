# 📝 Implementation Plan & Testing Strategy - Online Vaccination Booking

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap), bộ kịch bản kiểm thử (Test Cases) phục vụ bộ phận QA kiểm định chất lượng, ngăn chặn lỗi tiêm sai phác đồ y khoa, phòng chống tấn công IDOR chéo và kiểm tra sự đồng bộ tồn kho vắc-xin.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được phân chia thành 4 giai đoạn khép kín để bảo đảm tính ổn định y tế và độ bảo mật cao:

### Giai đoạn 1: Database Setup & Migration
* **Bước 1.1:** Mở rộng thực thể `Medicine` để thêm các trường y sinh học: `TargetSpecies` (Dog/Cat/All), `MinAgeWeeks`, `IntervalDays`. Cấu hình EF Core Fluent API.
* **Bước 1.2:** Tạo thực thể `VaccinationRecord` liên kết khóa ngoại với `Pets`, `Medicines` và `Appointments` (nếu có). Cấu hình quan hệ xóa mềm (Soft Delete).
* **Bước 1.3:** Tạo composite index `IX_VaccinationRecords_PetId_VaccineId_Date` để tăng tốc độ truy quét lịch sử và index lọc tồn kho `IX_Medicines_Vaccine_StockQuantity`.
* **Bước 1.4:** Viết EF Core Migration và cập nhật database PostgreSQL. Chạy SQL script nạp dữ liệu vắc-xin mẫu (Seed Data).

### Giai đoạn 2: Lõi xử lý Nghiệp vụ & Bảo mật (Backend Implementation)
* **Bước 2.1:** Xây dựng dịch vụ `VaccinationScheduleChecker` (hoặc method trong `VaccinationService`) để kiểm tra phác đồ:
  * So khớp loài thú cưng (chó/mèo) với loài đích của vắc-xin.
  * Kiểm tra độ tuổi tối thiểu tiêm chủng của bé (`MinAgeWeeks`).
  * So sánh khoảng cách thời gian từ ngày tiêm gần nhất của vắc-xin này đến ngày hẹn tiêm mới so với khoảng cách an toàn quy định (`IntervalDays`).
* **Bước 2.2:** Thiết lập logic kiểm kho dược y tế thời gian thực (Real-time Inventory Check) trong database transaction với lock bi quan (`SELECT FOR UPDATE`).
* **Bước 2.3:** Tạo endpoint API `POST /api/vaccination/validate-interval` và `POST /api/vaccination/book` trong `CustomerVaccinationController` kèm middleware kiểm tra IDOR sở hữu thú cưng (`pet.OwnerId == currentUserId`).
* **Bước 2.4:** Viết các Unit Test bằng xUnit bao phủ logic kiểm tra phác đồ tiêm của `VaccinationScheduleChecker` và Integration Test kiểm thử liên thông API tiêm chủng.

### Giai đoạn 3: Giao diện Front-End & Vue Components
* **Bước 3.1:** Xây dựng Pinia store `vaccinationBookingStore` để lưu trữ dữ liệu biểu mẫu đa bước, cache danh mục vắc-xin, lịch sử tiêm chủng của bé và lưu trữ cảnh báo y tế.
* **Bước 3.2:** Dựng giao diện Modal Wizard 4 bước đặt lịch tiêm phòng bằng CSS Glassmorphism mờ kính sang trọng:
  * Bước 1: Grid chọn bé cưng.
  * Bước 2: Danh sách vắc-xin lọc theo loài của bé + Lịch sử tiêm phòng nhanh ở cột bên cạnh.
  * Bước 3: Lịch chọn ngày và khung giờ rảnh của bác sĩ trực ca.
  * Bước 4: Tóm tắt thông tin đặt lịch tiêm + Trình bày Hộp cảnh báo phác đồ y tế (nếu vi phạm khoảng cách an toàn) có checkbox yêu cầu chủ nuôi xác nhận đồng ý tiếp tục.
* **Bước 3.3:** Viết client-side validation đảm bảo tính toàn vẹn thông tin và cờ khóa tương tác chống double-click khi gửi đơn.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Quy tắc Y tế (Functional & Medical Policy Testing)

#### TC-FUN-01: Đặt lịch tiêm phòng thành công cho loại vắc-xin phù hợp (Happy Path)
* **Mục tiêu:** Xác minh luồng đặt lịch tiêm phòng diễn ra bình thường khi thỏa mãn tất cả điều kiện tồn kho và phác đồ.
* **Dữ liệu đầu vào:**
  * Bé Miu (mèo, 16 tuần tuổi).
  * Vắc-xin được chọn: *Vắc-xin 4 Bệnh Mèo (Nobivac Tricat)* (Còn 30 liều trong kho).
  * Miu chưa từng tiêm vắc-xin này trong lịch sử.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Customer sở hữu Bé Miu, thực hiện đặt lịch qua Modal.
  2. Bấm xác nhận tạo đơn đặt lịch tiêm phòng.
* **Kết quả mong đợi:**
  * Hệ thống kiểm tra phác đồ hợp lệ (Miu đủ 16 tuần > 8 tuần tối thiểu, chưa có lịch sử tiêm nên khoảng cách an toàn thỏa mãn).
  * API trả về `200 OK` (success: true).
  * Database tạo bản ghi lịch hẹn mới ở trạng thái `pending`.
  * Tồn kho vắc-xin trên DB **chưa bị trừ** (vẫn giữ nguyên 30 liều, chỉ trừ khi thực hiện tiêm thực tế ở phòng khám).

#### TC-FUN-02: Chặn đặt lịch vắc-xin không đúng loài của vật nuôi
* **Mục tiêu:** Đảm bảo không thể chọn vắc-xin của chó cho mèo.
* **Các bước thực hiện:**
  1. Chọn bé cưng là mèo Miu ở bước 1.
  2. Tại bước 2, xem danh sách vắc-xin.
* **Kết quả mong đợi:**
  * Danh sách vắc-xin không hiển thị các loại vắc-xin dành riêng cho chó (như *Nobivac DHPPi*). Chỉ hiển thị vắc-xin cho mèo hoặc vắc-xin dùng chung (Dại).

#### TC-FUN-03: Cảnh báo phác đồ tiêm nhắc lại quá sớm (Interval Warning)
* **Mục tiêu:** Xác minh hệ thống đưa ra cảnh báo y tế khi khoảng cách tiêm nhỏ hơn khoảng cách an toàn quy định.
* **Kịch bản giả lập:** Bé Miu đã tiêm mũi 1 vắc-xin 4 bệnh mèo vào ngày `05/06/2026` (lưu trong `VaccinationRecords`).
* **Các bước thực hiện:**
  1. Chọn bé Miu, chọn vắc-xin 4 bệnh mèo, chọn ngày hẹn tiêm mới là ngày `15/06/2026` (Chỉ cách mũi gần nhất 10 ngày, trong khi khoảng cách tối thiểu khuyến nghị là 21 ngày).
  2. Chuyển sang bước xác nhận.
* **Kết quả mong đợi:**
  * API `/validate-interval` trả về kết quả `isValid: false` kèm cảnh báo chi tiết.
  * Màn hình hiển thị hộp cảnh báo màu vàng cảnh báo tiêm quá sớm, khuyến nghị tiêm sau ngày `26/06/2026`.
  * Nút "Xác nhận đặt lịch" bị vô hiệu hóa cho đến khi người dùng tích chọn "Tôi đồng ý tiếp tục".

---

### B. Kiểm thử Bảo mật & Biên (Security & Edge Cases)

#### TC-SEC-01: Chống IDOR xem trộm lịch sử tiêm chủng của thú cưng khác
* **Mục tiêu:** Đảm bảo người dùng không thể xem lịch sử tiêm phòng của bé cún không thuộc sở hữu của mình.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Customer A.
  2. Gửi request `GET /api/vaccination/pet-history/99` (Trong đó pet ID 99 thuộc sở hữu của Customer B).
* **Kết quả mong đợi:**
  * API chặn request và trả về mã lỗi `403 Forbidden` hoặc `400 Bad Request`.
  * Nội dung phản hồi: *"Thú cưng không hợp lệ hoặc không thuộc quyền sở hữu của bạn."*.

#### TC-SEC-02: Kiểm thử Race Condition khi vắc-xin hết hàng ở giây cuối
* **Mục tiêu:** Tránh việc tạo lịch tiêm khi vắc-xin vừa hết hàng thực tế tại clinic.
* **Kịch bản giả lập:** Vắc-xin dại chỉ còn 0 liều trong kho.
* **Các bước thực hiện:**
  1. Gửi request `POST /api/vaccination/book` chọn loại vắc-xin dại đó.
* **Kết quả mong đợi:**
  * API trả về `400 Bad Request`.
  * Trả về thông báo lỗi: *"Đặt lịch thất bại: Loại vắc-xin được chọn hiện đã hết hàng tại kho dược."*.

---

### C. Mã nguồn Unit Test C# tham khảo (xUnit & FluentAssertions)
Dưới đây là một phần mã nguồn Unit Test kiểm tra logic tính toán phác đồ tiêm chủng:

```csharp
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;

public class VaccinationScheduleCheckerTests
{
    [Fact]
    public void CheckSchedule_TooSoon_ShouldReturnInvalidWithWarning()
    {
        // Arrange
        var vaccine = new Medicine { Id = 16, Name = "Tricat", IntervalDays = 21, TargetSpecies = "Cat" };
        var history = new List<VaccinationRecord>
        {
            new() { VaccineId = 16, VaccinatedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)) } // Mũi 1 tiêm cách đây 10 ngày
        };
        var checker = new VaccinationScheduleChecker();

        // Act
        var result = checker.ValidateInterval(history, vaccine, DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        result.IsValid.Should().BeFalse();
        result.RequiresDoctorOverride.Should().BeTrue();
        result.WarningMessage.Should().Contain("vi phạm phác đồ");
    }
}
```
