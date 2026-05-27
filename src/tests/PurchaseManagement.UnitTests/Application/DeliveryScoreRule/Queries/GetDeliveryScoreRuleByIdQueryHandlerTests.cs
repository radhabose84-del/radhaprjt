using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleById;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DeliveryScoreRule.Queries
{
    public sealed class GetDeliveryScoreRuleByIdQueryHandlerTests
    {
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDeliveryScoreRuleByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = DeliveryScoreRuleBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<DeliveryScoreRuleDto>(It.IsAny<object>())).Returns(dto);
            var result = await CreateSut().Handle(new GetDeliveryScoreRuleByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DeliveryScoreRuleDto?)null);
            var result = await CreateSut().Handle(new GetDeliveryScoreRuleByIdQuery { Id = 99 }, CancellationToken.None);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = DeliveryScoreRuleBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMapper.Setup(m => m.Map<DeliveryScoreRuleDto>(It.IsAny<object>())).Returns(dto);
            await CreateSut().Handle(new GetDeliveryScoreRuleByIdQuery { Id = 1 }, CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
