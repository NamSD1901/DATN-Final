using System;

namespace MyPetClinic.Domain.Entities
{
    public class Appointment
    {
        public long Id { get; set; }
        public long PetId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DoctorId { get; set; }
        public long ServiceId { get; set; }
        public long? VaccineId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Status { get; set; } = "pending";
        public string? Symptom { get; set; }
        public string? Note { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public bool IsWalkIn { get; set; } = false;
        public bool IsEmergency { get; set; } = false;
        public int QueueNumber { get; set; } = 0;
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? QrToken { get; set; }

        public Pet? Pet { get; set; }
        public Customer? Customer { get; set; }
        public User? Doctor { get; set; }
        public Service? Service { get; set; }
        public Vaccine? Vaccine { get; set; }
        public User? Creator { get; set; }

        public MedicalRecord? MedicalRecord { get; set; }
        public VaccinationRecord? VaccinationRecord { get; set; }
        public Invoice? Invoice { get; set; }
        public Review? Review { get; set; }
    }
}
