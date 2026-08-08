using System;

namespace MyPetClinic.Domain.Entities
{
    public class UserOffer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid OfferId { get; set; }
        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; } = false;

        public User? User { get; set; }
        public Offer? Offer { get; set; }
    }
}
