using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }

        private Guid GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId)) return userId;
            throw new UnauthorizedAccessException("User ID not found in token/cookie.");
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserId();
                var profile = await _userService.GetUserProfileAsync(userId);
                if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ." });
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                bool success = await _userService.UpdateUserProfileAsync(userId, model);
                if (success) return Ok(new { success = true, message = "Cập nhật thông tin cá nhân thành công!" });
                return BadRequest(new { success = false, message = "Có lỗi xảy ra khi cập nhật thông tin." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                bool success = await _userService.ChangePasswordAsync(userId, model);
                if (success) return Ok(new { success = true, message = "Đổi mật khẩu thành công!" });
                return BadRequest(new { success = false, message = "Mật khẩu hiện tại không chính xác hoặc có lỗi xảy ra." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn một file ảnh hợp lệ." });

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif" });

            // Limit file size to 2MB
            if (avatarFile.Length > 2 * 1024 * 1024)
                return BadRequest(new { message = "Kích thước ảnh không được vượt quá 2MB." });

            try
            {
                var userId = GetUserId();
                string webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "avatars");
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

                string avatarUrl = $"/uploads/avatars/{uniqueFileName}";
                
                bool success = await _userService.UpdateAvatarAsync(userId, avatarUrl);
                if (success) return Ok(new { success = true, avatarUrl, message = "Cập nhật ảnh đại diện thành công!" });
                return BadRequest(new { success = false, message = "Có lỗi xảy ra khi lưu ảnh đại diện." });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
