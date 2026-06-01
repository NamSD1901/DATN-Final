using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(DateTime start, DateTime end, Guid? doctorId)
        {
            // FullCalendar passes ISO strings for start and end
            var events = await _appointmentService.GetCalendarEventsAsync(start, end, doctorId);
            return Json(events);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(long id, string status)
        {
            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> Reschedule(long id, DateTime newStart)
        {
            var success = await _appointmentService.RescheduleAppointmentAsync(id, newStart);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithNewCustomer([FromBody] MyPetClinic.Application.DTOs.AppointmentWithNewCustomerDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdStr, out var createdBy))
                {
                    return Unauthorized();
                }

                var appointmentId = await _appointmentService.CreateAppointmentWithNewCustomerAsync(dto, createdBy);
                return Json(new { success = true, id = appointmentId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetServices([FromServices] MyPetClinic.Infrastructure.Persistence.ApplicationDbContext context)
        {
            var services = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                context.Services.OrderBy(s => s.Name)
            );
            return Json(services);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MyPetClinic.Application.DTOs.AppointmentCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Vui lòng nhập đủ thông tin bắt buộc." });
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var createdBy = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

                var appointmentId = await _appointmentService.CreateAppointmentAsync(dto, createdBy);

                // Nếu Lễ tân tạo thì mặc định là confirmed
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, "confirmed");

                return Json(new { success = true, message = "Đã tạo lịch hẹn thành công!", id = appointmentId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
    }
}
