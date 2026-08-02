using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class MedicineDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public IEnumerable<MedicineBatchDto> Batches { get; set; } = new List<MedicineBatchDto>();
    }
}
