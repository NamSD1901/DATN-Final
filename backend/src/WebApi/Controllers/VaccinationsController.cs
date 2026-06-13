using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Doctor,Admin")]
    [ApiController]
    [Route("api/vaccinations")]
    public class VaccinationsController : ControllerBase
    {
        private readonly IVaccinationService _vaccinationService;

        public VaccinationsController(IVaccinationService vaccinationService)
        {
            _vaccinationService = vaccinationService;
        }

        [HttpPost]
        public async Task<IActionResult> RecordVaccination([FromBody] RecordVaccinationRequest req)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!Guid.TryParse(userIdStr, out var doctorId))
            {
                return BadRequest(new { message = "DoctorId không hợp lệ." });
            }

            try
            {
                var recordId = await _vaccinationService.RecordVaccinationAsync(req.PetId, req.AppointmentId, req.VaccineId, doctorId, req.Notes);
                return Ok(new { success = true, recordId });
            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                return Ok(history);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
    }

    public class RecordVaccinationRequest
    {
        public long PetId { get; set; }
        public long AppointmentId { get; set; }
        public long VaccineId { get; set; }
        public string? Notes { get; set; }
    }
}
