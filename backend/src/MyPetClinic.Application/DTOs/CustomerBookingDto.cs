using System;

namespace MyPetClinic.Application.DTOs
{
    /// <summary>
    /// DTO cho khách hàng tự đặt lịch hẹn.
    /// </summary>
    public class CustomerBookingDto
    {
        public long PetId { get; set; }
        public Guid? DoctorId { get; set; }
        public long ServiceId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Symptom { get; set; }
        public string? Note { get; set; }
        public long? VaccineId { get; set; }
        
        /// <summary>
        /// Đánh dấu xem đây có phải trường hợp cấp cứu không
        /// </summary>
        public bool IsEmergency { get; set; } = false;
    }
}
