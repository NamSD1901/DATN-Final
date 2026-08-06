using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự.")]
        public string? FullName { get; set; }

        [RegularExpression(@"^\+[1-9]\d{1,14}$", ErrorMessage = "Số điện thoại không hợp lệ (Phải bắt đầu bằng dấu + và theo chuẩn quốc tế).")]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public short? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}
