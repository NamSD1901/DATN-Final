using System;

namespace MyPetClinic.Application.DTOs
{
    public class CheckInRequestDto
    {
        public long? AppointmentId { get; set; }
        public string? QrToken { get; set; }
        public decimal? CurrentWeight { get; set; }
        public bool IsEmergency { get; set; }
    }

}
