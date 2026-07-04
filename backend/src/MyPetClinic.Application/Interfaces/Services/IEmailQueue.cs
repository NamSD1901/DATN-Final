using System.Threading;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs.Notification;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface IEmailQueue
    {
        ValueTask QueueEmailAsync(EmailMessageDto emailMessage);
        ValueTask<EmailMessageDto> DequeueEmailAsync(CancellationToken cancellationToken);
    }
}
