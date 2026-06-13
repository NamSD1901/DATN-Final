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
    public class AdminControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly Mock<IGenericRepository<User>> _mockUserRepository;
        private readonly Mock<IGenericRepository<Role>> _mockRoleRepository;
        private readonly Mock<IGenericRepository<Service>> _mockServiceRepository;

        public AdminControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _mockUserRepository = new Mock<IGenericRepository<User>>();
            _mockRoleRepository = new Mock<IGenericRepository<Role>>();
            _mockServiceRepository = new Mock<IGenericRepository<Service>>();

            _mockUnitOfWork.Setup(u => u.Users).Returns(_mockUserRepository.Object);
            _mockUnitOfWork.Setup(u => u.Roles).Returns(_mockRoleRepository.Object);
            _mockUnitOfWork.Setup(u => u.Services).Returns(_mockServiceRepository.Object);
        }

        private AdminController CreateController(string currentUserId)
        {
            var controller = new AdminController(_mockUnitOfWork.Object, _mockAuditLogService.Object);
            
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, currentUserId),
                new Claim(ClaimTypes.Role, "admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task UpdateUserRole_SelfChange_ReturnsBadRequest()
        {
            // Arrange
            var currentUserId = Guid.NewGuid().ToString();
            var controller = CreateController(currentUserId);
            var dto = new UpdateRoleDto { NewRole = "doctor" };

            // Act
            var result = await controller.UpdateUserRole(currentUserId, dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task ToggleUserStatus_SelfLock_ReturnsBadRequest()
        {
            // Arrange
            var currentUserId = Guid.NewGuid().ToString();
            var controller = CreateController(currentUserId);
            var dto = new ToggleStatusDto { IsActive = false };

            // Act
            var result = await controller.ToggleUserStatus(currentUserId, dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task UpdateUserRole_ValidUserAndRole_UpdatesSuccessfully()
        {
            // Arrange
            var currentAdminId = Guid.NewGuid().ToString();
            var targetUserId = Guid.NewGuid();
            var controller = CreateController(currentAdminId);
            var dto = new UpdateRoleDto { NewRole = "receptionist" };

            var targetUser = new User { Id = targetUserId, FullName = "Staff Member", Email = "staff@mypetclinic.vn" };
            _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { targetUser });

            var role = new Role { Id = 3, Name = "receptionist" };
            _mockRoleRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(new List<Role> { role });

            // Act
            var result = await controller.UpdateUserRole(targetUserId.ToString(), dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(role.Id, targetUser.RoleId);
            _mockUserRepository.Verify(r => r.Update(targetUser), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _mockAuditLogService.Verify(a => a.LogActionAsync(currentAdminId, "ChangeRole", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ToggleUserStatus_ValidUser_UpdatesSuccessfully()
        {
            // Arrange
            var currentAdminId = Guid.NewGuid().ToString();
            var targetUserId = Guid.NewGuid();
            var controller = CreateController(currentAdminId);
            var dto = new ToggleStatusDto { IsActive = false };

            var targetUser = new User { Id = targetUserId, FullName = "Staff Member", IsActive = true };
            _mockUserRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(new List<User> { targetUser });

            // Act
            var result = await controller.ToggleUserStatus(targetUserId.ToString(), dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False(targetUser.IsActive);
            _mockUserRepository.Verify(r => r.Update(targetUser), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
            _mockAuditLogService.Verify(a => a.LogActionAsync(currentAdminId, "SuspendUser", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetSlotConfig_ReturnsDefaultOrExistingConfig()
        {
            // Arrange
            var controller = CreateController(Guid.NewGuid().ToString());

            // Act
            var result = controller.GetSlotConfig();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var config = Assert.IsType<SlotConfigModel>(okResult.Value);
            Assert.NotNull(config.StartTime);
            Assert.NotNull(config.EndTime);
            Assert.True(config.DurationMinutes > 0);
        }
    }
}
