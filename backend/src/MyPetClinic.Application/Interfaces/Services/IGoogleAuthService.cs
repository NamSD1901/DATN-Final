using MyPetClinic.Application.DTOs;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IGoogleAuthService
    {
        Task<UserProfileDto> ProcessGoogleLoginAsync(string email, string fullName, string providerKey);
    }
}
