using Microsoft.AspNetCore.Mvc;
using Moq;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Controllers;
using MyPetClinic.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace MyPetClinic.Tests
{
    public class ScheduleAdminTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly Mock<IGenericRepository<DoctorSchedule>> _mockScheduleRepository;

        public ScheduleAdminTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _mockScheduleRepository = new Mock<IGenericRepository<DoctorSchedule>>();

            _mockUnitOfWork.Setup(u => u.DoctorSchedules).Returns(_mockScheduleRepository.Object);
        }

        private AdminController CreateController()
        {
            var controller = new AdminController(_mockUnitOfWork.Object, _mockAuditLogService.Object);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task GetSchedules_ReturnsFilteredSchedules()
        {
            // Arrange
            var controller = CreateController();
            var schedules = new List<DoctorSchedule>
            {
                new DoctorSchedule { Id = 1, DoctorId = Guid.NewGuid(), WorkDate = DateTime.UtcNow.Date, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(12, 0, 0), IsAvailable = true }
            };
            _mockScheduleRepository.Setup(r => r.FindWithIncludesAsync(It.IsAny<Expression<Func<DoctorSchedule, bool>>>(), It.IsAny<Expression<Func<DoctorSchedule, object>>[]>()))
                .ReturnsAsync(schedules);

            // Act
            var result = await controller.GetSchedules();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSchedules = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(returnedSchedules);
        }

        [Fact]
        public async Task CreateSchedule_Overlapping_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            var doctorId = Guid.NewGuid();
            var workDate = DateTime.UtcNow.Date;
            
            // Existing schedule: 08:00 - 12:00
            var existingSchedules = new List<DoctorSchedule>
            {
                new DoctorSchedule 
                { 
                    Id = 1, 
                    DoctorId = doctorId, 
                    WorkDate = workDate, 
                    StartTime = new TimeSpan(8, 0, 0), 
                    EndTime = new TimeSpan(12, 0, 0), 
                    IsAvailable = true 
                }
            };
            _mockScheduleRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DoctorSchedule, bool>>>()))
                .ReturnsAsync(existingSchedules);

            // New schedule that overlaps: 10:00 - 14:00
            var dto = new CreateScheduleDto
            {
                DoctorId = doctorId,
                WorkDate = workDate,
                StartTime = "10:00",
                EndTime = "14:00",
                MaxAppointments = 5,
                IsAvailable = true
            };

            // Act
            var result = await controller.CreateSchedule(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task CreateSchedule_ValidTime_CreatesSuccessfully()
        {
            // Arrange
            var controller = CreateController();
            var doctorId = Guid.NewGuid();
            var workDate = DateTime.UtcNow.Date;

            // Empty schedules list for that doctor on that day
            _mockScheduleRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DoctorSchedule, bool>>>()))
                .ReturnsAsync(new List<DoctorSchedule>());

            var dto = new CreateScheduleDto
            {
                DoctorId = doctorId,
                WorkDate = workDate,
                StartTime = "08:00",
                EndTime = "12:00",
                MaxAppointments = 5,
                IsAvailable = true
            };

            // Act
            var result = await controller.CreateSchedule(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockScheduleRepository.Verify(r => r.AddAsync(It.IsAny<DoctorSchedule>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateSchedule_OverlappingWithAnother_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            var doctorId = Guid.NewGuid();
            var workDate = DateTime.UtcNow.Date;

            var scheduleToUpdate = new DoctorSchedule 
            { 
                Id = 1, 
                DoctorId = doctorId, 
                WorkDate = workDate, 
                StartTime = new TimeSpan(8, 0, 0), 
                EndTime = new TimeSpan(10, 0, 0), 
                IsAvailable = true 
            };
            _mockScheduleRepository.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(scheduleToUpdate);

            // Another existing schedule of the same doctor: 11:00 - 13:00
            var existingSchedules = new List<DoctorSchedule>
            {
                new DoctorSchedule 
                { 
                    Id = 2, 
                    DoctorId = doctorId, 
                    WorkDate = workDate, 
                    StartTime = new TimeSpan(11, 0, 0), 
                    EndTime = new TimeSpan(13, 0, 0), 
                    IsAvailable = true 
                }
            };
            _mockScheduleRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<DoctorSchedule, bool>>>()))
                .ReturnsAsync(existingSchedules);

            // Update schedule 1 to 10:30 - 12:00 (overlaps with schedule 2)
            var dto = new CreateScheduleDto
            {
                DoctorId = doctorId,
                WorkDate = workDate,
                StartTime = "10:30",
                EndTime = "12:00",
                MaxAppointments = 5,
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateSchedule(1, dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task DeleteSchedule_ExistingSchedule_RemovesSuccessfully()
        {
            // Arrange
            var controller = CreateController();
            var schedule = new DoctorSchedule { Id = 1, DoctorId = Guid.NewGuid(), WorkDate = DateTime.UtcNow.Date };
            _mockScheduleRepository.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(schedule);

            // Act
            var result = await controller.DeleteSchedule(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockScheduleRepository.Verify(r => r.Remove(schedule), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
