using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Models;
using MyPetClinic.Application.Interfaces.Services;
using System.Security.Claims;


namespace MyPetClinic.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;

        public AccountController(ApplicationDbContext context, IGoogleAuthService googleAuthService, IEmailService emailService, IOtpService otpService)
        {
            _context = context;
            _googleAuthService = googleAuthService;
            _emailService = emailService;
            _otpService = otpService;
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
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email.Trim().ToLower());
            if (existingUser != null)
            {
                if (existingUser.IsActive)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng trong hệ thống.");
                    return View(model);
                }
                else
                {
                    // Nếu tài khoản chưa active, xoá đi tạo lại để cấp OTP mới
                    _context.Users.Remove(existingUser);
                    await _context.SaveChangesAsync();
                }
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

            // Tạo đối tượng User mới (IsActive = false)
            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email.Trim().ToLower(),
                Phone = model.Phone,
                PasswordHash = hashedPassword,
                Address = model.Address,
                RoleId = customerRole.Id,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Tạo OTP và gửi Email
            string emailKey = user.Email.ToLower();
            string otp = _otpService.GenerateOtp(emailKey);
            string emailBody = $@"
                <h3>Xin chào {user.FullName},</h3>
                <p>Cảm ơn bạn đã đăng ký tài khoản tại MyPetClinic.</p>
                <p>Mã OTP của bạn là: <strong><span style='font-size:24px;color:blue;'>{otp}</span></strong></p>
                <p>Mã OTP này sẽ hết hạn trong vòng 5 phút.</p>";
            
            await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản MyPetClinic", emailBody);

            return RedirectToAction("VerifyOtp", new { email = user.Email });

        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Register");
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otpCode)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập mã OTP.");
                ViewBag.Email = email;
                return View();
            }

            bool isValid = _otpService.ValidateOtp(email.ToLower(), otpCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
                ViewBag.Email = email;
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy người dùng.");
                ViewBag.Email = email;
                return View();
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("RegisterSuccess");
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
        // ĐĂNG NHẬP BẰNG GOOGLE (CLEAN ARCHITECTURE)
        // ==========================================

        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                result = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
            }

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var nameIdentifier = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy email từ Google.");
                return RedirectToAction("Login");
            }

            var user = await _googleAuthService.ProcessGoogleLoginAsync(email, name ?? "Người dùng Google", nameIdentifier ?? "");

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
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

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
