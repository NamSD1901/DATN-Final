using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class MedicalRecordSoapRequestDto
    {
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        
        // S: Subjective
        public SubjectiveDto Subjective { get; set; } = new();

        // O: Objective
        public ObjectiveDto Objective { get; set; } = new();

        // A: Assessment
        public AssessmentDto Assessment { get; set; } = new();

        // P: Plan
        public PlanDto Plan { get; set; } = new();
    }

    public class SubjectiveDto
    {
        public string ChiefComplaint { get; set; } = string.Empty;
        public string OnsetDuration { get; set; } = string.Empty;
        
        public string Appetite { get; set; } = "Bình thường";
        public string Thirst { get; set; } = "Bình thường";
        public bool HasVomiting { get; set; }
        public string? VomitingDetails { get; set; }
        public bool HasDiarrhea { get; set; }
        public string? DiarrheaDetails { get; set; }
        public bool HasConstipation { get; set; }
        public string UrinationIssues { get; set; } = "Bình thường";
        public bool HasCoughing { get; set; }
        public bool HasSneezing { get; set; }
        public bool HasBreathingDifficulty { get; set; }
        public bool HasItching { get; set; }
        public bool HasHairLoss { get; set; }
        public string ActivityLevel { get; set; } = "Bình thường";
        public string CurrentMedications { get; set; } = "Không có";
        public string PetOwnerNotes { get; set; } = string.Empty;
    }

    public class ObjectiveDto
    {
        public decimal Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public int BodyConditionScore { get; set; } = 5;
        public string Mentation { get; set; } = "Tỉnh táo";
        public string Hydration { get; set; } = "< 5% (Bình thường)";

        public SystemExamDto Eyes { get; set; } = new();
        public SystemExamDto Ears { get; set; } = new();
        public SystemExamDto Nose { get; set; } = new();
        public SystemExamDto Mouth { get; set; } = new();
        public SystemExamDto SkinCoat { get; set; } = new();
        public SystemExamDto Gastrointestinal { get; set; } = new();
        public SystemExamDto Respiratory { get; set; } = new();
    }

    public class SystemExamDto
    {
        public bool IsNormal { get; set; } = true;
        public string? Note { get; set; }
    }

    public class AssessmentDto
    {
        public string TentativeDiagnosis { get; set; } = string.Empty;
        public string DefinitiveDiagnosis { get; set; } = string.Empty;
        public string DifferentialDiagnosis { get; set; } = string.Empty;
        public string DiseaseSeverity { get; set; } = "Nhẹ";
        public string Prognosis { get; set; } = "Tốt";
    }

    public class PlanDto
    {
        public List<string> TreatmentDirections { get; set; } = new();
        public string CareInstructions { get; set; } = string.Empty;
        public DateTime? FollowUpDate { get; set; }

        // Auto-create Follow-up appointment
        public bool CreateFollowUpAppointment { get; set; }
        public string? FollowUpType { get; set; } // Normal, FollowUp, Revaccination
        public string? FollowUpNote { get; set; }
        public long? FollowUpServiceId { get; set; }
        public Guid? FollowUpDoctorId { get; set; }

        public List<PrescriptionLineDto> Prescriptions { get; set; } = new();
    }
}
