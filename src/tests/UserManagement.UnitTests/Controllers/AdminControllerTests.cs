using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin;
using UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword;
using UserManagement.Application.EntityLevelAdmin.Commands.SendOTP;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class AdminControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AdminController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            // Arrange
            var command = new CreateEntityLevelAdminCommand { Email = "test@test.com", EntityId = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEntityLevelAdminCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            var result = await sut.CreateAsync(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            // Arrange
            var command = new CreateEntityLevelAdminCommand { Email = "test@test.com", EntityId = 1 };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEntityLevelAdminCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            await sut.CreateAsync(command);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateEntityLevelAdminCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendOTP_ReturnsOkResult()
        {
            // Arrange
            var command = new SendOTPCommand { Email = "test@test.com" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendOTPCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendOTPDTO { Email = "test@test.com", VerificationCode = "123456" });

            var sut = CreateSut();

            // Act
            var result = await sut.SendOTP(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SendOTP_CallsMediatorSend_Once()
        {
            // Arrange
            var command = new SendOTPCommand { Email = "test@test.com" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<SendOTPCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendOTPDTO { Email = "test@test.com", VerificationCode = "123456" });

            var sut = CreateSut();

            // Act
            await sut.SendOTP(command);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<SendOTPCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SetAdminPassword_ReturnsOkResult()
        {
            // Arrange
            var command = new ResetPasswordCommand { UserId = 1, VerificationCode = "123456" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.SetAdminPassword(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SetAdminPassword_CallsMediatorSend_Once()
        {
            // Arrange
            var command = new ResetPasswordCommand { UserId = 1, VerificationCode = "123456" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            await sut.SetAdminPassword(command);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
