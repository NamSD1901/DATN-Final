namespace MyPetClinic.Application.DTOs
{
    public class InvoiceCatalogItemDto
    {
        public string Type { get; set; } = null!; // "service" or "medicine"
        public long Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public int StockQuantity { get; set; }
    }
}
