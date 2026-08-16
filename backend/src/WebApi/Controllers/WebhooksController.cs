using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hubs;
using System.Text.RegularExpressions;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;

        public WebhooksController(IInvoiceService invoiceService, IConfiguration configuration, IHubContext<NotificationHub> hubContext)
        {
            _invoiceService = invoiceService;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookDto payload)
        {
            // Bảo mật: Kiểm tra API Token từ SePay
            var expectedToken = _configuration["SePay:ApiToken"];
            
            // Lấy token từ Header Authorization (SePay gửi kèm tiền tố "Apikey ")
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Apikey ") || authHeader.Substring(7) != expectedToken)
            {
                return Unauthorized(new { message = "Invalid API Token" });
            }

            // Gọi service xử lý
            bool success = await _invoiceService.ProcessSePayWebhookAsync(payload);

            if (success)
            {
                // Bóc tách InvoiceId để gửi SignalR cho Frontend
                var match = Regex.Match(payload.TransactionContent ?? "", @"MPC(\d+)");
                if (match.Success && long.TryParse(match.Groups[1].Value, out long invoiceId))
                {
                    // Gửi SignalR tới toàn bộ client đang kết nối (hoặc có thể gửi vào group Receptionist nếu cấu hình)
                    // Ở đây gửi thông báo tới tất cả client, client nào đang mở Hóa đơn đó sẽ tự refresh
                    await _hubContext.Clients.All.SendAsync("ReceiveSePayPayment", new 
                    { 
                        invoiceId = invoiceId,
                        message = $"Khách hàng đã thanh toán thành công cho mã hóa đơn MPC{invoiceId}."
                    });
                }
            }

            // Luôn trả về 200 OK để SePay không gửi lại webhook
            return Ok(new { success = true });
        }
    }
}
