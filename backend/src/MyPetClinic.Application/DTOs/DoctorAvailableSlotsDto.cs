using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class DoctorAvailableSlotsDto
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public List<DateTime> AvailableSlots { get; set; } = new();
    }
}
