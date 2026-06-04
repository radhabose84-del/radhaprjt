using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesAutoComplete;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.BarcodeSeries.Queries
{
    public sealed class GetBarcodeSeriesAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IBarcodeSeriesQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetBarcodeSeriesAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeSeriesBuilders.ValidLookupList());

            var result = await CreateSut().Handle(new GetBarcodeSeriesAutoCompleteQuery("BCS"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_PassesEmptyString()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BarcodeSeriesBuilders.ValidLookupList());

            await CreateSut().Handle(new GetBarcodeSeriesAutoCompleteQuery(null), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
