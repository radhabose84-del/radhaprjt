using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaAutoComplete;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorEvaluationCriteria.Queries
{
    public sealed class GetVendorEvaluationCriteriaAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IVendorEvaluationCriteriaQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVendorEvaluationCriteriaAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = VendorEvaluationCriteriaBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Qua", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());

            var result = await CreateSut().Handle(new GetVendorEvaluationCriteriaAutoCompleteQuery("Qua"), CancellationToken.None);

            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var lookups = VendorEvaluationCriteriaBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());

            var result = await CreateSut().Handle(new GetVendorEvaluationCriteriaAutoCompleteQuery(""), CancellationToken.None);

            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = VendorEvaluationCriteriaBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Qua", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<VendorEvaluationCriteriaLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());

            await CreateSut().Handle(new GetVendorEvaluationCriteriaAutoCompleteQuery("Qua"), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
