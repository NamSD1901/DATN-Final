using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.DTOs.Notification;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Workers
{
    public class OverdueAppointmentCleanerWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueAppointmentCleanerWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public OverdueAppointmentCleanerWorker(IServiceProvider serviceProvider, ILogger<OverdueAppointmentCleanerWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Worker dọn dẹp lịch hẹn quá hạn đã khởi động.");

            // Đợi một chút khi ứng dụng khởi động
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanUpOverdueAppointmentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong tiến trình chạy ngầm dọn dẹp lịch hẹn.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        public async Task CleanUpOverdueAppointmentsAsync(CancellationToken stoppingToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var emailQueue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();

            var today = DateTime.UtcNow.Date;

            // Tìm các lịch hẹn chờ duyệt mà ngày hẹn đã qua
            var overdueAppointments = await unitOfWork.Appointments.Query()
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Where(a => a.Status == "pending" && a.AppointmentDate < today)
                .ToListAsync(stoppingToken);

            if (overdueAppointments.Count == 0)
            {
                return;
            }

            _logger.LogInformation("Quét dọn dẹp lịch hẹn quá hạn: Tìm thấy {Count} bản ghi.", overdueAppointments.Count);

            foreach (var appointment in overdueAppointments)
            {
                appointment.Status = "cancelled";
                appointment.CancelReason = "Hệ thống tự động hủy do quá hạn duyệt";
                unitOfWork.Appointments.Update(appointment);

                if (appointment.Customer != null && appointment.Pet != null)
                {
                    var customerUser = unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == appointment.CustomerId && u.IsActive == true);
                    
                    if (customerUser != null)
                    {
                        var message = $"Yêu cầu đặt lịch khám cho {appointment.Pet.Name} vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy} đã bị tự động hủy do hệ thống không thể xử lý trong thời gian chờ.";
                        
                        // Gửi thông báo trong app
                        await notificationService.CreateNotificationAsync(
                            customerUser.Id,
                            "Hủy lịch hẹn quá hạn",
                            message,
                            "AppointmentCancelled"
                        );

                        // Gửi email thông qua EmailQueueWorker
                        if (!string.IsNullOrEmpty(customerUser.Email))
                        {
                            var emailHtml = MyPetClinic.Application.Utils.EmailTemplateBuilder.BuildAppointmentCancelledEmail(
                                customerName: appointment.Customer.FullName,
                                petName: appointment.Pet.Name,
                                appointmentDate: appointment.AppointmentDate,
                                reason: appointment.CancelReason
                            );
                            
                            await emailQueue.QueueEmailAsync(new EmailMessageDto
                            {
                                ToEmail = customerUser.Email,
                                Subject = "MyPetClinic - Thông báo hủy lịch khám",
                                BodyHtml = emailHtml
                            });
                        }
                    }
                }
            }
            
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Đã dọn dẹp và hủy {Count} lịch hẹn quá hạn thành công.", overdueAppointments.Count);
        }
    }
}
