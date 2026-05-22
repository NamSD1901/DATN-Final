using System;

namespace MyPetClinic.Domain.Entities
{
    public class DoctorSchedule
    {
        public long Id { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? MaxAppointments { get; set; } = 10;
        public bool IsAvailable { get; set; } = true;

        public User? Doctor { get; set; }
    }
}
