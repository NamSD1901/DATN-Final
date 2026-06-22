using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class ClinicOperatingDayDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsOpen { get; set; }
        public List<ClinicOperatingShiftDto> Shifts { get; set; } = new();
    }

    public class ClinicOperatingShiftDto
    {
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
    }

    public class UpdateOperatingHoursDto
    {
        public List<ClinicOperatingDayDto> Days { get; set; } = new();
    }

    public class ClinicHolidayDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateHolidayDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
    
    public class UpdateHolidayDto : CreateHolidayDto
    {
        public bool IsActive { get; set; }
    }
}
