using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/medicines")]
    public class MedicinesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicinesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedicines()
        {
            var medicines = await _unitOfWork.Medicines.GetAllAsync();
            var result = medicines.Select(m => new
            {
                id = m.Id,
                name = m.Name,
                unit = m.Unit,
                stockQuantity = m.StockQuantity,
                sellPrice = m.SellPrice
            });
            return Ok(result);
        }

        [HttpGet("{id}/stock")]
        public async Task<IActionResult> GetMedicineStock(long id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(id);
            if (medicine == null)
            {
                return NotFound(new { message = "Không tìm thấy thuốc." });
            }

            return Ok(new
            {
                id = medicine.Id,
                name = medicine.Name,
                quantityInStock = medicine.StockQuantity,
                unit = medicine.Unit,
                sellPrice = medicine.SellPrice
            });
        }
    }
}
