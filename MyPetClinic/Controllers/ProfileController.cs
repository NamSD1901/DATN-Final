using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Services;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IUserService userService, IWebHostEnvironment webHostEnvironment)
        {
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _userService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            ViewBag.Profile = profile;
            return View(new UpdateProfileDto
            {
                FullName = profile.FullName,
                Phone = profile.Phone,
                Address = profile.Address,
                Gender = profile.Gender,
                DateOfBirth = profile.DateOfBirth
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var profile = await _userService.GetUserProfileAsync(userId);
                ViewBag.Profile = profile;
                return View("Index", model);
            }

            bool success = await _userService.UpdateUserProfileAsync(userId, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật thông tin.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ. Vui lòng kiểm tra lại mật khẩu (tối thiểu 8 ký tự và phải khớp nhau).";
                return RedirectToAction("Index", new { tab = "password" });
            }

            bool success = await _userService.ChangePasswordAsync(userId, model);
            if (success)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại không chính xác hoặc có lỗi xảy ra.";
            }

            return RedirectToAction("Index", new { tab = "password" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out Guid userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (avatarFile == null || avatarFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một file ảnh hợp lệ.";
                return RedirectToAction("Index");
            }

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Chỉ chấp nhận các file ảnh định dạng: .jpg, .jpeg, .png, .gif";
                return RedirectToAction("Index");
            }

            // Limit file size to 2MB
            if (avatarFile.Length > 2 * 1024 * 1024)
            {
                TempData["ErrorMessage"] = "Kích thước ảnh không được vượt quá 2MB.";
                return RedirectToAction("Index");
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
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
            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật ảnh đại diện thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu ảnh đại diện.";
            }

            return RedirectToAction("Index");
        }
    }
}
