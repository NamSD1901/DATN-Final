using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(Guid ownerId);
        Task<Pet?> GetByIdAsync(long id);
        Task<Pet> AddAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(Pet pet);
    }
}
