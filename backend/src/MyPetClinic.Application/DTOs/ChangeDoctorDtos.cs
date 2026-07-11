using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class EligibleDoctorDto
    {
        public Guid DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ExperienceMonths { get; set; }
        public int TotalScore { get; set; }
        public bool IsRecommended { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class ChangeDoctorRequestDto
    {
        public Guid NewDoctorId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool Force { get; set; } = false;
    }
}
