using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.DTOs;
using System.Security.Claims;


namespace MyPetClinic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGoogleAuthService _googleAuthService;

        public AccountController(IAuthService authService, IGoogleAuthService googleAuthService)
        {
            _authService = authService;
            _googleAuthService = googleAuthService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true, email = result.Email, message = "Đăng ký thành công. Vui lòng kiểm tra email để nhận mã OTP." });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest req)
        {
            if (string.IsNullOrEmpty(req.Email)) return BadRequest(new { message = "Email không hợp lệ." });

            var result = await _authService.ResendOtpAsync(req.Email, req.Type);

            if (!result.Success) return BadRequest(new { message = "Không thể gửi lại mã OTP." });

            return Ok(new { success = true, email = result.Email, message = "Đã gửi lại mã OTP mới vào email của bạn." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
        {
            if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.OtpCode))
                return BadRequest(new { message = "Vui lòng nhập email và mã OTP." });

            var result = await _authService.VerifyOtpAsync(req.Email, req.OtpCode);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage ?? "Lỗi xác thực." });

            return Ok(new { success = true, message = "Xác thực OTP thành công." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);

            if (!result.Success)
            {
                if (result.RequiresOtp)
                {
                    return BadRequest(new { requiresOtp = true, email = result.Email, message = "Tài khoản chưa kích hoạt. Chúng tôi vừa gửi lại mã OTP mới." });
                }
                return BadRequest(new { message = result.ErrorMessage ?? "Đăng nhập thất bại." });
            }

            var claimsIdentity = new ClaimsIdentity(result.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return Ok(new { success = true, message = "Đăng nhập thành công." });
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
            return Challenge(properties, Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                result = await HttpContext.AuthenticateAsync(Microsoft.AspNetCore.Authentication.Google.GoogleDefaults.AuthenticationScheme);
            }

            if (!result.Succeeded)
            {
                return Redirect("http://localhost:5173/login?error=GoogleAuthFailed");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var nameIdentifier = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Redirect("http://localhost:5173/login?error=NoEmail");
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

            return Redirect("http://localhost:5173/dashboard"); // Redirect to SPA
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            if (string.IsNullOrEmpty(req.Email)) return BadRequest(new { message = "Vui lòng nhập địa chỉ Email." });

            var result = await _authService.ForgotPasswordAsync(req.Email);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage ?? "Lỗi" });

            return Ok(new { success = true, email = result.Email, message = "Đã gửi mã OTP đặt lại mật khẩu." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.OtpCode) || string.IsNullOrEmpty(req.NewPassword))
                return BadRequest(new { message = "Vui lòng điền đầy đủ thông tin." });

            var result = await _authService.ResetPasswordAsync(req.Email, req.OtpCode, req.NewPassword, req.ConfirmPassword);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage ?? "Lỗi đặt lại mật khẩu." });

            return Ok(new { success = true, message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "Đã đăng xuất." });
        }
    }

    public class ResendOtpRequest
    {
        public required string Email { get; set; }
        public required string Type { get; set; }
    }

    public class VerifyOtpRequest
    {
        public required string Email { get; set; }
        public required string OtpCode { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required string OtpCode { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
}
