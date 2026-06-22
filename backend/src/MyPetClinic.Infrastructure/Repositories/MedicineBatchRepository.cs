using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class MedicineBatchRepository : IMedicineBatchRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicineBatchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicineBatch>> GetAvailableBatchesAsync(long medicineId)
        {
            return await _context.MedicineBatches
                .Where(b => b.MedicineId == medicineId && b.CurrentQuantity > 0)
                .OrderBy(b => b.ExpiryDate) // FEFO order
                .ToListAsync();
        }

        public async Task<MedicineBatch?> GetBatchWithMedicineAsync(long id)
        {
            return await _context.MedicineBatches
                .Include(b => b.Medicine)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<MedicineBatch?> GetBatchByNumberAsync(string batchNumber)
        {
            return await _context.MedicineBatches
                .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
        }
    }
}
