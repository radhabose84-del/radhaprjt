using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.EntityLevelAdmin.Commands
{
    public sealed class CreateEntityLevelAdminCommandHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IUserCommandRepository> _mockUserRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _mockUserQueryRepo = new(MockBehavior.Strict);

        private CreateEntityLevelAdminCommandHandler CreateSut() =>
            new(_mockMediator.Object, _mockUserRepo.Object, _mockMapper.Object, _mockUserQueryRepo.Object);

        [Fact]
        public async Task Handle_NewUser_ReturnsCreatedId()
        {
            var command = new CreateEntityLevelAdminCommand { Email = "test@test.com" };
            var user = new UserManagement.Domain.Entities.User { Id = Guid.NewGuid(), UserId = 1 };

            _mockUserQueryRepo
                .Setup(r => r.GetByUsernameAsync("test@test.com"))
                .ReturnsAsync((UserManagement.Domain.Entities.User?)null!);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.User>(command))
                .Returns(user);

            _mockUserRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.User>()))
                .ReturnsAsync(user);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ExistingUser_ThrowsValidationException()
        {
            _mockUserQueryRepo
                .Setup(r => r.GetByUsernameAsync("existing@test.com"))
                .ReturnsAsync(new UserManagement.Domain.Entities.User { Id = Guid.NewGuid() });

            Func<Task> act = () => CreateSut().Handle(
                new CreateEntityLevelAdminCommand { Email = "existing@test.com" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
