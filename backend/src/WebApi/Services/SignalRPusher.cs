using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MyPetClinic.Application.Interfaces.Services;
using WebApi.Hubs;

namespace WebApi.Services
{
    public class SignalRPusher : ISignalRPusher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRPusher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationAsync(Guid userId, string message)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
        }
    }
}
