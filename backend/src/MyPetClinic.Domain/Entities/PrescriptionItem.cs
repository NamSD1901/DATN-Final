namespace MyPetClinic.Domain.Entities
{
    public class PrescriptionItem
    {
        public long Id { get; set; }
        public long PrescriptionId { get; set; }
        public long MedicineId { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
        public int? Quantity { get; set; }
        public string? Instruction { get; set; }

        public Prescription? Prescription { get; set; }
        public Medicine? Medicine { get; set; }
    }
}
