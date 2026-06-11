# 04. Infrastructure & Security - Cashier & Invoicing

Tài liệu thiết kế hạ tầng, phân quyền vai trò (RBAC), bảo mật chống IDOR dữ liệu tài chính, và tích hợp API VietQR.

---

## 1. Cơ chế Phân quyền Người dùng (Role-Based Access Control)

Các API quản lý hóa đơn (tạo, cập nhật trạng thái thanh toán, hủy) chứa dữ liệu tài chính nhạy cảm của phòng khám, vì vậy cần áp dụng phân quyền nghiêm ngặt:
- **Vai trò Lễ tân / Thu ngân (`receptionist`, `cashier`):** Có toàn quyền xem danh sách hóa đơn chờ, lập hóa đơn mới, sinh QR Code, xác nhận khách đã thanh toán, hủy hóa đơn bị lập sai.
- **Vai trò Quản trị viên (`admin`):** Có toàn quyền tương tự thu ngân và có thêm quyền sửa đổi thông tin bảng giá dịch vụ cấu hình gốc.
- **Vai trò Khách hàng (`customer`):** Chỉ có quyền xem duy nhất chi tiết các hóa đơn của chính thú cưng do mình sở hữu (`Read-Only`). Không được quyền xem danh sách hóa đơn của khách hàng khác hay tự ý đổi trạng thái hóa đơn sang `Paid`.

### Mã nguồn C# Minh họa Phân quyền tại Controller:
```csharp
[ApiController]
[Route("api/receptionist/invoices")]
[Authorize] // Bắt buộc đăng nhập JWT hợp lệ
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    [Authorize(Roles = "receptionist,cashier,admin")] // Chỉ nhân viên được tạo
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        var cashierId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _invoiceService.CreateInvoiceFromAppointmentAsync(request.AppointmentId, cashierId);
        return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, result);
    }

    [HttpPut("{id}/pay")]
    [Authorize(Roles = "receptionist,cashier,admin")] // Chỉ nhân viên được xác nhận thu tiền
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request)
    {
        var cashierId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _invoiceService.ConfirmPaymentAsync(id, request, cashierId);
        return Ok(result);
    }
}
```

---

## 2. Phòng chống IDOR Tài chính (Insecure Direct Object Reference)

Để ngăn chặn khách hàng xấu thay đổi tham số `invoiceId` trên đường dẫn URL (ví dụ: `GET /api/customer/invoices/{id}`) để đọc lén thông tin đơn thuốc và chi phí thanh toán của người khác, backend áp dụng thuật toán kiểm định tính sở hữu sau:

```csharp
// Trong CustomerInvoiceController.cs hoặc UserService
[HttpGet("{id}")]
[Authorize(Roles = "Customer")]
public async Task<IActionResult> GetCustomerInvoiceById(Guid id)
{
    // 1. Giải mã ID người dùng hiện tại từ JWT Claims
    var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(currentUserIdStr) || !Guid.TryParse(currentUserIdStr, out var currentUserId))
    {
        return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn" });
    }

    // 2. Lấy thông tin hóa đơn kèm theo thông tin chủ sở hữu thú cưng
    var invoice = await _context.Invoices
        .Include(i => i.Appointment)
            .ThenInclude(a => a.Pet)
        .FirstOrDefaultAsync(i => i.Id == id);

    if (invoice == null)
    {
        return NotFound(new { message = "Không tìm thấy hóa đơn được yêu cầu" });
    }

    // 3. Đối chiếu: Nếu ID chủ thú cưng không trùng với ID người dùng đăng nhập -> Chặn đứng!
    if (invoice.Appointment.Pet.OwnerId != currentUserId)
    {
        // Trả về Forbidden (403) để chặn đứng IDOR
        return Forbid();
    }

    // 4. Nếu hợp lệ, trả về DTO
    var dto = MapToDto(invoice);
    return Ok(dto);
}
```

---

## 3. Tích hợp API Sinh mã VietQR Động

Hệ thống tích hợp cổng VietQR (chuẩn QR Napas 247 của Việt Nam) để sinh nhanh mã QR Code động mà không cần thông qua đơn vị trung gian thanh toán thu phí đắt đỏ.

- **URL API VietQR:** `https://api.vietqr.io/v2/generate`
- **Phương thức:** `POST`
- **Cấu hình Payload gửi đi:**
```json
{
  "accountNo": "190288889999",
  "accountName": "PHONG KHAM MYPETCLINIC",
  "acqId": "970422", 
  "amount": 430000,
  "addInfo": "MYPETCLINIC INVOICE INV-20260611-0043",
  "format": "text",
  "template": "qr_only"
}
```
*(Trong đó `acqId = 970422` là mã BIN ngân hàng Quân Đội MB Bank)*.

Mã nguồn C# gọi dịch vụ VietQR:
```csharp
public async Task<string> GenerateVietQrCodeAsync(decimal amount, string invoiceNumber)
{
    var client = _httpClientFactory.CreateClient();
    var payload = new
    {
        accountNo = "190288889999",
        accountName = "PHONG KHAM MYPETCLINIC",
        acqId = "970422", // MB Bank BIN
        amount = (int)amount,
        addInfo = $"MYPETCLINIC INVOICE {invoiceNumber}",
        format = "text",
        template = "qr_only"
    };

    var response = await client.PostAsJsonAsync("https://api.vietqr.io/v2/generate", payload);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<VietQrResponseDto>();
    return result?.Data?.QrDataUrl ?? throw new InvalidOperationException("Không thể sinh mã VietQR.");
}
```

---

## 4. Rate Limiting Chính sách Giới hạn Tần suất

Nhằm ngăn chặn hành vi spam hoặc tấn công từ chối dịch vụ (DDoS) vào cổng thanh toán y tế:
- **Tạo hóa đơn:** Giới hạn tối đa **20 requests / phút** trên một tài khoản nhân viên.
- **Xác nhận thanh toán:** Giới hạn **10 requests / phút** trên một tài khoản nhân viên để tránh click đúp sinh nhiều giao dịch đồng thời.
- **Xem mã QR động:** Sử dụng cache ngắn hạn (Memory Cache 1 phút) cho cùng một mã hóa đơn để tránh gọi API VietQR liên tục gây quá tải.
