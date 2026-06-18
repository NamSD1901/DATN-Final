using System;
using System.Collections.Generic;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.DTOs
{
    public class UpdateQueueStatusRequest
    {
        public string Status { get; set; } = null!;
    }

    public class UpdateEmergencyCustomerRequest
    {
        public Guid CustomerId { get; set; }
        public long PetId { get; set; }
    }

    public class CustomerDashboardDetailDto
    {
        public UserProfileDto Customer { get; set; } = null!;
        public IEnumerable<PetDto> Pets { get; set; } = new List<PetDto>();
        public IEnumerable<DoctorDto> ActiveDoctors { get; set; } = new List<DoctorDto>();
        public IEnumerable<ServiceDto> Services { get; set; } = new List<ServiceDto>();
        public IEnumerable<AppointmentDetailDto> Appointments { get; set; } = new List<AppointmentDetailDto>();
        public int TotalVisits { get; set; }
        public decimal TotalSpent { get; set; }
        public int NoShowCount { get; set; }
        public decimal UnpaidBalance { get; set; }
    }

    public class PetDashboardDetailDto
    {
        public PetDto Pet { get; set; } = null!;
        public UserProfileDto Customer { get; set; } = null!;
        public IEnumerable<AppointmentDetailDto> Appointments { get; set; } = new List<AppointmentDetailDto>();
    }
}
