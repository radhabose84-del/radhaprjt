using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Commands.UpdateRole;

namespace UserManagement.UnitTests.Application.Role.Commands
{
    public sealed class UpdateRoleCommandHandlerTests
    {
        private readonly Mock<IUserRoleCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockMappingCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockMappingQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateRoleCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateRoleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMappingCommandRepo.Object,
                _mockMappingQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
