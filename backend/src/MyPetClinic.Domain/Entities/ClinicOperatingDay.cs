using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class ClinicOperatingDay
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; } // 0: Sunday, 1: Monday, ...
        public bool IsOpen { get; set; } = true;
        
        public ICollection<ClinicOperatingShift> Shifts { get; set; } = new List<ClinicOperatingShift>();
    }
}
