using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using DomainEntities = SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Application.AgentCommissionConfig.Commands
{
    public class UpdateAgentCommissionConfigCommandHandlerTests
    {
        private readonly Mock<IAgentCommissionConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateAgentCommissionConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(UpdateAgentCommissionConfigCommand command, int returnId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<DomainEntities.AgentCommissionConfig>(command))
                .Returns(AgentCommissionConfigBuilders.ValidEntity(id: command.Id));

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<DomainEntities.AgentCommissionConfig>()))
                .ReturnsAsync(returnId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsUpdatedId()
        {
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(id: 7);
            SetupHappyPath(command, returnId: 7);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(7);
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateAsync_Once()
        {
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<DomainEntities.AgentCommissionConfig>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(id: 5);
            SetupHappyPath(command, returnId: 5);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "AGENT_COMMISSION_CONFIG_UPDATE" &&
                        e.ActionName == "5" &&
                        e.Module == "AgentCommissionConfig"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
