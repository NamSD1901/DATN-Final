using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Vaccine
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Description { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int StockQuantity { get { return VaccineBatches?.Where(b => b.ExpirationDate.ToUniversalTime() > System.DateTime.UtcNow).Sum(b => b.StockQuantity) ?? 0; } set { } }
        public string? TargetSpecies { get; set; } // "Dog", "Cat", or "All"
        public int? MinAgeWeeks { get; set; }
        public int? IntervalDays { get; set; }

        public ICollection<VaccineBatch> VaccineBatches { get; set; } = new List<VaccineBatch>();
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}
