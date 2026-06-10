# 🌐 Infrastructure & Security - Cashier & Invoicing

## 🔗 Skills Liên Quan
- **BE-A03 (RBAC):** Cấu hình phân quyền `[Authorize(Roles = "receptionist,cashier,admin")]` để kiểm soát chặt chẽ quyền lập và xác nhận hóa đơn, tránh thất thoát doanh thu.
- **BE-C02 (EF Core):** Thực thi Transaction cô lập an toàn mức độ cao để bảo toàn tính toàn vẹn dữ liệu khi cập nhật đồng thời hóa đơn và lịch hẹn.

---

## 1. Phân Quyền Hóa Đơn (RBAC Configuration)

Chỉ những vai trò có trách nhiệm quản lý tài chính phòng khám mới được phép ghi nhận dòng tiền và xác nhận đã nhận thanh toán:

```csharp
[Authorize(Roles = "receptionist,cashier,admin")]
[ApiController]
[Route("api/receptionist/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDraft([FromBody] CreateInvoiceRequest request)
    {
        var invoice = await _invoiceService.CreateInvoiceDraftAsync(request.AppointmentId);
        return Ok(invoice);
    }
}
```

---

## 2. Giao dịch đồng bộ thanh toán hóa đơn & lịch hẹn
Khi xác nhận thanh toán thành công, hệ thống bắt buộc phải thay đổi trạng thái của cả hóa đơn và lịch hẹn trong một Transaction duy nhất:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    var invoice = await _context.Invoices.FindAsync(invoiceId);
    invoice.Status = InvoiceStatus.Paid;
    invoice.PaymentMethod = paymentMethod;

    var appointment = await _context.Appointments.FindAsync(invoice.AppointmentId);
    appointment.PaymentStatus = PaymentStatus.Paid;

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```
