using Microsoft.Extensions.DependencyInjection;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.Services;
using MyPetClinic.Infrastructure.Repositories;
using MyPetClinic.Infrastructure.Services;

namespace MyPetClinic.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IReceptionistService, ReceptionistService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IPetService, PetService>();
            
            // Nếu muốn để logic Service ở Application Layer, ta chỉ cần đăng ký tại đây
            // Hoặc có thể tạo AddApplicationServices riêng biệt bên Application, 
            // nhưng để gọn thì ta đăng ký cả ở đây.
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // Đăng ký Email và OTP Service
            services.AddTransient<IEmailService, EmailService>();
            services.AddSingleton<IOtpService, OtpService>();

            // Đăng ký AI Chatbot Service
            services.AddScoped<IAiChatbotService, AiChatbotService>();

            return services;
        }
    }
}
