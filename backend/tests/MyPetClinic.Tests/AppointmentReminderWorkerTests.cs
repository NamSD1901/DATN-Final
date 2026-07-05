using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using MyPetClinic.Infrastructure.Workers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class AppointmentReminderWorkerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<AppointmentReminderWorker>> _mockLogger;
        private readonly IServiceProvider _serviceProvider;

        public AppointmentReminderWorkerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _mockEmailService = new Mock<IEmailService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<AppointmentReminderWorker>>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IUnitOfWork>(_unitOfWork);
            serviceCollection.AddSingleton<IEmailService>(_mockEmailService.Object);
            serviceCollection.AddSingleton<INotificationService>(_mockNotificationService.Object);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task SendAppointmentReminders_ShouldSendEmail_OnlyWhenDateIsExactlyTwoDaysAhead()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var targetDate = today.AddDays(2);
            var owner = new Customer { Id = Guid.NewGuid(), FullName = "Nam Nguyen", Email = "nam@gmail.com" };
            var pet = new Pet { Id = 1, Name = "LuLu", CustomerId = owner.Id };

            // Record 1: Target - System generated, pending, due in 2 days
            var appt1 = new Appointment
            {
                Id = 1,
                PetId = pet.Id,
                CustomerId = owner.Id,
                DoctorId = Guid.NewGuid(),
                ServiceId = 1,
                AppointmentDate = targetDate,
                StartTime = TimeSpan.FromHours(9),
                IsSystemGenerated = true,
                ReminderStatus = "Pending",
                Status = "pending",
                Type = "FollowUp"
            };

            // Record 2: Non-target - Not system generated
            var appt2 = new Appointment
            {
                Id = 2,
                PetId = pet.Id,
                CustomerId = owner.Id,
                DoctorId = Guid.NewGuid(),
                ServiceId = 1,
                AppointmentDate = targetDate,
                StartTime = TimeSpan.FromHours(10),
                IsSystemGenerated = false,
                ReminderStatus = "Pending",
                Status = "pending"
            };
            
            // Record 3: Non-target - Target date is today + 3 days
            var appt3 = new Appointment
            {
                Id = 3,
                PetId = pet.Id,
                CustomerId = owner.Id,
                DoctorId = Guid.NewGuid(),
                ServiceId = 1,
                AppointmentDate = today.AddDays(3),
                StartTime = TimeSpan.FromHours(10),
                IsSystemGenerated = true,
                ReminderStatus = "Pending",
                Status = "pending"
            };

            _context.Customers.Add(owner);
            _context.Pets.Add(pet);
            _context.Appointments.AddRange(appt1, appt2, appt3);
            await _context.SaveChangesAsync();

            var worker = new AppointmentReminderWorker(_serviceProvider, _mockLogger.Object);

            // Act
            await worker.SendAppointmentRemindersAsync();

            // Assert
            _mockEmailService.Verify(e => e.SendEmailAsync(
                "nam@gmail.com",
                It.Is<string>(s => s.Contains("tái khám")),
                It.Is<string>(b => b.Contains("LuLu") && b.Contains("tái khám"))
            ), Times.Once);
            
            // Validate the ReminderStatus is updated
            var updatedAppt = await _context.Appointments.FindAsync(1L);
            Assert.Equal("Sent", updatedAppt!.ReminderStatus);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
