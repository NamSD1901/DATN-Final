using System;

namespace MyPetClinic.Application.DTOs
{
    public class ServiceDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal? Price { get; set; }
    }
}
