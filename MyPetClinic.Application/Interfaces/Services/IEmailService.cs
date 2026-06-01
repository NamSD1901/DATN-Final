using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
