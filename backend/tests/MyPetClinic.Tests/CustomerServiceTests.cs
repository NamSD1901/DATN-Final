using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using MyPetClinic.Infrastructure.Persistence;
using MyPetClinic.Infrastructure.Repositories;
using Xunit;

namespace MyPetClinic.Tests
{
    public class CustomerServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _service = new CustomerService(_unitOfWork);
        }

        [Fact]
        public async Task SearchCustomersAsync_ShouldReturnMatchingCustomers()
        {
            // Arrange
            _context.Customers.Add(new Customer { Id = Guid.NewGuid(), FullName = "John Doe", Phone = "0987654321", CustomerCode = "CUS-001" });
            _context.Customers.Add(new Customer { Id = Guid.NewGuid(), FullName = "Jane Smith", Phone = "0123456789", CustomerCode = "CUS-002" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.SearchCustomersAsync("0987");

            // Assert
            Assert.Single(result);
            Assert.Equal("John Doe", result.First().FullName);
        }

        [Fact]
        public async Task CreateCustomerWithPetsAsync_ShouldCreateCustomerAndPets()
        {
            // Arrange
            var dto = new CustomerCreateDto
            {
                FullName = "Bob Nguyen",
                Phone = "0999999999",
                Email = "bob@example.com",
                Address = "123 Main St",
                Pets = new List<PetCreateDto>
                {
                    new PetCreateDto { Name = "Max", Species = "Dog", Weight = 10 }
                }
            };

            // Act
            var customerId = await _service.CreateCustomerWithPetsAsync(dto);

            // Assert
            var savedCustomer = await _context.Customers.FindAsync(customerId);
            Assert.NotNull(savedCustomer);
            Assert.Equal("Bob Nguyen", savedCustomer.FullName);

            var savedPets = await _context.Pets.Where(p => p.CustomerId == customerId).ToListAsync();
            Assert.Single(savedPets);
            Assert.Equal("Max", savedPets.First().Name);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
