using Microsoft.Extensions.DependencyInjection;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.Services;
using MyPetClinic.Application.Helpers;
using MyPetClinic.Infrastructure.Repositories;
using MyPetClinic.Infrastructure.Services;

namespace MyPetClinic.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register Generic Repository & Unit of Work
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IReceptionistService, ReceptionistService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<IVaccinationService, VaccinationService>();
            services.AddScoped<IPrescriptionService, PrescriptionService>();
            
            // Nếu muốn để logic Service ở Application Layer, ta chỉ cần đăng ký tại đây
            // Hoặc có thể tạo AddApplicationServices riêng biệt bên Application, 
            // nhưng để gọn thì ta đăng ký cả ở đây.
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // Đăng ký Email và OTP Service
            services.AddTransient<IEmailService, EmailService>();
            services.AddSingleton<IOtpService, OtpService>();

            // Đăng ký AI Chatbot Service
            services.AddScoped<IAiChatbotService, AiChatbotService>();
            services.AddScoped<IVaccinationScheduleChecker, VaccinationScheduleChecker>();

            // Đăng ký Audit Log Service
            services.AddScoped<IAuditLogService, AuditLogService>();

            // Đăng ký Report Service
            services.AddScoped<IReportService, ReportService>();

            // Đăng ký Background Service nhắc lịch tiêm phòng
            services.AddHostedService<Workers.VaccineReminderWorker>();

            return services;
        }
    }
}
