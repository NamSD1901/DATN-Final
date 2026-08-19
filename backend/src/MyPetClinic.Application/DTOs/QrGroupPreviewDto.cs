using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class QrGroupPreviewDto
    {
        public string QrToken { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerPhone { get; set; } = null!;
        public bool HasGlobalError { get; set; }
        public string? GlobalErrorMessage { get; set; }
        public List<AppointmentPreviewItemDto> Appointments { get; set; } = new List<AppointmentPreviewItemDto>();
    }
}
