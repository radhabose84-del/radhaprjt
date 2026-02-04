using UserManagement.Application.Users.Commands.ChangeUserPassword;
using UserManagement.Application.Users.Commands.CreateUser;
using UserManagement.Application.Users.Commands.DeleteUser;
using UserManagement.Application.Users.Commands.ForgotUserPassword;
using UserManagement.Application.Users.Commands.ResetUserPassword;
using UserManagement.Application.Users.Commands.UpdateFirstTimeUserPassword;
using UserManagement.Application.Users.Commands.UpdateUser;
using UserManagement.Application.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserManagement.Infrastructure.Data;
using MassTransit;

// Alias FluentValidation result types to avoid conflicts with MassTransit.ValidationResult
using FValidationResult = FluentValidation.Results.ValidationResult;
using FValidationFailure = FluentValidation.Results.ValidationFailure;
using Moq;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.API.Controllers;

namespace UserManagement.UnitTests.Presentation.Users;

[Trait("layer","api")]
public sealed class UserControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly Mock<FluentValidation.IValidator<CreateUserCommand>> _createVal = new();
    private readonly Mock<FluentValidation.IValidator<UpdateUserCommand>> _updateVal = new();
    private readonly Mock<FluentValidation.IValidator<FirstTimeUserPasswordCommand>> _firstTimeVal = new();
    private readonly Mock<FluentValidation.IValidator<ChangeUserPasswordCommand>> _changePassVal = new();
    private readonly Mock<FluentValidation.IValidator<ForgotUserPasswordCommand>> _forgotVal = new();
    private readonly Mock<FluentValidation.IValidator<ResetUserPasswordCommand>> _resetVal = new();
    private readonly Mock<ILogger<UserController>> _logger = new();
    private readonly Mock<IPublishEndpoint> _bus = new();

    private readonly ApplicationDbContext _db;
    private readonly UserController _sut;

    // Minimal derived DbContext to satisfy the ctor (your controller never uses it directly)
    private sealed class TestDbContext : ApplicationDbContext
    {
        public TestDbContext()
            : base(new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options, null!, null!)
        { }
    }

    public UserControllerTests()
    {
        _db = new TestDbContext();

        _sut = new UserController(
            mediator: _sender.Object,
            createUserCommandValidator: _createVal.Object,
            updateUserCommandValidator: _updateVal.Object,
            dbContext: _db,
            firstTimeUserPasswordCommandValidator: _firstTimeVal.Object,
            changeUserPasswordCommandValidator: _changePassVal.Object,
            logger: _logger.Object,
            forgotUserPasswordCommandValidator: _forgotVal.Object,
            resetUserPasswordCommandValidator: _resetVal.Object,
            publishEndpoint: _bus.Object
        );
    }

    [Fact]
    public async Task DeleteAsync_returns_400_when_id_invalid()
    {
        var result = await _sut.DeleteAsync(0);
        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_returns_404_when_mediator_returns_null()
    {
        const int id = 123;

        // IMPORTANT: return the correct TResponse type (UserByIdDTO?) not object
        _sender.Setup(m => m.Send(It.Is<GetUserByIdQuery>(q => q.UserId == id), It.IsAny<CancellationToken>()))
               .ReturnsAsync((UserByIdDTO?)null);

        var result = await _sut.GetByIdAsync(id);

        result.Should().BeOfType<NotFoundObjectResult>();
        _sender.VerifyAll();
    }

    [Fact]
    public async Task CreateAsync_returns_400_and_does_not_hit_mediator_when_validation_fails()
    {
        var cmd = new CreateUserCommand { UserName = "" };
        _createVal.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new FValidationResult(new List<FValidationFailure>
                  {
                      new FValidationFailure(nameof(CreateUserCommand.UserName), "required")
                  }));

        var result = await _sut.CreateAsync(cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_returns_400_and_does_not_hit_mediator_when_validation_fails()
    {
        var cmd = new UpdateUserCommand { UserId = 0, UserName = "" };
        _updateVal.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new FValidationResult(new List<FValidationFailure>
                  {
                      new FValidationFailure(nameof(UpdateUserCommand.UserName), "required")
                  }));

        var result = await _sut.UpdateAsync(cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_returns_400_when_validation_fails()
    {
        var cmd = new ChangeUserPasswordCommand { UserName = "", OldPassword = "", NewPassword = "" };
        _changePassVal.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new FValidationResult(new List<FValidationFailure>
                      {
                          new FValidationFailure(nameof(ChangeUserPasswordCommand.UserName), "required")
                      }));

        var result = await _sut.ChangePassword(cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<ChangeUserPasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FirstTimeUserChangePassword_returns_400_when_validation_fails()
    {
        var cmd = new FirstTimeUserPasswordCommand { UserName = "", Password = "" };
        _firstTimeVal.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FValidationResult(new List<FValidationFailure>
                     {
                         new FValidationFailure(nameof(FirstTimeUserPasswordCommand.UserName), "required")
                     }));

        var result = await _sut.FirstTimeUserChangePassword(cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<FirstTimeUserPasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotUserPassword_returns_400_when_validation_fails()
    {
        var cmd = new ForgotUserPasswordCommand { UserName = "" };
        _forgotVal.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new FValidationResult(new List<FValidationFailure>
                  {
                      new FValidationFailure(nameof(ForgotUserPasswordCommand.UserName), "required")
                  }));

        var result = await _sut.ForgotUserPassword(cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<ForgotUserPasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetUserPassword_returns_400_when_validation_fails()
    {
        // NOTE: Your ResetUserPasswordCommand apparently has no NewPassword – so we don't set it.
        var cmd = new ResetUserPasswordCommand { UserName = "", VerificationCode = "" };
        _resetVal.Setup(v => v.ValidateAsync(cmd, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new FValidationResult(new List<FValidationFailure>
                 {
                     new FValidationFailure(nameof(ResetUserPasswordCommand.VerificationCode), "invalid")
                 }));

        var result = await _sut.ResetUserPassword(cmd);

        result.Should().BeOfType<BadRequestObjectResult>();
        _sender.Verify(m => m.Send(It.IsAny<ResetUserPasswordCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
