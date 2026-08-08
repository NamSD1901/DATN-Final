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
            services.AddScoped<IMedicineRepository, MedicineRepository>();
            services.AddScoped<IMedicineBatchRepository, MedicineBatchRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IReceptionistAppointmentService, ReceptionistAppointmentService>();
            services.AddScoped<IDoctorAppointmentService, DoctorAppointmentService>();
            services.AddScoped<IReceptionistService, ReceptionistService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IPostCategoryService, PostCategoryService>();
            services.AddScoped<IMedicineService, MedicineService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<ICustomerAppointmentService, CustomerAppointmentService>();
            
            services.AddScoped<IPetService, PetService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IVaccinationService, VaccinationService>();
            // PrescriptionService sống ở Application layer (có business logic tính trạng thái đơn thuốc)
            services.AddScoped<IPrescriptionService, MyPetClinic.Application.Services.PrescriptionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
            services.AddScoped<IBlockTimeService, BlockTimeService>();
            services.AddScoped<IScheduleProfileService, ScheduleProfileService>();
            services.AddScoped<IScheduleExceptionService, ScheduleExceptionService>();
            
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

            // ReportService sống ở Application layer (tổng hợp nghiệp vụ báo cáo qua IUnitOfWork)
            services.AddScoped<IReportService, MyPetClinic.Application.Services.ReportService>();

            // Đăng ký Operating Hours Service
            services.AddScoped<IOperatingHoursService, OperatingHoursService>();

            // Đăng ký Background Service nhắc lịch tiêm phòng và lịch tái khám tự động
            services.AddHostedService<Workers.VaccineReminderWorker>();
            services.AddHostedService<Workers.AppointmentReminderWorker>();
            services.AddHostedService<Workers.ScheduleGeneratorWorker>();
            services.AddHostedService<Workers.OverdueAppointmentCleanerWorker>();

            // Đăng ký Background Email Queue
            services.AddSingleton<IEmailQueue, EmailQueue>();
            services.AddHostedService<Workers.EmailQueueWorker>();

            return services;
        }
    }
}
