using System;
using System.ComponentModel.DataAnnotations;

namespace MyPetClinic.Application.DTOs
{
    public class AppointmentCreateDto
    {
        [Required(ErrorMessage = "Vui lòng chọn khách hàng")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thú cưng")]
        public long PetId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bác sĩ phụ trách")]
        public Guid DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn dịch vụ")]
        public long ServiceId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do khám / triệu chứng")]
        public string Symptom { get; set; } = null!;

        public string? Note { get; set; }


    }
}
