using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using System.Collections.Generic;

namespace MyPetClinic.Web.Models
{
    public class PetProfileViewModel
    {
        public Pet Pet { get; set; } = new Pet();
        public User Customer { get; set; } = new User();
        public IEnumerable<AppointmentDetailDto> Appointments { get; set; } = new List<AppointmentDetailDto>();
    }
}
