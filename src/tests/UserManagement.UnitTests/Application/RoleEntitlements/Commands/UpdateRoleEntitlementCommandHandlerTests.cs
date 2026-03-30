using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.CreateRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.UpdateRoleRntitlement;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Commands
{
    public sealed class UpdateRoleEntitlementCommandHandlerTests
    {
        private readonly Mock<IRoleEntitlementCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleEntitlementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<UpdateRoleEntitlementCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private UpdateRoleEntitlementCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private UpdateRoleEntitlementCommand BuildCommand(int roleId = 1) =>
            new()
            {
                RoleId = roleId,
                RoleModules = new List<RoleModuleDTO>(),
                RoleParents = new List<RoleParentDTO>(),
                RoleChildren = new List<RoleChildDTO>(),
                RoleMenuPrivileges = new List<RoleMenuPrivilegesDTO>()
            };

        private void SetupHappyPath(bool repoResult = true)
        {
            _mockMapper.Setup(m => m.Map<RoleModule>(It.IsAny<RoleModuleDTO>())).Returns(new RoleModule());
            _mockMapper.Setup(m => m.Map<RoleParent>(It.IsAny<RoleParentDTO>())).Returns(new RoleParent());
            _mockMapper.Setup(m => m.Map<RoleChild>(It.IsAny<RoleChildDTO>())).Returns(new RoleChild());
            _mockMapper.Setup(m => m.Map<RoleMenuPrivileges>(It.IsAny<RoleMenuPrivilegesDTO>())).Returns(new RoleMenuPrivileges());

            _mockCommandRepo
                .Setup(r => r.UpdateRoleEntitlementsAsync(
                    It.IsAny<int>(),
                    It.IsAny<IList<RoleModule>>(),
                    It.IsAny<IList<RoleParent>>(),
                    It.IsAny<IList<RoleChild>>(),
                    It.IsAny<IList<RoleMenuPrivileges>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(repoResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = BuildCommand();
            SetupHappyPath(true);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var command = BuildCommand(roleId: 7);
            SetupHappyPath(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateRoleEntitlementsAsync(
                    7,
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
            SetupHappyPath(true);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.Module == "RoleEntitlement"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_RepositoryReturnsFalse_ThrowsException()
        {
            var command = BuildCommand();
            SetupHappyPath(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*update failed*");
        }
    }
}
