using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Medicine>> GetMedicinesWithStockAsync()
        {
            return await _context.Medicines
                .Include(m => m.Batches)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<Medicine?> GetMedicineWithBatchesAsync(long id)
        {
            return await _context.Medicines
                .Include(m => m.Batches)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Medicine?> GetMedicineByCodeAsync(string code)
        {
            return await _context.Medicines
                .FirstOrDefaultAsync(m => m.MedicineCode == code);
        }
    }
}
