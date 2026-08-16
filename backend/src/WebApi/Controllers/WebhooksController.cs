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
                var match = Regex.Match(payload.Content ?? "", @"MPC(\d+)");
                if (match.Success && long.TryParse(match.Groups[1].Value, out long invoiceId))
                {
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

        /// <summary>
        /// [CHỈ DÙNG ĐỂ TEST] Giả lập thanh toán thành công cho một hóa đơn.
        /// Endpoint này tự động bị vô hiệu hóa ở môi trường Production (Render).
        /// </summary>
        [HttpPost("simulate/{invoiceId}")]
        public async Task<IActionResult> SimulatePayment(long invoiceId)
        {
            // Bảo vệ: Chỉ cho phép chạy ở môi trường Development
            if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                return NotFound(); // Ẩn hoàn toàn trên Production
            }

            var fakePayload = new SePayWebhookDto
            {
                Id = 999999,
                Gateway = "TPBank",
                TransactionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                AccountNumber = "00001562694",
                Code = null,
                Content = $"MPC{invoiceId}",
                TransferAmount = 1000000,
                TransferType = "in",
                Accumulated = 0,
                SubAccount = null,
                ReferenceCode = $"SIMULATE-{invoiceId}",
                Description = $"THANH TOAN MPC{invoiceId}"
            };

            bool success = await _invoiceService.ProcessSePayWebhookAsync(fakePayload);

            // Gửi SignalR ngay lập tức để Frontend cập nhật
            await _hubContext.Clients.All.SendAsync("ReceiveSePayPayment", new
            {
                invoiceId = invoiceId,
                message = $"[GIẢ LẬP] Hóa đơn MPC{invoiceId} đã được thanh toán thành công!"
            });

            return Ok(new { success = true, simulated = true, invoiceId = invoiceId });
        }
    }
}
