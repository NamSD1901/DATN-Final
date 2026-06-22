using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IMedicineService
    {
        Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync();
        Task<MedicineDetailDto?> GetMedicineDetailsAsync(long id);
        Task<MedicineDto?> GetMedicineStockAsync(long id);
        Task<IEnumerable<MedicineDto>> GetLowStockMedicinesAsync();
        
        Task ImportMedicineAsync(ImportMedicineDto dto, Guid userId);
        Task ExportMedicineAsync(ExportMedicineDto dto, Guid userId);
        Task AdjustMedicineStockAsync(AdjustMedicineDto dto, Guid userId);
        Task AuditMedicineBatchAsync(AuditMedicineDto dto, Guid userId);
        Task<IEnumerable<InventoryTransactionDto>> GetMedicineTransactionsAsync(long medicineId);
        Task<IEnumerable<ExpiringMedicineDto>> GetExpiringMedicinesAsync(int daysThreshold);
    }
}
