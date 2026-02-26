using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.AgentCommissionConfig.Commands
{
    public class DeleteAgentCommissionConfigCommandHandlerTests
    {
        private readonly Mock<IAgentCommissionConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteAgentCommissionConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        private void SetupSoftDelete(int id = 1)
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(new DeleteAgentCommissionConfigCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteAsync_Once()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteAgentCommissionConfigCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditLogEvent_Once()
        {
            SetupSoftDelete(1);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteAgentCommissionConfigCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "AGENT_COMMISSION_CONFIG_DELETE" &&
                        e.ActionName == "1" &&
                        e.Module == "AgentCommissionConfig"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_AuditEvent_ContainsRequestId()
        {
            SetupSoftDelete(99);
            SetupPublishAudit();

            await CreateSut().Handle(new DeleteAgentCommissionConfigCommand(99), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "99"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
