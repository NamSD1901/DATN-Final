using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class ScheduleProfileShiftDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsDayOff { get; set; }
    }

    public class CreateScheduleProfileDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<ScheduleProfileShiftDto> Shifts { get; set; } = new();
    }

    public class UpdateScheduleProfileDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<ScheduleProfileShiftDto> Shifts { get; set; } = new();
    }

    public class ScheduleProfileDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public List<ScheduleProfileShiftDto> Shifts { get; set; } = new();
    }

    public class AssignProfileDto
    {
        [Required]
        public long ProfileId { get; set; }
        [Required]
        public List<Guid> DoctorIds { get; set; } = new();
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
