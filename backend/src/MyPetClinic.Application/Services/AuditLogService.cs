using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ILogger<AuditLogService> logger)
        {
            _logger = logger;
        }

        public Task LogActionAsync(string userId, string action, string details)
        {
            _logger.LogInformation("AUDIT LOG: User {UserId} performed action {Action}. Details: {Details}", userId, action, details);
            return Task.CompletedTask;
        }
    }
}
