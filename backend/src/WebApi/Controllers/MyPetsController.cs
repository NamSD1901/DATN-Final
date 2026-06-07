using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Controllers
{
    [Authorize(Roles = "customer")]
    public class MyPetsController : Controller
    {
        private readonly IPetService _petService;

        public MyPetsController(IPetService petService)
        {
            _petService = petService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var pets = await _petService.GetMyPetsAsync(userId);
                return View(pets);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePetDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _petService.AddPetAsync(dto, userId);
                TempData["SuccessMessage"] = "Thêm thú cưng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm thú cưng: " + ex.Message;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pet = await _petService.GetPetByIdAsync(id, userId);
                
                if (pet == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thú cưng.";
                    return RedirectToAction(nameof(Index));
                }

                var updateDto = new UpdatePetDto
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Species = pet.Species,
                    Breed = pet.Breed,
                    Gender = pet.Gender,
                    BirthDate = pet.BirthDate,
                    Weight = pet.Weight,
                    Color = pet.Color,
                    BloodType = pet.BloodType,
                    Sterilized = pet.Sterilized ?? false,
                    MicrochipCode = pet.MicrochipCode,
                    AllergyNote = pet.AllergyNote
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdatePetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _petService.UpdatePetAsync(dto, userId);
                TempData["SuccessMessage"] = "Cập nhật thông tin thú cưng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật thú cưng: " + ex.Message;
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pet = await _petService.GetPetByIdAsync(id, userId);
                if (pet == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thú cưng.";
                    return RedirectToAction(nameof(Index));
                }
                return View(pet);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pet = await _petService.GetPetByIdAsync(id, userId);
                if (pet == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thú cưng.";
                    return RedirectToAction(nameof(Index));
                }
                return View(pet);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _petService.DeletePetAsync(id, userId);
                TempData["SuccessMessage"] = "Đã xóa thú cưng thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa thú cưng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
