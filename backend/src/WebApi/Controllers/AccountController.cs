using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
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
        private readonly MyPetClinic.Application.Interfaces.Repositories.IUserRepository _userRepository;
        private readonly MyPetClinic.Application.Interfaces.Repositories.IUnitOfWork _unitOfWork;

        public AccountController(
            IAuthService authService, 
            IGoogleAuthService googleAuthService,
            MyPetClinic.Application.Interfaces.Repositories.IUserRepository userRepository,
            MyPetClinic.Application.Interfaces.Repositories.IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _googleAuthService = googleAuthService;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true, email = result.Email, message = "Đăng ký thành công. Vui lòng kiểm tra email để nhận mã OTP." });
        }

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAccount([FromBody] ActivateAccountRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ActivateAccountAsync(request);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { success = true, message = "Kích hoạt tài khoản thành công. Bạn có thể đăng nhập ngay bây giờ." });
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

            if (result.RequiresClaiming)
            {
                return Ok(new 
                { 
                    success = true, 
                    requiresClaiming = true, 
                    hasPets = result.HasPets, 
                    tempToken = result.TempToken,
                    message = "Vui lòng xác minh hồ sơ khách hàng của bạn."
                });
            }

            return Ok(new { success = true, requiresClaiming = false, message = "Xác thực OTP thành công." });
        }

        [HttpPost("claim-profile")]
        public async Task<IActionResult> ClaimProfile([FromBody] ClaimProfileDto req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ClaimProfileAsync(req);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage ?? "Xác minh thất bại." });

            return Ok(new { success = true, message = "Đồng bộ hồ sơ thành công." });
        }

        [HttpPost("skip-claim")]
        public async Task<IActionResult> SkipClaim([FromBody] SkipClaimDto req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.SkipClaimingAsync(req);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage ?? "Có lỗi xảy ra." });

            return Ok(new { success = true, message = "Tạo hồ sơ mới thành công." });
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
                new Claim(ClaimTypes.Role, user.RoleName ?? "customer")
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

        /// <summary>
        /// Tự động tạo Customer record nếu user đăng nhập chưa có CustomerId.
        /// Gọi ngay sau khi đăng nhập thành công để sửa các tài khoản cũ bị lỗi.
        /// </summary>
        [HttpPost("ensure-profile")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "customer")]
        public async Task<IActionResult> EnsureProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Không tìm thấy thông tin xác thực." });

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            // Nếu đã có CustomerId thì không cần làm gì
            if (user.CustomerId.HasValue)
                return Ok(new { success = true, customerId = user.CustomerId.Value, message = "Hồ sơ đã tồn tại." });

            // Tự động tạo Customer mới
            var newCustomer = new MyPetClinic.Domain.Entities.Customer
            {
                Id = Guid.NewGuid(),
                CustomerCode = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss"),
                FullName = user.FullName ?? user.Email,
                Phone = user.Phone,
                Email = user.Email,
                Address = user.Address,
                HasAccount = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Customers.AddAsync(newCustomer);
            user.CustomerId = newCustomer.Id;
            await _userRepository.UpdateUserAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();

            return Ok(new { success = true, customerId = newCustomer.Id, message = "Tạo hồ sơ khách hàng thành công." });
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
