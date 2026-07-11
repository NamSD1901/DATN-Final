using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class DoctorScheduleCreateDto
    {
        [Required]
        public Guid DoctorId { get; set; }
        
        [Required]
        public DateTime WorkDate { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        public int? MaxAppointments { get; set; } = 10;
        
        public Guid? RecurringGroupId { get; set; }
        
        public string? Notes { get; set; }
    }
}
