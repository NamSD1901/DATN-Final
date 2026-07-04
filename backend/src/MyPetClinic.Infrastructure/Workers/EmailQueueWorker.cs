using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyPetClinic.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Workers
{
    public class EmailQueueWorker : BackgroundService
    {
        private readonly ILogger<EmailQueueWorker> _logger;
        private readonly IEmailQueue _emailQueue;
        private readonly IServiceProvider _serviceProvider;

        public EmailQueueWorker(
            ILogger<EmailQueueWorker> logger,
            IEmailQueue emailQueue,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _emailQueue = emailQueue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Email Queue Worker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var emailMessage = await _emailQueue.DequeueEmailAsync(stoppingToken);

                    // Process the email
                    using var scope = _serviceProvider.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.SendEmailAsync(
                        toEmail: emailMessage.ToEmail,
                        subject: emailMessage.Subject,
                        body: emailMessage.BodyHtml
                    );

                    _logger.LogInformation("Successfully sent background email to {ToEmail}", emailMessage.ToEmail);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while executing Email Queue Worker.");
                }
            }

            _logger.LogInformation("Background Email Queue Worker is stopping.");
        }
    }
}
