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
    public class ReceptionistController : Controller
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

        // ==========================================
        // 0. HÀNG KHÁM (KANBAN)
        // ==========================================
        public IActionResult Queue()
        {
            return View();
        }

        // ==========================================
        // 1. DANH SÁCH KHÁCH HÀNG (có tìm kiếm)
        // ==========================================
        public async Task<IActionResult> Customers(string? search)
        {
            ViewBag.Search = search;
            var customers = string.IsNullOrWhiteSpace(search) 
                ? await _customerService.GetAllCustomersAsync() 
                : await _customerService.SearchCustomersAsync(search);
            return View(customers);
        }

        // ==========================================
        // 2. CHI TIẾT KHÁCH HÀNG + DANH SÁCH THÚ CƯNG
        // ==========================================
        public async Task<IActionResult> CustomerDetail(Guid id)
        {
            var customer = await _customerService.GetCustomerDetailAsync(id);
            if (customer == null) return NotFound();

            var pets = await _customerService.GetPetsByCustomerAsync(id);
            var doctors = await _receptionistService.GetActiveDoctorsAsync();
            var services = await _appointmentService.GetServicesAsync();
            var appointments = await _appointmentService.GetCustomerAppointmentsAsync(id);

            var viewModel = new MyPetClinic.Web.Models.CustomerProfileViewModel
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

            return View(viewModel);
        }

        // ==========================================
        // 2b. CHI TIẾT THÚ CƯNG
        // ==========================================
        public async Task<IActionResult> PetDetail(long id)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null) return NotFound();

            var appointments = await _appointmentService.GetPetAppointmentsAsync(id);

            var viewModel = new MyPetClinic.Web.Models.PetProfileViewModel
            {
                Pet = pet,
                Customer = pet.Owner!,
                Appointments = appointments
            };

            return View(viewModel);
        }

        // ==========================================
        // 3. THÊM KHÁCH HÀNG MỚI
        // ==========================================
        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join("<br/>", errors) });
            }

            try
            {
                var customerId = await _customerService.CreateCustomerWithPetsAsync(model);
                return Json(new { success = true, customerId = customerId, message = $"Đã tạo hồ sơ cho khách hàng {model.FullName} thành công!" });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                Console.WriteLine($"[ERROR] CreateCustomer failed: {ex.Message} | Inner: {inner}");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo hồ sơ. Chi tiết: " + ex.Message + " " + inner });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Vui lòng nhập đủ thông tin bắt buộc." });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var createdBy = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

                var appointmentId = await _appointmentService.CreateAppointmentAsync(dto, createdBy);

                return Json(new { success = true, message = "Đã tạo phiếu tiếp nhận khám thành công!", id = appointmentId });
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException != null ? ex.InnerException.Message : "";
                Console.WriteLine($"[ERROR] CreateAppointment failed: {ex.Message} | Inner: {inner}");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo phiếu khám. Chi tiết: " + ex.Message });
            }
        }

        // ==========================================
        // 4. THÊM THÚ CƯNG CHO KHÁCH HÀNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> AddPet(Guid customerId)
        {
            var customer = await _customerService.GetCustomerDetailAsync(customerId);
            if (customer == null) return NotFound();
            ViewBag.Customer = customer;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPet(Pet model, Guid customerId)
        {
            var customer = await _customerService.GetCustomerDetailAsync(customerId);
            if (customer == null) return NotFound();

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

            TempData["Success"] = $"Đã thêm thú cưng '{newPet.Name}' thành công!";
            return RedirectToAction("CustomerDetail", new { id = customerId });
        }

        // ==========================================
        // 5. SỬA THÔNG TIN THÚ CƯNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> EditPet(long id)
        {
            var pet = await _petRepository.GetPetByIdAsync(id);
            if (pet == null) return NotFound();
            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPet(Pet model)
        {
            var pet = await _petRepository.GetPetByIdAsync(model.Id);
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

            TempData["Success"] = $"Đã cập nhật thông tin thú cưng '{pet.Name}' thành công!";
            return RedirectToAction("CustomerDetail", new { id = pet.OwnerId });
        }

        // ==========================================
        // 6. TÌM KIẾM NHANH (AJAX) - QR Scan hỗ trợ
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> QuickSearch(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Json(new { found = false });

            var customerWithPets = await _receptionistService.GetCustomerWithPetsByPhoneAsync(phone);

            if (customerWithPets == null)
                return Json(new { found = false });

            return Json(new
            {
                found = true,
                customerId = customerWithPets.CustomerId,
                fullName = customerWithPets.FullName,
                phone = customerWithPets.Phone,
                email = customerWithPets.Email,
                pets = customerWithPets.Pets.Select(p => new { p.Id, p.Name, p.Species, p.Breed, p.Weight })
            });
        }
        // ==========================================
        // 7. THÊM WORKFLOW API (OmniSearch, CheckIn, Queue, WalkIn)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> OmniSearch(string q)
        {
            var results = await _receptionistService.OmniSearchAsync(q);
            return Json(results);
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
        {
            try
            {
                var success = await _receptionistService.CheckInAsync(request);
                if (success)
                    return Json(new { success = true, message = "Check-in thành công. Đã xếp vào hàng đợi." });
                return Json(new { success = false, message = "Check-in thất bại. Lỗi không xác định." });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Lỗi hệ thống. Vui lòng thử lại sau." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTodayQueue()
        {
            var queue = await _receptionistService.GetTodayQueueAsync();
            return Json(queue);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWalkIn([FromBody] WalkInRequestDto request)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu Walk-in không hợp lệ." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

            try
            {
                var appointmentId = await _receptionistService.CreateWalkInAsync(request, createdBy);
                return Json(new { success = true, message = "Đã tạo ca Walk-in thành công và xếp vào hàng đợi.", appointmentId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tạo ca Walk-in: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQueueStatus(long appointmentId, string status)
        {
            var success = await _receptionistService.UpdateQueueStatusAsync(appointmentId, status);
            return Json(new { success = success });
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _receptionistService.GetActiveDoctorsAsync();
            return Json(doctors.Select(d => new { Id = d.Id, FullName = d.FullName }));
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return Json(new { success = false });

            var customerWithPets = await _receptionistService.GetCustomerWithPetsByPhoneAsync(phone);
            
            if (customerWithPets == null) return Json(new { success = false });

            return Json(new { 
                success = true, 
                customer = new { Id = customerWithPets.CustomerId, FullName = customerWithPets.FullName },
                pets = customerWithPets.Pets.Select(p => new { p.Id, p.Name, p.Species })
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmergencyCustomer(long appointmentId, Guid customerId, long petId)
        {
            var success = await _receptionistService.UpdateEmergencyCustomerAsync(appointmentId, customerId, petId);
            if (success)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Không thể ghép nối hồ sơ. Ca khám có thể không tồn tại hoặc không phải ca cấp cứu ẩn danh." });
        }
    }
}
