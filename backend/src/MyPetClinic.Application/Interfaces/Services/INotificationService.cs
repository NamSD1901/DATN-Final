using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs.Notification;

namespace MyPetClinic.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(long notificationId, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task CreateNotificationAsync(Guid userId, string title, string content, string type);
    }
}
