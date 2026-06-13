using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class CreatePetDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thú cưng")]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn giống loài")]
        public string? Species { get; set; }
        
        public string? Breed { get; set; }
        
        public short? Gender { get; set; }
        
        public DateTime? BirthDate { get; set; }
        
        public decimal? Weight { get; set; }
        
        public string? Color { get; set; }
        
        public string? BloodType { get; set; }
        
        public bool Sterilized { get; set; }
        
        public string? MicrochipCode { get; set; }
        
        public string? AllergyNote { get; set; }
        
        public string? Avatar { get; set; }
    }
}
