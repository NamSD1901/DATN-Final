using System;

namespace MyPetClinic.Application.DTOs
{
    /// <summary>
    /// Request DTO cho API check-in bằng mã QR.
    /// </summary>
    public class AppointmentCheckInRequestDto
    {
        public string QrToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO để cập nhật trạng thái lịch hẹn (confirmed, cancelled, completed...).
    /// </summary>
    public class AppointmentUpdateStatusRequestDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request DTO để dời lịch hẹn sang thời gian mới.
    /// </summary>
    public class AppointmentRescheduleRequestDto
    {
        public DateTime NewStart { get; set; }
        public bool Force { get; set; } = false;
    }

    /// <summary>
    /// Request DTO để cập nhật bác sĩ phụ trách lịch hẹn.
    /// </summary>
    public class AppointmentUpdateDoctorRequestDto
    {
        public Guid DoctorId { get; set; }
        public bool Force { get; set; } = false;
    }
}
