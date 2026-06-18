using System;

namespace MyPetClinic.Application.DTOs
{
    public class MedicineDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal SellPrice { get; set; }
    }
}
