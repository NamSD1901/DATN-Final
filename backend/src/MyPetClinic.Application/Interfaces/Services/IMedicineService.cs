using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IMedicineService
    {
        Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync();
        Task<MedicineDto?> GetMedicineStockAsync(long id);
    }
}
