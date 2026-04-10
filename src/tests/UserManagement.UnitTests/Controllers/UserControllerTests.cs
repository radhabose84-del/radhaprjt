using Contracts.Common;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ValidationFailure = FluentValidation.Results.ValidationFailure;
using ValidationResult = FluentValidation.Results.ValidationResult;
using UserManagement.Application.Users.Commands.ChangeUserPassword;
using UserManagement.Application.Users.Commands.CreateUser;
using UserManagement.Application.Users.Commands.DeleteUser;
using UserManagement.Application.Users.Commands.ForgotUserPassword;
using UserManagement.Application.Users.Commands.RemoveVerificationCode;
using UserManagement.Application.Users.Commands.ResetUserPassword;
using UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword;
using UserManagement.Application.Users.Commands.UpdateUser;
using UserManagement.Application.Users.Queries.GetUserAutoComplete;
using UserManagement.Application.Users.Queries.GetUserById;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class UserControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateUserCommand>> _mockCreateValidator = new();
        private readonly Mock<IValidator<UpdateUserCommand>> _mockUpdateValidator = new();
        private readonly Mock<IValidator<FirstTimeUserPasswordCommand>> _mockFirstTimePasswordValidator = new();
        private readonly Mock<IValidator<ChangeUserPasswordCommand>> _mockChangePasswordValidator = new();
        private readonly Mock<ILogger<UserController>> _mockLogger = new();
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint = new();
        private readonly Mock<IValidator<ForgotUserPasswordCommand>> _mockForgotPasswordValidator = new();
        private readonly Mock<IValidator<ResetUserPasswordCommand>> _mockResetPasswordValidator = new();

        private UserController CreateSut() =>
            new(_mockSender.Object,
                _mockCreateValidator.Object,
                _mockUpdateValidator.Object,
                null!,  // ApplicationDbContext - not used in action methods
                _mockFirstTimePasswordValidator.Object,
                _mockChangePasswordValidator.Object,
                _mockLogger.Object,
                _mockForgotPasswordValidator.Object,
                _mockResetPasswordValidator.Object,
                _mockPublishEndpoint.Object);

        private void SetupValidatorSuccess<T>(Mock<IValidator<T>> validator) where T : class
        {
            validator
                .Setup(v => v.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
        }

        // --- GetAllUsersAsync ---

        [Fact]
        public async Task GetAllUsersAsync_SuccessfulResponse_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UserDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<UserDto> { new UserDto() },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUsersAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllUsersAsync_FailedResponse_ReturnsBadRequest()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UserDto>>
                {
                    IsSuccess = false,
                    Message = "Error",
                    Data = new List<UserDto>()
                });

            var result = await CreateSut().GetAllUsersAsync(1, 10);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserByIdDTO());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_NullResult_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserByIdDTO?)null!);

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ValidCommand_ReturnsOkResult()
        {
            var command = new CreateUserCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockCreateValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UserDto>
                {
                    IsSuccess = true,
                    Message = "User created successfully",
                    Data = new UserDto()
                });

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ValidationFails_ReturnsBadRequest()
        {
            var command = new CreateUserCommand { UserName = "" };
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("UserName", "UserName is required.")
                }));

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_HandlerFails_ReturnsBadRequest()
        {
            var command = new CreateUserCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockCreateValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UserDto>
                {
                    IsSuccess = false,
                    Message = "Creation failed"
                });

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_ValidCommand_ReturnsOkResult()
        {
            var command = new UpdateUserCommand { UserId = 1, UserName = "testuser" };
            SetupValidatorSuccess(_mockUpdateValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserByIdDTO());

            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "User updated successfully"
                });

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ValidationFails_ReturnsBadRequest()
        {
            var command = new UpdateUserCommand { UserId = 1 };
            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("UserName", "UserName is required.")
                }));

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_UserNotFound_ReturnsNotFound()
        {
            var command = new UpdateUserCommand { UserId = 999, UserName = "testuser" };
            SetupValidatorSuccess(_mockUpdateValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserByIdDTO?)null!);

            var result = await CreateSut().UpdateAsync(command);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- DeleteAsync ---

        [Fact]
        public async Task DeleteAsync_ValidId_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "User deleted"
                });

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_InvalidId_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteAsync(0);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_FailedResponse_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        // --- GetByUsernameAsync (AutoComplete) ---

        [Fact]
        public async Task GetByUsernameAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetUserAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UserAutoCompleteDto>>
                {
                    IsSuccess = true,
                    Data = new List<UserAutoCompleteDto>()
                });

            var result = await CreateSut().GetByUsernameAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        // --- FirstTimeUserChangePassword ---

        [Fact]
        public async Task FirstTimeUserChangePassword_ValidCommand_ReturnsOkResult()
        {
            var command = new FirstTimeUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockFirstTimePasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<FirstTimeUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Password changed"
                });

            var result = await CreateSut().FirstTimeUserChangePassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task FirstTimeUserChangePassword_ValidationFails_ReturnsBadRequest()
        {
            var command = new FirstTimeUserPasswordCommand();
            _mockFirstTimePasswordValidator
                .Setup(v => v.ValidateAsync(It.IsAny<FirstTimeUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("UserName", "Required")
                }));

            var result = await CreateSut().FirstTimeUserChangePassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task FirstTimeUserChangePassword_HandlerFails_ReturnsBadRequest()
        {
            var command = new FirstTimeUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockFirstTimePasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<FirstTimeUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Failed"
                });

            var result = await CreateSut().FirstTimeUserChangePassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- ChangePassword ---

        [Fact]
        public async Task ChangePassword_ValidCommand_ReturnsOkResult()
        {
            var command = new ChangeUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockChangePasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<ChangeUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Password changed"
                });

            var result = await CreateSut().ChangePassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_ValidationFails_ReturnsBadRequest()
        {
            var command = new ChangeUserPasswordCommand();
            _mockChangePasswordValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChangeUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("Password", "Required")
                }));

            var result = await CreateSut().ChangePassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_HandlerFails_ReturnsBadRequest()
        {
            var command = new ChangeUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockChangePasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<ChangeUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Failed"
                });

            var result = await CreateSut().ChangePassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- ForgotUserPassword ---

        [Fact]
        public async Task ForgotUserPassword_ValidCommand_ReturnsOkResult()
        {
            var command = new ForgotUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockForgotPasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<ForgotUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<ForgotPasswordResponse>
                {
                    IsSuccess = true,
                    Data = new ForgotPasswordResponse()
                });

            var result = await CreateSut().ForgotUserPassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ForgotUserPassword_ValidationFails_ReturnsBadRequest()
        {
            var command = new ForgotUserPasswordCommand();
            _mockForgotPasswordValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ForgotUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("UserName", "Required")
                }));

            var result = await CreateSut().ForgotUserPassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ForgotUserPassword_HandlerFails_ReturnsBadRequest()
        {
            var command = new ForgotUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockForgotPasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<ForgotUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<ForgotPasswordResponse>
                {
                    IsSuccess = false,
                    Message = "Invalid username"
                });

            var result = await CreateSut().ForgotUserPassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- ResetUserPassword ---

        [Fact]
        public async Task ResetUserPassword_ValidCommand_ReturnsOkResult()
        {
            var command = new ResetUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockResetPasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<ResetUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Password reset"
                });

            var result = await CreateSut().ResetUserPassword(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ResetUserPassword_ValidationFails_ReturnsBadRequest()
        {
            var command = new ResetUserPasswordCommand();
            _mockResetPasswordValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ResetUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                {
                    new ValidationFailure("UserName", "Required")
                }));

            var result = await CreateSut().ResetUserPassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ResetUserPassword_HandlerFails_ReturnsBadRequest()
        {
            var command = new ResetUserPasswordCommand { UserName = "testuser" };
            SetupValidatorSuccess(_mockResetPasswordValidator);

            _mockSender
                .Setup(m => m.Send(It.IsAny<ResetUserPasswordCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Failed"
                });

            var result = await CreateSut().ResetUserPassword(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        // --- RollbackDeleteUser ---

        [Fact]
        public async Task RollbackDeleteUser_SuccessfulResponse_ReturnsOkResult()
        {
            var command = new DeleteUserCommand { UserId = 1 };

            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "Rollback success"
                });

            var result = await CreateSut().RollbackDeleteUser(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RollbackDeleteUser_FailedResponse_ReturnsStatusCode500()
        {
            var command = new DeleteUserCommand { UserId = 1 };

            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "Rollback failed"
                });

            var result = await CreateSut().RollbackDeleteUser(command);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }

        // --- VerificationCodeRemove ---

        [Fact]
        public async Task VerificationCodeRemove_SuccessfulResponse_ReturnsOkResult()
        {
            var command = new RemoveVerficationCodeCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<RemoveVerficationCodeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "Removed"
                });

            var result = await CreateSut().VerificationCodeRemove(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task VerificationCodeRemove_FailedResponse_ReturnsStatusCode500()
        {
            var command = new RemoveVerficationCodeCommand();

            _mockSender
                .Setup(m => m.Send(It.IsAny<RemoveVerficationCodeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "Failed"
                });

            var result = await CreateSut().VerificationCodeRemove(command);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
        }
    }
}
