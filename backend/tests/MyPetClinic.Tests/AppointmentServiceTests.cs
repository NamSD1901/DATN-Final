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

using Microsoft.Extensions.Caching.Memory;

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
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            var checker = new MyPetClinic.Application.Helpers.VaccinationScheduleChecker();
            var cache = new MemoryCache(new MemoryCacheOptions());
            _service = new AppointmentService(_unitOfWork, checker, cache);
        }

        [Fact]
        public async Task CreateAppointment_ShouldThrowError_WhenDoubleBooking()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var appointmentTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            
            // Seed DoctorSchedule
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorId,
                WorkDate = appointmentTime.Date,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsAvailable = true
            });

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

        [Fact]
        public async Task CreateAppointment_ShouldCheckStockAndDecrement_WhenVaccineRequested()
        {
            // Arrange
            var pet = new Pet { Id = 5, Name = "Lu", Species = "Chó", BirthDate = DateTime.Today.AddYears(-1), OwnerId = Guid.NewGuid() };
            var vaccine = new Vaccine { Id = 10, Name = "Nobivac Rabies", StockQuantity = 5, TargetSpecies = "Chó", MinAgeWeeks = 12, IntervalDays = 305 };
            var doctorId = Guid.NewGuid();
            var appointmentTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            
            _context.Pets.Add(pet);
            _context.Vaccines.Add(vaccine);
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorId,
                WorkDate = appointmentTime.Date,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsAvailable = true
            });
            await _context.SaveChangesAsync();

            var dto = new AppointmentCreateDto
            {
                CustomerId = pet.OwnerId,
                PetId = pet.Id,
                ServiceId = 1, // Tiêm chủng service
                DoctorId = doctorId,
                AppointmentDate = appointmentTime,
                Symptom = "Tiêm định kỳ",
                VaccineId = vaccine.Id
            };

            // Act
            var appointmentId = await _service.CreateAppointmentAsync(dto, pet.OwnerId);

            // Assert
            var appt = await _context.Appointments.FindAsync(appointmentId);
            Assert.NotNull(appt);
            Assert.Equal(vaccine.Id, appt.VaccineId);
            
            var updatedVaccine = await _context.Vaccines.FindAsync(vaccine.Id);
            Assert.NotNull(updatedVaccine);
            Assert.Equal(4, updatedVaccine.StockQuantity); // 5 - 1 = 4
        }

        [Fact]
        public async Task GetPetMedicalHistory_ShouldThrowUnauthorizedAccessException_WhenUserIsNotPetOwner()
        {
            // Arrange
            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();
            
            var petB = new Pet { Id = 20, Name = "Mimi", Species = "Mèo", OwnerId = userB };
            _context.Pets.Add(petB);
            await _context.SaveChangesAsync();

            // Act & Assert (User A tries to query Pet B's medical history)
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.GetPetMedicalHistoryAsync(petB.Id, userA));
        }

        [Fact]
        public async Task GetCustomerAppointmentsPaginated_ShouldReturnPaginatedResults()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var pet = new Pet { Id = 30, Name = "Kiki", Species = "Chó", OwnerId = customerId };
            
            var customerUser = new User { Id = customerId, FullName = "Khách Hàng A", Phone = "0123456789", Email = "customer@test.com", RoleId = 3, IsActive = true };
            var doctorUser = new User { Id = doctorId, FullName = "Bác Sĩ B", Phone = "0987654321", Email = "doctor@test.com", RoleId = 2, IsActive = true };
            var service = new Service { Id = 1, Name = "Khám Tổng Quát", Price = 100000 };

            _context.Users.Add(customerUser);
            _context.Users.Add(doctorUser);
            _context.Services.Add(service);
            _context.Pets.Add(pet);

            for (int i = 1; i <= 5; i++)
            {
                _context.Appointments.Add(new Appointment
                {
                    Id = 100 + i,
                    CustomerId = customerId,
                    PetId = pet.Id,
                    ServiceId = 1,
                    DoctorId = doctorId,
                    AppointmentDate = DateTime.UtcNow.Date.AddDays(i).AddHours(10),
                    Status = i % 2 == 0 ? "confirmed" : "pending"
                });
            }
            await _context.SaveChangesAsync();

            // Act (Fetch page 1, size 2, status pending)
            var result = await _service.GetCustomerAppointmentsPaginatedAsync(customerId, "pending", 1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount); // 3 pending appointments (101, 103, 105)
            Assert.Equal(2, result.Items.Count()); // Page size is 2
            Assert.Equal(2, result.TotalPages); // CEILING(3/2) = 2
        }

        [Fact]
        public async Task GetAvailableSlots_ShouldReturnAvailableSlots_WhenDoctorHasSchedule()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var targetDate = DateTime.UtcNow.Date.AddDays(2);
            var doctorUser = new User { Id = doctorId, FullName = "Bác Sĩ C", Phone = "0987654322", Email = "doctorC@test.com", RoleId = 2, IsActive = true };
            _context.Users.Add(doctorUser);

            // Seed doctor schedule
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorId,
                WorkDate = targetDate,
                StartTime = new TimeSpan(9, 0, 0),  // 09:00
                EndTime = new TimeSpan(11, 0, 0),    // 11:00
                IsAvailable = true
            });

            // Seed an existing appointment from 09:30 to 10:00
            _context.Appointments.Add(new Appointment
            {
                Id = 200,
                DoctorId = doctorId,
                AppointmentDate = targetDate.AddHours(9.5), // 09:30
                StartTime = new TimeSpan(9, 30, 0),
                Status = "pending"
            });

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAvailableSlotsAsync(targetDate);

            // Assert
            Assert.NotNull(result);
            var doctorSlots = Assert.Single(result);
            Assert.Equal(doctorId, doctorSlots.DoctorId);
            Assert.Equal("Bác Sĩ C", doctorSlots.DoctorName);

            // With 9:00 - 11:00 schedule and 30-min slot duration:
            // Possible slots: 09:00, 09:30, 10:00, 10:30
            // Since 09:30 is booked, available should be: 09:00, 10:00, 10:30
            Assert.Equal(3, doctorSlots.AvailableSlots.Count());
            Assert.Contains("09:00", doctorSlots.AvailableSlots);
            Assert.DoesNotContain("09:30", doctorSlots.AvailableSlots);
            Assert.Contains("10:00", doctorSlots.AvailableSlots);
            Assert.Contains("10:30", doctorSlots.AvailableSlots);
        }

        [Fact]
        public async Task CreateAppointment_ShouldAutoAssignAvailableDoctor_WhenDoctorIdIsEmpty()
        {
            // Arrange
            var doctorIdA = Guid.NewGuid();
            var doctorIdB = Guid.NewGuid();
            var targetDate = DateTime.UtcNow.Date.AddDays(3);
            var appointmentTime = targetDate.AddHours(10); // 10:00 AM

            var doctorUserA = new User { Id = doctorIdA, FullName = "Bác Sĩ A", Phone = "0987654323", Email = "doctorA@test.com", RoleId = 2, IsActive = true };
            var doctorUserB = new User { Id = doctorIdB, FullName = "Bác Sĩ B", Phone = "0987654324", Email = "doctorB@test.com", RoleId = 2, IsActive = true };
            _context.Users.Add(doctorUserA);
            _context.Users.Add(doctorUserB);

            // Cả 2 bác sĩ đều trực ngày hôm đó
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorIdA,
                Doctor = doctorUserA,
                WorkDate = targetDate,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsAvailable = true
            });
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorIdB,
                Doctor = doctorUserB,
                WorkDate = targetDate,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsAvailable = true
            });

            // Bác sĩ A đã có 1 lịch hẹn bận đúng lúc 10:00
            _context.Appointments.Add(new Appointment
            {
                Id = 301,
                DoctorId = doctorIdA,
                AppointmentDate = appointmentTime,
                Status = "pending"
            });

            await _context.SaveChangesAsync();

            var dto = new AppointmentCreateDto
            {
                CustomerId = Guid.NewGuid(),
                PetId = 1,
                ServiceId = 1,
                DoctorId = Guid.Empty, // Yêu cầu tự động phân công
                AppointmentDate = appointmentTime
            };

            // Act
            var appointmentId = await _service.CreateAppointmentAsync(dto, Guid.NewGuid());

            // Assert
            var appt = await _context.Appointments.FindAsync(appointmentId);
            Assert.NotNull(appt);
            // Hệ thống phải tự phân công cho Bác sĩ B vì Bác sĩ A bận
            Assert.Equal(doctorIdB, appt.DoctorId);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
