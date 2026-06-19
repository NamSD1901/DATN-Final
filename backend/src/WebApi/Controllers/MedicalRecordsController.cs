using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "doctor,admin,receptionist,Doctor,Admin,Receptionist,SystemAdmin")]
    [ApiController]
    [Route("api/medical-records")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateMedicalRecord([FromBody] CreateMedicalRecordDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!Guid.TryParse(userIdStr, out var doctorId))
            {
                return BadRequest(new { message = "DoctorId không hợp lệ." });
            }

            try
            {
                var recordId = await _medicalRecordService.CreateMedicalRecordAsync(dto, doctorId);
                return Ok(new { success = true, recordId });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpGet("pet/{petId}")]
        public async Task<IActionResult> GetPetMedicalHistory(long petId)
        {
            try
            {
                var history = await _medicalRecordService.GetPetMedicalHistoryAsync(petId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetMedicalRecordByAppointment(long appointmentId)
        {
            try
            {
                var record = await _medicalRecordService.GetMedicalRecordByAppointmentAsync(appointmentId);
                if (record == null)
                {
                    return NotFound(new { message = "Không tìm thấy hồ sơ bệnh án cho cuộc hẹn này." });
                }
                return Ok(record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedicalRecord(long id, [FromBody] UpdateMedicalRecordDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!Guid.TryParse(userIdStr, out var doctorId))
            {
                return BadRequest(new { message = "DoctorId không hợp lệ." });
            }

            try
            {
                await _medicalRecordService.UpdateMedicalRecordAsync(id, dto, doctorId);
                return Ok(new { success = true, message = "Cập nhật bệnh án thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi hệ thống.", detail = ex.Message });
            }
        }
    }
}
