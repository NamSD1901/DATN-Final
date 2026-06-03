using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly ApplicationDbContext _context;

        public PetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Pet?> GetPetByIdAsync(long id)
        {
            return await _context.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedAt == null);
        }

        public async Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(Guid ownerId)
        {
            return await _context.Pets
                .Where(p => p.OwnerId == ownerId && p.DeletedAt == null)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task CreatePetAsync(Pet pet)
        {
            _context.Pets.Add(pet);
            await Task.CompletedTask;
        }

        public async Task CreatePetsAsync(IEnumerable<Pet> pets)
        {
            _context.Pets.AddRange(pets);
            await Task.CompletedTask;
        }

        public async Task UpdatePetAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await Task.CompletedTask;
        }

        public async Task SoftDeletePetAsync(long id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet != null)
            {
                pet.DeletedAt = DateTime.UtcNow;
                _context.Pets.Update(pet);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
