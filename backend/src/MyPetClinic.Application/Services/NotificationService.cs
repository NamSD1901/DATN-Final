using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs.Notification;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;

namespace MyPetClinic.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISignalRPusher _signalRPusher;

        public NotificationService(IUnitOfWork unitOfWork, ISignalRPusher signalRPusher)
        {
            _unitOfWork = unitOfWork;
            _signalRPusher = signalRPusher;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId);
            
            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Only take top 50 recent
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt
                });
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
            return notifications.Count();
        }

        public async Task<bool> MarkAsReadAsync(long notificationId, Guid userId)
        {
            var notification = await _unitOfWork.Notifications.GetFirstOrDefaultWithIncludesAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null) return false;

            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);

            if (!unreadNotifications.Any()) return true;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task CreateNotificationAsync(Guid userId, string title, string content, string type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            // Push SignalR message to the client
            await _signalRPusher.SendNotificationAsync(userId, $"Bạn có một thông báo mới: {title}");
        }
    }
}
