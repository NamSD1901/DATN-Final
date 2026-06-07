namespace MyPetClinic.Application.DTOs
{
    public class InvoiceItemDto
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public string? ItemType { get; set; } // "service" or "medicine" or "product"
        public long? ItemId { get; set; }
        public string? ItemName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
