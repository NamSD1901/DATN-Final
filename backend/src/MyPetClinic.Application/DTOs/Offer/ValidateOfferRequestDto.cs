using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs.Offer
{
    public class ValidateOfferRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public List<long> ServiceIds { get; set; } = new();
        public System.Guid? CustomerId { get; set; }
    }
    
    public class ValidateOfferResponseDto
    {
        public bool IsValid { get; set; }
        public decimal DiscountAmount { get; set; }
        public string Message { get; set; } = string.Empty;
        public OfferDto? Offer { get; set; }
    }
}
