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

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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
