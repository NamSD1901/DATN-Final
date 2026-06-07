using System;

namespace MyPetClinic.Application.DTOs
{
    public class AppointmentWithNewCustomerDto
    {
        // Customer Info
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;

        // Pet Info
        public string PetName { get; set; } = null!;
        public string Species { get; set; } = null!;
        public double? PetWeight { get; set; }

        // Appointment Info
        public Guid DoctorId { get; set; }
        public long ServiceId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Symptom { get; set; }
        public string? Note { get; set; }
    }
}
