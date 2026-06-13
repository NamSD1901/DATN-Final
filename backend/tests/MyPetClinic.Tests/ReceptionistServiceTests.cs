using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.Services;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class ReceptionistServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly ReceptionistService _service;

        public ReceptionistServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new ReceptionistService(_unitOfWork);
        }

        [Fact]
        public async Task OmniSearch_ShouldReturnMatches_WhenPetOrCustomerNameMatches()
        {
            // Arrange
            var role = new Role { Id = 3, Name = "customer" };
            _context.Roles.Add(role);

            var customer = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Nguyễn Văn Anh",
                Phone = "0912345678",
                Email = "vananh@test.com",
                RoleId = 3,
                IsActive = true
            };
            _context.Users.Add(customer);

            var pet = new Pet
            {
                Id = 1,
                OwnerId = customer.Id,
                Name = "Milu",
                Species = "Chó",
                IsDeceased = false
            };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            // Act 1: Search by pet name
            var searchPetResult = await _service.OmniSearchAsync("Milu");
            // Act 2: Search by customer phone
            var searchPhoneResult = await _service.OmniSearchAsync("09123");

            // Assert
            Assert.NotEmpty(searchPetResult);
            Assert.Equal(customer.FullName, searchPetResult.First().FullName);
            Assert.Equal("Milu", searchPetResult.First().Pets.First().Name);

            Assert.NotEmpty(searchPhoneResult);
            Assert.Equal(customer.FullName, searchPhoneResult.First().FullName);
        }

        [Fact]
        public async Task CheckIn_ShouldSucceed_WhenAppointmentIsToday()
        {
            // Arrange
            var pet = new Pet { Id = 2, Name = "Kiki", Species = "Chó", Weight = 5 };
            _context.Pets.Add(pet);

            var localNow = DateTime.UtcNow.AddHours(7);
            var apptDateUtc = DateTime.SpecifyKind(localNow.Date.AddHours(10 - 7), DateTimeKind.Utc); // 10:00 AM VN time

            var appt = new Appointment
            {
                Id = 201,
                PetId = pet.Id,
                CustomerId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                ServiceId = 1,
                AppointmentDate = apptDateUtc,
                Status = "pending",
                QueueNumber = 0
            };
            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            var request = new CheckInRequestDto
            {
                AppointmentId = appt.Id,
                CurrentWeight = 6.5m,
                IsEmergency = false
            };

            // Act
            var success = await _service.CheckInAsync(request);

            // Assert
            Assert.True(success);
            var updatedAppt = await _context.Appointments.FindAsync(appt.Id);
            Assert.NotNull(updatedAppt);
            Assert.Equal("waiting", updatedAppt.Status);
            Assert.Equal(1, updatedAppt.QueueNumber);
            
            var updatedPet = await _context.Pets.FindAsync(pet.Id);
            Assert.NotNull(updatedPet);
            Assert.Equal(6.5m, updatedPet.Weight);
        }

        [Fact]
        public async Task CheckIn_ShouldThrow_WhenAppointmentDateIsNotToday()
        {
            // Arrange
            var pet = new Pet { Id = 3, Name = "Mimi", Species = "Mèo" };
            _context.Pets.Add(pet);

            var tomorrowUtc = DateTime.UtcNow.AddDays(1);

            var appt = new Appointment
            {
                Id = 202,
                PetId = pet.Id,
                CustomerId = Guid.NewGuid(),
                DoctorId = Guid.NewGuid(),
                ServiceId = 1,
                AppointmentDate = tomorrowUtc,
                Status = "pending"
            };
            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            var request = new CheckInRequestDto { AppointmentId = appt.Id };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CheckInAsync(request));
            Assert.Contains("Không thể Check-in hôm nay", ex.Message);
        }

        [Fact]
        public async Task CreateWalkIn_ShouldCreateNewCustomerAndPet_WhenNotExists()
        {
            // Arrange
            var customerRole = new Role { Id = 3, Name = "customer" };
            var doctorRole = new Role { Id = 2, Name = "doctor" };
            var doctor = new User { Id = Guid.NewGuid(), FullName = "Dr. Strange", RoleId = 2, IsActive = true };
            
            _context.Roles.Add(customerRole);
            _context.Roles.Add(doctorRole);
            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            var request = new WalkInRequestDto
            {
                FullName = "Trần Bính",
                Phone = "0988888888",
                PetName = "Mực",
                Species = "Chó",
                Breed = "Corgi",
                Weight = 8.2m,
                Gender = 1,
                ServiceId = 2,
                IsEmergency = false
            };

            // Act
            var apptId = await _service.CreateWalkInAsync(request, Guid.NewGuid());

            // Assert
            Assert.True(apptId > 0);
            
            var createdCustomer = _context.Users.FirstOrDefault(u => u.Phone == "0988888888");
            Assert.NotNull(createdCustomer);
            Assert.Equal("Trần Bính", createdCustomer.FullName);

            var createdPet = _context.Pets.FirstOrDefault(p => p.OwnerId == createdCustomer.Id);
            Assert.NotNull(createdPet);
            Assert.Equal("Mực", createdPet.Name);

            var appt = await _context.Appointments.FindAsync(apptId);
            Assert.NotNull(appt);
            Assert.Equal("waiting", appt.Status);
            Assert.Equal(doctor.Id, appt.DoctorId);
        }

        [Fact]
        public async Task GetTodayQueue_ShouldPrioritizeEmergency_AndThenQueueNumber()
        {
            var nowUtc = DateTime.UtcNow;
            var customer = new User { Id = Guid.NewGuid(), FullName = "Chủ Nuôi A", Phone = "0912345678", Email = "customerA@test.com", RoleId = 3, IsActive = true };
            var doctor = new User { Id = Guid.NewGuid(), FullName = "Bác Sĩ B", Phone = "0987654321", Email = "doctorB@test.com", RoleId = 2, IsActive = true };
            var pet1 = new Pet { Id = 11, Name = "Lu", OwnerId = customer.Id, Species = "Chó" };
            var pet2 = new Pet { Id = 12, Name = "Lê", OwnerId = customer.Id, Species = "Chó" };
            var pet3 = new Pet { Id = 13, Name = "Lép", OwnerId = customer.Id, Species = "Chó" };

            _context.Users.AddRange(customer, doctor);
            _context.Pets.AddRange(pet1, pet2, pet3);

            _context.Appointments.Add(new Appointment
            {
                Id = 301, CustomerId = customer.Id, PetId = pet1.Id, DoctorId = doctor.Id, ServiceId = 1,
                AppointmentDate = nowUtc, Status = "waiting", QueueNumber = 1, IsEmergency = false
            });
            _context.Appointments.Add(new Appointment
            {
                Id = 302, CustomerId = customer.Id, PetId = pet2.Id, DoctorId = doctor.Id, ServiceId = 1,
                AppointmentDate = nowUtc, Status = "waiting", QueueNumber = 2, IsEmergency = true // Emergency
            });
            _context.Appointments.Add(new Appointment
            {
                Id = 303, CustomerId = customer.Id, PetId = pet3.Id, DoctorId = doctor.Id, ServiceId = 1,
                AppointmentDate = nowUtc, Status = "waiting", QueueNumber = 3, IsEmergency = false
            });

            await _context.SaveChangesAsync();

            // Act
            var queue = await _service.GetTodayQueueAsync();

            // Assert
            Assert.Equal(3, queue.Count);
            Assert.Equal(302, queue[0].AppointmentId); // Emergency first
            Assert.Equal(301, queue[1].AppointmentId); // Then Queue 1
            Assert.Equal(303, queue[2].AppointmentId); // Then Queue 3
        }

        [Fact]
        public async Task UpdateEmergencyCustomer_ShouldMapDummyToRealProfile_AndCleanupDummy()
        {
            // Arrange
            var dummyCustomer = new User { Id = Guid.NewGuid(), FullName = "Khách Cấp Cứu", Phone = "0000000000", IsActive = true };
            var dummyPet = new Pet { Id = 999, Name = "Cấp Cứu", OwnerId = dummyCustomer.Id };
            
            var realCustomer = new User { Id = Guid.NewGuid(), FullName = "Lê Kim", Phone = "0944444444", IsActive = true };
            var realPet = new Pet { Id = 888, Name = "Vàng", OwnerId = realCustomer.Id };

            _context.Users.AddRange(dummyCustomer, realCustomer);
            _context.Pets.AddRange(dummyPet, realPet);

            var appt = new Appointment
            {
                Id = 401,
                CustomerId = dummyCustomer.Id,
                PetId = dummyPet.Id,
                DoctorId = Guid.NewGuid(),
                ServiceId = 1,
                IsEmergency = true,
                AppointmentDate = DateTime.UtcNow
            };
            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();

            // Act
            var success = await _service.UpdateEmergencyCustomerAsync(appt.Id, realCustomer.Id, realPet.Id);

            // Assert
            Assert.True(success);
            
            var updatedAppt = await _context.Appointments.FindAsync(appt.Id);
            Assert.NotNull(updatedAppt);
            Assert.Equal(realCustomer.Id, updatedAppt.CustomerId);
            Assert.Equal(realPet.Id, updatedAppt.PetId);

            // Verify dummy patient and customer are cleaned up
            Assert.Null(await _context.Pets.FindAsync(dummyPet.Id));
            Assert.Null(await _context.Users.FindAsync(dummyCustomer.Id));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
