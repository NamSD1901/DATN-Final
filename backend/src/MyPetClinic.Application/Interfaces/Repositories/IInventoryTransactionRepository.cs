using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IInventoryTransactionRepository
    {
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByMedicineAsync(long medicineId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionsByBatchAsync(long batchId);
    }
}
