using System;

namespace MyPetClinic.Domain.Entities
{
    public class OfferUsageLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OfferId { get; set; }
        public Guid UserId { get; set; }
        public long? InvoiceId { get; set; }
        
        public decimal DiscountApplied { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string Status { get; set; } = "APPLIED"; // APPLIED, REVERTED

        public Offer? Offer { get; set; }
        public User? User { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
