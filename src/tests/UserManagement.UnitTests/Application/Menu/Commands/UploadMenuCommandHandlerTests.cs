using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Menu.Commands.UploadMenu;

namespace UserManagement.UnitTests.Application.Menu.Commands
{
    public sealed class UploadMenuCommandHandlerTests
    {
        private readonly Mock<IModuleQueryRepository> _mockModuleRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMenuCommand> _mockMenuCmd = new(MockBehavior.Loose);
        private readonly Mock<IMenuQuery> _mockMenuQuery = new(MockBehavior.Loose);

        private UploadMenuCommandHandler CreateSut() =>
            new(_mockModuleRepo.Object, _mockMapper.Object, _mockMenuCmd.Object, _mockMenuQuery.Object);

        [Fact]
        public void CreateSut_DoesNotThrow()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
