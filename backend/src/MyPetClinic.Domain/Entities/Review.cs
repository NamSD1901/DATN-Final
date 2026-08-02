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
        public DateTime? DeletedAt { get; set; }

        public bool IsVerified { get; set; }
        public string? ClinicReply { get; set; }
        public DateTime? RepliedAt { get; set; }
        public int HelpfulCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public string? ImageUrls { get; set; } // Comma-separated or JSON list of URLs

        public Customer? Customer { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
