using System;
using System.Collections.Generic;

namespace MyPetClinic.Application.DTOs
{
    public class OmniSearchDto
    {
        public Guid CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public List<OmniSearchPetDto> Pets { get; set; } = new List<OmniSearchPetDto>();
    }

    public class OmniSearchPetDto
    {
        public long PetId { get; set; }
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public decimal? Weight { get; set; }
        public string? MicrochipCode { get; set; }
    }
}
