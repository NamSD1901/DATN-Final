using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "doctor,admin,receptionist,Doctor,Admin,Receptionist,SystemAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly IReceptionistService _receptionistService;
        private readonly IAppointmentService _appointmentService;

        public DoctorController(IReceptionistService receptionistService, IAppointmentService appointmentService)
        {
            _receptionistService = receptionistService;
            _appointmentService = appointmentService;
        }

        [HttpGet("weekly-schedule")]
        public async Task<IActionResult> GetMyWeeklySchedule([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] Guid? targetDoctorId = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var currentDoctorId)) return Unauthorized();

            var isAdmin = User.IsInRole("admin") || User.IsInRole("Admin") || User.IsInRole("receptionist") || User.IsInRole("Receptionist") || User.IsInRole("SystemAdmin") || User.IsInRole("doctor");
            
            // Nếu là admin/lễ tân và có chọn bác sĩ cụ thể, lọc theo bác sĩ đó. Nếu không chọn, mặc định null (xem tất cả).
            // Nếu là bác sĩ, ép buộc chỉ xem của mình.
            var isStrictAdmin = User.IsInRole("admin") || User.IsInRole("Admin") || User.IsInRole("receptionist") || User.IsInRole("Receptionist") || User.IsInRole("SystemAdmin");
            Guid? filterDoctorId = isStrictAdmin ? targetDoctorId : currentDoctorId;

            // Sử dụng trực tiếp start, end từ query giống hệt như AppointmentController
            var events = await _appointmentService.GetCalendarEventsAsync(start, end, filterDoctorId);
            return Ok(events);
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var queue = await _receptionistService.GetTodayQueueAsync();
            var appt = queue.FirstOrDefault(q => q.AppointmentId == appointmentId);
            if (appt == null) return NotFound(new { message = "Không tìm thấy ca khám." });

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !appt.DoctorId.ToString().Equals(userIdStr, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Tương đương với việc kéo thẻ sang cột "Đang khám"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "in_progress");
            return Ok(new { success });
        }

        [HttpPost("finish-treatment")]
        public async Task<IActionResult> FinishTreatment([FromBody] long appointmentId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var queue = await _receptionistService.GetTodayQueueAsync();
            var appt = queue.FirstOrDefault(q => q.AppointmentId == appointmentId);
            if (appt == null) return NotFound(new { message = "Không tìm thấy ca khám." });

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !appt.DoctorId.ToString().Equals(userIdStr, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            // Tương đương với việc kéo thẻ sang cột "Chờ thanh toán"
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, "ready_to_pay");
            return Ok(new { success });
        }
    }
}
