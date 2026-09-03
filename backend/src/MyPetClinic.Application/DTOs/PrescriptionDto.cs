using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class PrescriptionDto
    {
        public long Id { get; set; }
        public string? Diagnosis { get; set; } // Map from MedicalRecord.Diagnosis
        public DateTime Date { get; set; } // Map from CreatedAt
        public string? Doctor { get; set; } // Map from Doctor.FullName
        public string? Notes { get; set; }
        public string Status { get; set; } = "completed"; // "active" or "completed"
        public string? MedicalHistory { get; set; } // Map from MedicalRecord.MedicalHistory (S)
        public string? ClinicalSigns { get; set; } // Map from MedicalRecord.ClinicalSigns (O)
        public string? TreatmentPlan { get; set; } // Map from MedicalRecord.TreatmentPlan (P)
        public List<PrescriptionItemDto> Medicines { get; set; } = new List<PrescriptionItemDto>();
    }
}
