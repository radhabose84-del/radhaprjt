using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Commands.CreateRole;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Role.Commands
{
    public sealed class CreateRoleCommandHandlerTests
    {
        private readonly Mock<IUserRoleCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockMappingRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateRoleCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateRoleCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMappingRepo.Object,
                _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NewRole_ReturnsDto()
        {
            var command = new CreateRoleCommand { RoleName = "TestRole" };
            var entity = new UserManagement.Domain.Entities.UserRole { Id = 1 };

            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync("TestRole"))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserRole>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserRole>()))
                .ReturnsAsync(entity);

            var dto = new UserRoleDto();
            _mockMapper
                .Setup(m => m.Map<UserRoleDto>(It.IsAny<UserManagement.Domain.Entities.UserRole>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
