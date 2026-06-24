using System;

namespace MyPetClinic.Domain.Entities
{
    public class Review
    {
        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public long AppointmentId { get; set; }
        public short Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Customer? Customer { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
