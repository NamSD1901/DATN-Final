using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/vaccinations")]
    public class VaccinationsController : ControllerBase
    {
        private readonly IVaccinationService _vaccinationService;

        public VaccinationsController(IVaccinationService vaccinationService)
        {
            _vaccinationService = vaccinationService;
        }

        [HttpGet("appointments/{appointmentId}")]
        public async Task<IActionResult> GetSoapRecord(long appointmentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var currentUserId)) return Unauthorized();

            var record = await _vaccinationService.GetSoapRecordByAppointmentAsync(appointmentId, currentUserId);
            if (record == null) return NotFound(new { message = "Không tìm thấy hồ sơ bệnh án." });

            return Ok(new { success = true, data = record });
        }

        [HttpPost("appointments/{appointmentId}")]
        [Authorize(Roles = "doctor,admin,Doctor,Admin,SystemAdmin,clinical_doctor,vaccination_doctor")]
        public async Task<IActionResult> SubmitSoapRecord(long appointmentId, [FromBody] VaccinationSoapRequestDto request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var doctorId)) return Unauthorized();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var recordId = await _vaccinationService.SubmitSoapRecordAsync(appointmentId, doctorId, request);
                return Ok(new { success = true, recordId });
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("pet/{petId}")]
        public async Task<IActionResult> GetPetVaccinationHistory(long petId)
        {
            try
            {
                var history = await _vaccinationService.GetPetVaccinationHistoryAsync(petId);
                return Ok(history); // Match format
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("vaccines")]
        [Authorize(Roles = "doctor,admin,receptionist,Doctor,Admin,Receptionist,SystemAdmin,clinical_doctor,vaccination_doctor")]
        public async Task<IActionResult> GetAvailableVaccines()
        {
            try
            {
                var vaccines = await _vaccinationService.GetAvailableVaccinesAsync();
                return Ok(new { success = true, data = vaccines });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
    }
}
