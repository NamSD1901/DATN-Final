using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IMedicineRepository
    {
        Task<IEnumerable<Medicine>> GetMedicinesWithStockAsync();
        Task<Medicine?> GetMedicineWithBatchesAsync(long id);
        Task<Medicine?> GetMedicineByCodeAsync(string code);
    }
}
