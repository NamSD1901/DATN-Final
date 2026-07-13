using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class CreateScheduleExceptionDto
    {
        [Required]
        public Guid DoctorId { get; set; }
        [Required]
        public string Type { get; set; } = string.Empty; // "TimeOff" or "ShiftSwap"
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        public Guid? SubstituteDoctorId { get; set; }
        public string? Reason { get; set; }
    }

    public class ScheduleExceptionDto
    {
        public long Id { get; set; }
        public Guid DoctorId { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? SubstituteDoctorId { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty; // "Pending", "Approved", "Rejected"
    }

    public class ApproveScheduleExceptionDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // "Approved" or "Rejected"
    }
}
