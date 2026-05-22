namespace MyPetClinic.Domain.Entities
{
    public class InvoiceItem
    {
        public long Id { get; set; }
        public long InvoiceId { get; set; }
        public string? ItemType { get; set; }
        public long? ItemId { get; set; }
        public string? ItemName { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; } = 0;
        public decimal TotalPrice { get; set; } = 0;

        public Invoice? Invoice { get; set; }
    }
}
