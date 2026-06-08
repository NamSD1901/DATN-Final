using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using System.Collections.Generic;

namespace MyPetClinic.Web.Models
{
    public class CustomerProfileViewModel
    {
        public User Customer { get; set; } = new User();
        public IEnumerable<Pet> Pets { get; set; } = new List<Pet>();
        public IEnumerable<AppointmentDetailDto> Appointments { get; set; } = new List<AppointmentDetailDto>();
        public IEnumerable<DoctorDto> ActiveDoctors { get; set; } = new List<DoctorDto>();
        public IEnumerable<ServiceDto> Services { get; set; } = new List<ServiceDto>();

        // Insights Helpers
        public decimal TotalSpent { get; set; }
        public int TotalVisits { get; set; }
        public int NoShowCount { get; set; }
        public decimal UnpaidBalance { get; set; }
    }
}
