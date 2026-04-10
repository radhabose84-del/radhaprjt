using Contracts.Common;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Interfaces.IUserSession;
using UserManagement.Application.UserLogin.Commands.DeactivateUserSession;
using UserManagement.Application.UserLogin.Commands.UnlockUser;
using UserManagement.Application.UserLogin.Commands.UserLogin;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class AuthControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UserLoginCommand>> _mockLoginValidator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AuthController>> _mockLogger = new();
        private readonly Mock<IUserSessionRepository> _mockSessionRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _mockUserQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ITimeZoneService> _mockTimeZoneService = new(MockBehavior.Strict);
        private readonly Mock<IValidator<DeactivateUserSessionCommand>> _mockDeactivateValidator = new(MockBehavior.Strict);

        private AuthController CreateSut() =>
            new(
                _mockMediator.Object,
                _mockLoginValidator.Object,
                _mockLogger.Object,
                _mockSessionRepo.Object,
                _mockUserQueryRepo.Object,
                _mockTimeZoneService.Object,
                _mockDeactivateValidator.Object
            );

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var command = new UserLoginCommand { Username = "admin", Password = "pass123" };
            var validResult = new ValidationResult();

            _mockLoginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validResult);
            _mockLoginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UserLoginCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validResult);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<LoginResponse>
                {
                    IsSuccess = true,
                    Message = "Login successful",
                    Data = new LoginResponse { Token = "test-token" }
                });

            var sut = CreateSut();

            // Act
            var result = await sut.Login(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_InvalidValidation_ReturnsBadRequest()
        {
            // Arrange
            var command = new UserLoginCommand { Username = "", Password = "" };
            var sessionResult = new ValidationResult();
            var failedResult = new ValidationResult(new[] { new ValidationFailure("Username", "Username is required.") });

            _mockLoginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UserLoginCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionResult);
            _mockLoginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);

            var sut = CreateSut();

            // Act
            var result = await sut.Login(command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetSessionByJwtId_ValidJwtId_ReturnsOkResult()
        {
            // Arrange
            var session = new UserManagement.Domain.Entities.UserSessions();
            _mockSessionRepo
                .Setup(r => r.GetSessionByJwtIdAsync("valid-jwt-id"))
                .ReturnsAsync(session);

            var sut = CreateSut();

            // Act
            var result = await sut.GetSessionByJwtId("valid-jwt-id");

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetSessionByJwtId_EmptyJwtId_ReturnsUnauthorized()
        {
            // Arrange
            var sut = CreateSut();

            // Act
            var result = await sut.GetSessionByJwtId("");

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetSessionByJwtId_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockSessionRepo
                .Setup(r => r.GetSessionByJwtIdAsync("missing-jwt"))
                .ReturnsAsync((UserManagement.Domain.Entities.UserSessions?)null);

            var sut = CreateSut();

            // Act
            var result = await sut.GetSessionByJwtId("missing-jwt");

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeactivateExpiredSessions_ReturnsOkResult()
        {
            // Arrange
            _mockSessionRepo
                .Setup(r => r.DeactivateExpiredSessionsAsync())
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.DeactivateExpiredSessions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeactivateUserSessionsAsync_ReturnsOkResult()
        {
            // Arrange
            _mockSessionRepo
                .Setup(r => r.DeactivateUserSessionsAsync(1))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.DeactivateUserSessionsAsync(1);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeactivateUserSessionsByUsername_ValidCommand_ReturnsOkResult()
        {
            // Arrange
            var command = new DeactivateUserSessionCommand { Username = "testuser" };
            var validResult = new ValidationResult();

            _mockDeactivateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<DeactivateUserSessionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validResult);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeactivateUserSessionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.DeactivateUserSessionsByUsername(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeactivateUserSessionsByUsername_InvalidValidation_ReturnsBadRequest()
        {
            // Arrange
            var command = new DeactivateUserSessionCommand { Username = "" };
            var failedResult = new ValidationResult(new[] { new ValidationFailure("Username", "Username is required.") });

            _mockDeactivateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<DeactivateUserSessionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);

            var sut = CreateSut();

            // Act
            var result = await sut.DeactivateUserSessionsByUsername(command);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UnlockUser_ReturnsOkResult()
        {
            // Arrange
            var command = new UnlockUserCommand { userName = "testuser" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            var result = await sut.UnlockUser(command);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UnlockUser_CallsMediatorSend_Once()
        {
            // Arrange
            var command = new UnlockUserCommand { userName = "testuser" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            await sut.UnlockUser(command);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
