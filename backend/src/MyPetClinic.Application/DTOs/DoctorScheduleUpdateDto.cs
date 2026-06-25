using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class DoctorScheduleUpdateDto
    {
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        public int? MaxAppointments { get; set; }
        
        public bool IsAvailable { get; set; }
    }
}
