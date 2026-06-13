using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class VaccinationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly VaccinationService _service;

        public VaccinationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new VaccinationService(_unitOfWork);
        }

        [Fact]
        public async Task RecordVaccination_ShouldAutoCalculateNextDueDate_BasedOnIntervalDays()
        {
            // Arrange
            var pet = new Pet { Id = 1, OwnerId = Guid.NewGuid(), Name = "Milo" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 1,
                PetId = 1,
                CustomerId = Guid.NewGuid(),
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "waiting",
                AppointmentDate = DateTime.UtcNow
            };

            var vaccine = new Vaccine
            {
                Id = 1,
                Name = "Rabies Vaccine",
                StockQuantity = 10,
                IntervalDays = 365 // 1 year
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Vaccines.Add(vaccine);
            await _context.SaveChangesAsync();

            // Act
            var recordId = await _service.RecordVaccinationAsync(1, 1, 1, doctorId, "Injection OK");

            // Assert
            var record = await _context.VaccinationRecords.FindAsync(recordId);
            Assert.NotNull(record);
            Assert.Equal(DateTime.Today.AddDays(365), record.NextDueDate);
            Assert.Equal("Injection OK", record.ReactionNote);

            var updatedVaccine = await _context.Vaccines.FindAsync(1L);
            Assert.Equal(9, updatedVaccine!.StockQuantity); // 10 - 1 = 9

            var updatedAppointment = await _context.Appointments.FindAsync(1L);
            Assert.Equal("completed", updatedAppointment!.Status);
        }

        [Fact]
        public async Task RecordVaccination_ShouldBeNullNextDueDate_WhenIntervalDaysIsZero()
        {
            // Arrange
            var pet = new Pet { Id = 2, OwnerId = Guid.NewGuid(), Name = "Milo2" };
            var doctorId = Guid.NewGuid();
            var appointment = new Appointment
            {
                Id = 2,
                PetId = 2,
                CustomerId = Guid.NewGuid(),
                DoctorId = doctorId,
                ServiceId = 1,
                Status = "waiting",
                AppointmentDate = DateTime.UtcNow
            };

            var vaccine = new Vaccine
            {
                Id = 2,
                Name = "Single Dose Vaccine",
                StockQuantity = 5,
                IntervalDays = 0 // No interval
            };

            _context.Pets.Add(pet);
            _context.Appointments.Add(appointment);
            _context.Vaccines.Add(vaccine);
            await _context.SaveChangesAsync();

            // Act
            var recordId = await _service.RecordVaccinationAsync(2, 2, 2, doctorId, null);

            // Assert
            var record = await _context.VaccinationRecords.FindAsync(recordId);
            Assert.NotNull(record);
            Assert.Null(record.NextDueDate);

            var updatedVaccine = await _context.Vaccines.FindAsync(2L);
            Assert.Equal(4, updatedVaccine!.StockQuantity); // 5 - 1 = 4
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
