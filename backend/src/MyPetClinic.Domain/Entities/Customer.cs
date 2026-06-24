namespace MyPetClinic.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string? CustomerCode { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public short? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public bool HasAccount { get; set; } = false;
        public string? Status { get; set; } = "Active";
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User? Account { get; set; }
        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }
}
