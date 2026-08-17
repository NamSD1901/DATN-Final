using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class VaccinationSoapRequestDto : IValidatableObject
    {
        // S - Subjective
        [Required(ErrorMessage = "Vui lòng chọn lý do tiêm")]
        public string ReasonForVisit { get; set; } = string.Empty;
        
        public string? PreviousVaccineHistory { get; set; }
        
        public bool IsAllergic { get; set; }
        public string? AllergyDetails { get; set; }
        
        public bool HasPreviousReaction { get; set; }
        public string? PreviousReactionDetails { get; set; }
        
        public bool IsUnderTreatment { get; set; }
        public string? TreatmentDetails { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn tình trạng ăn uống")]
        public string EatingStatus { get; set; } = string.Empty;
        
        public bool HasVomitingOrDiarrhea { get; set; }
        public bool HasCoughOrSneeze { get; set; }
        
        [MaxLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? OwnerNotes { get; set; }

        // O - Objective
        [Range(0.1, 150.0, ErrorMessage = "Cân nặng phải > 0 và <= 150 kg")] // VR01
        public decimal Weight { get; set; }
        
        [Range(30.0, 45.0, ErrorMessage = "Nhiệt độ không hợp lệ (30 - 45°C)")] // VR02
        public decimal? Temperature { get; set; }
        
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn tình trạng tinh thần")]
        public string MentalStatus { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng chọn tình trạng niêm mạc")]
        public string MucosaStatus { get; set; } = string.Empty;
        
        public string? EyeNoseEarStatus { get; set; }
        public string? LymphNodeStatus { get; set; }
        public int? DehydrationPercent { get; set; }
        public List<string> Attachments { get; set; } = new();
        
        // Vaccine info
        public long? VaccineId { get; set; }
        public long? VaccineBatchId { get; set; }
        
        [Range(0.1, 100.0, ErrorMessage = "Liều lượng phải > 0")] // VR05
        public decimal? Dose { get; set; }
        
        public string? Route { get; set; }
        public string? InjectionSite { get; set; }

        // A - Assessment
        [Required(ErrorMessage = "Vui lòng chọn kết luận lâm sàng")]
        public string ClinicalAssessment { get; set; } = string.Empty;
        
        public string? DoctorRemarks { get; set; }

        // P - Plan
        public DateTime? NextDueDate { get; set; }
        public string? FollowUpInstructions { get; set; }
        public string? ReactionNote { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // VR03: Vaccine must be selected if Assessment is Eligible
            if (ClinicalAssessment == "Đủ điều kiện" && (!VaccineId.HasValue || VaccineId.Value <= 0))
            {
                yield return new ValidationResult("Vui lòng chọn vaccine sử dụng", new[] { nameof(VaccineId) });
            }

            // VR04: Next Due Date must be in the future
            if (NextDueDate.HasValue && NextDueDate.Value.Date <= DateTime.UtcNow.Date)
            {
                yield return new ValidationResult("Ngày nhắc lại không hợp lệ", new[] { nameof(NextDueDate) });
            }

            // VR06: Defer notes must be > 10 chars
            if (ClinicalAssessment == "Hoãn tiêm" && (string.IsNullOrWhiteSpace(DoctorRemarks) || DoctorRemarks.Length < 10))
            {
                yield return new ValidationResult("Ghi rõ lý do hoãn tiêm (ít nhất 10 ký tự)", new[] { nameof(DoctorRemarks) });
            }
            
            if (IsAllergic && string.IsNullOrWhiteSpace(AllergyDetails))
            {
                yield return new ValidationResult("Vui lòng nhập chi tiết tình trạng dị ứng", new[] { nameof(AllergyDetails) });
            }
            
            if (HasPreviousReaction && string.IsNullOrWhiteSpace(PreviousReactionDetails))
            {
                yield return new ValidationResult("Vui lòng nhập chi tiết phản ứng tiêm trước đó", new[] { nameof(PreviousReactionDetails) });
            }
        }
    }
}
