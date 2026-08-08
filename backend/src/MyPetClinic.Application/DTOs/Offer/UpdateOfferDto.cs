using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs.Offer
{
    public class UpdateOfferDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty; 
        public int? TotalQuantity { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublic { get; set; }
        
        public List<long> AppliedServiceIds { get; set; } = new();
    }
}
