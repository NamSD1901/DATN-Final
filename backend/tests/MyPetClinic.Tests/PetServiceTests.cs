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
    public class PetServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PetRepository _petRepository;
        private readonly PetService _service;

        public PetServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _petRepository = new PetRepository(_context);
            _service = new PetService(_petRepository);
        }

        [Fact]
        public async Task GetPetById_ShouldReturnNull_WhenOwnerIdDoesNotMatch()
        {
            // Arrange
            var pet = new Pet
            {
                Id = 10,
                CustomerId = Guid.NewGuid(),
                Name = "Buddy",
                Species = "Chó"
            };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetPetByIdAsync(10, Guid.NewGuid()); // Random new owner ID

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePet_ShouldThrowUnauthorized_WhenOwnerIdDoesNotMatch()
        {
            // Arrange
            var pet = new Pet
            {
                Id = 11,
                CustomerId = Guid.NewGuid(),
                Name = "Buddy",
                Species = "Chó"
            };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            var dto = new UpdatePetDto
            {
                Id = 11,
                Name = "Buddy New"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.UpdatePetAsync(dto, Guid.NewGuid()) // Random new owner ID
            );
        }

        [Fact]
        public async Task DeletePet_ShouldThrowUnauthorized_WhenOwnerIdDoesNotMatch()
        {
            // Arrange
            var pet = new Pet
            {
                Id = 12,
                CustomerId = Guid.NewGuid(),
                Name = "Buddy",
                Species = "Chó"
            };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.DeletePetAsync(12, Guid.NewGuid()) // Random new owner ID
            );
        }

        [Fact]
        public async Task AddPet_ShouldCreateSuccessfully_WithValidData()
        {
            // Arrange
            var CustomerId = Guid.NewGuid();
            var dto = new CreatePetDto
            {
                Name = "Milo",
                Species = "Mèo",
                Breed = "Ba Tư",
                Gender = 2,
                Weight = 4.2m
            };

            // Act
            var result = await _service.AddPetAsync(dto, CustomerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Milo", result.Name);
            Assert.Equal(CustomerId, result.CustomerId);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
