using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperatingHoursController : ControllerBase
    {
        private readonly IOperatingHoursService _operatingHoursService;

        public OperatingHoursController(IOperatingHoursService operatingHoursService)
        {
            _operatingHoursService = operatingHoursService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetOperatingHours()
        {
            var result = await _operatingHoursService.GetOperatingHoursAsync();
            return Ok(new { success = true, data = result });
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOperatingHours([FromBody] UpdateOperatingHoursDto dto)
        {
            try
            {
                var result = await _operatingHoursService.UpdateOperatingHoursAsync(dto);
                return Ok(new { success = true, message = "Cập nhật giờ hoạt động thành công." });
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Conflict"))
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("holidays")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHolidays()
        {
            var result = await _operatingHoursService.GetHolidaysAsync();
            return Ok(new { success = true, data = result });
        }

        [HttpPost("holidays")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid userId = string.IsNullOrEmpty(userIdStr) ? Guid.Empty : Guid.Parse(userIdStr);

                var result = await _operatingHoursService.CreateHolidayAsync(dto, userId);
                return Ok(new { success = true, message = "Thêm ngày nghỉ thành công.", data = result });
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Conflict"))
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("holidays/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHoliday(int id, [FromBody] UpdateHolidayDto dto)
        {
            try
            {
                var success = await _operatingHoursService.UpdateHolidayAsync(id, dto);
                if (!success) return NotFound(new { success = false, message = "Không tìm thấy ngày nghỉ." });
                
                return Ok(new { success = true, message = "Cập nhật ngày nghỉ thành công." });
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Conflict"))
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("holidays/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var success = await _operatingHoursService.DeleteHolidayAsync(id);
            if (!success) return NotFound(new { success = false, message = "Không tìm thấy ngày nghỉ." });
            
            return Ok(new { success = true, message = "Xóa ngày nghỉ thành công." });
        }
    }
}
