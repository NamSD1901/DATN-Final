namespace MyPetClinic.Application.DTOs
{
    public class PrescribedMedicineDto
    {
        public string MedicineName { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
        public int? Quantity { get; set; }
        public string? Instruction { get; set; }
    }
}
