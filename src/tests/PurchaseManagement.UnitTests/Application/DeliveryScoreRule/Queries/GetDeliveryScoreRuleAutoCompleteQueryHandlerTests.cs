using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleAutoComplete;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.DeliveryScoreRule.Queries
{
    public sealed class GetDeliveryScoreRuleAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDeliveryScoreRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDeliveryScoreRuleAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = DeliveryScoreRuleBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("On", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<DeliveryScoreRuleLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());
            var result = await CreateSut().Handle(new GetDeliveryScoreRuleAutoCompleteQuery("On"), CancellationToken.None);
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = DeliveryScoreRuleBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("On", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<DeliveryScoreRuleLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());
            await CreateSut().Handle(new GetDeliveryScoreRuleAutoCompleteQuery("On"), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
