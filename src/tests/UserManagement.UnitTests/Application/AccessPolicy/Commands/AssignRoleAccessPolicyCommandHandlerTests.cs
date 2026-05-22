using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Commands
{
    public sealed class AssignRoleAccessPolicyCommandHandlerTests
    {
        private readonly Mock<IAccessPolicyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAccessPolicyQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IMediator>                      _mockMediator    = new(MockBehavior.Loose);
        private readonly Mock<IMapper>                        _mockMapper      = new(MockBehavior.Strict);

        private AssignRoleAccessPolicyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(AssignRoleAccessPolicyCommand command, int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.RoleAccessPolicy>(command))
                .Returns(AccessPolicyBuilders.ValidRoleEntity(newId));

            _mockCommandRepo
                .Setup(r => r.AssignRoleValueAsync(It.IsAny<UserManagement.Domain.Entities.RoleAccessPolicy>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = AccessPolicyBuilders.ValidAssignCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = AccessPolicyBuilders.ValidAssignCommand();
            SetupHappyPath(command, newId: 7);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsAssignRoleValueOnce()
        {
            var command = AccessPolicyBuilders.ValidAssignCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.AssignRoleValueAsync(It.IsAny<UserManagement.Domain.Entities.RoleAccessPolicy>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = AccessPolicyBuilders.ValidAssignCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode   == "ROLEACCESSPOLICY_ASSIGN" &&
                        e.Module       == "AccessPolicy"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SuccessMessageSet()
        {
            var command = AccessPolicyBuilders.ValidAssignCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Be("Role access policy assigned successfully.");
        }
    }
}
