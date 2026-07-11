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
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            var checker = new MyPetClinic.Application.Helpers.VaccinationScheduleChecker();
            var notificationServiceMock = new Mock<MyPetClinic.Application.Interfaces.Services.INotificationService>();
            var emailQueueMock = new Mock<MyPetClinic.Application.Interfaces.Services.IEmailQueue>();
            _service = new AppointmentService(_unitOfWork, checker, notificationServiceMock.Object, emailQueueMock.Object);
        }

        [Fact]
        public async Task CreateAppointment_ShouldThrowError_WhenDoubleBooking()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var appointmentTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            
            var doctorUser = new User { Id = doctorId, FullName = "Bác Sĩ A", Email = "bacsi_test@gmail.com", RoleId = 2, IsActive = true };
            _context.Users.Add(doctorUser);

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
                StartTime = appointmentTime.TimeOfDay,
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
        public async Task CreateAppointment_ShouldThrowError_WhenActiveAppointmentsReachLimit()
        {
            // Arrange
            var doctorId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var appointmentTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            
            var doctorUser = new User { Id = doctorId, FullName = "Bác Sĩ A", Email = "bacsi_test_limit@gmail.com", RoleId = 2, IsActive = true };
            _context.Users.Add(doctorUser);

            // Seed DoctorSchedule
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorId,
                WorkDate = appointmentTime.Date,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsAvailable = true
            });

            // Seed 3 active appointments for this customer
            for(int i = 1; i <= 3; i++)
            {
                _context.Appointments.Add(new Appointment
                {
                    Id = i + 10,
                    CustomerId = customerId,
                    DoctorId = doctorId,
                    AppointmentDate = appointmentTime.AddDays(i),
                    StartTime = appointmentTime.TimeOfDay,
                    Status = "pending"
                });
            }
            await _context.SaveChangesAsync();

            var dto = new AppointmentCreateDto
            {
                DoctorId = doctorId,
                AppointmentDate = appointmentTime.AddDays(4), // Different time to avoid double-booking
                CustomerId = customerId,
                PetId = 1,
                ServiceId = 1
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAppointmentAsync(dto, customerId));
            Assert.Equal("Bạn đang có 3 lịch hẹn chờ khám. Vui lòng hoàn tất hoặc hủy bớt lịch cũ trước khi đặt lịch mới.", ex.Message);
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
            var pet = new Pet { Id = 5, Name = "Lu", Species = "Chó", BirthDate = DateTime.Today.AddYears(-1), CustomerId = Guid.NewGuid() };
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
                CustomerId = pet.CustomerId,
                PetId = pet.Id,
                ServiceId = 1, // Tiêm chủng service
                DoctorId = doctorId,
                AppointmentDate = appointmentTime,
                Symptom = "Tiêm định kỳ",
                VaccineId = vaccine.Id
            };

            // Act
            var appointmentId = await _service.CreateAppointmentAsync(dto, pet.CustomerId);

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
            
            var petB = new Pet { Id = 20, Name = "Mimi", Species = "Mèo", CustomerId = userB };
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
            var pet = new Pet { Id = 30, Name = "Kiki", Species = "Chó", CustomerId = customerId };
            
            var customerUser = new Customer { Id = customerId, FullName = "Khách Hàng A", Phone = "0123456789", Email = "customer@test.com" };
            var doctorUser = new User { Id = doctorId, FullName = "Bác Sĩ B", Phone = "0987654321", Email = "doctor@test.com", RoleId = 2, IsActive = true };
            var service = new Service { Id = 1, Name = "Khám Tổng Quát", Price = 100000 };

            _context.Customers.Add(customerUser);
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
            var doctorRole = new Role { Id = 2, Name = "Doctor" };
            var doctorUser = new User { Id = doctorId, FullName = "Bác Sĩ C", Phone = "0987654322", Email = "bacsi_test@gmail.com", RoleId = 2, IsActive = true, Role = doctorRole };
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
                StartTime = new TimeSpan(9, 30, 0), // 09:30
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
            var targetDate = DateTime.Today.AddDays(3);
            var appointmentTime = targetDate.AddHours(10); // 10:00 AM

            var doctorRole = new Role { Id = 2, Name = "Doctor" };
            var serviceCat = new ServiceCategory { Id = 1, Name = "Khám bệnh" };
            var service = new Service { Id = 1, CategoryId = 1, Category = serviceCat, Name = "Test" };
            if (_context.Roles.Find(2L) == null) _context.Roles.Add(doctorRole);
            _context.ServiceCategories.Add(serviceCat);
            _context.Services.Add(service);

            var doctorUserA = new User { Id = doctorIdA, FullName = "Bác Sĩ A", Phone = "0987654323", Email = "bacsi_test@gmail.com", RoleId = 2, IsActive = true, Role = doctorRole };
            var doctorUserB = new User { Id = doctorIdB, FullName = "Bác Sĩ B", Phone = "0987654324", Email = "bacsituantran@gmail.com", RoleId = 2, IsActive = true, Role = doctorRole };
            _context.Users.Add(doctorUserA);
            _context.Users.Add(doctorUserB);

            // Cả 2 bác sĩ đều trực ngày hôm đó
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorIdA,
                WorkDate = targetDate,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(17, 0, 0),
                IsAvailable = true
            });
            _context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = doctorIdB,
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
                StartTime = appointmentTime.TimeOfDay,
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

        [Fact]
        public async Task GetSuitableDoctorsForAppointment_ShouldReturnDoctors_OrderedByScore()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var petId = 100L;
            var doc1 = Guid.NewGuid();
            var doc2 = Guid.NewGuid();
            var doc3 = Guid.NewGuid();
            var aptDate = DateTime.UtcNow.Date.AddDays(5);

            var clinicalRole = new Role { Id = 3, Name = "clinical_doctor" };
            if (await _context.Roles.FindAsync(3L) == null) _context.Roles.Add(clinicalRole);

            // Doc 1: Has past history (50 pts), 5.0 rating (30 pts), 2 years exp (20 pts) -> 100 pts
            _context.Users.Add(new User { Id = doc1, FullName = "Doctor 1", RoleId = 3, Role = clinicalRole, IsActive = true, EmployeeProfile = new EmployeeProfile { CreatedAt = DateTime.UtcNow.AddYears(-2) } });
            _context.Appointments.Add(new Appointment { Id = 401, CustomerId = customerId, PetId = petId, DoctorId = doc1, Status = "completed" });
            _context.Reviews.Add(new Review { Id = 1, AppointmentId = 401, Rating = 5, Appointment = new Appointment { DoctorId = doc1 } });

            // Doc 2: No history, 4.0 rating (24 pts), 1 year exp (12 pts) -> ~36 pts
            _context.Users.Add(new User { Id = doc2, FullName = "Doctor 2", RoleId = 3, Role = clinicalRole, IsActive = true, EmployeeProfile = new EmployeeProfile { CreatedAt = DateTime.UtcNow.AddYears(-1) } });
            _context.Appointments.Add(new Appointment { Id = 402, CustomerId = Guid.NewGuid(), PetId = 200, DoctorId = doc2, Status = "completed" });
            _context.Reviews.Add(new Review { Id = 2, AppointmentId = 402, Rating = 4, Appointment = new Appointment { DoctorId = doc2 } });

            // Doc 3: Has history with Customer but different pet (25 pts), 3.0 rating (18 pts), 0 exp -> ~43 pts
            _context.Users.Add(new User { Id = doc3, FullName = "Doctor 3", RoleId = 3, Role = clinicalRole, IsActive = true, EmployeeProfile = new EmployeeProfile { CreatedAt = DateTime.UtcNow } });
            _context.Appointments.Add(new Appointment { Id = 403, CustomerId = customerId, PetId = 999, DoctorId = doc3, Status = "completed" });
            _context.Reviews.Add(new Review { Id = 3, AppointmentId = 403, Rating = 3, Appointment = new Appointment { DoctorId = doc3 } });

            // Target appointment
            _context.Appointments.Add(new Appointment
            {
                Id = 400,
                CustomerId = customerId,
                Customer = new Customer { Id = customerId, FullName = "Test", Phone = "123" },
                PetId = petId,
                Pet = new Pet { Id = petId, Name = "Test Pet", CustomerId = customerId },
                AppointmentDate = aptDate,
                StartTime = new TimeSpan(10, 0, 0),
                ServiceId = 1,
                Service = new Service { Id = 1, Name = "Khám bệnh" }
            });
            await _context.SaveChangesAsync();

            // Act
            var result = (await _service.GetSuitableDoctorsForAppointmentAsync(400)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(doc1, result[0].DoctorId); // Doc 1 Highest (100)
            Assert.Equal(doc3, result[1].DoctorId); // Doc 3 (43)
            Assert.Equal(doc2, result[2].DoctorId); // Doc 2 (36)
        }

        [Fact]
        public async Task UpdateAppointmentDoctor_ShouldThrowError_WhenNewDoctorIsBlocked()
        {
            // Arrange
            var newDoctorId = Guid.NewGuid();
            var targetDate = DateTime.UtcNow.Date.AddDays(5);
            var oldDoctorId = Guid.NewGuid();

            var pet = new Pet { Id = 999, Name = "Test Pet", CustomerId = Guid.NewGuid() };
            var customer = new Customer { Id = pet.CustomerId, FullName = "Test Cust", Phone = "0999" };
            var oldDoctor = new User { Id = oldDoctorId, FullName = "Old Doc", RoleId = 2 };

            _context.Pets.Add(pet);
            _context.Customers.Add(customer);
            _context.Users.Add(oldDoctor);

            _context.Appointments.Add(new Appointment
            {
                Id = 500,
                AppointmentDate = targetDate,
                StartTime = new TimeSpan(9, 0, 0), // 09:00 - 09:30
                Status = "pending",
                DoctorId = oldDoctorId, // Old doctor
                CustomerId = customer.Id,
                PetId = pet.Id
            });

            // Block Time overlap with appointment
            _context.BlockTimes.Add(new BlockTime
            {
                Id = Guid.NewGuid(),
                DoctorId = newDoctorId,
                StartTime = new DateTimeOffset(targetDate.AddHours(8), TimeSpan.Zero),
                EndTime = new DateTimeOffset(targetDate.AddHours(12), TimeSpan.Zero)
            });
            await _context.SaveChangesAsync();

            var req = new ChangeDoctorRequestDto
            {
                NewDoctorId = newDoctorId,
                Force = false,
                Reason = "Khách hàng yêu cầu"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAppointmentDoctorAsync(500, req));
            Assert.Equal("Bác sĩ mới đang trong thời gian nghỉ phép/bận.", ex.Message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
