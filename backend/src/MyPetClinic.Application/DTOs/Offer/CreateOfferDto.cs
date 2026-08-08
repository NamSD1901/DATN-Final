using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs.Offer
{
    public class CreateOfferDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string DiscountType { get; set; } = "FIXED_AMOUNT";
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public int? TotalQuantity { get; set; }
        public int UsageLimitPerUser { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublic { get; set; } = true;
        
        public List<long> AppliedServiceIds { get; set; } = new();
    }
}
