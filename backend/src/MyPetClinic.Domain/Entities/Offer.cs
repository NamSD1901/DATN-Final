using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Offer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = "FIXED_AMOUNT"; // FIXED_AMOUNT, PERCENTAGE
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public int? TotalQuantity { get; set; }
        public int UsedQuantity { get; set; } = 0;
        public int UsageLimitPerUser { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "ACTIVE"; // DRAFT, ACTIVE, EXPIRED, LOCKED
        public bool IsPublic { get; set; } = true;
        
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public uint RowVersion { get; set; } // For Optimistic Concurrency (PostgreSQL xmin)

        public User? Creator { get; set; }
        public ICollection<OfferService> OfferServices { get; set; } = new List<OfferService>();
        public ICollection<OfferUsageLog> OfferUsageLogs { get; set; } = new List<OfferUsageLog>();
    }
}
