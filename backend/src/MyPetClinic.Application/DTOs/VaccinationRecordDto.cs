using System;

namespace MyPetClinic.Application.DTOs
{
    public class VaccinationRecordDto
    {
        public long RecordId { get; set; }
        public long PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public long VaccineId { get; set; }
        public string VaccineName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime InjectionDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string? ReactionNote { get; set; }
    }
}
