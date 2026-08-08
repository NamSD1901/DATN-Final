using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(Roles = "receptionist,admin,Receptionist,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingCheckouts()
        {
            var queue = await _invoiceService.GetPendingCheckoutsAsync();
            return Ok(queue);
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetDetail(long appointmentId)
        {
            try
            {
                var invoice = await _invoiceService.GetOrCreateInvoiceAsync(appointmentId);
                return Ok(invoice);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddInvoiceItemRequestDto req)
        {
            try
            {
                var invoice = await _invoiceService.AddInvoiceItemAsync(req.InvoiceId, req.ItemType, req.ItemId, req.Quantity);
                return Ok(new { success = true, invoice });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(long itemId)
        {
            try
            {
                var invoice = await _invoiceService.RemoveInvoiceItemAsync(itemId);
                return Ok(new { success = true, invoice });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("items/{itemId}")]
        public async Task<IActionResult> UpdateQty(long itemId, [FromBody] UpdateInvoiceItemQtyRequestDto req)
        {
            try
            {
                var invoice = await _invoiceService.UpdateInvoiceItemQtyAsync(itemId, req.Quantity);
                return Ok(new { success = true, invoice });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{invoiceId}/process-payment")]
        public async Task<IActionResult> ProcessPayment(long invoiceId, [FromBody] ProcessPaymentRequestDto req)
        {
            try
            {
                var success = await _invoiceService.ProcessPaymentAsync(invoiceId, req.PaymentMethod, req.DiscountAmount, req.VoucherCode);
                return Ok(new { success });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog([FromQuery] string query)
        {
            var catalog = await _invoiceService.GetCatalogItemsAsync(query);
            return Ok(catalog);
        }
    }

}
