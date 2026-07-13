using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Infrastructure.Workers
{
    public class ScheduleGeneratorWorker : BackgroundService
    {
        private readonly ILogger<ScheduleGeneratorWorker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ScheduleGeneratorWorker(ILogger<ScheduleGeneratorWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Schedule Generator Worker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running auto schedule generator...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var scheduleProfileService = scope.ServiceProvider.GetRequiredService<IScheduleProfileService>();
                        // Generate schedule for the next 30 days
                        int totalGenerated = await scheduleProfileService.AutoGenerateSchedulesForAllDoctorsAsync(30);
                        
                        if (totalGenerated > 0)
                        {
                            _logger.LogInformation($"Successfully generated {totalGenerated} new doctor schedule slots.");
                        }
                        else
                        {
                            _logger.LogInformation("No new schedules generated. All active profiles are fully scheduled.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while auto generating schedules.");
                }

                // Run every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
