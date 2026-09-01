using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IMedicineBatchRepository
    {
        Task<IEnumerable<MedicineBatch>> GetAvailableBatchesAsync(long medicineId, int durationDays = 0);
        Task<MedicineBatch?> GetBatchWithMedicineAsync(long id);
        Task<MedicineBatch?> GetBatchByNumberAndMedicineAsync(string batchNumber, long medicineId);
    }
}
