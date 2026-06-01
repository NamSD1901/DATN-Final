using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MyPetClinic.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await Task.CompletedTask;
        }

        public async Task CreateRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(long roleId)
        {
            return await _context.Users
                .Where(u => u.RoleId == roleId && u.IsActive && u.DeletedAt == null)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string keyword, long? roleId = null)
        {
            var query = _context.Users.Where(u => u.IsActive && u.DeletedAt == null).AsQueryable();
            
            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(u => 
                    (u.FullName != null && u.FullName.ToLower().Contains(keyword)) ||
                    (u.Phone != null && u.Phone.Contains(keyword)) ||
                    (u.Email != null && u.Email.ToLower().Contains(keyword))
                );
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task SoftDeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.DeletedAt = System.DateTime.UtcNow;
                user.IsActive = false; // Tùy logic, nếu đã xóa thì deactive luôn
                _context.Users.Update(user);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
