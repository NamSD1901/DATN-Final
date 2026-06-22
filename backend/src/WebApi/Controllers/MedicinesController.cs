using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "admin,doctor,receptionist")]
    [ApiController]
    [Route("api/medicines")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedicines()
        {
            var result = await _medicineService.GetAllMedicinesAsync();
            return Ok(result);
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "admin,receptionist")]
        public async Task<IActionResult> GetLowStockMedicines()
        {
            var result = await _medicineService.GetLowStockMedicinesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicineDetails(long id)
        {
            var medicine = await _medicineService.GetMedicineDetailsAsync(id);
            if (medicine == null)
            {
                return NotFound(new { message = "Không tìm thấy thuốc." });
            }
            return Ok(medicine);
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

        [HttpGet("{id}/transactions")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetMedicineTransactions(long id)
        {
            var transactions = await _medicineService.GetMedicineTransactionsAsync(id);
            return Ok(transactions);
        }

        [HttpPost("import")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ImportMedicine([FromBody] ImportMedicineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _medicineService.ImportMedicineAsync(dto, userId);
                return Ok(new { success = true, message = "Nhập kho thuốc thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi nhập kho: " + ex.Message });
            }
        }

        [HttpPost("export")]
        [Authorize(Roles = "admin,receptionist,doctor")]
        public async Task<IActionResult> ExportMedicine([FromBody] ExportMedicineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _medicineService.ExportMedicineAsync(dto, userId);
                return Ok(new { success = true, message = "Xuất kho thuốc thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xuất kho: " + ex.Message });
            }
        }

        [HttpPost("adjust")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdjustMedicineStock([FromBody] AdjustMedicineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _medicineService.AdjustMedicineStockAsync(dto, userId);
                return Ok(new { success = true, message = "Điều chỉnh tồn kho thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi điều chỉnh tồn kho: " + ex.Message });
            }
        }

        [HttpPost("audit")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AuditMedicineStock([FromBody] AuditMedicineDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _medicineService.AuditMedicineBatchAsync(dto, userId);
                return Ok(new { success = true, message = "Kiểm kê kho thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi kiểm kê kho: " + ex.Message });
            }
        }

        [HttpGet("expiring")]
        [Authorize(Roles = "admin,receptionist")]
        public async Task<IActionResult> GetExpiringMedicines([FromQuery] int days = 30)
        {
            var result = await _medicineService.GetExpiringMedicinesAsync(days);
            return Ok(result);
        }
    }
}
