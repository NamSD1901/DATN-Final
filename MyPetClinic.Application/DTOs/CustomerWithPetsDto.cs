using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class CustomerWithPetsDto
    {
        public bool Found { get; set; }
        public Guid CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public List<PetBasicDto> Pets { get; set; } = new List<PetBasicDto>();
    }

    public class PetBasicDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public decimal? Weight { get; set; }
    }
}
