using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ================= USER MANAGEMENT =================
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _adminService.GetUsersAsync();
            return Ok(result);
        }

        // ================= SERVICE MANAGEMENT =================
        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _adminService.GetServicesAsync();
            return Ok(services);
        }

        [HttpPost("services")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var service = await _adminService.CreateServiceAsync(dto, currentUserId!);
            return Ok(new { success = true, service });
        }

        [HttpPut("services/{id}")]
        public async Task<IActionResult> UpdateService(long id, [FromBody] CreateServiceDto dto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var service = await _adminService.UpdateServiceAsync(id, dto, currentUserId!);
                return Ok(new { success = true, service });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("services/{id}")]
        public async Task<IActionResult> DeleteService(long id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _adminService.DeleteServiceAsync(id, currentUserId!);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ================= SERVICE CATEGORY MANAGEMENT =================
        [HttpGet("services/categories")]
        public async Task<IActionResult> GetServiceCategories()
        {
            var categories = await _adminService.GetServiceCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost("services/categories")]
        public async Task<IActionResult> CreateServiceCategory([FromBody] CreateServiceCategoryDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var category = await _adminService.CreateServiceCategoryAsync(dto, currentUserId!);
            return Ok(new { success = true, category });
        }

        [HttpPut("services/categories/{id}")]
        public async Task<IActionResult> UpdateServiceCategory(long id, [FromBody] CreateServiceCategoryDto dto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var category = await _adminService.UpdateServiceCategoryAsync(id, dto, currentUserId!);
                return Ok(new { success = true, category });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("services/categories/{id}")]
        public async Task<IActionResult> DeleteServiceCategory(long id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _adminService.DeleteServiceCategoryAsync(id, currentUserId!);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ================= MEDICINES MANAGEMENT =================
        [HttpGet("medicines")]
        public async Task<IActionResult> GetMedicines()
        {
            var medicines = await _adminService.GetMedicinesAsync();
            return Ok(medicines);
        }

        [HttpGet("medicines/warnings")]
        public async Task<IActionResult> GetMedicineWarnings()
        {
            var warnings = await _adminService.GetMedicineWarningsAsync();
            return Ok(warnings);
        }

        [HttpPost("medicines")]
        public async Task<IActionResult> CreateMedicine([FromBody] CreateMedicineDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var medicine = await _adminService.CreateMedicineAsync(dto, currentUserId!);
            return Ok(new { success = true, medicine });
        }

        [HttpPut("medicines/{id}")]
        public async Task<IActionResult> UpdateMedicine(long id, [FromBody] CreateMedicineDto dto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var medicine = await _adminService.UpdateMedicineAsync(id, dto, currentUserId!);
                return Ok(new { success = true, medicine });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("medicines/{id}")]
        public async Task<IActionResult> DeleteMedicine(long id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _adminService.DeleteMedicineAsync(id, currentUserId!);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("medicines/batches/{batchId}")]
        public async Task<IActionResult> DeleteMedicineBatch(long batchId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _adminService.DeleteMedicineBatchAsync(batchId, currentUserId!);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        // ================= VACCINES MANAGEMENT =================
        [HttpGet("vaccines")]
        public async Task<IActionResult> GetVaccines()
        {
            var vaccines = await _adminService.GetVaccinesAsync();
            return Ok(vaccines);
        }

        [HttpPost("vaccines")]
        public async Task<IActionResult> CreateVaccine([FromBody] CreateVaccineDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vaccine = await _adminService.CreateVaccineAsync(dto, currentUserId!);
            return Ok(new { success = true, vaccine });
        }

        [HttpPut("vaccines/{id}")]
        public async Task<IActionResult> UpdateVaccine(long id, [FromBody] CreateVaccineDto dto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vaccine = await _adminService.UpdateVaccineAsync(id, dto, currentUserId!);
                return Ok(new { success = true, vaccine });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("vaccines/{id}")]
        public async Task<IActionResult> DeleteVaccine(long id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _adminService.DeleteVaccineAsync(id, currentUserId!);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("vaccines/{id}/batches")]
        public async Task<IActionResult> CreateVaccineBatch(long id, [FromBody] CreateVaccineBatchDto dto)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var batch = await _adminService.CreateVaccineBatchAsync(id, dto, currentUserId!);
                return Ok(new { success = true, batch });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("vaccines/batches/{batchId}")]
        public async Task<IActionResult> DeleteVaccineBatch(long batchId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _adminService.DeleteVaccineBatchAsync(batchId, currentUserId!);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }



        // ================= SLOT CONFIGURATION =================
        [HttpGet("slots/config")]
        public IActionResult GetSlotConfig()
        {
            var config = _adminService.GetSlotConfig();
            return Ok(config);
        }

        [HttpPut("slots/config")]
        public async Task<IActionResult> UpdateSlotConfig([FromBody] SlotConfigModel config)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _adminService.UpdateSlotConfigAsync(config, currentUserId!);
            return Ok(new { success = true, config });
        }
    }
}
