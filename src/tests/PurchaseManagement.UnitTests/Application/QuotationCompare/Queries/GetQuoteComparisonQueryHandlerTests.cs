using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison;

namespace PurchaseManagement.UnitTests.Application.QuotationCompare.Queries
{
    public sealed class GetQuoteComparisonQueryHandlerTests
    {
        private readonly Mock<IQuotationCompareQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);

        private GetQuoteComparisonQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockItemLookup.Object, _mockUomLookup.Object);

        [Fact]
        public async Task Handle_WhenNull_ReturnsNull()
        {
            _mockRepo
                .Setup(r => r.GetQuoteComparisonAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((QuoteComparisonDto?)null);

            var result = await CreateSut().Handle(
                new GetQuoteComparisonQuery(1),
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ValidRfqId_ReturnsDto()
        {
            var rawResult = new QuoteComparisonDto
            {
                RfqId = 1,
                Items = new List<QuoteComparisonDto.QuoteItemDto>()
            };
            var mappedDto = new QuoteComparisonDto
            {
                RfqId = 1,
                Items = new List<QuoteComparisonDto.QuoteItemDto>()
            };

            _mockRepo
                .Setup(r => r.GetQuoteComparisonAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rawResult);

            _mockMapper
                .Setup(m => m.Map<QuoteComparisonDto>(It.IsAny<object>()))
                .Returns(mappedDto);

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());

            _mockUomLookup
                .Setup(l => l.GetAllAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());

            var result = await CreateSut().Handle(
                new GetQuoteComparisonQuery(1),
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.RfqId.Should().Be(1);
        }
    }
}
