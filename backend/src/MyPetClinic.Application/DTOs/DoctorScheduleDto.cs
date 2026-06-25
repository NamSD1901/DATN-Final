using System;

namespace MyPetClinic.Application.DTOs
{
    public class DoctorScheduleDto
    {
        public long Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? MaxAppointments { get; set; }
        public bool IsAvailable { get; set; }
    }
}
