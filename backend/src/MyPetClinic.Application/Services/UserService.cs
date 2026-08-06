using System;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Avatar = user.Avatar,
                RoleName = user.Role?.Name
            };
        }

        public async Task<bool> UpdateUserProfileAsync(Guid userId, UpdateProfileDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != user.Phone)
            {
                string normalizedPhone = dto.Phone.Trim();
                string legacyPhone = normalizedPhone.StartsWith("+84") ? "0" + normalizedPhone.Substring(3) : normalizedPhone;

                var usersWithSamePhone = await _unitOfWork.Users.FindAsync(u => 
                    u.Id != userId && (u.Phone == normalizedPhone || u.Phone == legacyPhone) && u.IsActive == true);
                
                if (usersWithSamePhone.Any())
                {
                    throw new InvalidOperationException("Số điện thoại này đã được sử dụng bởi tài khoản khác.");
                }
            }

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.Address = dto.Address;
            user.Gender = dto.Gender;
            
            // Fix Npgsql DateTime issue by ensuring UTC
            if (dto.DateOfBirth.HasValue)
            {
                user.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc);
            }
            else
            {
                user.DateOfBirth = null;
            }

            if (user.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(user.CustomerId.Value);
                if (customer != null)
                {
                    customer.FullName = dto.FullName;
                    customer.Phone = dto.Phone;
                    customer.Address = dto.Address;
                    customer.Gender = dto.Gender;
                    customer.DateOfBirth = user.DateOfBirth;
                    _unitOfWork.Customers.Update(customer);
                }
            }
            else if (user.CustomerProfile != null)
            {
                user.CustomerProfile.FullName = dto.FullName;
                user.CustomerProfile.Phone = dto.Phone;
                user.CustomerProfile.Address = dto.Address;
                user.CustomerProfile.Gender = dto.Gender;
                user.CustomerProfile.DateOfBirth = user.DateOfBirth;
            }

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return false;

            // Verify current password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid) return false;

            // Hash new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAvatarAsync(Guid userId, string avatarPath)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.Avatar = avatarPath;

            await _userRepository.UpdateUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
    }
}
