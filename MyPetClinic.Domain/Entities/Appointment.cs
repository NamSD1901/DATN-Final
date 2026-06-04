using System;

namespace MyPetClinic.Domain.Entities
{
    public class Appointment
    {
        public long Id { get; set; }
        public long PetId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; } = "pending";
        public string? Symptom { get; set; }
        public string? Note { get; set; }
        public string? CancellationReason { get; set; }
        public string? CancelledByRole { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Pet? Pet { get; set; }
        public User? Customer { get; set; }
        public User? Doctor { get; set; }
        public User? Creator { get; set; }

        public MedicalRecord? MedicalRecord { get; set; }
        public Invoice? Invoice { get; set; }
        public Review? Review { get; set; }
    }
}
