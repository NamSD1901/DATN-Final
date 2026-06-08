using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        Task<Pet?> GetPetByIdAsync(long id);
        Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(Guid ownerId);
        Task CreatePetAsync(Pet pet);
        Task CreatePetsAsync(IEnumerable<Pet> pets);
        Task UpdatePetAsync(Pet pet);
        Task SoftDeletePetAsync(long id);
        Task SaveChangesAsync();
    }
}
