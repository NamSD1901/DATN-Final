using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetDoctorsAsync();
    }
}
