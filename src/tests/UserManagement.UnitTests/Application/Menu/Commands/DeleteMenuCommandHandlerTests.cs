using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Menu.Commands.DeleteMenu;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Menu.Commands
{
    public sealed class DeleteMenuCommandHandlerTests
    {
        private readonly Mock<IMenuCommand> _mockMenuCmd = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMenuCommandHandler CreateSut() =>
            new(_mockMenuCmd.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingMenu_ReturnsTrue()
        {
            var command = new DeleteMenuCommand { Id = 1 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Menu>(command)).Returns(new UserManagement.Domain.Entities.Menu());
            _mockMenuCmd.Setup(r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.Menu>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var command = new DeleteMenuCommand { Id = 99 };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Menu>(command)).Returns(new UserManagement.Domain.Entities.Menu());
            _mockMenuCmd.Setup(r => r.DeleteAsync(99, It.IsAny<UserManagement.Domain.Entities.Menu>())).ReturnsAsync(false);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
