namespace MyPetClinic.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public long RoleId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PasswordHash { get; set; }
        public string? Avatar { get; set; }
        public short? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public Role? Role { get; set; }
        public EmployeeProfile? EmployeeProfile { get; set; }
        public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
        
        public Guid? CustomerId { get; set; }
        public Customer? CustomerProfile { get; set; }
    }
}
