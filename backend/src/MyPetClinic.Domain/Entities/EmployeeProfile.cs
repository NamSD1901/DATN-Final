using System;

namespace MyPetClinic.Domain.Entities
{
    public class EmployeeProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? IdentityCard { get; set; }
        public string Position { get; set; } = string.Empty;
        public bool IsResigned { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }
    }
}
