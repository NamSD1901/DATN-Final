using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingCheckouts()
        {
            var queue = await _invoiceService.GetPendingCheckoutsAsync();
            return Json(queue);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetail(long appointmentId)
        {
            try
            {
                var invoice = await _invoiceService.GetOrCreateInvoiceAsync(appointmentId);
                return Json(invoice);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(long invoiceId, string itemType, long itemId, int quantity)
        {
            try
            {
                var invoice = await _invoiceService.AddInvoiceItemAsync(invoiceId, itemType, itemId, quantity);
                return Json(new { success = true, invoice });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(long itemId)
        {
            try
            {
                var invoice = await _invoiceService.RemoveInvoiceItemAsync(itemId);
                return Json(new { success = true, invoice });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQty(long itemId, int quantity)
        {
            try
            {
                var invoice = await _invoiceService.UpdateInvoiceItemQtyAsync(itemId, quantity);
                return Json(new { success = true, invoice });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(long invoiceId, string paymentMethod, decimal discountAmount)
        {
            try
            {
                var success = await _invoiceService.ProcessPaymentAsync(invoiceId, paymentMethod, discountAmount);
                return Json(new { success });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalog(string query)
        {
            var catalog = await _invoiceService.GetCatalogItemsAsync(query);
            return Json(catalog);
        }
    }
}
