using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Receptionist")]
    public class ScheduleProfileController : ControllerBase
    {
        private readonly IScheduleProfileService _scheduleProfileService;

        public ScheduleProfileController(IScheduleProfileService scheduleProfileService)
        {
            _scheduleProfileService = scheduleProfileService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateScheduleProfileDto dto)
        {
            var result = await _scheduleProfileService.CreateProfileAsync(dto);
            return Ok(result);
        }

        [HttpPost("Assign")]
        public async Task<IActionResult> AssignProfile([FromBody] AssignProfileDto dto)
        {
            await _scheduleProfileService.AssignProfileToDoctorsAsync(dto);
            return Ok(new { message = "Profile assigned successfully" });
        }

        [HttpPost("Generate")]
        public async Task<IActionResult> GenerateSchedule([FromQuery] Guid doctorId, [FromQuery] int daysToGenerate = 30)
        {
            var generatedCount = await _scheduleProfileService.GenerateScheduleFromProfileAsync(doctorId, daysToGenerate);
            return Ok(new { message = $"Generated {generatedCount} schedule slots." });
        }
    }
}
