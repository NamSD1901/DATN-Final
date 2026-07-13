using System;

namespace MyPetClinic.Domain.Entities
{
    public class ScheduleException
    {
        public long Id { get; set; }
        public Guid DoctorId { get; set; }
        public string Type { get; set; } = "TimeOff"; // TimeOff, ShiftSwap, Overtime
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? SubstituteDoctorId { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Approved"; // Pending, Approved, Rejected

        public User? Doctor { get; set; }
        public User? SubstituteDoctor { get; set; }
    }
}
