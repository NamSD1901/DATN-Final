using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class VaccineWithBatchesDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? TargetSpecies { get; set; }
        public int? MinAgeWeeks { get; set; }
        public int? IntervalDays { get; set; }
        public List<VaccineBatchDto> Batches { get; set; } = new();
    }
}
