using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Prescription
    {
        public long Id { get; set; }
        public long MedicalRecordId { get; set; }
        public Guid DoctorId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MedicalRecord? MedicalRecord { get; set; }
        public User? Doctor { get; set; }
        public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
    }
}
