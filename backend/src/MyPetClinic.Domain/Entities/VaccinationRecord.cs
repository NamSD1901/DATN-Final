using System;

namespace MyPetClinic.Domain.Entities
{
    public class VaccinationRecord
    {
        public long Id { get; set; }
        public long PetId { get; set; }
        public long VaccineId { get; set; }
        public long? AppointmentId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime InjectionDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string? ReactionNote { get; set; }

        public Pet? Pet { get; set; }
        public Vaccine? Vaccine { get; set; }
        public Appointment? Appointment { get; set; }
        public User? Doctor { get; set; }
    }
}
