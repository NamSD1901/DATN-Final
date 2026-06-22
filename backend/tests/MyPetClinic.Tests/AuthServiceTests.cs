using System.Threading.Tasks;
using Moq;
using MyPetClinic.Application.DTOs;
using MyPetClinic.Application.Interfaces.Repositories;
using MyPetClinic.Application.Interfaces.Services;
using MyPetClinic.Application.Services;
using MyPetClinic.Domain.Entities;
using Xunit;

namespace MyPetClinic.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _otpServiceMock = new Mock<IOtpService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _authService = new AuthService(
                _userRepositoryMock.Object,
                _emailServiceMock.Object,
                _otpServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExistsAndUserIsActive()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FullName = "Test User",
                Email = "existing@example.com",
                Password = "Password123",
                Phone = "0123456789",
                Address = "123 Street"
            };

            var existingUser = new User
            {
                Email = "existing@example.com",
                IsActive = true
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email này đã được sử dụng trong hệ thống.", result.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenNewEmail()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                FullName = "New User",
                Email = "new@example.com",
                Password = "Password123",
                Phone = "0123456789",
                Address = "123 Street"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null!);

            var role = new Role { Id = 1, Name = "customer" };
            _userRepositoryMock.Setup(repo => repo.GetRoleByNameAsync("customer"))
                .ReturnsAsync(role);

            _otpServiceMock.Setup(otp => otp.GenerateOtp(It.IsAny<string>()))
                .Returns("123456");

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("new@example.com", result.Email);
            _emailServiceMock.Verify(email => email.SendEmailAsync("new@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task VerifyOtpAsync_ShouldReturnSuccess_WhenOtpIsValid()
        {
            // Arrange
            var email = "user@example.com";
            var otp = "123456";
            var user = new User { Email = email, IsActive = false };

            _otpServiceMock.Setup(otpSvc => otpSvc.ValidateOtp(email, otp))
                .Returns(true);

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.VerifyOtpAsync(email, otp);

            // Assert
            Assert.True(result.Success);
            Assert.True(user.IsActive);
            _userRepositoryMock.Verify(repo => repo.UpdateUserAsync(user), Times.Once);
        }
    }
}
