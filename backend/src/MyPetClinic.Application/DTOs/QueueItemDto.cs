using System;

namespace MyPetClinic.Application.DTOs
{
    public class QueueItemDto
    {
        public long AppointmentId { get; set; }
        public long PetId { get; set; }
        public string? PetName { get; set; }
        public string? Species { get; set; }
        public decimal? Weight { get; set; }
        
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        
        public Guid DoctorId { get; set; }
        public string? DoctorName { get; set; }
        
        public string? Status { get; set; } // Waiting, InProgress, ReadyToPay
        public string? Symptom { get; set; }
        public bool IsEmergency { get; set; }
        public bool IsWalkIn { get; set; }
        public int QueueNumber { get; set; }
        public DateTime? CheckInTime { get; set; }
        
        // Cảnh báo (Ví dụ: Hung dữ)
        public bool IsAggressive { get; set; }
    }
}
