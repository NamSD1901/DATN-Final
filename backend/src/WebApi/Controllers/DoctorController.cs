using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Doctor,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IReceptionistService _receptionistService;

        public DoctorController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        [HttpGet("queue")]
        public async Task<IActionResult> GetMyQueue()
        {
            var queue = await _receptionistService.GetTodayQueueAsync();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userIdStr)) return Ok(new List<object>());

            var myQueue = queue.Where(q => q.DoctorId.ToString().Equals(userIdStr, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(myQueue);
        }

        [HttpPost("start-treatment")]
        public async Task<IActionResult> StartTreatment([FromBody] long appointmentId)
        {
            // Tương đương với việc kéo thẻ sang cột "Đang khám"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "in_progress");
            return Ok(new { success });
        }

        [HttpPost("finish-treatment")]
        public async Task<IActionResult> FinishTreatment([FromBody] long appointmentId)
        {
            // Tương đương với việc kéo thẻ sang cột "Chờ thanh toán"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "ready_to_pay");
            return Ok(new { success });
        }
    }
}
