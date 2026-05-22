using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Service
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ServiceCategory? Category { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
