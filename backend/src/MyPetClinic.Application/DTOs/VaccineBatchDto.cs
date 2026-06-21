using System;

namespace MyPetClinic.Application.DTOs
{
    public class VaccineBatchDto
    {
        public long Id { get; set; }
        public long VaccineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int StockQuantity { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
