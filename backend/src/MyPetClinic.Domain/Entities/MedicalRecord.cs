using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class MedicalRecord
    {
        public long Id { get; set; }
        public long AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Temperature { get; set; }
        public int? HeartRate { get; set; }
        public string? Symptoms { get; set; }
        public string? Diagnosis { get; set; }
        public string? TreatmentPlan { get; set; }
        public string? Note { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Appointment? Appointment { get; set; }
        public User? Doctor { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
