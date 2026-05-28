using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "doctor")]
    public class VeterinarianController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public VeterinarianController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // Dashboard xem lịch khám
        public async Task<IActionResult> Index()
        {
            // Lấy ID của Bác sĩ từ Context Auth (User đang đăng nhập)
            var userIdString = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid currentDoctorId))
            {
                return RedirectToAction("Login", "Account");
            }
            
            var appointments = await _appointmentService.GetDoctorAppointmentsAsync(currentDoctorId, DateTime.UtcNow);
            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(long id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest("Status cannot be empty.");
            }

            var result = await _appointmentService.UpdateAppointmentStatusAsync(id, status);
            if (!result)
            {
                return NotFound("Appointment not found.");
            }

            return RedirectToAction(nameof(Index));
        }
        
    }
}
