using System;

namespace MyPetClinic.Domain.Entities
{
    public class ScheduleProfileShift
    {
        public long Id { get; set; }
        public long ProfileId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsDayOff { get; set; } = false;

        public ScheduleProfile? Profile { get; set; }
    }
}
