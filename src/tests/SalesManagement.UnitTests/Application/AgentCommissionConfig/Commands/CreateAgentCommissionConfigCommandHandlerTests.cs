using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;
using DomainEntities = SalesManagement.Domain.Entities;

namespace SalesManagement.UnitTests.Application.AgentCommissionConfig.Commands
{
    public class CreateAgentCommissionConfigCommandHandlerTests
    {
        private readonly Mock<IAgentCommissionConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateAgentCommissionConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateAgentCommissionConfigCommand command, int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<DomainEntities.AgentCommissionConfig>(command))
                .Returns(AgentCommissionConfigBuilders.ValidEntity(id: 0));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<DomainEntities.AgentCommissionConfig>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessMessage()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<DomainEntities.AgentCommissionConfig>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "AGENT_COMMISSION_CONFIG_CREATE" &&
                        e.Module == "AgentCommissionConfig"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<DomainEntities.AgentCommissionConfig>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();
            DomainEntities.AgentCommissionConfig? capturedEntity = null;

            _mockMapper
                .Setup(m => m.Map<DomainEntities.AgentCommissionConfig>(command))
                .Returns(AgentCommissionConfigBuilders.ValidEntity(id: 0));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<DomainEntities.AgentCommissionConfig>()))
                .Callback<DomainEntities.AgentCommissionConfig>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsActive.Should().Be(Status.Active);
            capturedEntity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
