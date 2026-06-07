using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Models;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.DTOs;
using System.Security.Claims;


namespace MyPetClinic.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IGoogleAuthService _googleAuthService;

        public AccountController(IAuthService authService, IGoogleAuthService googleAuthService)
        {
            _authService = authService;
            _googleAuthService = googleAuthService;
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

            var registerDto = new RegisterDto
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Password = model.Password,
                Address = model.Address
            };

            var result = await _authService.RegisterAsync(registerDto);
            if (!result.Success)
            {
                ModelState.AddModelError(result.ErrorMessage == "Email này đã được sử dụng trong hệ thống." ? "Email" : string.Empty, result.ErrorMessage ?? "Lỗi đăng ký.");
                return View(model);
            }

            return RedirectToAction("VerifyOtp", new { email = result.Email });

        }

        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email, string type)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var result = await _authService.ResendOtpAsync(email, type);

            if (!result.Success)
                return RedirectToAction("Login");

            TempData["SuccessMessage"] = "Đã gửi lại mã OTP mới vào email của bạn.";
            string redirectAction = type == "register" ? "VerifyOtp" : "ResetPassword";
            return RedirectToAction(redirectAction, new { email = result.Email });
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

            var result = await _authService.VerifyOtpAsync(email, otpCode);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi xác thực.");
                ViewBag.Email = email;
                return View();
            }

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

            var loginDto = new LoginDto
            {
                Email = model.Email,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                if (result.RequiresOtp)
                {
                    TempData["SuccessMessage"] = "Tài khoản chưa kích hoạt. Chúng tôi vừa gửi lại mã OTP mới vào email của bạn.";
                    return RedirectToAction("VerifyOtp", new { email = result.Email });
                }
                
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Đăng nhập thất bại.");
                return View(model);
            }

            var claimsIdentity = new ClaimsIdentity(result.Claims, CookieAuthenticationDefaults.AuthenticationScheme);

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

            return RedirectToAction("Index", "Dashboard");
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

            return RedirectToAction("Index", "Dashboard");
        }

        // ==========================================
        // 3. QUÊN MẬT KHẨU (FORGOT PASSWORD)
        // ==========================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập địa chỉ Email.");
                return View();
            }

            var result = await _authService.ForgotPasswordAsync(email);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi");
                return View();
            }

            return RedirectToAction("ResetPassword", new { email = result.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string otpCode, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ thông tin.");
                return View();
            }

            var result = await _authService.ResetPasswordAsync(email, otpCode, newPassword, confirmPassword);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Lỗi đặt lại mật khẩu.");
                return View();
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        // ==========================================
        // 4. ĐĂNG XUẤT (LOGOUT)
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

        // ==========================================
        // UTILS
        // ==========================================


    }
}
