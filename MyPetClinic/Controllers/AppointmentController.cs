using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Linq;
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
            var services = await context.Services.OrderBy(s => s.Name).ToListAsync();
            return Json(services);
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentStats([FromServices] MyPetClinic.Infrastructure.Persistence.ApplicationDbContext context)
        {
            var today = DateTime.UtcNow.Date;
            var todayAppointments = await context.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .ToListAsync();

            var total = todayAppointments.Count;
            var pending = todayAppointments.Count(a => a.Status == "pending");
            var confirmed = todayAppointments.Count(a => a.Status == "confirmed");
            var waiting = todayAppointments.Count(a => a.Status == "waiting");
            var inProgress = todayAppointments.Count(a => a.Status == "in_progress");
            var completed = todayAppointments.Count(a => a.Status == "completed" || a.Status == "ready_to_pay");
            var cancelled = todayAppointments.Count(a => a.Status == "cancelled");

            return Json(new { total, pending, confirmed, waiting, inProgress, completed, cancelled });
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingAppointments([FromServices] MyPetClinic.Infrastructure.Persistence.ApplicationDbContext context)
        {
            var pendingAppointments = await context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .Where(a => a.Status == "pending")
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var result = pendingAppointments.Select(a => new
            {
                id = a.Id,
                petId = a.PetId,
                petName = a.Pet?.Name,
                species = a.Pet?.Species,
                breed = a.Pet?.Breed,
                weight = a.Pet?.Weight,
                isAggressive = a.Pet?.IsAggressive ?? false,
                customerId = a.CustomerId,
                customerName = a.Customer?.FullName,
                customerPhone = a.Customer?.Phone,
                serviceId = a.ServiceId,
                serviceName = a.Service?.Name,
                servicePrice = a.Service?.Price ?? 0,
                doctorId = a.DoctorId,
                doctorName = a.Doctor?.FullName,
                appointmentDate = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                symptom = a.Symptom,
                note = a.Note,
                qrToken = a.QrToken
            });

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetail(long id, [FromServices] MyPetClinic.Infrastructure.Persistence.ApplicationDbContext context)
        {
            var appt = await context.Appointments
                .Include(a => a.Pet)
                .Include(a => a.Customer)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (appt == null) return NotFound();
            
            return Json(new {
                id = appt.Id,
                petId = appt.PetId,
                petName = appt.Pet?.Name,
                species = appt.Pet?.Species,
                breed = appt.Pet?.Breed,
                weight = appt.Pet?.Weight,
                isAggressive = appt.Pet?.IsAggressive ?? false,
                customerId = appt.CustomerId,
                customerName = appt.Customer?.FullName,
                customerPhone = appt.Customer?.Phone,
                serviceId = appt.ServiceId,
                serviceName = appt.Service?.Name,
                servicePrice = appt.Service?.Price ?? 0,
                doctorId = appt.DoctorId,
                doctorName = appt.Doctor?.FullName,
                appointmentDate = appt.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                symptom = appt.Symptom,
                note = appt.Note,
                status = appt.Status,
                qrToken = appt.QrToken
            });
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
