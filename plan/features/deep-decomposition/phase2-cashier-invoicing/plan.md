# 📅 Implementation Plan & Test Strategy - Cashier & Invoicing

Tài liệu lập kế hoạch triển khai chi tiết (Micro-roadmap) và bộ kịch bản kiểm thử (Test Strategy) dành cho phân hệ Thu ngân & Hóa đơn.

---

## 1. Lộ trình Triển khai Chi tiết (4-Phase Micro-Roadmap)

### Giai đoạn 1: Thiết kế Cơ sở dữ liệu & C# Entities (Tuần 1)
- Xây dựng thực thể `Invoice` và `InvoiceItem` trong Domain Layer.
- Thiết lập cấu hình EF Core Fluent API trong Infrastructure Layer, đăng ký các index tối ưu.
- Thực hiện Database Migrations và chạy seed dữ liệu mẫu cho bảng dịch vụ phòng khám.

### Giai đoạn 2: Phát triển Backend Services & API (Tuần 2)
- Viết logic tính tiền hóa đơn có VAT 10% tại `InvoiceService`.
- Tích hợp Database Transaction (ACID) đồng bộ hóa trạng thái hóa đơn & lịch hẹn.
- Phát triển API Controller và các DTO validations.
- Thiết lập Rate Limiting và phân quyền truy cập Roles.

### Giai đoạn 3: Phát triển Giao diện Vue 3 Client (Tuần 3)
- Viết Pinia Store `useInvoiceStore.ts` bằng TypeScript kết nối RESTful API.
- Dựng giao diện Workspace Thu ngân với phong cách Glassmorphism.
- Thiết kế Popup sinh mã VietQR động sử dụng thư viện Canvas.
- Cấu hình `@media print` cho hóa đơn in nhiệt khổ K80.

### Giai đoạn 4: Kiểm thử, Tối ưu & Bàn giao (Tuần 4)
- Viết unit tests kiểm tra logic tính tiền thuốc và phí dịch vụ.
- Viết integration tests kiểm tra rollback transaction khi thanh toán lỗi.
- Kiểm tra tính tương thích in ấn trên các trình duyệt thực tế.

---

## 2. Kịch bản Kiểm thử QA (QA Test Cases)

| Mã Test Case | Phân loại | Mục tiêu kiểm thử | Các bước thực hiện | Kết quả mong đợi |
| :--- | :--- | :--- | :--- | :--- |
| **TC-INV-01** | Unit Test | Kiểm tra công thức tính tổng tiền hóa đơn | Lập hóa đơn cho ca khám có phí khám 100k, thuốc A giá 50k (SL 2), thuốc B giá 30k (SL 1). | Tổng tiền thuốc có thuế VAT = (100k + 30k) * 1.1 = 143k. Tổng hóa đơn = 100k (phí khám) + 143k = 243.000đ. |
| **TC-INV-02** | Security | Chống tấn công IDOR xem hóa đơn người khác | Đăng nhập tài khoản Customer A, gọi API `GET /api/customer/invoices/{id}` với `id` của hóa đơn thuộc Customer B. | Backend chặn đứng lập tức và trả về mã lỗi `403 Forbidden`. |
| **TC-INV-03** | Boundary | Chặn thanh toán hóa đơn đã thanh toán | Gọi API `PUT /pay` lên một hóa đơn có trạng thái đã là `Paid`. | Hệ thống từ chối giao dịch, trả về lỗi `400 Bad Request`. |
| **TC-INV-04** | Integration | Kiểm tra tính toàn vẹn Transaction khi DB lỗi | Giả lập lỗi ngắt kết nối DB ngay sau bước chuyển trạng thái Hóa đơn sang `Paid` nhưng trước bước cập nhật Lịch hẹn. | Toàn bộ giao dịch phải được rollback. Trạng thái Hóa đơn quay về `Pending`. |

---

## 3. Mã nguồn Unit Test C# xUnit mẫu

Dưới đây là mã nguồn unit test sử dụng **xUnit** và **FluentAssertions** kiểm định tính đúng đắn của logic tính tiền hóa đơn:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;
using Xunit;

namespace MyPetClinic.Tests
{
    public class InvoiceServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly InvoiceService _invoiceService;

        public InvoiceServiceTests()
        {
            // Sử dụng InMemory Database để test nhanh logic
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _invoiceService = new InvoiceService(_context, NullLogger<InvoiceService>.Instance);
        }

        [Fact]
        public async Task CreateInvoice_ShouldCalculateTotalAmountWith10PercentVatOnMedicines()
        {
            // Arrange (Thiết lập dữ liệu giả lập)
            var appointmentId = Guid.NewGuid();
            var cashierId = Guid.NewGuid();

            var pet = new Pet { Id = Guid.NewGuid(), Name = "LuLu", Species = "Cat" };
            var appointment = new Appointment
            {
                Id = appointmentId,
                Status = "Completed",
                ServiceFee = 100000m, // Phí khám 100k
                Pet = pet,
                PaymentStatus = "Unpaid"
            };

            var medicine = new Medicine { Id = Guid.NewGuid(), Name = "Amoxicillin", Price = 10000m }; // Đơn giá 10k
            var medicalRecord = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                Prescriptions = new List<Prescription>
                {
                    new Prescription
                    {
                        Id = Guid.NewGuid(),
                        MedicineId = medicine.Id,
                        Medicine = medicine,
                        Quantity = 5 // 5 viên * 10k = 50k tiền thuốc gốc
                    }
                }
            };
            appointment.MedicalRecord = medicalRecord;

            _context.Appointments.Add(appointment);
            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            // Act (Thực thi hàm test)
            var result = await _invoiceService.CreateInvoiceFromAppointmentAsync(appointmentId, cashierId);

            // Assert (Xác minh kết quả)
            // Phí khám: 100,000 đ
            // Tiền thuốc gốc: 5 * 10,000 = 50,000 đ
            // Tiền thuốc có VAT 10%: 50,000 * 1.10 = 55,000 đ
            // Tổng hóa đơn: 100,000 + 55,000 = 155,000 đ
            result.Should().NotBeNull();
            result.TotalAmount.Should().Be(155000m);
            result.Status.Should().Be("Pending");
            result.Items.Should().HaveCount(2);
            
            var serviceItem = result.Items.Find(i => i.ItemType == "Service");
            serviceItem.Should().NotBeNull();
            serviceItem!.SubTotal.Should().Be(100000m);

            var medicineItem = result.Items.Find(i => i.ItemType == "Medicine");
            medicineItem.Should().NotBeNull();
            medicineItem!.SubTotal.Should().Be(55000m);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
```
