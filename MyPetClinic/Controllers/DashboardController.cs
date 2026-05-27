using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // Lấy User ID và thông tin từ Claim
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.Email = email;

            // Phân luồng View dựa vào Role
            switch (role)
            {
                case "admin":
                    return View("AdminDashboard");
                case "doctor":
                    return View("DoctorDashboard");
                case "receptionist":
                    return View("ReceptionistDashboard");
                case "customer":
                default:
                    return View("CustomerDashboard");
            }
        }
    }
}
