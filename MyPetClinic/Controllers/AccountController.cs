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
            string emailBody = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, "Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi. Để hoàn tất việc đăng ký, vui lòng nhập mã xác thực (OTP) bên dưới:");
            
            await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản MyPetClinic", emailBody);

            return RedirectToAction("VerifyOtp", new { email = user.Email });

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
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            string emailKey = "";
            string emailTitle = "";
            string messageBody = "";
            string redirectAction = "";

            if (type == "register")
            {
                if (user.IsActive) return RedirectToAction("Login");
                emailKey = user.Email.ToLower();
                emailTitle = "Xác thực tài khoản MyPetClinic";
                messageBody = "Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi. Để hoàn tất việc đăng ký, vui lòng nhập mã xác thực (OTP) mới bên dưới:";
                redirectAction = "VerifyOtp";
            }
            else if (type == "forgot")
            {
                if (!user.IsActive) return RedirectToAction("Login");
                emailKey = "reset_" + user.Email.ToLower();
                emailTitle = "Yêu cầu đặt lại mật khẩu MyPetClinic";
                messageBody = "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác thực (OTP) mới bên dưới để tiến hành đổi mật khẩu:";
                redirectAction = "ResetPassword";
            }
            else
            {
                return RedirectToAction("Login");
            }

            string otp = _otpService.GenerateOtp(emailKey);
            string emailHtml = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, messageBody);
            await _emailService.SendEmailAsync(user.Email, emailTitle, emailHtml);

            TempData["SuccessMessage"] = "Đã gửi lại mã OTP mới vào email của bạn.";
            return RedirectToAction(redirectAction, new { email = user.Email });
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
                // Tài khoản chưa kích hoạt -> Gửi lại OTP và chuyển đến trang nhập OTP
                string emailKey = user.Email!.ToLower();
                string otp = _otpService.GenerateOtp(emailKey);
                string emailBody = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, "Tài khoản của bạn chưa được kích hoạt. Vui lòng sử dụng mã xác thực (OTP) bên dưới để tiến hành kích hoạt tài khoản:");
                await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản MyPetClinic", emailBody);

                TempData["SuccessMessage"] = "Tài khoản chưa kích hoạt. Chúng tôi vừa gửi lại mã OTP mới vào email của bạn.";
                return RedirectToAction("VerifyOtp", new { email = user.Email });
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

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Email không hợp lệ hoặc tài khoản chưa kích hoạt.");
                return View();
            }

            string emailKey = "reset_" + user.Email.ToLower();
            string otp = _otpService.GenerateOtp(emailKey);
            string emailBody = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác thực (OTP) bên dưới để tiến hành đổi mật khẩu mới:");

            await _emailService.SendEmailAsync(user.Email, "Yêu cầu đặt lại mật khẩu MyPetClinic", emailBody);

            return RedirectToAction("ResetPassword", new { email = user.Email });
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

            if (newPassword.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới phải có tối thiểu 8 ký tự.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
                return View();
            }

            bool isValid = _otpService.ValidateOtp("reset_" + email.ToLower(), otpCode);
            if (!isValid)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không hợp lệ hoặc đã hết hạn.");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy người dùng.");
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

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

        private string GenerateOtpEmailHtml(string fullName, string otp, string messageBody)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>MyPetClinic OTP</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f6f9; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 10px; border-top: 5px solid #f1c40f; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 20px;'>
            <h2 style='color: #2c3e50; margin: 0; font-size: 28px;'>MyPet<span style='color: #f1c40f;'>Clinic</span></h2>
        </div>
        <h3 style='color: #2c3e50; font-size: 18px;'>Xin chào {fullName},</h3>
        <p style='color: #555; line-height: 1.6; font-size: 15px;'>{messageBody}</p>
        <div style='text-align: center; margin: 30px 0;'>
            <div style='display: inline-block; padding: 15px 40px; background-color: #fef9e7; border: 2px dashed #f1c40f; border-radius: 8px; font-size: 32px; font-weight: bold; color: #d4ac0d; letter-spacing: 8px;'>
                {otp}
            </div>
        </div>
        <p style='color: #555; line-height: 1.6; font-size: 15px;'>Mã OTP này sẽ hết hạn trong vòng <strong>5 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai để đảm bảo an toàn.</p>
        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0 20px 0;'>
        <p style='color: #95a5a6; font-size: 13px; text-align: center; margin: 0;'>Email này được gửi tự động từ hệ thống MyPetClinic.<br>Vui lòng không trả lời thư này.</p>
    </div>
</body>
</html>";
        }
    }
}
