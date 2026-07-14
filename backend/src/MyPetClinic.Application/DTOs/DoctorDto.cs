using System;

namespace MyPetClinic.Application.DTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
