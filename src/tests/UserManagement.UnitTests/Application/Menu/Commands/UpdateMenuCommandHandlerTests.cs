using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Menu.Commands.UpdateMenu;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Menu.Commands
{
    public sealed class UpdateMenuCommandHandlerTests
    {
        private readonly Mock<IMenuCommand> _mockMenuCmd = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMenuCommandHandler CreateSut() =>
            new(_mockMenuCmd.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = new UpdateMenuCommand { MenuName = "Updated" };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Menu>(command)).Returns(new UserManagement.Domain.Entities.Menu());
            _mockMenuCmd.Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Menu>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_UpdateFails_ThrowsException()
        {
            var command = new UpdateMenuCommand { MenuName = "Test" };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Menu>(command)).Returns(new UserManagement.Domain.Entities.Menu());
            _mockMenuCmd.Setup(r => r.UpdateAsync(It.IsAny<UserManagement.Domain.Entities.Menu>())).ReturnsAsync(false);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
