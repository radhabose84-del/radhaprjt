using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.OfficerAgent.Commands
{
    public class CreateOfficerAgentCommandHandlerTests
    {
        private readonly Mock<IOfficerAgentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateOfficerAgentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateOfficerAgentCommand ValidCommand() => new()
        {
            MarketingOfficerId = 1,
            Agents = new List<OfficerAgentBatchItem>
            {
                new()
                {
                    AgentId = 2,
                    ValidityFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                    IsActive = 1
                }
            }
        };

        private void SetupMapper(CreateOfficerAgentCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<List<SalesManagement.Domain.Entities.OfficerAgent>>(cmd))
                .Returns(new List<SalesManagement.Domain.Entities.OfficerAgent>
                {
                    new() { MarketingOfficerId = cmd.MarketingOfficerId, AgentId = 2, IsActive = true }
                });
        }

        private void SetupCreateBatchAsync(int returnCount = 1)
        {
            _mockCommandRepo
                .Setup(r => r.CreateBatchAsync(It.IsAny<List<SalesManagement.Domain.Entities.OfficerAgent>>()))
                .ReturnsAsync(returnCount);
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
            SetupCreateBatchAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCount()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateBatchAsync(3);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateBatchAsync_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateBatchAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateBatchAsync(It.IsAny<List<SalesManagement.Domain.Entities.OfficerAgent>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateBatchAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "OFFICER_AGENT_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_AuditEvent_ContainsMarketingOfficerId()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupCreateBatchAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
