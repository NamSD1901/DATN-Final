using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IPetService
    {
        Task<IEnumerable<PetDto>> GetMyPetsAsync(Guid ownerId);
        Task<PetDto?> GetPetByIdAsync(long id, Guid ownerId);
        Task<PetDto> AddPetAsync(CreatePetDto dto, Guid ownerId);
        Task UpdatePetAsync(UpdatePetDto dto, Guid ownerId);
        Task DeletePetAsync(long id, Guid ownerId);
    }
}
