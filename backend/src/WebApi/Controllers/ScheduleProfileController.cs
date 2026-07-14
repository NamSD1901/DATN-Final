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

        [HttpGet]
        public async Task<IActionResult> GetAllProfiles()
        {
            var profiles = await _scheduleProfileService.GetAllProfilesAsync();
            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProfileById(long id)
        {
            var profile = await _scheduleProfileService.GetProfileByIdAsync(id);
            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateScheduleProfileDto dto)
        {
            var result = await _scheduleProfileService.CreateProfileAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(long id, [FromBody] UpdateScheduleProfileDto dto)
        {
            var result = await _scheduleProfileService.UpdateProfileAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(long id)
        {
            await _scheduleProfileService.DeleteProfileAsync(id);
            return Ok(new { message = "Profile deleted successfully" });
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
