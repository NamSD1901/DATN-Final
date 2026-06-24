using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System.Linq;

namespace MyPetClinic.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, IEmailService emailService, IOtpService otpService, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _otpService = otpService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResult> RegisterAsync(RegisterDto model)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(model.Email.Trim().ToLower());
            if (existingUser != null)
            {
                if (existingUser.IsActive == true)
                {
                    return new AuthResult { Success = false, ErrorMessage = "Email này đã được sử dụng trong hệ thống." };
                }
                else
                {
                    await _userRepository.HardDeleteUserAsync(existingUser);
                    await _userRepository.SaveChangesAsync();
                }
            }

            var customerRole = await _userRepository.GetRoleByNameAsync("customer");
            if (customerRole == null)
            {
                customerRole = new Role { Name = "customer" };
                await _userRepository.CreateRoleAsync(customerRole);
                await _userRepository.SaveChangesAsync();
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

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

            await _userRepository.CreateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            string emailKey = user.Email.ToLower();
            string otp = _otpService.GenerateOtp(emailKey);
            string emailBody = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, "Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi. Để hoàn tất việc đăng ký, vui lòng nhập mã xác thực (OTP) bên dưới:");
            
            await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản MyPetClinic", emailBody);

            return new AuthResult { Success = true, Email = user.Email };
        }

        public async Task<AuthResult> ResendOtpAsync(string email, string type)
        {
            var user = await _userRepository.GetUserByEmailAsync(email.ToLower());
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Không tìm thấy người dùng." };

            string emailKey = "";
            string emailTitle = "";
            string messageBody = "";

            if (type == "register")
            {
                if (user.IsActive == true) return new AuthResult { Success = false, ErrorMessage = "Tài khoản đã kích hoạt." };
                emailKey = user.Email!.ToLower();
                emailTitle = "Xác thực tài khoản MyPetClinic";
                messageBody = "Cảm ơn bạn đã đăng ký tài khoản tại hệ thống của chúng tôi. Để hoàn tất việc đăng ký, vui lòng nhập mã xác thực (OTP) mới bên dưới:";
            }
            else if (type == "forgot")
            {
                if (user.IsActive != true) return new AuthResult { Success = false, ErrorMessage = "Tài khoản chưa kích hoạt." };
                emailKey = "reset_" + user.Email!.ToLower();
                emailTitle = "Yêu cầu đặt lại mật khẩu MyPetClinic";
                messageBody = "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác thực (OTP) mới bên dưới để tiến hành đổi mật khẩu:";
            }
            else
            {
                return new AuthResult { Success = false, ErrorMessage = "Loại yêu cầu không hợp lệ." };
            }

            string otp = _otpService.GenerateOtp(emailKey);
            string emailHtml = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, messageBody);
            await _emailService.SendEmailAsync(user.Email!, emailTitle, emailHtml);

            return new AuthResult { Success = true, Email = user.Email };
        }

        public async Task<AuthResult> VerifyOtpAsync(string email, string otpCode)
        {
            bool isValid = _otpService.ValidateOtp(email.ToLower(), otpCode);
            if (!isValid)
                return new AuthResult { Success = false, ErrorMessage = "Mã OTP không hợp lệ hoặc đã hết hạn." };

            var user = await _userRepository.GetUserByEmailAsync(email.ToLower());
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Không tìm thấy người dùng." };

            user.IsActive = true;

            // PHẦN 10: AUTO-LINK ZALO/SĐT
            // Kiểm tra xem có Customer nào có cùng SĐT không, nếu có thì liên kết
            if (!string.IsNullOrWhiteSpace(user.Phone))
            {
                string phone = user.Phone.Trim();
                var customers = await _unitOfWork.Customers.FindAsync(c => c.Phone == phone);
                var existingCustomer = customers.FirstOrDefault();

                if (existingCustomer != null && !existingCustomer.HasAccount)
                {
                    // 2FA CLAIMING: Stop Auto-Link! Require user to claim the profile.
                    await _userRepository.UpdateUserAsync(user);
                    await _userRepository.SaveChangesAsync();

                    var pets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == existingCustomer.Id);
                    bool hasPets = pets.Any();

                    return new AuthResult 
                    { 
                        Success = true, 
                        RequiresClaiming = true, 
                        HasPets = hasPets, 
                        TempToken = "CLAIM_TOKEN"
                    };
                }
                else if (existingCustomer == null || existingCustomer.HasAccount)
                {
                    // Tự động sinh hồ sơ Customer mới cho khách hàng đăng ký Online
                    // (kể cả khi customer đã có tài khoản khác hoặc không có customer nào trùng SĐT)
                    if (user.CustomerId == null)
                    {
                        var newCustomer = new MyPetClinic.Domain.Entities.Customer
                        {
                            Id = Guid.NewGuid(),
                            CustomerCode = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss"),
                            FullName = user.FullName ?? user.Email,
                            Phone = phone,
                            Email = user.Email,
                            Address = user.Address,
                            HasAccount = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Customers.AddAsync(newCustomer);
                        user.CustomerId = newCustomer.Id;
                    }
                }
            }
            else
            {
                // Không có SĐT: vẫn tạo Customer mới để đảm bảo CustomerId không null
                if (user.CustomerId == null)
                {
                    var newCustomer = new MyPetClinic.Domain.Entities.Customer
                    {
                        Id = Guid.NewGuid(),
                        CustomerCode = "CUS" + DateTime.UtcNow.ToString("yyMMddHHmmss"),
                        FullName = user.FullName ?? user.Email,
                        Phone = null,
                        Email = user.Email,
                        Address = user.Address,
                        HasAccount = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Customers.AddAsync(newCustomer);
                    user.CustomerId = newCustomer.Id;
                }
            }

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();

            return new AuthResult { Success = true };
        }

        public async Task<AuthResult> LoginAsync(LoginDto model)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Email.Trim().ToLower());

            // Tài khoản bị xóa mềm (soft-delete) → không được đăng nhập dù mật khẩu đúng
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || user.DeletedAt != null)
                return new AuthResult { Success = false, ErrorMessage = "Email hoặc mật khẩu không chính xác." };

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
                return new AuthResult { Success = false, ErrorMessage = "Email hoặc mật khẩu không chính xác." };

            if (user.IsActive != true)
            {
                string emailKey = user.Email!.ToLower();
                string otp = _otpService.GenerateOtp(emailKey);
                string emailBody = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, "Tài khoản của bạn chưa được kích hoạt. Vui lòng sử dụng mã xác thực (OTP) bên dưới để tiến hành kích hoạt tài khoản:");
                await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản MyPetClinic", emailBody);

                return new AuthResult { Success = false, RequiresOtp = true, Email = user.Email };
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Email ?? "Khách Hàng"),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "customer")
            };

            return new AuthResult { Success = true, Claims = claims };
        }

        public async Task<AuthResult> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email.Trim().ToLower());
            if (user == null || user.IsActive != true)
                return new AuthResult { Success = false, ErrorMessage = "Email không hợp lệ hoặc tài khoản chưa kích hoạt." };

            string emailKey = "reset_" + user.Email!.ToLower();
            string otp = _otpService.GenerateOtp(emailKey);
            string emailBody = GenerateOtpEmailHtml(user.FullName ?? "Khách hàng", otp, "Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn. Vui lòng sử dụng mã xác thực (OTP) bên dưới để tiến hành đổi mật khẩu mới:");

            await _emailService.SendEmailAsync(user.Email, "Yêu cầu đặt lại mật khẩu MyPetClinic", emailBody);

            return new AuthResult { Success = true, Email = user.Email };
        }

        public async Task<AuthResult> ResetPasswordAsync(string email, string otpCode, string newPassword, string confirmPassword)
        {
            if (newPassword.Length < 8)
                return new AuthResult { Success = false, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự." };

            if (newPassword != confirmPassword)
                return new AuthResult { Success = false, ErrorMessage = "Mật khẩu xác nhận không khớp." };

            bool isValid = _otpService.ValidateOtp("reset_" + email.ToLower(), otpCode);
            if (!isValid)
                return new AuthResult { Success = false, ErrorMessage = "Mã OTP không hợp lệ hoặc đã hết hạn." };

            var user = await _userRepository.GetUserByEmailAsync(email.ToLower());
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Không tìm thấy người dùng." };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResult { Success = true };
        }

        public async Task<AuthResult> ActivateAccountAsync(ActivateAccountRequest request)
        {
            var invitations = await _unitOfWork.Invitations.FindWithIncludesAsync(i => i.Token == request.Token, i => i.User!);
            var invitation = invitations.FirstOrDefault();

            if (invitation == null || invitation.IsUsed)
                return new AuthResult { Success = false, ErrorMessage = "Token không hợp lệ hoặc đã được sử dụng." };

            if (invitation.ExpireAt < DateTime.UtcNow)
                return new AuthResult { Success = false, ErrorMessage = "Token đã hết hạn." };

            var user = invitation.User;
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Không tìm thấy người dùng." };

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.IsActive = true;
            
            invitation.IsUsed = true;

            _unitOfWork.Invitations.Update(invitation);
            await _userRepository.UpdateUserAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResult { Success = true };
        }

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
        // 2FA CLAIMING METHODS
        public async Task<AuthResult> ClaimProfileAsync(ClaimProfileDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email.ToLower());
            if (user == null || !user.IsActive.GetValueOrDefault())
                return new AuthResult { Success = false, ErrorMessage = "Tài khoản không hợp lệ hoặc chưa được xác thực." };

            if (user.CustomerId != null)
                return new AuthResult { Success = false, ErrorMessage = "Tài khoản này đã có hồ sơ." };

            string phone = user.Phone?.Trim() ?? "";
            if (string.IsNullOrEmpty(phone))
                return new AuthResult { Success = false, ErrorMessage = "Không thể xác định số điện thoại." };

            var customers = await _unitOfWork.Customers.FindAsync(c => c.Phone == phone);
            var existingCustomer = customers.FirstOrDefault();

            if (existingCustomer == null)
                return new AuthResult { Success = false, ErrorMessage = "Không tìm thấy hồ sơ vãng lai khớp với SĐT này." };

            // 2FA Validation 1: Customer Code (Must be exactly matched)
            if (string.IsNullOrEmpty(request.CustomerCode) || !existingCustomer.CustomerCode.Equals(request.CustomerCode, StringComparison.OrdinalIgnoreCase))
                return new AuthResult { Success = false, ErrorMessage = "Mã Khách Hàng không chính xác." };

            var pets = await _unitOfWork.Pets.FindAsync(p => p.CustomerId == existingCustomer.Id && !p.IsDeceased);
            
            // 2FA Validation 2: Pet Name (If customer has pets)
            if (pets.Any())
            {
                if (string.IsNullOrWhiteSpace(request.PetName))
                    return new AuthResult { Success = false, ErrorMessage = "Vui lòng nhập tên một bé thú cưng." };

                string inputPetName = request.PetName.Trim().ToLower();
                bool hasMatchedPet = pets.Any(p => p.Name != null && p.Name.Trim().ToLower() == inputPetName);
                
                if (!hasMatchedPet)
                    return new AuthResult { Success = false, ErrorMessage = "Tên thú cưng không chính xác." };
            }

            // Both checks passed! Link the profile.
            user.CustomerId = existingCustomer.Id;
            existingCustomer.HasAccount = true;
            
            _unitOfWork.Customers.Update(existingCustomer);
            await _userRepository.UpdateUserAsync(user);
            
            await _unitOfWork.SaveChangesAsync();
            await _userRepository.SaveChangesAsync();

            return new AuthResult { Success = true };
        }

        public async Task<AuthResult> SkipClaimingAsync(SkipClaimDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email.ToLower());
            if (user == null || !user.IsActive.GetValueOrDefault())
                return new AuthResult { Success = false, ErrorMessage = "Tài khoản không hợp lệ hoặc chưa được xác thực." };

            if (user.CustomerId != null)
                return new AuthResult { Success = false, ErrorMessage = "Tài khoản này đã có hồ sơ." };

            // Sinh hồ sơ Customer mới trống trơn
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
            
            await _unitOfWork.SaveChangesAsync();
            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResult { Success = true };
        }
    }
}
