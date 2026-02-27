using AutoMapper;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Dto;
using SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigById;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.AgentCommissionConfig.Queries
{
    public class GetAgentCommissionConfigByIdQueryHandlerTests
    {
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAgentCommissionConfigByIdQueryHandler CreateSut()
        {
            _mockMapper
                .Setup(m => m.Map<AgentCommissionConfigDto>(It.IsAny<AgentCommissionConfigDto>()))
                .Returns<AgentCommissionConfigDto>(o => o);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new GetAgentCommissionConfigByIdQueryHandler(
                _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = AgentCommissionConfigBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetAgentCommissionConfigByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFoundId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AgentCommissionConfigDto?)null);

            var result = await CreateSut().Handle(
                new GetAgentCommissionConfigByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NotFoundId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AgentCommissionConfigDto?)null);

            await CreateSut().Handle(
                new GetAgentCommissionConfigByIdQuery { Id = 999 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditLogEvent_Once()
        {
            var dto = AgentCommissionConfigBuilders.ValidDto(id: 5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetAgentCommissionConfigByIdQuery { Id = 5 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CallsQueryRepository_Once()
        {
            var dto = AgentCommissionConfigBuilders.ValidDto(id: 3);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(dto);

            await CreateSut().Handle(
                new GetAgentCommissionConfigByIdQuery { Id = 3 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(3), Times.Once);
        }
    }
}
