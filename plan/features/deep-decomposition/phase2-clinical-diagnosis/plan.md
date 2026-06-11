# 📝 Implementation Plan & Testing Strategy - Clinical Diagnosis & Treatment

Tài liệu này vạch ra chi tiết lộ trình phát triển (Micro-roadmap) và bộ kịch bản kiểm thử (Test Cases) phục vụ bộ phận QA kiểm định chất lượng, ngăn chặn lỗi thất thoát kho dược phẩm, phòng chống tấn công chéo bệnh lịch nhạy cảm và kiểm tra sự ổn định của các DbContext transactions.

---

## 1. Lộ trình phát triển Chi tiết (Micro-Roadmap)

Quy trình phát triển được thực thi tuần tự qua 4 giai đoạn khép kín để bảo đảm an toàn dữ liệu y tế:

### Giai đoạn 1: Database Setup & Configuration
* **Bước 1.1:** Tạo các bảng cơ sở dữ liệu `MedicalRecords`, `Prescriptions` và `PrescriptionItems` trong PostgreSQL với các ràng buộc khóa ngoại và check constraint số lượng thuốc > 0.
* **Bước 1.2:** Thiết lập chỉ mục composite `IX_MedicalRecords_PetId_CreatedAt` giúp bác sĩ tải nhanh bệnh sử cũ.
* **Bước 1.3:** Đăng ký các DbContext Entity Configuration bằng Fluent API và chạy Migration cập nhật DB.

### Giai đoạn 2: Lõi xử lý Nghiệp vụ & Bảo mật (Backend)
* **Bước 2.1:** Xây dựng `PrescriptionSafetyChecker` thực hiện quét trùng hoạt chất (Active Ingredient) của các thuốc kê đơn.
* **Bước 2.2:** Cài đặt logic giao dịch ACID tại `MedicalRecordService.CreateMedicalRecordAsync` thực thi khóa dòng bi quan PostgreSQL (`FOR UPDATE`) sắp xếp tăng dần theo `MedicineId` để chống Deadlock.
* **Bước 2.3:** Tạo các API endpoints tiếp nhận, bệnh sử và autocomplete trong `DoctorClinicalController` bảo mật vai trò `[Authorize(Roles = "doctor,admin")]` và chặn IDOR.
* **Bước 2.4:** Viết các Unit Test kiểm tra an toàn trùng hoạt chất và Integration Test kiểm tra tính atomic rollback khi thiếu kho.

### Giai đoạn 3: Giao diện & Workspace integration (Frontend)
* **Bước 3.1:** Thiết lập Pinia store `doctorSession` hỗ trợ auto-save draft vào LocalStorage, tìm kiếm autocomplete, và tính toán chi phí tạm tính.
* **Bước 3.2:** Dựng giao diện Workspace 3 cột mờ kính CSS HSL cho PC phòng khám, Slide-out panel bệnh sử cho laptop và responsive cho tablet đi buồng.
* **Bước 3.3:** Tích hợp bộ gõ Autocomplete thuốc có tích hợp `lodash.debounce` chống spam API và hiển thị nhãn tồn kho khả dụng có mã màu.
* **Bước 3.4:** Đăng ký popup in đơn thuốc PDF tự động bật lên sau khi hoàn thành ca khám.

---

## 2. Kịch bản Kiểm thử chất lượng chi tiết (QA Test Cases)

### A. Kiểm thử Nghiệp vụ & Tính Atomic (Functional & ACID Testing)

#### TC-FUN-01: Kê đơn thành công & Cập nhật kho y tế (Happy Path)
* **Mục tiêu:** Xác minh hệ thống trừ kho chính xác và sinh hóa đơn nháp khi đủ thuốc.
* **Kịch bản giả lập:** Lịch hẹn ID 147, thú cưng Bé Leo. Thuốc Amoxicillin 500mg còn 100 viên trong kho.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Bác sĩ Trần Quốc Anh.
  2. Tiếp nhận lịch hẹn 147, ghi triệu chứng, chẩn đoán.
  3. Kê Amoxicillin 500mg với số lượng 10. Bấm "Hoàn thành ca khám".
* **Kết quả mong đợi:**
  * API trả về `201 Created`.
  * Trạng thái lịch hẹn 147 chuyển sang `completed`.
  * Số lượng tồn kho Amoxicillin 500mg giảm từ 100 xuống 90.
  * DB tạo mới 1 bản ghi bệnh án, 1 đơn thuốc và 1 hóa đơn nháp (Draft Invoice) chứa giá dịch vụ + giá 10 viên thuốc.

#### TC-FUN-02: Kiểm thử tính Atomic (Rollback) khi một dòng thuốc bị thiếu kho
* **Mục tiêu:** Đảm bảo toàn bộ giao dịch bị hủy bỏ nếu có bất kỳ loại thuốc nào không đủ hàng.
* **Kịch bản giả lập:** Kê đơn 2 loại thuốc: Amoxicillin 500mg (cần 10, tồn kho 100) và Siro ho Astex (cần 2, tồn kho chỉ còn 1).
* **Các bước thực hiện:** Gửi request lưu bệnh án.
* **Kết quả mong đợi:**
  * API trả về lỗi `400 Bad Request` báo chi tiết: *"Thuốc Siro ho Astex trong kho hiện chỉ còn 1 liều, không đủ số lượng yêu cầu: 2"*.
  * Thực thi Database Rollback thành công: Không có bản ghi bệnh án hay đơn thuốc nào được lưu.
  * Tồn kho Amoxicillin 500mg **giữ nguyên là 100** (không bị trừ oan).

---

### B. Kiểm thử Bảo mật & Biên (Security & Concurrency Testing)

#### TC-SEC-01: Chặn IDOR bác sĩ xem trộm bệnh sử thú cưng ngoài ca khám
* **Mục tiêu:** Bác sĩ không được tự ý xem bệnh lịch của thú cưng không nằm trong danh sách ca trực hôm nay của mình.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản Bác sĩ A.
  2. Gửi request `GET /api/doctor/pets/99/medical-history` (Pet ID 99 thuộc hàng chờ khám của Bác sĩ B).
* **Kết quả mong muốn:** API trả về lỗi `403 Forbidden` kèm thông điệp: *"Quyền truy cập bệnh sử bị từ chối"*.

---

### C. Mã nguồn Integration Test C# tham khảo (xUnit & FluentAssertions)

```csharp
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class MedicalRecordIntegrationTests
{
    [Fact]
    public async Task CreateMedicalRecord_InsufficientStock_ShouldRollbackAllChanges()
    {
        // Arrange
        var medicineA = new Medicine { Id = 16, Name = "Amoxicillin", StockQuantity = 100, Price = 10 };
        var medicineB = new Medicine { Id = 17, Name = "Siro ho", StockQuantity = 1, Price = 50 };
        
        await dbContext.Medicines.AddRangeAsync(medicineA, medicineB);
        await dbContext.SaveChangesAsync();

        var service = new MedicalRecordService(dbContext, safetyChecker);
        var dto = new CreateMedicalRecordDto
        {
            AppointmentId = 147,
            PetId = 12,
            Diagnosis = "Viêm phế quản cấp",
            PrescriptionItems = new List<PrescriptionItemDto>
            {
                new() { MedicineId = 16, Quantity = 10, DosageInstructions = "Ngày 2 lần" },
                new() { MedicineId = 17, Quantity = 5, DosageInstructions = "Ngày 3 lần" } // Kê 5, tồn 1 (Thiếu)
            }
        };

        // Act
        Func<Task> act = async () => await service.CreateMedicalRecordAsync(dto, doctorId);

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>();
        
        // Xác minh dữ liệu được Rollback hoàn toàn
        var freshMedicineA = await dbContext.Medicines.FindAsync(16);
        freshMedicineA.StockQuantity.Should().Be(100); // Không bị trừ

        var medicalRecordExists = await dbContext.MedicalRecords.AnyAsync(r => r.AppointmentId == 147);
        medicalRecordExists.Should().BeFalse(); // Không tạo bệnh án
    }
}
```
