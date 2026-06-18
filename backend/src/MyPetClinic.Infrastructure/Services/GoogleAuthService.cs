using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IUserRepository _userRepository;

        public GoogleAuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserProfileDto> ProcessGoogleLoginAsync(string email, string fullName, string providerKey)
        {
            // Kiểm tra xem user đã tồn tại theo email chưa
            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                // Tìm role customer
                var customerRole = await _userRepository.GetRoleByNameAsync("customer");
                if (customerRole == null)
                {
                    customerRole = new Role { Name = "customer" };
                    await _userRepository.CreateRoleAsync(customerRole);
                    await _userRepository.SaveChangesAsync();
                }

                // Nếu là đăng nhập bằng Google lần đầu, tạo tài khoản mới
                // Mật khẩu sẽ được để null hoặc chuỗi ngẫu nhiên (chỉ cho phép đăng nhập qua Google trừ khi đổi pass)
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    RoleId = customerRole.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = string.Empty // Không có password
                };

                await _userRepository.CreateUserAsync(user);
                await _userRepository.SaveChangesAsync();
                
                // Gán lại role cho user mới tạo để có thể lấy Claim (vì EF chưa load Role Navigation)
                user.Role = customerRole; 
            }

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role?.Name
            };
        }
    }
}
