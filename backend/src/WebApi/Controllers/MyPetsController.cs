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
    [ApiController]
    [Route("api/[controller]")]
    public class MyPetsController : ControllerBase
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

        [HttpGet]
        public async Task<IActionResult> GetMyPets()
        {
            try
            {
                var userId = GetCurrentUserId();
                var pets = await _petService.GetMyPetsAsync(userId);
                return Ok(pets);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePet([FromBody] CreatePetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetCurrentUserId();
                await _petService.AddPetAsync(dto, userId);
                return Ok(new { success = true, message = "Thêm thú cưng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi thêm thú cưng: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [MyPetClinic.WebApi.Filters.AuthorizeOwner]
        public async Task<IActionResult> UpdatePet(long id, [FromBody] UpdatePetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != dto.Id)
            {
                return BadRequest(new { success = false, message = "ID thú cưng không hợp lệ." });
            }

            try
            {
                var userId = GetCurrentUserId();
                await _petService.UpdatePetAsync(dto, userId);
                return Ok(new { success = true, message = "Cập nhật thông tin thú cưng thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi cập nhật thú cưng: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        [MyPetClinic.WebApi.Filters.AuthorizeOwner]
        public async Task<IActionResult> GetPetDetails(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pet = await _petService.GetPetByIdAsync(id, userId);
                if (pet == null)
                {
                    return NotFound(new { message = "Không tìm thấy thú cưng." });
                }
                return Ok(pet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [MyPetClinic.WebApi.Filters.AuthorizeOwner]
        public async Task<IActionResult> DeletePet(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _petService.DeletePetAsync(id, userId);
                return Ok(new { success = true, message = "Đã xóa thú cưng thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi xóa thú cưng: " + ex.Message });
            }
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadPetAvatar(IFormFile avatarFile, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif" });

            if (avatarFile.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });

            try
            {
                string webRootPath = webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "pets");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                string avatarUrl = $"/uploads/pets/{uniqueFileName}";
                return Ok(new { success = true, avatarUrl, message = "Tải ảnh lên thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi tải ảnh lên: " + ex.Message });
            }
        }
    }
}
