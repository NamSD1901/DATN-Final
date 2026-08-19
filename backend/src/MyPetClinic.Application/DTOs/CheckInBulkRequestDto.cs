using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class CheckInItemDto
    {
        public long AppointmentId { get; set; }
        public decimal? CurrentWeight { get; set; }
    }

    public class CheckInBulkRequestDto
    {
        public string? QrToken { get; set; }
        public List<CheckInItemDto> Items { get; set; } = new List<CheckInItemDto>();
    }
}
