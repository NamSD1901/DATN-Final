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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class VaccineReminderWorkerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<VaccineReminderWorker>> _mockLogger;
        private readonly IServiceProvider _serviceProvider;

        public VaccineReminderWorkerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<VaccineReminderWorker>>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IUnitOfWork>(_unitOfWork);
            serviceCollection.AddSingleton<IEmailService>(_mockEmailService.Object);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task SendVaccineReminders_ShouldSendEmail_OnlyWhenNextDueDateIsExactlyThreeDaysAhead()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var owner = new User { Id = Guid.NewGuid(), FullName = "Nam Nguyen", Email = "nam@gmail.com" };
            var pet = new Pet { Id = 1, Name = "LuLu", OwnerId = owner.Id };
            var vaccine = new Vaccine { Id = 1, Name = "Rabies" };

            // Record 1: Target - NextDueDate is exactly today + 3 days
            var record1 = new VaccinationRecord
            {
                Id = 1,
                PetId = pet.Id,
                VaccineId = vaccine.Id,
                DoctorId = Guid.NewGuid(),
                InjectionDate = today.AddDays(-362),
                NextDueDate = today.AddDays(3)
            };

            // Record 2: Non-target - NextDueDate is today + 5 days
            var record2 = new VaccinationRecord
            {
                Id = 2,
                PetId = pet.Id,
                VaccineId = vaccine.Id,
                DoctorId = Guid.NewGuid(),
                InjectionDate = today.AddDays(-360),
                NextDueDate = today.AddDays(5)
            };

            _context.Users.Add(owner);
            _context.Pets.Add(pet);
            _context.Vaccines.Add(vaccine);
            _context.VaccinationRecords.AddRange(record1, record2);
            await _context.SaveChangesAsync();

            var worker = new VaccineReminderWorker(_serviceProvider, _mockLogger.Object);

            // Act
            await worker.SendVaccineRemindersAsync();

            // Assert
            _mockEmailService.Verify(e => e.SendEmailAsync(
                "nam@gmail.com",
                It.Is<string>(s => s.Contains("LuLu")),
                It.Is<string>(b => b.Contains("Rabies"))
            ), Times.Once);

            _mockEmailService.Verify(e => e.SendEmailAsync(
                "nam@gmail.com",
                It.IsAny<string>(),
                It.Is<string>(b => b.Contains("Parvo")) // Parvo is not in database, it shouldn't send
            ), Times.Never);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
