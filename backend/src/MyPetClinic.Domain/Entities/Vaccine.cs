using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Vaccine
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Description { get; set; }
        public int StockQuantity { get; set; } = 10;
        public string? TargetSpecies { get; set; } // "Dog", "Cat", or "All"
        public int? MinAgeWeeks { get; set; }
        public int? IntervalDays { get; set; }

        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}
