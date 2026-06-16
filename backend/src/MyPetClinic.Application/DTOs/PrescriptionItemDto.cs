using System;

namespace MyPetClinic.Application.DTOs
{
    public class PrescriptionItemDto
    {
        public long Id { get; set; }
        public long MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ActiveIngredient { get; set; } // Map from Medicine.Description
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
        public string? Usage { get; set; } // Derived or mapped from Instruction
        public int? Quantity { get; set; }
        public string? Unit { get; set; }
    }
}
