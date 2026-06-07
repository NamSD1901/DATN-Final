using System.Collections.Generic;
using System.Security.Claims;

namespace MyPetClinic.Application.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Email { get; set; }
        public List<Claim>? Claims { get; set; }
        public bool RequiresOtp { get; set; }
    }
}
