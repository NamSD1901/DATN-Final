using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/medicines")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedicines()
        {
            var result = await _medicineService.GetAllMedicinesAsync();
            return Ok(result);
        }

        [HttpGet("{id}/stock")]
        public async Task<IActionResult> GetMedicineStock(long id)
        {
            var medicine = await _medicineService.GetMedicineStockAsync(id);
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
