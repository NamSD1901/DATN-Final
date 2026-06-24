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
        private readonly ICustomerAppointmentService _customerAppointmentService;
        private readonly IInvoiceService _invoiceService;
        private readonly MyPetClinic.Application.Interfaces.Repositories.IUserRepository _userRepository;

        public CustomerAppointmentController(
            IAppointmentService appointmentService,
            IPetService petService,
            ICustomerAppointmentService customerAppointmentService,
            IInvoiceService invoiceService,
            MyPetClinic.Application.Interfaces.Repositories.IUserRepository userRepository)
        {
            _appointmentService = appointmentService;
            _petService = petService;
            _customerAppointmentService = customerAppointmentService;
            _invoiceService = invoiceService;
            _userRepository = userRepository;
        }

        private async Task<Guid> GetCurrentCustomerIdAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user != null && user.CustomerId.HasValue)
                {
                    return user.CustomerId.Value;
                }
            }
            throw new UnauthorizedAccessException("Không tìm thấy thông tin khách hàng. Vui lòng xác thực hồ sơ trước.");
        }

        /// <summary>
        /// Lấy tất cả lịch hẹn của khách hàng đang đăng nhập (hỗ trợ phân trang và lọc trạng thái).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyAppointments([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var appointments = await _appointmentService.GetCustomerAppointmentsPaginatedAsync(customerId, status, page, pageSize);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách các slot thời gian rảnh của các bác sĩ trong một ngày cụ thể.
        /// </summary>
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

        /// <summary>
        /// Lấy chi tiết một lịch hẹn của khách hàng (chỉ xem được lịch hẹn của mình).
        /// </summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetDetail(long id)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
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
                var customerId = await GetCurrentCustomerIdAsync();

                // Gọi CustomerAppointmentService để áp dụng các Business Rules
                var appointmentId = await _customerAppointmentService.BookAppointmentAsync(dto, customerId);
                return Ok(new { success = true, message = "Đặt lịch hẹn thành công! Chúng tôi sẽ xác nhận sớm.", id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Đặt lịch thất bại: " + ex.Message });
            }
        }

        [HttpPut("{id:long}/cancel")]
        public async Task<IActionResult> CancelAppointment(long id)
        {
            // Chức năng tự huỷ lịch đã bị vô hiệu hoá theo PRD mới.
            // Khách hàng phải liên hệ Lễ tân để huỷ lịch.
            return BadRequest(new { message = "Chức năng tự huỷ lịch trên hệ thống đã được tắt. Vui lòng liên hệ trực tiếp với phòng khám để huỷ lịch hẹn của bạn." });
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

        /// <summary>
        /// Lấy danh sách vắc-xin còn hàng trong kho.
        /// </summary>
        [HttpGet("vaccines")]
        public async Task<IActionResult> GetAvailableVaccines()
        {
            try
            {
                var vaccines = await _customerAppointmentService.GetAvailableVaccinesAsync();
                return Ok(vaccines);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra phác đồ tiêm chủng cho thú cưng trước khi đặt lịch.
        /// </summary>
        [HttpPost("validate-vaccine")]
        public async Task<IActionResult> ValidateVaccine([FromBody] ValidateVaccineDto dto)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var validation = await _customerAppointmentService.ValidateVaccineAsync(customerId, dto.PetId, dto.VaccineId, dto.TargetDate);
                return Ok(validation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử bệnh án khám của thú cưng thuộc sở hữu (chống IDOR).
        /// </summary>
        [HttpGet("pets/{petId:long}/medical-history")]
        public async Task<IActionResult> GetPetMedicalHistory(long petId)
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var history = await _appointmentService.GetPetMedicalHistoryAsync(petId, customerId);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách hóa đơn của khách hàng đang đăng nhập.
        /// </summary>
        [HttpGet("invoices")]
        public async Task<IActionResult> GetMyInvoices()
        {
            try
            {
                var customerId = await GetCurrentCustomerIdAsync();
                var invoices = await _invoiceService.GetCustomerInvoicesAsync(customerId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // CustomerBookingDto đã được di chuyển sang MyPetClinic.Application.DTOs

    /// <summary>
    /// DTO kiểm tra phác đồ vắc-xin.
    /// </summary>
    public class ValidateVaccineDto
    {
        public long PetId { get; set; }
        public long VaccineId { get; set; }
        public DateTime TargetDate { get; set; }
    }
}
