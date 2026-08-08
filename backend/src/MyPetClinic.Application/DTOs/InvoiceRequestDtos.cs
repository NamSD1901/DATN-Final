namespace MyPetClinic.Application.DTOs
{
    /// <summary>
    /// Request DTO để thêm một dòng vào hóa đơn (dịch vụ hoặc thuốc).
    /// </summary>
    public class AddInvoiceItemRequestDto
    {
        public long InvoiceId { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Request DTO để cập nhật số lượng một dòng hóa đơn.
    /// </summary>
    public class UpdateInvoiceItemQtyRequestDto
    {
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Request DTO để xử lý thanh toán hóa đơn.
    /// </summary>
    public class ProcessPaymentRequestDto
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public string? VoucherCode { get; set; }
    }
}
