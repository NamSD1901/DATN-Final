using System;

namespace MyPetClinic.Application.DTOs
{
    public class CalendarEventDto
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Start { get; set; } = null!;
        public string End { get; set; } = null!;
        public string Color { get; set; } = null!;
        public bool AllDay { get; set; } = false;
        
        // Custom extended properties for FullCalendar
        public object? ExtendedProps { get; set; }
    }
}
