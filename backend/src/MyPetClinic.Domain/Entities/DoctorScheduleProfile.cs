using System;

namespace MyPetClinic.Domain.Entities
{
    public class DoctorScheduleProfile
    {
        public long Id { get; set; }
        public Guid DoctorId { get; set; }
        public long ProfileId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }

        public User? Doctor { get; set; }
        public ScheduleProfile? Profile { get; set; }
    }
}
