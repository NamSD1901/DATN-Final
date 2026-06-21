using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class VaccineBatch
    {
        public long Id { get; set; }
        public long VaccineId { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public DateTime ImportDate { get; set; } = DateTime.UtcNow;
        public int StockQuantity { get; set; } = 0;
        public decimal ImportPrice { get; set; }
        public decimal SellingPrice { get; set; }

        public Vaccine? Vaccine { get; set; }
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}
