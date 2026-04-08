using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword;

namespace UserManagement.UnitTests.Application.EntityLevelAdmin.Commands
{
    public sealed class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUserCommandRepository> _mockUserRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _mockUserQueryRepo = new(MockBehavior.Strict);

        private ResetPasswordCommandHandler CreateSut() =>
            new(_mockMediator.Object, _mockMapper.Object, _mockUserRepo.Object, _mockUserQueryRepo.Object);

        [Fact]
        public async Task Handle_ExistingUser_ReturnsTrue()
        {
            var user = new UserManagement.Domain.Entities.User { Id = Guid.NewGuid(), UserId = 1 };

            _mockUserQueryRepo
                .Setup(r => r.GetByUsernameAsync("test@test.com"))
                .ReturnsAsync(user);

            _mockMapper
                .Setup(m => m.Map(It.IsAny<ResetPasswordCommand>(), user));

            _mockUserRepo
                .Setup(r => r.SetAdminPassword(1, It.IsAny<UserManagement.Domain.Entities.User>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(
                new ResetPasswordCommand { Email = "test@test.com", UserId = 1 },
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsValidationException()
        {
            _mockUserQueryRepo
                .Setup(r => r.GetByUsernameAsync("missing@test.com"))
                .ReturnsAsync((UserManagement.Domain.Entities.User?)null!);

            Func<Task> act = () => CreateSut().Handle(
                new ResetPasswordCommand { Email = "missing@test.com", UserId = 1 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
