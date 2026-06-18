using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Workers
{
    public class VaccineReminderWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VaccineReminderWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

        public VaccineReminderWorker(IServiceProvider serviceProvider, ILogger<VaccineReminderWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Worker nhắc lịch tiêm vắc-xin đã khởi động.");

            // Wait a short time before the first execution to let the app start up fully
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendVaccineRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi xảy ra trong tiến trình chạy ngầm gửi email nhắc lịch.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        public async Task SendVaccineRemindersAsync(CancellationToken stoppingToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var targetDate = DateTime.UtcNow.Date.AddDays(3);

            var upcomingVaccinations = await unitOfWork.VaccinationRecords.Query()
                .Include(v => v.Pet)
                    .ThenInclude(p => p!.Owner)
                .Include(v => v.Vaccine)
                .Where(v => v.NextDueDate.HasValue && v.NextDueDate.Value.Date == targetDate)
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Quét tiêm chủng nhắc lịch cho ngày {TargetDate:dd/MM/yyyy}: Tìm thấy {Count} bản ghi.", targetDate, upcomingVaccinations.Count);

            foreach (var record in upcomingVaccinations)
            {
                if (record.Pet?.Owner != null && record.Vaccine != null)
                {
                    var email = record.Pet.Owner.Email ?? string.Empty;
                    if (!string.IsNullOrEmpty(email))
                    {
                        var subject = $"🔔 Nhắc lịch tiêm chủng vắc-xin cho bé {record.Pet.Name}";
                        var body = $"<div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>" +
                                   $"<h2>Nhắc Lịch Tiêm Chủng Định Kỳ</h2>" +
                                   $"Chào bạn <b>{record.Pet.Owner.FullName}</b>,<br/><br/>" +
                                   $"Thú cưng <b>{record.Pet.Name}</b> của bạn có lịch tiêm nhắc lại mũi vắc-xin <b>{record.Vaccine.Name}</b> vào ngày <b>{record.NextDueDate!.Value:dd/MM/yyyy}</b>.<br/>" +
                                   $"Việc tiêm phòng đúng hạn giúp bé cưng duy trì hệ miễn dịch khỏe mạnh chống lại các bệnh truyền nhiễm.<br/><br/>" +
                                   $"Vui lòng truy cập cổng đặt lịch trực tuyến của <b>MyPetClinic</b> để đăng ký lịch hẹn tiêm phòng cho bé cưng sớm.<br/>" +
                                   $"<br/>Trân trọng,<br/>Đội ngũ MyPetClinic." +
                                   $"</div>";

                        try
                        {
                            await emailService.SendEmailAsync(email, subject, body);
                            _logger.LogInformation("Đã gửi thành công email nhắc lịch tiêm chủng vắc-xin cho {Email} (Thú cưng: {PetName})", email, record.Pet.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Lỗi khi gửi email nhắc lịch cho {Email}", email);
                        }
                    }
                }
            }
        }
    }
}
