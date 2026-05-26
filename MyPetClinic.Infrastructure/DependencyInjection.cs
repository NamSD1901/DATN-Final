using Microsoft.Extensions.DependencyInjection;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.Services;
using MyPetClinic.Infrastructure.Repositories;

namespace MyPetClinic.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Nếu muốn để logic Service ở Application Layer, ta chỉ cần đăng ký tại đây
            // Hoặc có thể tạo AddApplicationServices riêng biệt bên Application, 
            // nhưng để gọn thì ta đăng ký cả ở đây.
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            return services;
        }
    }
}
