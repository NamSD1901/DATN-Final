using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class MedicalRecordSoapResponseDto
    {
        public long RecordId { get; set; }
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;

        // S: Subjective
        public SubjectiveDto Subjective { get; set; } = new();

        // O: Objective
        public ObjectiveDto Objective { get; set; } = new();

        // A: Assessment
        public AssessmentDto Assessment { get; set; } = new();

        // P: Plan
        public PlanDto Plan { get; set; } = new();

        // Financial Visibility
        public long? InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
    }
}
