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
    public class MedicineAdminTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly Mock<IGenericRepository<Medicine>> _mockMedicineRepository;

        public MedicineAdminTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _mockMedicineRepository = new Mock<IGenericRepository<Medicine>>();

            _mockUnitOfWork.Setup(u => u.Medicines).Returns(_mockMedicineRepository.Object);
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
        public async Task GetMedicines_ReturnsAllMedicines()
        {
            // Arrange
            var controller = CreateController();
            var medicines = new List<Medicine>
            {
                new Medicine { Id = 1, Name = "Paracetamol", StockQuantity = 100 },
                new Medicine { Id = 2, Name = "Amoxicillin", StockQuantity = 50 }
            };
            _mockMedicineRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines);

            // Act
            var result = await controller.GetMedicines();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMedicines = Assert.IsAssignableFrom<IEnumerable<Medicine>>(okResult.Value);
            Assert.Equal(2, returnedMedicines.Count());
        }

        [Fact]
        public async Task GetMedicineWarnings_CategorizesLowStockAndExpiring()
        {
            // Arrange
            var controller = CreateController();
            var today = DateTime.UtcNow.Date;
            var medicines = new List<Medicine>
            {
                // Low stock (<= 10)
                new Medicine { Id = 1, Name = "Low Stock Medicine", StockQuantity = 5 },
                // Expiring (within 30 days)
                new Medicine { Id = 2, Name = "Expiring Medicine", StockQuantity = 50, ExpiryDate = today.AddDays(15) },
                // Normal
                new Medicine { Id = 3, Name = "Normal Medicine", StockQuantity = 100, ExpiryDate = today.AddDays(100) }
            };
            _mockMedicineRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(medicines);

            // Act
            var result = await controller.GetMedicineWarnings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;
            var lowStock = (List<Medicine>)value.GetType().GetProperty("lowStock").GetValue(value, null);
            var expiring = (List<Medicine>)value.GetType().GetProperty("expiring").GetValue(value, null);

            Assert.Single(lowStock);
            Assert.Equal("Low Stock Medicine", lowStock[0].Name);
            Assert.Single(expiring);
            Assert.Equal("Expiring Medicine", expiring[0].Name);
        }

        [Fact]
        public async Task CreateMedicine_AddsAndSaves()
        {
            // Arrange
            var controller = CreateController();
            var dto = new CreateMedicineDto
            {
                Name = "Aspirin",
                Unit = "vỉ",
                StockQuantity = 200,
                ImportPrice = 10000,
                SellPrice = 15000,
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            };

            // Act
            var result = await controller.CreateMedicine(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockMedicineRepository.Verify(r => r.AddAsync(It.Is<Medicine>(m => m.Name == "Aspirin")), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateMedicine_ExistingMedicine_UpdatesSuccessfully()
        {
            // Arrange
            var controller = CreateController();
            var medicine = new Medicine { Id = 1, Name = "Old Name", StockQuantity = 10 };
            _mockMedicineRepository.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(medicine);

            var dto = new CreateMedicineDto
            {
                Name = "New Name",
                Unit = "chai",
                StockQuantity = 20,
                ImportPrice = 20000,
                SellPrice = 25000
            };

            // Act
            var result = await controller.UpdateMedicine(1, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("New Name", medicine.Name);
            Assert.Equal(20, medicine.StockQuantity);
            _mockMedicineRepository.Verify(r => r.Update(medicine), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMedicine_ExistingMedicine_RemovesSuccessfully()
        {
            // Arrange
            var controller = CreateController();
            var medicine = new Medicine { Id = 1, Name = "To Be Deleted" };
            _mockMedicineRepository.Setup(r => r.GetByIdAsync(1L)).ReturnsAsync(medicine);

            // Act
            var result = await controller.DeleteMedicine(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockMedicineRepository.Verify(r => r.Remove(medicine), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
