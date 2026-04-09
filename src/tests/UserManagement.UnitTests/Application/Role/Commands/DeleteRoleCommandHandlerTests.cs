using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Commands.DeleteRole;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Role.Commands
{
    public sealed class DeleteRoleCommandHandlerTests
    {
        private readonly Mock<IUserRoleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeleteRoleCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockMappingRepo = new(MockBehavior.Loose);

        private DeleteRoleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockLogger.Object,
                _mockMediator.Object, _mockQueryRepo.Object, _mockMappingRepo.Object);

        [Fact]
        public async Task Handle_ExistingRole_ReturnsPositiveResult()
        {
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserRole>(It.IsAny<DeleteRoleCommand>()))
                .Returns(new UserManagement.Domain.Entities.UserRole());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.UserRole>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new DeleteRoleCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
