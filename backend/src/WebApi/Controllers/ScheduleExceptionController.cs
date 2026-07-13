using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScheduleExceptionController : ControllerBase
    {
        private readonly IScheduleExceptionService _scheduleExceptionService;

        public ScheduleExceptionController(IScheduleExceptionService scheduleExceptionService)
        {
            _scheduleExceptionService = scheduleExceptionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateException([FromBody] CreateScheduleExceptionDto dto)
        {
            var result = await _scheduleExceptionService.CreateExceptionAsync(dto);
            return Ok(result);
        }

        [HttpPost("{id}/Approve")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> ApproveException(long id, [FromBody] ApproveScheduleExceptionDto dto)
        {
            await _scheduleExceptionService.ApproveExceptionAsync(id, dto);
            return Ok(new { message = "Exception processed successfully" });
        }

        [HttpGet("Pending")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetPendingExceptions()
        {
            var result = await _scheduleExceptionService.GetAllPendingExceptionsAsync();
            return Ok(result);
        }
    }
}
