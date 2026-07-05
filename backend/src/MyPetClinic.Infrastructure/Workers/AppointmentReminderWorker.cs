using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Workers
{
    public class AppointmentReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public AppointmentReminderWorker(IServiceProvider serviceProvider, ILogger<AppointmentReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Worker nhắc lịch hẹn (Follow-up/Revaccination) đã khởi động.");

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendAppointmentRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong tiến trình chạy ngầm nhắc lịch hẹn.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        public async Task SendAppointmentRemindersAsync(CancellationToken stoppingToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Nhắc trước 2 ngày
            var targetDate = DateTime.UtcNow.Date.AddDays(2);

            var upcomingAppointments = await unitOfWork.Appointments.Query()
                .Include(a => a.Customer)
                .Include(a => a.Pet)
                .Include(a => a.Doctor)
                .Where(a => a.IsSystemGenerated &&
                            a.ReminderStatus == "Pending" &&
                            (a.Status == "pending" || a.Status == "confirmed") && 
                            a.AppointmentDate.Date == targetDate)
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Quét nhắc lịch hẹn (SystemGenerated) cho ngày {TargetDate:dd/MM/yyyy}: Tìm thấy {Count} bản ghi.", targetDate, upcomingAppointments.Count);

            foreach (var appointment in upcomingAppointments)
            {
                if (appointment.Customer != null && appointment.Pet != null)
                {
                    var email = appointment.Customer.Email ?? string.Empty;
                    var typeLabel = appointment.Type == "Revaccination" ? "tái tiêm" : "tái khám";
                    
                    var customerUser = unitOfWork.Users.Query().FirstOrDefault(u => u.CustomerId == appointment.CustomerId && u.IsActive == true);
                    
                    var message = $"Thú cưng {appointment.Pet.Name} có lịch {typeLabel} vào lúc {appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy}.";

                    if (!string.IsNullOrEmpty(email))
                    {
                        var subject = $"🔔 Nhắc lịch {typeLabel} cho bé {appointment.Pet.Name}";
                        var body = $"<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>" +
                                   $"<h2>Nhắc Lịch Hẹn {typeLabel}</h2>" +
                                   $"Chào bạn <b>{appointment.Customer.FullName}</b>,<br/><br/>" +
                                   $"MyPetClinic xin nhắc bạn về lịch hẹn {typeLabel} cho bé <b>{appointment.Pet.Name}</b>.<br/>" +
                                   $"Thời gian: <b>{appointment.AppointmentDate.Add(appointment.StartTime):HH:mm dd/MM/yyyy}</b>.<br/>" +
                                   $"Bác sĩ phụ trách: <b>{appointment.Doctor?.FullName ?? "Đang cập nhật"}</b>.<br/><br/>" +
                                   $"Ghi chú: {appointment.Note}<br/><br/>" +
                                   $"Vui lòng phản hồi tin nhắn hoặc truy cập ứng dụng để xác nhận hoặc dời lịch nếu bạn không thể đến đúng hẹn.<br/>" +
                                   $"<br/>Trân trọng,<br/>Đội ngũ MyPetClinic." +
                                   $"</div>";

                        try
                        {
                            await emailService.SendEmailAsync(email, subject, body);
                            _logger.LogInformation("Đã gửi email nhắc lịch {Type} cho {Email}", typeLabel, email);
                            
                            // Cập nhật trạng thái đã gửi nhắc
                            appointment.ReminderStatus = "Sent";
                            unitOfWork.Appointments.Update(appointment);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi gửi email nhắc lịch cho {Email}", email);
                        }
                    }
                    
                    if (customerUser != null)
                    {
                        await notificationService.CreateNotificationAsync(
                            customerUser.Id,
                            $"Nhắc lịch {typeLabel}",
                            message,
                            "AppointmentReminder"
                        );
                    }
                }
            }
            
            await unitOfWork.SaveChangesAsync();
        }
    }
}
