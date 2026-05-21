using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Commands
{
    public sealed class CreateRoleEntitlementCommandHandlerTests
    {
        private readonly Mock<IRoleEntitlementCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<CreateRoleEntitlementCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private CreateRoleEntitlementCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private CreateRoleEntitlementCommand BuildCommand(int roleId = 1) =>
            new()
            {
                RoleId = roleId,
                RoleModules = new List<RoleModuleDTO>(),
                RoleParents = new List<RoleParentDTO>(),
                RoleChildren = new List<RoleChildDTO>(),
                RoleMenuPrivileges = new List<RoleMenuPrivilegesDTO>()
            };

        private void SetupHappyPath()
        {
            _mockMapper.Setup(m => m.Map<RoleModule>(It.IsAny<RoleModuleDTO>())).Returns(new RoleModule());
            _mockMapper.Setup(m => m.Map<RoleParent>(It.IsAny<RoleParentDTO>())).Returns(new RoleParent());
            _mockMapper.Setup(m => m.Map<RoleChild>(It.IsAny<RoleChildDTO>())).Returns(new RoleChild());
            _mockMapper.Setup(m => m.Map<RoleMenuPrivileges>(It.IsAny<RoleMenuPrivilegesDTO>())).Returns(new RoleMenuPrivileges());

            _mockCommandRepo
                .Setup(r => r.SaveRoleEntitlementsAsync(
                    It.IsAny<int>(),
                    It.IsAny<IList<RoleModule>>(),
                    It.IsAny<IList<RoleParent>>(),
                    It.IsAny<IList<RoleChild>>(),
                    It.IsAny<IList<RoleMenuPrivileges>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = BuildCommand();
            SetupHappyPath();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsSaveRoleEntitlementsOnce()
        {
            var command = BuildCommand(roleId: 5);
            SetupHappyPath();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SaveRoleEntitlementsAsync(
                    5,
                    It.IsAny<IList<RoleModule>>(),
                    It.IsAny<IList<RoleParent>>(),
                    It.IsAny<IList<RoleChild>>(),
                    It.IsAny<IList<RoleMenuPrivileges>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = BuildCommand();
            SetupHappyPath();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.Module == "RoleEntitlement"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsAllCollections()
        {
            var command = BuildCommand();
            command.RoleModules = new List<RoleModuleDTO> { new RoleModuleDTO() };
            command.RoleParents = new List<RoleParentDTO> { new RoleParentDTO() };
            command.RoleChildren = new List<RoleChildDTO> { new RoleChildDTO() };
            command.RoleMenuPrivileges = new List<RoleMenuPrivilegesDTO> { new RoleMenuPrivilegesDTO() };

            SetupHappyPath();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
            _mockMapper.Verify(m => m.Map<RoleModule>(It.IsAny<RoleModuleDTO>()), Times.Once);
        }
    }
}
