using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs.Offer;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyPetClinic.Tests
{
    public class OfferServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly MyPetClinic.Application.Services.OfferService _service;

        public OfferServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new MyPetClinic.Application.Services.OfferService(_unitOfWork);
        }

        [Fact]
        public async Task ValidateOffer_ShouldFail_WhenOfferIsLocked()
        {
            // Arrange
            var offerId = Guid.NewGuid();
            _context.Offers.Add(new Offer
            {
                Id = offerId,
                Code = "TESTLOCK",
                Name = "Test Lock",
                DiscountType = "FIXED_AMOUNT",
                DiscountValue = 10000,
                Status = "LOCKED",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();

            var request = new ValidateOfferRequestDto { Code = "TESTLOCK", OrderAmount = 50000 };

            // Act
            var result = await _service.ValidateOfferAsync(request, null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Mã giảm giá đang bị khóa.", result.Message);
        }

        [Fact]
        public async Task ValidateOffer_ShouldFail_WhenOrderAmountLessThanMinOrderValue()
        {
            // Arrange
            var offerId = Guid.NewGuid();
            _context.Offers.Add(new Offer
            {
                Id = offerId,
                Code = "MIN100K",
                Name = "Min 100K",
                DiscountType = "FIXED_AMOUNT",
                DiscountValue = 10000,
                MinOrderValue = 100000,
                Status = "ACTIVE",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();

            var request = new ValidateOfferRequestDto { Code = "MIN100K", OrderAmount = 50000 };

            // Act
            var result = await _service.ValidateOfferAsync(request, null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Đơn hàng chưa đạt giá trị tối thiểu 100,000đ.", result.Message);
        }

        [Fact]
        public async Task ApplyOffer_ShouldThrowError_WhenQuantityExceeded()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var user = new User { Id = Guid.NewGuid(), CustomerId = customerId, FullName = "Test Customer" };
            _context.Users.Add(user);

            var offerId = Guid.NewGuid();
            _context.Offers.Add(new Offer
            {
                Id = offerId,
                Code = "LIMIT1",
                Name = "Limit 1",
                DiscountType = "FIXED_AMOUNT",
                DiscountValue = 10000,
                TotalQuantity = 1,
                UsedQuantity = 1, // Đã dùng hết
                Status = "ACTIVE",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ApplyOfferAsync(offerId, customerId, 1, 10000));
            Assert.Equal("Mã giảm giá đã hết lượt sử dụng.", ex.Message);
        }
        
        [Fact]
        public async Task ApplyOffer_Concurrency_ShouldPreventExceedingQuantity_LogicCheck()
        {
            // Note: EF InMemory database does not enforce RowVersion (concurrency tokens).
            // This test verifies the logical check within the service before the DB transaction.
            
            // Arrange
            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();
            _context.Users.Add(new User { Id = Guid.NewGuid(), CustomerId = customerId1, FullName = "Customer 1" });
            _context.Users.Add(new User { Id = Guid.NewGuid(), CustomerId = customerId2, FullName = "Customer 2" });

            var offerId = Guid.NewGuid();
            _context.Offers.Add(new Offer
            {
                Id = offerId,
                Code = "HOTSALE",
                Name = "Flash Sale 1 Slot",
                DiscountType = "FIXED_AMOUNT",
                DiscountValue = 50000,
                TotalQuantity = 1, // Only 1 slot available
                UsedQuantity = 0,
                Status = "ACTIVE",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();

            // Act - First customer applies successfully
            var result1 = await _service.ApplyOfferAsync(offerId, customerId1, 1, 50000);
            
            // Assert - First customer
            Assert.True(result1);
            
            // The offer in context will have UsedQuantity = 1 now
            
            // Act - Second customer tries to apply
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ApplyOfferAsync(offerId, customerId2, 2, 50000));
            
            // Assert - Second customer is blocked by logical check
            Assert.Equal("Mã giảm giá đã hết lượt sử dụng.", ex.Message);
            
            var logs = await _context.OfferUsageLogs.ToListAsync();
            Assert.Single(logs);
            Assert.Equal(customerId1, _context.Users.First(u => u.Id == logs[0].UserId).CustomerId);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
