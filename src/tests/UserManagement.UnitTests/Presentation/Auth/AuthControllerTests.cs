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

namespace UserManagement.UnitTests.Presentation.Auth
{
    public sealed class AuthControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UserLoginCommand>> _loginValidator = new();
        private readonly Mock<ILogger<AuthController>> _mockLogger = new();
        private readonly Mock<IUserSessionRepository> _mockSessionRepo = new();
        private readonly Mock<IUserQueryRepository> _mockUserQueryRepo = new();
        private readonly Mock<ITimeZoneService> _mockTimeZone = new();
        private readonly Mock<IValidator<DeactivateUserSessionCommand>> _deactivateValidator = new();

        private AuthController CreateSut() =>
            new(_mockMediator.Object, _loginValidator.Object, _mockLogger.Object,
                _mockSessionRepo.Object, _mockUserQueryRepo.Object, _mockTimeZone.Object,
                _deactivateValidator.Object);

        [Fact]
        public async Task Login_ValidationFails_ReturnsBadRequest()
        {
            var cmd = new UserLoginCommand { Username = "", Password = "" };
            // The controller first calls the RuleSets extension overload → ValidateAsync(ValidationContext<T>, CancellationToken)
            _loginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UserLoginCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new("UserName", "required")
                }));
            _loginValidator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new("UserName", "required")
                }));

            var result = await CreateSut().Login(cmd);

            result.Should().BeOfType<BadRequestObjectResult>();
            _mockMediator.Verify(
                m => m.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_ValidationPasses_ReturnsOkResult()
        {
            var cmd = new UserLoginCommand { Username = "admin", Password = "pass" };
            // The controller first calls the RuleSets extension overload → ValidateAsync(ValidationContext<T>, CancellationToken)
            _loginValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UserLoginCommand>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _loginValidator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<LoginResponse>
                {
                    IsSuccess = true,
                    Data = new LoginResponse()
                });

            var result = await CreateSut().Login(cmd);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UnlockUser_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UnlockUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UnlockUser(new UnlockUserCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeactivateUserSessionsByUsername_ValidationFails_ReturnsBadRequest()
        {
            var cmd = new DeactivateUserSessionCommand { Username = "" };
            _deactivateValidator
                .Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new("UserName", "required")
                }));

            var result = await CreateSut().DeactivateUserSessionsByUsername(cmd);

            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
