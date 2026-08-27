using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IEmailService _emailService;

        public EmployeeService(IUnitOfWork unitOfWork, ILogger<EmployeeService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
        {
            var users = await _unitOfWork.Users.FindWithIncludesAsync(u => u.EmployeeProfile != null, u => u.Role!, u => u.EmployeeProfile!);
            return users.OrderByDescending(u => u.CreatedAt).Select(u => MapToDto(u, u.EmployeeProfile!, u.Role?.Name ?? ""));
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingEmail = await _unitOfWork.Users.AnyAsync(u => u.Email == request.Email);
                if (existingEmail) throw new Exception("Email đã tồn tại trong hệ thống.");
                    
                var existingPhone = await _unitOfWork.Users.AnyAsync(u => u.Phone == request.Phone);
                if (existingPhone) throw new Exception("Số điện thoại đã tồn tại trong hệ thống.");

                var validRoles = new[] { "admin", "clinical_doctor", "vaccination_doctor", "receptionist" };
                if (!validRoles.Contains(request.RoleName))
                    throw new Exception("Chức vụ không hợp lệ.");

                var roles = await _unitOfWork.Roles.FindAsync(r => r.Name == request.RoleName);
                var role = roles.FirstOrDefault();
                if (role == null) throw new Exception("Role không tồn tại trong Database.");

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Gender = request.Gender,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    RoleId = role.Id,
                    IsActive = false // Cần kích hoạt
                };
                await _unitOfWork.Users.AddAsync(user);

                var profile = new EmployeeProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    IdentityCard = request.IdentityCard,
                    Position = request.RoleName
                };
                await _unitOfWork.EmployeeProfiles.AddAsync(profile);

                var token = Guid.NewGuid().ToString("N");
                var invitation = new Invitation
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = token,
                    ExpireAt = DateTime.UtcNow.AddHours(24)
                };
                await _unitOfWork.Invitations.AddAsync(invitation);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var link = $"http://localhost:5173/activate?token={token}";
                var emailHtml = GenerateActivationEmailHtml(user.FullName ?? "Nhân viên", link);
                await _emailService.SendEmailAsync(user.Email, "Thiết lập mật khẩu và Kích hoạt tài khoản MyPetClinic", emailHtml);

                return MapToDto(user, profile, role.Name);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw; // Rethrow to let controller handle it
            }
        }

        public async Task<EmployeeDto> UpdateEmployeeAsync(Guid id, UpdateEmployeeRequest request)
        {
            var users = await _unitOfWork.Users.FindWithIncludesAsync(u => u.Id == id && u.EmployeeProfile != null, u => u.Role!, u => u.EmployeeProfile!);
            var user = users.FirstOrDefault();

            if (user == null) throw new Exception("Không tìm thấy nhân viên.");

            if (user.Phone != request.Phone)
            {
                var existingPhone = await _unitOfWork.Users.AnyAsync(u => u.Phone == request.Phone);
                if (existingPhone) throw new Exception("Số điện thoại đã tồn tại.");
            }

            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;
            user.Address = request.Address;

            user.EmployeeProfile!.IsResigned = request.IsResigned;
            if (request.IsResigned)
            {
                user.IsActive = false; // Khóa tài khoản
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(user, user.EmployeeProfile, user.Role?.Name ?? "");
        }

        public async Task<bool> ResendActivationEmailAsync(Guid id)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Id == id);
            var user = users.FirstOrDefault();
            if (user == null || user.IsActive == true) return false;

            // Vô hiệu hóa token cũ
            var oldTokens = await _unitOfWork.Invitations.FindAsync(i => i.UserId == id && !i.IsUsed);
            foreach (var t in oldTokens) 
            {
                t.IsUsed = true;
                _unitOfWork.Invitations.Update(t);
            }

            // Tạo token mới
            var token = Guid.NewGuid().ToString("N");
            await _unitOfWork.Invitations.AddAsync(new Invitation
            {
                Id = Guid.NewGuid(),
                UserId = id,
                Token = token,
                ExpireAt = DateTime.UtcNow.AddHours(24)
            });

            await _unitOfWork.SaveChangesAsync();

            var link = $"http://localhost:5173/activate?token={token}";
            var emailHtml = GenerateActivationEmailHtml(user.FullName ?? "Nhân viên", link);
            await _emailService.SendEmailAsync(user.Email!, "Thiết lập mật khẩu và Kích hoạt tài khoản MyPetClinic", emailHtml);

            return true;
        }

        private EmployeeDto MapToDto(User user, EmployeeProfile profile, string roleName)
        {
            return new EmployeeDto

        
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                Phone = user.Phone ?? "",
                Avatar = user.Avatar,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                IsActive = user.IsActive ?? false,
                RoleName = roleName,
                IdentityCard = profile.IdentityCard ?? "",
                Position = profile.Position,
                IsResigned = profile.IsResigned
            };
        }

        private string GenerateActivationEmailHtml(string fullName, string activationLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Kích hoạt tài khoản MyPetClinic</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f6f9; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 10px; border-top: 5px solid #f1c40f; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 20px;'>
            <h2 style='color: #2c3e50; margin: 0; font-size: 28px;'>MyPet<span style='color: #f1c40f;'>Clinic</span></h2>
        </div>
        <h3 style='color: #2c3e50; font-size: 18px;'>Xin chào {fullName},</h3>
        <p style='color: #555; line-height: 1.6; font-size: 15px;'>Bạn đã được thêm vào hệ thống quản lý của MyPetClinic. Để bắt đầu sử dụng, vui lòng thiết lập mật khẩu mới bằng cách nhấp vào nút bên dưới:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{activationLink}' style='display: inline-block; padding: 15px 40px; background-color: #f1c40f; color: #fff; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold;'>Thiết lập mật khẩu & Kích hoạt</a>
        </div>
        <p style='color: #555; line-height: 1.6; font-size: 15px;'>Link kích hoạt này sẽ hết hạn trong vòng <strong>24 giờ</strong>. Vui lòng không chia sẻ link này với bất kỳ ai để đảm bảo an toàn.</p>
        <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'>
        <p style='color: #999; font-size: 13px; text-align: center; margin: 0;'>&copy; {DateTime.Now.Year} MyPetClinic. Mọi quyền được bảo lưu.</p>
    </div>
</body>
</html>";
        }
    }
}
