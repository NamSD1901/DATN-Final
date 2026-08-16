using MyPetClinic.Application.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<QueueItemDto>> GetPendingCheckoutsAsync();
        Task<InvoiceDto> GetOrCreateInvoiceAsync(long appointmentId);
        Task<InvoiceDto> AddInvoiceItemAsync(long invoiceId, string itemType, long itemId, int quantity);
        Task<InvoiceDto> RemoveInvoiceItemAsync(long itemId);
        Task<InvoiceDto> UpdateInvoiceItemQtyAsync(long itemId, int quantity);
        Task<bool> ProcessPaymentAsync(long invoiceId, string paymentMethod, decimal discountAmount, string? voucherCode = null);
        Task<IEnumerable<InvoiceCatalogItemDto>> GetCatalogItemsAsync(string query);
        Task<IEnumerable<InvoiceDto>> GetCustomerInvoicesAsync(System.Guid customerId);
        Task<bool> ProcessSePayWebhookAsync(SePayWebhookDto payload);
    }
}
