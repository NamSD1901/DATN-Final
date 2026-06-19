using System;
using System.Threading.Tasks;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface ISignalRPusher
    {
        Task SendNotificationAsync(Guid userId, string message);
    }
}
