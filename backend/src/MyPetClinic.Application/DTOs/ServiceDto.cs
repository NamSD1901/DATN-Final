using System;

namespace MyPetClinic.Application.DTOs
{
    public class ServiceDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public long? CategoryId { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
