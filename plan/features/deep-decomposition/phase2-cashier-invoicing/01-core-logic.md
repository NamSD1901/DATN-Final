# 🧠 Core Business Logic - Cashier & Invoicing

## 🔗 Skills Liên Quan
- **BE-F01 (C# Fundamentals):** Định dạng mã hóa đơn duy nhất theo ngày (VD: `HD-YYYYMMDD-XXX`) để phục vụ đối chiếu kế toán.
- **BE-F03 (Async/Await):** Truy vấn dữ liệu từ nhiều bảng (Appointments, MedicalRecords, Prescriptions) đồng thời bằng cách tận dụng truy vấn bất đồng bộ.

---

## 1. C# Logic: Tạo mã hóa đơn & Tính tổng tiền tự động

```csharp
public class InvoiceService
{
    private readonly MyPetClinicDbContext _context;

    public InvoiceService(MyPetClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateInvoiceDraftAsync(Guid appointmentId)
    {
        // 1. Lấy thông tin lịch hẹn, bao gồm thông tin bệnh án và thuốc
        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.MedicalRecord)
                .ThenInclude(m => m.Prescription)
                    .ThenInclude(p => p.PrescriptionItems)
                        .ThenInclude(pi => pi.Medicine)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            throw new Exception("Lịch hẹn không tồn tại.");

        if (appointment.MedicalRecord == null)
            throw new Exception("Bác sĩ chưa ghi nhận kết quả khám lâm sàng.");

        // 2. Tính tiền dịch vụ
        decimal serviceAmount = appointment.Service?.Price ?? 0;
        decimal medicineAmount = 0;
        var invoiceItems = new List<InvoiceItem>();

        // Thêm phí dịch vụ khám vào item hóa đơn
        if (appointment.Service != null)
        {
            invoiceItems.Add(new InvoiceItem
            {
                ItemName = appointment.Service.Name,
                Quantity = 1,
                UnitPrice = appointment.Service.Price,
                TotalPrice = appointment.Service.Price
            });
        }

        // 3. Tính tiền thuốc
        var prescription = appointment.MedicalRecord.Prescription;
        if (prescription != null && prescription.PrescriptionItems != null)
        {
            foreach (var item in prescription.PrescriptionItems)
            {
                var itemTotal = item.Quantity * item.Medicine.Price;
                medicineAmount += itemTotal;

                invoiceItems.Add(new InvoiceItem
                {
                    ItemName = item.Medicine.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.Medicine.Price,
                    TotalPrice = itemTotal
                });
            }
        }

        // 4. Sinh mã số hóa đơn duy nhất trong ngày
        string invoiceNumber = await GenerateInvoiceNumberAsync();

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            AppointmentId = appointmentId,
            ServiceAmount = serviceAmount,
            MedicineAmount = medicineAmount,
            TotalAmount = serviceAmount + medicineAmount,
            Status = InvoiceStatus.Unpaid,
            CreatedAt = DateTime.UtcNow,
            InvoiceItems = invoiceItems
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await _context.Invoices
            .Where(i => i.InvoiceNumber.StartsWith($"HD-{todayStr}"))
            .CountAsync();

        return $"HD-{todayStr}-{(countToday + 1):D3}";
    }
}
```
