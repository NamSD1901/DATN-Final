using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(Roles = "receptionist,admin,Receptionist,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReceptionistController : ControllerBase
    {
        private readonly IReceptionistService _receptionistService;

        public ReceptionistController(IReceptionistService receptionistService)
        {
            _receptionistService = receptionistService;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> Customers([FromQuery] string? search)
        {
            var customers = string.IsNullOrWhiteSpace(search) 
                ? await _receptionistService.GetAllCustomersAsync() 
                : await _receptionistService.SearchCustomersAsync(search);
            return Ok(customers);
        }

        [HttpGet("customers/{id}")]
        public async Task<IActionResult> CustomerDetail(Guid id)
        {
            var viewModel = await _receptionistService.GetCustomerDashboardDetailAsync(id);
            if (viewModel == null) return NotFound();

            return Ok(viewModel);
        }

        [HttpGet("pets/{id}")]
        public async Task<IActionResult> PetDetail(long id)
        {
            var viewModel = await _receptionistService.GetPetDashboardDetailAsync(id);
            if (viewModel == null) return NotFound();

            return Ok(viewModel);
        }

        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, message = string.Join("<br/>", errors) });
            }

            try
            {
                var customerId = await _receptionistService.CreateCustomerWithPetsAsync(model);
                return Ok(new { success = true, customerId = customerId, message = $"Đã tạo hồ sơ cho khách hàng {model.FullName} thành công!" });
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi nghiệp vụ: trùng email, SĐT, v.v.
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                return BadRequest(new { success = false, message = "Đã xảy ra lỗi khi tạo hồ sơ. Chi tiết: " + ex.Message + " " + inner });
            }
        }

        [HttpPost("appointments")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Vui lòng nhập đủ thông tin bắt buộc." });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var createdBy = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

                var appointmentId = await _receptionistService.CreateAppointmentAsync(dto, createdBy);

                return Ok(new { success = true, message = "Đã tạo phiếu tiếp nhận khám thành công!", id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Đã xảy ra lỗi khi tạo phiếu khám. Chi tiết: " + ex.Message });
            }
        }

        [HttpPost("customers/{customerId}/pets")]
        public async Task<IActionResult> AddPet(Guid customerId, [FromBody] CreatePetDto model)
        {
            try
            {
                var petId = await _receptionistService.AddPetAsync(customerId, model);
                return Ok(new { success = true, message = $"Đã thêm thú cưng '{model.Name}' thành công!", petId = petId });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("pets/{id}")]
        public async Task<IActionResult> EditPet(long id, [FromBody] UpdatePetDto model)
        {
            try
            {
                await _receptionistService.UpdatePetAsync(id, model);
                return Ok(new { success = true, message = $"Đã cập nhật thông tin thú cưng '{model.Name}' thành công!" });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("pets/{id}/status")]
        public async Task<IActionResult> UpdatePetStatus(long id, [FromBody] UpdatePetStatusRequest req, [FromServices] IPetService petService)
        {
            try
            {
                await petService.UpdatePetStatusAsync(id, req.IsDeceased, req.IsAggressive);
                return Ok(new { success = true, message = "Đã cập nhật trạng thái thú cưng thành công!" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, message = "Không tìm thấy thú cưng." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("quick-search")]
        public async Task<IActionResult> QuickSearch([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Ok(new { found = false });

            var customerWithPets = await _receptionistService.GetCustomerWithPetsByPhoneAsync(phone);

            if (customerWithPets == null)
                return Ok(new { found = false });

            return Ok(new
            {
                found = true,
                customerId = customerWithPets.CustomerId,
                fullName = customerWithPets.FullName,
                phone = customerWithPets.Phone,
                email = customerWithPets.Email,
                pets = customerWithPets.Pets.Select(p => new { p.Id, p.Name, p.Species, p.Breed, p.Weight })
            });
        }

        [HttpGet("omni-search")]
        public async Task<IActionResult> OmniSearch([FromQuery] string q)
        {
            var results = await _receptionistService.OmniSearchAsync(q);
            return Ok(results);
        }
        [HttpGet("appointment-preview")]
        public async Task<IActionResult> GetAppointmentPreview([FromQuery] string qrToken)
        {
            try
            {
                var preview = await _receptionistService.GetAppointmentPreviewByQrAsync(qrToken);
                return Ok(new { success = true, data = preview });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
        {
            try
            {
                var success = await _receptionistService.CheckInAsync(request);
                if (success)
                    return Ok(new { success = true, message = "Check-in thành công. Đã xếp vào hàng đợi." });
                return BadRequest(new { success = false, message = "Check-in thất bại. Lỗi không xác định." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }

        [HttpGet("queue")]
        public async Task<IActionResult> GetTodayQueue()
        {
            var queue = await _receptionistService.GetTodayQueueAsync();
            return Ok(queue);
        }

        [HttpPost("walk-in")]
        public async Task<IActionResult> CreateWalkIn([FromBody] WalkInRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu Walk-in không hợp lệ." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

            try
            {
                var appointmentId = await _receptionistService.CreateWalkInAsync(request, createdBy);
                return Ok(new { success = true, message = "Đã tạo ca Walk-in thành công và xếp vào hàng đợi.", appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi tạo ca Walk-in: " + ex.Message });
            }
        }

        [HttpPut("queue/{appointmentId}/status")]
        public async Task<IActionResult> UpdateQueueStatus(long appointmentId, [FromBody] UpdateQueueStatusRequest req)
        {
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, req.Status);
            return Ok(new { success = success });
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _receptionistService.GetActiveDoctorsAsync();
            return Ok(doctors.Select(d => new { Id = d.Id, FullName = d.FullName }));
        }

        [HttpGet("customer-by-phone")]
        public async Task<IActionResult> GetCustomerByPhone([FromQuery] string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return Ok(new { success = false });

            var customerWithPets = await _receptionistService.GetCustomerWithPetsByPhoneAsync(phone);
            
            if (customerWithPets == null) return Ok(new { success = false });

            return Ok(new { 
                success = true, 
                customer = new { Id = customerWithPets.CustomerId, FullName = customerWithPets.FullName },
                pets = customerWithPets.Pets.Select(p => new { p.Id, p.Name, p.Species })
            });
        }

        [HttpPut("emergency/{appointmentId}/customer")]
        public async Task<IActionResult> UpdateEmergencyCustomer(long appointmentId, [FromBody] UpdateEmergencyCustomerRequest req)
        {
            var success = await _receptionistService.UpdateEmergencyCustomerAsync(appointmentId, req.CustomerId, req.PetId);
            if (success)
            {
                return Ok(new { success = true });
            }
            return BadRequest(new { success = false, message = "Không thể ghép nối hồ sơ. Ca khám có thể không tồn tại hoặc không phải ca cấp cứu ẩn danh." });
        }
    }
}
