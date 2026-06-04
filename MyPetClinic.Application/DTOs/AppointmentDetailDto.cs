using System;

namespace MyPetClinic.Application.DTOs
{
    public class AppointmentDetailDto
    {
        public long Id { get; set; }
        public long PetId { get; set; }
        public string? PetName { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public decimal? Weight { get; set; }
        public bool IsAggressive { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public long ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string AppointmentDate { get; set; } = string.Empty;
        public string? Symptom { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public string? QrToken { get; set; }
    }
}
