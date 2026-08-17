using System;

namespace MyPetClinic.Application.DTOs
{
    public class VaccinationSoapResponseDto
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // S
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

        // O
        public decimal Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public string? MentalStatus { get; set; }
        public string? MucosaStatus { get; set; }
        public string? EyeNoseEarStatus { get; set; }
        public string? LymphNodeStatus { get; set; }
        public int? DehydrationPercent { get; set; }
        
        // Vaccine Details
        public long? VaccineId { get; set; }
        public string? VaccineName { get; set; }
        public long? VaccineBatchId { get; set; }
        public string? BatchNumber { get; set; }
        public decimal? Dose { get; set; }
        public string? Route { get; set; }
        public string? InjectionSite { get; set; }

        // A
        public string? ClinicalAssessment { get; set; }
        public string? DoctorRemarks { get; set; }

        // P
        public DateTime InjectionDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string? FollowUpInstructions { get; set; }
        public string? ReactionNote { get; set; }
        public List<string> Attachments { get; set; } = new();
    }
}
