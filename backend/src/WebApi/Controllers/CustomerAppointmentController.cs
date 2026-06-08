using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPetClinic.Controllers
{
    /// <summary>
    /// API dành riêng cho Khách hàng (customer role) để xem và quản lý lịch hẹn của họ.
    /// </summary>
    [Authorize(Roles = "customer")]
    [ApiController]
    [Route("api/my-appointments")]
    public class CustomerAppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPetService _petService;

        public CustomerAppointmentController(
            IAppointmentService appointmentService,
            IPetService petService)
        {
            _appointmentService = appointmentService;
            _petService = petService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
                return userId;
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
        }

        /// <summary>
        /// Lấy tất cả lịch hẹn của khách hàng đang đăng nhập.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyAppointments()
        {
            try
            {
                var customerId = GetCurrentUserId();
                var appointments = await _appointmentService.GetCustomerAppointmentsAsync(customerId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết một lịch hẹn của khách hàng (chỉ xem được lịch hẹn của mình).
        /// </summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetDetail(long id)
        {
            try
            {
                var customerId = GetCurrentUserId();
                var appt = await _appointmentService.GetAppointmentDetailAsync(id);

                if (appt == null)
                    return NotFound(new { message = "Không tìm thấy lịch hẹn." });

                // Security check: khách hàng chỉ được xem lịch hẹn của mình
                if (appt.CustomerId != customerId)
                    return Forbid();

                return Ok(appt);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Khách hàng tự đặt lịch hẹn cho thú cưng của mình.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] CustomerBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var customerId = GetCurrentUserId();

                // Verify the pet belongs to this customer
                var pet = await _petService.GetPetByIdAsync(dto.PetId, customerId);
                if (pet == null)
                    return BadRequest(new { message = "Thú cưng không hợp lệ hoặc không thuộc về bạn." });

                var createDto = new AppointmentCreateDto
                {
                    CustomerId = customerId,
                    PetId = dto.PetId,
                    DoctorId = dto.DoctorId ?? Guid.Empty,
                    ServiceId = dto.ServiceId,
                    AppointmentDate = dto.AppointmentDate,
                    Symptom = dto.Symptom ?? string.Empty,
                    Note = dto.Note,
                };

                var appointmentId = await _appointmentService.CreateAppointmentAsync(createDto, customerId);
                return Ok(new { success = true, message = "Đặt lịch hẹn thành công! Chúng tôi sẽ xác nhận sớm.", id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Đặt lịch thất bại: " + ex.Message });
            }
        }

        /// <summary>
        /// Khách hàng huỷ lịch hẹn (chỉ khi trạng thái là pending).
        /// </summary>
        [HttpPut("{id:long}/cancel")]
        public async Task<IActionResult> CancelAppointment(long id)
        {
            try
            {
                var customerId = GetCurrentUserId();
                var appt = await _appointmentService.GetAppointmentDetailAsync(id);

                if (appt == null)
                    return NotFound(new { message = "Không tìm thấy lịch hẹn." });

                if (appt.CustomerId != customerId)
                    return Forbid();

                if (appt.Status != "pending" && appt.Status != "confirmed")
                    return BadRequest(new { message = $"Không thể huỷ lịch hẹn ở trạng thái '{appt.Status}'." });

                var success = await _appointmentService.UpdateAppointmentStatusAsync(id, "cancelled");
                return Ok(new { success, message = "Đã huỷ lịch hẹn thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách dịch vụ để khách hàng chọn khi đặt lịch.
        /// </summary>
        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _appointmentService.GetServicesAsync();
            return Ok(services);
        }
    }

    /// <summary>
    /// DTO cho khách hàng tự đặt lịch hẹn.
    /// </summary>
    public class CustomerBookingDto
    {
        public long PetId { get; set; }
        public Guid? DoctorId { get; set; }
        public long ServiceId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Symptom { get; set; }
        public string? Note { get; set; }
    }
}
