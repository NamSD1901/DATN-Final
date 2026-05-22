using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Vaccine
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Description { get; set; }

        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}
