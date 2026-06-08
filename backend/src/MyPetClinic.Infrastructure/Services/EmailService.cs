using Microsoft.Extensions.Configuration;
using MyPetClinic.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MyPetClinic.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = _configuration["SmtpSettings:Email"];
            var password = _configuration["SmtpSettings:AppPassword"];
            var host = _configuration["SmtpSettings:Host"] ?? "smtp.gmail.com";
            var portString = _configuration["SmtpSettings:Port"] ?? "587";
            int port = int.Parse(portString);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(email, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(email!, "MyPetClinic"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
