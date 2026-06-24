using System;
using System.Collections.Generic;

namespace MyPetClinic.Domain.Entities
{
    public class Pet
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
        public bool? Sterilized { get; set; } = false;
        public string? MicrochipCode { get; set; }
        public string? AllergyNote { get; set; }
        public string? Avatar { get; set; }
        public string? ChronicDisease { get; set; }
        public string? CurrentDiet { get; set; }
        public bool IsDeceased { get; set; } = false;
        public bool IsAggressive { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }

        public Customer? Customer { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    }
}
