using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Menu.Commands.CreateMenu;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Menu.Commands
{
    public sealed class CreateMenuCommandHandlerTests
    {
        private readonly Mock<IMenuCommand> _mockMenuCmd = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMenuCommandHandler CreateSut() =>
            new(_mockMenuCmd.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveId()
        {
            var command = new CreateMenuCommand { MenuName = "TestMenu" };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Menu>(command)).Returns(new UserManagement.Domain.Entities.Menu());
            _mockMenuCmd.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Menu>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_CreateFails_ThrowsException()
        {
            var command = new CreateMenuCommand { MenuName = "TestMenu" };
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Menu>(command)).Returns(new UserManagement.Domain.Entities.Menu());
            _mockMenuCmd.Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Menu>())).ReturnsAsync(0);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
