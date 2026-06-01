using System;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(Guid userId);
        Task<bool> UpdateUserProfileAsync(Guid userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<bool> UpdateAvatarAsync(Guid userId, string avatarPath);
    }
}
