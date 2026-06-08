using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Services;
using Moq;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using MyPetClinic.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class AppointmentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new AppointmentService(_unitOfWork);
        }

        [Fact]
        public async Task CreateAppointment_ShouldThrowError_WhenDoubleBooking()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var appointmentTime = DateTime.UtcNow.AddDays(1);
            
            // Seed an existing appointment
            _context.Appointments.Add(new Appointment
            {
                Id = 1,
                DoctorId = doctorId,
                AppointmentDate = appointmentTime,
                Status = "pending"
            });
            await _context.SaveChangesAsync();

            var dto = new AppointmentCreateDto
            {
                DoctorId = doctorId,
                AppointmentDate = appointmentTime, // Exact same time
                CustomerId = Guid.NewGuid(),
                PetId = 1,
                ServiceId = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(dto, Guid.NewGuid()));
            Assert.Equal("Bác sĩ đã có lịch hẹn trong khoảng thời gian này.", ex.Message);
        }

        [Fact]
        public async Task RescheduleAppointment_ShouldThrowError_WhenDateInPast()
        {
            // Arrange
            var appointment = new Appointment
            {
                Id = 1,
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Status = "pending"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var pastDate = DateTime.UtcNow.AddDays(-1);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RescheduleAppointmentAsync(1, pastDate));
            Assert.Equal("Không thể dời lịch về quá khứ.", ex.Message);
        }

        [Fact]
        public async Task UpdateStatus_ShouldFail_WhenTransitionIsInvalid()
        {
            // Arrange
            var appointment = new Appointment
            {
                Id = 1,
                Status = "completed" // Đã hoàn thành
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAppointmentStatusAsync(1, "pending"));
            Assert.Equal("Chuyển đổi trạng thái không hợp lệ.", ex.Message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
