using System;

namespace MyPetClinic.Domain.Entities
{
    public class OfferService
    {
        public Guid OfferId { get; set; }
        public long ServiceId { get; set; }

        public Offer? Offer { get; set; }
        public Service? Service { get; set; }
    }
}
