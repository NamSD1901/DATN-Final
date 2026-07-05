using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class ReviewServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ReviewService _service;

        public ReviewServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            var unitOfWork = new UnitOfWork(_context);
            _service = new ReviewService(unitOfWork);
        }

        [Fact]
        public async Task CreateReview_ShouldThrowBadRequest_WhenAppointmentNotCompleted()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, FullName = "John Doe" };
            var service = new Service { Id = 1, Name = "Test Service", Price = 1000 };
            
            var appointment = new Appointment
            {
                Id = 1,
                CustomerId = customerId,
                ServiceId = 1,
                Status = "pending",
                AppointmentDate = DateTime.UtcNow
            };
            
            _context.Customers.Add(customer);
            _context.Services.Add(service);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var dto = new CreateReviewDto { AppointmentId = 1, Rating = 5, Comment = "Great" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateReviewAsync(customerId, dto));
        }

        [Fact]
        public async Task CreateReview_ShouldSucceed_WhenValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var service = new Service { Id = 1, Name = "Khám Tổng Quát", Description = "Khám", Price = 100000 };
            var customer = new Customer { Id = customerId, FullName = "John Doe" };
            
            var appointment = new Appointment
            {
                Id = 1,
                CustomerId = customerId,
                ServiceId = 1,
                Service = service,
                Customer = customer,
                Status = "completed",
                AppointmentDate = DateTime.UtcNow,
                CheckOutTime = DateTime.UtcNow
            };
            
            _context.Services.Add(service);
            _context.Customers.Add(customer);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var dto = new CreateReviewDto { AppointmentId = 1, Rating = 5, Comment = "Rất tốt" };

            // Act
            var result = await _service.CreateReviewAsync(customerId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Rating);
            Assert.Equal("Rất tốt", result.Comment);
        }

        [Fact]
        public async Task UpdateReview_ShouldThrowBadRequest_WhenOver7Days()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, FullName = "John Doe" };
            var service = new Service { Id = 1, Name = "Test Service", Price = 1000 };
            
            var appointment = new Appointment
            {
                Id = 1,
                CustomerId = customerId,
                ServiceId = 1,
                Status = "completed",
                AppointmentDate = DateTime.UtcNow.AddDays(-8),
                CheckOutTime = DateTime.UtcNow.AddDays(-8)
            };

            var review = new Review
            {
                Id = 1,
                CustomerId = customerId,
                AppointmentId = 1,
                Rating = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            };
            
            _context.Customers.Add(customer);
            _context.Services.Add(service);
            _context.Appointments.Add(appointment);
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            var dto = new UpdateReviewDto { Rating = 4, Comment = "Updated" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateReviewAsync(customerId, 1, dto));
        }

        [Fact]
        public async Task SoftDeleteReview_ShouldSetDeletedAt()
        {
            // Arrange
            var review = new Review
            {
                Id = 1,
                CustomerId = Guid.NewGuid(),
                AppointmentId = 1,
                Rating = 5,
                CreatedAt = DateTime.UtcNow
            };
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Act
            await _service.SoftDeleteReviewAsync(1);

            // Assert
            var deletedReview = await _context.Reviews.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == 1);
            Assert.NotNull(deletedReview);
            Assert.NotNull(deletedReview.DeletedAt);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
