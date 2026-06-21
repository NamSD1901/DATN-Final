using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class CreateEmployeeRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại phải là số điện thoại VN hợp lệ (10 số)")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CCCD là bắt buộc")]
        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "CCCD phải bao gồm đúng 12 chữ số")]
        public string IdentityCard { get; set; } = string.Empty;

        public short? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }

        [Required(ErrorMessage = "Chức vụ/Quyền là bắt buộc (admin, clinical_doctor, vaccination_doctor, receptionist)")]
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateEmployeeRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        public short? Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }

        public bool IsResigned { get; set; }
    }

    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public short? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // EmployeeProfile specific fields
        public string IdentityCard { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public bool IsResigned { get; set; }
    }
}
