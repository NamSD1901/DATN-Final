using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using System.Security.Claims;

using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.Interfaces.Repositories;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ReceptionistController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IPetRepository _petRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IReceptionistService _receptionistService;

        public ReceptionistController(
            ICustomerService customerService, 
            IPetRepository petRepository, 
            IAppointmentService appointmentService,
            IReceptionistService receptionistService)
        {
            _customerService = customerService;
            _petRepository = petRepository;
            _appointmentService = appointmentService;
            _receptionistService = receptionistService;
        }

        [HttpGet("customers")]
        public async Task<IActionResult> Customers([FromQuery] string? search)
        {
            var customers = string.IsNullOrWhiteSpace(search) 
                ? await _customerService.GetAllCustomersAsync() 
                : await _customerService.SearchCustomersAsync(search);
            return Ok(customers);
        }

        [HttpGet("customers/{id}")]
        public async Task<IActionResult> CustomerDetail(Guid id)
        {
            var customer = await _customerService.GetCustomerDetailAsync(id);
            if (customer == null) return NotFound();

            var pets = await _customerService.GetPetsByCustomerAsync(id);
            var doctors = await _receptionistService.GetActiveDoctorsAsync();
            var services = await _appointmentService.GetServicesAsync();
            var appointments = await _appointmentService.GetCustomerAppointmentsAsync(id);

            var viewModel = new 
            {
                Customer = customer,
                Pets = pets,
                ActiveDoctors = doctors,
                Services = services,
                Appointments = appointments,
                TotalVisits = appointments.Count(a => a.Status == "completed" || a.Status == "ready_to_pay"),
                TotalSpent = appointments.Where(a => a.InvoiceStatus == "paid").Sum(a => a.InvoiceTotalAmount ?? 0),
                NoShowCount = appointments.Count(a => a.Status == "cancelled"),
                UnpaidBalance = appointments.Where(a => a.InvoiceStatus == "unpaid").Sum(a => a.InvoiceTotalAmount ?? 0)
            };

            return Ok(viewModel);
        }

        [HttpGet("pets/{id}")]
        public async Task<IActionResult> PetDetail(long id)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null) return NotFound();

            var appointments = await _appointmentService.GetPetAppointmentsAsync(id);

            var viewModel = new 
            {
                Pet = pet,
                Customer = pet.Owner!,
                Appointments = appointments
            };

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
                var customerId = await _customerService.CreateCustomerWithPetsAsync(model);
                return Ok(new { success = true, customerId = customerId, message = $"Đã tạo hồ sơ cho khách hàng {model.FullName} thành công!" });
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

                var appointmentId = await _appointmentService.CreateAppointmentAsync(dto, createdBy);

                return Ok(new { success = true, message = "Đã tạo phiếu tiếp nhận khám thành công!", id = appointmentId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Đã xảy ra lỗi khi tạo phiếu khám. Chi tiết: " + ex.Message });
            }
        }

        [HttpPost("customers/{customerId}/pets")]
        public async Task<IActionResult> AddPet(Guid customerId, [FromBody] Pet model)
        {
            var customer = await _customerService.GetCustomerDetailAsync(customerId);
            if (customer == null) return NotFound(new { message = "Không tìm thấy khách hàng" });

            var newPet = new Pet
            {
                OwnerId = customerId,
                Name = model.Name?.Trim(),
                Species = model.Species?.Trim(),
                Breed = model.Breed?.Trim(),
                Gender = model.Gender,
                BirthDate = model.BirthDate,
                Weight = model.Weight,
                Color = model.Color?.Trim(),
                AllergyNote = model.AllergyNote?.Trim(),
                Sterilized = model.Sterilized,
                MicrochipCode = model.MicrochipCode?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _petRepository.CreatePetAsync(newPet);
            await _petRepository.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã thêm thú cưng '{newPet.Name}' thành công!", petId = newPet.Id });
        }

        [HttpPut("pets/{id}")]
        public async Task<IActionResult> EditPet(long id, [FromBody] Pet model)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null) return NotFound();

            pet.Name = model.Name?.Trim();
            pet.Species = model.Species?.Trim();
            pet.Breed = model.Breed?.Trim();
            pet.Gender = model.Gender;
            pet.BirthDate = model.BirthDate;
            pet.Weight = model.Weight;
            pet.Color = model.Color?.Trim();
            pet.AllergyNote = model.AllergyNote?.Trim();
            pet.Sterilized = model.Sterilized;
            pet.MicrochipCode = model.MicrochipCode?.Trim();

            await _petRepository.UpdatePetAsync(pet);
            await _petRepository.SaveChangesAsync();

            return Ok(new { success = true, message = $"Đã cập nhật thông tin thú cưng '{pet.Name}' thành công!" });
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

    public class UpdateQueueStatusRequest
    {
        public string Status { get; set; }
    }

    public class UpdateEmergencyCustomerRequest
    {
        public Guid CustomerId { get; set; }
        public long PetId { get; set; }
    }
}
