using System;

namespace MyPetClinic.Domain.Entities
{
    public class VaccinationRecord
    {
        public long Id { get; set; }
        public long PetId { get; set; }
        public long? VaccineId { get; set; }
        public long? VaccineBatchId { get; set; }
        public long? AppointmentId { get; set; }
        public Guid DoctorId { get; set; }

        // S - Subjective
        public string? ReasonForVisit { get; set; }
        public string? PreviousVaccineHistory { get; set; }
        public bool IsAllergic { get; set; }
        public string? AllergyDetails { get; set; }
        public bool HasPreviousReaction { get; set; }
        public string? PreviousReactionDetails { get; set; }
        public bool IsUnderTreatment { get; set; }
        public string? TreatmentDetails { get; set; }
        public string? EatingStatus { get; set; }
        public bool HasVomitingOrDiarrhea { get; set; }
        public bool HasCoughOrSneeze { get; set; }
        public string? OwnerNotes { get; set; }

        // O - Objective
        public decimal Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public string? MentalStatus { get; set; }
        public string? MucosaStatus { get; set; }
        public string? EyeNoseEarStatus { get; set; }
        public string? LymphNodeStatus { get; set; }
        public int? DehydrationPercent { get; set; }
        public string? Attachments { get; set; } // Danh sách URL ảnh cận lâm sàng (JSON string)
        
        // Vaccine Plan Specifics
        public decimal? Dose { get; set; }
        public string? Route { get; set; }
        public string? InjectionSite { get; set; }

        // A - Assessment
        public string? ClinicalAssessment { get; set; }
        public string? DoctorRemarks { get; set; }

        // P - Plan
        public DateTime InjectionDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string? FollowUpInstructions { get; set; }
        public string? ReactionNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Pet? Pet { get; set; }
        public Vaccine? Vaccine { get; set; }
        public VaccineBatch? VaccineBatch { get; set; }
        public Appointment? Appointment { get; set; }
        public User? Doctor { get; set; }
    }
}
