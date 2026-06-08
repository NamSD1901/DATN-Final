using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class CustomerCreateDto
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = null!;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public short? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public List<PetCreateDto> Pets { get; set; } = new List<PetCreateDto>();
    }
}
