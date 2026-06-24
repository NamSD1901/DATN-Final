using System;

namespace MyPetClinic.Application.DTOs
{
    public class PetDto
    {
        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public short? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public decimal? Weight { get; set; }
        public string? Color { get; set; }
        public string? BloodType { get; set; }
        public bool? Sterilized { get; set; }
        public string? MicrochipCode { get; set; }
        public string? AllergyNote { get; set; }
        public string? Avatar { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
