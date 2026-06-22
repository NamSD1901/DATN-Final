using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IMedicineBatchRepository
    {
        Task<IEnumerable<MedicineBatch>> GetAvailableBatchesAsync(long medicineId);
        Task<MedicineBatch?> GetBatchWithMedicineAsync(long id);
        Task<MedicineBatch?> GetBatchByNumberAsync(string batchNumber);
    }
}
