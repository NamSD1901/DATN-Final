using System;

namespace MyPetClinic.Domain.Entities
{
    public class Medicine
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int StockQuantity { get; set; } = 0;
        public decimal? ImportPrice { get; set; }
        public decimal? SellPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }
}
