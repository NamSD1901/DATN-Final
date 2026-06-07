using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDashboardInfo()
        {
            // Lấy User ID và thông tin từ Claim
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            return Ok(new
            {
                UserId = userId,
                UserName = userName,
                Email = email,
                Role = role
            });
        }
    }
}
