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
        public string RecordType { get; set; } = "Consultation";
        public string MedicalHistory { get; set; } = string.Empty;
        public string? RawMedicalHistory { get; set; }
        public decimal Weight { get; set; }
        public decimal Temperature { get; set; }
        public string ClinicalSigns { get; set; } = string.Empty;
        public string? RawClinicalSigns { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? RawDiagnosis { get; set; }
        public string TreatmentPlan { get; set; } = string.Empty;
        public string? RawTreatmentPlan { get; set; }
        public string DoctorNotes { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
        public List<string> Attachments { get; set; } = new List<string>();
        public List<PrescribedMedicineDto> PrescribedMedicines { get; set; } = new List<PrescribedMedicineDto>();
        
        // Financial Visibility
        public long? InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
    }

    public class CreateMedicalRecordDto
    {
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        public string RecordType { get; set; } = "Consultation";
        public string? MedicalHistory { get; set; }
        public decimal Weight { get; set; }
        public decimal Temperature { get; set; }
        public string ClinicalSigns { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
        public DateTime? FollowUpDate { get; set; }
        
        // Auto-create Follow-up appointment
        public bool CreateFollowUpAppointment { get; set; }
        public string? FollowUpType { get; set; } // Normal, FollowUp, Revaccination
        public string? FollowUpNote { get; set; }
        public long? FollowUpServiceId { get; set; }
        public Guid? FollowUpDoctorId { get; set; } // Nếu muốn chỉ định bác sĩ khác, hoặc null để giữ nguyên bác sĩ hiện tại

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
    public class UpdateMedicalRecordDto
    {
        public decimal Weight { get; set; }
        public decimal Temperature { get; set; }
        public string ClinicalSigns { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
        public DateTime? FollowUpDate { get; set; }
    }

    public class MedicalRecordCustomerViewDto
    {
        public long RecordId { get; set; }
        public long AppointmentId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal Temperature { get; set; }
        
        public string Diagnosis { get; set; } = string.Empty;
        public string MedicalHistory { get; set; } = string.Empty;
        public string ClinicalSigns { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;
        public string CareInstructions { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }
        
        public List<string> Attachments { get; set; } = new();
        public List<PrescribedMedicineDto> Prescriptions { get; set; } = new();

        public long? InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
    }
}
