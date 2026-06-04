using System;

namespace MyPetClinic.Application.DTOs
{
    public class AppointmentStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Waiting { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}
