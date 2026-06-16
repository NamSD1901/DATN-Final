using MyPetClinic.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<PrescriptionDto>> GetPetPrescriptionsAsync(long petId);
    }
}
