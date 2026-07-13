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
                    .ThenInclude(p => p!.Customer)
                .Include(v => v.Vaccine)
                .Where(v => v.NextDueDate.HasValue && v.NextDueDate.Value.Date == targetDate)
                .ToListAsync(stoppingToken);

            _logger.LogInformation("Quét tiêm chủng nhắc lịch cho ngày {TargetDate:dd/MM/yyyy}: Tìm thấy {Count} bản ghi.", targetDate, upcomingVaccinations.Count);

            foreach (var record in upcomingVaccinations)
            {
                if (record.Pet?.Customer != null && record.Vaccine != null)
                {
                    var email = record.Pet.Customer.Email ?? string.Empty;
                    if (!string.IsNullOrEmpty(email))
                    {
                        var subject = $"🔔 Nhắc lịch tiêm chủng vắc-xin cho bé {record.Pet.Name}";
                        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Nhắc Lịch Tiêm Chủng</title>
</head>
<body style='font-family: Arial, sans-serif; background-color: #f4f6f9; padding: 20px; margin: 0;'>
    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 10px; border-top: 5px solid #f1c40f; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
        <div style='text-align: center; margin-bottom: 20px;'>
            <h2 style='color: #2c3e50; margin: 0; font-size: 28px;'>MyPet<span style='color: #f1c40f;'>Clinic</span></h2>
        </div>
        <h3 style='color: #2c3e50; font-size: 18px; text-align: center; text-transform: uppercase;'>Nhắc Lịch Tiêm Chủng Định Kỳ</h3>
        <h3 style='color: #2c3e50; font-size: 16px;'>Xin chào {record.Pet.Customer.FullName},</h3>
        <p style='color: #555; line-height: 1.6; font-size: 15px;'>
            Thú cưng <b>{record.Pet.Name}</b> của bạn có lịch tiêm nhắc lại mũi vắc-xin <strong style='color: #2980b9;'>{record.Vaccine.Name}</strong> vào ngày <strong style='color: #e74c3c;'>{record.NextDueDate!.Value:dd/MM/yyyy}</strong>.<br><br>
            Việc tiêm phòng đúng hạn giúp bé cưng duy trì hệ miễn dịch khỏe mạnh chống lại các bệnh truyền nhiễm.<br><br>
            Vui lòng truy cập cổng đặt lịch trực tuyến của <b>MyPetClinic</b> để đăng ký lịch hẹn tiêm phòng cho bé cưng sớm.
        </p>
        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0 20px 0;'>
        <p style='color: #95a5a6; font-size: 13px; text-align: center; margin: 0;'>Email này được gửi tự động từ hệ thống MyPetClinic.<br>Vui lòng không trả lời thư này.</p>
    </div>
</body>
</html>";

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
