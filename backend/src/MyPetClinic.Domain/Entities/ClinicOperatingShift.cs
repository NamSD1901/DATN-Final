using System;

namespace MyPetClinic.Domain.Entities
{
    public class ClinicOperatingShift
    {
        public int Id { get; set; }
        public int ClinicOperatingDayId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public ClinicOperatingDay? ClinicOperatingDay { get; set; }
    }
}
