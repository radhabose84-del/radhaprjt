using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.OfficerAgent.Commands
{
    public class UpdateOfficerAgentCommandHandlerTests
    {
        private readonly Mock<IOfficerAgentCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateOfficerAgentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateOfficerAgentCommand ValidCommand() => new()
        {
            MarketingOfficerId = 1,
            Agents = new List<OfficerAgentUpdateItem>
            {
                new()
                {
                    Id = 10,
                    AgentId = 2,
                    ValidityFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                    IsActive = 1
                }
            }
        };

        private void SetupMapper()
        {
            _mockMapper
                .Setup(m => m.Map<List<SalesManagement.Domain.Entities.OfficerAgent>>(It.IsAny<UpdateOfficerAgentCommand>()))
                .Returns(new List<SalesManagement.Domain.Entities.OfficerAgent>
                {
                    new() { Id = 10, MarketingOfficerId = 1, AgentId = 2, IsActive = true }
                });
        }

        private void SetupUpdateBatchAsync(int returnCount = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateBatchAsync(It.IsAny<List<SalesManagement.Domain.Entities.OfficerAgent>>()))
                .ReturnsAsync(returnCount);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            SetupMapper();
            SetupUpdateBatchAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            SetupMapper();
            SetupUpdateBatchAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateBatchAsync_Once()
        {
            SetupMapper();
            SetupUpdateBatchAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateBatchAsync(It.IsAny<List<SalesManagement.Domain.Entities.OfficerAgent>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            SetupMapper();
            SetupUpdateBatchAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "OFFICER_AGENT_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsMarketingOfficerId()
        {
            SetupMapper();
            SetupUpdateBatchAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
