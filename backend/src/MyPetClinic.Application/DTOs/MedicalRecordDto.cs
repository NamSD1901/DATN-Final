using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class MedicalRecordDto
    {
        public long RecordId { get; set; }
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public decimal? Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public List<string> PrescribedMedicines { get; set; } = new List<string>();
    }

    public class CreateMedicalRecordDto
    {
        public long AppointmentId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? Note { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public List<PrescriptionLineDto> Prescriptions { get; set; } = new();
    }

    public class PrescriptionLineDto
    {
        public long MedicineId { get; set; }
        public int Quantity { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public int? DurationDays { get; set; }
        public string? Instruction { get; set; }
    }
}
