using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogActionAsync(string userId, string action, string details);
    }
}
