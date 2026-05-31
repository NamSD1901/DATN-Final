using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class WalkInRequestDto
    {
        // Customer Info
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        public string Phone { get; set; } = null!;
        
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc.")]
        public string FullName { get; set; } = null!;

        // Pet Info
        [Required(ErrorMessage = "Tên thú cưng là bắt buộc.")]
        public string PetName { get; set; } = null!;
        public string? Species { get; set; }
        public string? Breed { get; set; }
        public decimal? Weight { get; set; }
        public short? Gender { get; set; } // 1: Đực, 2: Cái

        // Appointment Info
        public long ServiceId { get; set; }
        public Guid DoctorId { get; set; }
        public string? Symptom { get; set; }
        public bool IsEmergency { get; set; }
    }
}
