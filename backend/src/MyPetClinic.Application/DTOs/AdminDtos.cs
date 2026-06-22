using System;

namespace MyPetClinic.Application.DTOs
{
    public class UpdateRoleDto
    {
        public string NewRole { get; set; } = string.Empty;
    }

    public class ToggleStatusDto
    {
        public bool IsActive { get; set; }
    }

    public class CreateServiceDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public long CategoryId { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
    }

    public class SlotConfigModel
    {
        public string StartTime { get; set; } = "08:00:00";
        public string EndTime { get; set; } = "17:00:00";
        public int DurationMinutes { get; set; } = 30;
        public int MaxAppointmentsPerSlot { get; set; } = 3;
    }

    public class CreateMedicineDto
    {
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SellPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }

    public class CreateScheduleDto
    {
        public Guid DoctorId { get; set; }
        public DateTime WorkDate { get; set; }
        public string StartTime { get; set; } = "08:00";
        public string EndTime { get; set; } = "12:00";
        public int MaxAppointments { get; set; } = 10;
        public bool IsAvailable { get; set; } = true;
    }

    public class DoctorScheduleDto
    {
        public long Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int MaxAppointments { get; set; }
        public bool IsAvailable { get; set; }
    }
}
