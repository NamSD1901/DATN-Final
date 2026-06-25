using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "admin,Admin")]
    [ApiController]
    [Route("api/doctor-schedules")]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly IDoctorScheduleService _doctorScheduleService;

        public DoctorSchedulesController(IDoctorScheduleService doctorScheduleService)
        {
            _doctorScheduleService = doctorScheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate, 
            [FromQuery] Guid? doctorId)
        {
            var schedules = await _doctorScheduleService.GetSchedulesAsync(startDate, endDate, doctorId);
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(long id)
        {
            var schedule = await _doctorScheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Không tìm thấy lịch trực." });
            }
            return Ok(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] DoctorScheduleCreateDto dto)
        {
            try
            {
                var id = await _doctorScheduleService.CreateScheduleAsync(dto);
                return Ok(new { success = true, id });
            }
            catch (InvalidOperationException ex)
            {
                // Xung đột lịch (BR01)
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(long id, [FromBody] DoctorScheduleUpdateDto dto)
        {
            try
            {
                var success = await _doctorScheduleService.UpdateScheduleAsync(id, dto);
                if (!success)
                {
                    return NotFound(new { message = "Không tìm thấy lịch trực." });
                }
                return Ok(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                // Xung đột lịch (BR01)
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(long id)
        {
            try
            {
                var success = await _doctorScheduleService.DeleteScheduleAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Không tìm thấy lịch trực." });
                }
                return Ok(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                // Đã có lịch khám liên kết (BR04)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
