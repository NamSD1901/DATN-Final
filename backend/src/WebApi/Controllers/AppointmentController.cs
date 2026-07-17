using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(Roles = "receptionist,admin,clinical_doctor,vaccination_doctor,Receptionist,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IReceptionistAppointmentService _appointmentService;

        public AppointmentController(IReceptionistAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] Guid? doctorId)
        {
            var events = await _appointmentService.GetCalendarEventsAsync(start, end, doctorId);
            return Ok(events);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] AppointmentUpdateStatusRequestDto req)
        {
            var success = await _appointmentService.UpdateAppointmentStatusAsync(id, req.Status, req.Reason);
            return Ok(new { success });
        }

        [HttpPut("{id}/reschedule")]
        public async Task<IActionResult> Reschedule(long id, [FromBody] AppointmentRescheduleRequestDto req)
        {
            try
            {
                var success = await _appointmentService.RescheduleAppointmentAsync(id, req.NewStart, req.Force);
                return Ok(new { success });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateTime date, [FromQuery] long? serviceId = null)
        {
            try
            {
                var slots = await _appointmentService.GetAvailableSlotsAsync(date, serviceId);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/change-doctor")]
        public async Task<IActionResult> UpdateDoctor(long id, [FromBody] ChangeDoctorRequestDto req)
        {
            try
            {
                var success = await _appointmentService.UpdateAppointmentDoctorAsync(id, req);
                return Ok(new { success });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("with-new-customer")]
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
                return Ok(new { success = true, id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _appointmentService.GetServicesAsync();
            return Ok(services);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetAppointmentStats()
        {
            var stats = await _appointmentService.GetAppointmentStatsAsync();
            return Ok(stats);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingAppointments()
        {
            var pendingAppointments = await _appointmentService.GetPendingAppointmentsAsync();
            return Ok(pendingAppointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var appt = await _appointmentService.GetAppointmentDetailAsync(id);
                
            if (appt == null) return NotFound();
            
            return Ok(appt);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MyPetClinic.Application.DTOs.AppointmentCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var createdBy = userIdClaim != null ? Guid.TryParse(userIdClaim.Value, out var uid) ? uid : Guid.Empty : Guid.Empty;

                var appointmentId = await _appointmentService.CreateAppointmentAsync(dto, createdBy);

                // Nếu Lễ tân tạo thì mặc định là confirmed
                await _appointmentService.UpdateAppointmentStatusAsync(appointmentId, "confirmed");

                return Ok(new { success = true, message = "Đã tạo lịch hẹn thành công!", id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] AppointmentCheckInRequestDto req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.QrToken))
                {
                    return BadRequest(new { success = false, message = "Mã QR không hợp lệ." });
                }

                var appt = await _appointmentService.CheckInAsync(new CheckInRequestDto { QrToken = req.QrToken });
                if (appt == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy lịch hẹn với mã QR này." });
                }

                return Ok(new { success = true, message = "Check-in thành công!", appointment = appt });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}/eligible-doctors")]
        public async Task<IActionResult> GetEligibleDoctors(long id)
        {
            try
            {
                var doctors = await _appointmentService.GetSuitableDoctorsForAppointmentAsync(id);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

}
