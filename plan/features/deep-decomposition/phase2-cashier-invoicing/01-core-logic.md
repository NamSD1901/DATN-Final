# 01. Core Business Logic Reference - Cashier & Invoicing

Tài liệu đặc tả các thuật toán xử lý nghiệp vụ cốt lõi, cơ chế đảm bảo giao dịch ACID và mã nguồn C# thực thi phía Backend cho phân hệ Thu ngân & Hóa đơn.

---

## 1. Cơ chế Đảm bảo Giao dịch ACID (Database Transaction Integrity)

Khi Thu ngân bấm nút "Xác nhận đã thanh toán", hệ thống phải thực hiện đồng bộ hai thao tác thay đổi dữ liệu:
1. Chuyển trạng thái của Hóa đơn (`Invoice`) từ `Pending` sang `Paid` và lưu thời điểm thanh toán (`PaidAt`).
2. Chuyển trạng thái thanh toán của Lịch hẹn (`Appointment`) tương ứng sang `Paid` (đồng thời có thể chuyển trạng thái lịch hẹn sang `Completed` nếu trước đó bác sĩ chưa chuyển).

> [!WARNING]
> Hai thao tác này bắt buộc phải diễn ra trong một **Database Transaction** duy nhất. Nếu một trong hai thao tác thất bại (ví dụ: DB sập giữa chừng, mất kết nối mạng hoặc lịch hẹn đã bị hủy trước đó), toàn bộ thay đổi phải được Rollback lập tức để tránh tình trạng mất cân đối tài chính (Hóa đơn báo đã thu tiền nhưng lịch hẹn vẫn ghi nhận chưa thanh toán).

---

## 2. Quy tắc Tính toán Tổng số tiền (Calculation Logic)

Tổng số tiền thanh toán của hóa đơn được tính toán dựa trên các thành phần sau:
- **Phí dịch vụ (`ServiceFee`):** Là phí cố định của dịch vụ khám y khoa hoặc tiêm chủng đã được đặt.
- **Tiền thuốc (`MedicineItems`):** Tính bằng `Số lượng kê đơn * Đơn giá bán lẻ` của từng loại thuốc được bác sĩ chỉ định.
- **Tiền vắc-xin (`VaccineItems`):** Tính bằng `Số lượng liều tiêm (luôn = 1) * Đơn giá vắc-xin`.
- **Thuế VAT:** Áp dụng mức thuế giá trị gia tăng mặc định của dịch vụ y tế thú y là **10%** lên tổng giá trị tiền thuốc và vắc-xin (phí dịch vụ được miễn thuế theo quy định của nhà nước hoặc áp dụng thuế suất tùy chọn).

### Công thức chi tiết:
$$\text{SubTotalServices} = \sum_{i} \text{ServiceFee}_i$$
$$\text{SubTotalMedicines} = \sum_{j} (\text{Quantity}_j \times \text{UnitPrice}_j)$$
$$\text{SubTotalVaccines} = \sum_{k} (\text{Quantity}_k \times \text{UnitPrice}_k)$$
$$\text{TotalAmount} = \text{SubTotalServices} + (\text{SubTotalMedicines} + \text{SubTotalVaccines}) \times 1.10$$

---

## 3. Mã nguồn C# Service Thực thi Logic

Dưới đây là cài đặt chi tiết của lớp `InvoiceService` trong tầng `Application` của dự án sử dụng Entity Framework Core để quản lý nghiệp vụ hóa đơn.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.DTOs.Invoice;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Data;

namespace MyPetClinic.Application.Services
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> CreateInvoiceFromAppointmentAsync(Guid appointmentId, Guid cashierId);
        Task<InvoiceDto> ConfirmPaymentAsync(Guid invoiceId, ConfirmPaymentRequest request, Guid cashierId);
        Task<InvoiceDto> GetInvoiceDetailsAsync(Guid invoiceId);
        Task<List<InvoiceDto>> GetPendingInvoicesAsync();
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(AppDbContext context, ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InvoiceDto> CreateInvoiceFromAppointmentAsync(Guid appointmentId, Guid cashierId)
        {
            // Kiểm tra lịch hẹn tồn tại và đã khám xong chưa
            var appointment = await _context.Appointments
                .Include(a => a.Pet)
                .ThenInclude(p => p.Owner)
                .Include(a => a.MedicalRecord)
                .ThenInclude(m => m.Prescriptions)
                .ThenInclude(p => p.Medicine)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch hẹn với ID {appointmentId}");
            }

            if (appointment.Status != "Completed")
            {
                throw new InvalidOperationException("Chỉ có thể lập hóa đơn cho ca khám đã hoàn thành (Completed).");
            }

            // Kiểm tra xem lịch hẹn đã có hóa đơn chưa
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);
            if (existingInvoice != null)
            {
                return await GetInvoiceDetailsAsync(existingInvoice.Id);
            }

            // Khởi tạo Transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tự sinh mã số hóa đơn duy nhất
                string dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
                var invoiceCount = await _context.Invoices
                    .Where(i => i.CreatedAt >= DateTime.UtcNow.Date)
                    .CountAsync() + 1;
                string invoiceNumber = $"INV-{dateStr}-{invoiceCount:D4}";

                // Tạo đối tượng Invoice
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointmentId,
                    InvoiceNumber = invoiceNumber,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    CashierId = cashierId,
                    TotalAmount = 0
                };

                _context.Invoices.Add(invoice);

                decimal total = 0;
                var items = new List<InvoiceItem>();

                // 1. Thêm phí dịch vụ khám bệnh
                var serviceFeeItem = new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ItemName = "Phí khám bệnh lâm sàng",
                    ItemType = "Service",
                    Quantity = 1,
                    UnitPrice = appointment.ServiceFee,
                    SubTotal = appointment.ServiceFee
                };
                items.Add(serviceFeeItem);
                total += serviceFeeItem.SubTotal;

                // 2. Thêm chi tiết thuốc từ bệnh án
                if (appointment.MedicalRecord != null && appointment.MedicalRecord.Prescriptions != null)
                {
                    foreach (var pres in appointment.MedicalRecord.Prescriptions)
                    {
                        decimal subTotalMedicines = pres.Quantity * pres.Medicine.Price;
                        // Cộng 10% VAT cho thuốc
                        decimal subTotalWithVat = subTotalMedicines * 1.10m;

                        var medicineItem = new InvoiceItem
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoice.Id,
                            ItemName = $"Thuốc: {pres.Medicine.Name} ({pres.Medicine.ActiveIngredient})",
                            ItemType = "Medicine",
                            ReferenceId = pres.MedicineId,
                            Quantity = pres.Quantity,
                            UnitPrice = pres.Medicine.Price,
                            SubTotal = subTotalWithVat
                        };
                        items.Add(medicineItem);
                        total += medicineItem.SubTotal;
                    }
                }

                // Cập nhật tổng số tiền
                invoice.TotalAmount = total;
                _context.InvoiceItems.AddRange(items);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Đã sinh hóa đơn {invoiceNumber} cho ca khám {appointmentId}");

                return await GetInvoiceDetailsAsync(invoice.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi xảy ra khi sinh hóa đơn cho ca khám {appointmentId}");
                throw;
            }
        }

        public async Task<InvoiceDto> ConfirmPaymentAsync(Guid invoiceId, ConfirmPaymentRequest request, Guid cashierId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Appointment)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hóa đơn với ID {invoiceId}");
            }

            if (invoice.Status == "Paid")
            {
                throw new InvalidOperationException("Hóa đơn đã được thanh toán trước đó.");
            }

            if (invoice.Status == "Cancelled")
            {
                throw new InvalidOperationException("Không thể thanh toán hóa đơn đã bị hủy.");
            }

            // Thực hiện giao dịch ACID thanh toán
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Cập nhật hóa đơn
                invoice.Status = "Paid";
                invoice.PaymentMethod = request.PaymentMethod;
                invoice.PaidAt = DateTime.UtcNow;
                invoice.CashierId = cashierId;

                // 2. Cập nhật Lịch hẹn
                if (invoice.Appointment != null)
                {
                    invoice.Appointment.PaymentStatus = "Paid";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Thanh toán thành công hóa đơn {invoice.InvoiceNumber} qua {request.PaymentMethod}");

                return await GetInvoiceDetailsAsync(invoice.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Thất bại khi xác nhận thanh toán hóa đơn {invoiceId}");
                throw;
            }
        }

        public async Task<InvoiceDto> GetInvoiceDetailsAsync(Guid invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Pet)
                        .ThenInclude(p => p.Owner)
                .Include(i => i.InvoiceItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hóa đơn với ID {invoiceId}");
            }

            return new InvoiceDto
            {
                Id = invoice.Id,
                AppointmentId = invoice.AppointmentId,
                InvoiceNumber = invoice.InvoiceNumber,
                TotalAmount = invoice.TotalAmount,
                PaymentMethod = invoice.PaymentMethod,
                Status = invoice.Status,
                CreatedAt = invoice.CreatedAt,
                PaidAt = invoice.PaidAt,
                CustomerName = invoice.Appointment?.Pet?.Owner?.FullName ?? "Khách vãng lai",
                PetName = invoice.Appointment?.Pet?.Name ?? "Thú cưng",
                Items = invoice.InvoiceItems.Select(item => new InvoiceItemDto
                {
                    Id = item.Id,
                    ItemName = item.ItemName,
                    ItemType = item.ItemType,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SubTotal = item.SubTotal
                }).ToList()
            };
        }

        public async Task<List<InvoiceDto>> GetPendingInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Pet)
                        .ThenInclude(p => p.Owner)
                .Where(i => i.Status == "Pending")
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InvoiceDto
                {
                    Id = i.Id,
                    AppointmentId = i.AppointmentId,
                    InvoiceNumber = i.InvoiceNumber,
                    TotalAmount = i.TotalAmount,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt,
                    CustomerName = i.Appointment.Pet.Owner.FullName,
                    PetName = i.Appointment.Pet.Name
                })
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
```
