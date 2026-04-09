using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.AgentCustomerMapping.Commands
{
    public class CreateAgentCustomerMappingCommandHandlerTests
    {
        private readonly Mock<IAgentCustomerMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateAgentCustomerMappingCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateAgentCustomerMappingCommand ValidCommand() => new()
        {
            CustomerId = 1,
            AgentId = 2,
            SubAgentId = 3,
            SalesSegmentId = 4,
            EffectiveFrom = DateTime.Today.AddDays(-30),
            EffectiveTo = DateTime.Today.AddDays(30),
            IsDefaultAgent = true,
            Remarks = "Test mapping"
        };

        private void SetupMapper(CreateAgentCustomerMappingCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.AgentCustomerMapping>(cmd))
                .Returns(new SalesManagement.Domain.Entities.AgentCustomerMapping
                {
                    CustomerId = cmd.CustomerId,
                    AgentId = cmd.AgentId,
                    SalesSegmentId = cmd.SalesSegmentId
                });
        }

        private void SetupCreateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.AgentCustomerMapping>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(42);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.AgentCustomerMapping>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "AGENT_CUSTOMER_MAPPING_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("created successfully");
        }
    }
}
