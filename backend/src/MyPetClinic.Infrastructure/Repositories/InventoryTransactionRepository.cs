using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class InventoryTransactionRepository : IInventoryTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByMedicineAsync(long medicineId)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Batch)
                .Where(t => t.MedicineId == medicineId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionsByBatchAsync(long batchId)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Medicine)
                .Where(t => t.BatchId == batchId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
