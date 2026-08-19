using System;

namespace MyPetClinic.Application.DTOs
{
    public class AppointmentPreviewItemDto
    {
        public long AppointmentId { get; set; }
        public string PetName { get; set; } = null!;
        public string? PetSpecies { get; set; }
        public double? PetWeight { get; set; }
        public string? DoctorName { get; set; }
        public string? ServiceName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
