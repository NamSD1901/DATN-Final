using MyPetClinic.Domain.Entities;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task CreateUserAsync(User user);
        Task CreateRoleAsync(Role role);
        Task SaveChangesAsync();
    }
}
