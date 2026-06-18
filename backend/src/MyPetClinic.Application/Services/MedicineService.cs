using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Application.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            return medicines.Select(m => new MedicineDto
            {
                Id = m.Id,
                Name = m.Name,
                Unit = m.Unit ?? "",
                StockQuantity = m.StockQuantity,
                SellPrice = m.SellPrice ?? 0
            });
        }

        public async Task<MedicineDto?> GetMedicineStockAsync(long id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id);
            if (medicine == null)
            {
                return null;
            }

            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                StockQuantity = medicine.StockQuantity,
                Unit = medicine.Unit ?? "",
                SellPrice = medicine.SellPrice ?? 0
            };
        }
    }
}
