using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Models;
using System.Security.Claims;

namespace MyPetClinic.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. ĐĂNG KÝ (REGISTER)
        // ==========================================
        
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra email trùng lặp
            bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email.Trim().ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng trong hệ thống.");
                return View(model);
            }

            // Lấy thông tin quyền mặc định (customer)
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == 4 || r.Name.ToLower() == "customer");
            if (customerRole == null)
            {
                // Phòng hờ nếu chưa có quyền 'customer' thì tạo mới
                customerRole = new Role { Name = "customer" };
                _context.Roles.Add(customerRole);
                await _context.SaveChangesAsync();
            }

            // Băm mật khẩu bằng BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Tạo đối tượng User mới
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email.Trim().ToLower(),
                Phone = model.Phone,
                PasswordHash = hashedPassword,
                Address = model.Address,
                RoleId = customerRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("RegisterSuccess");
        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        // ==========================================
        // 2. ĐĂNG NHẬP (LOGIN)
        // ==========================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Truy vấn người dùng theo Email
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email.Trim().ToLower());

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // Xác minh mật khẩu băm BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // Kiểm tra trạng thái kích hoạt tài khoản
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            // Thiết lập các Claim danh tính
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email ?? "Khách Hàng"),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Đăng nhập vào hệ thống
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Chuyển hướng an toàn
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. ĐĂNG XUẤT (LOGOUT)
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Hỗ trợ cả GET đăng xuất nếu người dùng nhập URL trực tiếp
        [HttpGet]
        public async Task<IActionResult> LogoutDirect()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
