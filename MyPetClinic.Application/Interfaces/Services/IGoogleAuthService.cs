using MyPetClinic.Domain.Entities;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IGoogleAuthService
    {
        Task<User> ProcessGoogleLoginAsync(string email, string fullName, string providerKey);
    }
}
