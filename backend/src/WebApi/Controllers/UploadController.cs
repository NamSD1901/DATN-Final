using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UploadController : ControllerBase
    {
        [HttpPost("medical-images")]
        public async Task<IActionResult> UploadMedicalImages(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "Không có file nào được tải lên." });

            var uploadedUrls = new List<string>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            long maxFileSize = 5 * 1024 * 1024; // 5MB

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "medical-records");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { message = $"File {file.FileName} vượt quá kích thước cho phép (5MB)." });
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    return BadRequest(new { message = $"Định dạng file {ext} không được hỗ trợ." });
                }

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // URL to be accessed from frontend (assuming wwwroot is served as static files)
                var fileUrl = $"/uploads/medical-records/{uniqueFileName}";
                uploadedUrls.Add(fileUrl);
            }

            return Ok(new { success = true, urls = uploadedUrls });
        }
    }
}
